using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanlyNhaHang.Models.DTOs;
using QuanLyNhaHang.Models;
using QuanLyNhaHang.Services;

namespace QuanLyNhaHang.Controllers
{

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


        // BƯỚC 3 (ĐĂNG KÝ)
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

          
            var token = _jwtService.GenerateToken(newKhachHang);

            return Ok(new AuthResponse
            {
                Token = token,
                HoTen = newKhachHang.HoTen,
                MaKhachHang = newKhachHang.MaKhachHang
            });
        }

        // BƯỚC 3 (ĐĂNG NHẬP)
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

        
            var token = _jwtService.GenerateToken(khachHang);

            return Ok(new AuthResponse
            {
                Token = token,
                HoTen = khachHang.HoTen,
                MaKhachHang = khachHang.MaKhachHang
            });
        }

    }
    
}
