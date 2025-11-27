// Controllers/KhuyenMaiController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyNhaHang.Models;
using System.Linq;
using System.Threading.Tasks;

namespace QuanLyNhaHang.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class KhuyenMaiController : ControllerBase
    {
        private readonly QLNhaHangContext _context;

        public KhuyenMaiController(QLNhaHangContext context)
        {
            _context = context;
        }

        // GET: api/KhuyenMai
        [HttpGet]
        public async Task<IActionResult> GetKhuyenMai()
        {
            var khuyenMais = await _context.KhuyenMais
                .Include(k => k.KhuyenMaiApDungSanPhams)
                .ThenInclude(a => a.MaCongThucNavigation)
                .ThenInclude(c => c.MaCtNavigation)
                .ThenInclude(ct => ct.MaMonAnNavigation)
                .Include(k => k.KhuyenMaiApDungSanPhams)
                .ThenInclude(a => a.MaDanhMucNavigation)
                .Select(k => new
                {
                    k.MaKhuyenMai,
                    k.TenKhuyenMai,
                    k.LoaiKhuyenMai,
                    k.GiaTri,
                    k.NgayBatDau,
                    k.NgayKetThuc,
                    k.TrangThai,
                    k.MoTa,
                    k.ApDungToiThieu,
                    ApDungSanPhams = k.KhuyenMaiApDungSanPhams.Select(a => new
                    {
                        a.Id,
                        a.MaCongThuc,
                        a.MaDanhMuc,
                        TenCongThuc = a.MaCongThucNavigation != null ? a.MaCongThucNavigation.MaPhienBanNavigation.TenPhienBan : null,
                        TenMonAn = a.MaCongThucNavigation != null ? a.MaCongThucNavigation.MaCtNavigation.MaMonAnNavigation.TenMonAn : null,
                        TenDanhMuc = a.MaDanhMucNavigation != null ? a.MaDanhMucNavigation.TenDanhMuc : null,
                        Gia = a.MaCongThucNavigation != null ? a.MaCongThucNavigation.Gia : (decimal?)null
                    })
                })
                .ToListAsync();

            return Ok(khuyenMais);
        }

        // GET: api/KhuyenMai/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetKhuyenMai(string id)
        {
            var khuyenMai = await _context.KhuyenMais
                .Include(k => k.KhuyenMaiApDungSanPhams)
                .FirstOrDefaultAsync(k => k.MaKhuyenMai == id);

            if (khuyenMai == null)
            {
                return NotFound();
            }

            return Ok(khuyenMai);
        }

        // POST: api/KhuyenMai
        [HttpPost]
        public async Task<IActionResult> PostKhuyenMai([FromBody] CreateKhuyenMaiDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Kiểm tra mã khuyến mãi trùng
            if (await _context.KhuyenMais.AnyAsync(k => k.MaKhuyenMai == dto.MaKhuyenMai))
            {
                return BadRequest(new { message = "Mã khuyến mãi đã tồn tại." });
            }

            var khuyenMai = new KhuyenMai
            {
                MaKhuyenMai = dto.MaKhuyenMai,
                TenKhuyenMai = dto.TenKhuyenMai,
                LoaiKhuyenMai = dto.LoaiKhuyenMai,
                GiaTri = dto.GiaTri,
                NgayBatDau = dto.NgayBatDau,
                NgayKetThuc = dto.NgayKetThuc,
                TrangThai = dto.TrangThai,
                MoTa = dto.MoTa,
                ApDungToiThieu = dto.ApDungToiThieu
            };

            _context.KhuyenMais.Add(khuyenMai);

            // Thêm các sản phẩm áp dụng
            if (dto.ApDungSanPhams != null)
            {
                foreach (var sp in dto.ApDungSanPhams)
                {
                    var apDung = new KhuyenMaiApDungSanPham
                    {
                        MaKhuyenMai = dto.MaKhuyenMai,
                        MaCongThuc = sp.MaCongThuc,
                        MaDanhMuc = sp.MaDanhMuc
                    };
                    _context.KhuyenMaiApDungSanPhams.Add(apDung);
                }
            }

            await _context.SaveChangesAsync();

            return Ok(new { message = "Tạo khuyến mãi thành công!" });
        }

        // PUT: api/KhuyenMai/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutKhuyenMai(string id, [FromBody] UpdateKhuyenMaiDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var khuyenMai = await _context.KhuyenMais
                .Include(k => k.KhuyenMaiApDungSanPhams)
                .FirstOrDefaultAsync(k => k.MaKhuyenMai == id);

            if (khuyenMai == null)
            {
                return NotFound();
            }

            // Cập nhật thông tin
            khuyenMai.TenKhuyenMai = dto.TenKhuyenMai;
            khuyenMai.LoaiKhuyenMai = dto.LoaiKhuyenMai;
            khuyenMai.GiaTri = dto.GiaTri;
            khuyenMai.NgayBatDau = dto.NgayBatDau;
            khuyenMai.NgayKetThuc = dto.NgayKetThuc;
            khuyenMai.TrangThai = dto.TrangThai;
            khuyenMai.MoTa = dto.MoTa;
            khuyenMai.ApDungToiThieu = dto.ApDungToiThieu;

            // Xóa các sản phẩm áp dụng cũ và thêm mới
            _context.KhuyenMaiApDungSanPhams.RemoveRange(khuyenMai.KhuyenMaiApDungSanPhams);

            if (dto.ApDungSanPhams != null)
            {
                foreach (var sp in dto.ApDungSanPhams)
                {
                    var apDung = new KhuyenMaiApDungSanPham
                    {
                        MaKhuyenMai = id,
                        MaCongThuc = sp.MaCongThuc,
                        MaDanhMuc = sp.MaDanhMuc
                    };
                    _context.KhuyenMaiApDungSanPhams.Add(apDung);
                }
            }

            await _context.SaveChangesAsync();

            return Ok(new { message = "Cập nhật khuyến mãi thành công!" });
        }

        // DELETE: api/KhuyenMai/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteKhuyenMai(string id)
        {
            var khuyenMai = await _context.KhuyenMais
                .Include(k => k.KhuyenMaiApDungSanPhams)
                .FirstOrDefaultAsync(k => k.MaKhuyenMai == id);

            if (khuyenMai == null)
            {
                return NotFound();
            }

            _context.KhuyenMaiApDungSanPhams.RemoveRange(khuyenMai.KhuyenMaiApDungSanPhams);
            _context.KhuyenMais.Remove(khuyenMai);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Xóa khuyến mãi thành công!" });
        }
        [HttpGet("DanhMuc")]
        public async Task<IActionResult> GetDanhMuc()
        {
            var danhMuc = await _context.DanhMucMonAns
                .Select(d => new { d.MaDanhMuc, d.TenDanhMuc })
                .ToListAsync();
            return Ok(danhMuc);
        }

        [HttpGet("CongThuc")]
        public async Task<IActionResult> GetCongThuc()
        {
            var congThuc = await _context.CongThucNauAns
                .Include(c => c.MaCtNavigation)
                .ThenInclude(ct => ct.MaMonAnNavigation)
                .Include(c => c.MaPhienBanNavigation)
                .Select(c => new
                {
                    c.MaCongThuc,
                    TenMonAn = c.MaCtNavigation.MaMonAnNavigation.TenMonAn,
                    TenPhienBan = c.MaPhienBanNavigation.TenPhienBan,
                    c.Gia
                })
                .ToListAsync();
            return Ok(congThuc);
        }

        [HttpGet("CongThucVaDanhMuc")]
        public async Task<IActionResult> GetCongThucVaDanhMuc()
        {
            var congThuc = await _context.CongThucNauAns
                .Include(c => c.MaCtNavigation)
                .ThenInclude(ct => ct.MaMonAnNavigation)
                .Include(c => c.MaPhienBanNavigation)
                .Select(c => new
                {
                    c.MaCongThuc,
                    TenMonAn = c.MaCtNavigation.MaMonAnNavigation.TenMonAn,
                    TenPhienBan = c.MaPhienBanNavigation.TenPhienBan,
                    c.Gia,
                    MaDanhMuc = c.MaCtNavigation.MaMonAnNavigation.MaDanhMuc,
                    TenDanhMuc = c.MaCtNavigation.MaMonAnNavigation.MaDanhMucNavigation.TenDanhMuc
                })
                .ToListAsync();
            return Ok(congThuc);
        }
    }

    public class CreateKhuyenMaiDto
    {
        public string MaKhuyenMai { get; set; } = null!;
        public string TenKhuyenMai { get; set; } = null!;
        public string LoaiKhuyenMai { get; set; } = null!;
        public decimal GiaTri { get; set; }
        public DateTime NgayBatDau { get; set; }
        public DateTime NgayKetThuc { get; set; }
        public string TrangThai { get; set; } = null!;
        public string? MoTa { get; set; }
        public decimal? ApDungToiThieu { get; set; }
        public List<ApDungSanPhamDto>? ApDungSanPhams { get; set; }
    }

    public class UpdateKhuyenMaiDto
    {
        public string TenKhuyenMai { get; set; } = null!;
        public string LoaiKhuyenMai { get; set; } = null!;
        public decimal GiaTri { get; set; }
        public DateTime NgayBatDau { get; set; }
        public DateTime NgayKetThuc { get; set; }
        public string TrangThai { get; set; } = null!;
        public string? MoTa { get; set; }
        public decimal? ApDungToiThieu { get; set; }
        public List<ApDungSanPhamDto>? ApDungSanPhams { get; set; }
    }

    public class ApDungSanPhamDto
    {
        public string? MaCongThuc { get; set; }
        public string? MaDanhMuc { get; set; }
    }
}