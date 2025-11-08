namespace QuanLyNhaHang.Models.DTO
{
    public class BanAnDTO
    {
        public string maBan { get; set; } = null!;
        public string tenBan { get; set; } = null!;

        public string? maTrangThai { get; set; }
        public string? tenTrangThai { get; set; }

        public int? sucChua {  get; set; }

  
    }
}
