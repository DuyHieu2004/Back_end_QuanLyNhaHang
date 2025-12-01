using System.ComponentModel.DataAnnotations;

namespace QuanLyNhaHang.Models.DTO
{
    public class CreateNguyenLieuDTO
    {
        [Required]
        public string TenNguyenLieu { get; set; } = null!;

        public string? DonViTinh { get; set; }

        public int SoLuongTonKho { get; set; } = 0;

        public int MinStock { get; set; } = 10; 

        public decimal GiaBan { get; set; } = 0;
    }
}

