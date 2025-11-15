using System;
using System.Collections.Generic;

namespace QuanLyNhaHang.Models;

public partial class ChiTietDonHang
{
    public long MaChiTietDonHang { get; set; }

    public string MaDonHang { get; set; } = null!;

    public string MaPhienBan { get; set; } = null!;

    public string MaCongThuc { get; set; } = null!;

    public int SoLuong { get; set; }

    public virtual CongThucNauAn MaCongThucNavigation { get; set; } = null!;

    public virtual DonHang MaDonHangNavigation { get; set; } = null!;

    public virtual PhienBanMonAn MaPhienBanNavigation { get; set; } = null!;
}
