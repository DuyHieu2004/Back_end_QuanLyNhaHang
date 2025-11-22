using System;
using System.Collections.Generic;

namespace QuanLyNhaHang.Models;

public partial class KhuyenMai
{
    public string MaKhuyenMai { get; set; } = null!;

    public string TenKhuyenMai { get; set; } = null!;

    public string? MoTa { get; set; }

    public string LoaiKhuyenMai { get; set; } = null!;

    public decimal GiaTri { get; set; }

    public decimal? ApDungToiThieu { get; set; }

    public DateTime NgayBatDau { get; set; }

    public DateTime NgayKetThuc { get; set; }

    public string TrangThai { get; set; } = null!;

    public virtual ICollection<DonHang> DonHangs { get; set; } = new List<DonHang>();

    public virtual ICollection<KhuyenMaiApDungSanPham> KhuyenMaiApDungSanPhams { get; set; } = new List<KhuyenMaiApDungSanPham>();
}
