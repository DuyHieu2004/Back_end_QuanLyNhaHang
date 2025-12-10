using System.ComponentModel.DataAnnotations;

namespace QuanLyNhaHang.Models.DTO
{
    public class DatBanDto
    {
        [Required]
        public List<string> DanhSachMaBan { get; set; } = new List<string>();

        [Required]
        public string HoTenKhach { get; set; } = null!;
        [Required]
        public string SoDienThoaiKhach { get; set; } = null!;

        [Required]
        // Thời gian khách muốn đến (booking/expected)
        public DateTime ThoiGianDatHang { get; set; }

        // Thời gian nhận bàn thực tế (nếu là walk-in hoặc check-in ngay); optional
        public DateTime? ThoiGianNhanBan { get; set; }
        [Required]
        public int SoLuongNguoi { get; set; }
        public string? GhiChu { get; set; }
        public string? MaNhanVien
        {
            get; set;
        }

        public decimal? TienDatCoc { get; set; }

        public string? MaKhachHang { get; set; }

        // 2. Để gửi email vé đặt bàn (Optional)
        public string? Email { get; set; }
    }
}
