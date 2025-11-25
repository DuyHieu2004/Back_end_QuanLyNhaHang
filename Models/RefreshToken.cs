using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyNhaHang.Models
{
    [Table("RefreshTokens")] // Đặt tên bảng trong SQL
    public class RefreshToken
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public string Token { get; set; } = null!;

        [Required]
        public string UserId { get; set; } = null!; // Lưu mã KH hoặc mã NV

        public DateTime Expires { get; set; }

        public bool IsRevoked { get; set; } = false;
    }
}