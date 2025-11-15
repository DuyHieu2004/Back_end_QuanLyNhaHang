using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyNhaHang.Models;

namespace QuanLyNhaHang.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SuppliersAPIController : ControllerBase
    {
        private readonly QLNhaHangContext _context;

        public SuppliersAPIController(QLNhaHangContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetSuppliers()
        {
            var suppliers = await _context.NhaCungCaps
                .Select(s => new
                {
                    s.MaNhaCungCap,
                    s.TenNhaCungCap,
                    s.SoDienThoai,
                    s.DiaChi
                })
                .OrderBy(s => s.TenNhaCungCap)
                .ToListAsync();

            return Ok(suppliers);
        }
    }
}

