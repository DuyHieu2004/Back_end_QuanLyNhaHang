using System;
using System.Collections.Generic;

namespace QuanLyNhaHang.Models;

public partial class VNguyenLieuCanNhapThem
{
    public string MaNguyenLieu { get; set; } = null!;

    public string TenNguyenLieu { get; set; } = null!;

    public string? DonViTinh { get; set; }

    public int SoLuongTonKho { get; set; }

    public int MucTonKhoToiThieu { get; set; }

    public int? SoLuongDeXuatNhapThem { get; set; }
}
