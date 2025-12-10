using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyNhaHang.Models;
using QuanLyNhaHang.Models.DTO;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace QuanLyNhaHang.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InventoryAPIController : ControllerBase
    {
        private readonly QLNhaHangContext _context;

        public InventoryAPIController(QLNhaHangContext context)
        {
            _context = context;
        }

        [HttpGet("ingredients")]
        public async Task<IActionResult> GetIngredients()
        {
            var ingredients = await _context.NguyenLieus
                .Select(n => new
                {
                    n.MaNguyenLieu,
                    n.TenNguyenLieu,
                    n.DonViTinh,
                    Stock = n.SoLuongTonKho,
                   // MinStock = n.MinStock, // Lấy MinStock từ DB
                    n.GiaBan
                })
                .ToListAsync();

            return Ok(ingredients);
        }

        [HttpPost("ingredients")]
        public async Task<IActionResult> CreateIngredient([FromBody] CreateNguyenLieuDTO dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                // Tạo mã nguyên liệu mới
                var maNguyenLieu = "NL" + DateTime.Now.ToString("yyMMddHHmmss");

                // Kiểm tra tên nguyên liệu đã tồn tại chưa
                var existing = await _context.NguyenLieus
                    .FirstOrDefaultAsync(n => n.TenNguyenLieu.ToLower() == dto.TenNguyenLieu.ToLower());

                if (existing != null)
                {
                    return BadRequest(new { message = $"Nguyên liệu \"{dto.TenNguyenLieu}\" đã tồn tại trong hệ thống." });
                }

                var nguyenLieu = new NguyenLieu
                {
                    MaNguyenLieu = maNguyenLieu,
                    TenNguyenLieu = dto.TenNguyenLieu,
                    DonViTinh = dto.DonViTinh,
                    SoLuongTonKho = dto.SoLuongTonKho
                };

                _context.NguyenLieus.Add(nguyenLieu);
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    message = "Thêm nguyên liệu thành công!",
                    ingredient = new
                    {
                        maNguyenLieu = nguyenLieu.MaNguyenLieu,
                        tenNguyenLieu = nguyenLieu.TenNguyenLieu,
                        donViTinh = nguyenLieu.DonViTinh,
                        soLuongTonKho = nguyenLieu.SoLuongTonKho
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi khi thêm nguyên liệu: " + ex.Message });
            }
        }

        [HttpGet("ingredients/{maNguyenLieu}")]
        public async Task<IActionResult> GetIngredient(string maNguyenLieu)
        {
            var ingredient = await _context.NguyenLieus
                .FirstOrDefaultAsync(n => n.MaNguyenLieu == maNguyenLieu);

            if (ingredient == null)
            {
                return NotFound(new { message = "Không tìm thấy nguyên liệu." });
            }

            return Ok(ingredient);
        }

        public class NhapKhoDTO
        {
            [Required]
            public string MaNhanVien { get; set; } = null!;

            [Required]
            public string MaNhaCungCap { get; set; } = null!; // Bắt buộc chọn NCC

            public string MaTrangThai { get; set; } // 0: Nháp, 1: Gửi, 2: Hoàn tất

            public List<ChiTietNhapKhoDTO> ChiTiet { get; set; } = new();
        }

        public class ChiTietNhapKhoDTO
        {
            [Required]
            public string MaCungUng { get; set; } = null!; // Mã Cung Ứng (chứ ko phải mã NL)
            public int SoLuong { get; set; }
            public decimal GiaNhap { get; set; } // Có thể = 0 nếu là nháp
        }


        [HttpGet("transactions")]
        public async Task<IActionResult> GetTransactions([FromQuery] string? trangThai)
        {
            var query = _context.NhapHangs
                .Include(n => n.MaNhaCungCapNavigation) // Lấy tên NCC
                .Include(n => n.MaNhanVienNavigation)   // Lấy tên Nhân viên
                .Include(n => n.MaTrangThaiNavigation)  // Lấy tên Trạng thái (Tiếng Việt)
                .Include(n => n.ChiTietNhapHangs)
                .AsQueryable();

            // Filter theo mã trạng thái (VD: "MOI_TAO", "DA_HOAN_TAT")
            if (!string.IsNullOrEmpty(trangThai))
            {
                query = query.Where(n => n.MaTrangThai == trangThai);
            }

            var transactions = await query
                .OrderByDescending(n => n.NgayLapPhieu)
                .Select(n => new
                {
                    n.MaNhapHang,
                    NgayLap = n.NgayLapPhieu,
                    NgayNhap = n.NgayNhapHang, // Có thể null nếu chưa nhập kho

                    // Xử lý null an toàn cho tên NCC (phòng trường hợp dữ liệu cũ chưa có NCC)
                    TenNhaCungCap = n.MaNhaCungCapNavigation != null ? n.MaNhaCungCapNavigation.TenNhaCungCap : "Chưa xác định",

                    TenNhanVien = n.MaNhanVienNavigation.HoTen,
                    TongTien = n.ChiTietNhapHangs.Sum(ct => ct.SoLuong * ct.GiaNhap),

                    // Trả về cả Mã và Tên hiển thị
                    MaTrangThai = n.MaTrangThai,
                    TenTrangThai = n.MaTrangThaiNavigation != null ? n.MaTrangThaiNavigation.TenTrangThai : n.MaTrangThai
                })
                .ToListAsync();

            return Ok(transactions);
        }

        [HttpGet("ingredients-by-supplier/{maNCC}")]
        public async Task<IActionResult> GetIngredientsBySupplier(string maNCC)
        {
            // Join bảng CungUng để lấy nguyên liệu ông này bán
            var list = await _context.CungUngs
                .Where(cu => cu.MaNhaCungCap == maNCC)
                .Include(cu => cu.MaNguyenLieuNavigation)
                .Select(cu => new
                {
                    MaCungUng = cu.MaCungUng, // Quan trọng: Dùng mã này để lưu chi tiết
                    MaNguyenLieu = cu.MaNguyenLieu,
                    TenNguyenLieu = cu.MaNguyenLieuNavigation.TenNguyenLieu,
                    DonViTinh = cu.MaNguyenLieuNavigation.DonViTinh,
                    // Giá bán hiện tại của nguyên liệu (để tham khảo)
                    GiaGoiY = cu.MaNguyenLieuNavigation.GiaBan 
                })
                .ToListAsync();

            return Ok(list);
        }

        [HttpGet("receipt-detail/{maPhieu}")]
        public async Task<IActionResult> GetReceiptDetail(string maPhieu)
        {
            var phieu = await _context.NhapHangs
                .Include(n => n.MaNhaCungCapNavigation) // Lấy tên NCC
                .Include(n => n.MaNhanVienNavigation)   // Lấy tên NV
                .Include(n => n.ChiTietNhapHangs)
                    .ThenInclude(ct => ct.MaNguyenLieuNavigation) // Lấy tên NL, ĐVT
                .FirstOrDefaultAsync(n => n.MaNhapHang == maPhieu);

            if (phieu == null) return NotFound(new { message = "Không tìm thấy phiếu nhập" });

            // Lấy danh sách CungUng để map MaCungUng
            var cungUngs = await _context.CungUngs
                .Where(cu => cu.MaNhaCungCap == phieu.MaNhaCungCap)
                .ToListAsync();
            
            var cungUngDict = cungUngs
                .Where(cu => !string.IsNullOrEmpty(cu.MaNguyenLieu))
                .GroupBy(cu => cu.MaNguyenLieu!)
                .ToDictionary(g => g.Key, g => g.First().MaCungUng);

            var result = new
            {
                phieu.MaNhapHang,
                phieu.MaNhaCungCap,
                TenNhaCungCap = phieu.MaNhaCungCapNavigation?.TenNhaCungCap,
                phieu.MaNhanVien,
                TenNhanVien = phieu.MaNhanVienNavigation?.HoTen,
                phieu.NgayLapPhieu,
                phieu.MaTrangThai,
                phieu.TongTien,
                ChiTiet = phieu.ChiTietNhapHangs.Select(ct => new
                {
                    // Tìm MaCungUng từ dictionary
                    MaCungUng = cungUngDict.GetValueOrDefault(ct.MaNguyenLieu, ""),
                    MaNguyenLieu = ct.MaNguyenLieu,
                    TenNguyenLieu = ct.MaNguyenLieuNavigation?.TenNguyenLieu ?? "",
                    DonViTinh = ct.MaNguyenLieuNavigation?.DonViTinh ?? "",
                    ct.SoLuong,
                    ct.GiaNhap,
                    ThanhTien = ct.SoLuong * ct.GiaNhap
                }).ToList()
            };

            return Ok(result);
        }


        [HttpGet("stock")]
        public async Task<IActionResult> GetInventoryStock()
        {
            // Cần Include hoặc Join bảng CungUng để biết ai bán món này
            var stockList = await _context.NguyenLieus
                .Include(n => n.CungUngs) // Giả sử model NguyenLieu có quan hệ 1-n với CungUng
                    .ThenInclude(cu => cu.MaNhaCungCapNavigation)
                .Select(n => new
                {
                    n.MaNguyenLieu,
                    n.TenNguyenLieu,
                    n.DonViTinh,
                    SoLuongTon = n.SoLuongTonKho,
                    DonGia = n.GiaBan,
                    TrangThai = n.SoLuongTonKho <= 0 ? "HET_HANG" : (n.SoLuongTonKho < 10 ? "SAP_HET" : "CON_HANG"),

                    // --- BỔ SUNG QUAN TRỌNG: Danh sách NCC bán món này ---
                    CacNhaCungCap = n.CungUngs.Select(cu => new
                    {
                        cu.MaNhaCungCap,
                        TenNhaCungCap = cu.MaNhaCungCapNavigation.TenNhaCungCap,
                        cu.MaCungUng, // Cái này quan trọng để lát lưu vào ChiTietPhieuNhap
                        GiaGoiY = n.GiaBan // Hoặc lấy giá nhập gần nhất nếu có
                    }).ToList()
                })
                .OrderBy(n => n.SoLuongTon)
                .ToListAsync();

            return Ok(stockList);
        }


        // ==================================================================================
        // 2. API: Tạo phiếu nhập mới (Create)
        // ==================================================================================
         [HttpPost("import")]
        public async Task<IActionResult> CreateReceipt([FromBody] NhapKhoDTO dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            // Dùng Transaction để đảm bảo: Nhập kho ok thì mới lưu phiếu, lỗi thì hoàn tác hết
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                string maNhapHang = "NH" + DateTime.Now.ToString("yyMMddHHmmss");

                // 1. TẠO PHIẾU NHẬP (HEADER)
                var phieuNhap = new NhapHang
                {
                    MaNhapHang = maNhapHang,
                    MaNhanVien = dto.MaNhanVien,
                    MaNhaCungCap = dto.MaNhaCungCap,
                    NgayLapPhieu = DateTime.Now,
                    MaTrangThai = dto.MaTrangThai, // Lưu đúng trạng thái user chọn luôn
                    TongTien = dto.ChiTiet.Sum(c => c.SoLuong * c.GiaNhap),
                    // Nếu hoàn tất thì lưu ngày nhập, còn không thì null hoặc ngày lập
                    NgayNhapHang = (dto.MaTrangThai == "DA_HOAN_TAT") ? DateTime.Now : DateTime.Now
                };

                _context.NhapHangs.Add(phieuNhap);

                // 2. TẠO CHI TIẾT VÀ CẬP NHẬT KHO (NẾU HOÀN TẤT)
                foreach (var item in dto.ChiTiet)
                {
                    // Tìm mã nguyên liệu gốc từ bảng CungUng
                    var cungUng = await _context.CungUngs.FirstOrDefaultAsync(cu => cu.MaCungUng == item.MaCungUng);

                    if (cungUng == null)
                    {
                        throw new Exception($"Không tìm thấy mã cung ứng {item.MaCungUng}");
                    }

                    // Tạo dòng chi tiết phiếu nhập
                    var chiTiet = new ChiTietNhapHang
                    {
                        MaNhapHang = maNhapHang,
                        MaNguyenLieu = cungUng.MaNguyenLieu,
                        SoLuong = item.SoLuong,
                        GiaNhap = item.GiaNhap
                    };
                    _context.ChiTietNhapHangs.Add(chiTiet);

                    // --- LOGIC MỚI: CỘNG TỒN KHO TRỰC TIẾP TẠI ĐÂY ---
                    if (dto.MaTrangThai == "DA_HOAN_TAT")
                    {
                        // Tìm nguyên liệu trong kho
                        var nguyenLieu = await _context.NguyenLieus.FirstOrDefaultAsync(nl => nl.MaNguyenLieu == cungUng.MaNguyenLieu);
                        if (nguyenLieu != null)
                        {
                            // Cộng dồn số lượng
                            nguyenLieu.SoLuongTonKho += item.SoLuong;

                            // Cập nhật trạng thái tồn kho (Logic giống hệt trigger cũ của bạn)
                            if (nguyenLieu.SoLuongTonKho == 0) nguyenLieu.TrangThaiTonKho = "HET_HANG";
                            else if (nguyenLieu.SoLuongTonKho < 10) nguyenLieu.TrangThaiTonKho = "CAN_CANH_BAO"; // Hoặc SAP_HET tùy enum
                            else nguyenLieu.TrangThaiTonKho = "BINH_THUONG"; // Hoặc CON_HANG

                            // Đánh dấu nguyên liệu đã bị thay đổi để EF cập nhật
                            _context.NguyenLieus.Update(nguyenLieu);
                        }
                    }
                }

                // 3. LƯU TẤT CẢ VÀO DB MỘT LẦN DUY NHẤT
                await _context.SaveChangesAsync();

                // Commit transaction
                await transaction.CommitAsync();

                return Ok(new { message = "Tạo phiếu và nhập kho thành công!", maPhieu = maNhapHang });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(); // Gặp lỗi thì hoàn tác sạch sẽ
                return StatusCode(500, new { message = "Lỗi hệ thống: " + ex.Message });
            }
        }

        // ==================================================================================
        // 3. API: Cập nhật phiếu cũ (Update)
        // ==================================================================================
        [HttpPut("update")]
        public async Task<IActionResult> UpdateReceipt([FromQuery] string maPhieu, [FromBody] NhapKhoDTO dto)
        {
            var phieu = await _context.NhapHangs
                .Include(n => n.ChiTietNhapHangs)
                .FirstOrDefaultAsync(n => n.MaNhapHang == maPhieu);

            if (phieu == null) return NotFound("Không tìm thấy phiếu.");

            // Kiểm tra: Nếu phiếu đã hoàn tất rồi thì có cho sửa không? (Tùy nghiệp vụ của bạn)
            // if (phieu.MaTrangThai == "DA_HOAN_TAT") return BadRequest("Phiếu đã chốt, không thể sửa.");

            try
            {
                // 1. Update Header
                phieu.MaNhaCungCap = dto.MaNhaCungCap;
                phieu.MaNhanVien = dto.MaNhanVien;
                phieu.MaTrangThai = dto.MaTrangThai;

                // Nếu chuyển sang Hoàn tất -> Cập nhật ngày nhập
                if (dto.MaTrangThai == "DA_HOAN_TAT")
                {
                    phieu.NgayNhapHang = DateTime.Now;
                }
                // Nếu không phải Hoàn tất, giữ nguyên ngày nhập hiện tại (không reset về null)

                // 2. Update Chi tiết (Xóa hết cũ -> Thêm mới)
                _context.ChiTietNhapHangs.RemoveRange(phieu.ChiTietNhapHangs);

                decimal tongTien = 0;
                foreach (var item in dto.ChiTiet)
                {
                    // Lấy MaNguyenLieu từ CungUng
                    var cungUng = await _context.CungUngs
                        .FirstOrDefaultAsync(cu => cu.MaCungUng == item.MaCungUng);
                    
                    if (cungUng == null || string.IsNullOrEmpty(cungUng.MaNguyenLieu))
                    {
                        return BadRequest(new { message = $"Không tìm thấy nguyên liệu cho mã cung ứng: {item.MaCungUng}" });
                    }

                    var chiTietMoi = new ChiTietNhapHang
                    {
                        MaNhapHang = maPhieu,
                        MaNguyenLieu = cungUng.MaNguyenLieu,
                        SoLuong = item.SoLuong,
                        GiaNhap = item.GiaNhap,
                        //GhiChu = ""
                    };
                    _context.ChiTietNhapHangs.Add(chiTietMoi);
                    tongTien += (item.SoLuong * item.GiaNhap);
                }
                phieu.TongTien = tongTien;

                await _context.SaveChangesAsync();
                return Ok(new { message = "Cập nhật phiếu thành công!" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Lỗi cập nhật: " + ex.Message);
            }
        }
    }
}

