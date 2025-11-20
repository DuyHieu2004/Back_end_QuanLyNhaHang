using System;
using System.Collections.Generic;

namespace QuanLyNhaHang.Models;

public partial class NhaCungCap
{
    public string MaNhaCungCap { get; set; } = null!;

    public string TenNhaCungCap { get; set; } = null!;

    public string SoDienThoai { get; set; } = null!;

    public string? DiaChi { get; set; }
    public virtual ICollection<NhapHang> NhapHangs { get; set; } = new List<NhapHang>();

    public virtual ICollection<CungUng> CungUngs { get; set; } = new List<CungUng>();
}
