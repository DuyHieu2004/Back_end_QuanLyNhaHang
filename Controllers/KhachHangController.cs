using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyNhaHang.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QuanLyNhaHang.Controllers
{
    // DTO hiển thị danh sách
    public class KhachHangListDto
    {
        public string MaKhachHang { get; set; } = null!;
        public string HoTen { get; set; } = null!;
        public string SoDienThoai { get; set; } = null!;
        public string? Email { get; set; }
        public string? HinhAnh { get; set; }

        // Trường này được tính toán động (Calculated)
        public int SoLanAnTichLuy { get; set; }

        public int? NoShowCount { get; set; }
        public DateTime? NgayTao { get; set; }
        public DateTime? NgayCuoiCungTichLuy { get; set; } // Thêm để hiển thị nếu cần
    }

    // Model tạo mới
    public class KhachHangCreateModel
    {
        public string HoTen { get; set; } = null!;
        public string SoDienThoai { get; set; } = null!;
        public string? Email { get; set; }
        public string? HinhAnh { get; set; }
    }

    // Model cập nhật
    public class KhachHangUpdateModel
    {
        public string HoTen { get; set; } = null!;
        public string SoDienThoai { get; set; } = null!;
        public string? Email { get; set; }
        public string? HinhAnh { get; set; }
    }

    [Route("api/[controller]")]
    [ApiController]
    public class KhachHangController : ControllerBase
    {
        private readonly QLNhaHangContext _context;

        public KhachHangController(QLNhaHangContext context)
        {
            _context = context;
        }

        // ============================================================
        // 1. API THỐNG KÊ NHANH
        // ============================================================
        [HttpGet("ThongKe")]
        public async Task<IActionResult> GetThongKe()
        {
            try
            {
                var today = DateTime.Today;
                var startOfMonth = new DateTime(today.Year, today.Month, 1);
                var endOfMonth = startOfMonth.AddMonths(1).AddDays(-1);

                var totalKhachHang = await _context.KhachHangs.CountAsync();

                var khachHangMoiThang = await _context.KhachHangs
                    .Where(k => k.NgayTao >= startOfMonth && k.NgayTao <= endOfMonth)
                    .CountAsync();

                // --- LOGIC MỚI: Đếm khách thân thiết (>= 5 lần ăn trong chu kỳ hiện tại) ---
                var khachHangThanThiet = await _context.KhachHangs
                    .Where(k => _context.DonHangs.Count(dh =>
                        dh.MaKhachHang == k.MaKhachHang
                        && dh.MaTrangThaiDonHang == "DA_HOAN_THANH"
                        && (k.NgayCuoiCungTichLuy == null || (dh.TGNhanBan.HasValue && dh.TGNhanBan > k.NgayCuoiCungTichLuy))
                    ) >= 5)
                    .CountAsync();

                var khachNoShow = await _context.KhachHangs
                    .Where(k => k.NoShowCount > 0)
                    .CountAsync();

                return Ok(new
                {
                    tongKhachHang = totalKhachHang,
                    khachHangMoiThang = khachHangMoiThang,
                    khachHangThanThiet = khachHangThanThiet,
                    khachNoShow = khachNoShow
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine("LỖI ThongKe KhachHang: " + ex.ToString());
                return StatusCode(500, new { success = false, message = "Lỗi server: " + ex.Message });
            }
        }

        // ============================================================
        // 2. API LẤY DANH SÁCH KHÁCH HÀNG (PHÂN TRANG & TÌM KIẾM)
        // ============================================================
        [HttpGet]
        public async Task<IActionResult> GetKhachHangs([FromQuery] string search = "", [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                var query = _context.KhachHangs.AsQueryable();
                // Loại bỏ khách vãng lai khỏi danh sách quản lý
                query = query.Where(k => k.MaKhachHang != "KH_VANG_LAI");

                if (!string.IsNullOrEmpty(search))
                {
                    query = query.Where(k =>
                        k.HoTen.Contains(search) ||
                        k.SoDienThoai.Contains(search) ||
                        (k.Email != null && k.Email.Contains(search))
                    );
                }

                var totalRecords = await query.CountAsync();

                var khachHangs = await query
                    .AsNoTracking()
                    .OrderByDescending(k => k.NgayTao) // Sắp xếp theo ngày tạo mới nhất
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(k => new KhachHangListDto
                    {
                        MaKhachHang = k.MaKhachHang,
                        HoTen = k.HoTen,
                        SoDienThoai = k.SoDienThoai,
                        Email = k.Email,
                        HinhAnh = k.HinhAnh,

                        // --- TÍNH TOÁN SỐ LẦN ĂN TÍCH LŨY (SUB-QUERY) ---
                        SoLanAnTichLuy = _context.DonHangs.Count(dh =>
                            dh.MaKhachHang == k.MaKhachHang
                            && dh.MaTrangThaiDonHang == "DA_HOAN_THANH"
                            // Nếu chưa có ngày reset (null) thì lấy hết, ngược lại lấy sau ngày reset
                            && (k.NgayCuoiCungTichLuy == null || (dh.TGNhanBan.HasValue && dh.TGNhanBan > k.NgayCuoiCungTichLuy))
                        ),
                        // -----------------------------------------------

                        NoShowCount = k.NoShowCount,
                        NgayTao = k.NgayTao,
                        NgayCuoiCungTichLuy = k.NgayCuoiCungTichLuy
                    })
                    .ToListAsync();

                return Ok(new
                {
                    totalRecords = totalRecords,
                    page = page,
                    pageSize = pageSize,
                    data = khachHangs
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine("LỖI GetKhachHangs: " + ex.ToString());
                return StatusCode(500, new { success = false, message = "Lỗi server: " + ex.Message });
            }
        }

        // ============================================================
        // 3. API THÊM KHÁCH HÀNG MỚI
        // ============================================================
        [HttpPost]
        public async Task<IActionResult> CreateKhachHang([FromBody] KhachHangCreateModel model)
        {
            try
            {
                if (await _context.KhachHangs.AnyAsync(k => k.SoDienThoai == model.SoDienThoai))
                {
                    return BadRequest(new { success = false, message = "Số điện thoại đã tồn tại" });
                }

                var khachHang = new KhachHang
                {
                    MaKhachHang = await GenerateCustomerCodeAsync(),
                    HoTen = model.HoTen,
                    SoDienThoai = model.SoDienThoai,
                    Email = model.Email,
                    HinhAnh = model.HinhAnh,
                    NgayTao = DateTime.Now,

                    // Khởi tạo giá trị mặc định
                    NoShowCount = 0,
                    NgayCuoiCungTichLuy = null // Chưa reset lần nào
                };

                _context.KhachHangs.Add(khachHang);
                await _context.SaveChangesAsync();

                return Ok(new { success = true, message = "Thêm khách hàng thành công", data = khachHang });
            }
            catch (Exception ex)
            {
                Console.WriteLine("LỖI CreateKhachHang: " + ex.ToString());
                return StatusCode(500, new { success = false, message = "Lỗi server: " + ex.Message });
            }
        }

        // ============================================================
        // 4. API LẤY CHI TIẾT KHÁCH HÀNG
        // ============================================================
        [HttpGet("{id}")]
        public async Task<IActionResult> GetKhachHangDetail(string id)
        {
            try
            {
                var khachHang = await _context.KhachHangs
                    .AsNoTracking()
                    .FirstOrDefaultAsync(k => k.MaKhachHang == id);

                if (khachHang == null)
                {
                    return NotFound(new { success = false, message = "Không tìm thấy khách hàng" });
                }

                // --- TÍNH TOÁN SỐ LẦN ĂN HIỆN TẠI CHO KHÁCH NÀY ---
                var soLanAnHienTai = await _context.DonHangs
                    .Where(dh => dh.MaKhachHang == id
                                 && dh.MaTrangThaiDonHang == "DA_HOAN_THANH"
                                 && (khachHang.NgayCuoiCungTichLuy == null || (dh.TGNhanBan.HasValue && dh.TGNhanBan > khachHang.NgayCuoiCungTichLuy)))
                    .CountAsync();
                // --------------------------------------------------

                // Lấy lịch sử đơn hàng (Vẫn lấy 20 đơn gần nhất để xem lịch sử, không bị lọc bởi ngày tích lũy)
                var donHangs = await _context.DonHangs
                    .AsNoTracking()
                    .Where(d => d.MaKhachHang == id)
                    .OrderByDescending(d => d.ThoiGianDatHang)
                    .Select(d => new
                    {
                        maDonHang = d.MaDonHang,
                        thoiGianDatHang = d.ThoiGianDatHang,
                        tienDatCoc = d.TienDatCoc,
                        trangThai = d.MaTrangThaiDonHang,
                        soLuongNguoiDK = d.SoLuongNguoiDK,
                        ghiChu = d.GhiChu,
                        // Flag này giúp FE biết đơn nào đang được tính điểm
                        isAccumulated = (d.MaTrangThaiDonHang == "DA_HOAN_THANH" &&
                                        (khachHang.NgayCuoiCungTichLuy == null || (d.TGNhanBan.HasValue && d.TGNhanBan > khachHang.NgayCuoiCungTichLuy)))
                    })
                    .Take(20)
                    .ToListAsync();

                // Lấy lịch sử đặt bàn
                var datBans = await _context.BanAnDonHangs
                    .AsNoTracking()
                    .Include(b => b.MaDonHangNavigation)
                    .Include(b => b.MaBanNavigation)
                    .Where(b => b.MaDonHangNavigation!.MaKhachHang == id)
                    .Select(b => new
                    {
                        maDonHang = b.MaDonHang,
                        tenBan = b.MaBanNavigation!.TenBan,
                        thoiGianDatHang = b.MaDonHangNavigation!.ThoiGianDatHang,
                        trangThai = b.MaDonHangNavigation!.MaTrangThaiDonHang
                    })
                    .OrderByDescending(b => b.thoiGianDatHang)
                    .Take(10)
                    .ToListAsync();

                return Ok(new
                {
                    success = true,
                    profile = new
                    {
                        maKhachHang = khachHang.MaKhachHang,
                        hoTen = khachHang.HoTen,
                        soDienThoai = khachHang.SoDienThoai,
                        email = khachHang.Email,
                        hinhAnh = khachHang.HinhAnh,

                        // Trả về giá trị đã tính toán
                        soLanAnTichLuy = soLanAnHienTai,

                        noShowCount = khachHang.NoShowCount,
                        ngayTao = khachHang.NgayTao,
                        ngayCuoiCungTichLuy = khachHang.NgayCuoiCungTichLuy
                    },
                    donHangs = donHangs,
                    datBans = datBans
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine("LỖI GetKhachHangDetail: " + ex.ToString());
                return StatusCode(500, new { success = false, message = "Lỗi server: " + ex.Message });
            }
        }

        // ============================================================
        // 5. API CẬP NHẬT KHÁCH HÀNG
        // ============================================================
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateKhachHang(string id, [FromBody] KhachHangUpdateModel model)
        {
            try
            {
                var existing = await _context.KhachHangs.FirstOrDefaultAsync(k => k.MaKhachHang == id);
                if (existing == null)
                {
                    return NotFound(new { success = false, message = "Không tìm thấy khách hàng" });
                }

                if (await _context.KhachHangs.AnyAsync(k => k.SoDienThoai == model.SoDienThoai && k.MaKhachHang != id))
                {
                    return BadRequest(new { success = false, message = "Số điện thoại đã được sử dụng bởi khách hàng khác" });
                }

                existing.HoTen = model.HoTen;
                existing.SoDienThoai = model.SoDienThoai;
                existing.Email = model.Email;
                existing.HinhAnh = model.HinhAnh;

                await _context.SaveChangesAsync();

                return Ok(new { success = true, message = "Cập nhật khách hàng thành công" });
            }
            catch (Exception ex)
            {
                Console.WriteLine("LỖI UpdateKhachHang: " + ex.ToString());
                return StatusCode(500, new { success = false, message = "Lỗi server: " + ex.Message });
            }
        }

        // ============================================================
        // 6. API XUẤT EXCEL
        // ============================================================
        [HttpGet("Export")]
        public async Task<IActionResult> ExportKhachHangs([FromQuery] string search = "")
        {
            try
            {
                var query = _context.KhachHangs.AsQueryable();

                if (!string.IsNullOrEmpty(search))
                {
                    query = query.Where(k =>
                        k.HoTen.Contains(search) ||
                        k.SoDienThoai.Contains(search) ||
                        (k.Email != null && k.Email.Contains(search))
                    );
                }

                var khachHangs = await query
                    .OrderByDescending(k => k.NgayTao)
                    .Select(k => new
                    {
                        MaKhachHang = k.MaKhachHang,
                        HoTen = k.HoTen,
                        SoDienThoai = k.SoDienThoai,
                        Email = k.Email ?? "",

                        // Tính toán khi xuất Excel luôn cho chính xác
                        SoLanAn = _context.DonHangs.Count(dh =>
                            dh.MaKhachHang == k.MaKhachHang
                            && dh.MaTrangThaiDonHang == "DA_HOAN_THANH"
                            && (k.NgayCuoiCungTichLuy == null || (dh.TGNhanBan.HasValue && dh.TGNhanBan > k.NgayCuoiCungTichLuy))
                        ),

                        NoShowCount = k.NoShowCount ?? 0,
                        NgayTao = k.NgayTao,
                    })
                    .ToListAsync();

                return Ok(new
                {
                    success = true,
                    message = "Dữ liệu xuất Excel đã sẵn sàng",
                    data = khachHangs
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine("LỖI Export KhachHang: " + ex.ToString());
                return StatusCode(500, new { success = false, message = "Lỗi server: " + ex.Message });
            }
        }

        // ============================================================
        // HÀM HỖ TRỢ TẠO MÃ TỰ ĐỘNG
        // ============================================================
        private async Task<string> GenerateCustomerCodeAsync()
        {
            var lastCustomer = await _context.KhachHangs
                .AsNoTracking()
                .Where(k => k.MaKhachHang != "KH_VANG_LAI")
                .OrderByDescending(k => k.MaKhachHang)
                .Select(k => k.MaKhachHang)
                .FirstOrDefaultAsync();

            if (string.IsNullOrEmpty(lastCustomer))
            {
                return "KH0001";
            }

            int lastNumber = 0;
            // Giả sử mã có dạng "KHxxxx"
            if (lastCustomer.Length >= 3 &&
                lastCustomer.StartsWith("KH") &&
                int.TryParse(lastCustomer.Substring(2), out int parsedNumber))
            {
                lastNumber = parsedNumber;
            }

            return "KH" + (lastNumber + 1).ToString("D4");
        }
    }
}
