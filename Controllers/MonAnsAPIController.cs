using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyNhaHang.Models;

[Route("api/[controller]")]
[ApiController]
public class MonAnsAPIController : ControllerBase
{
    private readonly QLNhaHangContext _context;

    public MonAnsAPIController(QLNhaHangContext context)
    {
        _context = context;
    }


    [HttpGet]
    public async Task<ActionResult<IEnumerable<MonAn>>> GetMonAns([FromQuery] string? maDanhMuc, [FromQuery] string? searchString)
    {

        var query = _context.MonAns
                           .Include(m => m.HinhAnhMonAns)
                            .Include(m => m.MaDanhMucNavigation)
                            .Include(m => m.PhienBanMonAns)
                            .AsSplitQuery()
                            .AsQueryable();


        if (!string.IsNullOrEmpty(maDanhMuc))
        {
            query = query.Where(m => m.MaDanhMuc == maDanhMuc);
        }


        if (!string.IsNullOrEmpty(searchString))
        {
            query = query.Where(m => m.TenMonAn.Contains(searchString));
        }


        var monAns = await query.ToListAsync();
        return Ok(monAns);
    }


    [HttpGet("{id}")]
    public async Task<ActionResult<MonAn>> GetMonAn(string id)
    {
        var monAn = await _context.MonAns
                                  .Include(m => m.HinhAnhMonAns)
                                  .Include(m => m.MaDanhMucNavigation)
                                  .Include(m => m.PhienBanMonAns)
                                  .AsSplitQuery()
                                  .FirstOrDefaultAsync(m => m.MaMonAn == id);

        if (monAn == null)
        {
            return NotFound();
        }

        return monAn;
    }
}

