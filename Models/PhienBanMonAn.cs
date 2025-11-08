using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace QuanLyNhaHang.Models;

[Table("PhienBanMonAn")]
public partial class PhienBanMonAn
{
    [Key]
    [StringLength(15)]
    [Unicode(false)]
    public string MaPhienBan { get; set; } = null!;

    [StringLength(15)]
    [Unicode(false)]
    public string MaMonAn { get; set; } = null!;

    [StringLength(100)]
    public string TenPhienBan { get; set; } = null!;

    [Column(TypeName = "decimal(10, 2)")]
    public decimal Gia { get; set; }

    [StringLength(50)]
    public string TrangThai { get; set; } = null!;

    [InverseProperty("MaPhienBanNavigation")]
    public virtual ICollection<CheBienMonAn> CheBienMonAns { get; set; } = new List<CheBienMonAn>();

    [InverseProperty("MaPhienBanNavigation")]
    public virtual ICollection<ChiTietDonHang> ChiTietDonHangs { get; set; } = new List<ChiTietDonHang>();

    [InverseProperty("MaPhienBanNavigation")]
    public virtual ICollection<CongThucNauAn> CongThucNauAns { get; set; } = new List<CongThucNauAn>();

    [ForeignKey("MaMonAn")]
    [InverseProperty("PhienBanMonAns")]
    public virtual MonAn MaMonAnNavigation { get; set; } = null!;
}
