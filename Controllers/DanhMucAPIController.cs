using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyNhaHang.Models;

[Route("api/[controller]")]
[ApiController]
public class DanhMucAPIController : ControllerBase
{
    private readonly QLNhaHangContext _context;

    public DanhMucAPIController(QLNhaHangContext context)
    {
        _context = context;
    }


    [HttpGet]
    public async Task<ActionResult<IEnumerable<DanhMucMonAn>>> GetDanhMucMonAns()
    {
        return await _context.DanhMucMonAns.ToListAsync();
    }
}