using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyNhaHang.Models;
using QuanLyNhaHang.Models.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace QuanLyNhaHang.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DonHangsAPIController : ControllerBase
    {
        private readonly QLNhaHangContext _context;

        public DonHangsAPIController(QLNhaHangContext context)
        {
            _context = context;
        }

        [HttpGet("GetMyBookingDetail")]
        [Authorize]
        public async Task<IActionResult> GetMyBookingDetail(
            [FromQuery] string? maDonHang,
            [FromQuery] string? maBan,
           // [FromQuery] string? maKhachHang,
            [FromQuery] DateTime? dateTime)
        {
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (currentUserId == null) return Unauthorized();
            var maKhachHang = currentUserId;

            var query = _context.DonHangs
                .Include(dh => dh.MaBanNavigation)
                .Include(dh => dh.ChiTietDonHangs)
                    .ThenInclude(ct => ct.MaPhienBanNavigation)
                        .ThenInclude(pb => pb.MaMonAnNavigation)
                            .ThenInclude(m => m.HinhAnhMonAns)
                .Include(dh => dh.MaTrangThaiDonHangNavigation)
                .AsQueryable();

            DonHang? donHang = null;

            if (!string.IsNullOrEmpty(maDonHang))
            {
                donHang = await query.FirstOrDefaultAsync(dh => dh.MaDonHang == maDonHang);
            }
            else if (!string.IsNullOrEmpty(maBan) && !string.IsNullOrEmpty(maKhachHang) && dateTime != null)
            {
                var gioBatDau = dateTime.Value;
                var gioKetThuc = dateTime.Value.AddMinutes(120);

                donHang = await query.FirstOrDefaultAsync(dh =>
                    dh.MaBan == maBan &&
                    dh.MaKhachHang == maKhachHang &&
                    (dh.MaTrangThaiDonHang == "CHO_XAC_NHAN" || dh.MaTrangThaiDonHang == "DA_XAC_NHAN") &&
                    (gioBatDau < dh.ThoiGianDatHang.Value.AddMinutes(120)) &&
                    (gioKetThuc > dh.ThoiGianDatHang.Value)
                );
            }
            else
            {
                return BadRequest("Thiếu thông tin tìm kiếm.");
            }

            if (donHang == null)
            {
                return NotFound("Không tìm thấy thông tin đặt bàn.");
            }

            // --- SỬA LẠI KHÚC MAP DTO (Bỏ hết mấy dấu ?? vô lý) ---
            var result = new ChiTietDatBanDTO
            {
                MaDonHang = donHang.MaDonHang,
                ThoiGianDat = donHang.ThoiGianDatHang ?? DateTime.Now, // Cái này có thể Null nên giữ lại ??
                TenBan = donHang.MaBanNavigation?.TenBan ?? "Bàn chưa xác định",

                // Lấy giờ ăn thực tế (Giả sử trong DB bạn lưu là ThoiGianAn)
                // Nếu DB bạn không có cột ThoiGianAn mà dùng chung cột ThoiGianDatHang thì logic DB bị sai nhé!
                ThoiGianNhanBan = donHang.ThoiGianBatDau,
                // SỬA Ở ĐÂY: Bỏ ?? 0
                SoNguoi = donHang.SoLuongNguoi,

                GhiChu = donHang.GhiChu,
                TienDatCoc = donHang.TienDatCoc,
                TrangThai = donHang.MaTrangThaiDonHangNavigation.TenTrangThai,

                TenNguoiDat = donHang.TenNguoiDat ?? donHang.MaKhachHangNavigation.HoTen,
                SDTNguoiDat = donHang.SDTNguoiDat ?? donHang.MaKhachHangNavigation.SoDienThoai,

                MonAns = donHang.ChiTietDonHangs.Select(ct =>
                {
                    var phienBan = ct.MaPhienBanNavigation;
                    var monAn = phienBan.MaMonAnNavigation;

                    return new MonAnDatDTO
                    {
                        TenMon = monAn.TenMonAn,
                        TenPhienBan = phienBan.TenPhienBan,

                        // SỬA Ở ĐÂY: Bỏ ?? 0
                        SoLuong = ct.SoLuong,
                        DonGia = phienBan.Gia,

                        HinhAnh = monAn.HinhAnhMonAns.FirstOrDefault()?.URLHinhAnh ?? ""
                    };
                }).ToList()
            };

            return Ok(result);
        }



        // API: Lấy danh sách các đơn hàng sắp tới mà khách KHÔNG CÓ EMAIL (Để nhân viên gọi điện)
        [HttpGet("get-customers-to-call")]
        public async Task<IActionResult> GetCustomersToCall()
        {
            var now = DateTime.Now;
            var thoiGianQuetDen = now.AddHours(24); // Lọc đơn trong 24h tới

            var listCanGoi = await _context.DonHangs
                .Include(dh => dh.MaKhachHangNavigation)
                .Where(dh =>
                    (dh.MaTrangThaiDonHang == "CHO_XAC_NHAN" || dh.MaTrangThaiDonHang == "DA_XAC_NHAN") &&
                    dh.ThoiGianBatDau > now &&
                    dh.ThoiGianBatDau <= thoiGianQuetDen &&
                    // ĐIỀU KIỆN QUAN TRỌNG: Email bị rỗng hoặc null
                    (dh.MaKhachHangNavigation.Email == null || dh.MaKhachHangNavigation.Email == "")
                )
                .Select(dh => new
                {
                    dh.MaDonHang,
                    TenKhach = dh.TenNguoiDat ?? dh.MaKhachHangNavigation.HoTen,
                    SDT = dh.SDTNguoiDat ?? dh.MaKhachHangNavigation.SoDienThoai,
                    GioHen = dh.ThoiGianBatDau,
                    SoNguoi = dh.SoLuongNguoi,
                    GhiChu = "Khách không có email, cần gọi nhắc."
                })
                .OrderBy(dh => dh.GioHen)
                .ToListAsync();

            return Ok(listCanGoi);
        }
    }
}