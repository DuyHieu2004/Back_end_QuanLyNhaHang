using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace QuanLyNhaHang.Models;

[Table("ChiTietDonHang")]
public partial class ChiTietDonHang
{
    [Key]
    public long MaChiTietDonHang { get; set; }

    [StringLength(15)]
    [Unicode(false)]
    public string MaDonHang { get; set; } = null!;

    [StringLength(15)]
    [Unicode(false)]
    public string MaPhienBan { get; set; } = null!;

    public int SoLuong { get; set; }

    [ForeignKey("MaDonHang")]
    [InverseProperty("ChiTietDonHangs")]
    public virtual DonHang MaDonHangNavigation { get; set; } = null!;

    [ForeignKey("MaPhienBan")]
    [InverseProperty("ChiTietDonHangs")]
    public virtual PhienBanMonAn MaPhienBanNavigation { get; set; } = null!;
}
