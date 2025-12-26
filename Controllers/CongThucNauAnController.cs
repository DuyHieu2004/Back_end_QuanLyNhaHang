using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyNhaHang.Models;
using QuanLyNhaHang.Models.Dtos;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace QuanLyNhaHang.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CongThucNauAnController : ControllerBase
    {
        private readonly QLNhaHangContext _context;

        public CongThucNauAnController(QLNhaHangContext context)
        {
            _context = context;
        }

        // GET: api/CongThucNauAn
        [HttpGet]
        public async Task<IActionResult> GetCongThucNauAn()
        {
            var congThucs = await _context.CongThucNauAns
                .Include(c => c.MaCtNavigation)
                .ThenInclude(ct => ct.MaMonAnNavigation)
                .Include(c => c.MaPhienBanNavigation)
                .Include(c => c.ChiTietCongThucs)
                .Select(c => new RecipeListItemDto
                {
                    MaCongThuc = c.MaCongThuc,
                    TenMonAn = c.MaCtNavigation.MaMonAnNavigation.TenMonAn,
                    TenCt = c.MaCtNavigation.TenCt,
                    TenPhienBan = c.MaPhienBanNavigation.TenPhienBan,
                    Gia = c.Gia,
                    SoLuongNguyenLieu = c.ChiTietCongThucs.Count
                })
                .ToListAsync();

            return Ok(congThucs);
        }

        // GET: api/CongThucNauAn/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetCongThucNauAnById(string id)
        {
            var congThuc = await _context.CongThucNauAns
                .Include(c => c.MaCtNavigation)
                .ThenInclude(ct => ct.MaMonAnNavigation)
                .Include(c => c.MaPhienBanNavigation)
                .Include(c => c.ChiTietCongThucs)
                .ThenInclude(ct => ct.MaNguyenLieuNavigation)
                .FirstOrDefaultAsync(c => c.MaCongThuc == id);

            if (congThuc == null)
            {
                return NotFound(new { message = "Không tìm thấy công thức" });
            }

            var recipeDto = new RecipeDto
            {
                MaCongThuc = congThuc.MaCongThuc,
                MaCt = congThuc.MaCt,
                TenCt = congThuc.MaCtNavigation.TenCt,
                MaMonAn = congThuc.MaCtNavigation.MaMonAn,
                TenMonAn = congThuc.MaCtNavigation.MaMonAnNavigation.TenMonAn,
                MaPhienBan = congThuc.MaPhienBan,
                TenPhienBan = congThuc.MaPhienBanNavigation.TenPhienBan,
                Gia = congThuc.Gia,
                Ingredients = congThuc.ChiTietCongThucs.Select(ct => new RecipeIngredientDto
                {
                    MaChiTietCongThuc = ct.MaChiTietCongThuc,
                    MaNguyenLieu = ct.MaNguyenLieu,
                    TenNguyenLieu = ct.MaNguyenLieuNavigation.TenNguyenLieu,
                    DonViTinh = ct.MaNguyenLieuNavigation.DonViTinh,
                    SoLuongCanDung = ct.SoLuongCanDung,
                    GiaBan = ct.MaNguyenLieuNavigation.GiaBan,
                    ThanhTien = ct.SoLuongCanDung * ct.MaNguyenLieuNavigation.GiaBan
                }).ToList()
            };

            return Ok(recipeDto);
        }

        // POST: api/CongThucNauAn
        [HttpPost]
        public async Task<IActionResult> CreateCongThucNauAn([FromBody] CreateRecipeDto dto)
        {
            // Validate ChiTietMonAn exists
            var chiTietMonAn = await _context.ChiTietMonAns.FindAsync(dto.MaCt);
            if (chiTietMonAn == null)
            {
                return BadRequest(new { message = "Chi tiết món ăn không tồn tại" });
            }

            // Validate PhienBan exists
            var phienBan = await _context.PhienBanMonAns.FindAsync(dto.MaPhienBan);
            if (phienBan == null)
            {
                return BadRequest(new { message = "Phiên bản không tồn tại" });
            }

            // Check if recipe already exists for this combination
            var existingRecipe = await _context.CongThucNauAns
                .FirstOrDefaultAsync(c => c.MaCt == dto.MaCt && c.MaPhienBan == dto.MaPhienBan);
            if (existingRecipe != null)
            {
                return BadRequest(new { message = "Công thức cho chi tiết món ăn và phiên bản này đã tồn tại" });
            }

            // Generate new ID
            var maxId = await _context.CongThucNauAns
                .OrderByDescending(c => c.MaCongThuc)
                .Select(c => c.MaCongThuc)
                .FirstOrDefaultAsync();

            string newId;
            if (string.IsNullOrEmpty(maxId))
            {
                newId = "CT001";
            }
            else
            {
                var numPart = int.Parse(maxId.Substring(2));
                newId = $"CT{(numPart + 1):D3}";
            }

            var congThuc = new CongThucNauAn
            {
                MaCongThuc = newId,
                MaCt = dto.MaCt,
                MaPhienBan = dto.MaPhienBan,
                Gia = dto.Gia
            };

            _context.CongThucNauAns.Add(congThuc);
            await _context.SaveChangesAsync();

            // Add ingredients if provided
            if (dto.Ingredients != null && dto.Ingredients.Any())
            {
                foreach (var ingredient in dto.Ingredients)
                {
                    var chiTietCongThuc = new ChiTietCongThuc
                    {
                        MaCongThuc = newId,
                        MaNguyenLieu = ingredient.MaNguyenLieu,
                        SoLuongCanDung = ingredient.SoLuongCanDung
                    };
                    _context.ChiTietCongThucs.Add(chiTietCongThuc);
                }
                await _context.SaveChangesAsync();
            }

            return CreatedAtAction(nameof(GetCongThucNauAnById), new { id = newId }, new { maCongThuc = newId });
        }

        // PUT: api/CongThucNauAn/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCongThucNauAn(string id, [FromBody] UpdateRecipeDto dto)
        {
            var congThuc = await _context.CongThucNauAns.FindAsync(id);
            if (congThuc == null)
            {
                return NotFound(new { message = "Không tìm thấy công thức" });
            }

            if (!string.IsNullOrEmpty(dto.MaCt))
            {
                var chiTietMonAn = await _context.ChiTietMonAns.FindAsync(dto.MaCt);
                if (chiTietMonAn == null)
                {
                    return BadRequest(new { message = "Chi tiết món ăn không tồn tại" });
                }
                congThuc.MaCt = dto.MaCt;
            }

            if (!string.IsNullOrEmpty(dto.MaPhienBan))
            {
                var phienBan = await _context.PhienBanMonAns.FindAsync(dto.MaPhienBan);
                if (phienBan == null)
                {
                    return BadRequest(new { message = "Phiên bản không tồn tại" });
                }
                congThuc.MaPhienBan = dto.MaPhienBan;
            }

            if (dto.Gia.HasValue)
            {
                congThuc.Gia = dto.Gia.Value;
            }

            await _context.SaveChangesAsync();

            return Ok(new { message = "Cập nhật công thức thành công" });
        }

        // DELETE: api/CongThucNauAn/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCongThucNauAn(string id)
        {
            var congThuc = await _context.CongThucNauAns
                .Include(c => c.ChiTietCongThucs)
                .Include(c => c.ChiTietDonHangs)
                .Include(c => c.ChiTietMenus)
                .FirstOrDefaultAsync(c => c.MaCongThuc == id);

            if (congThuc == null)
            {
                return NotFound(new { message = "Không tìm thấy công thức" });
            }

            // Check if recipe is used in orders
            if (congThuc.ChiTietDonHangs.Any())
            {
                return BadRequest(new { message = "Không thể xóa công thức đã được sử dụng trong đơn hàng" });
            }

            // Check if recipe is used in menus
            if (congThuc.ChiTietMenus.Any())
            {
                return BadRequest(new { message = "Không thể xóa công thức đã được sử dụng trong menu" });
            }

            // Delete all ingredients first
            _context.ChiTietCongThucs.RemoveRange(congThuc.ChiTietCongThucs);
            _context.CongThucNauAns.Remove(congThuc);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Xóa công thức thành công" });
        }

        // GET: api/CongThucNauAn/5/ingredients
        [HttpGet("{id}/ingredients")]
        public async Task<IActionResult> GetRecipeIngredients(string id)
        {
            var congThuc = await _context.CongThucNauAns.FindAsync(id);
            if (congThuc == null)
            {
                return NotFound(new { message = "Không tìm thấy công thức" });
            }

            var ingredients = await _context.ChiTietCongThucs
                .Include(ct => ct.MaNguyenLieuNavigation)
                .Where(ct => ct.MaCongThuc == id)
                .Select(ct => new RecipeIngredientDto
                {
                    MaChiTietCongThuc = ct.MaChiTietCongThuc,
                    MaNguyenLieu = ct.MaNguyenLieu,
                    TenNguyenLieu = ct.MaNguyenLieuNavigation.TenNguyenLieu,
                    DonViTinh = ct.MaNguyenLieuNavigation.DonViTinh,
                    SoLuongCanDung = ct.SoLuongCanDung,
                    GiaBan = ct.MaNguyenLieuNavigation.GiaBan,
                    ThanhTien = ct.SoLuongCanDung * ct.MaNguyenLieuNavigation.GiaBan
                })
                .ToListAsync();

            return Ok(ingredients);
        }

        // POST: api/CongThucNauAn/5/ingredients
        [HttpPost("{id}/ingredients")]
        public async Task<IActionResult> AddIngredientToRecipe(string id, [FromBody] AddIngredientToRecipeDto dto)
        {
            var congThuc = await _context.CongThucNauAns.FindAsync(id);
            if (congThuc == null)
            {
                return NotFound(new { message = "Không tìm thấy công thức" });
            }

            var nguyenLieu = await _context.NguyenLieus.FindAsync(dto.MaNguyenLieu);
            if (nguyenLieu == null)
            {
                return BadRequest(new { message = "Nguyên liệu không tồn tại" });
            }

            // Check if ingredient already exists in recipe
            var existingIngredient = await _context.ChiTietCongThucs
                .FirstOrDefaultAsync(ct => ct.MaCongThuc == id && ct.MaNguyenLieu == dto.MaNguyenLieu);

            if (existingIngredient != null)
            {
                return BadRequest(new { message = "Nguyên liệu đã tồn tại trong công thức" });
            }

            var chiTietCongThuc = new ChiTietCongThuc
            {
                MaCongThuc = id,
                MaNguyenLieu = dto.MaNguyenLieu,
                SoLuongCanDung = dto.SoLuongCanDung
            };

            _context.ChiTietCongThucs.Add(chiTietCongThuc);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Thêm nguyên liệu vào công thức thành công", maChiTietCongThuc = chiTietCongThuc.MaChiTietCongThuc });
        }

        // DELETE: api/CongThucNauAn/5/ingredients/10
        [HttpDelete("{id}/ingredients/{ingredientId}")]
        public async Task<IActionResult> RemoveIngredientFromRecipe(string id, long ingredientId)
        {
            var chiTietCongThuc = await _context.ChiTietCongThucs
                .FirstOrDefaultAsync(ct => ct.MaCongThuc == id && ct.MaChiTietCongThuc == ingredientId);

            if (chiTietCongThuc == null)
            {
                return NotFound(new { message = "Không tìm thấy nguyên liệu trong công thức" });
            }

            _context.ChiTietCongThucs.Remove(chiTietCongThuc);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Xóa nguyên liệu khỏi công thức thành công" });
        }

        // GET: api/CongThucNauAn/5/calculate?servings=10
        [HttpGet("{id}/calculate")]
        public async Task<IActionResult> CalculateIngredients(string id, [FromQuery] int servings = 1)
        {
            if (servings <= 0)
            {
                return BadRequest(new { message = "Số suất ăn phải lớn hơn 0" });
            }

            var congThuc = await _context.CongThucNauAns
                .Include(c => c.MaCtNavigation)
                .ThenInclude(ct => ct.MaMonAnNavigation)
                .Include(c => c.MaPhienBanNavigation)
                .Include(c => c.ChiTietCongThucs)
                .ThenInclude(ct => ct.MaNguyenLieuNavigation)
                .FirstOrDefaultAsync(c => c.MaCongThuc == id);

            if (congThuc == null)
            {
                return NotFound(new { message = "Không tìm thấy công thức" });
            }

            var calculatedIngredients = congThuc.ChiTietCongThucs.Select(ct => new CalculatedIngredientDto
            {
                TenNguyenLieu = ct.MaNguyenLieuNavigation.TenNguyenLieu,
                DonViTinh = ct.MaNguyenLieuNavigation.DonViTinh,
                SoLuongGoc = ct.SoLuongCanDung,
                SoLuongCanDung = ct.SoLuongCanDung * servings,
                GiaBan = ct.MaNguyenLieuNavigation.GiaBan,
                ThanhTien = ct.SoLuongCanDung * servings * ct.MaNguyenLieuNavigation.GiaBan
            }).ToList();

            var result = new CalculateIngredientsDto
            {
                MaCongThuc = congThuc.MaCongThuc,
                TenMonAn = congThuc.MaCtNavigation.MaMonAnNavigation.TenMonAn,
                TenPhienBan = congThuc.MaPhienBanNavigation.TenPhienBan,
                SoSuatAn = servings,
                Ingredients = calculatedIngredients,
                TongChiPhi = calculatedIngredients.Sum(i => i.ThanhTien)
            };

            return Ok(result);
        }
    }
}
