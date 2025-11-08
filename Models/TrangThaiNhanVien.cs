using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace QuanLyNhaHang.Models;

[Table("TrangThaiNhanVien")]
public partial class TrangThaiNhanVien
{
    [Key]
    [StringLength(15)]
    [Unicode(false)]
    public string MaTrangThai { get; set; } = null!;

    [StringLength(50)]
    public string TenTrangThai { get; set; } = null!;

    [InverseProperty("MaTrangThaiNavigation")]
    public virtual ICollection<NhanVien> NhanViens { get; set; } = new List<NhanVien>();
}
