namespace QuanLyNhaHang.Models.DTO
{
    public class ChiTietDatBanDTO
    {
        public string MaDonHang { get; set; }
        public DateTime ThoiGianDat { get; set; }
        public int SoNguoi { get; set; }
        public string GhiChu { get; set; }
        public decimal? TienDatCoc { get; set; }
        public string TrangThai { get; set; }

        public string TenNguoiDat { get; set; }
        public string SDTNguoiDat { get; set; }
        public List<MonAnDatDTO> MonAns { get; set; }
    }
}
