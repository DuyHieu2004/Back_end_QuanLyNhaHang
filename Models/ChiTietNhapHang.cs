using System;
using System.Collections.Generic;

namespace QuanLyNhaHang.Models;

public partial class ChiTietNhapHang
{
    public long MaChiTietNhapHang { get; set; }

    public string MaNhapHang { get; set; } = null!;

    public string MaNguyenLieu { get; set; } = null!;

    public int SoLuong { get; set; }

    public decimal GiaNhap { get; set; }

    public virtual NguyenLieu MaNguyenLieuNavigation { get; set; } = null!;

    public virtual NhapHang MaNhapHangNavigation { get; set; } = null!;
}
