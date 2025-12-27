using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyNhaHang.Models;
using QuanLyNhaHang.Models.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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

        // ==================================================================================
        // 1. API CHO QUẢN LÝ (XEM TRẠNG THÁI BÀN CHI TIẾT)
        // ==================================================================================
        [HttpGet("GetManagerTableStatus")]
        [Authorize(Roles = "NhanVien,QuanLy")] // Chỉ nhân viên và quản lý mới được xem
        public async Task<IActionResult> GetManagerTableStatus([FromQuery] DateTime dateTime)
        {
            if (_context.BanAns == null) return NotFound();

            // SỬA: Lấy booking trực tiếp từ DonHang.BanAnDonHangs (không qua ChiTietDonHang)
            // Vì khi đặt bàn chưa có món, sẽ KHÔNG có ChiTietDonHang!
            var activeTableBookings = await _context.DonHangs
                .Include(dh => dh.BanAnDonHangs) // Direct table assignments
                .Where(dh =>
                    // Check trạng thái đơn
                    (dh.MaTrangThaiDonHang == "CHO_XAC_NHAN" ||
                     dh.MaTrangThaiDonHang == "DA_XAC_NHAN" ||
                     dh.MaTrangThaiDonHang == "DANG_PHUC_VU" ||
                     dh.MaTrangThaiDonHang == "CHO_THANH_TOAN") &&

                    // Có ít nhất một mốc thời gian
                    (dh.TGNhanBan != null ||
                     dh.ThoiGianDatHang != null ||
                     dh.TgdatDuKien != null)
                )
                .SelectMany(dh => dh.BanAnDonHangs.Select(badh => new
                {
                    MaBan = badh.MaBan,
                    MaDonHang = dh.MaDonHang,
                    TenKhach = dh.TenNguoiNhan ?? "Khách vãng lai",
                    SDT = dh.SdtnguoiNhan,
                    // GioDen: Ưu tiên TGNhanBan (đã check-in), nếu null thì dùng TgdatDuKien, cuối cùng là ThoiGianDatHang
                    GioDen = dh.TGNhanBan ?? dh.TgdatDuKien ?? dh.ThoiGianDatHang,
                    TGNhanBan = dh.TGNhanBan,
                    TgdatDuKien = dh.TgdatDuKien,
                    ThoiGianKetThuc = dh.ThoiGianKetThuc,
                    TrangThaiDon = dh.MaTrangThaiDonHang
                }))
                .ToListAsync();

            // DEBUG: Log để kiểm tra
            Console.WriteLine($"🔍 [GetManagerTableStatus] DateTime: {dateTime:yyyy-MM-dd HH:mm}");
            Console.WriteLine($"🔍 Found {activeTableBookings.Count} active table bookings:");
            foreach (var booking in activeTableBookings)
            {
                Console.WriteLine($"   - Bàn {booking.MaBan}: Đơn {booking.MaDonHang}, GioDen={booking.GioDen:yyyy-MM-dd HH:mm}, Status={booking.TrangThaiDon}");
            }

            // Lấy tất cả bàn
            var allTables = await _context.BanAns
                .Include(b => b.MaTrangThaiNavigation)
                .Include(b => b.MaTangNavigation)
                .Where(b => b.IsShow == true)
                .ToListAsync();

            // Map kết quả với logic kiểm tra khoảng thời gian
            var result = allTables.Select(ban =>
            {
                string finalStatus = "Trống";
                string note = "";
                DateTime? thoiGianVao = null;

                // SỬA: Tìm đơn MATCH với datetime (không chỉ lấy đơn đầu tiên!)
                // OPTION A: Chỉ hiển thị đơn ĐÚNG thời điểm, không hiển thị đơn tương lai xa
                var allBookingsForTable = activeTableBookings.Where(o => o.MaBan == ban.MaBan).ToList();
                
                // Ưu tiên 1: Tìm đơn ĐANG PHỤC VỤ match với datetime
                var bookingInfo = allBookingsForTable.FirstOrDefault(b =>
                    (b.TrangThaiDon == "CHO_THANH_TOAN" || b.TrangThaiDon == "DANG_PHUC_VU") &&
                    b.TGNhanBan.HasValue &&
                    dateTime >= b.TGNhanBan.Value &&
                    dateTime <= (b.ThoiGianKetThuc ?? b.TGNhanBan.Value.AddHours(2))
                );

                // Ưu tiên 2: Nếu không có đơn đang phục vụ, tìm đơn ĐẶT TRƯỚC CÙNG NGÀY
                if (bookingInfo == null && allBookingsForTable.Any())
                {
                    var selectedDate = dateTime.Date;
                    bookingInfo = allBookingsForTable
                        .Where(b => 
                            (b.TrangThaiDon == "CHO_XAC_NHAN" || b.TrangThaiDon == "DA_XAC_NHAN") &&
                            b.GioDen.HasValue &&
                            b.GioDen.Value.Date == selectedDate  // ← FILTER: Chỉ lấy đơn CÙNG NGÀY
                        )
                        .OrderBy(b => Math.Abs((b.GioDen.Value - dateTime).TotalMinutes))
                        .FirstOrDefault();
                }

                // DEBUG: Log để trace logic cho từng bàn quan trọng
                if (ban.MaBan == "B003")
                {
                    Console.WriteLine($"🔍 [Bàn B003] DateTime selected: {dateTime:yyyy-MM-dd HH:mm}");
                    Console.WriteLine($"   Found {allBookingsForTable.Count} total bookings for this table");
                    if (bookingInfo != null)
                    {
                        Console.WriteLine($"   ✅ Selected booking: {bookingInfo.MaDonHang}, GioDen={bookingInfo.GioDen:yyyy-MM-dd HH:mm}, Status={bookingInfo.TrangThaiDon}");
                        var diff = (bookingInfo.GioDen.Value - dateTime).TotalMinutes;
                        Console.WriteLine($"   minutesDiff = {diff:F1} phút");
                    }
                    else
                    {
                        Console.WriteLine($"   ❌ No booking matches this datetime");
                        foreach (var b in allBookingsForTable)
                        {
                            Console.WriteLine($"      - Skipped: {b.MaDonHang}, GioDen={b.GioDen:yyyy-MM-dd HH:mm} (different date or not in range)");
                        }
                    }
                }

                // KIỂM TRA BẢO TRÌ TRƯỚC (Ưu tiên cao nhất)
                if (ban.MaTrangThai == "TTBA004")
                {
                    finalStatus = "Bảo trì";
                    note = ban.MaTrangThaiNavigation?.TenTrangThai ?? "Bảo trì thủ công";
                }
                // KIỂM TRA ĐƠN HÀNG ĐANG PHỤC VỤ/CHỜ THANH TOÁN
                else if (bookingInfo != null &&
                         (bookingInfo.TrangThaiDon == "DANG_PHUC_VU" || bookingInfo.TrangThaiDon == "CHO_THANH_TOAN"))
                {
                    // *** FIX: Kiểm tra xem dateTime có nằm trong khoảng phục vụ không ***
                    bool isInServiceTime = false;
                    
                    if (bookingInfo.TGNhanBan.HasValue && bookingInfo.ThoiGianKetThuc.HasValue)
                    {
                        // Kiểm tra dateTime có nằm giữa TGNhanBan và ThoiGianKetThuc không
                        isInServiceTime = dateTime >= bookingInfo.TGNhanBan.Value && 
                                         dateTime <= bookingInfo.ThoiGianKetThuc.Value;
                    }
                    else if (bookingInfo.TGNhanBan.HasValue)
                    {
                        // Nếu chỉ có TGNhanBan (chưa có giờ kết thúc), giả sử phục vụ 2 tiếng
                        var estimatedEnd = bookingInfo.TGNhanBan.Value.AddHours(2);
                        isInServiceTime = dateTime >= bookingInfo.TGNhanBan.Value && 
                                         dateTime <= estimatedEnd;
                    }

                    // Chỉ hiển thị "Đang phục vụ" nếu datetime nằm trong khoảng thời gian phục vụ
                    if (isInServiceTime)
                    {
                        finalStatus = (bookingInfo.TrangThaiDon == "CHO_THANH_TOAN") ? "Chờ thanh toán" : "Đang phục vụ";
                        note = $"Khách: {bookingInfo.TenKhach} - Đơn #{bookingInfo.MaDonHang}";
                        thoiGianVao = bookingInfo.TGNhanBan;
                    }
                    else
                    {
                        // Nếu không trong khoảng thời gian, coi như trống
                        finalStatus = "Trống";
                    }
                }
                // KIỂM TRA ĐƠN ĐẶT TRƯỚC (CHỜ XÁC NHẬN / ĐÃ XÁC NHẬN)
                else if (bookingInfo != null &&
                         (bookingInfo.TrangThaiDon == "DA_XAC_NHAN" || bookingInfo.TrangThaiDon == "CHO_XAC_NHAN"))
                {
                    // Logic đặt trước:
                    // - Nếu còn <= 120 phút (2 tiếng) tới giờ đến  => "Đã đặt (Sắp đến)"
                    // - Nếu đã quá giờ đến                        => "Đã đặt (Quá giờ)"
                    // - Nếu còn > 120 phút                        => vẫn coi là "Trống" (chỉ là có booking xa)
                    if (bookingInfo.GioDen.HasValue)
                    {
                        var minutesDiff = (bookingInfo.GioDen.Value - dateTime).TotalMinutes;

                        if (minutesDiff >= 0)
                        {
                            // Chưa đến giờ đặt
                            if (minutesDiff <= 120)
                            {
                                // Còn trong cửa sổ 2 tiếng tới giờ đến
                                finalStatus = "Đã đặt (Sắp đến)";
                                var minutesLeft = Math.Ceiling(minutesDiff);
                                note = $"Đơn: {bookingInfo.TenKhach} ({bookingInfo.GioDen:HH:mm}) - Còn {minutesLeft} phút";
                            }
                            else
                            {
                                // COMMENTED: Rule 2 tiếng - Giờ hiển thị tất cả đơn đặt
                                // minutesDiff > 120  => giờ đến còn xa, vẫn cho hiển thị là Trống
                                // finalStatus = "Trống";
                                
                                // NEW: Hiển thị "Đã đặt" cho tất cả đơn, kể cả còn xa
                                finalStatus = "Đã đặt";
                                var days = (int)(minutesDiff / 1440);
                                var hours = (int)((minutesDiff % 1440) / 60);
                                
                                if (days > 0)
                                    note = $"Đơn: {bookingInfo.TenKhach} ({bookingInfo.GioDen:dd/MM HH:mm}) - Còn {days} ngày";
                                else if (hours > 2)
                                    note = $"Đơn: {bookingInfo.TenKhach} ({bookingInfo.GioDen:HH:mm}) - Còn {hours} giờ";
                                else
                                    note = $"Đơn: {bookingInfo.TenKhach} ({bookingInfo.GioDen:HH:mm})";
                            }
                        }
                        else
                        {
                            // Đã quá giờ mà khách chưa check-in
                            finalStatus = "Đã đặt (Quá giờ)";
                            note = $"Đơn: {bookingInfo.TenKhach} (Lẽ ra đến {bookingInfo.GioDen:HH:mm}) - Chờ Check-in/Hủy";
                        }
                    }
                }
                // OPTION 3: KHÔNG CÓ ĐƠN ACTIVE - Kiểm tra DB mismatch
                else if (bookingInfo == null)
                {
                    // Nếu DB cho rằng bàn đang bận nhưng KHÔNG có đơn active
                    // → Có thể DB chưa được reset sau khi đơn hoàn thành
                    if (ban.MaTrangThai == "TTBA002" || ban.MaTrangThai == "TTBA003")
                    {
                        // OPTION 3: Log warning nhưng vẫn hiển thị Trống
                        Console.WriteLine($"⚠️ [DB Mismatch] Bàn {ban.MaBan} có DB status {ban.MaTrangThai} nhưng không có đơn active tại {dateTime:yyyy-MM-dd HH:mm}");
                        finalStatus = "Trống";
                        note = ""; // Không hiển thị note nhầm lẫn
                    }
                    else
                    {
                        // Bàn thực sự trống (TTBA001)
                        finalStatus = "Trống";
                    }
                }
                else
                {
                    finalStatus = "Trống";
                }

                // OPTION 3: Kiểm tra DB có khớp với logic không
                bool isDbMismatch = false;
                if (ban.MaTrangThai == "TTBA002" && finalStatus != "Đang phục vụ" && finalStatus != "Chờ thanh toán")
                {
                    isDbMismatch = true;
                }
                else if (ban.MaTrangThai == "TTBA003" && !finalStatus.Contains("Đã đặt"))
                {
                    isDbMismatch = true;
                }
                else if (ban.MaTrangThai == "TTBA001" && finalStatus != "Trống")
                {
                    isDbMismatch = true;
                }

                return new
                {
                    MaBan = ban.MaBan,
                    TenBan = ban.TenBan,
                    SucChua = ban.SucChua,
                    MaTang = ban.MaTang,
                    TenTang = ban.MaTangNavigation?.TenTang,
                    TrangThaiHienThi = finalStatus,
                    GhiChu = note,
                    ThoiGianVao = thoiGianVao?.ToString("yyyy-MM-ddTHH:mm:ss"),
                    MaTrangThaiGoc = ban.MaTrangThai, // DB status gốc
                    IsDbMismatch = isDbMismatch // OPTION 3: Flag debug
                };
            }).OrderBy(b => b.MaBan).ToList();

            return Ok(result);
        }

        // ==================================================================================
        // 2. API CHO DASHBOARD (TỔNG QUAN)
        // ==================================================================================
        [HttpGet("GetDashboardTableStatus")]
        [Authorize(Roles = "NhanVien,QuanLy")] // Chỉ nhân viên và quản lý mới được xem
        public async Task<IActionResult> GetDashboardTableStatus([FromQuery] DateTime dateTime)
        {
            var allTables = await _context.BanAns
                .Include(b => b.MaTrangThaiNavigation)
                .Include(b => b.MaTangNavigation)
                .Where(b => b.IsShow == true)
                .ToListAsync();

            var gioBatDau = dateTime;
            var gioKetThuc = dateTime.AddMinutes(120);

            // SỬA LỖI: Truy vấn từ BanAnDonHangs -> ChiTiet -> DonHang
            // Xử lý cả trường hợp đơn đã check-in (TGNhanBan) và chưa check-in (TgdatDuKien)
            var conflictingData = await _context.BanAnDonHangs
                .Include(badh => badh.MaChiTietDonHangNavigation)
                    .ThenInclude(ct => ct.MaDonHangNavigation)
                .Where(badh =>
                    (badh.MaChiTietDonHangNavigation.MaDonHangNavigation.MaTrangThaiDonHang == "CHO_XAC_NHAN" ||
                     badh.MaChiTietDonHangNavigation.MaDonHangNavigation.MaTrangThaiDonHang == "DA_XAC_NHAN" ||
                     badh.MaChiTietDonHangNavigation.MaDonHangNavigation.MaTrangThaiDonHang == "CHO_THANH_TOAN" ||
                     badh.MaChiTietDonHangNavigation.MaDonHangNavigation.MaTrangThaiDonHang == "DANG_PHUC_VU") &&

                    // Xác định thời gian bắt đầu: ưu tiên TGNhanBan (đã check-in), nếu không có thì dùng TgdatDuKien
                    ((badh.MaChiTietDonHangNavigation.MaDonHangNavigation.TGNhanBan != null) ||
                     (badh.MaChiTietDonHangNavigation.MaDonHangNavigation.TgdatDuKien != null)) &&

                    badh.MaChiTietDonHangNavigation.MaDonHangNavigation.ThoiGianKetThuc != null &&

                    // Logic overlap: gioBatDau < thoiGianKetThuc && gioKetThuc > thoiGianBatDau
                    (gioBatDau < badh.MaChiTietDonHangNavigation.MaDonHangNavigation.ThoiGianKetThuc) &&
                    (gioKetThuc > (badh.MaChiTietDonHangNavigation.MaDonHangNavigation.TGNhanBan ?? 
                                   badh.MaChiTietDonHangNavigation.MaDonHangNavigation.TgdatDuKien ?? 
                                   badh.MaChiTietDonHangNavigation.MaDonHangNavigation.ThoiGianDatHang).Value)
                )
                .Select(badh => new
                {
                    MaBan = badh.MaBan,
                    TrangThaiDon = badh.MaChiTietDonHangNavigation.MaDonHangNavigation.MaTrangThaiDonHang
                })
                .ToListAsync();

            var result = allTables.Select(ban =>
            {
                string statusToDisplay = "Đang trống";
                string maTrangThaiDB = ban.MaTrangThai;
                var conflict = conflictingData.FirstOrDefault(c => c.MaBan == ban.MaBan);

                if (maTrangThaiDB == "TTBA004") statusToDisplay = "Bảo trì";
                else if (maTrangThaiDB == "TTBA002" || (conflict != null && conflict.TrangThaiDon == "CHO_THANH_TOAN")) statusToDisplay = "Đang phục vụ";
                else if (conflict != null) statusToDisplay = "Đã đặt";

                return new BanAnDto
                {
                    maBan = ban.MaBan,
                    tenBan = ban.TenBan,
                    sucChua = ban.SucChua,
                    maTang = ban.MaTang,
                    tenTang = ban.MaTangNavigation?.TenTang,
                    tenTrangThai = statusToDisplay,
                    isShow = ban.IsShow
                };
            }).ToList();

            return Ok(result);
        }

        // ==================================================================================
        // 3. API LẤY DANH SÁCH BÀN CƠ BẢN (KHÔNG ĐỔI)
        // ==================================================================================
        [HttpGet]
        public async Task<ActionResult<IEnumerable<BanAnDto>>> GetBanAns()
        {
            if (_context.BanAns == null) return NotFound();

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
                }).ToListAsync();

            return Ok(banAns);
        }

        // ==================================================================================
        // 4. API CẬP NHẬT TRẠNG THÁI BÀN
        // ==================================================================================
        [HttpPut("{maBan}/status")]
        [Authorize(Roles = "NhanVien,QuanLy")] // Chỉ nhân viên và quản lý mới được cập nhật
        public async Task<IActionResult> UpdateTableStatus(string maBan, [FromBody] string maTrangThai)
        {
            if (string.IsNullOrEmpty(maTrangThai)) return BadRequest(new { message = "Mã trạng thái rỗng." });

            var trangThaiExists = await _context.TrangThaiBanAns.AnyAsync(t => t.MaTrangThai == maTrangThai);
            if (!trangThaiExists) return BadRequest(new { message = "Mã trạng thái không tồn tại." });

            var banAn = await _context.BanAns.FindAsync(maBan);
            if (banAn == null) return NotFound(new { message = "Không tìm thấy bàn." });

            banAn.MaTrangThai = maTrangThai;
            _context.BanAns.Update(banAn);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Cập nhật thành công." });
        }

        // ==================================================================================
        // 5. API LẤY TRẠNG THÁI THEO GIỜ
        // ==================================================================================
        [HttpGet("GetStatusByTime")]
        public async Task<IActionResult> GetStatusByTime([FromQuery] DateTime dateTime, [FromQuery] int soNguoi, [FromQuery] string? maKhachHang = null)
        {
            if (_context.BanAns == null || _context.DonHangs == null) return NotFound();

            // Giới hạn ngày hợp lệ cho SQL Server
            var sqlMinDate = new DateTime(1753, 1, 1);
            var sqlMaxDate = new DateTime(9999, 12, 31);

            var gioBatDau = dateTime;
            var gioKetThuc = dateTime.AddMinutes(120);
            var ngayChon = dateTime.Date;

            // SỬA LỖI: Đi từ BanAnDonHang -> ChiTiet -> DonHang
            // Xử lý cả trường hợp đơn đã check-in (TGNhanBan) và chưa check-in (TgdatDuKien)
            var conflictingBookingData = await _context.BanAnDonHangs
                .Include(badh => badh.MaChiTietDonHangNavigation)
                    .ThenInclude(ct => ct.MaDonHangNavigation)
                .Where(badh =>
                    (badh.MaChiTietDonHangNavigation.MaDonHangNavigation.MaTrangThaiDonHang == "CHO_XAC_NHAN" ||
                     badh.MaChiTietDonHangNavigation.MaDonHangNavigation.MaTrangThaiDonHang == "DA_XAC_NHAN" ||
                     badh.MaChiTietDonHangNavigation.MaDonHangNavigation.MaTrangThaiDonHang == "CHO_THANH_TOAN" ||
                     badh.MaChiTietDonHangNavigation.MaDonHangNavigation.MaTrangThaiDonHang == "DANG_PHUC_VU") &&

                    // Có ít nhất một mốc thời gian bắt đầu
                    (badh.MaChiTietDonHangNavigation.MaDonHangNavigation.TGNhanBan != null ||
                     badh.MaChiTietDonHangNavigation.MaDonHangNavigation.TgdatDuKien != null ||
                     badh.MaChiTietDonHangNavigation.MaDonHangNavigation.ThoiGianDatHang != null) &&

                    badh.MaChiTietDonHangNavigation.MaDonHangNavigation.ThoiGianKetThuc != null &&

                    // Logic overlap: gioBatDau < thoiGianKetThuc && gioKetThuc > thoiGianBatDau
                    (gioBatDau < (badh.MaChiTietDonHangNavigation.MaDonHangNavigation.ThoiGianKetThuc ?? sqlMaxDate)) &&
                    (gioKetThuc > (badh.MaChiTietDonHangNavigation.MaDonHangNavigation.TGNhanBan ??
                                   badh.MaChiTietDonHangNavigation.MaDonHangNavigation.TgdatDuKien ??
                                   badh.MaChiTietDonHangNavigation.MaDonHangNavigation.ThoiGianDatHang ??
                                   sqlMinDate)) &&

                    // Chỉ xét các đơn cùng ngày người dùng chọn
                    EF.Functions.DateDiffDay(
                        ngayChon,
                        (badh.MaChiTietDonHangNavigation.MaDonHangNavigation.TGNhanBan ??
                         badh.MaChiTietDonHangNavigation.MaDonHangNavigation.TgdatDuKien ??
                         badh.MaChiTietDonHangNavigation.MaDonHangNavigation.ThoiGianDatHang ??
                         sqlMinDate)
                    ) == 0
                )
                .Select(badh => new
                {
                    MaBan = badh.MaBan,
                    MaKhachHang = badh.MaChiTietDonHangNavigation.MaDonHangNavigation.MaKhachHang
                })
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
                if (string.IsNullOrEmpty(maTang) && ban.MaBan.StartsWith("B") && int.TryParse(ban.MaBan.Substring(1), out int banNum))
                {
                    maTang = banNum <= 14 ? "T001" : banNum <= 27 ? "T002" : "T003";
                }
                string tenTang = (!string.IsNullOrEmpty(maTang) && allTangs.ContainsKey(maTang)) ? allTangs[maTang] : ban.MaTangNavigation?.TenTang;

                var conflictRecord = conflictingBookingData.FirstOrDefault(c => c.MaBan == ban.MaBan);
                bool isConflicting = conflictRecord != null;
                bool isCuaTui = isConflicting && !string.IsNullOrEmpty(maKhachHang) && conflictRecord.MaKhachHang == maKhachHang;

                string trangThaiHienThi;
                if (ban.MaTrangThaiNavigation?.TenTrangThai?.ToLower() == "đang bảo trì")
                    trangThaiHienThi = "Đang bảo trì";
                else if (isCuaTui)
                    trangThaiHienThi = "CuaTui";
                else if (isConflicting)
                    trangThaiHienThi = "Đã đặt";
                else if (ban.SucChua < soNguoi)
                    trangThaiHienThi = "Không đủ sức chứa";
                else
                    trangThaiHienThi = "Đang trống";

                return new BanAnDto
                {
                    maBan = ban.MaBan,
                    tenBan = ban.TenBan,
                    sucChua = ban.SucChua,
                    maTang = maTang ?? string.Empty,
                    tenTang = tenTang ?? string.Empty,
                    tenTrangThai = trangThaiHienThi
                };
            }).ToList();

            return Ok(result);
        }

        // ==================================================================================
        // 6. API LẤY BÀN TRỐNG ĐỂ GỢI Ý (AVAILABLE TABLES)
        // ==================================================================================
        [HttpGet("GetAvailableBanAns")]
        public async Task<IActionResult> GetAvailableBanAns([FromQuery] DateTime dateTime, [FromQuery] int soNguoi, [FromQuery] string? maKhachHang)
        {
            if (_context.BanAns == null) return NotFound("DB chưa sẵn sàng.");

            var gioBatDauKhachChon = dateTime;
            var gioKetThucKhachChon = dateTime.AddMinutes(120);

            // SỬA LỖI: Truy vấn BanAnDonHangs -> ChiTiet -> DonHang
            // Xử lý cả trường hợp đơn đã check-in (TGNhanBan) và chưa check-in (TgdatDuKien)
            var conflictingRecords = await _context.BanAnDonHangs
                .Include(badh => badh.MaChiTietDonHangNavigation)
                    .ThenInclude(ct => ct.MaDonHangNavigation)
                .Where(badh =>
                    (badh.MaChiTietDonHangNavigation.MaDonHangNavigation.MaTrangThaiDonHang == "CHO_XAC_NHAN" ||
                     badh.MaChiTietDonHangNavigation.MaDonHangNavigation.MaTrangThaiDonHang == "DA_XAC_NHAN" ||
                     badh.MaChiTietDonHangNavigation.MaDonHangNavigation.MaTrangThaiDonHang == "CHO_THANH_TOAN" ||
                     badh.MaChiTietDonHangNavigation.MaDonHangNavigation.MaTrangThaiDonHang == "DANG_PHUC_VU") &&

                    // Xác định thời gian bắt đầu: ưu tiên TGNhanBan (đã check-in), nếu không có thì dùng TgdatDuKien
                    ((badh.MaChiTietDonHangNavigation.MaDonHangNavigation.TGNhanBan != null) ||
                     (badh.MaChiTietDonHangNavigation.MaDonHangNavigation.TgdatDuKien != null)) &&

                    badh.MaChiTietDonHangNavigation.MaDonHangNavigation.ThoiGianKetThuc != null &&

                    // Logic overlap: gioBatDau < thoiGianKetThuc && gioKetThuc > thoiGianBatDau
                    (gioBatDauKhachChon < badh.MaChiTietDonHangNavigation.MaDonHangNavigation.ThoiGianKetThuc) &&
                    (gioKetThucKhachChon > (badh.MaChiTietDonHangNavigation.MaDonHangNavigation.TGNhanBan ?? 
                                            badh.MaChiTietDonHangNavigation.MaDonHangNavigation.TgdatDuKien ?? 
                                            badh.MaChiTietDonHangNavigation.MaDonHangNavigation.ThoiGianDatHang).Value)
                )
                .Select(badh => new
                {
                    MaBan = badh.MaBan,
                    MaKhachHang = badh.MaChiTietDonHangNavigation.MaDonHangNavigation.MaKhachHang
                })
                .ToListAsync();

            var banNguoiKhacDatIds = conflictingRecords.Where(r => r.MaKhachHang != maKhachHang).Select(r => r.MaBan).Distinct().ToList();
            var banCuaTuiIds = conflictingRecords.Where(r => r.MaKhachHang == maKhachHang).Select(r => r.MaBan).Distinct().ToList();

            var allTables = await _context.BanAns
                .Include(b => b.MaTrangThaiNavigation)
                .Include(b => b.MaTangNavigation)
                .Where(b => b.IsShow == true)
                .ToListAsync();

            var result = allTables.Select(ban =>
            {
                string trangThaiHienThi = "Trong";

                if (ban.MaTrangThai == "TTBA004") trangThaiHienThi = "BaoTri";
                else if (banCuaTuiIds.Contains(ban.MaBan)) trangThaiHienThi = "CuaTui";
                else if (banNguoiKhacDatIds.Contains(ban.MaBan)) trangThaiHienThi = "DaDat";
                else if (ban.SucChua < soNguoi) trangThaiHienThi = "CanGhep";
                else trangThaiHienThi = "Trong";

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