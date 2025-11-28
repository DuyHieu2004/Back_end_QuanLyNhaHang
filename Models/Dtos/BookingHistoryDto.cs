namespace QuanLyNhaHang.Models.DTO
{
    public class BookingHistoryDto
    {
        public string MaDonHang { get; set; }
        public string TenBan { get; set; }
        public DateTime ThoiGianBatDau { get; set; }
        public DateTime? ThoiGianDuKien { get; set; }
        public int SoLuongNguoi { get; set; }
        public string? GhiChu { get; set; }

       
        public bool DaHuy { get; set; }
        public bool CoTheHuy { get; set; } 
        public string? TrangThai { get; set; }
        public string? MaTrangThai { get; set; }
    }
}
