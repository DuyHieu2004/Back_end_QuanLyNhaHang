using System;
using System.Collections.Generic;

namespace QuanLyNhaHang.Models;

public partial class NhanVien
{
    public string MaNhanVien { get; set; } = null!;

    public string HoTen { get; set; } = null!;

    public string TenDangNhap { get; set; } = null!;

    public string MatKhau { get; set; } = null!;

    public string MaVaiTro { get; set; } = null!;

    public string? Email { get; set; }

    public string? SoDienThoai { get; set; }

    public string? HinhAnh { get; set; }

    public virtual ICollection<DonHang> DonHangs { get; set; } = new List<DonHang>();

    public virtual VaiTro MaVaiTroNavigation { get; set; } = null!;

    public virtual ICollection<NhapHang> NhapHangs { get; set; } = new List<NhapHang>();
}
