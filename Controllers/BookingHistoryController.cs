using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyNhaHang.Models;
using QuanLyNhaHang.Models.DTO;
using QuanLyNhaHang.Services;
using System.Security.Claims;
using System.Collections.Generic;
using System.Linq;

[ApiController]
[Route("api/[controller]")]
[Authorize] // BẮT BUỘC: Chỉ người đã đăng nhập (có Token) mới được gọi
public class BookingHistoryController : ControllerBase
{
    private readonly QLNhaHangContext _context;
    private readonly IEmailService _emailService;

    public BookingHistoryController(QLNhaHangContext context, IEmailService emailService)
    {
        _context = context;
        _emailService = emailService;
    }

    // =========================================================================
    // API 1: LẤY LỊCH SỬ ĐẶT BÀN CỦA TÔI
    // =========================================================================
    [HttpGet("me")]
    public async Task<IActionResult> GetMyBookingHistory()
    {
        var maKhachHang = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (maKhachHang == null) return Unauthorized();

        var donHangs = await _context.DonHangs
            .Where(dh => dh.MaKhachHang == maKhachHang)
            // SỬA: Đi từ ChiTietDonHang -> BanAnDonHang -> BanAn
            .Include(dh => dh.ChiTietDonHangs)
                .ThenInclude(ct => ct.BanAnDonHangs)
                    .ThenInclude(badh => badh.MaBanNavigation)
            .Include(dh => dh.MaTrangThaiDonHangNavigation)
            .OrderByDescending(dh => dh.ThoiGianDatHang)
            .Select(dh => new BookingHistoryDto
            {
                MaDonHang = dh.MaDonHang,
                // SỬA: Lấy tên bàn từ ChiTiet -> BanAnDonHang
                TenBan = string.Join(", ", dh.ChiTietDonHangs
                                    .SelectMany(ct => ct.BanAnDonHangs)
                                    .Select(b => b.MaBanNavigation.TenBan)
                                    .Distinct()),
                ThoiGianBatDau = dh.ThoiGianDatHang ?? DateTime.Now,
                ThoiGianDuKien = dh.TgdatDuKien,
                SoLuongNguoi = dh.SoLuongNguoiDK,
                GhiChu = dh.GhiChu,
                DaHuy = (dh.MaTrangThaiDonHang == "DA_HUY"),
                MaTrangThai = dh.MaTrangThaiDonHang,
                CoTheHuy = ((dh.TgdatDuKien ?? dh.ThoiGianDatHang ?? DateTime.Now) > DateTime.Now
                            && dh.MaTrangThaiDonHang != "DA_HUY"
                            && dh.MaTrangThaiDonHang != "DA_HOAN_THANH"),
                TrangThai = dh.MaTrangThaiDonHangNavigation.TenTrangThai
            })
            .ToListAsync();

        return Ok(donHangs);
    }

