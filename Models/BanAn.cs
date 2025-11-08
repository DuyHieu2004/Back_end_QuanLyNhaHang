using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace QuanLyNhaHang.Models;

[Table("BanAn")]
public partial class BanAn
{
    [Key]
    [StringLength(15)]
    [Unicode(false)]
    public string MaBan { get; set; } = null!;

    [StringLength(50)]
    public string TenBan { get; set; } = null!;

    [StringLength(15)]
    [Unicode(false)]
    public string MaTrangThai { get; set; } = null!;

    public int SucChua { get; set; }

    [InverseProperty("MaBanNavigation")]
    public virtual ICollection<DonHang> DonHangs { get; set; } = new List<DonHang>();

    [ForeignKey("MaTrangThai")]
    [InverseProperty("BanAns")]
    public virtual TrangThaiBanAn MaTrangThaiNavigation { get; set; } = null!;
}
