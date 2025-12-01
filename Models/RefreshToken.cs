using System;
using System.Collections.Generic;

namespace QuanLyNhaHang.Models;

public partial class RefreshToken
{
    public int Id { get; set; }

    public string Token { get; set; } = null!;

    public string UserId { get; set; } = null!;

    public DateTime Expires { get; set; }

    public bool IsRevoked { get; set; }
}
