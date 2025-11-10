using System;
using System.Collections.Generic;

namespace QuanLyNhaHang.Models;

public partial class TrangThaiDonHang
{
    public string MaTrangThai { get; set; } = null!;

    public string TenTrangThai { get; set; } = null!;

    public virtual ICollection<DonHang> DonHangs { get; set; } = new List<DonHang>();
}
