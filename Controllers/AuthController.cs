using Microsoft.AspNetCore.Mvc;
using QuanLyNhaHang.Models; // Để dùng LoginModel và NhanVien
using QuanLyNhaHang.Services; // Để dùng JwtService
using Microsoft.EntityFrameworkCore; // Để dùng các hàm EF Core
using System.Security.Claims; // Để tạo Claims

[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly QLNhaHangContext _context;
    private readonly JwtService _jwtService;

    // Dependency Injection: Nhận Context CSDL và JwtService
    public AuthController(QLNhaHangContext context, JwtService jwtService)
    {
        _context = context;
        _jwtService = jwtService;
    }

    [HttpPost("nhanvien-login")]
    public async Task<IActionResult> Login([FromBody] DangNhap  model)
    {
        // 1. TÌM NGƯỜI DÙNG BẰNG Tên đăng nhập (TenDangNhap)
        // **LƯU Ý:** Đảm bảo QLNhaHangContext của bạn có DbSet cho NhanVien
        var user = await _context.NhanViens
            // Tìm kiếm bằng TenDangNhap
            .SingleOrDefaultAsync(u => u.TenDangNhap == model.Username);

        if (user == null)
        {
            return Unauthorized(new { message = "Tên đăng nhập hoặc mật khẩu không đúng." });
        }

        bool isPasswordValid = (user.MatKhau == model.Password);

        if (!isPasswordValid)
        {
            return Unauthorized(new { message = "Tên đăng nhập hoặc mật khẩu không đúng." });
        }

        // 3. TẠO CLAIMS
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.MaNhanVien), // ID người dùng
            new Claim(ClaimTypes.Name, user.TenDangNhap), // Tên đăng nhập
            new Claim(ClaimTypes.Role, user.MaVaiTro), // MaVaiTro để phân quyền
        };

        // 4. TẠO VÀ TRẢ VỀ TOKEN (Sử dụng phương thức GenerateToken mới)
        var token = _jwtService.GenerateToken(claims);

        // Trả về token và MaVaiTro (Role) để Front-end lưu trữ
        return Ok(new { token, role = user.MaVaiTro, username = user.TenDangNhap });
    }
}