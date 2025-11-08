using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace QuanLyNhaHang.Models;

[Table("CheBienMonAn")]
public partial class CheBienMonAn
{
    [Key]
    [StringLength(15)]
    [Unicode(false)]
    public string MaCheBien { get; set; } = null!;

    [Column(TypeName = "datetime")]
    public DateTime? NgayNau { get; set; }

    [StringLength(15)]
    [Unicode(false)]
    public string MaPhienBan { get; set; } = null!;

    public int? SoLuong { get; set; }

    [ForeignKey("MaPhienBan")]
    [InverseProperty("CheBienMonAns")]
    public virtual PhienBanMonAn MaPhienBanNavigation { get; set; } = null!;
}
