using System;
using System.Collections.Generic;

namespace QuanLyNhaHang.Models;

public partial class ChiTietMonAn
{
    public string MaCt { get; set; } = null!;

    public string TenCt { get; set; } = null!;

    public string MaMonAn { get; set; } = null!;

    public virtual ICollection<CongThucNauAn> CongThucNauAns { get; set; } = new List<CongThucNauAn>();

    public virtual MonAn MaMonAnNavigation { get; set; } = null!;
}
