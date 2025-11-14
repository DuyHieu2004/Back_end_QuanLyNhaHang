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

        public DatBanAPIController(QLNhaHangContext  context)
        {
            _context = context;
        }

        [HttpPost("TaoDatBan")]
        public async Task<IActionResult> CreateDatBan([FromBody] DatBanDTO datBanDto)
        {
            // 1. Validate dữ liệu gửi lên
            if (!ModelState.IsValid) return BadRequest(ModelState);

            try
            {
                // =================================================================
                // BƯỚC 1: KIỂM TRA BÀN & LỊCH
                // =================================================================
                var banAn = await _context.BanAns.FindAsync(datBanDto.MaBan);
                if (banAn == null) return NotFound(new { message = "Không tìm thấy bàn." });

                // Check sức chứa
                if (datBanDto.SoLuongNguoi > banAn.SucChua)
                {
                    return BadRequest(new { message = $"Số lượng người ({datBanDto.SoLuongNguoi}) vượt quá sức chứa tối đa ({banAn.SucChua})." });
                }

                // Check trùng lịch
                var thoiGianBatDau = datBanDto.ThoiGianDatHang;
                var thoiGianKetThuc = thoiGianBatDau.AddMinutes(120);

                var bookingConflict = await _context.DonHangs
                    .AnyAsync(dh =>
                        dh.MaBan == datBanDto.MaBan &&
                        // Chỉ check đơn đang hoạt động
                        (dh.MaTrangThaiDonHang == "CHO_XAC_NHAN" || dh.MaTrangThaiDonHang == "DA_XAC_NHAN" || dh.MaTrangThaiDonHang == "CHO_THANH_TOAN") &&
                        dh.ThoiGianDatHang != null &&
                        (thoiGianBatDau < dh.ThoiGianDatHang.Value.AddMinutes(120)) &&
                        (thoiGianKetThuc > dh.ThoiGianDatHang.Value)
                    );

                if (bookingConflict)
                {
                    return BadRequest(new { message = $"Bàn này đã bị đặt trong khung giờ {thoiGianBatDau:HH:mm} - {thoiGianKetThuc:HH:mm}." });
                }

                // =================================================================
                // BƯỚC 2: XÁC ĐỊNH KHÁCH HÀNG (LOGIC QUÉT KÉP & HỢP NHẤT)
                // =================================================================
                string maKhachHangCuoiCung = "";
                KhachHang? khachHang = null;
                bool laDatHo = false;

                // --- CHIẾN THUẬT 1: KIỂM TRA USER ĐANG ĐĂNG NHẬP ---
                if (!string.IsNullOrEmpty(datBanDto.MaKhachHang))
                {
                    var khachDangNhap = await _context.KhachHangs.FindAsync(datBanDto.MaKhachHang);

                    if (khachDangNhap != null)
                    {
                        // Chuẩn hóa chuỗi để so sánh
                        string sdtTrongDb = khachDangNhap.SoDienThoai?.Trim() ?? "";
                        string sdtNhapVao = datBanDto.SoDienThoaiKhach?.Trim() ?? "";

                        // LOGIC QUYẾT ĐỊNH CHÍNH CHỦ:
                        // 1. SĐT trùng nhau.
                        // 2. HOẶC Trong DB chưa có SĐT (do đăng ký bằng Email).
                        bool laChinhChu = (sdtTrongDb == sdtNhapVao) || string.IsNullOrEmpty(sdtTrongDb);

                        if (laChinhChu)
                        {
                            // 1.1 CHÍNH CHỦ -> Dùng tài khoản này
                            khachHang = khachDangNhap;
                            bool canUpdate = false;

                            // Cập nhật SĐT nếu trong DB đang thiếu
                            if (string.IsNullOrEmpty(sdtTrongDb) && !string.IsNullOrEmpty(sdtNhapVao))
                            {
                                // Check xem SĐT này có bị trùng với ai khác không (Tránh lỗi Unique)
                                var sdtDaTonTai = await _context.KhachHangs.AnyAsync(k => k.SoDienThoai == sdtNhapVao && k.MaKhachHang != khachHang.MaKhachHang);
                                if (!sdtDaTonTai)
                                {
                                    khachHang.SoDienThoai = sdtNhapVao;
                                    canUpdate = true;
                                }
                                else
                                {
                                    // Nếu SĐT đã thuộc về người khác -> Coi như Đặt Hộ -> Nhảy xuống tìm kiếm
                                    khachHang = null;
                                    laDatHo = true;
                                    goto SkipLoginCheck;
                                }
                            }

                            // Cập nhật Email nếu trong DB đang thiếu
                            if (!string.IsNullOrEmpty(datBanDto.Email) && string.IsNullOrEmpty(khachHang.Email))
                            {
                                khachHang.Email = datBanDto.Email;
                                canUpdate = true;
                            }

                            if (canUpdate)
                            {
                                _context.KhachHangs.Update(khachHang);
                                await _context.SaveChangesAsync();
                            }
                        }
                        else
                        {
                            // 1.2 KHÁC SĐT -> ĐẶT HỘ -> Bỏ qua ID đăng nhập, đi tìm theo SĐT
                            laDatHo = true;
                        }
                    }
                }

            SkipLoginCheck:; // Label nhảy tới nếu bị trùng SĐT

                // --- CHIẾN THUẬT 2: TÌM THEO SỐ ĐIỆN THOẠI (Nếu chưa tìm ra ở trên) ---
                if (khachHang == null)
                {
                    khachHang = await _context.KhachHangs
                        .FirstOrDefaultAsync(k => k.SoDienThoai == datBanDto.SoDienThoaiKhach);
                }

                // --- CHIẾN THUẬT 3: TÌM THEO EMAIL (Quét kép - Nếu có nhập mail) ---
                if (khachHang == null && !string.IsNullOrEmpty(datBanDto.Email))
                {
                    khachHang = await _context.KhachHangs
                        .FirstOrDefaultAsync(k => k.Email == datBanDto.Email);
                }

                // =================================================================
                // CHỐT HẠ KHÁCH HÀNG
                // =================================================================
                if (khachHang != null)
                {
                    // TÌM THẤY -> Dùng ID cũ
                    maKhachHangCuoiCung = khachHang.MaKhachHang;

                    // Logic Merge thông tin cho khách cũ (nếu thiếu)
                    bool canUpdate = false;

                    // Bổ sung SĐT nếu thiếu
                    if (string.IsNullOrEmpty(khachHang.SoDienThoai) && !string.IsNullOrEmpty(datBanDto.SoDienThoaiKhach))
                    {
                        khachHang.SoDienThoai = datBanDto.SoDienThoaiKhach;
                        canUpdate = true;
                    }
                    // Bổ sung Email nếu thiếu
                    if (string.IsNullOrEmpty(khachHang.Email) && !string.IsNullOrEmpty(datBanDto.Email))
                    {
                        khachHang.Email = datBanDto.Email;
                        canUpdate = true;
                    }

                    if (canUpdate)
                    {
                        _context.KhachHangs.Update(khachHang);
                        await _context.SaveChangesAsync();
                    }
                }
                else
                {
                    // KHÔNG TÌM THẤY AI -> TẠO MỚI (Khách vãng lai hoặc người được đặt hộ)
                    var newKhach = new KhachHang
                    {
                        MaKhachHang = "KH" + DateTime.Now.ToString("yyMMddHHmmss"),
                        HoTen = datBanDto.HoTenKhach,
                        SoDienThoai = datBanDto.SoDienThoaiKhach,
                        // Xử lý Email: Rỗng thì cho NULL để tránh lỗi Unique
                        Email = !string.IsNullOrEmpty(datBanDto.Email) ? datBanDto.Email : null,
                        NoShowCount = 0
                    };

                    _context.KhachHangs.Add(newKhach);
                    await _context.SaveChangesAsync();

                    khachHang = newKhach;
                    maKhachHangCuoiCung = newKhach.MaKhachHang;
                }

                // =================================================================
                // BƯỚC 3: LOGIC TÍNH TIỀN CỌC
                // =================================================================
                decimal tienCocYeuCau = 0;
                bool canThanhToanOnline = false;

                // Điều kiện 1: Đặt đông người (>= 6 người)
                bool isDongNguoi = datBanDto.SoLuongNguoi >= 6;

                // Điều kiện 2: Khách hay bùng kèo (NoShow > 3)
                bool isHayBungKeo = (khachHang.NoShowCount ?? 0) > 3;

                if (isDongNguoi || isHayBungKeo)
                {
                    // Quy định: Cọc cứng 200k (hoặc bạn tính % tùy ý)
                    tienCocYeuCau = 200000;
                    canThanhToanOnline = true;
                }

                // Xác định trạng thái: Cần cọc -> Chờ TT, Không cọc -> Tự động duyệt (DA_XAC_NHAN)
                string trangThaiBanDau = canThanhToanOnline ? "CHO_THANH_TOAN" : "DA_XAC_NHAN";

                // =================================================================
                // BƯỚC 4: TẠO ĐƠN HÀNG
                // =================================================================
                var newDonHang = new DonHang
                {
                    MaDonHang = "DH" + DateTime.Now.ToString("yyMMddHHmmss"),
                    MaBan = datBanDto.MaBan,
                    MaKhachHang = maKhachHangCuoiCung,

                    TenNguoiDat = datBanDto.HoTenKhach,       // Lưu tên người đi ăn (ví dụ: Bố bạn)
                    SDTNguoiDat = datBanDto.SoDienThoaiKhach,
                    EmailNguoiDat = datBanDto.Email,

                    MaNhanVien = datBanDto.MaNhanVien,
                    MaTrangThaiDonHang = trangThaiBanDau,

                    

                    ThoiGianDatHang = DateTime.Now,
                    ThoiGianBatDau = datBanDto.ThoiGianDatHang,
                    ThoiGianKetThuc = datBanDto.ThoiGianDatHang.AddMinutes(120),

                    ThoiGianCho = 60,
                    SoLuongNguoi = datBanDto.SoLuongNguoi,
                    GhiChu = datBanDto.GhiChu,

                    TienDatCoc = tienCocYeuCau
                };

                _context.DonHangs.Add(newDonHang);
                await _context.SaveChangesAsync();

                // =================================================================
                // BƯỚC 5: XỬ LÝ KẾT QUẢ (THANH TOÁN / GỬI EMAIL)
                // =================================================================
                string paymentUrl = "";
                string messageRes = "Đặt bàn thành công! Đã gửi vé qua email.";

                if (canThanhToanOnline)
                {
                    // Giả lập link thanh toán VNPAY
                    paymentUrl = $"https://sandbox.vnpayment.vn/paymentv2/vpcpay.html?token={newDonHang.MaDonHang}";
                    messageRes = "Đơn hàng cần đặt cọc. Vui lòng thanh toán.";
                }
                else
                {
                    // Nếu không cần cọc -> Gửi email xác nhận ngay
                    // Ưu tiên gửi về Email khách nhập trong Form (để nhận vé ngay)
                    string emailNhanVe = !string.IsNullOrEmpty(datBanDto.Email)
                                         ? datBanDto.Email
                                         : khachHang?.Email;

                    if (!string.IsNullOrEmpty(emailNhanVe))
                    {
                        try
                        {
                            var emailService = HttpContext.RequestServices.GetRequiredService<Services.IEmailService>();

                            _ = emailService.SendBookingConfirmationEmailAsync(
                                emailNhanVe, // Gửi về email ưu tiên
                                khachHang?.HoTen ?? datBanDto.HoTenKhach,
                                newDonHang.MaDonHang,
                                banAn.TenBan,
                                newDonHang.ThoiGianBatDau ?? DateTime.Now,
                                newDonHang.SoLuongNguoi,
                                newDonHang.GhiChu
                            );
                        }
                        catch { /* Log lỗi email */ }
                    }
                }

                return Ok(new
                {
                    success = true,
                    message = messageRes,

                    requirePayment = canThanhToanOnline,
                    paymentUrl = paymentUrl,
                    depositAmount = tienCocYeuCau,

                    donHang = newDonHang
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return StatusCode(500, new { message = "Lỗi máy chủ: " + ex.Message });
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
