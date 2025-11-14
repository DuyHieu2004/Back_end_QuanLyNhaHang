using System;
using System.Collections.Generic;

namespace QuanLyNhaHang.Models;

public partial class Tang
{
    public string MaTang { get; set; } = null!;

    public string TenTang { get; set; } = null!;

    public virtual ICollection<BanAn> BanAns { get; set; } = new List<BanAn>();
}

