
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyNhaHang.Models;
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
                .Select(c => new
                {
                    c.MaCongThuc,
                    c.MaCt,
                    c.MaPhienBan,
                    TenMonAn = c.MaCtNavigation.MaMonAnNavigation.TenMonAn,
                    TenPhienBan = c.MaPhienBanNavigation.TenPhienBan,
                    c.Gia
                })
                .ToListAsync();

            return Ok(congThucs);
        }
    }
}