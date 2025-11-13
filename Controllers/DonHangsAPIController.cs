using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyNhaHang.Models;
using QuanLyNhaHang.Models.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
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
        public async Task<IActionResult> GetMyBookingDetail(
    [FromQuery] string? maDonHang,
    [FromQuery] string? maBan,
    [FromQuery] string? maKhachHang,
    [FromQuery] DateTime? dateTime)
        {
            var query = _context.DonHangs
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

        // Bạn có thể thêm các API khác (TaoDatBan, HuyDatBan...) ở dưới đây
        // ...
    }
}