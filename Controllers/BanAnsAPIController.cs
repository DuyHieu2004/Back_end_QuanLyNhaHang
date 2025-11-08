using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyNhaHang.Models;
using QuanLyNhaHang.Models.DTO;

namespace QuanLyNhaHang.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BanAnsAPIController : ControllerBase
    {
        private readonly QLNhaHangContext _context;

        public BanAnsAPIController(QLNhaHangContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<BanAnDTO>>> GetBanAns()
        {
            if (_context.BanAns == null)
            {
                return NotFound();
            }

            var banAns = await _context.BanAns
                .Include(b => b.MaTrangThaiNavigation)
                .Select(b => new BanAnDTO
                {
                    maBan = b.MaBan,
                    tenBan = b.TenBan,
                    maTrangThai = b.MaTrangThai,
                    tenTrangThai = b.MaTrangThaiNavigation.TenTrangThai,
                    sucChua = b.SucChua
                }
                ).ToListAsync();

            return Ok(banAns);
        }


        [HttpGet("GetStatusByTime")]
        public async Task<IActionResult> GetStatusByTime([FromQuery] DateTime dateTime, [FromQuery] int soNguoi)
        {
            if (_context.BanAns == null || _context.DonHangs == null)
            {
                return NotFound();
            }

            var thoiGianBatDauKhachChon = dateTime;
            var thoiGianKetThucKhachChon = dateTime.AddMinutes(120);


            var conflictingBookingIds = await _context.DonHangs
                .Where(dh =>
                    dh.ThoiGianKetThuc == null &&
                    dh.ThoiGianDatHang != null &&
                    (thoiGianBatDauKhachChon < dh.ThoiGianDatHang.Value.AddMinutes(120)) &&
                    (thoiGianKetThucKhachChon > dh.ThoiGianDatHang.Value)
                )
                .Select(dh => dh.MaBan)
                .Distinct()
                .ToListAsync();


            var allTables = await _context.BanAns
                .Include(b => b.MaTrangThaiNavigation)
                .Select(ban => new
                {
                    ban.MaBan,
                    ban.TenBan,
                    ban.SucChua,
                    TrangThaiGoc = ban.MaTrangThaiNavigation.TenTrangThai,

                    IsConflicting = conflictingBookingIds.Contains(ban.MaBan)
                })
                .ToListAsync();


            var result = allTables.Select(ban => new BanAnDTO
            {
                maBan = ban.MaBan,
                tenBan = ban.TenBan,
                sucChua = ban.SucChua,


                tenTrangThai =
                    (ban.TrangThaiGoc.ToLower() == "đang bảo trì") ? "Đang bảo trì" :
                    (ban.SucChua < soNguoi) ? "Không đủ sức chứa" :
                    (ban.IsConflicting) ? "Đã đặt" :
                    "Đang trống"
            }).ToList();

            return Ok(result);
        }
    }


}
