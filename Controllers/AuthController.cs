using Microsoft.AspNetCore.Mvc;
using QuanLyNhaHang.Models; 
using QuanLyNhaHang.Services; 
using Microsoft.EntityFrameworkCore; 
using System.Security.Claims; 

[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly QLNhaHangContext _context;
    private readonly JwtService _jwtService;

    //Nhận Context CSDL và JwtService
    public AuthController(QLNhaHangContext context, JwtService jwtService)
    {
        _context = context;
        _jwtService = jwtService;
    }

    [HttpPost("nhanvien-login")]
    public async Task<IActionResult> Login([FromBody] DangNhap  model)
    {
        // 1.(TenDangNhap)
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

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.MaNhanVien), // ID người dùng
            new Claim(ClaimTypes.Name, user.TenDangNhap), // Tên đăng nhập
            new Claim(ClaimTypes.Role, user.MaVaiTro), // MaVaiTro để phân quyền
        };
        var token = _jwtService.GenerateToken(claims);

        return Ok(new { token, role = user.MaVaiTro, username = user.TenDangNhap });
    }
}