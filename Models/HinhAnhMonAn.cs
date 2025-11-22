using System;
using System.Collections.Generic;

namespace QuanLyNhaHang.Models;

public partial class HinhAnhMonAn
{
    public int Id { get; set; }

    public string MaMonAn { get; set; } = null!;

    public string UrlhinhAnh { get; set; } = null!;

    public virtual MonAn MaMonAnNavigation { get; set; } = null!;
}
