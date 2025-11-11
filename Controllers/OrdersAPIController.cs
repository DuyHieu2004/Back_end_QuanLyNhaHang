using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyNhaHang.Models;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace QuanLyNhaHang.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersAPIController : ControllerBase
    {
        private readonly QLNhaHangContext _context;

        public OrdersAPIController(QLNhaHangContext context)
        {
            _context = context;
        }

        public class CreateOrderDTO
        {
            [Required]
            public string MaBan { get; set; } = null!;
            [Required]
            public string MaKhachHang { get; set; } = null!;
            public string? MaNhanVien { get; set; }
            public List<OrderItemDTO> ChiTietDonHang { get; set; } = new();
            public string? GhiChu { get; set; }
        }

        public class OrderItemDTO
        {
            [Required]
            public string MaPhienBan { get; set; } = null!;
            [Required]
            public int SoLuong { get; set; }
        }

        public class UpdateOrderDTO
        {
            public List<OrderItemDTO> ChiTietDonHang { get; set; } = new();
            public string? GhiChu { get; set; }
        }

        [HttpPost]
        public async Task<IActionResult> CreateOrder([FromBody] CreateOrderDTO dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var banAn = await _context.BanAns.FindAsync(dto.MaBan);
                if (banAn == null)
                {
                    return NotFound(new { message = "Không tìm thấy bàn." });
                }

                var khachHang = await _context.KhachHangs.FindAsync(dto.MaKhachHang);
                if (khachHang == null)
                {
                    return NotFound(new { message = "Không tìm thấy khách hàng." });
                }

                string maDonHang = "DH" + DateTime.Now.ToString("yyMMddHHmmss");
                var donHang = new DonHang
                {
                    MaDonHang = maDonHang,
                    MaBan = dto.MaBan,
                    MaKhachHang = dto.MaKhachHang,
                    MaNhanVien = dto.MaNhanVien,
                    MaTrangThaiDonHang = "DANG_PHUC_VU",
                    ThoiGianDatHang = DateTime.Now,
                    ThoiGianBatDau = DateTime.Now,
                    SoLuongNguoi = banAn.SucChua,
                    GhiChu = dto.GhiChu
                };

                _context.DonHangs.Add(donHang);

                foreach (var item in dto.ChiTietDonHang)
                {
                    var phienBan = await _context.PhienBanMonAns.FindAsync(item.MaPhienBan);
                    if (phienBan == null)
                    {
                        return BadRequest(new { message = $"Không tìm thấy phiên bản món ăn: {item.MaPhienBan}" });
                    }

                    var chiTiet = new ChiTietDonHang
                    {
                        MaDonHang = maDonHang,
                        MaPhienBan = item.MaPhienBan,
                        SoLuong = item.SoLuong
                    };
                    _context.ChiTietDonHangs.Add(chiTiet);
                }

                await _context.SaveChangesAsync();

                var result = await _context.DonHangs
                    .Include(d => d.ChiTietDonHangs)
                        .ThenInclude(c => c.MaPhienBanNavigation)
                            .ThenInclude(p => p.MaMonAnNavigation)
                    .Include(d => d.MaBanNavigation)
                    .Include(d => d.MaKhachHangNavigation)
                    .FirstOrDefaultAsync(d => d.MaDonHang == maDonHang);

                return Ok(new { message = "Tạo đơn hàng thành công!", donHang = result });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi máy chủ: " + ex.Message });
            }
        }

        [HttpGet("{maDonHang}")]
        public async Task<IActionResult> GetOrder(string maDonHang)
        {
            var donHang = await _context.DonHangs
                .Include(d => d.ChiTietDonHangs)
                    .ThenInclude(c => c.MaPhienBanNavigation)
                        .ThenInclude(p => p.MaMonAnNavigation)
                            .ThenInclude(m => m.HinhAnhMonAns)
                .Include(d => d.MaBanNavigation)
                .Include(d => d.MaKhachHangNavigation)
                .Include(d => d.MaTrangThaiDonHangNavigation)
                .FirstOrDefaultAsync(d => d.MaDonHang == maDonHang);

            if (donHang == null)
            {
                return NotFound(new { message = "Không tìm thấy đơn hàng." });
            }

            return Ok(donHang);
        }

        [HttpGet("by-table/{maBan}")]
        public async Task<IActionResult> GetOrderByTable(string maBan)
        {
            var donHang = await _context.DonHangs
                .Where(d => d.MaBan == maBan && 
                           (d.MaTrangThaiDonHang == "DANG_PHUC_VU" || d.MaTrangThaiDonHang == "CHO_XAC_NHAN"))
                .Include(d => d.ChiTietDonHangs)
                    .ThenInclude(c => c.MaPhienBanNavigation)
                        .ThenInclude(p => p.MaMonAnNavigation)
                            .ThenInclude(m => m.HinhAnhMonAns)
                .Include(d => d.MaBanNavigation)
                .Include(d => d.MaKhachHangNavigation)
                .Include(d => d.MaTrangThaiDonHangNavigation)
                .OrderByDescending(d => d.ThoiGianBatDau)
                .FirstOrDefaultAsync();

            if (donHang == null)
            {
                return NotFound(new { message = "Không tìm thấy đơn hàng đang hoạt động cho bàn này." });
            }

            return Ok(donHang);
        }

        [HttpPut("{maDonHang}")]
        public async Task<IActionResult> UpdateOrder(string maDonHang, [FromBody] UpdateOrderDTO dto)
        {
            var donHang = await _context.DonHangs
                .Include(d => d.ChiTietDonHangs)
                .FirstOrDefaultAsync(d => d.MaDonHang == maDonHang);

            if (donHang == null)
            {
                return NotFound(new { message = "Không tìm thấy đơn hàng." });
            }

            if (donHang.MaTrangThaiDonHang == "DA_HOAN_THANH" || donHang.MaTrangThaiDonHang == "DA_HUY")
            {
                return BadRequest(new { message = "Không thể cập nhật đơn hàng đã hoàn thành hoặc đã hủy." });
            }

            try
            {
                _context.ChiTietDonHangs.RemoveRange(donHang.ChiTietDonHangs);

                foreach (var item in dto.ChiTietDonHang)
                {
                    var phienBan = await _context.PhienBanMonAns.FindAsync(item.MaPhienBan);
                    if (phienBan == null)
                    {
                        return BadRequest(new { message = $"Không tìm thấy phiên bản món ăn: {item.MaPhienBan}" });
                    }

                    var chiTiet = new ChiTietDonHang
                    {
                        MaDonHang = maDonHang,
                        MaPhienBan = item.MaPhienBan,
                        SoLuong = item.SoLuong
                    };
                    _context.ChiTietDonHangs.Add(chiTiet);
                }

                if (!string.IsNullOrEmpty(dto.GhiChu))
                {
                    donHang.GhiChu = dto.GhiChu;
                }

                await _context.SaveChangesAsync();

                var result = await _context.DonHangs
                    .Include(d => d.ChiTietDonHangs)
                        .ThenInclude(c => c.MaPhienBanNavigation)
                            .ThenInclude(p => p.MaMonAnNavigation)
                    .FirstOrDefaultAsync(d => d.MaDonHang == maDonHang);

                return Ok(new { message = "Cập nhật đơn hàng thành công!", donHang = result });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi máy chủ: " + ex.Message });
            }
        }

        [HttpPost("{maDonHang}/add-item")]
        public async Task<IActionResult> AddItemToOrder(string maDonHang, [FromBody] OrderItemDTO itemDto)
        {
            var donHang = await _context.DonHangs.FindAsync(maDonHang);
            if (donHang == null)
            {
                return NotFound(new { message = "Không tìm thấy đơn hàng." });
            }

            if (donHang.MaTrangThaiDonHang == "DA_HOAN_THANH" || donHang.MaTrangThaiDonHang == "DA_HUY")
            {
                return BadRequest(new { message = "Không thể thêm món vào đơn hàng đã hoàn thành hoặc đã hủy." });
            }

            var phienBan = await _context.PhienBanMonAns.FindAsync(itemDto.MaPhienBan);
            if (phienBan == null)
            {
                return BadRequest(new { message = "Không tìm thấy phiên bản món ăn." });
            }

            var existingItem = await _context.ChiTietDonHangs
                .FirstOrDefaultAsync(c => c.MaDonHang == maDonHang && c.MaPhienBan == itemDto.MaPhienBan);

            if (existingItem != null)
            {
                existingItem.SoLuong += itemDto.SoLuong;
            }
            else
            {
                var chiTiet = new ChiTietDonHang
                {
                    MaDonHang = maDonHang,
                    MaPhienBan = itemDto.MaPhienBan,
                    SoLuong = itemDto.SoLuong
                };
                _context.ChiTietDonHangs.Add(chiTiet);
            }

            await _context.SaveChangesAsync();

            var result = await _context.DonHangs
                .Include(d => d.ChiTietDonHangs)
                    .ThenInclude(c => c.MaPhienBanNavigation)
                        .ThenInclude(p => p.MaMonAnNavigation)
                .FirstOrDefaultAsync(d => d.MaDonHang == maDonHang);

            return Ok(new { message = "Thêm món vào đơn hàng thành công!", donHang = result });
        }

        [HttpPost("{maDonHang}/complete")]
        public async Task<IActionResult> CompleteOrder(string maDonHang)
        {
            var donHang = await _context.DonHangs.FindAsync(maDonHang);
            if (donHang == null)
            {
                return NotFound(new { message = "Không tìm thấy đơn hàng." });
            }

            donHang.MaTrangThaiDonHang = "DA_HOAN_THANH";
            donHang.ThoiGianKetThuc = DateTime.Now;

            await _context.SaveChangesAsync();

            return Ok(new { message = "Đơn hàng đã được hoàn thành!", donHang });
        }
    }
}

