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
            public string MaBan { get; set; } // Món này được gọi cho bàn nào
            public List<MonAnOrderDTO> Items { get; set; }
        }

        public class MonAnOrderDTO
        {
            public string MaMonAn { get; set; }
            public string MaPhienBan { get; set; }
            public int SoLuong { get; set; }
            public string GhiChu { get; set; }
        }

        // =============================================================
        // API: THÊM MÓN (SỬA LẠI LOGIC: TẠO CHI TIẾT -> GÁN BÀN)
        // =============================================================
        [HttpPost("ThemMonVaoBan")]
        public async Task<IActionResult> ThemMonVaoBan([FromBody] ThemMonVaoBanRequest request)
        {
            // Kiểm tra đơn hàng có tồn tại không
            var donHang = await _context.DonHangs.FindAsync(request.MaDonHang);
            if (donHang == null) return NotFound(new { message = "Không tìm thấy đơn hàng" });

            // Kiểm tra bàn có tồn tại không
            var banAn = await _context.BanAns.FindAsync(request.MaBan);
            if (banAn == null) return NotFound(new { message = "Không tìm thấy bàn ăn" });

            try
            {
                foreach (var item in request.Items)
                {
                    // 1. Tìm công thức
                    var congThuc = await _context.CongThucNauAns
                        .Include(ct => ct.MaCtNavigation)
                        .FirstOrDefaultAsync(ct =>
                            ct.MaPhienBan == item.MaPhienBan &&
                            ct.MaCtNavigation.MaMonAn == item.MaMonAn
                        );

                    if (congThuc == null)
                    {
                        return BadRequest(new { message = $"Không tìm thấy món: {item.MaMonAn} - {item.MaPhienBan}" });
                    }

                    // 2. Tạo Chi Tiết Đơn Hàng (Liên kết với DonHang)
                    var maChiTietMoi = "CTDH" + DateTime.Now.Ticks.ToString().Substring(10) + new Random().Next(100, 999); // Sinh ID tạm

                    var chiTietMoi = new ChiTietDonHang
                    {
                        MaChiTietDonHang = maChiTietMoi,
                        MaDonHang = request.MaDonHang,
                        MaPhienBan = item.MaPhienBan,
                        MaCongThuc = congThuc.MaCongThuc,
                        SoLuong = item.SoLuong
                    };
                    _context.ChiTietDonHangs.Add(chiTietMoi);

                    // Lưu ChiTietDonHang trước để có ID (nếu DB tự tăng thì cần SaveChanges ở đây, nhưng bạn dùng varchar nên add luôn được)

                    // 3. Tạo BanAnDonHang (Liên kết ChiTietDonHang với BanAn)
                    // Vì theo Diagram: BanAnDonHang nối với ChiTietDonHang
                    var banAnDonHang = new BanAnDonHang
                    {
                        MaBanAnDonHang = "BADH" + DateTime.Now.Ticks.ToString().Substring(10) + new Random().Next(100, 999),
                        MaBan = request.MaBan,
                        MaChiTietDonHang = maChiTietMoi,
                        // Lưu ý: Trong Diagram bạn gửi, BanAnDonHang KHÔNG có MaDonHang
                        // Nó chỉ nối ChiTietDonHang <-> BanAn
                    };
                    _context.BanAnDonHangs.Add(banAnDonHang);
                }

                await _context.SaveChangesAsync();
                return Ok(new { message = "Thêm món thành công" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi server: " + ex.Message });
            }
        }

        // =============================================================
        // HÀM HELPER: CHUYỂN ĐỔI DỮ LIỆU (ĐI THEO ĐƯỜNG DẪN MỚI)
        // Đường dẫn: DonHang -> ChiTietDonHangs -> BanAnDonHangs -> MaBanNavigation
        // =============================================================
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

                // LẤY DANH SÁCH BÀN: Phải đi từ ChiTietDonHangs -> BanAnDonHangs
                danhSachBan = dh.ChiTietDonHangs
                    .SelectMany(ct => ct.BanAnDonHangs)
                    .Select(badh => badh.MaBanNavigation.TenBan)
                    .Distinct() // Loại bỏ bàn trùng lặp
                    .ToList(),

                // TÍNH TỔNG TIỀN: Chỉ cần dựa vào ChiTietDonHang
                tongTien = dh.ChiTietDonHangs
                    .Sum(ct => ct.SoLuong * ct.MaCongThucNavigation.Gia)
            });
        }

        // 1. API: Lấy tất cả đơn hàng
        [HttpGet]
        public async Task<IActionResult> GetOrders()
        {
            try
            {
                var sixMonthsAgo = DateTime.Now.AddMonths(-6);

                var ordersQuery = _context.DonHangs
                    .Include(dh => dh.MaTrangThaiDonHangNavigation)
                    .Include(dh => dh.MaKhachHangNavigation)
                    .Include(dh => dh.MaNhanVienNavigation)
                    .Include(dh => dh.ChiTietDonHangs)
                        .ThenInclude(ct => ct.BanAnDonHangs)
                            .ThenInclude(badh => badh.MaBanNavigation)
                    .Include(dh => dh.ChiTietDonHangs)
                        .ThenInclude(ct => ct.MaCongThucNavigation)

                    .Where(dh => dh.ThoiGianKetThuc >= sixMonthsAgo ||
                                 dh.ThoiGianKetThuc == null ||
                                 dh.MaTrangThaiDonHang != "DA_HUY")
                    .OrderByDescending(dh => dh.ThoiGianDatHang);

                var result = await ordersQuery.Select(dh => new
                {
                    maDonHang = dh.MaDonHang,
                    maNhanVien = dh.MaNhanVien,
                    maKhachHang = dh.MaKhachHang,
                    tenTrangThai = dh.MaTrangThaiDonHangNavigation.TenTrangThai,
                    maTrangThaiDonHang = dh.MaTrangThaiDonHang,
                    thoiGianDatHang = dh.ThoiGianDatHang,
                    tgNhanBan = dh.TGNhanBan,
                    thanhToan = dh.ThanhToan,
                    soLuongNguoiDK = dh.SoLuongNguoiDK,

                    // SỬA: Ưu tiên TenNguoiNhan (trong đơn), nếu ko có thì lấy HoTen (khách hàng)
                    tenNguoiNhan = !string.IsNullOrEmpty(dh.TenNguoiNhan) ? dh.TenNguoiNhan : dh.MaKhachHangNavigation.HoTen,

                    // SỬA: Ưu tiên SDT người nhận, nếu ko có lấy SDT khách hàng
                    sdtNguoiNhan = !string.IsNullOrEmpty(dh.SdtnguoiNhan) ? dh.SdtnguoiNhan : dh.MaKhachHangNavigation.SoDienThoai,

                    emailNguoiNhan = dh.EmailNguoiNhan,

                    // Vẫn giữ field gốc để debug nếu cần
                    hoTenKhachHangGoc = dh.MaKhachHangNavigation.HoTen,

                    emailKhachHang = dh.MaKhachHangNavigation.Email,
                    tenNhanVien = dh.MaNhanVienNavigation.HoTen,

                    danhSachBan = string.Join(", ", dh.ChiTietDonHangs
                                            .SelectMany(ct => ct.BanAnDonHangs)
                                            .Select(b => b.MaBanNavigation.TenBan)
                                            .Distinct()),
                    tongTien = dh.ChiTietDonHangs.Sum(ct => ct.SoLuong * ct.MaCongThucNavigation.Gia)
                }).ToListAsync();

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi server", error = ex.Message });
            }
        }

        // 2. API: Lấy đơn hàng theo trạng thái
        [HttpGet("status/{maTrangThai}")]
        public async Task<IActionResult> GetOrdersByStatus([FromRoute] string maTrangThai)
        {
            try
            {
                var statusUpper = maTrangThai.ToUpper();
                var ordersQuery = _context.DonHangs
                    .Include(dh => dh.MaTrangThaiDonHangNavigation)
                    .Include(dh => dh.ChiTietDonHangs)
                        .ThenInclude(ct => ct.BanAnDonHangs)
                            .ThenInclude(badh => badh.MaBanNavigation)
                    .Where(dh => dh.MaTrangThaiDonHang == statusUpper)
                    .OrderByDescending(dh => dh.ThoiGianKetThuc ?? dh.ThoiGianDatHang);

                var result = await ordersQuery.Select(dh => new
                {
                    maDonHang = dh.MaDonHang,
                    maTrangThaiDonHang = dh.MaTrangThaiDonHang,
                    tenTrangThai = dh.MaTrangThaiDonHangNavigation.TenTrangThai,
                    thoiGianDatHang = dh.ThoiGianDatHang,
                    tgNhanBan = dh.TGNhanBan,
                    tenNguoiNhan = dh.TenNguoiNhan,
                    sdtNguoiNhan = dh.SdtnguoiNhan,
                    // Lấy bàn
                    danhSachBan = string.Join(", ", dh.ChiTietDonHangs
                                        .SelectMany(ct => ct.BanAnDonHangs)
                                        .Select(b => b.MaBanNavigation.TenBan)
                                        .Distinct()),
                    tongTien = dh.ChiTietDonHangs.Sum(ct => ct.SoLuong * ct.MaCongThucNavigation.Gia)
                }).ToListAsync();

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi server", error = ex.Message });
            }
        }

        // 3. API: Thống kê
        [HttpGet("stats")]
        public async Task<IActionResult> GetOrderStats()
        {
            try
            {
                var today = DateTime.Today;

                // Tính doanh thu: DonHang -> ChiTietDonHang (đơn giản hơn cấu trúc cũ)
                var totalRevenueToday = await _context.DonHangs
                    .Where(dh => dh.MaTrangThaiDonHang == "DA_HOAN_THANH" &&
                                 dh.ThoiGianKetThuc.HasValue &&
                                 dh.ThoiGianKetThuc.Value.Date == today)
                    .SelectMany(dh => dh.ChiTietDonHangs) // Truy cập trực tiếp Chi Tiết
                    .SumAsync(ct => ct.SoLuong * (ct.MaCongThucNavigation.Gia));

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
                return StatusCode(500, new { message = "Lỗi server", error = ex.Message });
            }
        }

        // 4. API: Lấy Booking đang active
        [HttpGet("GetActiveBookings")]
        public async Task<IActionResult> GetActiveBookings([FromQuery] DateTime? ngay)
        {
            DateTime filterDate = ngay?.Date ?? DateTime.Today;
            var activeStatuses = new[] { "CHO_XAC_NHAN", "DA_XAC_NHAN", "CHO_THANH_TOAN", "DANG_PHUC_VU" };

            var bookings = await _context.DonHangs
                .Include(dh => dh.MaTrangThaiDonHangNavigation)
                .Include(dh => dh.MaKhachHangNavigation)
                // Include để lấy Bàn
                .Include(dh => dh.ChiTietDonHangs)
                    .ThenInclude(ct => ct.BanAnDonHangs)
                        .ThenInclude(badh => badh.MaBanNavigation)
                .Where(dh => activeStatuses.Contains(dh.MaTrangThaiDonHang) &&
                             dh.TGNhanBan.HasValue &&
                             dh.TGNhanBan.Value.Date == filterDate)
                .OrderBy(dh => dh.TGNhanBan)
                .ToListAsync(); // Lấy về trước để xử lý Select phức tạp bên dưới

            var result = bookings.Select(dh => new
            {
                maDonHang = dh.MaDonHang,
                tenNguoiNhan = dh.TenNguoiNhan ?? dh.MaKhachHangNavigation.HoTen,
                soNguoi = dh.SoLuongNguoiDK,
                thoiGianNhanBan = dh.TGNhanBan,
                trangThai = dh.MaTrangThaiDonHangNavigation.TenTrangThai,
                maTrangThai = dh.MaTrangThaiDonHang,
                // Logic lấy Bàn: Gom từ tất cả chi tiết món ăn
                listMaBan = dh.ChiTietDonHangs
                              .SelectMany(ct => ct.BanAnDonHangs)
                              .Select(b => b.MaBan)
                              .Distinct().ToList(),
                banAn = dh.ChiTietDonHangs
                          .SelectMany(ct => ct.BanAnDonHangs)
                          .Select(b => b.MaBanNavigation.TenBan)
                          .Distinct().ToList()
            });

            return Ok(result);
        }

        // 5. API: Chi tiết Booking (CỦA NHÂN VIÊN)
        [HttpGet("GetMyBookingDetail")]
        public async Task<IActionResult> GetMyBookingDetail(
            [FromQuery] string? maDonHang,
            [FromQuery] string? maBan,
            [FromQuery] DateTime? dateTime)
        {
            // Xây dựng query cơ sở
            var query = _context.DonHangs
                .Include(dh => dh.MaKhachHangNavigation)
                .Include(dh => dh.MaTrangThaiDonHangNavigation)
                // Include sâu để lấy Bàn và Món
                .Include(dh => dh.ChiTietDonHangs)
                    .ThenInclude(ct => ct.BanAnDonHangs)
                        .ThenInclude(badh => badh.MaBanNavigation)
                .Include(dh => dh.ChiTietDonHangs)
                    .ThenInclude(ct => ct.MaPhienBanNavigation)
                .Include(dh => dh.ChiTietDonHangs)
                    .ThenInclude(ct => ct.MaCongThucNavigation)
                        .ThenInclude(ctnau => ctnau.MaCtNavigation)
                            .ThenInclude(ctma => ctma.MaMonAnNavigation)
                                .ThenInclude(m => m.HinhAnhMonAns)
                .AsSplitQuery();

            DonHang? donHang = null;

            // Tìm theo Mã Đơn
            if (!string.IsNullOrEmpty(maDonHang))
            {
                donHang = await query.FirstOrDefaultAsync(dh => dh.MaDonHang == maDonHang);
            }
            // Tìm theo Mã Bàn (Phải check xem bàn đó có món nào trong đơn này không)
            else if (!string.IsNullOrEmpty(maBan) && dateTime != null)
            {
                var gioCheck = dateTime.Value;
                // Logic: Lấy các đơn hàng "active", sau đó lọc trong bộ nhớ hoặc query phức tạp
                // Do cấu trúc Don -> ChiTiet -> BanAnDonHang, query ngược hơi khó
                // Ta tìm các BanAnDonHang có MaBan = maBan, từ đó suy ra ChiTiet, suy ra DonHang
                var relatedOrderIds = await _context.BanAnDonHangs
                    .Where(b => b.MaBan == maBan)
                    .Select(b => b.MaChiTietDonHangNavigation.MaDonHang)
                    .Distinct()
                    .ToListAsync();

                // Lọc lại đơn hàng thỏa mãn thời gian và trạng thái
                donHang = await query.FirstOrDefaultAsync(dh =>
                    relatedOrderIds.Contains(dh.MaDonHang) &&
                    (dh.MaTrangThaiDonHang == "CHO_XAC_NHAN" ||
                     dh.MaTrangThaiDonHang == "DA_XAC_NHAN" ||
                     dh.MaTrangThaiDonHang == "DANG_PHUC_VU" ||
                     dh.MaTrangThaiDonHang == "CHO_THANH_TOAN") &&
                    (gioCheck < dh.TGNhanBan.Value.AddMinutes(120)) &&
                    (gioCheck.AddMinutes(120) > dh.TGNhanBan.Value)
                );
            }

            if (donHang == null) return NotFound(new { message = "Không tìm thấy thông tin đặt bàn." });

            // Lấy danh sách bàn
            var tables = donHang.ChiTietDonHangs
                .SelectMany(ct => ct.BanAnDonHangs)
                .Select(b => b.MaBanNavigation.TenBan)
                .Distinct();

            string tenCacBan = string.Join(", ", tables);
            if (string.IsNullOrEmpty(tenCacBan)) tenCacBan = "Chưa xếp bàn";

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

                MonAns = donHang.ChiTietDonHangs.Select(ct =>
                {
                    var congThuc = ct.MaCongThucNavigation;
                    var monAn = congThuc?.MaCtNavigation?.MaMonAnNavigation;

                    // Tìm bàn của món này (1 món có thể nhiều bàn, lấy bàn đầu hoặc join chuỗi)
                    var bansOfItem = ct.BanAnDonHangs.Select(b => b.MaBanNavigation.TenBan).Distinct();
                    string banStr = string.Join(", ", bansOfItem);

                    return new MonAnDatDto
                    {
                        TenMon = monAn?.TenMonAn ?? "Món ???",
                        TenPhienBan = ct.MaPhienBanNavigation?.TenPhienBan ?? "",
                        SoLuong = ct.SoLuong,
                        DonGia = congThuc?.Gia ?? 0,
                        HinhAnh = monAn?.HinhAnhMonAns.FirstOrDefault()?.UrlhinhAnh ?? "",
                        MaBan = "", // Không quan trọng ở view tổng
                        TenBan = string.IsNullOrEmpty(banStr) ? "Chung" : banStr,
                        GhiChu = ""
                    };
                }).OrderBy(m => m.TenBan).ToList()
            };

            return Ok(result);
        }

        // 6. GetCustomersToCall (Giữ nguyên)
        [HttpGet("get-customers-to-call")]
        public async Task<IActionResult> GetCustomersToCall()
        {
            var now = DateTime.Now;
            var thoiGianQuetDen = now.AddHours(24);

            var listCanGoi = await _context.DonHangs
                .Include(dh => dh.MaKhachHangNavigation)
                .Where(dh =>
                    (dh.MaTrangThaiDonHang == "CHO_XAC_NHAN" || dh.MaTrangThaiDonHang == "DA_XAC_NHAN") &&
                    dh.TGNhanBan.HasValue &&
                    dh.TGNhanBan > now &&
                    dh.TGNhanBan <= thoiGianQuetDen &&
                    (string.IsNullOrEmpty(dh.MaKhachHangNavigation.Email))
                )
                .Select(dh => new
                {
                    dh.MaDonHang,
                    TenKhach = dh.TenNguoiNhan ?? dh.MaKhachHangNavigation.HoTen,
                    SDT = dh.SdtnguoiNhan ?? dh.MaKhachHangNavigation.SoDienThoai,
                    GioHen = dh.TGNhanBan,
                    SoNguoi = dh.SoLuongNguoiDK,
                    GhiChu = "Khách không có email, cần gọi nhắc."
                })
                .OrderBy(dh => dh.GioHen)
                .ToListAsync();

            return Ok(listCanGoi);
        }
    }
}