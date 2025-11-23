using System;
using System.Collections.Generic;

namespace QuanLyNhaHang.Models;

public partial class KhuyenMaiApDungSanPham
{
    public long Id { get; set; }

    public string MaKhuyenMai { get; set; } = null!;

    public string? MaCongThuc { get; set; }

    public string? MaDanhMuc { get; set; }

    public virtual CongThucNauAn? MaCongThucNavigation { get; set; }

    public virtual DanhMucMonAn? MaDanhMucNavigation { get; set; }

    public virtual KhuyenMai MaKhuyenMaiNavigation { get; set; } = null!;
}
