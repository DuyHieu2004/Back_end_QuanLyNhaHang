using System;
using System.Collections.Generic;

namespace QuanLyNhaHang.Models;

public partial class BanAn
{
    public string MaBan { get; set; } = null!;

    public string TenBan { get; set; } = null!;

    public string MaTrangThai { get; set; } = null!;

    public int SucChua { get; set; }

    public string? MaTang { get; set; }

    public bool IsShow { get; set; }

    public virtual ICollection<DonHang> DonHangs { get; set; } = new List<DonHang>();

    public virtual TrangThaiBanAn MaTrangThaiNavigation { get; set; } = null!;

    public virtual Tang? MaTangNavigation { get; set; }
}
