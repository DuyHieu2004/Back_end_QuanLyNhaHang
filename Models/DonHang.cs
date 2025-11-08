using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace QuanLyNhaHang.Models;

[Table("DonHang")]
public partial class DonHang
{
    [Key]
    [StringLength(15)]
    [Unicode(false)]
    public string MaDonHang { get; set; } = null!;

    [StringLength(15)]
    [Unicode(false)]
    public string MaBan { get; set; } = null!;

    [StringLength(15)]
    [Unicode(false)]
    public string? MaNhanVien { get; set; }

    [StringLength(15)]
    [Unicode(false)]
    public string MaKhachHang { get; set; } = null!;

    [Column(TypeName = "datetime")]
    public DateTime? ThoiGianDatHang { get; set; }

    public int? ThoiGianCho { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? ThoiGianBatDau { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? ThoiGianKetThuc { get; set; }

    public int SoLuongNguoi { get; set; }

    [StringLength(500)]
    public string? GhiChu { get; set; }

    [InverseProperty("MaDonHangNavigation")]
    public virtual ICollection<ChiTietDonHang> ChiTietDonHangs { get; set; } = new List<ChiTietDonHang>();

    [ForeignKey("MaBan")]
    [InverseProperty("DonHangs")]
    public virtual BanAn MaBanNavigation { get; set; } = null!;

    [ForeignKey("MaKhachHang")]
    [InverseProperty("DonHangs")]
    public virtual KhachHang MaKhachHangNavigation { get; set; } = null!;

    [ForeignKey("MaNhanVien")]
    [InverseProperty("DonHangs")]
    public virtual NhanVien? MaNhanVienNavigation { get; set; }
}
