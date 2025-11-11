using System.ComponentModel.DataAnnotations;

namespace QuanLyNhaHang.Models.DTO
{
    public class DatBanDTO
    {
        [Required]
        public string MaBan { get; set; } = null!;
        [Required]
        public string HoTenKhach { get; set; } = null!;
        [Required]
        public string SoDienThoaiKhach { get; set; } = null!;

        [Required]
        public DateTime ThoiGianDatHang { get; set; }
        [Required]
        public int SoLuongNguoi { get; set; }
        public string? GhiChu { get; set; }
        public string? MaNhanVien
        {
            get; set;
        }

        public decimal? TienDatCoc { get; set; }
    }
}
