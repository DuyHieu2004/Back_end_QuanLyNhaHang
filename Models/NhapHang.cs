using System;
using System.Collections.Generic;

namespace QuanLyNhaHang.Models;

public partial class NhapHang
{
    public string MaNhapHang { get; set; } = null!;

    public string MaNhanVien { get; set; } = null!;

    public DateTime NgayNhapHang { get; set; }

    public decimal TongTien { get; set; }

    public DateTime NgayLapPhieu { get; set; }

    public string MaTrangThai { get; set; } = null!;

    public virtual ICollection<ChiTietNhapHang> ChiTietNhapHangs { get; set; } = new List<ChiTietNhapHang>();

    public virtual NhanVien MaNhanVienNavigation { get; set; } = null!;

    public virtual TrangThaiNhapHang MaTrangThaiNavigation { get; set; } = null!;
}
