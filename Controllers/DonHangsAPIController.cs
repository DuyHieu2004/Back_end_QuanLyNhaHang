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


        public class ThemMonVaoBanRequest
        {
            public string MaDonHang { get; set; }
            public string MaBan { get; set; }
            public List<MonAnOrderDTO> Items { get; set; }
        }

        public class MonAnOrderDTO
        {
            public string MaMonAn { get; set; } // Để tham khảo
            public string MaPhienBan { get; set; } // Kích thước (Nhỏ/Lớn...)
            public int SoLuong { get; set; }
            public string GhiChu { get; set; }
        }

        [HttpPost("ThemMonVaoBan")]
        public async Task<IActionResult> ThemMonVaoBan([FromBody] ThemMonVaoBanRequest request)
        {
            // 1. Tìm liên kết giữa Bàn và Đơn Hàng
            var lienKetBanDon = await _context.BanAnDonHangs
                .FirstOrDefaultAsync(x => x.MaDonHang == request.MaDonHang && x.MaBan == request.MaBan);

            if (lienKetBanDon == null)
            {
                return BadRequest(new { message = "Bàn này không thuộc đơn hàng này (hoặc chưa được ghép vào đơn)." });
            }

            // 2. Duyệt qua danh sách món ăn
            foreach (var item in request.Items)
            {
                var congThuc = await _context.CongThucNauAns
                    .FirstOrDefaultAsync(ct => ct.MaPhienBan == item.MaPhienBan);

                if (congThuc == null) continue;

                var chiTietMoi = new ChiTietDonHang
                {
                    // --- ĐÃ SỬA: Bỏ dòng gán MaDonHang tại đây ---
                    // MaDonHang = request.MaDonHang, // <--- XÓA DÒNG NÀY VÌ TRONG DB KHÔNG CÒN CỘT NÀY

                    MaPhienBan = item.MaPhienBan,
                    MaCongThuc = congThuc.MaCongThuc,
                    SoLuong = item.SoLuong,

                    // QUAN TRỌNG: Món ăn giờ chỉ bám vào mã liên kết bàn-đơn
                    MaBanAnDonHang = lienKetBanDon.MaBanAnDonHang
                };

                _context.ChiTietDonHangs.Add(chiTietMoi);
            }

            await _context.SaveChangesAsync();
            return Ok(new { message = "Thêm món thành công" });
        }


        [HttpGet("GetActiveBookings")]
        public async Task<IActionResult> GetActiveBookings([FromQuery] DateTime? ngay)
        {
            DateTime filterDate = ngay?.Date ?? DateTime.Today;
            var activeStatuses = new[] { "CHO_XAC_NHAN", "DA_XAC_NHAN", "CHO_THANH_TOAN" };

            var bookings = await _context.DonHangs
                .Include(dh => dh.BanAnDonHangs)
                    .ThenInclude(badh => badh.MaBanNavigation)
                .Include(dh => dh.MaTrangThaiDonHangNavigation)
                .Where(dh =>
                    activeStatuses.Contains(dh.MaTrangThaiDonHang) &&
                    dh.TgnhanBan.HasValue &&
                    dh.TgnhanBan.Value.Date == filterDate
                )
                .OrderBy(dh => dh.TgnhanBan)
                .Select(dh => new
                {
                    maDonHang = dh.MaDonHang,
                    tenNguoiNhan = dh.TenNguoiNhan ?? dh.MaKhachHangNavigation.HoTen,
                    soNguoi = dh.SoLuongNguoiDk,
                    thoiGianNhanBan = dh.TgnhanBan,
                    trangThai = dh.MaTrangThaiDonHangNavigation.TenTrangThai,
                    maTrangThai = dh.MaTrangThaiDonHang,
                    listMaBan = dh.BanAnDonHangs.Select(b => b.MaBan).ToList(),
                    banAn = dh.BanAnDonHangs.Select(b => b.MaBanNavigation.TenBan).ToList()
                })
                .ToListAsync();

            return Ok(bookings);
        }

        [HttpGet("GetMyBookingDetail")]
        public async Task<IActionResult> GetMyBookingDetail(
            [FromQuery] string? maDonHang,
            [FromQuery] string? maBan,
            [FromQuery] DateTime? dateTime)
        {
            // --- ĐÃ SỬA: Logic Include thay đổi hoàn toàn ---
            // Không đi trực tiếp DonHang -> ChiTietDonHang được nữa
            // Phải đi DonHang -> BanAnDonHangs -> ChiTietDonHangs

            var query = _context.DonHangs
                // 1. Lấy Bàn và lấy luôn Chi Tiết Đơn Hàng nằm trong bàn đó
                .Include(dh => dh.BanAnDonHangs)
                    .ThenInclude(badh => badh.MaBanNavigation)
                .Include(dh => dh.BanAnDonHangs)
                    .ThenInclude(badh => badh.ChiTietDonHangs) // <--- Lấy món ăn qua đường này
                        .ThenInclude(ct => ct.MaPhienBanNavigation)

                // 2. Lấy sâu hơn vào Công thức -> Món ăn -> Hình ảnh (Đi từ ChiTietDonHang trong BanAnDonHangs)
                .Include(dh => dh.BanAnDonHangs)
                    .ThenInclude(badh => badh.ChiTietDonHangs)
                        .ThenInclude(ct => ct.MaCongThucNavigation)
                            .ThenInclude(ctnau => ctnau.MaCtNavigation)
                                .ThenInclude(ctma => ctma.MaMonAnNavigation)
                                    .ThenInclude(m => m.HinhAnhMonAns)

                .Include(dh => dh.MaKhachHangNavigation)
                .Include(dh => dh.MaTrangThaiDonHangNavigation)
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
                donHang = await query.FirstOrDefaultAsync(dh =>
                    dh.BanAnDonHangs.Any(b => b.MaBan == maBan) &&
                    (dh.MaTrangThaiDonHang == "CHO_XAC_NHAN" ||
                     dh.MaTrangThaiDonHang == "DA_XAC_NHAN" ||
                     dh.MaTrangThaiDonHang == "CHO_THANH_TOAN") &&
                    (gioBatDau < dh.TgnhanBan.Value.AddMinutes(120)) &&
                    (gioBatDau.AddMinutes(120) > dh.TgnhanBan.Value)
                );
            }
            else
            {
                return BadRequest(new { message = "Thiếu thông tin tìm kiếm." });
            }

            if (donHang == null) return NotFound(new { message = "Không tìm thấy thông tin đặt bàn." });

            string tenCacBan = donHang.BanAnDonHangs != null && donHang.BanAnDonHangs.Any()
                ? string.Join(", ", donHang.BanAnDonHangs.Select(b => b.MaBanNavigation.TenBan))
                : "Chưa xếp bàn";

            // --- ĐÃ SỬA: Logic gom món ăn ---
            // Vì món ăn giờ nằm rải rác trong các Bàn (BanAnDonHangs), ta phải dùng SelectMany để gom lại
            var tatCaMonAn = donHang.BanAnDonHangs.SelectMany(b => b.ChiTietDonHangs).ToList();

            var result = new ChiTietDatBanDto
            {
                MaDonHang = donHang.MaDonHang,
                ThoiGianDat = donHang.ThoiGianDatHang ?? DateTime.Now,
                TenBan = tenCacBan,
                ThoiGianNhanBan = donHang.TgnhanBan,
                ThoiGianKetThuc = donHang.ThoiGianKetThuc,
                SoNguoi = donHang.SoLuongNguoiDk,
                GhiChu = donHang.GhiChu,
                TienDatCoc = donHang.TienDatCoc,
                TrangThai = donHang.MaTrangThaiDonHangNavigation.TenTrangThai,
                TenNguoiDat = donHang.TenNguoiNhan ?? donHang.MaKhachHangNavigation.HoTen,
                SDTNguoiDat = donHang.SDTNguoiNhan ?? donHang.MaKhachHangNavigation.SoDienThoai,

                // Map từ danh sách đã gom
                MonAns = tatCaMonAn
                    .Select(ct =>
                    {
                        var congThuc = ct.MaCongThucNavigation;
                        var phienBan = ct.MaPhienBanNavigation;
                        var monAn = congThuc?.MaCtNavigation?.MaMonAnNavigation;
                        string hinhAnhUrl = monAn?.HinhAnhMonAns.FirstOrDefault()?.URLHinhAnh ?? "";

                        // Lấy lại thông tin bàn từ khóa ngoại trong ChiTiet (vẫn còn giữ MaBanAnDonHang)
                        // Lưu ý: Ở đây phải truy cập ngược lại cha của nó
                        var banInfo = donHang.BanAnDonHangs.FirstOrDefault(b => b.MaBanAnDonHang == ct.MaBanAnDonHang);

                        return new MonAnDatDto
                        {
                            TenMon = monAn?.TenMonAn ?? "Món không xác định",
                            TenPhienBan = phienBan?.TenPhienBan ?? "",
                            SoLuong = ct.SoLuong,
                            DonGia = congThuc?.Gia ?? 0,
                            HinhAnh = hinhAnhUrl,
                            MaBan = banInfo?.MaBan ?? "",
                            TenBan = banInfo?.MaBanNavigation?.TenBan ?? "Chung",
                            GhiChu = ""
                        };
                    })
                    .OrderBy(m => m.TenBan)
                    .ToList()
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