using System;
using System.Collections.Generic;

namespace QuanLyNhaHang.Models
{
    public partial class ChiTietDonHang
    {
        // Giữ nguyên khóa chính
        public int MaChiTietDonHang { get; set; }

        // --- [ĐÃ XÓA] public string MaDonHang { get; set; } ---

        // Đây là khóa ngoại quan trọng nhất bây giờ (Nối với bảng trung gian)
        public string MaBanAnDonHang { get; set; }

        public string MaCongThuc { get; set; } = null!;
        public string MaPhienBan { get; set; } = null!;
        public int SoLuong { get; set; }
        public string? GhiChu { get; set; }

        // --- [ĐÃ XÓA] public virtual DonHang MaDonHangNavigation { get; set; } = null!; ---

        // Giữ lại các mối liên kết khác
        public virtual BanAnDonHang MaBanAnDonHangNavigation { get; set; } = null!;
        public virtual CongThucNauAn MaCongThucNavigation { get; set; } = null!;
        public virtual PhienBanMonAn MaPhienBanNavigation { get; set; } = null!;
    }
}