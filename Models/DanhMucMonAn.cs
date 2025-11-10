using System;
using System.Collections.Generic;

namespace QuanLyNhaHang.Models;

public partial class DanhMucMonAn
{
    public string MaDanhMuc { get; set; } = null!;

    public string TenDanhMuc { get; set; } = null!;

    public virtual ICollection<MonAn> MonAns { get; set; } = new List<MonAn>();
}