    // =========================================================================
    // API 2: TÌM LỊCH SỬ BẰNG SỐ ĐIỆN THOẠI (CHO NHÂN VIÊN/KHÁCH VÃNG LAI)
    // =========================================================================
    [AllowAnonymous]
    [HttpGet("by-phone/{phone}")]
    public async Task<IActionResult> GetBookingHistoryByPhone(string phone)
    {
        if (string.IsNullOrWhiteSpace(phone))
        {
            return BadRequest(new { found = false, message = "Vui lòng nhập SĐT." });
        }

        var normalizedPhone = phone.Trim();
        var khachHang = await _context.KhachHangs.FirstOrDefaultAsync(k => k.SoDienThoai == normalizedPhone);

        if (khachHang == null)
        {
            return Ok(new { found = false, message = "Không tìm thấy khách hàng.", bookings = new List<BookingHistoryDto>() });
        }

        var donHangs = await _context.DonHangs
            .Where(dh => dh.MaKhachHang == khachHang.MaKhachHang)
            // SỬA: Include đúng đường dẫn
            .Include(dh => dh.ChiTietDonHangs)
                .ThenInclude(ct => ct.BanAnDonHangs)
                    .ThenInclude(badh => badh.MaBanNavigation)
            .Include(dh => dh.MaTrangThaiDonHangNavigation)
            .OrderByDescending(dh => dh.ThoiGianDatHang)
            .Select(dh => new BookingHistoryDto
            {
                MaDonHang = dh.MaDonHang,
                // SỬA: Lấy tên bàn
                TenBan = string.Join(", ", dh.ChiTietDonHangs
                                    .SelectMany(ct => ct.BanAnDonHangs)
                                    .Select(b => b.MaBanNavigation.TenBan)
                                    .Distinct()),
                ThoiGianBatDau = dh.ThoiGianDatHang ?? DateTime.Now,
                ThoiGianDuKien = dh.TgdatDuKien,
                SoLuongNguoi = dh.SoLuongNguoiDK,
                GhiChu = dh.GhiChu,
                DaHuy = (dh.MaTrangThaiDonHang == "DA_HUY"),
                MaTrangThai = dh.MaTrangThaiDonHang,
                CoTheHuy = ((dh.TgdatDuKien ?? dh.ThoiGianDatHang ?? DateTime.Now) > DateTime.Now
                            && dh.MaTrangThaiDonHang != "DA_HUY"
                            && dh.MaTrangThaiDonHang != "DA_HOAN_THANH"),
                TrangThai = dh.MaTrangThaiDonHangNavigation.TenTrangThai
            })
            .ToListAsync();

        return Ok(new
        {
            found = true,
            message = $"Đã tìm thấy khách hàng {khachHang.HoTen}.",
            customer = new
            {
                maKhachHang = khachHang.MaKhachHang,
                hoTen = khachHang.HoTen,
                email = khachHang.Email,
                soDienThoai = khachHang.SoDienThoai,
                // SỬA: Dùng cột mới NgayCuoiCungTichLuy thay vì SoLanAnTichLuy (nếu có trong DTO)
                // soLanAn = khachHang.SoLanAnTichLuy 
            },
            bookings = donHangs
        });
    }

    // =========================================================================
    // API 3: HỦY ĐẶT BÀN (TỪ APP)
    // =========================================================================
    [HttpPost("cancel")]
    public async Task<IActionResult> CancelBooking([FromBody] CancelBookingRequest request)
    {
        var maKhachHang = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (maKhachHang == null) return Unauthorized();

        var donHang = await _context.DonHangs
            // SỬA: Include để lấy thông tin bàn cần reset trạng thái
            .Include(dh => dh.ChiTietDonHangs)
                .ThenInclude(ct => ct.BanAnDonHangs)
                    .ThenInclude(badh => badh.MaBanNavigation)
            .FirstOrDefaultAsync(d => d.MaDonHang == request.MaDonHang && d.MaKhachHang == maKhachHang);

        if (donHang == null) return NotFound(new { message = "Không tìm thấy đơn hàng." });
        if (donHang.MaTrangThaiDonHang == "DA_HUY") return BadRequest(new { message = "Đơn hàng đã hủy rồi." });
        if (donHang.MaTrangThaiDonHang == "DA_HOAN_THANH") return BadRequest(new { message = "Đơn hàng đã hoàn thành." });

        var gioAn = donHang.TgdatDuKien ?? donHang.ThoiGianDatHang ?? DateTime.Now;
        if (DateTime.Now >= gioAn) return BadRequest(new { message = "Đã quá giờ đặt, không thể hủy." });

        // --- TIẾN HÀNH HỦY ---
        donHang.MaTrangThaiDonHang = "DA_HUY";
        donHang.GhiChu += $" | Khách hủy lúc {DateTime.Now:HH:mm dd/MM}";

        // SỬA: Cập nhật trạng thái bàn về TRỐNG
        // Duyệt qua tất cả các bàn liên kết với đơn hàng này (thông qua ChiTiet)
        var allTables = donHang.ChiTietDonHangs
            .SelectMany(ct => ct.BanAnDonHangs)
            .Select(b => b.MaBanNavigation)
            .Where(b => b != null)
            .Distinct();

        foreach (var ban in allTables)
        {
            ban.MaTrangThai = "TTBA001"; // Trống
        }

        await _context.SaveChangesAsync();
        return Ok(new { message = "Hủy đặt bàn thành công." });
    }

