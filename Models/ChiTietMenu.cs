using System;
using System.Collections.Generic;

namespace QuanLyNhaHang.Models;

public partial class ChiTietMenu
{
    public long MaChiTietMenu { get; set; }

    public string MaMenu { get; set; } = null!;

    public string MaCongThuc { get; set; } = null!;

    public int SoLuong { get; set; }

    public string? GhiChu { get; set; }

    public int? ThuTu { get; set; }

    public virtual CongThucNauAn MaCongThucNavigation { get; set; } = null!;

    public virtual Menu MaMenuNavigation { get; set; } = null!;
}
