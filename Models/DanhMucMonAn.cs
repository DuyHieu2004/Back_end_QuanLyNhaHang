using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace QuanLyNhaHang.Models;

[Table("DanhMucMonAn")]
public partial class DanhMucMonAn
{
    [Key]
    [StringLength(15)]
    [Unicode(false)]
    public string MaDanhMuc { get; set; } = null!;

    [StringLength(255)]
    public string TenDanhMuc { get; set; } = null!;

    [InverseProperty("MaDanhMucNavigation")]
    public virtual ICollection<MonAn> MonAns { get; set; } = new List<MonAn>();
}
