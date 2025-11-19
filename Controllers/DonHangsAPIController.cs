using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyNhaHang.Models;
using QuanLyNhaHang.Models.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace QuanLyNhaHang.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DonHangsAPIController : ControllerBase
    {
        private readonly QLNhaHangContext _context;

        public DonHangsAPIController(QLNhaHangContext context)
        {
            _context = context;
        }


        public class ThemMonVaoBanRequest
        {
            public string MaDonHang { get; set; }
            public string MaBan { get; set; }
            public List<MonAnOrderDTO> Items { get; set; }
        }

        public class MonAnOrderDTO
        {
            public string MaMonAn { get; set; } // Để tham khảo
            public string MaPhienBan { get; set; } // Kích thước (Nhỏ/Lớn...)
            public int SoLuong { get; set; }
            public string GhiChu { get; set; }
        }

        [HttpPost("ThemMonVaoBan")]
        public async Task<IActionResult> ThemMonVaoBan([FromBody] ThemMonVaoBanRequest request)
        {
            // 1. Tìm cái "Khóa ngoại" liên kết giữa Bàn và Đơn Hàng (Bảng BanAnDonHang)
            // Đây là mấu chốt để biết món ăn thuộc bàn nào
            var lienKetBanDon = await _context.BanAnDonHangs
                .FirstOrDefaultAsync(x => x.MaDonHang == request.MaDonHang && x.MaBan == request.MaBan);

            if (lienKetBanDon == null)
            {
                return BadRequest(new { message = "Bàn này không thuộc đơn hàng này (hoặc chưa được ghép vào đơn)." });
            }

            // 2. Duyệt qua danh sách món ăn gửi lên để thêm vào DB
            foreach (var item in request.Items)
            {
                // Tìm công thức nấu ăn dựa trên Phiên bản (Size)
                // (Giả định mỗi phiên bản món ăn có 1 công thức chính, hoặc bạn phải gửi MaCongThuc từ FE lên)
                var congThuc = await _context.CongThucNauAns
                    .FirstOrDefaultAsync(ct => ct.MaPhienBan == item.MaPhienBan);

                if (congThuc == null) continue; // Bỏ qua nếu không tìm thấy công thức

                var chiTietMoi = new ChiTietDonHang
                {
                    MaDonHang = request.MaDonHang,
                    MaPhienBan = item.MaPhienBan,
                    MaCongThuc = congThuc.MaCongThuc,
                    SoLuong = item.SoLuong,

                    // QUAN TRỌNG: Gắn món ăn này vào cái bàn cụ thể
                    MaBanAnDonHang = lienKetBanDon.MaBanAnDonHang
                };

                _context.ChiTietDonHangs.Add(chiTietMoi);

                // (Tùy chọn) Trừ kho ở đây nếu cần
            }

            await _context.SaveChangesAsync();
            return Ok(new { message = "Thêm món thành công" });
        }


        [HttpGet("GetActiveBookings")]
        public async Task<IActionResult> GetActiveBookings([FromQuery] DateTime? ngay)
        {
            // Mặc định là hôm nay nếu không truyền
            DateTime filterDate = ngay?.Date ?? DateTime.Today;

            var activeStatuses = new[] { "CHO_XAC_NHAN", "DA_XAC_NHAN", "CHO_THANH_TOAN" };
            var bookings = await _context.DonHangs
                // SỬA: Include bảng trung gian để lấy bàn
                .Include(dh => dh.BanAnDonHangs)
                    .ThenInclude(badh => badh.MaBanNavigation)
                .Include(dh => dh.MaTrangThaiDonHangNavigation) // Lấy tên trạng thái
                .Where(dh =>
                    activeStatuses.Contains(dh.MaTrangThaiDonHang) &&
                    // THÊM ĐIỀU KIỆN LỌC THEO NGÀY (dựa trên TgnhanBan)
                    dh.TgnhanBan.HasValue &&
                    dh.TgnhanBan.Value.Date == filterDate
                )
                .OrderBy(dh => dh.TgnhanBan)
                .Select(dh => new
                {
                   
                    maDonHang = dh.MaDonHang,
                    tenNguoiNhan = dh.TenNguoiNhan ?? dh.MaKhachHangNavigation.HoTen,
                    soNguoi = dh.SoLuongNguoiDk,
                    thoiGianNhanBan = dh.TgnhanBan,
                    trangThai = dh.MaTrangThaiDonHangNavigation.TenTrangThai,
                    maTrangThai = dh.MaTrangThaiDonHang,
                    listMaBan = dh.BanAnDonHangs.Select(b => b.MaBan).ToList(),
                    // SỬA: Lấy danh sách TÊN BÀN từ bảng trung gian
                    banAn = dh.BanAnDonHangs.Select(b => b.MaBanNavigation.TenBan).ToList()
                })
                .ToListAsync();

            return Ok(bookings);
        }

        [HttpGet("GetMyBookingDetail")]
        public async Task<IActionResult> GetMyBookingDetail(
    [FromQuery] string? maDonHang,
    [FromQuery] string? maBan,
    [FromQuery] DateTime? dateTime)
        {
            // 1. Xây dựng câu truy vấn với đầy đủ các bảng liên quan
            var query = _context.DonHangs
                // Lấy danh sách các bàn thuộc đơn hàng (Qua bảng trung gian mới)
                .Include(dh => dh.BanAnDonHangs)
                    .ThenInclude(badh => badh.MaBanNavigation)

                // Lấy thông tin khách hàng và trạng thái đơn
                .Include(dh => dh.MaKhachHangNavigation)
                .Include(dh => dh.MaTrangThaiDonHangNavigation)

                // Lấy thông tin chi tiết món ăn - Phần 1: Phiên bản (Size)
                .Include(dh => dh.ChiTietDonHangs)
                    .ThenInclude(ct => ct.MaPhienBanNavigation)

                // Lấy thông tin chi tiết món ăn - Phần 2: Công thức -> Món ăn -> Hình ảnh
                .Include(dh => dh.ChiTietDonHangs)
                    .ThenInclude(ct => ct.MaCongThucNavigation)
                        .ThenInclude(ctnau => ctnau.MaCtNavigation)
                            .ThenInclude(ctma => ctma.MaMonAnNavigation)
                                .ThenInclude(m => m.HinhAnhMonAns)

                // Lấy thông tin chi tiết món ăn - Phần 3: Món này thuộc Bàn nào? (QUAN TRỌNG)
                .Include(dh => dh.ChiTietDonHangs)
                    .ThenInclude(ct => ct.MaBanAnDonHangNavigation) // Link tới bảng trung gian
                        .ThenInclude(badh => badh.MaBanNavigation)  // Link tới bảng Bàn

                .AsSplitQuery() // Tối ưu hiệu năng truy vấn
                .AsQueryable();

            DonHang? donHang = null;

            // 2. Logic tìm kiếm đơn hàng
            if (!string.IsNullOrEmpty(maDonHang))
            {
                // Trường hợp 1: Tìm theo Mã Đơn Hàng
                donHang = await query.FirstOrDefaultAsync(dh => dh.MaDonHang == maDonHang);
            }
            else if (!string.IsNullOrEmpty(maBan) && dateTime != null)
            {
                // Trường hợp 2: Tìm theo Mã Bàn và Thời gian
                var gioBatDau = dateTime.Value;

                donHang = await query.FirstOrDefaultAsync(dh =>
                    // Kiểm tra xem đơn hàng có chứa bàn này không (qua bảng trung gian BanAnDonHangs)
                    dh.BanAnDonHangs.Any(b => b.MaBan == maBan) &&

                    // Chỉ lấy các đơn hàng đang hoạt động
                    (dh.MaTrangThaiDonHang == "CHO_XAC_NHAN" ||
                     dh.MaTrangThaiDonHang == "DA_XAC_NHAN" ||
                     dh.MaTrangThaiDonHang == "CHO_THANH_TOAN") &&

                    // Kiểm tra trùng khung giờ (Logic Active Time)
                    (gioBatDau < dh.TgnhanBan.Value.AddMinutes(120)) &&
                    (gioBatDau.AddMinutes(120) > dh.TgnhanBan.Value)
                );
            }
            else
            {
                return BadRequest(new { message = "Thiếu thông tin tìm kiếm (cần Mã Đơn Hàng hoặc Mã Bàn + Thời gian)." });
            }

            // 3. Kiểm tra kết quả tìm kiếm
            if (donHang == null)
            {
                return NotFound(new { message = "Không tìm thấy thông tin đặt bàn." });
            }

            // 4. Xử lý dữ liệu trả về (Mapping DTO)

            // Tạo chuỗi danh sách tên bàn (Ví dụ: "Bàn 1, Bàn 2")
            // SỬA: Dùng BanAnDonHangs thay vì MaBans cũ
            string tenCacBan = donHang.BanAnDonHangs != null && donHang.BanAnDonHangs.Any()
                ? string.Join(", ", donHang.BanAnDonHangs.Select(b => b.MaBanNavigation.TenBan))
                : "Chưa xếp bàn";

            var result = new ChiTietDatBanDto
            {
                MaDonHang = donHang.MaDonHang,
                ThoiGianDat = donHang.ThoiGianDatHang ?? DateTime.Now,
                TenBan = tenCacBan,
                ThoiGianNhanBan = donHang.TgnhanBan,
                ThoiGianKetThuc = donHang.ThoiGianKetThuc,
                SoNguoi = donHang.SoLuongNguoiDk,
                GhiChu = donHang.GhiChu,
                TienDatCoc = donHang.TienDatCoc,
                TrangThai = donHang.MaTrangThaiDonHangNavigation.TenTrangThai,
                TenNguoiDat = donHang.TenNguoiNhan ?? donHang.MaKhachHangNavigation.HoTen,
                SDTNguoiDat = donHang.SDTNguoiNhan ?? donHang.MaKhachHangNavigation.SoDienThoai,

                // Map danh sách món ăn chi tiết
                MonAns = donHang.ChiTietDonHangs
                    .Select(ct =>
                    {
                        var congThuc = ct.MaCongThucNavigation;
                        var phienBan = ct.MaPhienBanNavigation;
                        var monAn = congThuc?.MaCtNavigation?.MaMonAnNavigation;

                        // Lấy hình ảnh đầu tiên (nếu có)
                        string hinhAnhUrl = monAn?.HinhAnhMonAns.FirstOrDefault()?.URLHinhAnh ?? "";

                        // Xác định mã bàn và tên bàn cụ thể cho món ăn này (QUAN TRỌNG ĐỂ FRONTEND LỌC)
                        string maBanCuThe = ct.MaBanAnDonHangNavigation?.MaBan ?? "";
                        string tenBanCuThe = ct.MaBanAnDonHangNavigation?.MaBanNavigation?.TenBan ?? "Chung";

                        return new MonAnDatDto
                        {
                            TenMon = monAn?.TenMonAn ?? "Món không xác định",
                            TenPhienBan = phienBan?.TenPhienBan ?? "",
                            SoLuong = ct.SoLuong,
                            DonGia = congThuc?.Gia ?? 0,
                            HinhAnh = hinhAnhUrl,

                            // Trả về thông tin bàn để Frontend biết món này của bàn nào
                            MaBan = maBanCuThe,
                            TenBan = tenBanCuThe,

                            GhiChu = "" // (Nếu DB có cột GhiChu trong ChiTietDonHang thì map vào đây)
                        };
                    })
                    .OrderBy(m => m.TenBan) // Sắp xếp danh sách món theo tên bàn cho dễ nhìn
                    .ToList()
            };

            return Ok(result);
        }

        // API: Lấy danh sách cần gọi điện (Giữ nguyên logic nhưng update tên biến)
        [HttpGet("get-customers-to-call")]
        public async Task<IActionResult> GetCustomersToCall()
        {
            var now = DateTime.Now;
            var thoiGianQuetDen = now.AddHours(24);

            var listCanGoi = await _context.DonHangs
                .Include(dh => dh.MaKhachHangNavigation)
                .Where(dh =>
                    (dh.MaTrangThaiDonHang == "CHO_XAC_NHAN" || dh.MaTrangThaiDonHang == "DA_XAC_NHAN") &&
                    dh.TgnhanBan != null && // Kiểm tra null
                    dh.TgnhanBan > now &&   // Dùng TgnhanBan thay vì ThoiGianBatDau
                    dh.TgnhanBan <= thoiGianQuetDen &&
                    (dh.MaKhachHangNavigation.Email == null || dh.MaKhachHangNavigation.Email == "")
                )
                .Select(dh => new
                {
                    dh.MaDonHang,
                    TenKhach = dh.TenNguoiNhan ?? dh.MaKhachHangNavigation.HoTen,
                    SDT = dh.SDTNguoiNhan ?? dh.MaKhachHangNavigation.SoDienThoai,
                    GioHen = dh.TgnhanBan,
                    SoNguoi = dh.SoLuongNguoiDk, // Update tên biến
                    GhiChu = "Khách không có email, cần gọi nhắc."
                })
                .OrderBy(dh => dh.GioHen)
                .ToListAsync();

            return Ok(listCanGoi);
        }
    }
}