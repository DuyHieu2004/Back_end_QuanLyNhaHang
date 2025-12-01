using System;
using System.Collections.Generic;

namespace QuanLyNhaHang.Models;

public partial class BanAnDonHang
{
    public string MaBanAnDonHang { get; set; } = null!;

    public string MaDonHang { get; set; } = null!;

    public string MaBan { get; set; } = null!;

    public string? MaChiTietDonHang { get; set; }

    public virtual BanAn MaBanNavigation { get; set; } = null!;

    public virtual ChiTietDonHang? MaChiTietDonHangNavigation { get; set; }

    public virtual DonHang MaDonHangNavigation { get; set; } = null!;
}
