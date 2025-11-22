using System;
using System.Collections.Generic;

namespace QuanLyNhaHang.Models;

public partial class CungUng
{
    public string MaCungUng { get; set; } = null!;

    public string? MaNguyenLieu { get; set; }

    public string? MaNhaCungCap { get; set; }

    public virtual NguyenLieu? MaNguyenLieuNavigation { get; set; }

    public virtual NhaCungCap? MaNhaCungCapNavigation { get; set; }
}
