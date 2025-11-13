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
    private readonly  QLNhaHangContext _context;

    private readonly IEmailService _emailService; // Inject thêm Email Service

    public BookingHistoryController(QLNhaHangContext context, IEmailService emailService)
    {
        _context = context;
        _emailService = emailService;
    }

    // HÀM 1: LẤY LỊCH SỬ CỦA TÔI
    [HttpGet("me")]
    public async Task<IActionResult> GetMyBookingHistory()
    {
        // 1. Lấy MaKhachHang từ Token (y hệt AuthController)
        var maKhachHang = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (maKhachHang == null)
        {
            return Unauthorized();
        }

        // 2. Truy vấn Database
        var donHangs = await _context.DonHangs
            .Where(dh => dh.MaKhachHang == maKhachHang)
            .Include(dh => dh.MaBanNavigation) // Join với bảng BanAn
            .Include(dh => dh.MaTrangThaiDonHangNavigation)
            .OrderByDescending(dh => dh.ThoiGianDatHang) // Sắp xếp mới nhất lên đầu
            .Select(dh => new BookingHistoryDto
            {
                MaDonHang = dh.MaDonHang,
                TenBan = dh.MaBanNavigation.TenBan,
                ThoiGianBatDau = dh.ThoiGianBatDau ?? dh.ThoiGianDatHang ?? DateTime.Now,
                SoLuongNguoi = dh.SoLuongNguoi,
                GhiChu = dh.GhiChu,
                DaHuy = (dh.MaTrangThaiDonHang == "DA_HUY"),
                // "Có thể hủy" NẾU: chưa diễn ra VÀ chưa bị hủy
                CoTheHuy = ((dh.ThoiGianBatDau ?? dh.ThoiGianDatHang) > DateTime.Now && dh.MaTrangThaiDonHang != "DA_HUY"),
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
            .Include(dh => dh.MaBanNavigation)
            .FirstOrDefaultAsync(d => d.MaDonHang == maDonHang);

        if (donHang == null) return NotFound(new { Message = "Không tìm thấy đơn." });
        if (donHang.MaKhachHang != maKhachHang) return Forbid();

        // Check logic: Không thể hủy nếu đã hủy hoặc đã hoàn thành
        if (donHang.MaTrangThaiDonHang == "DA_HUY" || donHang.MaTrangThaiDonHang == "DA_HOAN_THANH")
            return BadRequest(new { Message = "Trạng thái đơn hàng không hợp lệ để hủy." });

        // Check thời gian: Không thể hủy nếu đã qua giờ ăn
        var gioAn = donHang.ThoiGianBatDau ?? donHang.ThoiGianDatHang ?? DateTime.Now;
        if (DateTime.Now >= gioAn)
            return BadRequest(new { Message = "Không thể hủy đơn đã diễn ra." });

        // --- LOGIC HOÀN TIỀN ---
        bool duocHoanTien = false;
        var gioDat = donHang.ThoiGianDatHang ?? DateTime.Now;
        double phutTuLucDat = (DateTime.Now - gioDat).TotalMinutes;
        double gioConLai = (gioAn - DateTime.Now).TotalHours;

        // Điều kiện hoàn tiền: Hủy trước 12h HOẶC Hủy trong 30p đầu tiên (Ân hạn)
        if (donHang.TienDatCoc > 0 && (gioConLai >= 12 || phutTuLucDat <= 30))
        {
            duocHoanTien = true;
        }

        // --- CẬP NHẬT DB ---
        donHang.MaTrangThaiDonHang = "DA_HUY";
        donHang.GhiChu += duocHoanTien ? " | Khách hủy (Hoàn tiền)." : " | Khách hủy (Mất cọc).";

        // Trả bàn về trạng thái TRỐNG
        var banAn = await _context.BanAns.FindAsync(donHang.MaBan);
        if (banAn != null) banAn.MaTrangThai = "TTBA001";

        await _context.SaveChangesAsync();

        // --- GỬI MAIL THÔNG BÁO ---
        if (!string.IsNullOrEmpty(donHang.MaKhachHangNavigation.Email))
        {
            // Gọi Service đã viết bên SendGridEmailService
            _ = _emailService.SendCancellationEmailAsync(
                donHang.MaKhachHangNavigation.Email,
                donHang.MaKhachHangNavigation.HoTen,
                donHang.MaDonHang,
                donHang.MaBanNavigation.TenBan,
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
                dh.ThoiGianBatDau > now &&
                dh.ThoiGianBatDau <= limit
            ).ToListAsync();

        int emailCount = 0;
        int smsCount = 0;

        foreach (var dh in donHangs)
        {
            // Ưu tiên lấy thông tin người đi ăn thực tế (nếu có lưu), không thì lấy chủ tài khoản
            var email = dh.MaKhachHangNavigation.Email;
            var sdt = dh.SDTNguoiDat ?? dh.MaKhachHangNavigation.SoDienThoai;
            var ten = dh.TenNguoiDat ?? dh.MaKhachHangNavigation.HoTen;

            // 1. ƯU TIÊN GỬI EMAIL
            if (!string.IsNullOrEmpty(email))
            {

                string baseUrl = $"{Request.Scheme}://{Request.Host}";

                // Ghép vào API
                string linkHuy = $"{baseUrl}/api/BookingHistory/quick-cancel/{dh.MaDonHang}";


                _ = _emailService.SendReminderEmailAsync(
                    email, ten, dh.ThoiGianBatDau ?? DateTime.Now, "", linkHuy
                );
                emailCount++;
            }
            // 2. KHÔNG CÓ EMAIL -> GIẢ LẬP GỬI SMS
            else
            {
                // In ra console server để thầy cô thấy là có xử lý logic này
                Console.WriteLine($"[SMS MOCK] Gửi đến {sdt}: Chào {ten}, nhắc bạn có lịch đặt bàn lúc {dh.ThoiGianBatDau:HH:mm}.");
                smsCount++;
            }
        }

        return Ok(new { Message = "Đã chạy tiến trình nhắc nhở.", SentEmails = emailCount, MockSMS = smsCount });
    }

    // ENDPOINT 3: HỦY NHANH TỪ EMAIL (Quick Cancel)
    // Lưu ý: Dùng HttpGet vì bấm link trong mail là thao tác GET
    [HttpGet("quick-cancel/{maDonHang}")]
    [AllowAnonymous] // Cho phép bấm từ mail mà không cần đăng nhập
    public async Task<IActionResult> QuickCancelFromEmail(string maDonHang, [FromQuery] bool confirm = false)
    {
        // 1. Tìm đơn hàng trong Database
        var donHang = await _context.DonHangs.FirstOrDefaultAsync(d => d.MaDonHang == maDonHang);

        // 2. Kiểm tra: Đơn không tồn tại hoặc Đã hủy
        if (donHang == null || donHang.MaTrangThaiDonHang == "DA_HUY")
        {
            string htmlLoi = _emailService.GetHtml_ThongBaoLoi("Đơn hàng không tồn tại hoặc đã được hủy trước đó.");
            return Content(htmlLoi, "text/html");
        }

        // 3. Kiểm tra: Đơn đã hoàn thành
        if (donHang.MaTrangThaiDonHang == "DA_HOAN_THANH")
        {
            string htmlLoi = _emailService.GetHtml_ThongBaoLoi("Đơn hàng đã hoàn thành, không thể hủy.");
            return Content(htmlLoi, "text/html");
        }

        // 4. Kiểm tra thời gian: Đã quá giờ ăn chưa?
        var gioAn = donHang.ThoiGianBatDau ?? donHang.ThoiGianDatHang ?? DateTime.Now;
        if (DateTime.Now >= gioAn)
        {
            string htmlLoi = _emailService.GetHtml_ThongBaoLoi("Không thể hủy đơn hàng vì thời gian đặt bàn đã diễn ra.");
            return Content(htmlLoi, "text/html");
        }

        // ========================================================================
        // TRƯỜNG HỢP A: CHƯA XÁC NHẬN -> HIỆN TRANG WEB CÓ NÚT BẤM
        // ========================================================================
        if (confirm == false)
        {
            // Tạo link xác nhận (gọi lại chính API này với confirm=true)
            // Lưu ý: Đường dẫn này là tương đối, trình duyệt sẽ tự hiểu dựa trên domain hiện tại
            string linkXacNhan = $"/api/BookingHistory/quick-cancel/{maDonHang}?confirm=true";

            // Lấy HTML từ Service
            string htmlConfirm = _emailService.GetHtml_XacNhanHuy(maDonHang, linkXacNhan);

            return Content(htmlConfirm, "text/html");
        }

        // ========================================================================
        // TRƯỜNG HỢP B: ĐÃ BẤM XÁC NHẬN (confirm=true) -> TIẾN HÀNH HỦY
        // ========================================================================

        // 1. Cập nhật trạng thái đơn hàng
        donHang.MaTrangThaiDonHang = "DA_HUY";
        donHang.GhiChu += " | Khách hủy nhanh qua Email.";

        // 2. Cập nhật trạng thái bàn về TRỐNG (Để người khác đặt)
        var banAn = await _context.BanAns.FindAsync(donHang.MaBan);
        if (banAn != null)
        {
            banAn.MaTrangThai = "TTBA001"; // Mã trạng thái Trống
        }

        // 3. Lưu thay đổi vào Database
        await _context.SaveChangesAsync();

        // 4. Trả về trang thông báo thành công
        string htmlSuccess = _emailService.GetHtml_HuyThanhCong();
        return Content(htmlSuccess, "text/html");
    }

}