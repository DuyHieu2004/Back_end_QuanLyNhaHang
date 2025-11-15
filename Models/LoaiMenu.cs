using System;
using System.Collections.Generic;

namespace QuanLyNhaHang.Models;

public partial class LoaiMenu
{
    public string MaLoaiMenu { get; set; } = null!;

    public string TenLoaiMenu { get; set; } = null!;

    public string? MoTa { get; set; }

    public virtual ICollection<Menu> Menus { get; set; } = new List<Menu>();
}
