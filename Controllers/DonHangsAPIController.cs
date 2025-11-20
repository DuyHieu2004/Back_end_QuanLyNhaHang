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

            foreach (var item in request.Items)
            {
                // Tìm công thức nấu ăn dựa trên Phiên bản (Size)
                var congThuc = await _context.CongThucNauAns
                    .FirstOrDefaultAsync(ct => ct.MaPhienBan == item.MaPhienBan);

                // --- SỬA ĐOẠN NÀY ---
                if (congThuc == null)
                {
                    // Đừng continue nữa, báo lỗi luôn để biết đường sửa
                    return BadRequest(new
                    {
                        message = $"Không tìm thấy công thức nấu ăn cho phiên bản món ăn: {item.MaPhienBan}. Kiểm tra lại bảng CongThucNauAn."
                    });
                }
                // --------------------

                var chiTietMoi = new ChiTietDonHang
                {
                    MaPhienBan = item.MaPhienBan,
                    MaCongThuc = congThuc.MaCongThuc,
                    SoLuong = item.SoLuong,
                   // GhiChu =  "", // Nhớ handle vụ null ghi chú
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
            // 1. Xây dựng câu truy vấn (LOGIC MỚI)
            // Vì cấu trúc DB thay đổi, ta không thể Include trực tiếp ChiTietDonHangs từ DonHang được nữa
            // Ta phải đi xuyên qua bảng trung gian BanAnDonHangs

            var query = _context.DonHangs
                // Lấy thông tin Khách và Trạng thái (Giữ nguyên)
                .Include(dh => dh.MaKhachHangNavigation)
                .Include(dh => dh.MaTrangThaiDonHangNavigation)

                // Lấy danh sách Bàn và Món ăn nằm trong bàn đó (QUAN TRỌNG)
                .Include(dh => dh.BanAnDonHangs)
                    .ThenInclude(badh => badh.MaBanNavigation) // Lấy tên bàn

                // Lấy Món ăn -> Phiên bản
                .Include(dh => dh.BanAnDonHangs)
                    .ThenInclude(badh => badh.ChiTietDonHangs)
                        .ThenInclude(ct => ct.MaPhienBanNavigation)

                // Lấy Món ăn -> Công thức -> Tên món -> Hình ảnh
                .Include(dh => dh.BanAnDonHangs)
                    .ThenInclude(badh => badh.ChiTietDonHangs)
                        .ThenInclude(ct => ct.MaCongThucNavigation)
                            .ThenInclude(ctnau => ctnau.MaCtNavigation)
                                .ThenInclude(ctma => ctma.MaMonAnNavigation)
                                    .ThenInclude(m => m.HinhAnhMonAns)

                // Tối ưu hiệu suất vì query quá nhiều bảng lồng nhau
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
                    // Logic tìm bàn: Phải chui vào bảng trung gian để tìm
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

            // 3. Kiểm tra kết quả
            if (donHang == null)
            {
                return NotFound(new { message = "Không tìm thấy thông tin đặt bàn." });
            }

            if (donHang == null) return NotFound(new { message = "Không tìm thấy thông tin đặt bàn." });

            // Lấy tên các bàn
            string tenCacBan = donHang.BanAnDonHangs != null && donHang.BanAnDonHangs.Any()
                ? string.Join(", ", donHang.BanAnDonHangs.Select(b => b.MaBanNavigation.TenBan))
                : "Chưa xếp bàn";

            // --- [QUAN TRỌNG] ---
            // Vì món ăn giờ nằm rải rác trong từng bàn (BanAnDonHangs), ta phải gom tất cả lại thành 1 danh sách chung
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

                // Map danh sách món ăn từ list đã gom
                MonAns = tatCaMonAn
                    .Select(ct =>
                    {
                        var congThuc = ct.MaCongThucNavigation;
                        var phienBan = ct.MaPhienBanNavigation;
                        var monAn = congThuc?.MaCtNavigation?.MaMonAnNavigation;
                        string hinhAnhUrl = monAn?.HinhAnhMonAns.FirstOrDefault()?.URLHinhAnh ?? "";

                        // Lấy thông tin bàn chứa món này (truy ngược lại parent của nó)
                        // Lưu ý: ct.MaBanAnDonHang là khóa ngoại mới
                        var banChuaMonNay = donHang.BanAnDonHangs.FirstOrDefault(b => b.MaBanAnDonHang == ct.MaBanAnDonHang);

                        return new MonAnDatDto
                        {
                            TenMon = monAn?.TenMonAn ?? "Món không xác định",
                            TenPhienBan = phienBan?.TenPhienBan ?? "",
                            SoLuong = ct.SoLuong,
                            DonGia = congThuc?.Gia ?? 0,
                            HinhAnh = hinhAnhUrl,

                            // Map bàn cụ thể
                            MaBan = banChuaMonNay?.MaBan ?? "",
                            TenBan = banChuaMonNay?.MaBanNavigation?.TenBan ?? "Chung",

                            // GhiChu = ct.GhiChu ?? ""
                            GhiChu = ""
                        };
                    })
                    .OrderBy(m => m.TenBan) // Gom món theo bàn cho đẹp
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