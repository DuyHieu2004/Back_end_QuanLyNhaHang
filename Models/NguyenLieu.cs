using System;
using System.Collections.Generic;

namespace QuanLyNhaHang.Models;

public partial class NguyenLieu
{
    public string MaNguyenLieu { get; set; } = null!;

    public string TenNguyenLieu { get; set; } = null!;

    public string? DonViTinh { get; set; }

    public int SoLuongTonKho { get; set; }

    public virtual ICollection<ChiTietCongThuc> ChiTietCongThucs { get; set; } = new List<ChiTietCongThuc>();

    public virtual ICollection<CungUng> CungUngs { get; set; } = new List<CungUng>();
}
