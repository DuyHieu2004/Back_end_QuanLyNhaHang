using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace QuanLyNhaHang.Models;

[Table("NhaCungCap")]
public partial class NhaCungCap
{
    [Key]
    [StringLength(15)]
    [Unicode(false)]
    public string MaNhaCungCap { get; set; } = null!;

    [StringLength(255)]
    public string TenNhaCungCap { get; set; } = null!;

    [StringLength(15)]
    public string SoDienThoai { get; set; } = null!;

    [StringLength(255)]
    public string? DiaChi { get; set; }

    [InverseProperty("MaNhaCungCapNavigation")]
    public virtual ICollection<CungUng> CungUngs { get; set; } = new List<CungUng>();
}
