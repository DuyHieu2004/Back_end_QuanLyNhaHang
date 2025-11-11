using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyNhaHang.Models;
using QuanLyNhaHang.Models.DTO;
using System.Security.Claims;

[ApiController]
[Route("api/[controller]")]
[Authorize] // BẮT BUỘC: Chỉ người đã đăng nhập (có Token) mới được gọi
public class BookingHistoryController : ControllerBase
{
    private readonly QLNhaHangContext _context;

    public BookingHistoryController(QLNhaHangContext context)
    {
        _context = context;
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
    public async Task<IActionResult> CancelBooking(string maDonHang)
    {
        // 1. Lấy MaKhachHang từ Token
        var maKhachHang = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (maKhachHang == null) return Unauthorized();

        // 2. Tìm đơn hàng
        var donHang = await _context.DonHangs.FindAsync(maDonHang);

        if (donHang == null)
        {
            return NotFound("Không tìm thấy đơn đặt bàn.");
        }

        // 3. KIỂM TRA BẢO MẬT (QUAN TRỌNG)
        // Kiểm tra xem có đúng là chủ của đơn hàng không
        if (donHang.MaKhachHang != maKhachHang)
        {
            return Forbid("Bạn không có quyền hủy đơn này.");
        }

        // 4. KIỂM TRA NGHIỆP VỤ
        if (donHang.ThoiGianBatDau <= DateTime.Now)
        {
            return BadRequest("Không thể hủy đặt bàn đã (hoặc đang) diễn ra.");
        }

        if (donHang.GhiChu == "DA_HUY")
        {
            return BadRequest("Đơn này đã được hủy trước đó.");
        }

        // 5. Cập nhật trạng thái hủy
        donHang.MaTrangThaiDonHang = "DA_HUY";
        _context.DonHangs.Update(donHang);
        await _context.SaveChangesAsync();

        return Ok(new { Message = "Hủy đặt bàn thành công." });
    }
}