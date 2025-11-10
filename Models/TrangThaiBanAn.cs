using System;
using System.Collections.Generic;

namespace QuanLyNhaHang.Models;

public partial class TrangThaiBanAn
{
    public string MaTrangThai { get; set; } = null!;

    public string TenTrangThai { get; set; } = null!;

    public virtual ICollection<BanAn> BanAns { get; set; } = new List<BanAn>();
}
