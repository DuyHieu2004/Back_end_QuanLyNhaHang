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
                .Include(b => b.MaTangNavigation)
                .Where(b => b.IsShow == true)
                .Select(b => new BanAnDTO
                {
                    maBan = b.MaBan,
                    tenBan = b.TenBan,
                    maTrangThai = b.MaTrangThai,
                    tenTrangThai = b.MaTrangThaiNavigation.TenTrangThai,
                    sucChua = b.SucChua,
                    maTang = b.MaTang,
                    tenTang = b.MaTangNavigation != null ? b.MaTangNavigation.TenTang : null,
                    isShow = b.IsShow
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

            var gioBatDau = dateTime;
            var gioKetThuc = dateTime.AddMinutes(120);


            var conflictingBookingIds = await _context.DonHangs
                .Include(dh => dh.MaBans)
                .Where(dh =>

                    (dh.MaTrangThaiDonHang == "CHO_XAC_NHAN" || dh.MaTrangThaiDonHang == "DA_XAC_NHAN" || dh.MaTrangThaiDonHang == "CHO_THANH_TOAN") &&

                    dh.ThoiGianDatHang != null &&
                    (gioBatDau < dh.ThoiGianKetThuc) &&
                    (gioKetThuc > dh.ThoiGianDatHang)
                )
                .SelectMany(dh => dh.MaBans)
                .Select(b=>b.MaBan)
                .Distinct()
                .ToListAsync();


            var allTables = await _context.BanAns
                .Include(b => b.MaTrangThaiNavigation)
                .Include(b => b.MaTangNavigation)
                .Where(b => b.IsShow == true)
                .ToListAsync();

            var allTangs = await _context.Tangs.ToDictionaryAsync(t => t.MaTang, t => t.TenTang);

            var result = allTables.Select(ban =>
            {
                string maTang = ban.MaTang;
                if (string.IsNullOrEmpty(maTang) && ban.MaBan.StartsWith("B"))
                {
                    if (int.TryParse(ban.MaBan.Substring(1), out int banNum))
                    {
                        maTang = banNum <= 14 ? "T001" : banNum <= 27 ? "T002" : "T003";
                    }
                }

                string tenTang = ban.MaTangNavigation?.TenTang;
                if (string.IsNullOrEmpty(tenTang) && !string.IsNullOrEmpty(maTang) && allTangs.ContainsKey(maTang))
                {
                    tenTang = allTangs[maTang];
                }

                bool isConflicting = conflictingBookingIds.Contains(ban.MaBan);

                return new BanAnDTO
                {
                    maBan = ban.MaBan,
                    tenBan = ban.TenBan,
                    sucChua = ban.SucChua,
                    maTang = maTang ?? string.Empty,
                    tenTang = tenTang ?? string.Empty,
                    tenTrangThai =
                        (ban.MaTrangThaiNavigation?.TenTrangThai?.ToLower() == "đang bảo trì") ? "Đang bảo trì" :
                        (ban.SucChua < soNguoi) ? "Không đủ sức chứa" :
                        (isConflicting) ? "Đã đặt" :
                        "Đang trống"
                };
            }).ToList();

            return Ok(result);
        }


        [HttpGet("GetAvailableBanAns")]
        public async Task<IActionResult> GetAvailableBanAns(
            [FromQuery] DateTime dateTime,
            [FromQuery] int soNguoi,
            [FromQuery] string? maKhachHang)
        {
            if (_context.BanAns == null || _context.DonHangs == null)
            {
                return NotFound("Cơ sở dữ liệu chưa sẵn sàng.");
            }

            var gioBatDauKhachChon = dateTime;
            var gioKetThucKhachChon = dateTime.AddMinutes(120);

            var conflictingOrders = await _context.DonHangs
                .Where(dh =>
                    (dh.MaTrangThaiDonHang == "CHO_XAC_NHAN" ||
                     dh.MaTrangThaiDonHang == "DA_XAC_NHAN" ||
                     dh.MaTrangThaiDonHang == "CHO_THANH_TOAN") &&

                    dh.ThoiGianDatHang != null &&
                    dh.ThoiGianKetThuc != null &&

                    (gioBatDauKhachChon < dh.ThoiGianKetThuc) &&
                    (gioKetThucKhachChon > dh.ThoiGianDatHang)
                )
                .SelectMany(
                        dh => dh.MaBans,
                        (dh, ban) => new 
                        {
                            MaBan = ban.MaBan,
                            MaKhachHang = dh.MaKhachHang
                        }
                    )
                .ToListAsync();

            var banNguoiKhacDatIds = conflictingOrders
                .Where(o => o.MaKhachHang != maKhachHang)
                .Select(o => o.MaBan).Distinct().ToList();

            var banCuaTuiIds = conflictingOrders
                .Where(o => o.MaKhachHang == maKhachHang)
                .Select(o => o.MaBan).Distinct().ToList();

            var allTables = await _context.BanAns
                .Include(b => b.MaTrangThaiNavigation)
                .Include(b => b.MaTangNavigation)
                .Where(b => b.IsShow == true)
                .ToListAsync();

            var result = allTables.Select(ban =>
            {
                string trangThaiHienThi = "Trong";

                if (ban.MaTrangThai == "TTBA004")
                {
                    trangThaiHienThi = "BaoTri";
                }
                else if (banCuaTuiIds.Contains(ban.MaBan))
                {
                    trangThaiHienThi = "CuaTui";
                }
                else if (banNguoiKhacDatIds.Contains(ban.MaBan))
                {
                    trangThaiHienThi = "DaDat";
                }
                else if (ban.SucChua < soNguoi)
                {
                    trangThaiHienThi = "CanGhep";
                }
                else
                {
                    trangThaiHienThi = "Trong";
                }

                return new BanAnDTO
                {
                    maBan = ban.MaBan,
                    tenBan = ban.TenBan,
                    sucChua = ban.SucChua,
                    maTang = ban.MaTang,
                    tenTang = ban.MaTangNavigation != null ? ban.MaTangNavigation.TenTang : null,
                    isShow = ban.IsShow,
                    tenTrangThai = trangThaiHienThi
                };
            }).ToList();

            return Ok(result);
        }
    }
}





