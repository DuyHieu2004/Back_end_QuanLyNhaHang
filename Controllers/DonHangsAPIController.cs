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

        private IQueryable<object> ProjectOrderToDTO(IQueryable<DonHang> query)
        {
            return query.Select(dh => new
            {
                maDonHang = dh.MaDonHang,
                maNhanVien = dh.MaNhanVien,
                maKhachHang = dh.MaKhachHang,
                maTrangThaiDonHang = dh.MaTrangThaiDonHang,
                tenTrangThai = dh.MaTrangThaiDonHangNavigation.TenTrangThai,
                thoiGianDatHang = dh.ThoiGianDatHang,
                tgDatDuKien = dh.TgdatDuKien,
                tgNhanBan = dh.TGNhanBan,
                thanhToan = dh.ThanhToan,
                thoiGianKetThuc = dh.ThoiGianKetThuc,
                soLuongNguoiDK = dh.SoLuongNguoiDK,
                tienDatCoc = dh.TienDatCoc,
                ghiChu = dh.GhiChu,
                tenNguoiNhan = dh.TenNguoiNhan,
                sdtNguoiNhan = dh.SdtnguoiNhan,
                emailNguoiNhan = dh.EmailNguoiNhan,
                hoTenKhachHang = dh.MaKhachHangNavigation.HoTen,
                soDienThoaiKhach = dh.MaKhachHangNavigation.SoDienThoai,
                emailKhachHang = dh.MaKhachHangNavigation.Email,
                tenNhanVien = dh.MaNhanVienNavigation.HoTen,
                danhSachBan = dh.BanAnDonHangs.Any()
                    ? string.Join(", ", dh.BanAnDonHangs.Select(badh => badh.MaBanNavigation.TenBan))
                    : null,
                tongTien = dh.ChiTietDonHangs.Sum(ct => ct.SoLuong * ct.MaCongThucNavigation.Gia)
            });
        }

        // 1. API: Lấy tất cả đơn hàng (cho tab 'all')
        // Route: GET /api/DonHangsAPI
        [HttpGet]
        public async Task<IActionResult> GetOrders()
        {
            try
            {
                var sixMonthsAgo = DateTime.Now.AddMonths(-6);

                var orders = await _context.DonHangs
                    .Include(dh => dh.MaTrangThaiDonHangNavigation)
                    .Include(dh => dh.MaKhachHangNavigation)
                    .Include(dh => dh.MaNhanVienNavigation)
                    .Include(dh => dh.BanAnDonHangs)
                        .ThenInclude(b => b.MaBanNavigation)
                    .Where(dh => dh.ThoiGianKetThuc >= sixMonthsAgo ||
                                 dh.ThoiGianKetThuc == null || // Bao gồm cả đơn chưa kết thúc
                                 dh.MaTrangThaiDonHang == "CHO_XAC_NHAN" ||
                                 dh.MaTrangThaiDonHang == "DA_XAC_NHAN" ||
                                 dh.MaTrangThaiDonHang == "CHO_THANH_TOAN" ||
                                 dh.MaTrangThaiDonHang == "DA_HOAN_THANH" ||
                                 dh.MaTrangThaiDonHang == "DA_HUY")
                    .OrderByDescending(dh => dh.ThoiGianDatHang)
                    .Select(dh => new
                    {
                        maDonHang = dh.MaDonHang,
                        maNhanVien = dh.MaNhanVien,
                        maKhachHang = dh.MaKhachHang,
                        maTrangThaiDonHang = dh.MaTrangThaiDonHang,
                        tenTrangThai = dh.MaTrangThaiDonHangNavigation.TenTrangThai,
                        thoiGianDatHang = dh.ThoiGianDatHang,
                        tgDatDuKien = dh.TgdatDuKien,
                        tgNhanBan = dh.TGNhanBan,
                        thanhToan = dh.ThanhToan,
                        thoiGianKetThuc = dh.ThoiGianKetThuc,
                        soLuongNguoiDK = dh.SoLuongNguoiDK,
                        tienDatCoc = dh.TienDatCoc ?? 0,
                        ghiChu = dh.GhiChu,
                        tenNguoiNhan = dh.TenNguoiNhan,
                        sdtNguoiNhan = dh.SdtnguoiNhan,
                        emailNguoiNhan = dh.EmailNguoiNhan,
                        hoTenKhachHang = dh.MaKhachHangNavigation.HoTen,
                        soDienThoaiKhach = dh.MaKhachHangNavigation.SoDienThoai,
                        emailKhachHang = dh.MaKhachHangNavigation.Email,
                        tenNhanVien = dh.MaNhanVienNavigation.HoTen,
                        danhSachBan = dh.BanAnDonHangs.Any()
                            ? string.Join(", ", dh.BanAnDonHangs.Select(b => b.MaBanNavigation.TenBan))
                            : null,
                        tongTien = dh.TienDatCoc ?? 0 // Tạm thời dùng tiền cọc
                    })
                    .ToListAsync();

                return Ok(orders);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi GetOrders: {ex.Message}");
                return StatusCode(500, new { message = "Lỗi server khi lấy danh sách đơn hàng", error = ex.Message });
            }
        }

        // 2. API: Lấy đơn hàng theo trạng thái cụ thể (cho tab 'completed'/'cancelled')
        // Route: GET /api/DonHangsAPI/status/{maTrangThai}
        [HttpGet("status/{maTrangThai}")]
        public async Task<IActionResult> GetOrdersByStatus([FromRoute] string maTrangThai)
        {
            try
            {
                if (string.IsNullOrEmpty(maTrangThai))
                {
                    return BadRequest(new { message = "Cần cung cấp mã trạng thái đơn hàng." });
                }

                var statusUpper = maTrangThai.ToUpper();

                // Kiểm tra trạng thái có tồn tại không
                var statusExists = await _context.TrangThaiDonHangs
                    .AnyAsync(s => s.MaTrangThai == statusUpper);

                if (!statusExists)
                {
                    return BadRequest(new { message = $"Trạng thái '{maTrangThai}' không tồn tại" });
                }

                var ordersQuery = _context.DonHangs
                    .Include(dh => dh.MaTrangThaiDonHangNavigation)
                    .Include(dh => dh.MaKhachHangNavigation)
                    .Include(dh => dh.MaNhanVienNavigation)
                    .Include(dh => dh.BanAnDonHangs).ThenInclude(badh => badh.MaBanNavigation)
                    .Include(dh => dh.ChiTietDonHangs).ThenInclude(ct => ct.MaCongThucNavigation)
                    .Where(dh => dh.MaTrangThaiDonHang == statusUpper)
                    .OrderByDescending(dh => dh.ThoiGianKetThuc ?? dh.ThoiGianDatHang);

                var orders = await ProjectOrderToDTO(ordersQuery).ToListAsync();

                return Ok(orders);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi GetOrdersByStatus: {ex.Message}");
                return StatusCode(500, new { message = "Lỗi server khi lấy đơn hàng", error = ex.Message });
            }
        }


        // 3. API: Lấy thống kê đơn hàng (cho phần stats của OrdersManagement)
        // Route: GET /api/DonHangsAPI/stats
        [HttpGet("stats")]
        public async Task<IActionResult> GetOrderStats()
        {
            try
            {
                var today = DateTime.Today;

                // Tính tổng doanh thu hôm nay - xử lý null an toàn
                var totalRevenueToday = await _context.DonHangs
                    .Where(dh => dh.MaTrangThaiDonHang == "DA_HOAN_THANH" &&
                                 dh.ThoiGianKetThuc.HasValue &&
                                 dh.ThoiGianKetThuc.Value.Date == today)
                    .Include(dh => dh.ChiTietDonHangs)
                        .ThenInclude(ct => ct.MaCongThucNavigation)
                    .SelectMany(dh => dh.ChiTietDonHangs)
                    .SumAsync(ct => ct.SoLuong * (ct.MaCongThucNavigation.Gia));

                // Đếm số đơn theo trạng thái
                var statusCounts = await _context.DonHangs
                    .GroupBy(dh => dh.MaTrangThaiDonHang)
                    .Select(g => new { Status = g.Key, Count = g.Count() })
                    .ToListAsync();

                var stats = new
                {
                    tongSoDon = statusCounts.Sum(s => s.Count),
                    tongDoanhThu = totalRevenueToday,
                    donHoanThanh = statusCounts.FirstOrDefault(s => s.Status == "DA_HOAN_THANH")?.Count ?? 0,
                    donDaHuy = statusCounts.FirstOrDefault(s => s.Status == "DA_HUY")?.Count ?? 0,
                    donChoXacNhan = statusCounts.FirstOrDefault(s => s.Status == "CHO_XAC_NHAN")?.Count ?? 0,
                };

                return Ok(stats);
            }
            catch (Exception ex)
            {
                // Log lỗi để debug
                Console.WriteLine($"Lỗi GetOrderStats: {ex.Message}");
                return StatusCode(500, new { message = "Lỗi server khi lấy thống kê", error = ex.Message });
            }
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
                    dh.TGNhanBan.HasValue &&
                    dh.TGNhanBan.Value.Date == filterDate
                )
                .OrderBy(dh => dh.TGNhanBan)
                .Select(dh => new
                {
                    maDonHang = dh.MaDonHang,
                    tenNguoiNhan = dh.TenNguoiNhan ?? dh.MaKhachHangNavigation.HoTen,
                    soNguoi = dh.SoLuongNguoiDK,
                    thoiGianNhanBan = dh.TGNhanBan,
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

                    (gioBatDau < dh.TGNhanBan.Value.AddMinutes(120)) &&
                    (gioBatDau.AddMinutes(120) > dh.TGNhanBan.Value)
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
                ThoiGianNhanBan = donHang.TGNhanBan,
                ThoiGianKetThuc = donHang.ThoiGianKetThuc,
                SoNguoi = donHang.SoLuongNguoiDK,
                GhiChu = donHang.GhiChu,
                TienDatCoc = donHang.TienDatCoc,
                TrangThai = donHang.MaTrangThaiDonHangNavigation.TenTrangThai,
                TenNguoiDat = donHang.TenNguoiNhan ?? donHang.MaKhachHangNavigation.HoTen,
                SDTNguoiDat = donHang.SdtnguoiNhan ?? donHang.MaKhachHangNavigation.SoDienThoai,

                // Map danh sách món ăn từ list đã gom
                MonAns = tatCaMonAn
                    .Select(ct =>
                    {
                        var congThuc = ct.MaCongThucNavigation;
                        var phienBan = ct.MaPhienBanNavigation;
                        var monAn = congThuc?.MaCtNavigation?.MaMonAnNavigation;
                        string hinhAnhUrl = monAn?.HinhAnhMonAns.FirstOrDefault()?.UrlhinhAnh ?? "";

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
                    dh.TGNhanBan != null && // Kiểm tra null
                    dh.TGNhanBan > now &&   // Dùng TGNhanBan thay vì ThoiGianBatDau
                    dh.TGNhanBan <= thoiGianQuetDen &&
                    (dh.MaKhachHangNavigation.Email == null || dh.MaKhachHangNavigation.Email == "")
                )
                .Select(dh => new
                {
                    dh.MaDonHang,
                    TenKhach = dh.TenNguoiNhan ?? dh.MaKhachHangNavigation.HoTen,
                    SDT = dh.SdtnguoiNhan ?? dh.MaKhachHangNavigation.SoDienThoai,
                    GioHen = dh.TGNhanBan,
                    SoNguoi = dh.SoLuongNguoiDK, // Update tên biến
                    GhiChu = "Khách không có email, cần gọi nhắc."
                })
                .OrderBy(dh => dh.GioHen)
                .ToListAsync();

            return Ok(listCanGoi);
        }
    }
}