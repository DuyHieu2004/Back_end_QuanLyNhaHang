using System;
using System.Collections.Generic;

namespace QuanLyNhaHang.Models;

public partial class TrangThaiPhienBanMonAn
{
    public string MaTrangThai { get; set; } = null!;

    public string TenTrangThai { get; set; } = null!;

    public virtual ICollection<PhienBanMonAn> PhienBanMonAns { get; set; } = new List<PhienBanMonAn>();
}
