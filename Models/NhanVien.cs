using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace QuanLyNhaHang.Models;

[Table("NhanVien")]
[Index("SoDienThoai", Name = "UQ__NhanVien__0389B7BDB8FE6D35", IsUnique = true)]
[Index("TenDangNhap", Name = "UQ__NhanVien__55F68FC0ACD2B7E4", IsUnique = true)]
[Index("Email", Name = "UQ__NhanVien__A9D10534ABDC09AD", IsUnique = true)]
public partial class NhanVien
{
    [Key]
    [StringLength(15)]
    [Unicode(false)]
    public string MaNhanVien { get; set; } = null!;

    [StringLength(100)]
    public string HoTen { get; set; } = null!;

    [StringLength(50)]
    public string TenDangNhap { get; set; } = null!;

    [StringLength(256)]
    public string MatKhau { get; set; } = null!;

    [StringLength(15)]
    [Unicode(false)]
    public string MaVaiTro { get; set; } = null!;

    [StringLength(100)]
    public string? Email { get; set; }

    [StringLength(15)]
    public string? SoDienThoai { get; set; }

    public string? HinhAnh { get; set; }

    [StringLength(15)]
    [Unicode(false)]
    public string MaTrangThai { get; set; } = null!;

    [InverseProperty("MaNhanVienNavigation")]
    public virtual ICollection<DonHang> DonHangs { get; set; } = new List<DonHang>();

    [ForeignKey("MaTrangThai")]
    [InverseProperty("NhanViens")]
    public virtual TrangThaiNhanVien MaTrangThaiNavigation { get; set; } = null!;

    [ForeignKey("MaVaiTro")]
    [InverseProperty("NhanViens")]
    public virtual VaiTro MaVaiTroNavigation { get; set; } = null!;

    [InverseProperty("MaNhanVienNavigation")]
    public virtual ICollection<NhapNguyenLieu> NhapNguyenLieus { get; set; } = new List<NhapNguyenLieu>();
}
