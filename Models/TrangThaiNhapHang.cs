using System;
using System.Collections.Generic;

namespace QuanLyNhaHang.Models;

public partial class TrangThaiNhapHang
{
    public string MaTrangThai { get; set; } = null!;

    public string TenTrangThai { get; set; } = null!;

    public virtual ICollection<NhapHang> NhapHangs { get; set; } = new List<NhapHang>();
}