    // =========================================================================
    // API 4: HỦY NHANH TỪ EMAIL (QUICK CANCEL)
    // =========================================================================
    [HttpGet("quick-cancel/{maDonHang}")]
    [AllowAnonymous]
    public async Task<IActionResult> QuickCancelFromEmail(string maDonHang, [FromQuery] bool confirm = false)
    {
        var donHang = await _context.DonHangs
            // SỬA: Include đúng đường dẫn
            .Include(dh => dh.ChiTietDonHangs)
                .ThenInclude(ct => ct.BanAnDonHangs)
                    .ThenInclude(badh => badh.MaBanNavigation)
            .FirstOrDefaultAsync(d => d.MaDonHang == maDonHang);

        if (donHang == null || donHang.MaTrangThaiDonHang == "DA_HUY")
        {
            string htmlLoi = _emailService.GetHtml_ThongBaoLoi("Đơn hàng không tồn tại hoặc đã hủy.");
            return Content(htmlLoi, "text/html");
        }

        if (donHang.MaTrangThaiDonHang == "DA_HOAN_THANH")
        {
            string htmlLoi = _emailService.GetHtml_ThongBaoLoi("Đơn hàng đã hoàn thành.");
            return Content(htmlLoi, "text/html");
        }

        var gioAn = donHang.TgdatDuKien ?? donHang.ThoiGianDatHang ?? DateTime.Now;
        if (DateTime.Now >= gioAn)
        {
            string htmlLoi = _emailService.GetHtml_ThongBaoLoi("Đã quá giờ đặt.");
            return Content(htmlLoi, "text/html");
        }

        // TRƯỜNG HỢP A: CHƯA XÁC NHẬN
        if (confirm == false)
        {
            string linkXacNhan = $"/api/BookingHistory/quick-cancel/{maDonHang}?confirm=true";
            string htmlConfirm = _emailService.GetHtml_XacNhanHuy(maDonHang, linkXacNhan);
            return Content(htmlConfirm, "text/html");
        }

        // TRƯỜNG HỢP B: ĐÃ XÁC NHẬN -> HỦY
        donHang.MaTrangThaiDonHang = "DA_HUY";
        donHang.GhiChu += " | Hủy nhanh qua Email.";

        // SỬA: Cập nhật trạng thái bàn
        var allTables = donHang.ChiTietDonHangs
            .SelectMany(ct => ct.BanAnDonHangs)
            .Select(b => b.MaBanNavigation)
            .Where(b => b != null)
            .Distinct();

        foreach (var ban in allTables)
        {
            ban.MaTrangThai = "TTBA001";
        }

        await _context.SaveChangesAsync();
        string htmlSuccess = _emailService.GetHtml_HuyThanhCong();
        return Content(htmlSuccess, "text/html");
    }

    // =========================================================================
    // API 5: GỬI NHẮC NHỞ (Giữ nguyên logic query DonHang cơ bản)
    // =========================================================================
    [HttpPost("send-reminders")]
    public async Task<IActionResult> SendDailyReminders()
    {
        var now = DateTime.Now;
        var limit = now.AddHours(24);

        var donHangs = await _context.DonHangs
            .Include(dh => dh.MaKhachHangNavigation)
            .Where(dh =>
                (dh.MaTrangThaiDonHang == "CHO_XAC_NHAN" || dh.MaTrangThaiDonHang == "DA_XAC_NHAN") &&
                dh.TgdatDuKien > now &&
                dh.TgdatDuKien <= limit
            ).ToListAsync();

        int emailCount = 0;
        foreach (var dh in donHangs)
        {
            var email = dh.MaKhachHangNavigation.Email;
            var ten = dh.TenNguoiNhan ?? dh.MaKhachHangNavigation.HoTen;

            if (!string.IsNullOrEmpty(email))
            {
                string baseUrl = $"{Request.Scheme}://{Request.Host}";
                string linkHuy = $"{baseUrl}/api/BookingHistory/quick-cancel/{dh.MaDonHang}";

                _ = _emailService.SendReminderEmailAsync(
                    email, ten, dh.TgdatDuKien ?? DateTime.Now, "", linkHuy
                );
                emailCount++;
            }
        }
        return Ok(new { Message = "Đã gửi nhắc nhở.", SentEmails = emailCount });
    }
}

public class CancelBookingRequest
{
    public string MaDonHang { get; set; }
}