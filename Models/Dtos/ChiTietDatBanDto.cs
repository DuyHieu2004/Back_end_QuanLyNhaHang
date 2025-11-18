namespace QuanLyNhaHang.Models.DTO
{
    public class ChiTietDatBanDto
    {
        public string MaDonHang { get; set; }
        public DateTime ThoiGianDat { get; set; }
        public int SoNguoi { get; set; }
        public string GhiChu { get; set; }
        public decimal? TienDatCoc { get; set; }
        public string TrangThai { get; set; }

        public string TenNguoiDat { get; set; }
        public string SDTNguoiDat { get; set; }
        public string TenBan { get; set; }      // <--- THÊM
        public DateTime? ThoiGianNhanBan { get; set; }
        public DateTime? ThoiGianKetThuc { get; set; }
        public List<MonAnDatDto> MonAns { get; set; }
    }
}
