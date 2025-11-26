using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using QuanLyNhaHang.Models;
using QuanLyNhaHang.Models.DTO;
using QuanLyNhaHang.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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


        public class StaffLoginRequest
        {
            public string Username { get; set; }
            public string Password { get; set; }
        }

        // [Authorize(Roles = "Staff, Admin")] 
        [HttpPost("staff/create")]
        public async Task<IActionResult> TaoDatBanChoNhanVien([FromBody] DatBanDto donHangDto)
        {
            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    // BƯỚC 2: GIẢ LẬP NHÂN VIÊN ĐANG THAO TÁC
                    // Khi chưa có Token, biến User sẽ rỗng. Ta phải gán cứng mã nhân viên để test.
                    // SAU NÀY CÓ LOGIN: Comment dòng dưới, mở comment dòng User.FindFirst...

                   // string maNhanVienHienTai = "NV001"; // <--- Gán cứng mã của bạn để test (Đảm bảo NV001 có trong DB)
                     string maNhanVienHienTai = User.FindFirst("MaNhanVien")?.Value; // <--- Code chuẩn sau này

                    if (string.IsNullOrEmpty(maNhanVienHienTai))
                    {
                        return Unauthorized("Không xác định được nhân viên thực hiện.");
                    }

                    // -----------------------------------------------------------
                    // LOGIC 1: XỬ LÝ THÔNG TIN KHÁCH HÀNG (Nhập tay từ Web)
                    // -----------------------------------------------------------
                    KhachHang khachHang = null;

                    // Tìm xem SĐT này có chưa
                    var khachCu = await _context.KhachHangs.FirstOrDefaultAsync(k => k.SoDienThoai == donHangDto.SoDienThoaiKhach);

                    if (khachCu != null)
                    {
                        // Case A: Khách cũ -> Dùng lại
                        khachHang = khachCu;
                    }
                    else
                    {
                        // Case B: Khách mới hoặc Vãng lai
                        if (string.IsNullOrEmpty(donHangDto.SoDienThoaiKhach))
                        {
                            // Không nhập SĐT -> Gán khách vãng lai
                            khachHang = await _context.KhachHangs.FindAsync("KH_VANG_LAI");
                        }
                        else
                        {
                            // Có SĐT nhưng chưa có trong DB -> Tạo khách mới ngay lập tức
                            khachHang = new KhachHang
                            {
                                // Sinh mã KH tự động (Ví dụ: KH + Ticks)
                                MaKhachHang = "KH" + DateTime.Now.Ticks.ToString().Substring(12),
                                HoTen = donHangDto.HoTenKhach,
                                SoDienThoai = donHangDto.SoDienThoaiKhach,
                                Email = donHangDto.Email,
                                SoLanAnTichLuy = 0,
                                NgayTao = DateTime.Now,
                                NoShowCount = 0
                            };
                            _context.KhachHangs.Add(khachHang);
                            await _context.SaveChangesAsync(); // Lưu ngay để lấy ID dùng bên dưới
                        }
                    }

                    // -----------------------------------------------------------
                    // LOGIC 2: TẠO ĐƠN HÀNG
                    // -----------------------------------------------------------
                    var donHang = new DonHang
                    {
                        MaDonHang = "DH" + DateTime.Now.ToString("yyyyMMddHHmmss"),
                        MaNhanVien = maNhanVienHienTai, // Lưu vết nhân viên nào tạo đơn
                        MaKhachHang = khachHang.MaKhachHang,

                        // Xử lý ngày giờ (Parse từ chuỗi React gửi lên)
                        ThoiGianDatHang = DateTime.Now,
                        //TGNhanBan = DateTime.Parse(donHangDto.ThoiGianDatHang),
                        TGNhanBan = donHangDto.ThoiGianDatHang,
                        MaTrangThaiDonHang = "CHO_XAC_NHAN", // Hoặc DA_XAC_NHAN tùy quy trình
                        SoLuongNguoiDK = donHangDto.SoLuongNguoi,
                        GhiChu = "Đặt tại quầy/Web Admin",
                        ThanhToan = false
                    };

                    // -----------------------------------------------------------
                    // LOGIC 3: TỰ ĐỘNG TÍNH KHUYẾN MÃI TÍCH LŨY (Ăn 10 lần giảm 10%)
                    // -----------------------------------------------------------
                    bool duocGiamGia = false;

                    // Chỉ áp dụng nếu không phải khách vãng lai
                    if (khachHang.MaKhachHang != "KH_VANG_LAI" && khachHang.SoLanAnTichLuy >= 10)
                    {
                        donHang.MaKhuyenMai = "KM_TICHLUY_VIP"; // Gán mã giảm giá
                        duocGiamGia = true;
                    }

                    // -----------------------------------------------------------
                    // LOGIC 4: LƯU BÀN ĂN (Quan hệ nhiều nhiều)
                    // -----------------------------------------------------------
                    if (donHangDto.DanhSachMaBan != null && donHangDto.DanhSachMaBan.Count > 0)
                    {
                        foreach (var maBan in donHangDto.DanhSachMaBan)
                        {
                            var banAnDonHang = new BanAnDonHang
                            {
                                MaBanAnDonHang = Guid.NewGuid().ToString().Substring(0, 8),
                                MaDonHang = donHang.MaDonHang,
                                MaBan = maBan
                            };
                            _context.BanAnDonHangs.Add(banAnDonHang);

                            // Cập nhật trạng thái bàn thành "ĐÃ ĐẶT" (TTBA003)
                            var ban = await _context.BanAns.FindAsync(maBan);
                            if (ban != null) ban.MaTrangThai = "TTBA003";
                        }
                    }

                    _context.DonHangs.Add(donHang);
                    await _context.SaveChangesAsync();

                    transaction.Commit();

                    return Ok(new
                    {
                        Success = true,
                        Message = "Tạo đặt bàn thành công!",
                        MaDonHang = donHang.MaDonHang,
                        KhuyenMai = duocGiamGia ? "Đã áp dụng giảm giá khách quen (10%)" : "Không có"
                    });
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    return StatusCode(500, new { Message = "Lỗi Server: " + ex.Message });
                }
            }
        }



        [HttpPost("TaoDatBan")]
        public async Task<IActionResult> CreateDatBan([FromBody] DatBanDto datBanDto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            try
            {
                // =================================================================
                // BƯỚC 1: KIỂM TRA DANH SÁCH BÀN & SỨC CHỨA
                // =================================================================

                var listBanAn = await _context.BanAns
                                            .Where(b => datBanDto.DanhSachMaBan.Contains(b.MaBan))
                                            .ToListAsync();

                if (listBanAn.Count != datBanDto.DanhSachMaBan.Count)
                {
                    return NotFound(new { message = "Một số mã bàn không tồn tại." });
                }

                int tongSucChua = listBanAn.Sum(b => b.SucChua);
                if (datBanDto.SoLuongNguoi > tongSucChua)
                {
                    return BadRequest(new { message = $"Số người ({datBanDto.SoLuongNguoi}) vượt quá tổng sức chứa ({tongSucChua})." });
                }

                // =================================================================
                // BƯỚC 2: KIỂM TRA TRÙNG LỊCH (Đã sửa query qua BanAnDonHangs)
                // =================================================================
                var thoiGianBatDauDuKien = datBanDto.ThoiGianDatHang;
                var thoiGianKetThucDuKien = thoiGianBatDauDuKien.AddMinutes(120);

                var bookingConflict = await _context.DonHangs
                    .Include(d => d.BanAnDonHangs)
                    .AnyAsync(dh =>
                        // 1. Đơn hàng đang hoạt động
                        (dh.MaTrangThaiDonHang == "CHO_XAC_NHAN" ||
                         dh.MaTrangThaiDonHang == "DA_XAC_NHAN" ||
                         dh.MaTrangThaiDonHang == "CHO_THANH_TOAN") &&

                        dh.TGNhanBan != null &&

                        // 2. Check va chạm thời gian (dựa trên giờ nhận bàn)
                        (thoiGianBatDauDuKien < dh.TGNhanBan.Value.AddMinutes(120)) &&
                        (thoiGianKetThucDuKien > dh.TGNhanBan.Value) &&

                        // 3. Check xem có bàn nào trùng trong bảng trung gian không
                        dh.BanAnDonHangs.Any(b => datBanDto.DanhSachMaBan.Contains(b.MaBan))
                    );

                if (bookingConflict)
                {
                    return BadRequest(new { message = "Một trong các bàn đã chọn bị trùng lịch." });
                }

                // =================================================================
                // BƯỚC 3: XỬ LÝ KHÁCH HÀNG
                // =================================================================
                string maKhachHangCuoiCung = "";
                KhachHang? khachHang = null;

                // 3.1 Kiểm tra user nếu có gửi mã
                if (!string.IsNullOrEmpty(datBanDto.MaKhachHang))
                {
                    var khachDangNhap = await _context.KhachHangs.FindAsync(datBanDto.MaKhachHang);
                    if (khachDangNhap != null)
                    {
                        string sdtTrongDb = khachDangNhap.SoDienThoai?.Trim() ?? "";
                        string sdtNhapVao = datBanDto.SoDienThoaiKhach?.Trim() ?? "";

                        // Nếu SĐT khớp hoặc DB chưa có SĐT -> Chấp nhận là chính chủ
                        if ((sdtTrongDb == sdtNhapVao) || string.IsNullOrEmpty(sdtTrongDb))
                        {
                            khachHang = khachDangNhap;
                            bool changed = false;

                            if (string.IsNullOrEmpty(sdtTrongDb) && !string.IsNullOrEmpty(sdtNhapVao))
                            {
                                bool existSdt = await _context.KhachHangs.AnyAsync(k => k.SoDienThoai == sdtNhapVao && k.MaKhachHang != khachHang.MaKhachHang);
                                if (!existSdt) { khachHang.SoDienThoai = sdtNhapVao; changed = true; }
                            }
                            if (string.IsNullOrEmpty(khachHang.Email) && !string.IsNullOrEmpty(datBanDto.Email))
                            {
                                khachHang.Email = datBanDto.Email; changed = true;
                            }

                            if (changed)
                            {
                                _context.KhachHangs.Update(khachHang);
                                await _context.SaveChangesAsync();
                            }
                        }
                    }
                }

                // 3.2 Tìm theo SĐT (nếu chưa tìm thấy ở trên)
                if (khachHang == null)
                {
                    khachHang = await _context.KhachHangs.FirstOrDefaultAsync(k => k.SoDienThoai == datBanDto.SoDienThoaiKhach);
                }

                // 3.3 Tìm theo Email (nếu chưa tìm thấy)
                if (khachHang == null && !string.IsNullOrEmpty(datBanDto.Email))
                {
                    khachHang = await _context.KhachHangs.FirstOrDefaultAsync(k => k.Email == datBanDto.Email);
                }

                // 3.4 Chốt khách hàng (Tạo mới nếu chưa có)
                if (khachHang != null)
                {
                    maKhachHangCuoiCung = khachHang.MaKhachHang;
                    if (string.IsNullOrEmpty(khachHang.Email) && !string.IsNullOrEmpty(datBanDto.Email))
                    {
                        khachHang.Email = datBanDto.Email;
                        _context.KhachHangs.Update(khachHang);
                        await _context.SaveChangesAsync();
                    }
                }
                else
                {
                    var newKhach = new KhachHang
                    {
                        MaKhachHang = "KH" + DateTime.Now.ToString("yyMMddHHmmss"),
                        HoTen = datBanDto.HoTenKhach,
                        SoDienThoai = datBanDto.SoDienThoaiKhach,
                        Email = !string.IsNullOrEmpty(datBanDto.Email) ? datBanDto.Email : null,
                        NoShowCount = 0
                    };
                    _context.KhachHangs.Add(newKhach);
                    await _context.SaveChangesAsync();

                    khachHang = newKhach;
                    maKhachHangCuoiCung = newKhach.MaKhachHang;
                }

                // =================================================================
                // BƯỚC 4: TÍNH TIỀN CỌC
                // =================================================================
                decimal tienCocYeuCau = 0;
                bool canThanhToanOnline = false;

                bool isDongNguoi = datBanDto.SoLuongNguoi >= 6;
                bool isHayBungKeo = (khachHang?.NoShowCount ?? 0) > 3;

                if (isDongNguoi || isHayBungKeo)
                {
                    decimal donGiaCoc = 50000;
                    tienCocYeuCau = datBanDto.SoLuongNguoi * donGiaCoc;

                    if (tienCocYeuCau < 200000)
                    {
                        tienCocYeuCau = 200000;
                    }
                    canThanhToanOnline = true;
                }

                string trangThaiBanDau = "CHO_XAC_NHAN";

                // =================================================================
                // BƯỚC 5: TẠO ĐƠN HÀNG & BẢNG TRUNG GIAN (Đã sửa logic)
                // =================================================================

                string maDonHangMoi = "DH" + DateTime.Now.ToString("yyMMddHHmmss");

                var newDonHang = new DonHang
                {
                    MaDonHang = maDonHangMoi,

                    // Logic mới: Tạo danh sách BanAnDonHang thay vì gán listBanAn
                    BanAnDonHangs = listBanAn.Select(b => new BanAnDonHang
                    {
                        MaBanAnDonHang = $"BDH_{maDonHangMoi}_{b.MaBan}", // Tự sinh Key duy nhất
                        MaDonHang = maDonHangMoi,
                        MaBan = b.MaBan
                    }).ToList(),

                    MaKhachHang = maKhachHangCuoiCung,
                    TenNguoiNhan = datBanDto.HoTenKhach,
                    SdtnguoiNhan = datBanDto.SoDienThoaiKhach,
                    EmailNguoiNhan = datBanDto.Email,
                    MaNhanVien = datBanDto.MaNhanVien,

                    MaTrangThaiDonHang = trangThaiBanDau,

                    ThoiGianDatHang = DateTime.Now,
                    TGNhanBan = datBanDto.ThoiGianDatHang,
                    TgdatDuKien = datBanDto.ThoiGianDatHang,
                    ThoiGianKetThuc = datBanDto.ThoiGianDatHang.AddMinutes(120),

                    SoLuongNguoiDK = datBanDto.SoLuongNguoi,
                    GhiChu = datBanDto.GhiChu,
                    TienDatCoc = tienCocYeuCau,
                    ThanhToan = false
                };

                _context.DonHangs.Add(newDonHang);
                await _context.SaveChangesAsync();

                // =================================================================
                // BƯỚC 6: XỬ LÝ THANH TOÁN ONLINE & EMAIL
                // =================================================================
                string paymentUrl = "";
                string messageRes = "Đặt bàn thành công! Đã gửi vé qua email.";
                string tenCacBan = string.Join(", ", listBanAn.Select(b => b.TenBan));

                if (canThanhToanOnline)
                {
                    // Cấu hình VNPay Sandbox
                    string vnp_Url = "https://sandbox.vnpayment.vn/paymentv2/vpcpay.html";
                    string vnp_TmnCode = "FUAECD4Z";
                    string vnp_HashSecret = "AUEIM3PYKKST5ATLESXNJYPBEMTUHDKT";

                    VnPayLibrary vnpay = new VnPayLibrary();

                    vnpay.AddRequestData("vnp_Version", "2.1.0");
                    vnpay.AddRequestData("vnp_Command", "pay");
                    vnpay.AddRequestData("vnp_TmnCode", vnp_TmnCode);

                    long amount = (long)(tienCocYeuCau * 100);
                    vnpay.AddRequestData("vnp_Amount", amount.ToString());

                    vnpay.AddRequestData("vnp_CreateDate", DateTime.Now.ToString("yyyyMMddHHmmss"));
                    vnpay.AddRequestData("vnp_CurrCode", "VND");
                    vnpay.AddRequestData("vnp_IpAddr", "127.0.0.1");
                    vnpay.AddRequestData("vnp_Locale", "vn");
                    vnpay.AddRequestData("vnp_OrderInfo", "Dat coc don hang " + newDonHang.MaDonHang);
                    vnpay.AddRequestData("vnp_OrderType", "other");

                    var request = HttpContext.Request;
                    var baseUrl = $"{request.Scheme}://{request.Host}";
                    vnpay.AddRequestData("vnp_ReturnUrl", $"{baseUrl}/api/DatBanAPI/PaymentCallback");

                    vnpay.AddRequestData("vnp_TxnRef", newDonHang.MaDonHang);

                    paymentUrl = vnpay.CreateRequestUrl(vnp_Url, vnp_HashSecret);
                    messageRes = "Đơn hàng cần đặt cọc. Đang chuyển sang cổng thanh toán...";
                }
                else
                {
                    // Gửi Email xác nhận
                    string emailNhanVe = !string.IsNullOrEmpty(datBanDto.Email) ? datBanDto.Email : khachHang?.Email;
                    if (!string.IsNullOrEmpty(emailNhanVe))
                    {
                        try
                        {
                            // Resolve Service gửi mail (đảm bảo bạn đã đăng ký trong Program.cs)
                            var emailService = HttpContext.RequestServices.GetService<Services.IEmailService>();
                            if (emailService != null)
                            {
                                _ = emailService.SendBookingConfirmationEmailAsync(
                                    emailNhanVe,
                                    khachHang?.HoTen ?? datBanDto.HoTenKhach,
                                    newDonHang.MaDonHang,
                                    tenCacBan,
                                    newDonHang.TGNhanBan ?? DateTime.Now,
                                    newDonHang.SoLuongNguoiDK,
                                    newDonHang.GhiChu
                                );
                            }
                        }
                        catch
                        {
                            // Log lỗi gửi mail nếu cần, nhưng không chặn flow chính
                        }
                    }
                }

                return Ok(new
                {
                    success = true,
                    message = messageRes,
                    requirePayment = canThanhToanOnline,
                    paymentUrl = paymentUrl,
                    depositAmount = tienCocYeuCau,
                    maDonHang = newDonHang.MaDonHang,
                    danhSachBan = tenCacBan
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return StatusCode(500, new { message = "Lỗi máy chủ: " + ex.Message });
            }
        }


        [HttpGet("PaymentCallback")]
        public async Task<IActionResult> PaymentCallback()
        {
            // 1. Lấy kết quả trả về từ VNPay
            var query = Request.Query;
            string vnp_ResponseCode = query["vnp_ResponseCode"]; // 00 là thành công
            string vnp_TxnRef = query["vnp_TxnRef"]; // Mã đơn hàng mình gửi đi lúc đầu

            // 2. Tìm đơn hàng trong Database
            var donHang = await _context.DonHangs.FindAsync(vnp_TxnRef);

            if (donHang == null)
            {
                return Content("Lỗi: Không tìm thấy đơn hàng.");
            }

            // 3. Kiểm tra kết quả
            if (vnp_ResponseCode == "00")
            {
                // --- THANH TOÁN THÀNH CÔNG ---

                // Cập nhật trạng thái đơn hàng
                if (donHang.MaTrangThaiDonHang == "CHO_THANH_TOAN")
                {
                    donHang.MaTrangThaiDonHang = "DA_XAC_NHAN"; // Đã cọc xong -> Xác nhận
                    donHang.ThanhToan = true; // Đánh dấu đã thanh toán (cọc)

                    _context.DonHangs.Update(donHang);
                    await _context.SaveChangesAsync();
                }

                // Trả về trang HTML thông báo thành công
                string htmlSuccess = @"
                <html>
                    <head>
                        <meta name='viewport' content='width=device-width, initial-scale=1.0'>
                        <meta charset='UTF-8'> 
                        <style>
                            body { text-align: center; padding: 20px; font-family: Arial, sans-serif; }
                            .success { color: green; font-size: 20px; font-weight: bold; }
                        </style>
                    </head>
                    <body>
                        <h2 class='success'>THANH TOÁN THÀNH CÔNG!</h2>
                        <p>Cảm ơn bạn đã đặt cọc.</p>
                        <p>Đơn hàng: " + donHang.MaDonHang + @"</p>
                        <br>
                        <button onclick='window.close()'>Đóng và quay lại App</button>
                    </body>
                </html>";

                // <--- SỬA 2: THÊM charset=utf-8 VÀO ĐÂY -->
                return Content(htmlSuccess, "text/html; charset=utf-8");
            }
            else
            {
                // --- THANH TOÁN THẤT BẠI ---
                string htmlFail = @"
                <html>
                    <head>
                        <meta name='viewport' content='width=device-width, initial-scale=1.0'>
                        <meta charset='UTF-8'> 
                    </head>
                    <body style='text-align:center; padding:20px; font-family: Arial, sans-serif;'>
                        <h2 style='color:red'>THANH TOÁN THẤT BẠI</h2>
                        <p>Vui lòng thử lại.</p>
                        <button onclick='window.close()'>Quay lại</button>
                    </body>
                </html>";

                // <--- SỬA 2 (cho trang thất bại) -->
                return Content(htmlFail, "text/html; charset=utf-8");
            }
        }


        //[HttpPut("CapNhatTrangThai/{maDonHang}")]
        //public async Task<IActionResult> CapNhatTrangThai(string maDonHang, [FromBody] string maTrangThai)
        //{
        //    if (string.IsNullOrWhiteSpace(maTrangThai)) return BadRequest(new { message = "Mã trạng thái không hợp lệ." });

        //    var donHang = await _context.DonHangs.FindAsync(maDonHang);
        //    if (donHang == null) return NotFound(new { message = "Không tìm thấy đơn hàng." });

        //    var exists = await _context.TrangThaiDonHangs.AnyAsync(t => t.MaTrangThai == maTrangThai);
        //    if (!exists) return BadRequest(new { message = "Trạng thái không tồn tại." });

        //    donHang.MaTrangThaiDonHang = maTrangThai;

        //    // SỬA LỖI CÚ PHÁP & LOGIC THỜI GIAN
        //    if (maTrangThai == "DA_HOAN_THANH")
        //    {
        //        // Nếu TGNhanBan chưa có (null) thì lấy thời gian đặt
        //        donHang.TGNhanBan ??= donHang.ThoiGianDatHang;

        //        // Set thời gian kết thúc
        //        donHang.ThoiGianKetThuc ??= (donHang.TGNhanBan ?? DateTime.Now).AddMinutes(120);

        //        donHang.ThanhToan = true; // Đã hoàn thành thì coi như đã thanh toán
        //    }

        //    _context.DonHangs.Update(donHang);
        //    await _context.SaveChangesAsync();
        //    return Ok(new { message = "Cập nhật trạng thái thành công.", maDonHang, maTrangThai });
        //}




        [HttpPut("CapNhatTrangThai/{maDonHang}")]
        public async Task<IActionResult> CapNhatTrangThai(string maDonHang, [FromBody] string maTrangThai)
        {
            if (string.IsNullOrWhiteSpace(maTrangThai)) return BadRequest(new { message = "Mã trạng thái không hợp lệ." });

            var donHang = await _context.DonHangs
                .Include(dh => dh.BanAnDonHangs)
                .ThenInclude(badh => badh.MaBanNavigation)
                .FirstOrDefaultAsync(dh => dh.MaDonHang == maDonHang);

            if (donHang == null) return NotFound(new { message = "Không tìm thấy đơn hàng." });

            var exists = await _context.TrangThaiDonHangs.AnyAsync(t => t.MaTrangThai == maTrangThai);
            if (!exists) return BadRequest(new { message = "Trạng thái không tồn tại." });

            donHang.MaTrangThaiDonHang = maTrangThai;

            string maTrangThaiBanMoi = null;

            switch (maTrangThai)
            {
                case "CHO_THANH_TOAN":
                    maTrangThaiBanMoi = "TTBA002";
                    if (donHang.TGNhanBan == null) donHang.TGNhanBan = DateTime.Now;
                    break;

                case "DA_HOAN_THANH":
                    maTrangThaiBanMoi = "TTBA001";
                    if (donHang.ThoiGianKetThuc == null) donHang.ThoiGianKetThuc = DateTime.Now;
                    donHang.ThanhToan = true;
                    break;

                case "DA_HUY":
                case "NO_SHOW":
                    maTrangThaiBanMoi = "TTBA001";
                    break;
            }

            if (maTrangThaiBanMoi != null && donHang.BanAnDonHangs != null && donHang.BanAnDonHangs.Any())
            {
                foreach (var banAnDonHang in donHang.BanAnDonHangs)
                {
                    if (banAnDonHang.MaBanNavigation != null)
                    {
                        banAnDonHang.MaBanNavigation.MaTrangThai = maTrangThaiBanMoi;
                        _context.BanAns.Update(banAnDonHang.MaBanNavigation);
                    }
                }
            }

            _context.DonHangs.Update(donHang);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Cập nhật trạng thái thành công.", maDonHang, maTrangThai });
        }
    }
}