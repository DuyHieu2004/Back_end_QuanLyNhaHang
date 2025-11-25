using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanlyNhaHang.Models.DTOs;
using QuanLyNhaHang.Models;
using QuanLyNhaHang.Services;

namespace QuanLyNhaHang.Controllers
{
    // --- CÁC CLASS DTO (Giữ nguyên) ---
    public class AuthResponse
    {
        public string Token { get; set; }
        public string RefreshToken { get; set; }
        public string HoTen { get; set; }
        public string MaKhachHang { get; set; }
    }

    public class AdminAuthResponse
    {
        public string Token { get; set; }
        public string RefreshToken { get; set; }
        public string HoTen { get; set; }
        public string MaNhanVien { get; set; }
        public string MaVaiTro { get; set; }
        public string TenVaiTro { get; set; }
    }

    public class RefreshTokenRequest
    {
        public string RefreshToken { get; set; }
    }

    public class RefreshTokenResponse
    {
        public string AccessToken { get; set; }
        public string NewRefreshToken { get; set; }
    }

    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly QLNhaHangContext _context;
        private readonly IOtpService _otpService;
        private readonly JwtService _jwtService;
        private readonly IEmailService _emailService;

        public AuthController(QLNhaHangContext context,
                              IOtpService otpService,
                              JwtService jwtService,
                              IEmailService emailService)
        {
            _context = context;
            _otpService = otpService;
            _jwtService = jwtService;
            _emailService = emailService;
        }

