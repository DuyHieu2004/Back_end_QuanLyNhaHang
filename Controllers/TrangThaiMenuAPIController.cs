using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyNhaHang.Models;

namespace QuanLyNhaHang.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TrangThaiMenuAPIController : ControllerBase
    {
        private readonly QLNhaHangContext _context;

        public TrangThaiMenuAPIController(QLNhaHangContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Lấy danh sách tất cả trạng thái menu
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetTrangThaiMenus()
        {
            try
            {
                var trangThaiMenus = await _context.TrangThaiMenus
                    .OrderBy(ttm => ttm.MaTrangThai)
                    .ToListAsync();

                var result = trangThaiMenus.Select(ttm => new
                {
                    MaTrangThai = ttm.MaTrangThai,
                    TenTrangThai = ttm.TenTrangThai
                }).ToList();

                return Ok(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi máy chủ: " + ex.Message });
            }
        }

        /// <summary>
        /// Lấy chi tiết trạng thái menu theo mã
        /// </summary>
        [HttpGet("{maTrangThai}")]
        public async Task<ActionResult<object>> GetTrangThaiMenu(string maTrangThai)
        {
            try
            {
                var trangThaiMenu = await _context.TrangThaiMenus
                    .FirstOrDefaultAsync(ttm => ttm.MaTrangThai == maTrangThai);

                if (trangThaiMenu == null)
                {
                    return NotFound(new { message = "Không tìm thấy trạng thái menu." });
                }

                var result = new
                {
                    MaTrangThai = trangThaiMenu.MaTrangThai,
                    TenTrangThai = trangThaiMenu.TenTrangThai
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

