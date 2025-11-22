using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyNhaHang.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QuanLyNhaHang.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MenuAPIController : ControllerBase
    {
        private readonly QLNhaHangContext _context;

        public MenuAPIController(QLNhaHangContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Lấy danh sách menu đang áp dụng (theo thời gian hiện tại)
        /// </summary>
        [HttpGet("DangApDung")]
        public async Task<IActionResult> GetMenuDangApDung([FromQuery] string? maLoaiMenu = null)
        {
            try
            {
                var now = DateTime.Now;
                var dayOfWeek = (int)now.DayOfWeek; // 0 = Chủ nhật, 1 = Thứ 2, ...

                var query = _context.Menus
                    .Include(m => m.MaLoaiMenuNavigation)
                    .Include(m => m.MaTrangThaiNavigation)
                    .Include(m => m.ChiTietMenus)
                        .ThenInclude(ct => ct.MaCongThucNavigation)
                            .ThenInclude(cta => cta.MaCtNavigation)
                                .ThenInclude(ctma => ctma.MaMonAnNavigation)
                                    .ThenInclude(ma => ma.HinhAnhMonAns)
                    .Include(m => m.ChiTietMenus)
                        .ThenInclude(ct => ct.MaCongThucNavigation)
                            .ThenInclude(cta => cta.MaPhienBanNavigation)
                    .Where(m => m.MaTrangThai == "DANG_AP_DUNG" &&
                                m.IsShow == true &&
                                (m.NgayBatDau == null || m.NgayBatDau <= now) &&
                                (m.NgayKetThuc == null || m.NgayKetThuc >= now));

                // Lọc theo loại menu nếu có
                if (!string.IsNullOrEmpty(maLoaiMenu))
                {
                    query = query.Where(m => m.MaLoaiMenu == maLoaiMenu);
                }

                var menus = await query
                    .OrderBy(m => m.ThuTu)
                    .ThenBy(m => m.TenMenu)
                    .ToListAsync();

                var result = menus.Select(m => new
                {
                    MaMenu = m.MaMenu,
                    TenMenu = m.TenMenu,
                    LoaiMenu = m.MaLoaiMenuNavigation?.TenLoaiMenu,
                    GiaMenu = m.GiaMenu,
                    GiaGoc = m.GiaGoc,
                    PhanTramGiamGia = m.GiaGoc > 0 
                        ? Math.Round(((m.GiaGoc.Value - m.GiaMenu) * 100m / m.GiaGoc.Value), 2) 
                        : 0,
                    MoTa = m.MoTa,
                    HinhAnh = m.HinhAnh,
                    NgayBatDau = m.NgayBatDau,
                    NgayKetThuc = m.NgayKetThuc,
                    ChiTietMenus = m.ChiTietMenus.Select(ct => new
                    {
                        SoLuong = ct.SoLuong,
                        GhiChu = ct.GhiChu,
                        MonAn = new
                        {
                            TenMonAn = ct.MaCongThucNavigation?.MaCtNavigation?.MaMonAnNavigation?.TenMonAn,
                            HinhAnh = ct.MaCongThucNavigation?.MaCtNavigation?.MaMonAnNavigation?.HinhAnhMonAns
                                .Select(h => h.UrlhinhAnh)
                                .FirstOrDefault(),
                            Gia = ct.MaCongThucNavigation?.Gia
                        }
                    }).ToList()
                }).ToList();

                return Ok(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi máy chủ: " + ex.Message });
            }
        }

        /// <summary>
        /// Lấy menu theo khung giờ (sáng/trưa/tối)
        /// </summary>
        [HttpGet("TheoKhungGio")]
        public async Task<IActionResult> GetMenuTheoKhungGio([FromQuery] string? khungGio = null)
        {
            try
            {
                var now = DateTime.Now;
                var hour = now.Hour;

                // Xác định khung giờ nếu không truyền vào
                if (string.IsNullOrEmpty(khungGio))
                {
                    if (hour >= 6 && hour < 11)
                        khungGio = "SANG";
                    else if (hour >= 11 && hour < 14)
                        khungGio = "TRUA";
                    else if (hour >= 14 && hour < 17)
                        khungGio = "CHIEU";
                    else
                        khungGio = "TOI";
                }

                // Chuẩn hóa khung giờ để tìm kiếm linh hoạt hơn
                var khungGioLower = khungGio.ToUpper();
                var searchTerms = new List<string> { khungGioLower };
                
                // Thêm các từ khóa tương đương
                if (khungGioLower == "SANG")
                {
                    searchTerms.AddRange(new[] { "SÁNG", "BUỔI SÁNG", "BREAKFAST", "ĂN SÁNG" });
                }
                else if (khungGioLower == "TRUA")
                {
                    searchTerms.AddRange(new[] { "TRƯA", "BUỔI TRƯA", "LUNCH", "ĂN TRƯA" });
                }
                else if (khungGioLower == "CHIEU")
                {
                    searchTerms.AddRange(new[] { "CHIỀU", "BUỔI CHIỀU", "AFTERNOON", "ĂN CHIỀU" });
                }
                else if (khungGioLower == "TOI")
                {
                    searchTerms.AddRange(new[] { "TỐI", "BUỔI TỐI", "DINNER", "ĂN TỐI", "ĐÊM" });
                }

                // Lấy menu có tên chứa khung giờ hoặc menu đặc biệt
                var menus = await _context.Menus
                    .Include(m => m.MaLoaiMenuNavigation)
                    .Include(m => m.ChiTietMenus)
                        .ThenInclude(ct => ct.MaCongThucNavigation)
                            .ThenInclude(cta => cta.MaCtNavigation)
                                .ThenInclude(ctma => ctma.MaMonAnNavigation)
                                    .ThenInclude(ma => ma.HinhAnhMonAns)
                    .Include(m => m.ChiTietMenus)
                        .ThenInclude(ct => ct.MaCongThucNavigation)
                            .ThenInclude(cta => cta.MaPhienBanNavigation)
                    .Where(m => m.MaTrangThai == "DANG_AP_DUNG" &&
                                m.IsShow == true &&
                                (m.NgayBatDau == null || m.NgayBatDau <= now) &&
                                (m.NgayKetThuc == null || m.NgayKetThuc >= now) &&
                                (searchTerms.Any(term => m.TenMenu.ToUpper().Contains(term)) || 
                                 (m.MoTa != null && searchTerms.Any(term => m.MoTa.ToUpper().Contains(term))) ||
                                 m.MaLoaiMenu == "LM003")) // Menu theo ngày
                    .OrderBy(m => m.ThuTu)
                    .ToListAsync();

                var result = menus.Select(m => new
                {
                    MaMenu = m.MaMenu,
                    TenMenu = m.TenMenu,
                    LoaiMenu = m.MaLoaiMenuNavigation?.TenLoaiMenu,
                    GiaMenu = m.GiaMenu,
                    GiaGoc = m.GiaGoc,
                    PhanTramGiamGia = m.GiaGoc > 0 
                        ? Math.Round(((m.GiaGoc.Value - m.GiaMenu) * 100m / m.GiaGoc.Value), 2) 
                        : 0,
                    MoTa = m.MoTa,
                    HinhAnh = m.HinhAnh,
                    ChiTietMenus = m.ChiTietMenus.Select(ct => new
                    {
                        SoLuong = ct.SoLuong,
                        GhiChu = ct.GhiChu,
                        MonAn = new
                        {
                            TenMonAn = ct.MaCongThucNavigation?.MaCtNavigation?.MaMonAnNavigation?.TenMonAn,
                            HinhAnh = ct.MaCongThucNavigation?.MaCtNavigation?.MaMonAnNavigation?.HinhAnhMonAns
                                .Select(h => h.UrlhinhAnh)
                                .FirstOrDefault(),
                            Gia = ct.MaCongThucNavigation?.Gia
                        }
                    }).ToList()
                }).ToList();

                return Ok(new { 
                    success = true, 
                    khungGio = khungGio,
                    data = result 
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi máy chủ: " + ex.Message });
            }
        }

        /// <summary>
        /// Lấy menu theo ngày lễ/đặc biệt
        /// </summary>
        [HttpGet("TheoNgayLe")]
        public async Task<IActionResult> GetMenuTheoNgayLe([FromQuery] DateTime? ngay = null)
        {
            try
            {
                var targetDate = ngay ?? DateTime.Now;
                var dayOfWeek = (int)targetDate.DayOfWeek;

                // Kiểm tra xem có phải ngày lễ không (có thể mở rộng thêm logic)
                bool isNgayLe = IsNgayLe(targetDate);

                var query = _context.Menus
                    .Include(m => m.MaLoaiMenuNavigation)
                    .Include(m => m.ChiTietMenus)
                        .ThenInclude(ct => ct.MaCongThucNavigation)
                            .ThenInclude(cta => cta.MaCtNavigation)
                                .ThenInclude(ctma => ctma.MaMonAnNavigation)
                                    .ThenInclude(ma => ma.HinhAnhMonAns)
                    .Where(m => m.MaTrangThai == "DANG_AP_DUNG" &&
                                m.IsShow == true &&
                                (m.NgayBatDau == null || m.NgayBatDau <= targetDate) &&
                                (m.NgayKetThuc == null || m.NgayKetThuc >= targetDate));

                // Nếu là ngày lễ, ưu tiên menu sự kiện
                if (isNgayLe)
                {
                    query = query.Where(m => m.MaLoaiMenu == "LM004" || m.MaLoaiMenu == "LM003");
                }

                var menus = await query
                    .OrderBy(m => m.ThuTu)
                    .ToListAsync();

                var result = menus.Select(m => new
                {
                    MaMenu = m.MaMenu,
                    TenMenu = m.TenMenu,
                    LoaiMenu = m.MaLoaiMenuNavigation?.TenLoaiMenu,
                    GiaMenu = m.GiaMenu,
                    GiaGoc = m.GiaGoc,
                    PhanTramGiamGia = m.GiaGoc > 0 
                        ? Math.Round(((m.GiaGoc.Value - m.GiaMenu) * 100m / m.GiaGoc.Value), 2) 
                        : 0,
                    MoTa = m.MoTa,
                    HinhAnh = m.HinhAnh,
                    NgayBatDau = m.NgayBatDau,
                    NgayKetThuc = m.NgayKetThuc,
                    ChiTietMenus = m.ChiTietMenus.Select(ct => new
                    {
                        SoLuong = ct.SoLuong,
                        GhiChu = ct.GhiChu,
                        MonAn = new
                        {
                            TenMonAn = ct.MaCongThucNavigation?.MaCtNavigation?.MaMonAnNavigation?.TenMonAn,
                            HinhAnh = ct.MaCongThucNavigation?.MaCtNavigation?.MaMonAnNavigation?.HinhAnhMonAns
                                .Select(h => h.UrlhinhAnh)
                                .FirstOrDefault(),
                            Gia = ct.MaCongThucNavigation?.Gia
                        }
                    }).ToList()
                }).ToList();

                return Ok(new { 
                    success = true, 
                    ngay = targetDate,
                    isNgayLe = isNgayLe,
                    data = result 
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi máy chủ: " + ex.Message });
            }
        }

        /// <summary>
        /// Lấy chi tiết menu theo mã
        /// </summary>
        [HttpGet("{maMenu}")]
        public async Task<IActionResult> GetChiTietMenu(string maMenu)
        {
            try
            {
                var menu = await _context.Menus
                    .Include(m => m.MaLoaiMenuNavigation)
                    .Include(m => m.MaTrangThaiNavigation)
                    .Include(m => m.ChiTietMenus)
                        .ThenInclude(ct => ct.MaCongThucNavigation)
                            .ThenInclude(cta => cta.MaCtNavigation)
                                .ThenInclude(ctma => ctma.MaMonAnNavigation)
                                    .ThenInclude(ma => ma.HinhAnhMonAns)
                    .Include(m => m.ChiTietMenus)
                        .ThenInclude(ct => ct.MaCongThucNavigation)
                            .ThenInclude(cta => cta.MaPhienBanNavigation)
                    .FirstOrDefaultAsync(m => m.MaMenu == maMenu);

                if (menu == null)
                {
                    return NotFound(new { message = "Không tìm thấy menu." });
                }

                var result = new
                {
                    MaMenu = menu.MaMenu,
                    TenMenu = menu.TenMenu,
                    LoaiMenu = menu.MaLoaiMenuNavigation?.TenLoaiMenu,
                    TrangThai = menu.MaTrangThaiNavigation?.TenTrangThai,
                    GiaMenu = menu.GiaMenu,
                    GiaGoc = menu.GiaGoc,
                    PhanTramGiamGia = menu.GiaGoc > 0 
                        ? Math.Round(((menu.GiaGoc.Value - menu.GiaMenu) * 100m / menu.GiaGoc.Value), 2) 
                        : 0,
                    MoTa = menu.MoTa,
                    HinhAnh = menu.HinhAnh,
                    NgayBatDau = menu.NgayBatDau,
                    NgayKetThuc = menu.NgayKetThuc,
                    ChiTietMenus = menu.ChiTietMenus.OrderBy(ct => ct.ThuTu).Select(ct => new
                    {
                        SoLuong = ct.SoLuong,
                        GhiChu = ct.GhiChu,
                        ThuTu = ct.ThuTu,
                        MonAn = new
                        {
                            MaMonAn = ct.MaCongThucNavigation?.MaCtNavigation?.MaMonAnNavigation?.MaMonAn,
                            TenMonAn = ct.MaCongThucNavigation?.MaCtNavigation?.MaMonAnNavigation?.TenMonAn,
                            HinhAnh = ct.MaCongThucNavigation?.MaCtNavigation?.MaMonAnNavigation?.HinhAnhMonAns
                                .Select(h => new { URL = h.UrlhinhAnh })
                                .ToList(),
                            Gia = ct.MaCongThucNavigation?.Gia,
                            PhienBan = ct.MaCongThucNavigation?.MaPhienBanNavigation?.TenPhienBan
                        }
                    }).ToList()
                };

                return Ok(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi máy chủ: " + ex.Message });
            }
        }

        /// <summary>
        /// Lấy món ăn theo danh mục với đầy đủ thông tin (E-menu)
        /// </summary>
        [HttpGet("MonAnTheoDanhMuc")]
        public async Task<IActionResult> GetMonAnTheoDanhMuc([FromQuery] string? maDanhMuc = null)
        {
            try
            {
                var query = _context.MonAns
                    .Include(m => m.HinhAnhMonAns)
                    .Include(m => m.MaDanhMucNavigation)
                    .Include(m => m.ChiTietMonAns)
                        .ThenInclude(ct => ct.CongThucNauAns)
                            .ThenInclude(cta => cta.MaPhienBanNavigation)
                    .Where(m => m.IsShow == true);

                // Lọc theo danh mục nếu có
                if (!string.IsNullOrEmpty(maDanhMuc) && maDanhMuc != "All" && maDanhMuc != "Tất cả")
                {
                    query = query.Where(m => m.MaDanhMuc == maDanhMuc);
                }

                var monAns = await query
                    .OrderBy(m => m.MaDanhMucNavigation != null ? m.MaDanhMucNavigation.TenDanhMuc : "")
                    .ThenBy(m => m.TenMonAn)
                    .ToListAsync();

                var result = monAns.Select(m =>
                {
                    // Lấy tất cả phiên bản (sizes) với giá
                    var phienBans = m.ChiTietMonAns
                        .SelectMany(ct => ct.CongThucNauAns)
                        .Where(cta => cta.MaPhienBanNavigation != null)
                        .GroupBy(cta => cta.MaPhienBan)
                        .Select(g => new
                        {
                            MaPhienBan = g.Key,
                            TenPhienBan = g.First().MaPhienBanNavigation.TenPhienBan,
                            Gia = g.First().Gia,
                            ThuTu = g.First().MaPhienBanNavigation.ThuTu ?? 0
                        })
                        .OrderBy(pb => pb.ThuTu)
                        .ToList();

                    return new
                    {
                        MaMonAn = m.MaMonAn,
                        TenMonAn = m.TenMonAn,
                        MaDanhMuc = m.MaDanhMuc,
                        TenDanhMuc = m.MaDanhMucNavigation?.TenDanhMuc,
                        MoTa = "", // Có thể thêm trường mô tả sau
                        HinhAnhs = m.HinhAnhMonAns.Select(h => h.UrlhinhAnh).ToList(),
                        PhienBans = phienBans.Select(pb => new
                        {
                            pb.MaPhienBan,
                            pb.TenPhienBan,
                            pb.Gia
                        }).ToList(),
                        GiaMin = phienBans.Any() ? phienBans.Min(pb => pb.Gia) : 0,
                        GiaMax = phienBans.Any() ? phienBans.Max(pb => pb.Gia) : 0,
                        ConHang = phienBans.Any() // Có thể kiểm tra trạng thái chi tiết hơn
                    };
                }).ToList();

                return Ok(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi máy chủ: " + ex.Message });
            }
        }

        /// <summary>
        /// Lấy menu hiện tại theo khung giờ tự động (API chính cho frontend)
        /// </summary>
        [HttpGet("HienTai")]
        public async Task<IActionResult> GetMenuHienTai()
        {
            try
            {
                var now = DateTime.Now;
                var hour = now.Hour;
                var dayOfWeek = (int)now.DayOfWeek;

                // Xác định khung giờ
                string khungGio;
                string tenKhungGio;
                if (hour >= 6 && hour < 11)
                {
                    khungGio = "SANG";
                    tenKhungGio = "Buổi sáng";
                }
                else if (hour >= 11 && hour < 14)
                {
                    khungGio = "TRUA";
                    tenKhungGio = "Buổi trưa";
                }
                else if (hour >= 14 && hour < 17)
                {
                    khungGio = "CHIEU";
                    tenKhungGio = "Buổi chiều";
                }
                else
                {
                    khungGio = "TOI";
                    tenKhungGio = "Buổi tối";
                }

                // Kiểm tra ngày lễ
                bool isNgayLe = IsNgayLe(now);

                // Lấy menu theo khung giờ
                var searchTerms = new List<string> { khungGio };
                if (khungGio == "SANG") searchTerms.AddRange(new[] { "SÁNG", "BUỔI SÁNG" });
                else if (khungGio == "TRUA") searchTerms.AddRange(new[] { "TRƯA", "BUỔI TRƯA" });
                else if (khungGio == "CHIEU") searchTerms.AddRange(new[] { "CHIỀU", "BUỔI CHIỀU" });
                else if (khungGio == "TOI") searchTerms.AddRange(new[] { "TỐI", "BUỔI TỐI", "ĐÊM" });

                var menus = await _context.Menus
                    .Include(m => m.MaLoaiMenuNavigation)
                    .Include(m => m.ChiTietMenus)
                        .ThenInclude(ct => ct.MaCongThucNavigation)
                            .ThenInclude(cta => cta.MaCtNavigation)
                                .ThenInclude(ctma => ctma.MaMonAnNavigation)
                                    .ThenInclude(ma => ma.HinhAnhMonAns)
                    .Where(m => m.MaTrangThai == "DANG_AP_DUNG" &&
                                m.IsShow == true &&
                                (m.NgayBatDau == null || m.NgayBatDau <= now) &&
                                (m.NgayKetThuc == null || m.NgayKetThuc >= now) &&
                                (searchTerms.Any(term => m.TenMenu.ToUpper().Contains(term)) ||
                                 (m.MoTa != null && searchTerms.Any(term => m.MoTa.ToUpper().Contains(term))) ||
                                 m.MaLoaiMenu == "LM003"))
                    .OrderBy(m => m.ThuTu)
                    .ToListAsync();

                // Nếu là ngày lễ, ưu tiên menu sự kiện
                if (isNgayLe)
                {
                    var menuNgayLe = menus.Where(m => m.MaLoaiMenu == "LM004").ToList();
                    if (menuNgayLe.Any())
                    {
                        menus = menuNgayLe.Concat(menus.Except(menuNgayLe)).ToList();
                    }
                }

                var result = menus.Select(m => new
                {
                    MaMenu = m.MaMenu,
                    TenMenu = m.TenMenu,
                    LoaiMenu = m.MaLoaiMenuNavigation?.TenLoaiMenu,
                    GiaMenu = m.GiaMenu,
                    GiaGoc = m.GiaGoc,
                    PhanTramGiamGia = m.GiaGoc > 0
                        ? Math.Round(((m.GiaGoc.Value - m.GiaMenu) * 100m / m.GiaGoc.Value), 2)
                        : 0,
                    MoTa = m.MoTa,
                    HinhAnh = m.HinhAnh,
                    ChiTietMenus = m.ChiTietMenus.Select(ct => new
                    {
                        SoLuong = ct.SoLuong,
                        GhiChu = ct.GhiChu,
                        MonAn = new
                        {
                            TenMonAn = ct.MaCongThucNavigation?.MaCtNavigation?.MaMonAnNavigation?.TenMonAn,
                            HinhAnh = ct.MaCongThucNavigation?.MaCtNavigation?.MaMonAnNavigation?.HinhAnhMonAns
                                .Select(h => h.UrlhinhAnh)
                                .FirstOrDefault(),
                            Gia = ct.MaCongThucNavigation?.Gia
                        }
                    }).ToList()
                }).ToList();

                // Tính thời gian còn lại của khung giờ hiện tại
                DateTime nextTimeSlot;
                if (hour >= 6 && hour < 11)
                    nextTimeSlot = new DateTime(now.Year, now.Month, now.Day, 11, 0, 0);
                else if (hour >= 11 && hour < 14)
                    nextTimeSlot = new DateTime(now.Year, now.Month, now.Day, 14, 0, 0);
                else if (hour >= 14 && hour < 17)
                    nextTimeSlot = new DateTime(now.Year, now.Month, now.Day, 17, 0, 0);
                else
                    nextTimeSlot = new DateTime(now.Year, now.Month, now.Day, 22, 0, 0).AddDays(1);

                var timeRemaining = (int)(nextTimeSlot - now).TotalSeconds;

                return Ok(new
                {
                    success = true,
                    khungGio = khungGio,
                    tenKhungGio = tenKhungGio,
                    isNgayLe = isNgayLe,
                    timeRemaining = timeRemaining,
                    nextTimeSlot = nextTimeSlot,
                    data = result
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi máy chủ: " + ex.Message });
            }
        }

        /// <summary>
        /// Kiểm tra xem ngày có phải ngày lễ không
        /// </summary>
        private bool IsNgayLe(DateTime date)
        {
            // Danh sách ngày lễ cố định (có thể mở rộng thêm)
            var ngayLe = new List<(int month, int day)>
            {
                (1, 1),   // Tết Dương lịch
                (4, 30),  // Ngày Giải phóng miền Nam
                (5, 1),   // Ngày Quốc tế Lao động
                (9, 2),   // Quốc khánh
                // Có thể thêm Tết Nguyên Đán, Giỗ Tổ Hùng Vương, v.v.
            };

            // Kiểm tra ngày lễ cố định
            if (ngayLe.Any(nl => nl.month == date.Month && nl.day == date.Day))
                return true;

            // Kiểm tra cuối tuần (thứ 7, chủ nhật)
            if (date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday)
                return true;

            return false;
        }
    }
}

