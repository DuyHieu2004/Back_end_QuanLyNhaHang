using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace QuanLyNhaHang.Models;

[Table("NguyenLieu")]
public partial class NguyenLieu
{
    [Key]
    [StringLength(15)]
    [Unicode(false)]
    public string MaNguyenLieu { get; set; } = null!;

    [StringLength(100)]
    public string TenNguyenLieu { get; set; } = null!;

    [StringLength(50)]
    public string? DonViTinh { get; set; }

    public int SoLuongTonKho { get; set; }

    [InverseProperty("MaNguyenLieuNavigation")]
    public virtual ICollection<CongThucNauAn> CongThucNauAns { get; set; } = new List<CongThucNauAn>();

    [InverseProperty("MaNguyenLieuNavigation")]
    public virtual ICollection<CungUng> CungUngs { get; set; } = new List<CungUng>();
}
