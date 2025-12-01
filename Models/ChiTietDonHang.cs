using System;
using System.Collections.Generic;

namespace QuanLyNhaHang.Models;

public partial class ChiTietDonHang
{
    public string MaChiTietDonHang { get; set; } = null!;

    public string MaDonHang { get; set; } = null!;

    public string MaPhienBan { get; set; } = null!;

    public string MaCongThuc { get; set; } = null!;

    public int SoLuong { get; set; }

    public virtual ICollection<BanAnDonHang> BanAnDonHangs { get; set; } = new List<BanAnDonHang>();

    public virtual CongThucNauAn MaCongThucNavigation { get; set; } = null!;

    public virtual DonHang MaDonHangNavigation { get; set; } = null!;

    public virtual PhienBanMonAn MaPhienBanNavigation { get; set; } = null!;
}
