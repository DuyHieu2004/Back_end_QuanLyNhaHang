namespace QuanLyNhaHang.Models.DTO
{
    public class BanAnDto
    {
        public string maBan { get; set; } = null!;
        public string tenBan { get; set; } = null!;

        public string? maTrangThai { get; set; }
        public string? tenTrangThai { get; set; }

        public int? sucChua {  get; set; }

        public string? maTang { get; set; }
        public string? tenTang { get; set; }

        public bool? isShow { get; set; }
    }
}
