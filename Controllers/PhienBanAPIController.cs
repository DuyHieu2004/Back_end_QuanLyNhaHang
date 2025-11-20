using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyNhaHang.Models;
using QuanLyNhaHang.Models.DTO;

namespace QuanLyNhaHang.Controllers;

[Route("api/[controller]")]
[ApiController]
public class PhienBanAPIController : ControllerBase
{
    private readonly QLNhaHangContext _context;

    public PhienBanAPIController(QLNhaHangContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<PhienBanLookupDto>>> GetPhienBans()
    {
        var data = await _context.PhienBanMonAns
            .OrderBy(pb => pb.ThuTu ?? int.MaxValue)
            .Select(pb => new PhienBanLookupDto
            {
                MaPhienBan = pb.MaPhienBan,
                TenPhienBan = pb.TenPhienBan,
                MaTrangThai = pb.MaTrangThai,
                ThuTu = pb.ThuTu
            })
            .ToListAsync();

        return Ok(data);
    }
}

