using System;
using System.Collections.Generic;

namespace QuanLyNhaHang.Models;

public partial class PhienBanMonAn
{
    public string MaPhienBan { get; set; } = null!;

    public string TenPhienBan { get; set; } = null!;

    public string MaTrangThai { get; set; } = null!;

    public int? ThuTu { get; set; }

    public virtual ICollection<ChiTietDonHang> ChiTietDonHangs { get; set; } = new List<ChiTietDonHang>();

    public virtual ICollection<CongThucNauAn> CongThucNauAns { get; set; } = new List<CongThucNauAn>();
}
