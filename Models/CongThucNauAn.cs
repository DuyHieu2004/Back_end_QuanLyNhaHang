using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace QuanLyNhaHang.Models;

[Table("CongThucNauAn")]
public partial class CongThucNauAn
{
    [Key]
    [StringLength(15)]
    [Unicode(false)]
    public string MaCongThuc { get; set; } = null!;

    [StringLength(15)]
    [Unicode(false)]
    public string MaNguyenLieu { get; set; } = null!;

    [StringLength(15)]
    [Unicode(false)]
    public string MaPhienBan { get; set; } = null!;

    public int SoLuongCanDung { get; set; }

    [ForeignKey("MaNguyenLieu")]
    [InverseProperty("CongThucNauAns")]
    public virtual NguyenLieu MaNguyenLieuNavigation { get; set; } = null!;

    [ForeignKey("MaPhienBan")]
    [InverseProperty("CongThucNauAns")]
    public virtual PhienBanMonAn MaPhienBanNavigation { get; set; } = null!;
}
