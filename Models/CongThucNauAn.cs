using System;
using System.Collections.Generic;

namespace QuanLyNhaHang.Models;

public partial class CongThucNauAn
{
    public string MaCongThuc { get; set; } = null!;

    public string MaNguyenLieu { get; set; } = null!;

    public string MaPhienBan { get; set; } = null!;

    public int SoLuongCanDung { get; set; }

    public virtual NguyenLieu MaNguyenLieuNavigation { get; set; } = null!;

    public virtual PhienBanMonAn MaPhienBanNavigation { get; set; } = null!;
}
