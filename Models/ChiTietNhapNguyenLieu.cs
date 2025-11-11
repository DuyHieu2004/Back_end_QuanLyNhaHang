using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
namespace QuanLyNhaHang.Models;

public partial class ChiTietNhapNguyenLieu
{
    public long MaChiTietNhapHang { get; set; }

    public string MaNhapHang { get; set; } = null!;

    public string MaCungUng { get; set; } = null!;

    public int SoLuong { get; set; }
    
    [Column(TypeName = "decimal(10, 2)")]
    public decimal GiaNhap { get; set; }

    public virtual CungUng MaCungUngNavigation { get; set; } = null!;

    public virtual NhapNguyenLieu MaNhapHangNavigation { get; set; } = null!;
}
