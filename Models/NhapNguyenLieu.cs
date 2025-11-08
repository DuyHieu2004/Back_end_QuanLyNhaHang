using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace QuanLyNhaHang.Models;

[Table("NhapNguyenLieu")]
public partial class NhapNguyenLieu
{
    [Key]
    [StringLength(15)]
    [Unicode(false)]
    public string MaNhapHang { get; set; } = null!;

    [StringLength(15)]
    [Unicode(false)]
    public string MaNhanVien { get; set; } = null!;

    [Column(TypeName = "datetime")]
    public DateTime NgayNhapHang { get; set; }

    [Column(TypeName = "decimal(10, 2)")]
    public decimal TongTien { get; set; }

    [InverseProperty("MaNhapHangNavigation")]
    public virtual ICollection<ChiTietNhapNguyenLieu> ChiTietNhapNguyenLieus { get; set; } = new List<ChiTietNhapNguyenLieu>();

    [ForeignKey("MaNhanVien")]
    [InverseProperty("NhapNguyenLieus")]
    public virtual NhanVien MaNhanVienNavigation { get; set; } = null!;
}
