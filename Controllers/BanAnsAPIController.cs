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
            // SỬA ĐỔI: Lấy luôn cả trạng thái đơn (CHO_THANH_TOAN) để phân biệt màu
            var conflictingData = await _context.DonHangs
                .Include(dh => dh.MaBans)
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
                .SelectMany(dh => dh.MaBans.Select(b => new
                {
                    MaBan = b.MaBan,
                    TrangThaiDon = dh.MaTrangThaiDonHang
                }))
                .ToListAsync();

            // 4. Map dữ liệu trả về
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





