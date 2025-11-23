using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyNhaHang.Models;
using QuanLyNhaHang.Models.DTO;
using QuanLyNhaHang.Services;
using System.Security.Claims;

[ApiController]
[Route("api/[controller]")]
[Authorize] // BẮT BUỘC: Chỉ người đã đăng nhập (có Token) mới được gọi
public class BookingHistoryController : ControllerBase
{
    private readonly QLNhaHangContext _context;

    private readonly IEmailService _emailService; // Inject thêm Email Service

    public BookingHistoryController(QLNhaHangContext context, IEmailService emailService)
    {
        _context = context;
        _emailService = emailService;
    }

    [HttpGet("me")]
    public async Task<IActionResult> GetMyBookingHistory()
    {
        var maKhachHang = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (maKhachHang == null)
        {
            return Unauthorized();
        }

        var donHangs = await _context.DonHangs
            .Where(dh => dh.MaKhachHang == maKhachHang)
            // SỬA: Include bảng trung gian để lấy thông tin bàn
            .Include(dh => dh.BanAnDonHangs)
                .ThenInclude(badh => badh.MaBanNavigation)
            .Include(dh => dh.MaTrangThaiDonHangNavigation)
            .OrderByDescending(dh => dh.ThoiGianDatHang)
            .Select(dh => new BookingHistoryDto
            {
                MaDonHang = dh.MaDonHang,
                // SỬA: Lấy tên bàn từ bảng trung gian
                TenBan = string.Join(", ", dh.BanAnDonHangs.Select(b => b.MaBanNavigation.TenBan)),
                ThoiGianBatDau = dh.ThoiGianDatHang ?? DateTime.Now,
                SoLuongNguoi = dh.SoLuongNguoiDK,
                GhiChu = dh.GhiChu,
                DaHuy = (dh.MaTrangThaiDonHang == "DA_HUY"),
                MaTrangThai = dh.MaTrangThaiDonHang,
                CoTheHuy = ((dh.ThoiGianDatHang) > DateTime.Now && dh.MaTrangThaiDonHang != "DA_HUY"),
                TrangThai = dh.MaTrangThaiDonHangNavigation.TenTrangThai
            })
            .ToListAsync();

        return Ok(donHangs);
    }

    [HttpPost("cancel")]
    public async Task<IActionResult> CancelBooking([FromBody] CancelBookingRequest request)
    {
        // Lấy user ID từ Token để đảm bảo khách chỉ hủy đơn của chính mình
        var maKhachHang = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (maKhachHang == null) return Unauthorized();

        var donHang = await _context.DonHangs
            // SỬA: Include bảng trung gian để cập nhật trạng thái bàn
            .Include(dh => dh.BanAnDonHangs)
                .ThenInclude(badh => badh.MaBanNavigation)
            .FirstOrDefaultAsync(d => d.MaDonHang == request.MaDonHang && d.MaKhachHang == maKhachHang);

        if (donHang == null)
        {
            return NotFound(new { message = "Đơn hàng không tồn tại hoặc không thuộc quyền sở hữu." });
        }

        if (donHang.MaTrangThaiDonHang == "DA_HUY")
        {
            return BadRequest(new { message = "Đơn hàng đã bị hủy trước đó." });
        }

        if (donHang.MaTrangThaiDonHang == "DA_HOAN_THANH")
        {
            return BadRequest(new { message = "Đơn hàng đã hoàn thành, không thể hủy." });
        }

        // Kiểm tra thời gian (Ví dụ: Không cho hủy nếu đã quá giờ đặt hoặc sắp tới giờ đặt)
        var gioAn = donHang.TgdatDuKien ?? donHang.ThoiGianDatHang ?? DateTime.Now;
        if (DateTime.Now >= gioAn)
        {
            return BadRequest(new { message = "Đã quá giờ đặt bàn, không thể hủy online. Vui lòng gọi hotline." });
        }

        // --- TIẾN HÀNH HỦY ---
        donHang.MaTrangThaiDonHang = "DA_HUY";
        donHang.GhiChu += $" | Khách tự hủy lúc {DateTime.Now:HH:mm dd/MM}";

        // Cập nhật trạng thái bàn về TRỐNG (TTBA001)
        // SỬA: Lặp qua danh sách BanAnDonHangs
        if (donHang.BanAnDonHangs != null)
        {
            foreach (var banAnDonHang in donHang.BanAnDonHangs)
            {
                if (banAnDonHang.MaBanNavigation != null)
                {
                    banAnDonHang.MaBanNavigation.MaTrangThai = "TTBA001"; // Mã bàn Trống
                }
            }
        }

        await _context.SaveChangesAsync();

        return Ok(new { message = "Hủy đặt bàn thành công." });
    }





