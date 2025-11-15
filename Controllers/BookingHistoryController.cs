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
            .Include(dh => dh.MaBans) 
            .Include(dh => dh.MaTrangThaiDonHangNavigation)
            .OrderByDescending(dh => dh.ThoiGianDatHang)
            .Select(dh => new BookingHistoryDto
            {
                MaDonHang = dh.MaDonHang,
                TenBan = string.Join(", ", dh.MaBans.Select(b => b.TenBan)),
                ThoiGianBatDau = dh.ThoiGianDatHang ?? DateTime.Now, 
                SoLuongNguoi = dh.SoLuongNguoiDk, 
                GhiChu = dh.GhiChu,
                DaHuy = (dh.MaTrangThaiDonHang == "DA_HUY"),
                MaTrangThai = dh.MaTrangThaiDonHang,
                CoTheHuy = ((dh.ThoiGianDatHang) > DateTime.Now && dh.MaTrangThaiDonHang != "DA_HUY"),
                TrangThai = dh.MaTrangThaiDonHangNavigation.TenTrangThai
            })
            .ToListAsync();

        return Ok(donHangs);
    }

    // HÀM 2: HỦY ĐẶT BÀN
    [HttpPost("cancel/{maDonHang}")]
    [Authorize]
    public async Task<IActionResult> CancelBooking(string maDonHang)
    {
        var maKhachHang = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (maKhachHang == null) return Unauthorized();

        var donHang = await _context.DonHangs
            .Include(dh => dh.MaKhachHangNavigation)
            .Include(dh => dh.MaBans) 
            .FirstOrDefaultAsync(d => d.MaDonHang == maDonHang);

        if (donHang == null) return NotFound(new { Message = "Không tìm thấy đơn." });
        if (donHang.MaKhachHang != maKhachHang) return Forbid();

        if (donHang.MaTrangThaiDonHang == "DA_HUY" || donHang.MaTrangThaiDonHang == "DA_HOAN_THANH")
            return BadRequest(new { Message = "Trạng thái đơn hàng không hợp lệ để hủy." });

        var gioAn = donHang.ThoiGianDatHang ?? DateTime.Now;
        if (DateTime.Now >= gioAn)
            return BadRequest(new { Message = "Không thể hủy đơn đã diễn ra." });

        bool duocHoanTien = false;
        var gioDat = donHang.ThoiGianDatHang ?? DateTime.Now; 
        double phutTuLucDat = (DateTime.Now - gioDat).TotalMinutes;
        double gioConLai = (gioAn - DateTime.Now).TotalHours;

        if (donHang.TienDatCoc > 0 && (gioConLai >= 12 || phutTuLucDat <= 30))
        {
            duocHoanTien = true;
        }

        donHang.MaTrangThaiDonHang = "DA_HUY";
        donHang.GhiChu += duocHoanTien ? " | Khách hủy (Hoàn tiền)." : " | Khách hủy (Mất cọc).";

        foreach (var ban in donHang.MaBans)
        {
            ban.MaTrangThai = "TTBA001";
        }

        await _context.SaveChangesAsync();

        if (!string.IsNullOrEmpty(donHang.MaKhachHangNavigation.Email))
        {
            string tenBanGop = string.Join(", ", donHang.MaBans.Select(b => b.TenBan));

            _ = _emailService.SendCancellationEmailAsync(
                donHang.MaKhachHangNavigation.Email,
                donHang.MaKhachHangNavigation.HoTen,
                donHang.MaDonHang,
                tenBanGop,
                gioAn,
                donHang.TienDatCoc ?? 0,
                duocHoanTien
            );
        }

        return Ok(new { Message = "Hủy thành công.", DuocHoanTien = duocHoanTien });
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
            var sdt = dh.SDTNguoiNhan ?? dh.MaKhachHangNavigation.SoDienThoai;
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
        var donHang = await _context.DonHangs.FirstOrDefaultAsync(d => d.MaDonHang == maDonHang);

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

            string linkXacNhan = $"/api/BookingHistory/quick-cancel/{maDonHang}?confirm=true";

            string htmlConfirm = _emailService.GetHtml_XacNhanHuy(maDonHang, linkXacNhan);

            return Content(htmlConfirm, "text/html");
        }

   
        // TRƯỜNG HỢP B: ĐÃ BẤM XÁC NHẬN (confirm=true) -> TIẾN HÀNH HỦY
        donHang.MaTrangThaiDonHang = "DA_HUY";
        donHang.GhiChu += " | Khách hủy nhanh qua Email.";

        foreach (var ban in donHang.MaBans)
        {
            ban.MaTrangThai = "TTBA001"; 
        }
  
        await _context.SaveChangesAsync();

        string htmlSuccess = _emailService.GetHtml_HuyThanhCong();
        return Content(htmlSuccess, "text/html");
    }

}