namespace QuanLyNhaHang.Models.DTO
{
    public class MonAnDetailDTO
    {
        public string MaMonAn { get; set; } = null!;
        public string TenMonAn { get; set; } = null!;
        public string? MaDanhMuc { get; set; }
        public string? TenDanhMuc { get; set; }
        public bool IsShow { get; set; }
        public List<HinhAnhDTO> HinhAnhMonAns { get; set; } = new List<HinhAnhDTO>();
        public List<PhienBanMonAnDetailDTO> PhienBanMonAns { get; set; } = new List<PhienBanMonAnDetailDTO>();
    }

    public class HinhAnhDTO
    {
        public int Id { get; set; }
        public string URLHinhAnh { get; set; } = null!;
    }

    public class PhienBanMonAnDetailDTO
    {
        public string MaPhienBan { get; set; } = null!;
        public string TenPhienBan { get; set; } = null!;
        public decimal Gia { get; set; }
        public string MaTrangThai { get; set; } = null!;
        public string TenTrangThai { get; set; } = null!;
        public bool IsShow { get; set; }
        public int? ThuTu { get; set; }
        public List<CongThucNauAnDetailDTO> CongThucNauAns { get; set; } = new List<CongThucNauAnDetailDTO>();
    }

    public class CongThucNauAnDetailDTO
    {
        public string MaCongThuc { get; set; } = null!;
        public string MaNguyenLieu { get; set; } = null!;
        public string TenNguyenLieu { get; set; } = null!;
        public string? DonViTinh { get; set; }
        public int SoLuongCanDung { get; set; }
    }
}

