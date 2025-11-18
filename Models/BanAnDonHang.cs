using System;
using System.Collections.Generic;

namespace QuanLyNhaHang.Models;

public partial class BanAnDonHang
{
    public string MaBanAnDonHang { get; set; } = null!;

    public string MaDonHang { get; set; } = null!;

    public string MaBan { get; set; } = null!;

    // Navigation: Trỏ về Đơn Hàng cha
    public virtual DonHang MaDonHangNavigation { get; set; } = null!;

    // Navigation: Trỏ về Bàn Ăn
    public virtual BanAn MaBanNavigation { get; set; } = null!;

    // QUAN TRỌNG: Một "Bàn trong Đơn" sẽ chứa nhiều món ăn (Chi tiết đơn hàng)
    public virtual ICollection<ChiTietDonHang> ChiTietDonHangs { get; set; } = new List<ChiTietDonHang>();
}