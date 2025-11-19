namespace QuanLyNhaHang.Models.DTO
{
    public class MonAnDatDto
    {
        public string TenMon { get; set; }
        public string TenPhienBan { get; set; } // Thêm trường này để hiện Size/Kích thước
        public int SoLuong { get; set; }
        public decimal DonGia { get; set; }
        public string HinhAnh { get; set; }

        public decimal ThanhTien => SoLuong * DonGia;

        public string TenBan { get; set; }
        public string MaBan { get; set; }   // Để Front-end lọc món theo bàn
        public string GhiChu { get; set; }
    }
}
