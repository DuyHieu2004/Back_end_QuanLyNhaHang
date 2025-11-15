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

        //[HttpPost]
        //public async Task<IActionResult> CreateOrder([FromBody] CreateOrderDTO dto)
        //{
        //    if (!ModelState.IsValid) return BadRequest(ModelState);

        //    using var transaction = await _context.Database.BeginTransactionAsync();
        //    try
        //    {
        //        var banAn = await _context.BanAns.FindAsync(dto.MaBan);
        //        if (banAn == null) return NotFound(new { message = "Không tìm thấy bàn." });

        //        var khachHang = await _context.KhachHangs.FindAsync(dto.MaKhachHang);
        //        if (khachHang == null) return NotFound(new { message = "Không tìm thấy khách hàng." });

        //        string maDonHang = "DH" + DateTime.Now.ToString("yyMMddHHmmss");
        //        var donHang = new DonHang
        //        {
        //            MaDonHang = maDonHang,
        //            MaKhachHang = dto.MaKhachHang,
        //            MaNhanVien = dto.MaNhanVien,
        //            MaTrangThaiDonHang = "DANG_PHUC_VU",
        //            ThoiGianDatHang = DateTime.Now,
        //            ThoiGianKetThuc = DateTime.Now,
        //            SoLuongNguoiDk = banAn.SucChua,
        //            GhiChu = dto.GhiChu
        //        };

        //        donHang.MaBans.Add(banAn);
        //        _context.DonHangs.Add(donHang);

        //        foreach (var item in dto.ChiTietDonHang)
        //        {
        //            var phienBan = await _context.PhienBanMonAns
        //                .Include(p => p.MaTrangThaiNavigation)
        //                .FirstOrDefaultAsync(p => p.MaPhienBan == item.MaPhienBan);
        //            if (phienBan == null)
        //            {
        //                return BadRequest(new { message = $"Không tìm thấy phiên bản món ăn: {item.MaPhienBan}" });
        //            }
        //            if (!phienBan.IsShow)
        //            {
        //                return BadRequest(new { message = $"Phiên bản món ăn {phienBan.TenPhienBan} đã bị ẩn." });
        //            }

        //            if (phienBan.MaTrangThai != "CON_HANG") 
        //                return BadRequest(new { message = $"Món {phienBan.TenPhienBan} đang tạm ngưng phục vụ." });

        //            var congThuc = await _context.CongThucNauAns
        //                .Where(ct => ct.MaPhienBan == item.MaPhienBan)
        //                .OrderByDescending(ct => ct.Gia)
        //                .FirstOrDefaultAsync();

        //            if (congThuc == null) return BadRequest(new { message = $"Lỗi dữ liệu: Món {phienBan.TenPhienBan} chưa có giá." });

        //            var chiTiet = new ChiTietDonHang
        //            {
        //                MaDonHang = maDonHang,
        //                MaPhienBan = item.MaPhienBan,
        //                MaCongThuc = congThuc.MaCongThuc, 
        //                SoLuong = item.SoLuong
        //            };
        //            _context.ChiTietDonHangs.Add(chiTiet);
        //        }

        //        await _context.SaveChangesAsync();
        //        await transaction.CommitAsync();

        //        return Ok(new { message = "Tạo đơn hàng thành công!", maDonHang = maDonHang });
        //    }
        //    catch (Exception ex)
        //    {
        //        await transaction.RollbackAsync();
        //        return StatusCode(500, new { message = "Lỗi máy chủ: " + ex.Message });
        //    }
        //}

        // 2. LẤY CHI TIẾT ĐƠN HÀNG
        //[HttpGet("{maDonHang}")]
        //public async Task<IActionResult> GetOrder(string maDonHang)
        //{
        //    var donHang = await _context.DonHangs
        //        .Include(d => d.ChiTietDonHangs)
        //            .ThenInclude(ct => ct.MaPhienBanNavigation)
        //        .Include(d => d.ChiTietDonHangs)
        //            .ThenInclude(ct => ct.MaCongThucNavigation)
        //                .ThenInclude(congThuc => congThuc.MaCtNavigation)
        //                    .ThenInclude(chiTietMon => chiTietMon.MaMonAnNavigation)
        //                        .ThenInclude(mon => mon.HinhAnhMonAns)
        //        .Include(d => d.MaBans)
        //        .Include(d => d.MaKhachHangNavigation)
        //        .Include(d => d.MaTrangThaiDonHangNavigation)
        //        .FirstOrDefaultAsync(d => d.MaDonHang == maDonHang);

        //    if (donHang == null) return NotFound(new { message = "Không tìm thấy đơn hàng." });

        //    return Ok(donHang);
        //}

        //// 3. LẤY ĐƠN HÀNG THEO BÀN
        //[HttpGet("by-table/{maBan}")]
        //public async Task<IActionResult> GetOrderByTable(string maBan)
        //{
        //    var donHang = await _context.DonHangs
        //        .Where(d => d.MaBans.Any(b => b.MaBan == maBan) &&
        //                   (d.MaTrangThaiDonHang == "DANG_PHUC_VU" || d.MaTrangThaiDonHang == "CHO_XAC_NHAN"))
        //        .Include(d => d.ChiTietDonHangs)
        //            .ThenInclude(ct => ct.MaPhienBanNavigation)
        //        .Include(d => d.ChiTietDonHangs)
        //            .ThenInclude(ct => ct.MaCongThucNavigation)
        //                .ThenInclude(congThuc => congThuc.MaCtNavigation)
        //                    .ThenInclude(chiTietMon => chiTietMon.MaMonAnNavigation)
        //                        .ThenInclude(mon => mon.HinhAnhMonAns)
        //        .Include(d => d.MaBans)
        //        .Include(d => d.MaKhachHangNavigation)
        //        .Include(d => d.MaTrangThaiDonHangNavigation)
        //        .OrderByDescending(d => d.ThoiGianDatHang)
        //        .FirstOrDefaultAsync();

        //    if (donHang == null) return NotFound(new { message = "Bàn này đang trống." });

        //    return Ok(donHang);
        //}

        // //4. CẬP NHẬT ĐƠN HÀNG(Ghi đè món)
        //[HttpPut("{maDonHang}")]
        //public async Task<IActionResult> UpdateOrder(string maDonHang, [FromBody] UpdateOrderDTO dto)
        //{
        //    var donHang = await _context.DonHangs
        //        .Include(d => d.ChiTietDonHangs)
        //        .FirstOrDefaultAsync(d => d.MaDonHang == maDonHang);

        //    if (donHang == null) return NotFound(new { message = "Không tìm thấy đơn hàng." });

        //    if (donHang.MaTrangThaiDonHang == "DA_HOAN_THANH" || donHang.MaTrangThaiDonHang == "DA_HUY")
        //        return BadRequest(new { message = "Đơn hàng đã đóng, không thể sửa." });

        //    using var transaction = await _context.Database.BeginTransactionAsync();
        //    try
        //    {
        //        _context.ChiTietDonHangs.RemoveRange(donHang.ChiTietDonHangs);

        //        foreach (var item in dto.ChiTietDonHang)
        //        {
        //            var phienBan = await _context.PhienBanMonAns
        //                .Include(p => p.MaTrangThaiNavigation)
        //                .FirstOrDefaultAsync(p => p.MaPhienBan == item.MaPhienBan);
        //            if (phienBan == null)
        //            {
        //                return BadRequest(new { message = $"Không tìm thấy phiên bản món ăn: {item.MaPhienBan}" });
        //            }
        //            if (!phienBan.IsShow)
        //            {
        //                return BadRequest(new { message = $"Phiên bản món ăn {phienBan.TenPhienBan} đã bị ẩn." });
        //            }

        //            _context.ChiTietDonHangs.Add(new ChiTietDonHang
        //            {
        //                MaDonHang = maDonHang,
        //                MaPhienBan = item.MaPhienBan,
        //                MaCongThuc = congThuc.MaCongThuc,
        //                SoLuong = item.SoLuong
        //            });
        //        }

        //        if (!string.IsNullOrEmpty(dto.GhiChu)) donHang.GhiChu = dto.GhiChu;

        //        await _context.SaveChangesAsync();
        //        await transaction.CommitAsync();

        //        return Ok(new { message = "Cập nhật thành công!" });
        //    }
        //    catch (Exception ex)
        //    {
        //        await transaction.RollbackAsync();
        //        return StatusCode(500, new { message = ex.Message });
        //    }
        //}

        //// 5. THÊM MÓN VÀO ĐƠN (Add Item)
        //[HttpPost("{maDonHang}/add-item")]
        //public async Task<IActionResult> AddItemToOrder(string maDonHang, [FromBody] OrderItemDTO itemDto)
        //{
        //    var donHang = await _context.DonHangs.FindAsync(maDonHang);
        //    if (donHang == null) return NotFound(new { message = "Không tìm thấy đơn hàng." });

        //    if (donHang.MaTrangThaiDonHang == "DA_HOAN_THANH" || donHang.MaTrangThaiDonHang == "DA_HUY")
        //        return BadRequest(new { message = "Đơn hàng đã đóng." });

        //    var phienBan = await _context.PhienBanMonAns
        //        .Include(p => p.MaTrangThaiNavigation)
        //        .FirstOrDefaultAsync(p => p.MaPhienBan == itemDto.MaPhienBan);
        //    if (phienBan == null)
        //    {
        //        return BadRequest(new { message = "Không tìm thấy phiên bản món ăn." });
        //    }
        //    if (!phienBan.IsShow)
        //    {
        //        return BadRequest(new { message = $"Phiên bản món ăn {phienBan.TenPhienBan} đã bị ẩn." });
        //    }

        //    var existingItem = await _context.ChiTietDonHangs
        //        .FirstOrDefaultAsync(c => c.MaDonHang == maDonHang && c.MaPhienBan == itemDto.MaPhienBan);

        //    if (existingItem != null)
        //    {
        //        existingItem.SoLuong += itemDto.SoLuong;
        //    }
        //    else
        //    {
        //        var congThuc = await _context.CongThucNauAns
        //                .Where(ct => ct.MaPhienBan == itemDto.MaPhienBan)
        //                .OrderByDescending(ct => ct.Gia).FirstOrDefaultAsync();

        //        if (congThuc == null) return BadRequest(new { message = "Lỗi dữ liệu giá món." });

        //        var chiTiet = new ChiTietDonHang
        //        {
        //            MaDonHang = maDonHang,
        //            MaPhienBan = itemDto.MaPhienBan,
        //            MaCongThuc = congThuc.MaCongThuc,
        //            SoLuong = itemDto.SoLuong
        //        };
        //        _context.ChiTietDonHangs.Add(chiTiet);
        //    }

        //    await _context.SaveChangesAsync();
        //    return Ok(new { message = "Thêm món thành công!" });
        //}

        // 6. HOÀN THÀNH ĐƠN
        [HttpPost("{maDonHang}/complete")]
        public async Task<IActionResult> CompleteOrder(string maDonHang)
        {
            var donHang = await _context.DonHangs.FindAsync(maDonHang);
            if (donHang == null) return NotFound(new { message = "Không tìm thấy đơn hàng." });

            donHang.MaTrangThaiDonHang = "DA_HOAN_THANH";
            donHang.ThoiGianKetThuc = DateTime.Now;

            await _context.SaveChangesAsync();
            return Ok(new { message = "Đơn hàng đã hoàn thành!" });
        }
    }
}


