using System;
using System.Collections.Generic;

namespace QuanLyNhaHang.Models;

public partial class DonHang
{
    public string MaDonHang { get; set; } = null!;

    public string? MaNhanVien { get; set; }

    public string MaKhachHang { get; set; } = null!;

    public string MaTrangThaiDonHang { get; set; } = null!;

    public DateTime? ThoiGianDatHang { get; set; }

    public DateTime? TgdatDuKien { get; set; }

    public DateTime? TgnhanBan { get; set; }

    public bool ThanhToan { get; set; }

    public DateTime? ThoiGianKetThuc { get; set; }

    public int SoLuongNguoiDk { get; set; }

    public decimal? TienDatCoc { get; set; }

    public string? GhiChu { get; set; }

    public string? TenNguoiNhan { get; set; }

    public string? SDTNguoiNhan { get; set; }

    public string? EmailNguoiNhan { get; set; }

    public virtual ICollection<ChiTietDonHang> ChiTietDonHangs { get; set; } = new List<ChiTietDonHang>();

    public virtual KhachHang MaKhachHangNavigation { get; set; } = null!;

    public virtual NhanVien? MaNhanVienNavigation { get; set; }

    public virtual TrangThaiDonHang MaTrangThaiDonHangNavigation { get; set; } = null!;

    public virtual ICollection<BanAnDonHang> BanAnDonHangs { get; set; } = new List<BanAnDonHang>();
}
