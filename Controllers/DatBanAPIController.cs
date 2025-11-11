using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using QuanLyNhaHang.Models;
using QuanLyNhaHang.Models.DTO;

namespace QuanLyNhaHang.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DatBanAPIController : ControllerBase
    {
        private readonly QLNhaHangContext _context;

        public DatBanAPIController(QLNhaHangContext context)
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
                    MaTrangThaiDonHang = "CHO_XAC_NHAN",


                    ThoiGianDatHang = datBanDto.ThoiGianDatHang,
                    ThoiGianCho = 60,
                    SoLuongNguoi = datBanDto.SoLuongNguoi,
                    GhiChu = datBanDto.GhiChu,
                    TienDatCoc = datBanDto.TienDatCoc ?? 0,


                    ThoiGianBatDau = null,
                    ThoiGianKetThuc = null,
                };

                _context.DonHangs.Add(newDonHang);
                await _context.SaveChangesAsync();

                // Gửi email xác nhận nếu khách hàng có email
                if (!string.IsNullOrEmpty(khachHang.Email))
                {
                    try
                    {
                        var emailService = HttpContext.RequestServices.GetRequiredService<Services.IEmailService>();
                        await emailService.SendBookingConfirmationEmailAsync(
                            khachHang.Email,
                            khachHang.HoTen,
                            newDonHang.MaDonHang,
                            banAn.TenBan,
                            newDonHang.ThoiGianDatHang ?? DateTime.Now,
                            newDonHang.SoLuongNguoi,
                            newDonHang.GhiChu
                        );
                    }
                    catch (Exception emailEx)
                    {
                        // Log lỗi nhưng không fail request
                        // Có thể log vào logger nếu có
                    }
                }

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

        [HttpPut("CapNhatTrangThai/{maDonHang}")]
        public async Task<IActionResult> CapNhatTrangThai(string maDonHang, [FromBody] string maTrangThai)
        {
            if (string.IsNullOrWhiteSpace(maTrangThai))
            {
                return BadRequest(new { message = "Mã trạng thái không hợp lệ." });
            }

            var donHang = await _context.DonHangs.FindAsync(maDonHang);
            if (donHang == null)
            {
                return NotFound(new { message = "Không tìm thấy đơn hàng." });
            }

            var exists = await _context.TrangThaiDonHangs.AnyAsync(t => t.MaTrangThai == maTrangThai);
            if (!exists)
            {
                return BadRequest(new { message = "Trạng thái không tồn tại." });
            }

            donHang.MaTrangThaiDonHang = maTrangThai;

            // Nếu hoàn thành mà chưa có ThoiGianBatDau/KetThuc thì set nhanh theo khung 2h
            if (maTrangThai == "DA_HOAN_THANH")
            {
                donHang.ThoiGianBatDau ??= donHang.ThoiGianDatHang;
                donHang.ThoiGianKetThuc ??= (donHang.ThoiGianBatDau ?? DateTime.Now).AddMinutes(120);
            }

            _context.DonHangs.Update(donHang);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Cập nhật trạng thái thành công.", maDonHang, maTrangThai });
        }
    }
}
