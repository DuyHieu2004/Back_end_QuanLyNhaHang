using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace QuanLyNhaHang.Models;

[Table("HinhAnhMonAn")]
public partial class HinhAnhMonAn
{
    [Key]
    public int Id { get; set; }

    [StringLength(15)]
    [Unicode(false)]
    public string MaMonAn { get; set; } = null!;

    [Column("URLHinhAnh")]
    public string URLHinhAnh { get; set; } = null!;

    [ForeignKey("MaMonAn")]
    [InverseProperty("HinhAnhMonAns")]
    public virtual MonAn MaMonAnNavigation { get; set; } = null!;
}
