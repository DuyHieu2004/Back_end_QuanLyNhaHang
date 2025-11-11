using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyNhaHang.Models;
using QuanLyNhaHang.Models.DTO;

namespace QuanLyNhaHang.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DatBanAPIController : ControllerBase
    {
        private readonly QLNhaHangContext _context;

        public DatBanAPIController(QLNhaHangContext  context)
        {
            _context = context;
        }

        [HttpPost("TaoDatBan")]
        public async Task<IActionResult> CreateDatBan([FromBody] DatBanDTO datBanDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {

                var banAn = await _context.BanAns.FindAsync(datBanDto.MaBan);
                if (banAn == null)
                {
                    return NotFound(new { message = "Không tìm thấy bàn." });
                }

                if (datBanDto.SoLuongNguoi > banAn.SucChua)
                {
                    return BadRequest(new { message = $"Số lượng người ({datBanDto.SoLuongNguoi}) vượt quá sức chứa của bàn ({banAn.SucChua})." });
                }


                var thoiGianBatDauDat = datBanDto.ThoiGianDatHang;
                var thoiGianKetThucDat = thoiGianBatDauDat.AddMinutes(120);

                var bookingConflict = await _context.DonHangs
                    .AnyAsync(dh =>
                        dh.MaBan == datBanDto.MaBan &&
                        dh.ThoiGianKetThuc == null &&
                        dh.ThoiGianDatHang != null &&
                        (thoiGianBatDauDat < dh.ThoiGianDatHang.Value.AddMinutes(120)) &&
                        (thoiGianKetThucDat > dh.ThoiGianDatHang.Value)
                    );

                if (bookingConflict)
                {
                    return BadRequest(new { message = $"Bàn này đã bị trùng lịch đặt trong khung giờ này." });
                }

                var khachHang = await _context.KhachHangs
                    .FirstOrDefaultAsync(kh => kh.SoDienThoai == datBanDto.SoDienThoaiKhach);

                if (khachHang == null)
                {
                    string newMaKhachHang = "KH" + DateTime.Now.ToString("yyMMddHHmmss");
                    khachHang = new KhachHang
                    {
                        MaKhachHang = newMaKhachHang,
                        HoTen = datBanDto.HoTenKhach,
                        SoDienThoai = datBanDto.SoDienThoaiKhach
                    };
                    _context.KhachHangs.Add(khachHang);
                    await _context.SaveChangesAsync();
                }


                string newMaDonHang = "DH" + DateTime.Now.ToString("yyMMddHHmmss");
                DonHang newDonHang = new DonHang
                {
                    MaDonHang = newMaDonHang,
                    MaBan = datBanDto.MaBan,
                    MaKhachHang = khachHang.MaKhachHang,
                    MaNhanVien = datBanDto.MaNhanVien,


                    ThoiGianDatHang = datBanDto.ThoiGianDatHang,
                    ThoiGianCho = 60,
                    SoLuongNguoi = datBanDto.SoLuongNguoi,
                    GhiChu = datBanDto.GhiChu,


                    ThoiGianBatDau = null,
                    ThoiGianKetThuc = null,
                };

                _context.DonHangs.Add(newDonHang);
                await _context.SaveChangesAsync();


                return Ok(new { message = "Đặt bàn thành công!", donHang = newDonHang });
            }
            catch (Exception ex)
            {
                string errorMessage = ex.Message;
                if (ex.InnerException != null)
                {
                    errorMessage = ex.InnerException.Message;
                }

                return StatusCode(500, new { message = "Lỗi máy chủ: " + errorMessage });
            }
        }
    }
}