    // =================================================================
    // ENDPOINT 2: GỬI NHẮC NHỞ (Quét đơn trong 24h tới)
    // =================================================================
    // Endpoint này để Admin bấm hoặc Job tự gọi mỗi sáng
    [HttpPost("send-reminders")]
    public async Task<IActionResult> SendDailyReminders()
    {
        var now = DateTime.Now;
        var limit = now.AddHours(24);

        // Lấy các đơn SẮP DIỄN RA và CHƯA HỦY
        var donHangs = await _context.DonHangs
            .Include(dh => dh.MaKhachHangNavigation)
            .Where(dh =>
                (dh.MaTrangThaiDonHang == "CHO_XAC_NHAN" || dh.MaTrangThaiDonHang == "DA_XAC_NHAN") &&
                dh.TgdatDuKien > now &&
                dh.TgdatDuKien <= limit
            ).ToListAsync();

        int emailCount = 0;
        int smsCount = 0;

        foreach (var dh in donHangs)
        {
            var email = dh.MaKhachHangNavigation.Email;
            var sdt = dh.SdtnguoiNhan ?? dh.MaKhachHangNavigation.SoDienThoai;
            var ten = dh.TenNguoiNhan ?? dh.MaKhachHangNavigation.HoTen;

            if (!string.IsNullOrEmpty(email))
            {

                string baseUrl = $"{Request.Scheme}://{Request.Host}";

                string linkHuy = $"{baseUrl}/api/BookingHistory/quick-cancel/{dh.MaDonHang}";


                _ = _emailService.SendReminderEmailAsync(
                    email, ten, dh.TgdatDuKien?? DateTime.Now, "", linkHuy
                );
                emailCount++;
            }
            else
            {
                Console.WriteLine($"[SMS MOCK] Gửi đến {sdt}: Chào {ten}, nhắc bạn có lịch đặt bàn lúc {dh.TgdatDuKien:HH:mm}.");
                smsCount++;
            }
        }

        return Ok(new { Message = "Đã chạy tiến trình nhắc nhở.", SentEmails = emailCount, MockSMS = smsCount });
    }

    // ENDPOINT 3: HỦY NHANH TỪ EMAIL (Quick Cancel)
    [HttpGet("quick-cancel/{maDonHang}")]
    [AllowAnonymous]
    public async Task<IActionResult> QuickCancelFromEmail(string maDonHang, [FromQuery] bool confirm = false)
    {
        var donHang = await _context.DonHangs
            // SỬA: Include bảng trung gian để cập nhật trạng thái bàn
            .Include(dh => dh.BanAnDonHangs)
                .ThenInclude(badh => badh.MaBanNavigation)
            .FirstOrDefaultAsync(d => d.MaDonHang == maDonHang);

        if (donHang == null || donHang.MaTrangThaiDonHang == "DA_HUY")
        {
            string htmlLoi = _emailService.GetHtml_ThongBaoLoi("Đơn hàng không tồn tại hoặc đã được hủy trước đó.");
            return Content(htmlLoi, "text/html");
        }

        if (donHang.MaTrangThaiDonHang == "DA_HOAN_THANH")
        {
            string htmlLoi = _emailService.GetHtml_ThongBaoLoi("Đơn hàng đã hoàn thành, không thể hủy.");
            return Content(htmlLoi, "text/html");
        }

        var gioAn = donHang.TgdatDuKien ?? donHang.ThoiGianDatHang ?? DateTime.Now;
        if (DateTime.Now >= gioAn)
        {
            string htmlLoi = _emailService.GetHtml_ThongBaoLoi("Không thể hủy đơn hàng vì thời gian đặt bàn đã diễn ra.");
            return Content(htmlLoi, "text/html");
        }
        // TRƯỜNG HỢP A: CHƯA XÁC NHẬN -> HIỆN TRANG WEB CÓ NÚT BẤM

        if (confirm == false)
        {
            // Lưu ý: Thay đổi Base URL cho đúng với Domain thật khi deploy
            string linkXacNhan = $"/api/BookingHistory/quick-cancel/{maDonHang}?confirm=true";

            string htmlConfirm = _emailService.GetHtml_XacNhanHuy(maDonHang, linkXacNhan);

            return Content(htmlConfirm, "text/html");
        }


        // TRƯỜNG HỢP B: ĐÃ BẤM XÁC NHẬN (confirm=true) -> TIẾN HÀNH HỦY
        donHang.MaTrangThaiDonHang = "DA_HUY";
        donHang.GhiChu += " | Khách hủy nhanh qua Email.";

        // SỬA: Cập nhật trạng thái bàn qua bảng trung gian
        if (donHang.BanAnDonHangs != null)
        {
            foreach (var banAnDonHang in donHang.BanAnDonHangs)
            {
                if (banAnDonHang.MaBanNavigation != null)
                {
                    banAnDonHang.MaBanNavigation.MaTrangThai = "TTBA001";
                }
            }
        }

        await _context.SaveChangesAsync();

        string htmlSuccess = _emailService.GetHtml_HuyThanhCong();
        return Content(htmlSuccess, "text/html");
    }

}

public class CancelBookingRequest
{
    public string MaDonHang { get; set; }
}