using System.ComponentModel.DataAnnotations;

namespace QuanLyNhaHang.Models.DTO
{
    public class CreateMonAnDTO
    {
        [Required]
        public string TenMonAn { get; set; } = null!;

        public string? MaDanhMuc { get; set; }

        public bool IsShow { get; set; } = true;

        public List<string>? HinhAnhUrls { get; set; }

        public List<PhienBanMonAnDTO> PhienBanMonAns { get; set; } = new List<PhienBanMonAnDTO>();
    }

    public class PhienBanMonAnDTO
    {
        public string? MaPhienBan { get; set; }

        [Required]
        public string TenPhienBan { get; set; } = null!;

        [Required]
        public decimal Gia { get; set; }

        public string MaTrangThai { get; set; } = "CON_HANG";

        public bool IsShow { get; set; } = true;

        public int? ThuTu { get; set; }

        public List<CongThucNauAnDTO> CongThucNauAns { get; set; } = new List<CongThucNauAnDTO>();
    }

    public class CongThucNauAnDTO
    {
        [Required]
        public string MaNguyenLieu { get; set; } = null!;

        [Required]
        public int SoLuongCanDung { get; set; }
    }
}

