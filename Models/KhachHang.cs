using System;
using System.Collections.Generic;

namespace QuanLyNhaHang.Models;

public partial class KhachHang
{
    public string MaKhachHang { get; set; } = null!;

    public string HoTen { get; set; } = null!;

    public string SoDienThoai { get; set; } = null!;

    public string? Email { get; set; }

    public string? HinhAnh { get; set; }

    public int? NoShowCount { get; set; }

    public DateTime? NgayCuoiCungTichLuy { get; set; }

    public DateTime? NgayTao { get; set; }

    public virtual ICollection<DonHang> DonHangs { get; set; } = new List<DonHang>();
}
