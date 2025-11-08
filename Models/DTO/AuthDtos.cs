using System.ComponentModel.DataAnnotations;


namespace QuanlyNhaHang.Models.DTOs
{

    public class CheckUserRequest
    {
        [Required]
        public string Identifier { get; set; } 
    }

    public class CheckUserResponse
    {
        public bool UserExists { get; set; }
    }

    public class RegisterRequest
    {
        [Required]
        public string Identifier { get; set; } 
        [Required]
        public string HoTen { get; set; }
        [Required]
        public string Otp { get; set; }
    }


    public class LoginRequest
    {
        [Required]
        public string Identifier { get; set; } 
        [Required]
        public string Otp { get; set; }
    }


    public class AuthResponse
    {
        public string Token { get; set; }
        public string HoTen { get; set; }
        public string MaKhachHang { get; set; }
    }
}