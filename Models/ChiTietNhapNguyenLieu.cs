using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace QuanLyNhaHang.Models;

[Table("ChiTietNhapNguyenLieu")]
public partial class ChiTietNhapNguyenLieu
{
    [Key]
    public long MaChiTietNhapHang { get; set; }

    [StringLength(15)]
    [Unicode(false)]
    public string MaNhapHang { get; set; } = null!;

    [StringLength(15)]
    [Unicode(false)]
    public string MaCungUng { get; set; } = null!;

    public int SoLuong { get; set; }

    [Column(TypeName = "decimal(10, 2)")]
    public decimal GiaNhap { get; set; }

    [ForeignKey("MaCungUng")]
    [InverseProperty("ChiTietNhapNguyenLieus")]
    public virtual CungUng MaCungUngNavigation { get; set; } = null!;

    [ForeignKey("MaNhapHang")]
    [InverseProperty("ChiTietNhapNguyenLieus")]
    public virtual NhapNguyenLieu MaNhapHangNavigation { get; set; } = null!;
}
