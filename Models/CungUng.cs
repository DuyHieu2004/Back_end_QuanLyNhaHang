using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace QuanLyNhaHang.Models;

[Table("CungUng")]
public partial class CungUng
{
    [Key]
    [StringLength(15)]
    [Unicode(false)]
    public string MaCungUng { get; set; } = null!;

    [StringLength(15)]
    [Unicode(false)]
    public string? MaNguyenLieu { get; set; }

    [StringLength(15)]
    [Unicode(false)]
    public string? MaNhaCungCap { get; set; }

    [InverseProperty("MaCungUngNavigation")]
    public virtual ICollection<ChiTietNhapNguyenLieu> ChiTietNhapNguyenLieus { get; set; } = new List<ChiTietNhapNguyenLieu>();

    [ForeignKey("MaNguyenLieu")]
    [InverseProperty("CungUngs")]
    public virtual NguyenLieu? MaNguyenLieuNavigation { get; set; }

    [ForeignKey("MaNhaCungCap")]
    [InverseProperty("CungUngs")]
    public virtual NhaCungCap? MaNhaCungCapNavigation { get; set; }
}
