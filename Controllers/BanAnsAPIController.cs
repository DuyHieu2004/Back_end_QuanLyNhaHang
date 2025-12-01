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

        // API DÀNH RIÊNG CHO NHÂN VIÊN/QUẢN LÝ (Xem chi tiết)
        [HttpGet("GetManagerTableStatus")]
        public async Task<IActionResult> GetManagerTableStatus([FromQuery] DateTime dateTime)
        {
            if (_context.BanAns == null) return NotFound();

            // 1. Xác định khung giờ check: Từ thời điểm nhân viên tra cứu (dateTime) đến 2 tiếng sau
            var checkTimeStart = dateTime;
            var checkTimeEnd = dateTime.AddMinutes(120);

            // 2. Lấy danh sách Đơn hàng dính dáng tới khung giờ này
            var activeOrders = await _context.DonHangs
                .Include(dh => dh.BanAnDonHangs)
                .Where(dh =>
                    // Chỉ lấy các trạng thái đang chiếm dụng bàn: CHỜ/ĐÃ XÁC NHẬN (chưa Check-in), ĐANG PHỤC VỤ, CHỜ THANH TOÁN
                    (dh.MaTrangThaiDonHang == "CHO_XAC_NHAN" ||
                     dh.MaTrangThaiDonHang == "DA_XAC_NHAN" ||
                     dh.MaTrangThaiDonHang == "DANG_PHUC_VU" ||
                     dh.MaTrangThaiDonHang == "CHO_THANH_TOAN") &&

                    dh.ThoiGianDatHang != null &&
                    dh.ThoiGianKetThuc != null &&

                    // Logic trùng giờ (Overlap)
                    (checkTimeStart < dh.ThoiGianKetThuc) &&
                    (checkTimeEnd > dh.ThoiGianDatHang)
                )
                // Flatten (làm phẳng) danh sách bàn
                .SelectMany(dh => dh.BanAnDonHangs.Select(badh => new
                {
                    MaBan = badh.MaBan,
                    MaDonHang = dh.MaDonHang,
                    TenKhach = dh.TenNguoiNhan ?? "Khách vãng lai",
                    SDT = dh.SdtnguoiNhan,
                    GioDen = dh.ThoiGianDatHang,
                    TrangThaiDon = dh.MaTrangThaiDonHang
                }))
                .ToListAsync();

            // 3. Lấy tất cả bàn
            var allTables = await _context.BanAns
                .Include(b => b.MaTrangThaiNavigation)
                .Include(b => b.MaTangNavigation)
                .Where(b => b.IsShow == true)
                .ToListAsync();

            // 4. Ghép dữ liệu để trả về cho nhân viên
            var result = allTables.Select(ban =>
            {
                string finalStatus = "Trống";
                string note = "";

                // Lấy đơn hàng có trạng thái "Đang ngồi" hoặc "Sắp đến"
                var bookingInfo = activeOrders.FirstOrDefault(o => o.MaBan == ban.MaBan);

                // --- LOGIC PHÂN LOẠI TRẠNG THÁI CHO NHÂN VIÊN (Ưu tiên giảm dần) ---

                // Ưu tiên 1: Bàn đang bảo trì
                if (ban.MaTrangThai == "TTBA004")
                {
                    finalStatus = "Bảo trì";
                    note = ban.MaTrangThaiNavigation?.TenTrangThai ?? "Bảo trì thủ công";
                }
                // Ưu tiên 2: Bàn đã Check-in (Đang phục vụ/Chờ thanh toán)
                else if (bookingInfo != null &&
                         (bookingInfo.TrangThaiDon == "DANG_PHUC_VU" || bookingInfo.TrangThaiDon == "CHO_THANH_TOAN"))
                {
                    finalStatus = (bookingInfo.TrangThaiDon == "CHO_THANH_TOAN") ? "Chờ thanh toán" : "Đang phục vụ";
                    note = $"Khách: {bookingInfo.TenKhach} - Đơn #{bookingInfo.MaDonHang}";
                }
                // Ưu tiên 3: Bàn đã đặt nhưng khách chưa Check-in
                else if (bookingInfo != null &&
                         (bookingInfo.TrangThaiDon == "DA_XAC_NHAN" || bookingInfo.TrangThaiDon == "CHO_XAC_NHAN"))
                {
                    // Kiểm tra thời gian: Khách SẮP đến hay ĐÃ QUÁ giờ đến
                    if (bookingInfo.GioDen.HasValue && dateTime < bookingInfo.GioDen.Value)
                    {
                        // Sắp đến: thời gian check hiện tại nhỏ hơn giờ hẹn
                        finalStatus = "Đã đặt (Sắp đến)";
                        var minutesLeft = (bookingInfo.GioDen.Value - dateTime).TotalMinutes;
                        note = $"Đơn: {bookingInfo.TenKhach} ({bookingInfo.GioDen:HH:mm}) - Còn {Math.Ceiling(minutesLeft)} phút";
                    }
                    else
                    {
                        // Quá giờ đến: thời gian check hiện tại lớn hơn giờ hẹn (Khách trễ/Chờ No-show)
                        finalStatus = "Đã đặt (Quá giờ)";
                        note = $"Đơn: {bookingInfo.TenKhach} (Lẽ ra đến {bookingInfo.GioDen:HH:mm}) - Chờ Check-in/Hủy";
                    }
                }
                // Ưu tiên 4: Bàn đang phục vụ theo trạng thái DB (Walk-in hoặc Đơn đã Completed nhưng chưa reset)
                else if (ban.MaTrangThai == "TTBA002" || ban.MaTrangThai == "TTBA003")
                {
                    finalStatus = "Đang phục vụ (Walk-in/Cũ)";
                    note = ban.MaTrangThaiNavigation?.TenTrangThai ?? "Kiểm tra thủ công";
                }
                // Ưu tiên 5: Trống
                else
                {
                    finalStatus = "Trống";
                }

                return new
                {
                    MaBan = ban.MaBan,
                    TenBan = ban.TenBan,
                    SucChua = ban.SucChua,
                    TenTang = ban.MaTangNavigation?.TenTang,
                    TrangThaiHienThi = finalStatus, // Chuỗi chuẩn để FE tô màu
                    GhiChu = note,                 // Thông tin chi tiết cho nhân viên
                    MaTrangThaiGoc = ban.MaTrangThai
                };
            }).OrderBy(b => b.MaBan).ToList();

            return Ok(result);
        }


        [HttpGet("GetDashboardTableStatus")]
        public async Task<IActionResult> GetDashboardTableStatus([FromQuery] DateTime dateTime)
        {
            // 1. Lấy danh sách tất cả bàn từ DB
            var allTables = await _context.BanAns
                .Include(b => b.MaTrangThaiNavigation)
                .Include(b => b.MaTangNavigation)
                .Where(b => b.IsShow == true)
                .ToListAsync();

            // 2. Xác định khung giờ cần xem
            var gioBatDau = dateTime;
            var gioKetThuc = dateTime.AddMinutes(120); // Giả sử mỗi slot ăn là 2 tiếng

            // 3. Lấy danh sách các đơn hàng "chen chân" vào khung giờ này
            // SỬA ĐỔI: Truy vấn qua bảng trung gian BanAnDonHangs
            var conflictingData = await _context.DonHangs
                // Include bảng trung gian để lấy được thông tin bàn
                .Include(dh => dh.BanAnDonHangs)
                    .ThenInclude(badh => badh.MaBanNavigation)
                .Where(dh =>
                    // Lấy cả 3 trạng thái quan trọng
                    (dh.MaTrangThaiDonHang == "CHO_XAC_NHAN" ||
                     dh.MaTrangThaiDonHang == "DA_XAC_NHAN" ||
                     dh.MaTrangThaiDonHang == "CHO_THANH_TOAN") &&

                    dh.ThoiGianDatHang != null &&

                    // Logic kiểm tra trùng giờ (Overlap)
                    (gioBatDau < dh.ThoiGianKetThuc) &&
                    (gioKetThuc > dh.ThoiGianDatHang)
                )
                // FlatMap (Làm phẳng) danh sách bàn từ các đơn hàng tìm được
                .SelectMany(dh => dh.BanAnDonHangs.Select(badh => new
                {
                    MaBan = badh.MaBan, // Lấy mã bàn từ bảng trung gian
                    TrangThaiDon = dh.MaTrangThaiDonHang
                }))
                .ToListAsync();

            // 4. Map dữ liệu trả về (Logic này giữ nguyên)
            var result = allTables.Select(ban =>
            {
                string statusToDisplay = "Đang trống"; // Mặc định
                string maTrangThaiDB = ban.MaTrangThai;

                // Tìm xem bàn này có nằm trong danh sách trùng giờ không
                var conflict = conflictingData.FirstOrDefault(c => c.MaBan == ban.MaBan);

                // --- LOGIC XẾP HẠNG ƯU TIÊN (QUAN TRỌNG) ---

                // Mức 1: Bàn đang Bảo trì (Cao nhất)
                if (maTrangThaiDB == "TTBA004")
                {
                    statusToDisplay = "Bảo trì";
                }
                // Mức 2: Bàn đang có khách ngồi ăn (Active)
                // Gồm 2 trường hợp: 
                // a) DB ghi là đang phục vụ (TTBA002)
                // b) HOẶC có đơn hàng trùng giờ đang ở trạng thái "Chờ thanh toán"
                else if (maTrangThaiDB == "TTBA002" || (conflict != null && conflict.TrangThaiDon == "CHO_THANH_TOAN"))
                {
                    statusToDisplay = "Đang phục vụ"; // Màu Đỏ
                }
                // Mức 3: Bàn đã được đặt trước (Reserved)
                // Chỉ xảy ra khi có conflict và trạng thái là Chờ/Đã xác nhận
                else if (conflict != null)
                {
                    statusToDisplay = "Đã đặt"; // Màu Vàng
                }
                // Mức 4: Còn lại là Trống (Màu Xanh)

                // --- KẾT THÚC LOGIC ---

                return new BanAnDto
                {
                    maBan = ban.MaBan,
                    tenBan = ban.TenBan,
                    sucChua = ban.SucChua,
                    maTang = ban.MaTang,
                    tenTang = ban.MaTangNavigation?.TenTang,
                    tenTrangThai = statusToDisplay, // Trả về chuỗi chuẩn để FE tô màu
                    isShow = ban.IsShow
                };
            }).ToList();

            return Ok(result);
        }


        [HttpGet]
        public async Task<ActionResult<IEnumerable<BanAnDto>>> GetBanAns()
        {
            if (_context.BanAns == null)
            {
                return NotFound();
            }

            var banAns = await _context.BanAns
                .Include(b => b.MaTrangThaiNavigation)
                .Include(b => b.MaTangNavigation)
                .Where(b => b.IsShow == true)
                .Select(b => new BanAnDto
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


        [HttpPut("{maBan}/status")]
        public async Task<IActionResult> UpdateTableStatus(string maBan, [FromBody] string maTrangThai)
        {
            if (string.IsNullOrEmpty(maTrangThai))
            {
                return BadRequest(new { message = "Mã trạng thái không được rỗng." });
            }

            // Kiểm tra xem mã trạng thái gửi lên có hợp lệ không
            var trangThaiExists = await _context.TrangThaiBanAns.AnyAsync(t => t.MaTrangThai == maTrangThai);
            if (!trangThaiExists)
            {
                return BadRequest(new { message = "Mã trạng thái không tồn tại." });
            }

            var banAn = await _context.BanAns.FindAsync(maBan);
            if (banAn == null)
            {
                return NotFound(new { message = "Không tìm thấy bàn." });
            }

            banAn.MaTrangThai = maTrangThai;
            _context.BanAns.Update(banAn);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Cập nhật trạng thái bàn thành công." });
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

            // --- SỬA ĐỔI Ở ĐÂY ---
            // Truy vấn danh sách mã bàn bị trùng qua bảng trung gian
            var conflictingBookingIds = await _context.DonHangs
                .Include(dh => dh.BanAnDonHangs) // Include bảng trung gian
                .Where(dh =>
                    (dh.MaTrangThaiDonHang == "CHO_XAC_NHAN" ||
                     dh.MaTrangThaiDonHang == "DA_XAC_NHAN" ||
                     dh.MaTrangThaiDonHang == "CHO_THANH_TOAN") &&
                    dh.ThoiGianDatHang != null &&
                    (gioBatDau < dh.ThoiGianKetThuc) &&
                    (gioKetThuc > dh.ThoiGianDatHang)
                )
                // Flatten (làm phẳng) danh sách mã bàn
                .SelectMany(dh => dh.BanAnDonHangs.Select(badh => badh.MaBan))
                .Distinct()
                .ToListAsync();
            // ---------------------

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

                return new BanAnDto
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

            // SỬA ĐỔI: Truy vấn từ bảng trung gian BanAnDonHangs kết hợp với DonHangs
            var conflictingOrders = await _context.DonHangs
                // Include bảng trung gian để lấy thông tin bàn
                .Include(dh => dh.BanAnDonHangs)
                .Where(dh =>
                    // Các trạng thái Đơn hàng cần kiểm tra trùng lặp
                    (dh.MaTrangThaiDonHang == "CHO_XAC_NHAN" ||
                     dh.MaTrangThaiDonHang == "DA_XAC_NHAN" ||
                     dh.MaTrangThaiDonHang == "CHO_THANH_TOAN") &&

                    dh.ThoiGianDatHang != null &&
                    dh.ThoiGianKetThuc != null &&

                    // Logic trùng giờ (Time Overlap)
                    (gioBatDauKhachChon < dh.ThoiGianKetThuc) &&
                    (gioKetThucKhachChon > dh.ThoiGianDatHang)
                )
                // Flatten danh sách: Từ 1 Đơn -> Nhiều dòng (Mỗi dòng là 1 Bàn + Mã Khách Hàng)
                .SelectMany(dh => dh.BanAnDonHangs.Select(badh => new
                {
                    MaBan = badh.MaBan,         // Lấy Mã bàn từ bảng trung gian
                    MaKhachHang = dh.MaKhachHang // Lấy Mã khách từ bảng Đơn hàng
                }))
                .ToListAsync();

            // Tách danh sách bàn trùng thành 2 loại
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

                return new BanAnDto
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





