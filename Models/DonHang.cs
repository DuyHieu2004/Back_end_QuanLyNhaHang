using System;
using System.Collections.Generic;

namespace QuanLyNhaHang.Models;

public partial class DonHang
{
    public string MaDonHang { get; set; } = null!;

    public string MaBan { get; set; } = null!;

    public string? MaNhanVien { get; set; }

    public string MaKhachHang { get; set; } = null!;

    public string MaTrangThaiDonHang { get; set; } = null!;

    public string? TenNguoiDat { get; set; }
    public string? SDTNguoiDat { get; set; }

    public DateTime? ThoiGianDatHang { get; set; }

    public int? ThoiGianCho { get; set; }

    public DateTime? ThoiGianBatDau { get; set; }

    public DateTime? ThoiGianKetThuc { get; set; }

    public int SoLuongNguoi { get; set; }

    public decimal? TienDatCoc { get; set; }

    public string? GhiChu { get; set; }

    public virtual ICollection<ChiTietDonHang> ChiTietDonHangs { get; set; } = new List<ChiTietDonHang>();

    public virtual BanAn MaBanNavigation { get; set; } = null!;

    public virtual KhachHang MaKhachHangNavigation { get; set; } = null!;

    public virtual NhanVien? MaNhanVienNavigation { get; set; }

    public virtual TrangThaiDonHang MaTrangThaiDonHangNavigation { get; set; } = null!;
}
