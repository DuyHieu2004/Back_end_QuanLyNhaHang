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

            var thoiGianBatDauKhachChon = dateTime;
            var thoiGianKetThucKhachChon = dateTime.AddMinutes(120);


            var conflictingBookingIds = await _context.DonHangs
                .Where(dh =>
                    dh.ThoiGianKetThuc == null &&
                    dh.ThoiGianDatHang != null &&
                    (thoiGianBatDauKhachChon < dh.ThoiGianDatHang.Value.AddMinutes(120)) &&
                    (thoiGianKetThucKhachChon > dh.ThoiGianDatHang.Value)
                )
                .Select(dh => dh.MaBan)
                .Distinct()
                .ToListAsync();


            var allTables = await _context.BanAns
                .Include(b => b.MaTrangThaiNavigation)
                .Include(b => b.MaTangNavigation)
                .Where(b => b.IsShow == true)
                .ToListAsync();

            // Load all tầng để map later if needed
            var allTangs = await _context.Tangs.ToDictionaryAsync(t => t.MaTang, t => t.TenTang);

            var result = allTables.Select(ban => {
                // Infer MaTang from MaBan if null (fallback)
                string maTang = ban.MaTang;
                if (string.IsNullOrEmpty(maTang) && ban.MaBan.StartsWith("B"))
                {
                    if (int.TryParse(ban.MaBan.Substring(1), out int banNum))
                    {
                        maTang = banNum <= 14 ? "T001" : banNum <= 27 ? "T002" : "T003";
                    }
                }

                // Get TenTang from navigation or dictionary
                string tenTang = ban.MaTangNavigation?.TenTang;
                if (string.IsNullOrEmpty(tenTang) && !string.IsNullOrEmpty(maTang) && allTangs.ContainsKey(maTang))
                {
                    tenTang = allTangs[maTang];
                }

                // Check if table is conflicting
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


        // API MỚI: Lấy danh sách bàn theo trạng thái chi tiết cho Khách Hàng
        [HttpGet("GetAvailableBanAns")]
        public async Task<IActionResult> GetAvailableBanAns(
            [FromQuery] DateTime dateTime,
            [FromQuery] int soNguoi,
            [FromQuery] string? maKhachHang) // Nhận thêm MaKhachHang để phân biệt chủ sở hữu
        {
            // Kiểm tra null để tránh lỗi server
            if (_context.BanAns == null || _context.DonHangs == null)
            {
                return NotFound("Cơ sở dữ liệu chưa sẵn sàng.");
            }

            // 1. Xác định khung giờ khách muốn đặt (Mặc định ăn 2 tiếng)
            var gioBatDau = dateTime;
            var gioKetThuc = dateTime.AddMinutes(120);

            // 2. Lấy danh sách các đơn hàng GÂY XUNG ĐỘT (Trùng giờ & Trạng thái đang hoạt động)
            // Dựa vào hình bạn gửi: Chỉ quan tâm đơn "CHO_XAC_NHAN" và "DA_XAC_NHAN"
            var conflictingOrders = await _context.DonHangs
                .Where(dh =>
                    // Lọc trạng thái đơn hàng: Chỉ lấy đơn Chờ duyệt hoặc Đã duyệt
                    (dh.MaTrangThaiDonHang == "CHO_XAC_NHAN" || dh.MaTrangThaiDonHang == "DA_XAC_NHAN") &&

                    dh.ThoiGianDatHang != null &&
                    // Logic trùng giờ: (StartA < EndB) && (EndA > StartB)
                    (gioBatDau < dh.ThoiGianDatHang.Value.AddMinutes(120)) &&
                    (gioKetThuc > dh.ThoiGianDatHang.Value)
                )
                .Select(dh => new { dh.MaBan, dh.MaKhachHang }) // Lấy thêm MaKhachHang để so sánh
                .ToListAsync();

            // Tách ra 2 danh sách ID bàn:
            // - Bàn do chính khách hàng này đặt (CuaTui)
            // - Bàn do người khác đặt (DaDat)
            var banNguoiKhacDatIds = conflictingOrders
                .Where(o => o.MaKhachHang != maKhachHang)
                .Select(o => o.MaBan).Distinct().ToList();

            var banCuaTuiIds = conflictingOrders
                .Where(o => o.MaKhachHang == maKhachHang)
                .Select(o => o.MaBan).Distinct().ToList();

            // 3. Lấy tất cả bàn từ DB và gán trạng thái hiển thị
            // (Include bảng TrangThaiBanAn để check mã TTBA004)
            var allTables = await _context.BanAns
                .Include(b => b.MaTrangThaiNavigation)
                .Include(b => b.MaTangNavigation)
                .Where(b => b.IsShow == true)
                .ToListAsync();

            var result = allTables.Select(ban =>
            {
                string trangThaiHienThi = "Trong";

                // --- QUY TẮC ƯU TIÊN ---

                // Ưu tiên 1: Check trạng thái CỨNG của bàn (Dựa vào hình 3: TTBA004 - Bảo trì)
                // Nếu bàn đang bảo trì thì chặn luôn, không quan tâm giờ giấc.
                if (ban.MaTrangThai == "TTBA004")
                {
                    trangThaiHienThi = "BaoTri";
                }
                // Ưu tiên 2: Check xem có phải bàn MÌNH đã đặt không?
                else if (banCuaTuiIds.Contains(ban.MaBan))
                {
                    trangThaiHienThi = "CuaTui"; // Trả về mã này để Flutter hiện màu Tím
                }
                // Ưu tiên 3: Check xem có phải bàn NGƯỜI KHÁC đã đặt không?
                else if (banNguoiKhacDatIds.Contains(ban.MaBan))
                {
                    trangThaiHienThi = "DaDat"; // Trả về mã này để Flutter hiện màu Đỏ/Xám
                }
                // Ưu tiên 4: Bàn trống nhưng KHÔNG ĐỦ CHỖ
                else if (ban.SucChua < soNguoi)
                {
                    trangThaiHienThi = "CanGhep"; // Trả về mã này để Flutter hiện màu Cam
                }
                // Ưu tiên 5: Bàn trống và đủ chỗ (Perfect)
                else
                {
                    // Lưu ý: Các trạng thái như TTBA005 (Dọn dẹp) hay TTBA002 (Đang phục vụ)
                    // tui coi là "Trong" vì đây là đặt bàn cho tương lai.
                    trangThaiHienThi = "Trong"; // Trả về mã này để Flutter hiện màu Xanh
                }

                return new BanAnDTO
                {
                    maBan = ban.MaBan,
                    tenBan = ban.TenBan,
                    sucChua = ban.SucChua,
                    maTang = ban.MaTang,
                    tenTang = ban.MaTangNavigation != null ? ban.MaTangNavigation.TenTang : null,
                    isShow = ban.IsShow,
                    // Trả về keyword để Flutter switch-case: "Trong", "CuaTui", "DaDat", "CanGhep", "BaoTri"
                    tenTrangThai = trangThaiHienThi
                };
            }).ToList();

            return Ok(result);
        }

    }





}
