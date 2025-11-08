using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace QuanLyNhaHang.Models;

[Table("MonAn")]
public partial class MonAn
{
    [Key]
    [StringLength(15)]
    [Unicode(false)]
    public string MaMonAn { get; set; } = null!;

    [StringLength(100)]
    public string TenMonAn { get; set; } = null!;

    [StringLength(15)]
    [Unicode(false)]
    public string? MaDanhMuc { get; set; }

    [InverseProperty("MaMonAnNavigation")]
    public virtual ICollection<HinhAnhMonAn> HinhAnhMonAns { get; set; } = new List<HinhAnhMonAn>();

    [ForeignKey("MaDanhMuc")]
    [InverseProperty("MonAns")]
    public virtual DanhMucMonAn? MaDanhMucNavigation { get; set; }

    [InverseProperty("MaMonAnNavigation")]
    public virtual ICollection<PhienBanMonAn> PhienBanMonAns { get; set; } = new List<PhienBanMonAn>();
}