        [HttpPost("check-user")]
        public async Task<IActionResult> CheckUser([FromBody] CheckUserRequest req)
        {
            try
            {
                KhachHang? kh = null;
                bool userExists = false;

                if (req.Identifier.Contains("@"))
                {
                    kh = await _context.KhachHangs.FirstOrDefaultAsync(k => k.Email == req.Identifier);
                }
                else
                {
                    kh = await _context.KhachHangs.FirstOrDefaultAsync(k => k.SoDienThoai == req.Identifier);
                }

                userExists = (kh != null);
                var otp = _otpService.GenerateAndStoreOtp(req.Identifier);

                if (req.Identifier.Contains("@"))
                {
                    var hoTen = userExists ? kh.HoTen : "bạn";
                    await _emailService.SendOtpEmailAsync(req.Identifier, hoTen, otp);
                }
                else
                {
                    Console.WriteLine($"*** OTP cho SĐT {req.Identifier} là: {otp} ***");
                }

                return Ok(new CheckUserResponse { UserExists = userExists });
            }
            catch (Exception e)
            {
                Console.WriteLine($"*** LỖI CHECKUSER: {e.Message} ***");
                return StatusCode(500, $"Lỗi máy chủ nội bộ: {e.Message}");
            }
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] QuanlyNhaHang.Models.DTOs.RegisterRequest req)
        {
            if (!_otpService.ValidateOtp(req.Identifier, req.Otp))
            {
                return BadRequest(new { Message = "OTP không hợp lệ hoặc đã hết hạn." });
            }

            var newKhachHang = new KhachHang
            {
                MaKhachHang = $"KH{DateTime.Now.Ticks}",
                HoTen = req.HoTen
            };

            if (req.Identifier.Contains("@"))
            {
                newKhachHang.Email = req.Identifier;
                newKhachHang.SoDienThoai = "";
            }
            else
            {
                newKhachHang.SoDienThoai = req.Identifier;
                newKhachHang.Email = "";
            }

            _context.KhachHangs.Add(newKhachHang);
            await _context.SaveChangesAsync();

            var tokens = await _jwtService.GenerateTokens(newKhachHang);

            return Ok(new AuthResponse
            {
                Token = tokens.AccessToken,
                RefreshToken = tokens.RefreshToken,
                HoTen = newKhachHang.HoTen,
                MaKhachHang = newKhachHang.MaKhachHang
            });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] QuanlyNhaHang.Models.DTOs.LoginRequest req)
        {
            if (!_otpService.ValidateOtp(req.Identifier, req.Otp))
            {
                return BadRequest(new { Message = "OTP không hợp lệ hoặc đã hết hạn." });
            }

            KhachHang? khachHang = null;
            if (req.Identifier.Contains("@"))
            {
                khachHang = await _context.KhachHangs.FirstOrDefaultAsync(kh => kh.Email == req.Identifier);
            }
            else
            {
                khachHang = await _context.KhachHangs.FirstOrDefaultAsync(kh => kh.SoDienThoai == req.Identifier);
            }

            if (khachHang == null)
            {
                return NotFound(new { Message = "Không tìm thấy tài khoản." });
            }

            var tokens = await _jwtService.GenerateTokens(khachHang);

            return Ok(new AuthResponse
            {
                Token = tokens.AccessToken,
                RefreshToken = tokens.RefreshToken,
                HoTen = khachHang.HoTen,
                MaKhachHang = khachHang.MaKhachHang
            });
        }

        // --- ĐÂY LÀ HÀM ADMIN LOGIN DUY NHẤT (ĐÃ GẮN LOG DEBUG) ---
        [HttpPost("admin/login")]
        public async Task<IActionResult> AdminLogin([FromBody] QuanlyNhaHang.Models.DTOs.AdminLoginRequest req)
        {
            // 1. Gắn log bắt đầu
            Console.WriteLine("--------------------------------------------------");
            Console.WriteLine($"[DEBUG] Bắt đầu đăng nhập cho user: {req.TenDangNhap}");

            try
            {
                if (string.IsNullOrWhiteSpace(req.TenDangNhap) || string.IsNullOrWhiteSpace(req.MatKhau))
                {
                    Console.WriteLine("[DEBUG] Lỗi: Thiếu tên đăng nhập hoặc mật khẩu");
                    return BadRequest(new { Message = "Tên đăng nhập và mật khẩu không được để trống." });
                }

                // 2. Log tìm user
                Console.WriteLine("[DEBUG] Đang tìm nhân viên trong DB...");
                var nhanVien = await _context.NhanViens
                    .Include(n => n.MaVaiTroNavigation)
                    .FirstOrDefaultAsync(n => n.TenDangNhap == req.TenDangNhap);

                if (nhanVien == null)
                {
                    Console.WriteLine("[DEBUG] Lỗi: Không tìm thấy nhân viên trong DB");
                    return Unauthorized(new { Message = "Tên đăng nhập hoặc mật khẩu không đúng." });
                }
                Console.WriteLine($"[DEBUG] Tìm thấy nhân viên: {nhanVien.HoTen} (Mã: {nhanVien.MaNhanVien})");

                // 3. Log kiểm tra pass
                Console.WriteLine("[DEBUG] Đang kiểm tra mật khẩu...");
                bool isPasswordValid = BCrypt.Net.BCrypt.Verify(req.MatKhau, nhanVien.MatKhau);

                if (!isPasswordValid)
                {
                    Console.WriteLine("[DEBUG] Lỗi: Sai mật khẩu");
                    return Unauthorized(new { Message = "Tên đăng nhập hoặc mật khẩu không đúng." });
                }
                Console.WriteLine("[DEBUG] Mật khẩu đúng!");

                // 4. Log tạo token (NGHI NGỜ LỖI Ở ĐÂY)
                Console.WriteLine("[DEBUG] Đang gọi JwtService.GenerateTokens...");

                // Kiểm tra xem _jwtService có bị null không
                if (_jwtService == null)
                {
                    throw new Exception("_jwtService bị NULL! Chưa đăng ký trong Program.cs?");
                }

                var tokens = await _jwtService.GenerateTokens(nhanVien);

                Console.WriteLine("[DEBUG] Tạo token thành công!");
                Console.WriteLine($"[DEBUG] AccessToken: {tokens.AccessToken.Substring(0, 10)}...");
                Console.WriteLine($"[DEBUG] RefreshToken: {tokens.RefreshToken}");

                return Ok(new AdminAuthResponse
                {
                    Token = tokens.AccessToken,
                    RefreshToken = tokens.RefreshToken,
                    HoTen = nhanVien.HoTen,
                    MaNhanVien = nhanVien.MaNhanVien,
                    MaVaiTro = nhanVien.MaVaiTro,
                    TenVaiTro = nhanVien.MaVaiTroNavigation?.TenVaiTro ?? ""
                });
            }
            catch (Exception e)
            {
                // 5. BẮT ĐƯỢC LỖI -> IN RA MÀN HÌNH ĐEN
                Console.WriteLine("**************************************************");
                Console.WriteLine("[DEBUG] LỖI CHẾT NGƯỜI (EXCEPTION):");
                Console.WriteLine(e.ToString());
                Console.WriteLine("**************************************************");

                // Trả về lỗi chi tiết cho Frontend
                return StatusCode(500, new { Message = "Lỗi Server: " + e.Message, Detail = e.ToString() });
            }
        }

        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest req)
        {
            if (string.IsNullOrWhiteSpace(req.RefreshToken))
            {
                return BadRequest(new { Message = "Refresh Token là bắt buộc." });
            }

            var validationResult = await _jwtService.ValidateRefreshToken(req.RefreshToken);

            if (!validationResult.IsValid)
            {
                return Unauthorized(new { Message = "Phiên đăng nhập hết hạn hoặc không hợp lệ." });
            }

            object? user = null;
            if (validationResult.UserId.StartsWith("KH"))
            {
                user = await _context.KhachHangs.FindAsync(validationResult.UserId);
            }
            else
            {
                user = await _context.NhanViens.FindAsync(validationResult.UserId);
            }

            if (user == null)
            {
                return Unauthorized(new { Message = "Thông tin người dùng không hợp lệ." });
            }

            TokenPair newTokens;
            if (user is KhachHang kh)
            {
                newTokens = await _jwtService.GenerateTokens(kh);
            }
            else if (user is NhanVien nv)
            {
                newTokens = await _jwtService.GenerateTokens(nv);
            }
            else
            {
                return StatusCode(500, "Lỗi xác định loại tài khoản.");
            }

            return Ok(new RefreshTokenResponse
            {
                AccessToken = newTokens.AccessToken,
                NewRefreshToken = newTokens.RefreshToken
            });
        }
    }
}