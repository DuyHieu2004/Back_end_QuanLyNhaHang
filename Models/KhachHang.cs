using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace QuanLyNhaHang.Models;

[Table("KhachHang")]
[Index("SoDienThoai", Name = "UQ__KhachHan__0389B7BD71B249B4", IsUnique = true)]
[Index("Email", Name = "UQ__KhachHan__A9D10534D103D775", IsUnique = true)]
public partial class KhachHang
{
    [Key]
    [StringLength(15)]
    [Unicode(false)]
    public string MaKhachHang { get; set; } = null!;

    [StringLength(100)]
    public string HoTen { get; set; } = null!;

    [StringLength(15)]
    public string SoDienThoai { get; set; } = null!;

    [StringLength(100)]
    public string? Email { get; set; }

    public string? HinhAnh { get; set; }

    [InverseProperty("MaKhachHangNavigation")]
    public virtual ICollection<DonHang> DonHangs { get; set; } = new List<DonHang>();
}
