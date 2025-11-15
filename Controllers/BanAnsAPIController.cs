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

            // 1. Xác định khung giờ khách chọn (Ăn trong 2 tiếng)
            var gioBatDau = dateTime;
            var gioKetThuc = dateTime.AddMinutes(120);

            // 2. Tìm các bàn bị trùng lịch
            // (Đã sửa logic: So sánh với ThoiGianBatDau ăn, chứ không phải ThoiGianDatHang)
            var conflictingBookingIds = await _context.DonHangs
                .Where(dh =>
                    // Chỉ check các đơn đang hoạt động
                    (dh.MaTrangThaiDonHang == "CHO_XAC_NHAN" || dh.MaTrangThaiDonHang == "DA_XAC_NHAN" || dh.MaTrangThaiDonHang == "CHO_THANH_TOAN") &&

                    dh.ThoiGianBatDau != null &&
                    // Logic trùng giờ: (StartA < EndB) && (EndA > StartB)
                    (gioBatDau < dh.ThoiGianKetThuc) &&
                    (gioKetThuc > dh.ThoiGianBatDau)
                )
                .Select(dh => dh.MaBan)
                .Distinct()
                .ToListAsync();

            // 3. Lấy danh sách bàn + Kèm thông tin Tầng
            var allTables = await _context.BanAns
                .Include(b => b.MaTrangThaiNavigation)
                .Include(b => b.MaTangNavigation) // <--- QUAN TRỌNG: Lấy thông tin Tầng
                .Select(ban => new
                {
                    ban.MaBan,
                    ban.TenBan,
                    ban.SucChua,
                    TrangThaiGoc = ban.MaTrangThaiNavigation.TenTrangThai,
                    // Lấy tên tầng (xử lý null nếu lỡ chưa gán)
                    TenTang = ban.MaTangNavigation != null ? ban.MaTangNavigation.TenTang : "Chưa phân tầng",
                    IsConflicting = conflictingBookingIds.Contains(ban.MaBan),
                    MaTrangThaiCode = ban.MaTrangThai // Lấy mã trạng thái để check Bảo trì
                })
                .ToListAsync();

            // 4. Map sang DTO trả về
            var result = allTables.Select(ban => new BanAnDTO
            {
                maBan = ban.MaBan,
                tenBan = ban.TenBan,
                sucChua = ban.SucChua,
                tenTang = ban.TenTang, // <--- Gán tên tầng vào DTO

                tenTrangThai =
                    (ban.MaTrangThaiCode == "TTBA004") ? "Đang bảo trì" : // Check mã cứng cho chắc
                    (ban.IsConflicting) ? "Đã đặt" :
                    (ban.SucChua < soNguoi) ? "Không đủ sức chứa" :
                    "Đang trống"
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

            // 1. Xác định khung giờ khách muốn đặt (Khách đến -> +2 tiếng)
            var gioBatDauKhachChon = dateTime;
            var gioKetThucKhachChon = dateTime.AddMinutes(120);

            // 2. Tìm các đơn hàng GÂY XUNG ĐỘT
            var conflictingOrders = await _context.DonHangs
                .Where(dh =>
                    // Lọc trạng thái: Chỉ tính các đơn đang "Sống"
                    (dh.MaTrangThaiDonHang == "CHO_XAC_NHAN" ||
                     dh.MaTrangThaiDonHang == "DA_XAC_NHAN" ||
                     dh.MaTrangThaiDonHang == "CHO_THANH_TOAN") &&

                    // Phải kiểm tra trên THỜI GIAN BẮT ĐẦU và KẾT THÚC của bữa ăn
                    dh.ThoiGianBatDau != null &&
                    dh.ThoiGianKetThuc != null &&

                    // Công thức trùng lịch chuẩn: (StartA < EndB) && (EndA > StartB)
                    // A: Khách đang chọn | B: Đơn trong DB
                    (gioBatDauKhachChon < dh.ThoiGianKetThuc) &&
                    (gioKetThucKhachChon > dh.ThoiGianBatDau)
                )
                .Select(dh => new { dh.MaBan, dh.MaKhachHang })
                .ToListAsync();

            // Tách danh sách ID bàn
            var banNguoiKhacDatIds = conflictingOrders
                .Where(o => o.MaKhachHang != maKhachHang)
                .Select(o => o.MaBan).Distinct().ToList();

            var banCuaTuiIds = conflictingOrders
                .Where(o => o.MaKhachHang == maKhachHang)
                .Select(o => o.MaBan).Distinct().ToList();

            // 3. Lấy danh sách bàn (Kèm Tầng)
            var allTables = await _context.BanAns
                .Include(b => b.MaTrangThaiNavigation)
                .Include(b => b.MaTangNavigation)
                .ToListAsync();

            // 4. Map kết quả ra DTO
            var result = allTables.Select(ban =>
            {
                string trangThaiHienThi = "Trong";

                // Ưu tiên 1: Bàn hỏng/Bảo trì (Check mã cứng)
                if (ban.MaTrangThai == "TTBA004")
                {
                    trangThaiHienThi = "BaoTri";
                }
                // Ưu tiên 2: Bàn Của Tui (Trùng lịch với chính mình)
                else if (banCuaTuiIds.Contains(ban.MaBan))
                {
                    trangThaiHienThi = "CuaTui";
                }
                // Ưu tiên 3: Bàn Người Khác (Trùng lịch với người khác)
                else if (banNguoiKhacDatIds.Contains(ban.MaBan))
                {
                    trangThaiHienThi = "DaDat";
                }
                // Ưu tiên 4: Bàn nhỏ
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
                    tenTrangThai = trangThaiHienThi,
                    tenTang = ban.MaTangNavigation != null ? ban.MaTangNavigation.TenTang : ""
                };
            }).ToList();

            return Ok(result);
        }
    }
}





