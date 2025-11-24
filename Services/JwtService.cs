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

    public string GenerateToken(IEnumerable<Claim> claims)
    {
        
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims), 

            
            Expires = DateTime.Now.AddHours(3),

            Issuer = _config["Jwt:Issuer"],
            Audience = _config["Jwt:Audience"], 

            SigningCredentials = credentials
        };

        //Tạo Token và trả về chuỗi
        var tokenHandler = new JwtSecurityTokenHandler();
        var securityToken = tokenHandler.CreateToken(tokenDescriptor);

        return tokenHandler.WriteToken(securityToken);
    }
}