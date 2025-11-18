using System;
using System.Collections.Generic;

namespace QuanLyNhaHang.Models;

public partial class ChiTietDonHang
{
    public long MaChiTietDonHang { get; set; }

    // Vẫn giữ MaDonHang để query nhanh (theo DB cũ vẫn còn cột này)
    public string MaDonHang { get; set; } = null!;

    public string MaPhienBan { get; set; } = null!;

    public string MaCongThuc { get; set; } = null!;

    public int SoLuong { get; set; }

    // 1. THÊM CỘT KHÓA NGOẠI MỚI (Cho phép null vì dữ liệu cũ có thể null)
    public string? MaBanAnDonHang { get; set; }

    // --- CÁC MỐI QUAN HỆ (NAVIGATION) ---

    public virtual CongThucNauAn MaCongThucNavigation { get; set; } = null!;

    public virtual PhienBanMonAn MaPhienBanNavigation { get; set; } = null!;

    // Vẫn giữ liên kết trực tiếp với Đơn hàng (để code cũ không bị gãy ngay lập tức)
    public virtual DonHang MaDonHangNavigation { get; set; } = null!;

    // 2. THÊM LIÊN KẾT MỚI VỚI BÀN-ĐƠN-HÀNG
    public virtual BanAnDonHang? MaBanAnDonHangNavigation { get; set; }
}