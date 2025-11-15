using System;
using System.Collections.Generic;

namespace QuanLyNhaHang.Models;

public partial class NhapHang
{
    public string MaNhapHang { get; set; } = null!;

    public string MaNhanVien { get; set; } = null!;

    public DateTime NgayNhapHang { get; set; }

    public decimal TongTien { get; set; }

    public virtual ICollection<ChiTietNhapHang> ChiTietNhapHangs { get; set; } = new List<ChiTietNhapHang>();

    public virtual NhanVien MaNhanVienNavigation { get; set; } = null!;
}
