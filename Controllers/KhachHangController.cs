using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyNhaHang.Models;
using System.Globalization;
using System.Linq;

namespace QuanLyNhaHang.Controllers
{
    // DTO DÙNG CHO API TRẢ VỀ DANH SÁCH
    public class KhachHangListDto
    {
        public string MaKhachHang { get; set; } = null!;
        public string HoTen { get; set; } = null!;
        public string SoDienThoai { get; set; } = null!;
        public string? Email { get; set; }
        public string? HinhAnh { get; set; }
        public int SoLanAnTichLuy { get; set; }
        public int? NoShowCount { get; set; }
        public DateTime? NgayTao { get; set; }

        public decimal TongChiTieu { get; set; } = 0;
        public DateTime? LanCuoiDen { get; set; }
    }

    // KHÁCH HÀNG CONTROLLER

    [Route("api/[controller]")]
    [ApiController]
    public class KhachHangController : ControllerBase
    {
        private readonly QLNhaHangContext _context;

        public KhachHangController(QLNhaHangContext context)
        {
            _context = context;
        }

        // API lấy thống kê nhanh
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

                var khachHangThanThiet = await _context.KhachHangs
                    .Where(k => k.SoLanAnTichLuy >= 5)
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
                Console.WriteLine(" LỖI ThongKe KhachHang: " + ex.ToString());
                return StatusCode(500, new { success = false, message = "Lỗi server: " + ex.Message });
            }
        }

        // API lấy danh sách khách hàng
        [HttpGet]
        public async Task<IActionResult> GetKhachHangs([FromQuery] string search = "", [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                var query = _context.KhachHangs.AsQueryable();
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
                    .OrderByDescending(k => k.SoLanAnTichLuy)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(k => new KhachHangListDto
                    {
                        
                        MaKhachHang = k.MaKhachHang,
                        HoTen = k.HoTen,
                        SoDienThoai = k.SoDienThoai,
                        Email = k.Email,
                        HinhAnh = k.HinhAnh,
                        SoLanAnTichLuy = k.SoLanAnTichLuy,
                        NoShowCount = k.NoShowCount,
                        NgayTao = k.NgayTao,
                        TongChiTieu = 0.0m,
                        LanCuoiDen = (DateTime?)null
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
                Console.WriteLine(" LỖI NGHIÊM TRỌNG KHI TẢI KHÁCH HÀNG (GetKhachHangs): " + ex.ToString());
                return StatusCode(500, new { success = false, message = "Lỗi nghiêm trọng khi tải danh sách khách hàng. Vui lòng kiểm tra log backend." });
            }
        }

        // API thêm khách hàng mới 
        [HttpPost]
        public async Task<IActionResult> CreateKhachHang([FromBody] KhachHangCreateModel model)
        {
            try
            {
                // Kiểm tra số điện thoại đã tồn tại
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
                    SoLanAnTichLuy = 0,
                    NoShowCount = 0
                };

                _context.KhachHangs.Add(khachHang);
                await _context.SaveChangesAsync();

                return Ok(new { success = true, message = "Thêm khách hàng thành công", data = khachHang });
            }
            catch (Exception ex)
            {
                Console.WriteLine(" LỖI NGHIÊM TRỌNG KHI THÊM KHÁCH HÀNG (API POST): " + ex.ToString());
                return StatusCode(500, new { success = false, message = "Lỗi kết nối Database hoặc server. Vui lòng kiểm tra log backend." });
            }
        }

        // API lấy chi tiết khách hàng 
        [HttpGet("{id}")]
        public async Task<IActionResult> GetKhachHangDetail(string id)
        {
            try
            {
                // 1. Lấy hồ sơ khách hàng cơ bản
                var khachHang = await _context.KhachHangs
                    .AsNoTracking()
                    .FirstOrDefaultAsync(k => k.MaKhachHang == id);

                if (khachHang == null)
                {
                    return NotFound(new { success = false, message = "Không tìm thấy khách hàng" });
                }

                // 2. Lấy lịch sử đơn hàng
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
                        ghiChu = d.GhiChu
                    })
                    .Take(20)
                    .ToListAsync();

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
                        soLanAnTichLuy = khachHang.SoLanAnTichLuy,
                        noShowCount = khachHang.NoShowCount,
                        ngayTao = khachHang.NgayTao
                    },
                    donHangs = donHangs,
                    datBans = datBans
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine("🔴 LỖI GetKhachHangDetail: " + ex.ToString());
                return StatusCode(500, new { success = false, message = "Lỗi server khi lấy chi tiết khách hàng: " + ex.Message });
            }
        }

        // API cập nhật khách hàng
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

                // Kiểm tra số điện thoại trùng
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
                Console.WriteLine("LỖI Cập nhật KhachHang: " + ex.ToString());
                return StatusCode(500, new { success = false, message = "Lỗi server: " + ex.Message });
            }
        }

        // API xuất Excel
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
                    .OrderByDescending(k => k.SoLanAnTichLuy)
                    .Select(k => new
                    {
                        MaKhachHang = k.MaKhachHang,
                        HoTen = k.HoTen,
                        SoDienThoai = k.SoDienThoai,
                        Email = k.Email ?? "",
                        SoLanAn = k.SoLanAnTichLuy,
                        NoShowCount = k.NoShowCount ?? 0,
                        NgayTao = k.NgayTao,
                        TongChiTieu = 0.0m
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

        // Hàm tạo mã khách hàng
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
            if (lastCustomer.Length >= 3 &&
                lastCustomer.StartsWith("KH") &&
                int.TryParse(lastCustomer.Substring(2), out int parsedNumber))
            {
                lastNumber = parsedNumber;
            }
            else
            {
                lastNumber = 0;
            }

            return "KH" + (lastNumber + 1).ToString("D4");
        }
    }

    // Model cho tạo khách hàng
    public class KhachHangCreateModel
    {
        public string HoTen { get; set; } = null!;
        public string SoDienThoai { get; set; } = null!;
        public string? Email { get; set; }
        public string? HinhAnh { get; set; }
    }

    // Model cho cập nhật khách hàng
    public class KhachHangUpdateModel
    {
        public string HoTen { get; set; } = null!;
        public string SoDienThoai { get; set; } = null!;
        public string? Email { get; set; }
        public string? HinhAnh { get; set; }
    }
}