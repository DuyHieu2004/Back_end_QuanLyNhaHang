using System;
using System.Collections.Generic;

namespace QuanLyNhaHang.Models;

public partial class CongThucNauAn
{
    public string MaCongThuc { get; set; } = null!;

    public string MaCt { get; set; } = null!;

    public string MaPhienBan { get; set; } = null!;

    public decimal Gia { get; set; }

    public virtual ICollection<ChiTietCongThuc> ChiTietCongThucs { get; set; } = new List<ChiTietCongThuc>();

    public virtual ICollection<ChiTietDonHang> ChiTietDonHangs { get; set; } = new List<ChiTietDonHang>();

    public virtual ICollection<ChiTietMenu> ChiTietMenus { get; set; } = new List<ChiTietMenu>();

    public virtual ICollection<KhuyenMaiApDungSanPham> KhuyenMaiApDungSanPhams { get; set; } = new List<KhuyenMaiApDungSanPham>();

    public virtual ChiTietMonAn MaCtNavigation { get; set; } = null!;

    public virtual PhienBanMonAn MaPhienBanNavigation { get; set; } = null!;
}
