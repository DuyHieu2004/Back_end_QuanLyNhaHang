using Microsoft.AspNetCore.Authorization;
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
        private readonly IWebHostEnvironment _webHostEnvironment;

        public MenuAPIController(QLNhaHangContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
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

                var result = menus.Select(m => {
                    // Tính giá menu từ chi tiết nếu giá menu = 0 hoặc null
                    decimal giaMenu = m.GiaMenu;
                    if (giaMenu == 0 && m.ChiTietMenus != null && m.ChiTietMenus.Any())
                    {
                        giaMenu = m.ChiTietMenus
                            .Where(ct => ct.MaCongThucNavigation != null)
                            .Sum(ct => ct.MaCongThucNavigation.Gia * ct.SoLuong);
                    }
                    
                    return new
                    {
                        MaMenu = m.MaMenu,
                        TenMenu = m.TenMenu,
                        LoaiMenu = m.MaLoaiMenuNavigation?.TenLoaiMenu,
                        GiaMenu = giaMenu,
                        GiaGoc = m.GiaGoc,
                        PhanTramGiamGia = m.GiaGoc > 0 && giaMenu > 0
                            ? Math.Round(((m.GiaGoc.Value - giaMenu) * 100m / m.GiaGoc.Value), 2) 
                            : 0,
                        MoTa = m.MoTa,
                        HinhAnh = m.HinhAnh,
                        NgayBatDau = m.NgayBatDau,
                        NgayKetThuc = m.NgayKetThuc,
                        ChiTietMenus = (m.ChiTietMenus ?? new List<ChiTietMenu>()).Select(ct => new
                        {
                            SoLuong = ct.SoLuong,
                            GhiChu = ct.GhiChu,
                            MonAn = new
                            {
                                TenMonAn = ct.MaCongThucNavigation?.MaCtNavigation?.MaMonAnNavigation?.TenMonAn,
                                HinhAnh = ct.MaCongThucNavigation?.MaCtNavigation?.MaMonAnNavigation?.HinhAnhMonAns
                                    ?.Select(h => h.UrlhinhAnh)
                                    .FirstOrDefault(),
                                Gia = ct.MaCongThucNavigation?.Gia
                            }
                        }).ToList()
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

                // Chuẩn hóa khung giờ
                var khungGioUpper = khungGio.ToUpper();

                // Lấy menu theo khung giờ - sử dụng trường KhungGio
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
                                (m.KhungGio == khungGioUpper ||           // Menu thuộc khung giờ được yêu cầu
                                 m.KhungGio == null ||                      // Menu dùng cả ngày (không gán khung giờ)
                                 m.MaLoaiMenu == "LM003"))                 // Menu theo ngày (giữ nguyên logic cũ)
                    .OrderBy(m => m.ThuTu)
                    .ToListAsync();

                var result = menus.Select(m => {
                    // Tính giá menu từ chi tiết nếu giá menu = 0 hoặc null
                    decimal giaMenu = m.GiaMenu;
                    if (giaMenu == 0 && m.ChiTietMenus != null && m.ChiTietMenus.Any())
                    {
                        giaMenu = m.ChiTietMenus
                            .Where(ct => ct.MaCongThucNavigation != null)
                            .Sum(ct => ct.MaCongThucNavigation.Gia * ct.SoLuong);
                    }
                    
                    return new
                    {
                        MaMenu = m.MaMenu,
                        TenMenu = m.TenMenu,
                        LoaiMenu = m.MaLoaiMenuNavigation?.TenLoaiMenu,
                        GiaMenu = giaMenu,
                        GiaGoc = m.GiaGoc,
                        PhanTramGiamGia = m.GiaGoc > 0 && giaMenu > 0
                            ? Math.Round(((m.GiaGoc.Value - giaMenu) * 100m / m.GiaGoc.Value), 2) 
                            : 0,
                        MoTa = m.MoTa,
                        HinhAnh = m.HinhAnh,
                        KhungGio = m.KhungGio,
                        ChiTietMenus = (m.ChiTietMenus ?? new List<ChiTietMenu>()).Select(ct => new
                        {
                            SoLuong = ct.SoLuong,
                            GhiChu = ct.GhiChu,
                            MonAn = new
                            {
                                TenMonAn = ct.MaCongThucNavigation?.MaCtNavigation?.MaMonAnNavigation?.TenMonAn,
                                HinhAnh = ct.MaCongThucNavigation?.MaCtNavigation?.MaMonAnNavigation?.HinhAnhMonAns
                                    ?.Select(h => h.UrlhinhAnh)
                                    .FirstOrDefault(),
                                Gia = ct.MaCongThucNavigation?.Gia
                            }
                        }).ToList()
                    };
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

                var result = menus.Select(m => {
                    // Tính giá menu từ chi tiết nếu giá menu = 0 hoặc null
                    decimal giaMenu = m.GiaMenu;
                    if (giaMenu == 0 && m.ChiTietMenus != null && m.ChiTietMenus.Any())
                    {
                        giaMenu = m.ChiTietMenus
                            .Where(ct => ct.MaCongThucNavigation != null)
                            .Sum(ct => ct.MaCongThucNavigation.Gia * ct.SoLuong);
                    }
                    
                    return new
                    {
                        MaMenu = m.MaMenu,
                        TenMenu = m.TenMenu,
                        LoaiMenu = m.MaLoaiMenuNavigation?.TenLoaiMenu,
                        GiaMenu = giaMenu,
                        GiaGoc = m.GiaGoc,
                        PhanTramGiamGia = m.GiaGoc > 0 && giaMenu > 0
                            ? Math.Round(((m.GiaGoc.Value - giaMenu) * 100m / m.GiaGoc.Value), 2) 
                            : 0,
                        MoTa = m.MoTa,
                        HinhAnh = m.HinhAnh,
                        NgayBatDau = m.NgayBatDau,
                        NgayKetThuc = m.NgayKetThuc,
                        ChiTietMenus = (m.ChiTietMenus ?? new List<ChiTietMenu>()).Select(ct => new
                        {
                            SoLuong = ct.SoLuong,
                            GhiChu = ct.GhiChu,
                            MonAn = new
                            {
                                TenMonAn = ct.MaCongThucNavigation?.MaCtNavigation?.MaMonAnNavigation?.TenMonAn,
                                HinhAnh = ct.MaCongThucNavigation?.MaCtNavigation?.MaMonAnNavigation?.HinhAnhMonAns
                                    ?.Select(h => h.UrlhinhAnh)
                                    .FirstOrDefault(),
                                Gia = ct.MaCongThucNavigation?.Gia
                            }
                        }).ToList()
                    };
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

                // Tính giá menu từ chi tiết nếu giá menu = 0 hoặc null
                decimal giaMenu = menu.GiaMenu;
                if (giaMenu == 0 && menu.ChiTietMenus != null && menu.ChiTietMenus.Any())
                {
                    giaMenu = menu.ChiTietMenus
                        .Where(ct => ct.MaCongThucNavigation != null)
                        .Sum(ct => ct.MaCongThucNavigation.Gia * ct.SoLuong);
                }

                var result = new
                {
                    MaMenu = menu.MaMenu,
                    TenMenu = menu.TenMenu,
                    MaLoaiMenu = menu.MaLoaiMenu,
                    TenLoaiMenu = menu.MaLoaiMenuNavigation?.TenLoaiMenu,
                    MaTrangThai = menu.MaTrangThai,
                    TenTrangThai = menu.MaTrangThaiNavigation?.TenTrangThai,
                    TrangThai = menu.MaTrangThaiNavigation?.TenTrangThai,
                    LoaiMenu = menu.MaLoaiMenuNavigation?.TenLoaiMenu,
                    GiaMenu = giaMenu,
                    GiaGoc = menu.GiaGoc,
                    PhanTramGiamGia = menu.GiaGoc > 0 && giaMenu > 0
                        ? Math.Round(((menu.GiaGoc.Value - giaMenu) * 100m / menu.GiaGoc.Value), 2) 
                        : 0,
                    MoTa = menu.MoTa,
                    HinhAnh = menu.HinhAnh,
                    NgayBatDau = menu.NgayBatDau,
                    NgayKetThuc = menu.NgayKetThuc,
                    IsShow = menu.IsShow,
                    ThuTu = menu.ThuTu,
                    KhungGio = menu.KhungGio,
                    ChiTietMenus = menu.ChiTietMenus.OrderBy(ct => ct.ThuTu).Select(ct => new
                    {
                        MaCongThuc = ct.MaCongThuc,
                        MaPhienBan = ct.MaCongThucNavigation?.MaPhienBan,
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
                            PhienBan = ct.MaCongThucNavigation?.MaPhienBanNavigation?.TenPhienBan,
                            MaPhienBan = ct.MaCongThucNavigation?.MaPhienBan
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

                // Lấy menu theo khung giờ - sử dụng trường KhungGio
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
                                (m.KhungGio == khungGio ||           // Menu thuộc khung giờ hiện tại
                                 m.KhungGio == null ||                // Menu dùng cả ngày (không gán khung giờ)
                                 m.MaLoaiMenu == "LM003"))           // Menu theo ngày (giữ nguyên logic cũ)
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

                var result = menus.Select(m => {
                    // Tính giá menu từ chi tiết nếu giá menu = 0 hoặc null
                    decimal giaMenu = m.GiaMenu;
                    if (giaMenu == 0 && m.ChiTietMenus != null && m.ChiTietMenus.Any())
                    {
                        giaMenu = m.ChiTietMenus
                            .Where(ct => ct.MaCongThucNavigation != null)
                            .Sum(ct => ct.MaCongThucNavigation.Gia * ct.SoLuong);
                    }
                    
                    return new
                    {
                        MaMenu = m.MaMenu,
                        TenMenu = m.TenMenu,
                        LoaiMenu = m.MaLoaiMenuNavigation?.TenLoaiMenu,
                        GiaMenu = giaMenu,
                        GiaGoc = m.GiaGoc,
                        PhanTramGiamGia = m.GiaGoc > 0 && giaMenu > 0
                            ? Math.Round(((m.GiaGoc.Value - giaMenu) * 100m / m.GiaGoc.Value), 2) 
                            : 0,
                        MoTa = m.MoTa,
                        HinhAnh = m.HinhAnh,
                        KhungGio = m.KhungGio,
                        ChiTietMenus = (m.ChiTietMenus ?? new List<ChiTietMenu>()).Select(ct => new
                        {
                            SoLuong = ct.SoLuong,
                            GhiChu = ct.GhiChu,
                            MonAn = new
                            {
                                TenMonAn = ct.MaCongThucNavigation?.MaCtNavigation?.MaMonAnNavigation?.TenMonAn,
                                HinhAnh = ct.MaCongThucNavigation?.MaCtNavigation?.MaMonAnNavigation?.HinhAnhMonAns
                                    ?.Select(h => h.UrlhinhAnh)
                                    .FirstOrDefault(),
                                Gia = ct.MaCongThucNavigation?.Gia
                            }
                        }).ToList()
                    };
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
        /// Lấy tất cả menu (dành cho admin - không filter theo thời gian)
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAllMenus([FromQuery] string? maLoaiMenu = null, [FromQuery] string? searchString = null)
        {
            try
            {
                var query = _context.Menus
                    .Include(m => m.MaLoaiMenuNavigation)
                    .Include(m => m.MaTrangThaiNavigation)
                    .Include(m => m.ChiTietMenus)
                        .ThenInclude(ct => ct.MaCongThucNavigation)
                            .ThenInclude(cta => cta.MaCtNavigation)
                                .ThenInclude(ctma => ctma.MaMonAnNavigation)
                    .AsQueryable();

                // Lọc theo loại menu
                if (!string.IsNullOrEmpty(maLoaiMenu))
                {
                    query = query.Where(m => m.MaLoaiMenu == maLoaiMenu);
                }

                // Tìm kiếm theo tên menu
                if (!string.IsNullOrEmpty(searchString))
                {
                    query = query.Where(m => m.TenMenu.Contains(searchString) || 
                                            (m.MoTa != null && m.MoTa.Contains(searchString)));
                }

                var menus = await query
                    .OrderBy(m => m.ThuTu)
                    .ThenBy(m => m.TenMenu)
                    .ToListAsync();

                var result = menus.Select(m => new
                {
                    MaMenu = m.MaMenu,
                    TenMenu = m.TenMenu,
                    MaLoaiMenu = m.MaLoaiMenu,
                    TenLoaiMenu = m.MaLoaiMenuNavigation?.TenLoaiMenu,
                    MaTrangThai = m.MaTrangThai,
                    TenTrangThai = m.MaTrangThaiNavigation?.TenTrangThai,
                    GiaMenu = m.GiaMenu,
                    GiaGoc = m.GiaGoc,
                    PhanTramGiamGia = m.GiaGoc > 0 
                        ? Math.Round(((m.GiaGoc.Value - m.GiaMenu) * 100m / m.GiaGoc.Value), 2) 
                        : 0,
                    MoTa = m.MoTa,
                    HinhAnh = m.HinhAnh,
                    NgayBatDau = m.NgayBatDau,
                    NgayKetThuc = m.NgayKetThuc,
                    IsShow = m.IsShow,
                    ThuTu = m.ThuTu,
                    NgayTao = m.NgayTao,
                    NgayCapNhat = m.NgayCapNhat,
                    SoLuongMonAn = m.ChiTietMenus.Count
                }).ToList();

                return Ok(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi máy chủ: " + ex.Message });
            }
        }

        /// <summary>
        /// Tạo menu mới
        /// </summary>
        [HttpPost]
        // [Authorize(Roles = "NhanVien,QuanLy")] // Chỉ nhân viên và quản lý mới được tạo menu
        public async Task<IActionResult> CreateMenu([FromBody] CreateMenuDto dto)
        {
            try
            {
                // Validation theo schema SQL
                if (string.IsNullOrWhiteSpace(dto.TenMenu))
                {
                    return BadRequest(new { message = "Tên menu không được để trống." });
                }

                if (dto.GiaMenu <= 0)
                {
                    return BadRequest(new { message = "Giá menu phải lớn hơn 0." });
                }

                // Kiểm tra MaLoaiMenu (NOT NULL trong schema)
                string maLoaiMenu = dto.MaLoaiMenu ?? "LM001";
                var loaiMenuExists = await _context.LoaiMenus.AnyAsync(lm => lm.MaLoaiMenu == maLoaiMenu);
                if (!loaiMenuExists)
                {
                    return BadRequest(new { message = $"Không tìm thấy loại menu với mã: {maLoaiMenu}" });
                }

                // Kiểm tra MaTrangThai (NOT NULL trong schema)
                string maTrangThai = dto.MaTrangThai ?? "CHUA_AP_DUNG";
                var trangThaiExists = await _context.TrangThaiMenus.AnyAsync(tt => tt.MaTrangThai == maTrangThai);
                if (!trangThaiExists)
                {
                    return BadRequest(new { message = $"Không tìm thấy trạng thái menu với mã: {maTrangThai}" });
                }

                // Validation chi tiết menu
                if (dto.ChiTietMenus != null && dto.ChiTietMenus.Any())
                {
                    foreach (var ct in dto.ChiTietMenus)
                    {
                        if (string.IsNullOrWhiteSpace(ct.MaCongThuc))
                        {
                            return BadRequest(new { message = "Mã công thức không được để trống trong chi tiết menu." });
                        }

                        if (ct.SoLuong <= 0)
                        {
                            return BadRequest(new { message = "Số lượng phải lớn hơn 0 (theo CHECK constraint trong database)." });
                        }

                        // Kiểm tra công thức có tồn tại không
                        var congThuc = await _context.CongThucNauAns
                            .FirstOrDefaultAsync(c => c.MaCongThuc == ct.MaCongThuc);
                        
                        if (congThuc == null)
                        {
                            return BadRequest(new { message = $"Không tìm thấy công thức với mã: {ct.MaCongThuc}" });
                        }
                    }
                }

                // Tạo mã menu tự động nếu không có
                string maMenu = dto.MaMenu;
                if (string.IsNullOrWhiteSpace(maMenu))
                {
                    var lastMenu = await _context.Menus
                        .OrderByDescending(m => m.MaMenu)
                        .FirstOrDefaultAsync();
                    
                    if (lastMenu != null && lastMenu.MaMenu.StartsWith("MENU"))
                    {
                        var lastNumber = int.TryParse(lastMenu.MaMenu.Substring(4), out int num) ? num : 0;
                        maMenu = $"MENU{(lastNumber + 1):D3}";
                    }
                    else
                    {
                        maMenu = "MENU001";
                    }
                }

                // Kiểm tra mã menu đã tồn tại chưa
                if (await _context.Menus.AnyAsync(m => m.MaMenu == maMenu))
                {
                    return BadRequest(new { message = "Mã menu đã tồn tại." });
                }

                // Validate KhungGio nếu có
                string? khungGio = null;
                if (!string.IsNullOrWhiteSpace(dto.KhungGio))
                {
                    var khungGioUpper = dto.KhungGio.Trim().ToUpper();
                    if (khungGioUpper == "SANG" || khungGioUpper == "TRUA" || 
                        khungGioUpper == "CHIEU" || khungGioUpper == "TOI")
                    {
                        khungGio = khungGioUpper;
                    }
                    else
                    {
                        return BadRequest(new { message = "KhungGio không hợp lệ. Chỉ chấp nhận: SANG, TRUA, CHIEU, TOI hoặc để trống (dùng cả ngày)." });
                    }
                }

                // Tạo menu mới
                var menu = new Menu
                {
                    MaMenu = maMenu,
                    TenMenu = dto.TenMenu.Trim(),
                    MaLoaiMenu = maLoaiMenu,
                    MaTrangThai = maTrangThai,
                    GiaMenu = dto.GiaMenu,
                    GiaGoc = dto.GiaGoc > 0 ? dto.GiaGoc : null, // Chỉ lưu nếu > 0
                    MoTa = !string.IsNullOrWhiteSpace(dto.MoTa) ? dto.MoTa.Trim() : null,
                    HinhAnh = !string.IsNullOrWhiteSpace(dto.HinhAnh) ? dto.HinhAnh.Trim() : null,
                    NgayBatDau = dto.NgayBatDau,
                    NgayKetThuc = dto.NgayKetThuc,
                    IsShow = dto.IsShow ?? true, // Default 1 theo schema
                    ThuTu = dto.ThuTu,
                    KhungGio = khungGio,
                    NgayTao = DateTime.Now, // Default GETDATE() theo schema
                    NgayCapNhat = null
                };

                _context.Menus.Add(menu);

                // Thêm chi tiết menu
                if (dto.ChiTietMenus != null && dto.ChiTietMenus.Any())
                {
                    int thuTuCounter = 1;
                    foreach (var ct in dto.ChiTietMenus)
                    {
                        var chiTietMenu = new ChiTietMenu
                        {
                            MaMenu = menu.MaMenu,
                            MaCongThuc = ct.MaCongThuc,
                            SoLuong = ct.SoLuong, // Default 1 theo schema, nhưng đã validate > 0
                            GhiChu = !string.IsNullOrWhiteSpace(ct.GhiChu) ? ct.GhiChu.Trim() : null,
                            ThuTu = ct.ThuTu ?? thuTuCounter
                        };

                        _context.ChiTietMenus.Add(chiTietMenu);
                        thuTuCounter++;
                    }
                }

                await _context.SaveChangesAsync();

                return Ok(new { success = true, message = "Tạo menu thành công.", data = new { MaMenu = menu.MaMenu } });
            }
            catch (DbUpdateException dbEx)
            {
                // Xử lý lỗi database constraint
                return StatusCode(500, new { message = "Lỗi cơ sở dữ liệu: " + dbEx.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi máy chủ: " + ex.Message });
            }
        }

        /// <summary>
        /// Cập nhật menu
        /// </summary>
        [HttpPut("{maMenu}")]
        // [Authorize(Roles = "NhanVien,QuanLy")] // Chỉ nhân viên và quản lý mới được cập nhật menu
        public async Task<IActionResult> UpdateMenu(string maMenu, [FromBody] UpdateMenuDto dto)
        {
            try
            {
                var menu = await _context.Menus
                    .Include(m => m.ChiTietMenus)
                    .FirstOrDefaultAsync(m => m.MaMenu == maMenu);

                if (menu == null)
                {
                    return NotFound(new { message = "Không tìm thấy menu." });
                }

                // Validation theo schema SQL
                if (!string.IsNullOrWhiteSpace(dto.TenMenu))
                    menu.TenMenu = dto.TenMenu.Trim();
                
                if (!string.IsNullOrWhiteSpace(dto.MaLoaiMenu))
                {
                    // Kiểm tra MaLoaiMenu tồn tại
                    var loaiMenuExists = await _context.LoaiMenus.AnyAsync(lm => lm.MaLoaiMenu == dto.MaLoaiMenu);
                    if (!loaiMenuExists)
                    {
                        return BadRequest(new { message = $"Không tìm thấy loại menu với mã: {dto.MaLoaiMenu}" });
                    }
                    menu.MaLoaiMenu = dto.MaLoaiMenu;
                }
                
                if (!string.IsNullOrWhiteSpace(dto.MaTrangThai))
                {
                    // Kiểm tra MaTrangThai tồn tại
                    var trangThaiExists = await _context.TrangThaiMenus.AnyAsync(tt => tt.MaTrangThai == dto.MaTrangThai);
                    if (!trangThaiExists)
                    {
                        return BadRequest(new { message = $"Không tìm thấy trạng thái menu với mã: {dto.MaTrangThai}" });
                    }
                    menu.MaTrangThai = dto.MaTrangThai;
                }
                
                if (dto.GiaMenu.HasValue)
                {
                    if (dto.GiaMenu.Value <= 0)
                    {
                        return BadRequest(new { message = "Giá menu phải lớn hơn 0." });
                    }
                    menu.GiaMenu = dto.GiaMenu.Value;
                }
                
                if (dto.GiaGoc.HasValue)
                    menu.GiaGoc = dto.GiaGoc.Value > 0 ? dto.GiaGoc.Value : null;
                
                if (dto.MoTa != null)
                    menu.MoTa = !string.IsNullOrWhiteSpace(dto.MoTa) ? dto.MoTa.Trim() : null;
                
                if (dto.HinhAnh != null)
                    menu.HinhAnh = !string.IsNullOrWhiteSpace(dto.HinhAnh) ? dto.HinhAnh.Trim() : null;
                
                if (dto.NgayBatDau.HasValue)
                    menu.NgayBatDau = dto.NgayBatDau;
                
                if (dto.NgayKetThuc.HasValue)
                    menu.NgayKetThuc = dto.NgayKetThuc;
                
                if (dto.IsShow.HasValue)
                    menu.IsShow = dto.IsShow.Value;
                
                if (dto.ThuTu.HasValue)
                    menu.ThuTu = dto.ThuTu;

                // Cập nhật KhungGio nếu có
                if (dto.KhungGio != null)
                {
                    if (string.IsNullOrWhiteSpace(dto.KhungGio))
                    {
                        // Nếu là chuỗi rỗng, set về null (menu dùng cả ngày)
                        menu.KhungGio = null;
                    }
                    else
                    {
                        var khungGioUpper = dto.KhungGio.Trim().ToUpper();
                        if (khungGioUpper == "SANG" || khungGioUpper == "TRUA" || 
                            khungGioUpper == "CHIEU" || khungGioUpper == "TOI")
                        {
                            menu.KhungGio = khungGioUpper;
                        }
                        else
                        {
                            return BadRequest(new { message = "KhungGio không hợp lệ. Chỉ chấp nhận: SANG, TRUA, CHIEU, TOI hoặc để trống (dùng cả ngày)." });
                        }
                    }
                }

                menu.NgayCapNhat = DateTime.Now; // Cập nhật thời gian sửa

                // Cập nhật chi tiết menu nếu có
                if (dto.ChiTietMenus != null)
                {
                    // Validation chi tiết menu
                    foreach (var ct in dto.ChiTietMenus)
                    {
                        if (string.IsNullOrWhiteSpace(ct.MaCongThuc))
                        {
                            return BadRequest(new { message = "Mã công thức không được để trống trong chi tiết menu." });
                        }

                        if (ct.SoLuong <= 0)
                        {
                            return BadRequest(new { message = "Số lượng phải lớn hơn 0 (theo CHECK constraint trong database)." });
                        }

                        // Kiểm tra công thức có tồn tại không
                        var congThuc = await _context.CongThucNauAns
                            .FirstOrDefaultAsync(c => c.MaCongThuc == ct.MaCongThuc);
                        
                        if (congThuc == null)
                        {
                            return BadRequest(new { message = $"Không tìm thấy công thức với mã: {ct.MaCongThuc}" });
                        }
                    }

                    // Xóa chi tiết cũ (CASCADE DELETE sẽ tự động xóa, nhưng ta xóa thủ công để rõ ràng)
                    _context.ChiTietMenus.RemoveRange(menu.ChiTietMenus);

                    // Thêm chi tiết mới
                    int thuTuCounter = 1;
                    foreach (var ct in dto.ChiTietMenus)
                    {
                        var chiTietMenu = new ChiTietMenu
                        {
                            MaMenu = menu.MaMenu,
                            MaCongThuc = ct.MaCongThuc,
                            SoLuong = ct.SoLuong,
                            GhiChu = !string.IsNullOrWhiteSpace(ct.GhiChu) ? ct.GhiChu.Trim() : null,
                            ThuTu = ct.ThuTu ?? thuTuCounter
                        };

                        _context.ChiTietMenus.Add(chiTietMenu);
                        thuTuCounter++;
                    }
                }

                await _context.SaveChangesAsync();

                return Ok(new { success = true, message = "Cập nhật menu thành công." });
            }
            catch (DbUpdateException dbEx)
            {
                // Xử lý lỗi database constraint
                return StatusCode(500, new { message = "Lỗi cơ sở dữ liệu: " + dbEx.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi máy chủ: " + ex.Message });
            }
        }

        /// <summary>
        /// Xóa menu
        /// </summary>
        [HttpDelete("{maMenu}")]
        public async Task<IActionResult> DeleteMenu(string maMenu)
        {
            try
            {
                var menu = await _context.Menus
                    .Include(m => m.ChiTietMenus)
                    .FirstOrDefaultAsync(m => m.MaMenu == maMenu);

                if (menu == null)
                {
                    return NotFound(new { message = "Không tìm thấy menu." });
                }

                // Xóa chi tiết menu (cascade delete sẽ tự động xóa)
                _context.ChiTietMenus.RemoveRange(menu.ChiTietMenus);
                
                // Xóa menu
                _context.Menus.Remove(menu);

                await _context.SaveChangesAsync();

                return Ok(new { success = true, message = "Xóa menu thành công." });
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

        /// <summary>
        /// Upload hình ảnh cho menu
        /// </summary>
        [HttpPost("upload-image")]
        public async Task<IActionResult> UploadImage(IFormFile file, [FromQuery] string? maMenu = null)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest(new { message = "Không có file được chọn." });
            }

            // Kiểm tra định dạng file
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
            var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(fileExtension))
            {
                return BadRequest(new { message = "Định dạng file không hợp lệ. Chỉ chấp nhận: jpg, jpeg, png, gif, webp" });
            }

            // Kiểm tra kích thước file (tối đa 10MB)
            if (file.Length > 10 * 1024 * 1024)
            {
                return BadRequest(new { message = "File quá lớn. Kích thước tối đa là 10MB." });
            }

            try
            {
                string webRootPath = _webHostEnvironment.WebRootPath;
                string uploadsFolder;
                string imageUrl;

                // Nếu có maMenu, upload vào thư mục của menu
                if (!string.IsNullOrEmpty(maMenu))
                {
                    uploadsFolder = Path.Combine(webRootPath, "images", "menus", maMenu);
                    Directory.CreateDirectory(uploadsFolder);

                    // Tạo tên file với số thứ tự
                    int imageOrder = Directory.GetFiles(uploadsFolder).Length + 1;
                    string fileName = Path.GetFileNameWithoutExtension(file.FileName);
                    string uniqueFileName = $"{fileName}_{imageOrder}{fileExtension}";
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    // Lưu file
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(fileStream);
                    }

                    imageUrl = $"images/menus/{maMenu}/{uniqueFileName}";
                }
                else
                {
                    // Upload vào thư mục temp nếu chưa có maMenu
                    uploadsFolder = Path.Combine(webRootPath, "images", "menus", "temp");
                    Directory.CreateDirectory(uploadsFolder);

                    // Tạo tên file unique
                    string fileName = Path.GetFileNameWithoutExtension(file.FileName);
                    string uniqueFileName = $"{fileName}_{DateTime.Now:yyyyMMddHHmmss}{fileExtension}";
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    // Lưu file
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(fileStream);
                    }

                    imageUrl = $"images/menus/temp/{uniqueFileName}";
                }

                return Ok(new { 
                    message = "Upload ảnh thành công!",
                    url = imageUrl 
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi khi upload ảnh: " + ex.Message });
            }
        }
    }

    // DTOs cho Menu
    public class CreateMenuDto
    {
        public string? MaMenu { get; set; }
        public string TenMenu { get; set; } = null!;
        public string? MaLoaiMenu { get; set; }
        public string? MaTrangThai { get; set; }
        public decimal GiaMenu { get; set; }
        public decimal? GiaGoc { get; set; }
        public string? MoTa { get; set; }
        public string? HinhAnh { get; set; }
        public DateTime? NgayBatDau { get; set; }
        public DateTime? NgayKetThuc { get; set; }
        public bool? IsShow { get; set; }
        public int? ThuTu { get; set; }
        public string? KhungGio { get; set; }
        public List<ChiTietMenuDto>? ChiTietMenus { get; set; }
    }

    public class UpdateMenuDto
    {
        public string? TenMenu { get; set; }
        public string? MaLoaiMenu { get; set; }
        public string? MaTrangThai { get; set; }
        public decimal? GiaMenu { get; set; }
        public decimal? GiaGoc { get; set; }
        public string? MoTa { get; set; }
        public string? HinhAnh { get; set; }
        public DateTime? NgayBatDau { get; set; }
        public DateTime? NgayKetThuc { get; set; }
        public bool? IsShow { get; set; }
        public int? ThuTu { get; set; }
        public string? KhungGio { get; set; }
        public List<ChiTietMenuDto>? ChiTietMenus { get; set; }
    }

    public class ChiTietMenuDto
    {
        public string MaCongThuc { get; set; } = null!;
        public int SoLuong { get; set; }
        public string? GhiChu { get; set; }
        public int? ThuTu { get; set; }
    }
}


