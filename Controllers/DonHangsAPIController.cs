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
    [Authorize] // Yêu cầu đăng nhập cho tất cả các endpoint trong controller này
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

        // DTO nhận dữ liệu từ Client (Không cần MaBan nữa, chỉ cần MaDonHang)
        public class ThemMonVaoDonRequest
        {
            public string MaDonHang { get; set; }
            public List<MonAnOrderDTO> Items { get; set; }
        }

        // =============================================================
        // API: THÊM MÓN VÀO ĐƠN HÀNG (Hỗ trợ đơn gộp bàn)
        // =============================================================
        [HttpPost("ThemMonVaoDon")]
        public async Task<IActionResult> ThemMonVaoDon([FromBody] ThemMonVaoDonRequest request)
        {
            // 1. Kiểm tra đơn hàng và lấy danh sách bàn của đơn đó
            var donHang = await _context.DonHangs
                .Include(dh => dh.BanAnDonHangs) // Lấy các bàn đang thuộc đơn này
                .FirstOrDefaultAsync(dh => dh.MaDonHang == request.MaDonHang);

            if (donHang == null)
                return NotFound(new { message = "Không tìm thấy đơn hàng" });

            // Kiểm tra đơn hàng có bàn nào không? (Lý thuyết phải có ít nhất 1 bàn)
            if (donHang.BanAnDonHangs == null || !donHang.BanAnDonHangs.Any())
            {
                return BadRequest(new { message = "Đơn hàng này chưa được xếp bàn nào." });
            }

            // Lấy ra bàn đầu tiên để gán món (Đại diện)
            // Vì món ăn thuộc về Đơn hàng là chính, việc gán vào bàn nào trong nhóm bàn gộp chỉ là kỹ thuật lưu trữ
            var banDaiDien = donHang.BanAnDonHangs.FirstOrDefault()?.MaBan;

            try
            {
                foreach (var item in request.Items)
                {
                    // 2. Tìm công thức món ăn (Validate)
                    var congThuc = await _context.CongThucNauAns
                        .Include(ct => ct.MaCtNavigation)
                        .FirstOrDefaultAsync(ct =>
                            ct.MaPhienBan == item.MaPhienBan &&
                            ct.MaCtNavigation.MaMonAn == item.MaMonAn
                        );

                    if (congThuc == null)
                    {
                        return BadRequest(new { message = $"Không tìm thấy món hoặc size: {item.MaMonAn} - {item.MaPhienBan}" });
                    }

                    // 3. Tạo Chi Tiết Đơn Hàng
                    var maChiTietMoi = "CTDH" + DateTime.Now.Ticks.ToString().Substring(10) + new Random().Next(100, 999);

                    var chiTietMoi = new ChiTietDonHang
                    {
                        MaChiTietDonHang = maChiTietMoi,
                        MaDonHang = request.MaDonHang,
                        MaPhienBan = item.MaPhienBan,
                        MaCongThuc = congThuc.MaCongThuc,
                        SoLuong = item.SoLuong,
                        // GhiChu = item.GhiChu // Nếu có trường Ghi chú
                    };
                    _context.ChiTietDonHangs.Add(chiTietMoi);

                    // 4. Tạo liên kết BanAnDonHang (Gán vào bàn đại diện)
                    // Bắt buộc phải có bản ghi này thì món mới hiện lên khi query theo bàn
                    var banAnDonHang = new BanAnDonHang
                    {
                        MaBanAnDonHang = "BADH" + DateTime.Now.Ticks.ToString().Substring(10) + new Random().Next(100, 999),
                        MaBan = banDaiDien,
                        MaChiTietDonHang = maChiTietMoi,
                        MaDonHang = request.MaDonHang // (Nếu DB bạn có cột này ở bảng trung gian thì bỏ comment)
                    };
                    _context.BanAnDonHangs.Add(banAnDonHang);
                }

                // Cập nhật trạng thái đơn hàng nếu cần (VD: Đang từ "Chờ thanh toán" -> quay lại "Đang phục vụ"?)
                // donHang.MaTrangThaiDonHang = "DANG_PHUC_VU"; 

                await _context.SaveChangesAsync();

                // Tính lại tổng tiền để trả về cho frontend cập nhật UI ngay (Option)
                var tongTienMoi = await _context.ChiTietDonHangs
                    .Where(ct => ct.MaDonHang == request.MaDonHang)
                    .SumAsync(ct => ct.SoLuong * ct.MaCongThucNavigation.Gia);

                return Ok(new { message = "Thêm món thành công", tongTien = tongTienMoi });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi server: " + ex.Message });
            }
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
                    tgDatDuKien = dh.TgdatDuKien, // Thêm field ngày dự kiến
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
                // Sửa lỗi: Xử lý trường hợp tập rỗng bằng cách dùng DefaultIfEmpty() hoặc kiểm tra trước
                var revenueQuery = _context.DonHangs
                    .Where(dh => dh.MaTrangThaiDonHang == "DA_HOAN_THANH" &&
                                 dh.ThoiGianKetThuc.HasValue &&
                                 dh.ThoiGianKetThuc.Value.Date == today)
                    .SelectMany(dh => dh.ChiTietDonHangs)
                    .Select(ct => ct.SoLuong * (ct.MaCongThucNavigation.Gia));

                // Kiểm tra xem có dữ liệu không trước khi Sum
                var totalRevenueToday = await revenueQuery.AnyAsync() 
                    ? await revenueQuery.SumAsync() 
                    : 0m;

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

        [HttpGet("GetActiveBookings")]
        public async Task<IActionResult> GetActiveBookings([FromQuery] DateTime? ngay)
        {
            DateTime filterDate = ngay?.Date ?? DateTime.Today;
            // Add DA_XAC_NHAN and CHO_XAC_NHAN which are common for future bookings
            var activeStatuses = new[] { "CHO_XAC_NHAN", "DA_XAC_NHAN", "CHO_THANH_TOAN", "DANG_PHUC_VU" };

            var bookings = await _context.DonHangs
                .Include(dh => dh.MaTrangThaiDonHangNavigation)
                .Include(dh => dh.MaKhachHangNavigation)
                // Include để lấy Bàn - SỬA: Lấy trực tiếp từ DonHang.BanAnDonHangs
                .Include(dh => dh.BanAnDonHangs)
                    .ThenInclude(badh => badh.MaBanNavigation)
                // Cũng include ChiTietDonHangs để lấy bàn từ món ăn (nếu có)
                .Include(dh => dh.ChiTietDonHangs)
                    .ThenInclude(ct => ct.BanAnDonHangs)
                        .ThenInclude(badh => badh.MaBanNavigation)
                .Where(dh => activeStatuses.Contains(dh.MaTrangThaiDonHang) &&
                             (
                                // Case 1: Đã check-in (TGNhanBan có giá trị) -> So sánh TGNhanBan
                                (dh.TGNhanBan.HasValue && dh.TGNhanBan.Value.Date == filterDate) ||
                                // Case 2: Chưa check-in (TGNhanBan null, dùng TgdatDuKien) -> So sánh TgdatDuKien
                                (!dh.TGNhanBan.HasValue && dh.TgdatDuKien.HasValue && dh.TgdatDuKien.Value.Date == filterDate)
                             )
                )
                // Order by actual arrival time or expected time
                .OrderBy(dh => dh.TGNhanBan ?? dh.TgdatDuKien)
                .ToListAsync(); // Lấy về trước để xử lý Select phức tạp bên dưới

            var result = bookings.Select(dh =>
            {
                // SỬA LOGIC: Lấy bàn từ cả hai nguồn:
                // 1. Từ DonHang.BanAnDonHangs (bàn đã đặt, chưa có món) - QUAN TRỌNG: Đây là nguồn chính khi đặt bàn
                // 2. Từ ChiTietDonHangs -> BanAnDonHangs (bàn có món)
                var banTuDatBan = dh.BanAnDonHangs
                    .Where(b => b.MaBanNavigation != null)
                    .Select(b => new { MaBan = b.MaBan, TenBan = b.MaBanNavigation.TenBan })
                    .ToList();

                var banTuMonAn = dh.ChiTietDonHangs
                    .SelectMany(ct => ct.BanAnDonHangs)
                    .Where(b => b.MaBanNavigation != null)
                    .Select(b => new { MaBan = b.MaBan, TenBan = b.MaBanNavigation.TenBan })
                    .ToList();

                // Gộp và loại bỏ trùng lặp
                var allBans = banTuDatBan
                    .Union(banTuMonAn)
                    .GroupBy(b => b.MaBan)
                    .Select(g => g.First())
                    .ToList();

                return new
                {
                    maDonHang = dh.MaDonHang,
                    tenNguoiNhan = dh.TenNguoiNhan ?? dh.MaKhachHangNavigation?.HoTen ?? "Khách lẻ",
                    soNguoi = dh.SoLuongNguoiDK,
                    // Trả về thời gian hiển thị: Nếu chưa đến thì hiện giờ dự kiến
                    thoiGianNhanBan = dh.TGNhanBan ?? dh.TgdatDuKien,
                    trangThai = dh.MaTrangThaiDonHangNavigation?.TenTrangThai ?? dh.MaTrangThaiDonHang,
                    maTrangThai = dh.MaTrangThaiDonHang,
                    listMaBan = allBans.Select(b => b.MaBan).ToList(),
                    banAn = allBans.Select(b => b.TenBan).ToList()
                };
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
                // SỬA: THÊM Include cho DonHang.BanAnDonHangs (bàn đặt trực tiếp)
                .Include(dh => dh.BanAnDonHangs)
                    .ThenInclude(badh => badh.MaBanNavigation)
                // Include sâu để lấy Bàn và Món từ ChiTietDonHangs
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

            // SỬA: Lấy danh sách bàn từ CẢ HAI nguồn
            // 1. Từ DonHang.BanAnDonHangs (bàn đặt trực tiếp khi booking - QUAN TRỌNG!)
            // 2. Từ ChiTietDonHangs.BanAnDonHangs (bàn từ món ăn - nếu đã gọi món)
            
            var banTuDatBan = donHang.BanAnDonHangs
                .Where(b => b.MaBanNavigation != null)
                .Select(b => b.MaBanNavigation.TenBan)
                .Distinct()
                .ToList();

            var banTuMonAn = donHang.ChiTietDonHangs
                .SelectMany(ct => ct.BanAnDonHangs)
                .Where(b => b.MaBanNavigation != null)
                .Select(b => b.MaBanNavigation.TenBan)
                .Distinct()
                .ToList();

            // Gộp cả hai danh sách và loại bỏ trùng lặp
            var allTables = banTuDatBan.Union(banTuMonAn).Distinct().ToList();
            
            string tenCacBan = allTables.Any() ? string.Join(", ", allTables) : "Chưa xếp bàn";

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