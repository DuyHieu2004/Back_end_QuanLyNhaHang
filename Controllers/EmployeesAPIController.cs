using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyNhaHang.Models;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Claims;

namespace QuanLyNhaHang.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // Yêu cầu đăng nhập cho tất cả các endpoint
    public class EmployeesAPIController : ControllerBase
    {
        private readonly QLNhaHangContext _context;

        public EmployeesAPIController(QLNhaHangContext context)
        {
            _context = context;
        }

        // Helper method để kiểm tra quyền quản lý
        private bool IsManager()
        {
            var maVaiTroClaim = HttpContext.User?.FindFirst("MaVaiTro")?.Value;
            return maVaiTroClaim == "VT001"; // VT001 là mã vai trò Quản lý
        }

        // Helper method để lấy mã nhân viên từ token
        private string? GetCurrentEmployeeId()
        {
            return HttpContext.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value 
                ?? HttpContext.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        }

        [HttpGet]
        public async Task<IActionResult> GetEmployees()
        {
            var employees = await _context.NhanViens
                .Include(n => n.MaVaiTroNavigation)
              
                .Select(n => new
                {
                    n.MaNhanVien,
                    n.HoTen,
                    n.Email,
                    n.SoDienThoai,
                    n.TenDangNhap,
                    VaiTro = n.MaVaiTroNavigation != null ? n.MaVaiTroNavigation.TenVaiTro : null,
                })
                .ToListAsync();

            return Ok(employees);
        }

        [HttpGet("{maNhanVien}")]
        public async Task<IActionResult> GetEmployee(string maNhanVien)
        {
            var employee = await _context.NhanViens
                .Include(n => n.MaVaiTroNavigation)
                .FirstOrDefaultAsync(n => n.MaNhanVien == maNhanVien);

            if (employee == null)
            {
                return NotFound(new { message = "Không tìm thấy nhân viên." });
            }

            return Ok(employee);
        }

        public class CreateEmployeeDTO
        {
            [Required]
            public string HoTen { get; set; } = null!;
            [Required]
            public string TenDangNhap { get; set; } = null!;
            [Required]
            public string MatKhau { get; set; } = null!;
            public string? Email { get; set; }
            public string? SoDienThoai { get; set; }
            [Required]
            public string MaVaiTro { get; set; } = null!;
        }

        [HttpPost]
        public async Task<IActionResult> CreateEmployee([FromBody] CreateEmployeeDTO dto)
        {
            // Kiểm tra quyền quản lý
            if (!IsManager())
            {
                return StatusCode(403, new { message = "Chỉ quản lý mới có quyền tạo tài khoản nhân viên." });
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var existingUsername = await _context.NhanViens
                    .AnyAsync(n => n.TenDangNhap == dto.TenDangNhap);
                if (existingUsername)
                {
                    return BadRequest(new { message = "Tên đăng nhập đã tồn tại." });
                }

                var vaiTro = await _context.VaiTros.FindAsync(dto.MaVaiTro);
                if (vaiTro == null)
                {
                    return BadRequest(new { message = "Vai trò không tồn tại." });
                }

                string maNhanVien = "NV" + DateTime.Now.ToString("yyMMddHHmmss");
                var nhanVien = new NhanVien
                {
                    MaNhanVien = maNhanVien,
                    HoTen = dto.HoTen,
                    TenDangNhap = dto.TenDangNhap,
                    MatKhau = BCrypt.Net.BCrypt.HashPassword(dto.MatKhau),
                    Email = dto.Email,
                    SoDienThoai = dto.SoDienThoai,
                    MaVaiTro = dto.MaVaiTro
                };

                _context.NhanViens.Add(nhanVien);
                await _context.SaveChangesAsync();

                var result = await _context.NhanViens
                    .Include(n => n.MaVaiTroNavigation)
                    .FirstOrDefaultAsync(n => n.MaNhanVien == maNhanVien);

                return Ok(new { message = "Tạo nhân viên thành công!", nhanVien = result });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi máy chủ: " + ex.Message });
            }
        }

        public class UpdateEmployeeDTO
        {
            public string? HoTen { get; set; }
            public string? Email { get; set; }
            public string? SoDienThoai { get; set; }
            public string? MaVaiTro { get; set; }
        }

        [HttpPut("{maNhanVien}")]
        public async Task<IActionResult> UpdateEmployee(string maNhanVien, [FromBody] UpdateEmployeeDTO dto)
        {
            // Kiểm tra quyền quản lý
            if (!IsManager())
            {
                return StatusCode(403, new { message = "Chỉ quản lý mới có quyền cập nhật thông tin nhân viên." });
            }

            var nhanVien = await _context.NhanViens.FindAsync(maNhanVien);
            if (nhanVien == null)
            {
                return NotFound(new { message = "Không tìm thấy nhân viên." });
            }

            if (!string.IsNullOrEmpty(dto.HoTen))
            {
                nhanVien.HoTen = dto.HoTen;
            }

            if (!string.IsNullOrEmpty(dto.Email))
            {
                nhanVien.Email = dto.Email;
            }

            if (!string.IsNullOrEmpty(dto.SoDienThoai))
            {
                nhanVien.SoDienThoai = dto.SoDienThoai;
            }

            if (!string.IsNullOrEmpty(dto.MaVaiTro))
            {
                var vaiTro = await _context.VaiTros.FindAsync(dto.MaVaiTro);
                if (vaiTro == null)
                {
                    return BadRequest(new { message = "Vai trò không tồn tại." });
                }
                nhanVien.MaVaiTro = dto.MaVaiTro;
            }

          

            await _context.SaveChangesAsync();

            var result = await _context.NhanViens
                .Include(n => n.MaVaiTroNavigation)
                .FirstOrDefaultAsync(n => n.MaNhanVien == maNhanVien);

            return Ok(new { message = "Cập nhật nhân viên thành công!", nhanVien = result });
        }

        [HttpDelete("{maNhanVien}")]
        public async Task<IActionResult> DeleteEmployee(string maNhanVien)
        {
            // Kiểm tra quyền quản lý
            if (!IsManager())
            {
                return StatusCode(403, new { message = "Chỉ quản lý mới có quyền xóa nhân viên." });
            }

            var nhanVien = await _context.NhanViens.FindAsync(maNhanVien);
            if (nhanVien == null)
            {
                return NotFound(new { message = "Không tìm thấy nhân viên." });
            }

            // Xóa nhân viên thay vì đánh dấu nghỉ việc (vì không có MaTrangThai)
            _context.NhanViens.Remove(nhanVien);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Nhân viên đã được xóa thành công." });
        }

        [HttpGet("roles")]
        public async Task<IActionResult> GetRoles()
        {
            var roles = await _context.VaiTros.ToListAsync();
            return Ok(roles);
        }
    }
}

