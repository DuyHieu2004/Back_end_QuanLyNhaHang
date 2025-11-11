using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyNhaHang.Models;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace QuanLyNhaHang.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InventoryAPIController : ControllerBase
    {
        private readonly QLNhaHangContext _context;

        public InventoryAPIController(QLNhaHangContext context)
        {
            _context = context;
        }

        [HttpGet("ingredients")]
        public async Task<IActionResult> GetIngredients()
        {
            var ingredients = await _context.NguyenLieus
                .Select(n => new
                {
                    n.MaNguyenLieu,
                    n.TenNguyenLieu,
                    n.DonViTinh,
                    Stock = 0, // Cần tính từ các bảng nhập/xuất
                    MinStock = 0
                })
                .ToListAsync();

            return Ok(ingredients);
        }

        [HttpGet("ingredients/{maNguyenLieu}")]
        public async Task<IActionResult> GetIngredient(string maNguyenLieu)
        {
            var ingredient = await _context.NguyenLieus
                .FirstOrDefaultAsync(n => n.MaNguyenLieu == maNguyenLieu);

            if (ingredient == null)
            {
                return NotFound(new { message = "Không tìm thấy nguyên liệu." });
            }

            return Ok(ingredient);
        }

        public class NhapKhoDTO
        {
            [Required]
            public string MaNhanVien { get; set; } = null!;
            public string? MaNhaCungCap { get; set; }
            public List<ChiTietNhapKhoDTO> ChiTiet { get; set; } = new();
            public string? GhiChu { get; set; }
        }

        public class ChiTietNhapKhoDTO
        {
            [Required]
            public string MaCungUng { get; set; } = null!;
            [Required]
            public int SoLuong { get; set; }
            [Required]
            public decimal GiaNhap { get; set; }
        }

        [HttpPost("import")]
        public async Task<IActionResult> ImportInventory([FromBody] NhapKhoDTO dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var nhanVien = await _context.NhanViens.FindAsync(dto.MaNhanVien);
                if (nhanVien == null)
                {
                    return NotFound(new { message = "Không tìm thấy nhân viên." });
                }

                string maNhapHang = "NH" + DateTime.Now.ToString("yyMMddHHmmss");
                decimal tongTien = dto.ChiTiet.Sum(c => c.SoLuong * c.GiaNhap);

                var nhapNguyenLieu = new NhapNguyenLieu
                {
                    MaNhapHang = maNhapHang,
                    MaNhanVien = dto.MaNhanVien,
                    NgayNhapHang = DateTime.Now,
                    TongTien = tongTien
                };

                _context.NhapNguyenLieus.Add(nhapNguyenLieu);

                foreach (var item in dto.ChiTiet)
                {
                    var cungUng = await _context.CungUngs
                        .Include(c => c.MaNguyenLieuNavigation)
                        .FirstOrDefaultAsync(c => c.MaCungUng == item.MaCungUng);
                    if (cungUng == null)
                    {
                        return BadRequest(new { message = $"Không tìm thấy cung ứng: {item.MaCungUng}" });
                    }

                    var chiTiet = new ChiTietNhapNguyenLieu
                    {
                        MaNhapHang = maNhapHang,
                        MaCungUng = item.MaCungUng,
                        SoLuong = item.SoLuong,
                        GiaNhap = item.GiaNhap
                    };
                    _context.ChiTietNhapNguyenLieus.Add(chiTiet);
                }

                await _context.SaveChangesAsync();

                var result = await _context.NhapNguyenLieus
                    .Include(n => n.ChiTietNhapNguyenLieus)
                        .ThenInclude(c => c.MaCungUngNavigation)
                            .ThenInclude(c => c.MaNguyenLieuNavigation)
                    .Include(n => n.MaNhanVienNavigation)
                    .FirstOrDefaultAsync(n => n.MaNhapHang == maNhapHang);

                return Ok(new { message = "Nhập kho thành công!", nhapHang = result });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi máy chủ: " + ex.Message });
            }
        }

        [HttpGet("transactions")]
        public async Task<IActionResult> GetTransactions([FromQuery] DateTime? fromDate, [FromQuery] DateTime? toDate)
        {
            var query = _context.NhapNguyenLieus
                .Include(n => n.ChiTietNhapNguyenLieus)
                    .ThenInclude(c => c.MaCungUngNavigation)
                        .ThenInclude(c => c.MaNguyenLieuNavigation)
                .Include(n => n.MaNhanVienNavigation)
                .AsQueryable();

            if (fromDate.HasValue)
            {
                query = query.Where(n => n.NgayNhapHang >= fromDate.Value);
            }

            if (toDate.HasValue)
            {
                query = query.Where(n => n.NgayNhapHang <= toDate.Value);
            }

            var transactions = await query
                .OrderByDescending(n => n.NgayNhapHang)
                .ToListAsync();

            return Ok(transactions);
        }
    }
}

