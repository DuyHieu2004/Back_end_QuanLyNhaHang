namespace QuanLyNhaHang.Models.DTO
{
    public class MonAnDatDTO
    {
        public string TenMon { get; set; }
        public string TenPhienBan { get; set; } // Thêm trường này để hiện Size/Kích thước
        public int SoLuong { get; set; }
        public decimal DonGia { get; set; }
        public string HinhAnh { get; set; }

        // Tính thành tiền (Optional - Có thể tính ở Flutter cũng được)
        public decimal ThanhTien => SoLuong * DonGia;
    }
}
