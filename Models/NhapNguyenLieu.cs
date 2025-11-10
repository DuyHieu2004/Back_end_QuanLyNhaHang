using System;
using System.Collections.Generic;

namespace QuanLyNhaHang.Models;

public partial class NhapNguyenLieu
{
    public string MaNhapHang { get; set; } = null!;

    public string MaNhanVien { get; set; } = null!;

    public DateTime NgayNhapHang { get; set; }

    public decimal TongTien { get; set; }

    public virtual ICollection<ChiTietNhapNguyenLieu> ChiTietNhapNguyenLieus { get; set; } = new List<ChiTietNhapNguyenLieu>();

    public virtual NhanVien MaNhanVienNavigation { get; set; } = null!;
}
