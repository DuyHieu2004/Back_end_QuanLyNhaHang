using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyNhaHang.Models;

namespace QuanLyNhaHang.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TangAPIController : ControllerBase
    {
        private readonly QLNhaHangContext _context;

        public TangAPIController(QLNhaHangContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Tang>>> GetTangs()
        {
            if (_context.Tangs == null)
            {
                return NotFound();
            }

            var tangs = await _context.Tangs
                .Include(t => t.BanAns)
                .ToListAsync();

            return Ok(tangs);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Tang>> GetTang(string id)
        {
            if (_context.Tangs == null)
            {
                return NotFound();
            }

            var tang = await _context.Tangs
                .Include(t => t.BanAns)
                .FirstOrDefaultAsync(t => t.MaTang == id);

            if (tang == null)
            {
                return NotFound();
            }

            return tang;
        }
    }
}

