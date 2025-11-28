using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyNhaHang.Models;

namespace QuanLyNhaHang.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LoaiMenuAPIController : ControllerBase
    {
        private readonly QLNhaHangContext _context;

        public LoaiMenuAPIController(QLNhaHangContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Lấy danh sách tất cả loại menu
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetLoaiMenus()
        {
            try
            {
                var loaiMenus = await _context.LoaiMenus
                    .OrderBy(lm => lm.MaLoaiMenu)
                    .ToListAsync();

                var result = loaiMenus.Select(lm => new
                {
                    MaLoaiMenu = lm.MaLoaiMenu,
                    TenLoaiMenu = lm.TenLoaiMenu,
                    MoTa = lm.MoTa
                }).ToList();

                return Ok(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi máy chủ: " + ex.Message });
            }
        }

        /// <summary>
        /// Lấy chi tiết loại menu theo mã
        /// </summary>
        [HttpGet("{maLoaiMenu}")]
        public async Task<ActionResult<object>> GetLoaiMenu(string maLoaiMenu)
        {
            try
            {
                var loaiMenu = await _context.LoaiMenus
                    .FirstOrDefaultAsync(lm => lm.MaLoaiMenu == maLoaiMenu);

                if (loaiMenu == null)
                {
                    return NotFound(new { message = "Không tìm thấy loại menu." });
                }

                var result = new
                {
                    MaLoaiMenu = loaiMenu.MaLoaiMenu,
                    TenLoaiMenu = loaiMenu.TenLoaiMenu,
                    MoTa = loaiMenu.MoTa
                };

                return Ok(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi máy chủ: " + ex.Message });
            }
        }
    }
}

