using System;
using System.Collections.Generic;

namespace QuanLyNhaHang.Models;

public partial class ChiTietCongThuc
{
    public long MaChiTietCongThuc { get; set; }

    public string MaCongThuc { get; set; } = null!;

    public string MaNguyenLieu { get; set; } = null!;

    public int SoLuongCanDung { get; set; }

    public virtual CongThucNauAn MaCongThucNavigation { get; set; } = null!;

    public virtual NguyenLieu MaNguyenLieuNavigation { get; set; } = null!;
}
