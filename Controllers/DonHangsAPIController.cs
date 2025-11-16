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
        // [Authorize] 
        public async Task<IActionResult> GetMyBookingDetail(
            [FromQuery] string? maDonHang,
            [FromQuery] string? maBan,
            [FromQuery] DateTime? dateTime)
        {
            // Logic query không thay đổi (đã giải thích ở trên)
            var query = _context.DonHangs
                .Include(dh => dh.MaBans)
                .Include(dh => dh.MaKhachHangNavigation)
                .Include(dh => dh.MaTrangThaiDonHangNavigation)
                .Include(dh => dh.ChiTietDonHangs)
                    .ThenInclude(ct => ct.MaPhienBanNavigation)
                .Include(dh => dh.ChiTietDonHangs)
                    .ThenInclude(ct => ct.MaCongThucNavigation)
                        .ThenInclude(ctnau => ctnau.MaCtNavigation)
                            .ThenInclude(ctma => ctma.MaMonAnNavigation)
                                .ThenInclude(m => m.HinhAnhMonAns)
                .AsSplitQuery()
                .AsQueryable();

            DonHang? donHang = null;

            if (!string.IsNullOrEmpty(maDonHang))
            {
                donHang = await query.FirstOrDefaultAsync(dh => dh.MaDonHang == maDonHang);
            }
            else if (!string.IsNullOrEmpty(maBan) && dateTime != null)
            {
                var gioBatDau = dateTime.Value;
                // Logic tìm bàn trong danh sách N-N:
                donHang = await query.FirstOrDefaultAsync(dh =>
                    // Trong danh sách bàn của đơn hàng, có bàn nào trùng mã bàn cần tìm không?
                    dh.MaBans.Any(b => b.MaBan == maBan) &&

                    (dh.MaTrangThaiDonHang == "CHO_XAC_NHAN" || dh.MaTrangThaiDonHang == "DA_XAC_NHAN" || dh.MaTrangThaiDonHang == "CHO_THANH_TOAN") &&

                    (gioBatDau < dh.TgnhanBan.Value.AddMinutes(120)) &&
                    (gioBatDau.AddMinutes(120) > dh.TgnhanBan.Value)
                );
            }
            else
            {
                return BadRequest(new { message = "Thiếu thông tin tìm kiếm." });
            }

            if (donHang == null)
            {
                return NotFound(new { message = "Không tìm thấy thông tin đặt bàn." });
            }

            // --- MAP DATA ---
            string tenCacBan = donHang.MaBans.Any()
                ? string.Join(", ", donHang.MaBans.Select(b => b.TenBan))
                : "Chưa xếp bàn";

            var result = new ChiTietDatBanDTO
            {
                MaDonHang = donHang.MaDonHang,
                ThoiGianDat = donHang.ThoiGianDatHang ?? DateTime.Now,
                TenBan = tenCacBan,
                ThoiGianNhanBan = donHang.TgnhanBan,

                // Thêm dòng này để trả về giờ kết thúc (dự kiến hoặc thực tế)
                ThoiGianKetThuc = donHang.ThoiGianKetThuc,

                SoNguoi = donHang.SoLuongNguoiDk,
                GhiChu = donHang.GhiChu,
                TienDatCoc = donHang.TienDatCoc,
                TrangThai = donHang.MaTrangThaiDonHangNavigation.TenTrangThai,

                TenNguoiDat = donHang.TenNguoiNhan ?? donHang.MaKhachHangNavigation.HoTen,
                SDTNguoiDat = donHang.SDTNguoiNhan ?? donHang.MaKhachHangNavigation.SoDienThoai,

                MonAns = donHang.ChiTietDonHangs.Select(ct =>
                {
                    var congThuc = ct.MaCongThucNavigation;
                    var phienBan = ct.MaPhienBanNavigation;
                    var monAn = congThuc?.MaCtNavigation?.MaMonAnNavigation;
                    string hinhAnhUrl = monAn?.HinhAnhMonAns.FirstOrDefault()?.URLHinhAnh ?? ""; // Check lại URLHinhAnh/UrlhinhAnh

                    return new MonAnDatDTO
                    {
                        TenMon = monAn?.TenMonAn ?? "Món không xác định",
                        TenPhienBan = phienBan?.TenPhienBan ?? "",
                        SoLuong = ct.SoLuong,
                        DonGia = congThuc?.Gia ?? 0,
                        HinhAnh = hinhAnhUrl
                    };
                }).ToList()
            };

            return Ok(result);
        }
    

        // API: Lấy danh sách cần gọi điện (Giữ nguyên logic nhưng update tên biến)
        [HttpGet("get-customers-to-call")]
        public async Task<IActionResult> GetCustomersToCall()
        {
            var now = DateTime.Now;
            var thoiGianQuetDen = now.AddHours(24);

            var listCanGoi = await _context.DonHangs
                .Include(dh => dh.MaKhachHangNavigation)
                .Where(dh =>
                    (dh.MaTrangThaiDonHang == "CHO_XAC_NHAN" || dh.MaTrangThaiDonHang == "DA_XAC_NHAN") &&
                    dh.TgnhanBan != null && // Kiểm tra null
                    dh.TgnhanBan > now &&   // Dùng TgnhanBan thay vì ThoiGianBatDau
                    dh.TgnhanBan <= thoiGianQuetDen &&
                    (dh.MaKhachHangNavigation.Email == null || dh.MaKhachHangNavigation.Email == "")
                )
                .Select(dh => new
                {
                    dh.MaDonHang,
                    TenKhach = dh.TenNguoiNhan ?? dh.MaKhachHangNavigation.HoTen,
                    SDT = dh.SDTNguoiNhan ?? dh.MaKhachHangNavigation.SoDienThoai,
                    GioHen = dh.TgnhanBan,
                    SoNguoi = dh.SoLuongNguoiDk, // Update tên biến
                    GhiChu = "Khách không có email, cần gọi nhắc."
                })
                .OrderBy(dh => dh.GioHen)
                .ToListAsync();

            return Ok(listCanGoi);
        }
    }
}