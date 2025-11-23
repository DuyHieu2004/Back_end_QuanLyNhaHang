using Microsoft.IdentityModel.Tokens;
using QuanLyNhaHang.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

public class JwtService
{
    private readonly IConfiguration _config;
    public JwtService(IConfiguration config)
    {
        _config = config;
    }

    public string GenerateToken(KhachHang khachHang)
    {
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

      
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, khachHang.MaKhachHang),
            new Claim(JwtRegisteredClaimNames.Name, khachHang.HoTen),
            new Claim(JwtRegisteredClaimNames.Email, khachHang.Email ?? ""),
            new Claim("phone_number", khachHang.SoDienThoai),
            new Claim(ClaimTypes.Role, "KhachHang")
        };

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Issuer"],
            claims: claims,
            expires: DateTime.Now.AddDays(30),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public string GenerateToken(NhanVien nhanVien)
    {
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, nhanVien.MaNhanVien),
            new Claim(JwtRegisteredClaimNames.Name, nhanVien.HoTen),
            new Claim(JwtRegisteredClaimNames.Email, nhanVien.Email ?? ""),
            new Claim("phone_number", nhanVien.SoDienThoai ?? ""),
            new Claim(ClaimTypes.Role, "NhanVien"),
            new Claim("MaVaiTro", nhanVien.MaVaiTro)
        };

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Issuer"],
            claims: claims,
            expires: DateTime.Now.AddDays(30),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}