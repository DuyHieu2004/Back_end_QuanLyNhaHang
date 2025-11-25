using Microsoft.IdentityModel.Tokens;
using QuanLyNhaHang.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Security.Cryptography;
using Microsoft.Extensions.Configuration; // Đảm bảo đã có
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore; // Cần thiết cho thao tác DB

// --- DTOs cho JwtService ---

// Cặp Token trả về cho Frontend
public class TokenPair
{
    public string AccessToken { get; set; }
    public string RefreshToken { get; set; }
}

// Kết quả xác thực Refresh Token
public class RefreshTokenValidationResult
{
    public bool IsValid { get; set; }
    public string UserId { get; set; } // MaKhachHang hoặc MaNhanVien
}



// --- Service Chính ---
public class JwtService
{
    private readonly IConfiguration _config;
    private readonly QLNhaHangContext _context; // Inject Context

    public JwtService(IConfiguration config, QLNhaHangContext context)
    {
        _config = config;
        _context = context;
    }

    // ------------------------------------------------------------
    // 1. ACCESS TOKEN GENERATION
    // ------------------------------------------------------------
    private string CreateAccessToken(KhachHang khachHang, int days)
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
            expires: DateTime.Now.AddDays(days),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    // OVERLOAD cho NhanVien
    private string CreateAccessToken(NhanVien nhanVien, int days)
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
            expires: DateTime.Now.AddDays(days),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    // ------------------------------------------------------------
    // 2. REFRESH TOKEN GENERATION (Chuỗi ngẫu nhiên)
    // ------------------------------------------------------------
    public string GenerateRandomRefreshTokenString()
    {
        var randomNumber = new byte[32];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }
    }

    // ------------------------------------------------------------
    // 3. COMBINED TOKEN GENERATION (Cấp phát và lưu Refresh Token vào DB)
    // ------------------------------------------------------------
    public async Task<TokenPair> GenerateTokens(KhachHang khachHang)
    {
        // 1. Tạo Access Token (Ngắn hạn: 1 ngày)
        var accessToken = CreateAccessToken(khachHang, days: 1);

        // 2. Tạo Refresh Token String
        var refreshTokenString = GenerateRandomRefreshTokenString();

        // 3. Xóa các Refresh Token cũ của người dùng này (để chỉ có 1 token active)
        var existingTokens = await _context.Set<RefreshToken>()
            .Where(t => t.UserId == khachHang.MaKhachHang)
            .ToListAsync();
        _context.Set<RefreshToken>().RemoveRange(existingTokens);

        // 4. Lưu Refresh Token mới vào DB
        var newRefreshToken = new RefreshToken
        {
            Token = refreshTokenString,
            UserId = khachHang.MaKhachHang,
            Expires = DateTime.Now.AddDays(60), // Refresh Token sống lâu hơn (ví dụ 60 ngày)
            IsRevoked = false
        };

        _context.Set<RefreshToken>().Add(newRefreshToken);
        await _context.SaveChangesAsync();

        return new TokenPair
        {
            AccessToken = accessToken,
            RefreshToken = refreshTokenString
        };
    }

    public async Task<TokenPair> GenerateTokens(NhanVien nhanVien)
    {
        // 1. Tạo Access Token (Ngắn hạn: 1 ngày)
        var accessToken = CreateAccessToken(nhanVien, days: 1);

        // 2. Tạo Refresh Token String
        var refreshTokenString = GenerateRandomRefreshTokenString();

        // 3. Xóa các Refresh Token cũ của nhân viên này
        var existingTokens = await _context.Set<RefreshToken>()
            .Where(t => t.UserId == nhanVien.MaNhanVien)
            .ToListAsync();
        _context.Set<RefreshToken>().RemoveRange(existingTokens);

        // 4. Lưu Refresh Token mới vào DB
        var newRefreshToken = new RefreshToken
        {
            Token = refreshTokenString,
            UserId = nhanVien.MaNhanVien,
            Expires = DateTime.Now.AddDays(60),
            IsRevoked = false
        };

        _context.Set<RefreshToken>().Add(newRefreshToken);
        await _context.SaveChangesAsync();

        return new TokenPair
        {
            AccessToken = accessToken,
            RefreshToken = refreshTokenString
        };
    }

    // ------------------------------------------------------------
    // 4. REFRESH TOKEN VALIDATION
    // ------------------------------------------------------------
    public async Task<RefreshTokenValidationResult> ValidateRefreshToken(string refreshToken)
    {
        var storedToken = await _context.Set<RefreshToken>()
            .FirstOrDefaultAsync(t => t.Token == refreshToken);

        if (storedToken == null || storedToken.IsRevoked)
        {
            return new RefreshTokenValidationResult { IsValid = false, UserId = string.Empty };
        }

        if (storedToken.Expires < DateTime.Now)
        {
            // Token đã hết hạn
            // Tùy chọn: Xóa token hết hạn khỏi DB tại đây
            return new RefreshTokenValidationResult { IsValid = false, UserId = string.Empty };
        }

        // Token hợp lệ
        return new RefreshTokenValidationResult { IsValid = true, UserId = storedToken.UserId };
    }
}