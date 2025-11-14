using System;
using System.Collections.Generic;

namespace QuanLyNhaHang.Models;

public partial class PhienBanMonAn
{
    public string MaPhienBan { get; set; } = null!;

    public string MaMonAn { get; set; } = null!;

    public string TenPhienBan { get; set; } = null!;

    public decimal Gia { get; set; }

    public string MaTrangThai { get; set; } = null!;

    public bool IsShow { get; set; }

    public int? ThuTu { get; set; }

    public virtual ICollection<CheBienMonAn> CheBienMonAns { get; set; } = new List<CheBienMonAn>();

    public virtual ICollection<ChiTietDonHang> ChiTietDonHangs { get; set; } = new List<ChiTietDonHang>();

    public virtual ICollection<CongThucNauAn> CongThucNauAns { get; set; } = new List<CongThucNauAn>();

    public virtual MonAn MaMonAnNavigation { get; set; } = null!;

    public virtual TrangThaiPhienBanMonAn MaTrangThaiNavigation { get; set; } = null!;
}
