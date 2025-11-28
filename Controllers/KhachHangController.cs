
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyNhaHang.Models;
using System.Globalization;

namespace QuanLyNhaHang.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class KhachHangController : ControllerBase
    {
        private readonly QLNhaHangContext _context;

        public KhachHangController(QLNhaHangContext context)
        {
            _context = context;
        }

        // API tìm kiếm theo số điện thoại
        [HttpGet("TimKiem/{sdt}")]
        public async Task<IActionResult> GetKhachHangBySDT(string sdt)
        {
            try
            {
                var khachHang = await _context.KhachHangs
                                        .FirstOrDefaultAsync(k => k.SoDienThoai == sdt);

                if (khachHang != null)
                {
                    return Ok(new
                    {
                        Success = true,
                        Message = "Đã tìm thấy khách hàng!",
                        Data = new
                        {
                            MaKhachHang = khachHang.MaKhachHang,
                            HoTen = khachHang.HoTen,
                            Email = khachHang.Email,
                            SoLanAn = khachHang.SoLanAnTichLuy,
                            NoShowCount = khachHang.NoShowCount
                        }
                    });
                }
                else
                {
                    return Ok(new
                    {
                        Success = false,
                        Message = "Khách hàng mới",
                        Data = (object)null
                    });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Success = false, Message = "Lỗi server: " + ex.Message });
            }
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

                // Khách hàng thân thiết: trên 5 lần ăn
                var khachHangThanThiet = await _context.KhachHangs
                    .Where(k => k.SoLanAnTichLuy >= 5)
                    .CountAsync();

                // Khách No-show: có ít nhất 1 lần no-show
                var khachNoShow = await _context.KhachHangs
                    .Where(k => k.NoShowCount > 0)
                    .CountAsync();

                return Ok(new
                {
                    TongKhachHang = totalKhachHang,
                    KhachHangMoiThang = khachHangMoiThang,
                    KhachHangThanThiet = khachHangThanThiet,
                    KhachNoShow = khachNoShow
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Success = false, Message = "Lỗi server: " + ex.Message });
            }
        }

        // API lấy danh sách khách hàng với phân trang và tìm kiếm
        [HttpGet]
        public async Task<IActionResult> GetKhachHangs([FromQuery] string search = "", [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
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

                var totalRecords = await query.CountAsync();

                var khachHangs = await query
                    .OrderByDescending(k => k.SoLanAnTichLuy)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(k => new
                    {
                        MaKhachHang = k.MaKhachHang,
                        HoTen = k.HoTen,
                        SoDienThoai = k.SoDienThoai,
                        Email = k.Email,
                        HinhAnh = k.HinhAnh,
                        SoLanAnTichLuy = k.SoLanAnTichLuy,
                        NoShowCount = k.NoShowCount,
                        NgayTao = k.NgayTao,
                        // Tính tổng chi tiêu từ tiền đặt cọc các đơn hàng đã thanh toán
                        TongChiTieu = _context.DonHangs
                            .Where(d => d.MaKhachHang == k.MaKhachHang && d.ThanhToan == true)
                            .Sum(d => d.TienDatCoc ?? 0),
                        // Lấy thời gian đặt hàng gần nhất
                        LanCuoiDen = _context.DonHangs
                            .Where(d => d.MaKhachHang == k.MaKhachHang)
                            .OrderByDescending(d => d.ThoiGianDatHang)
                            .Select(d => d.ThoiGianDatHang)
                            .FirstOrDefault()
                    })
                    .ToListAsync();

                return Ok(new
                {
                    TotalRecords = totalRecords,
                    Page = page,
                    PageSize = pageSize,
                    Data = khachHangs
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Success = false, Message = "Lỗi server: " + ex.Message });
            }
        }

        // API lấy chi tiết khách hàng
        [HttpGet("{id}")]
        public async Task<IActionResult> GetKhachHangDetail(string id)
        {
            try
            {
                var khachHang = await _context.KhachHangs
                    .FirstOrDefaultAsync(k => k.MaKhachHang == id);

                if (khachHang == null)
                {
                    return NotFound(new { Success = false, Message = "Không tìm thấy khách hàng" });
                }

                // Lấy lịch sử đơn hàng
                var donHangs = await _context.DonHangs
                    .Where(d => d.MaKhachHang == id)
                    .OrderByDescending(d => d.ThoiGianDatHang)
                    .Select(d => new
                    {
                        MaDonHang = d.MaDonHang,
                        ThoiGianDatHang = d.ThoiGianDatHang,
                        TienDatCoc = d.TienDatCoc,
                        TrangThai = d.MaTrangThaiDonHang,
                        SoLuongNguoiDK = d.SoLuongNguoiDK,
                        GhiChu = d.GhiChu
                    })
                    .Take(20)
                    .ToListAsync();

                // Lấy thông tin bàn ăn từ lịch sử đơn hàng
                var datBans = await _context.BanAnDonHangs
                    .Where(b => b.MaDonHangNavigation.MaKhachHang == id)
                    .Select(b => new
                    {
                        MaDonHang = b.MaDonHang,
                        TenBan = b.MaBanNavigation.TenBan,
                        ThoiGianDatHang = b.MaDonHangNavigation.ThoiGianDatHang,
                        TrangThai = b.MaDonHangNavigation.MaTrangThaiDonHang
                    })
                    .OrderByDescending(b => b.ThoiGianDatHang)
                    .Take(10)
                    .ToListAsync();

                return Ok(new
                {
                    Success = true,
                    Profile = new
                    {
                        khachHang.MaKhachHang,
                        khachHang.HoTen,
                        khachHang.SoDienThoai,
                        khachHang.Email,
                        khachHang.HinhAnh,
                        khachHang.SoLanAnTichLuy,
                        khachHang.NoShowCount,
                        khachHang.NgayTao
                    },
                    DonHangs = donHangs,
                    DatBans = datBans
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Success = false, Message = "Lỗi server: " + ex.Message });
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
                    return BadRequest(new { Success = false, Message = "Số điện thoại đã tồn tại" });
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

                return Ok(new { Success = true, Message = "Thêm khách hàng thành công", Data = khachHang });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Success = false, Message = "Lỗi server: " + ex.Message });
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
                    return NotFound(new { Success = false, Message = "Không tìm thấy khách hàng" });
                }

                // Kiểm tra số điện thoại trùng
                if (await _context.KhachHangs.AnyAsync(k => k.SoDienThoai == model.SoDienThoai && k.MaKhachHang != id))
                {
                    return BadRequest(new { Success = false, Message = "Số điện thoại đã được sử dụng bởi khách hàng khác" });
                }

                existing.HoTen = model.HoTen;
                existing.SoDienThoai = model.SoDienThoai;
                existing.Email = model.Email;
                existing.HinhAnh = model.HinhAnh;

                await _context.SaveChangesAsync();

                return Ok(new { Success = true, Message = "Cập nhật khách hàng thành công" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Success = false, Message = "Lỗi server: " + ex.Message });
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
                        TongChiTieu = _context.DonHangs
                            .Where(d => d.MaKhachHang == k.MaKhachHang && d.ThanhToan == true)
                            .Sum(d => d.TienDatCoc ?? 0)
                    })
                    .ToListAsync();

                return Ok(new
                {
                    Success = true,
                    Message = "Dữ liệu xuất Excel đã sẵn sàng",
                    Data = khachHangs
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Success = false, Message = "Lỗi server: " + ex.Message });
            }
        }

        private async Task<string> GenerateCustomerCodeAsync()
        {
            var lastCustomer = await _context.KhachHangs
                .OrderByDescending(k => k.MaKhachHang)
                .FirstOrDefaultAsync();

            if (lastCustomer == null)
            {
                return "KH0001";
            }

            var lastNumber = int.Parse(lastCustomer.MaKhachHang.Substring(2));
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