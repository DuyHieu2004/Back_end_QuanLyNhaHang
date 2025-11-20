using System;
using System.Collections.Generic;

namespace QuanLyNhaHang.Models;

public partial class BanAnDonHang
{
    public string MaBanAnDonHang { get; set; } = null!;

    public string MaDonHang { get; set; } = null!;

    public string MaBan { get; set; } = null!;

    public virtual ICollection<ChiTietDonHang> ChiTietDonHangs { get; set; } = new List<ChiTietDonHang>();

    public virtual BanAn MaBanNavigation { get; set; } = null!;

    public virtual DonHang MaDonHangNavigation { get; set; } = null!;
}
