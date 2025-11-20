using System;
using System.Collections.Generic;

namespace QuanLyNhaHang.Models;

public partial class MonAn
{
    public string MaMonAn { get; set; } = null!;

    public string TenMonAn { get; set; } = null!;

    public string? MaDanhMuc { get; set; }

    public bool? IsShow { get; set; }

    public virtual ICollection<ChiTietMonAn> ChiTietMonAns { get; set; } = new List<ChiTietMonAn>();

    public virtual ICollection<HinhAnhMonAn> HinhAnhMonAns { get; set; } = new List<HinhAnhMonAn>();

    public virtual DanhMucMonAn? MaDanhMucNavigation { get; set; }
}
