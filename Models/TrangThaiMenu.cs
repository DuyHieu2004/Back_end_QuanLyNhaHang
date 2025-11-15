using System;
using System.Collections.Generic;

namespace QuanLyNhaHang.Models;

public partial class TrangThaiMenu
{
    public string MaTrangThai { get; set; } = null!;

    public string TenTrangThai { get; set; } = null!;

    public virtual ICollection<Menu> Menus { get; set; } = new List<Menu>();
}
