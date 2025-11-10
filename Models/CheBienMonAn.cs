using System;
using System.Collections.Generic;

namespace QuanLyNhaHang.Models;

public partial class CheBienMonAn
{
    public string MaCheBien { get; set; } = null!;

    public DateTime? NgayNau { get; set; }

    public string MaPhienBan { get; set; } = null!;

    public int? SoLuong { get; set; }

    public virtual PhienBanMonAn MaPhienBanNavigation { get; set; } = null!;
}
