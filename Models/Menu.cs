using System;
using System.Collections.Generic;

namespace QuanLyNhaHang.Models;

public partial class Menu
{
    public string MaMenu { get; set; } = null!;

    public string TenMenu { get; set; } = null!;

    public string MaLoaiMenu { get; set; } = null!;

    public string MaTrangThai { get; set; } = null!;

    public decimal GiaMenu { get; set; }

    public decimal? GiaGoc { get; set; }

    public string? MoTa { get; set; }

    public string? HinhAnh { get; set; }

    public DateTime? NgayBatDau { get; set; }

    public DateTime? NgayKetThuc { get; set; }

    public bool IsShow { get; set; }

    public int? ThuTu { get; set; }

    public string? KhungGio { get; set; }

    public DateTime NgayTao { get; set; }

    public DateTime? NgayCapNhat { get; set; }

    public virtual ICollection<ChiTietMenu> ChiTietMenus { get; set; } = new List<ChiTietMenu>();

    public virtual LoaiMenu MaLoaiMenuNavigation { get; set; } = null!;

    public virtual TrangThaiMenu MaTrangThaiNavigation { get; set; } = null!;
}
