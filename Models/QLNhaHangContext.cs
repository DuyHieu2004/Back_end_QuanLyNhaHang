using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace QuanLyNhaHang.Models;

public partial class QLNhaHangContext : DbContext
{
    public QLNhaHangContext()
    {
    }

    public QLNhaHangContext(DbContextOptions<QLNhaHangContext> options)
        : base(options)
    {
    }

    public virtual DbSet<BanAn> BanAns { get; set; }

    public virtual DbSet<CheBienMonAn> CheBienMonAns { get; set; }

    public virtual DbSet<ChiTietDonHang> ChiTietDonHangs { get; set; }

    public virtual DbSet<ChiTietNhapNguyenLieu> ChiTietNhapNguyenLieus { get; set; }

    public virtual DbSet<CongThucNauAn> CongThucNauAns { get; set; }

    public virtual DbSet<CungUng> CungUngs { get; set; }

    public virtual DbSet<DanhMucMonAn> DanhMucMonAns { get; set; }

    public virtual DbSet<DonHang> DonHangs { get; set; }

    public virtual DbSet<HinhAnhMonAn> HinhAnhMonAns { get; set; }

    public virtual DbSet<KhachHang> KhachHangs { get; set; }

    public virtual DbSet<MonAn> MonAns { get; set; }

    public virtual DbSet<NguyenLieu> NguyenLieus { get; set; }

    public virtual DbSet<NhaCungCap> NhaCungCaps { get; set; }

    public virtual DbSet<NhanVien> NhanViens { get; set; }

    public virtual DbSet<NhapNguyenLieu> NhapNguyenLieus { get; set; }

    public virtual DbSet<PhienBanMonAn> PhienBanMonAns { get; set; }

    public virtual DbSet<TrangThaiBanAn> TrangThaiBanAns { get; set; }

    public virtual DbSet<TrangThaiNhanVien> TrangThaiNhanViens { get; set; }

    public virtual DbSet<VaiTro> VaiTros { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=LAPTOP-1RKGC1HF\\SQLEXPRESS;Database=QL_NhaHang_DoAn_Test2;Trusted_Connection=True;TrustServerCertificate=True");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<BanAn>(entity =>
        {
            entity.HasKey(e => e.MaBan).HasName("PK__BanAn__3520ED6C0C15D8DB");

            entity.Property(e => e.SucChua).HasDefaultValue(4);

            entity.HasOne(d => d.MaTrangThaiNavigation).WithMany(p => p.BanAns)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_BanAn_TrangThaiBanAn");
        });

        modelBuilder.Entity<CheBienMonAn>(entity =>
        {
            entity.HasKey(e => e.MaCheBien).HasName("PK__CheBienM__B16F8A8F022EAA42");

            entity.HasOne(d => d.MaPhienBanNavigation).WithMany(p => p.CheBienMonAns)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__CheBienMo__MaPhi__534D60F1");
        });

        modelBuilder.Entity<ChiTietDonHang>(entity =>
        {
            entity.HasKey(e => e.MaChiTietDonHang).HasName("PK__ChiTietD__4B0B45DD70866446");

            entity.HasOne(d => d.MaDonHangNavigation).WithMany(p => p.ChiTietDonHangs)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ChiTietDo__MaDon__5FB337D6");

            entity.HasOne(d => d.MaPhienBanNavigation).WithMany(p => p.ChiTietDonHangs)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ChiTietDo__MaPhi__60A75C0F");
        });

        modelBuilder.Entity<ChiTietNhapNguyenLieu>(entity =>
        {
            entity.HasKey(e => e.MaChiTietNhapHang).HasName("PK__ChiTietN__9CC62AA886CF2BA3");

            entity.ToTable("ChiTietNhapNguyenLieu", tb => tb.HasTrigger("trg_ThemChiTietNhap"));

            entity.HasOne(d => d.MaCungUngNavigation).WithMany(p => p.ChiTietNhapNguyenLieus)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ChiTietNh__MaCun__72C60C4A");

            entity.HasOne(d => d.MaNhapHangNavigation).WithMany(p => p.ChiTietNhapNguyenLieus)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ChiTietNh__MaNha__71D1E811");
        });

        modelBuilder.Entity<CongThucNauAn>(entity =>
        {
            entity.HasKey(e => e.MaCongThuc).HasName("PK__CongThuc__6E223AF792F2DABB");

            entity.HasOne(d => d.MaNguyenLieuNavigation).WithMany(p => p.CongThucNauAns)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__CongThucN__MaNgu__6754599E");

            entity.HasOne(d => d.MaPhienBanNavigation).WithMany(p => p.CongThucNauAns)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__CongThucN__MaPhi__68487DD7");
        });

        modelBuilder.Entity<CungUng>(entity =>
        {
            entity.HasKey(e => e.MaCungUng).HasName("PK__CungUng__CB2EB54997BBCBF3");

            entity.HasOne(d => d.MaNguyenLieuNavigation).WithMany(p => p.CungUngs).HasConstraintName("FK__CungUng__MaNguye__6E01572D");

            entity.HasOne(d => d.MaNhaCungCapNavigation).WithMany(p => p.CungUngs).HasConstraintName("FK__CungUng__MaNhaCu__6EF57B66");
        });

        modelBuilder.Entity<DanhMucMonAn>(entity =>
        {
            entity.HasKey(e => e.MaDanhMuc).HasName("PK__DanhMucM__B375088704C17E51");
        });

        modelBuilder.Entity<DonHang>(entity =>
        {
            entity.HasKey(e => e.MaDonHang).HasName("PK__DonHang__129584ADB7801C36");

            entity.ToTable("DonHang", tb =>
                {
                    tb.HasTrigger("trg_UpdateBanStatusOnChange");
                    tb.HasTrigger("trg_UpdateTrangThaiBanAn");
                    tb.HasTrigger("trg_UpdateTrangThaiBanAn_ThoiGianKetThuc");
                });

            entity.Property(e => e.SoLuongNguoi).HasDefaultValue(1);

            entity.HasOne(d => d.MaBanNavigation).WithMany(p => p.DonHangs)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__DonHang__MaBan__5AEE82B9");

            entity.HasOne(d => d.MaKhachHangNavigation).WithMany(p => p.DonHangs)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__DonHang__MaKhach__5CD6CB2B");

            entity.HasOne(d => d.MaNhanVienNavigation).WithMany(p => p.DonHangs).HasConstraintName("FK__DonHang__MaNhanV__5BE2A6F2");
        });

        modelBuilder.Entity<HinhAnhMonAn>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__HinhAnhM__3214EC077326ACA6");

            entity.HasOne(d => d.MaMonAnNavigation).WithMany(p => p.HinhAnhMonAns)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__HinhAnhMo__MaMon__5070F446");
        });

        modelBuilder.Entity<KhachHang>(entity =>
        {
            entity.HasKey(e => e.MaKhachHang).HasName("PK__KhachHan__88D2F0E54D13E82A");
        });

        modelBuilder.Entity<MonAn>(entity =>
        {
            entity.HasKey(e => e.MaMonAn).HasName("PK__MonAn__B1171625E6A388E3");

            entity.HasOne(d => d.MaDanhMucNavigation).WithMany(p => p.MonAns).HasConstraintName("FK__MonAn__MaDanhMuc__4AB81AF0");
        });

        modelBuilder.Entity<NguyenLieu>(entity =>
        {
            entity.HasKey(e => e.MaNguyenLieu).HasName("PK__NguyenLi__C751935568D61FCC");
        });

        modelBuilder.Entity<NhaCungCap>(entity =>
        {
            entity.HasKey(e => e.MaNhaCungCap).HasName("PK__NhaCungC__53DA92054068CFDA");
        });

        modelBuilder.Entity<NhanVien>(entity =>
        {
            entity.HasKey(e => e.MaNhanVien).HasName("PK__NhanVien__77B2CA476BC57CA5");

            entity.HasOne(d => d.MaTrangThaiNavigation).WithMany(p => p.NhanViens)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__NhanVien__MaTran__403A8C7D");

            entity.HasOne(d => d.MaVaiTroNavigation).WithMany(p => p.NhanViens)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__NhanVien__MaVaiT__3F466844");
        });

        modelBuilder.Entity<NhapNguyenLieu>(entity =>
        {
            entity.HasKey(e => e.MaNhapHang).HasName("PK__NhapNguy__42ECBDEAE1DA6CE1");

            entity.HasOne(d => d.MaNhanVienNavigation).WithMany(p => p.NhapNguyenLieus)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__NhapNguye__MaNha__6B24EA82");
        });

        modelBuilder.Entity<PhienBanMonAn>(entity =>
        {
            entity.HasKey(e => e.MaPhienBan).HasName("PK__PhienBan__DB433AD421589442");

            entity.HasOne(d => d.MaMonAnNavigation).WithMany(p => p.PhienBanMonAns)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__PhienBanM__MaMon__4D94879B");
        });

        modelBuilder.Entity<TrangThaiBanAn>(entity =>
        {
            entity.HasKey(e => e.MaTrangThai).HasName("PK__TrangTha__AADE413819F425AF");
        });

        modelBuilder.Entity<TrangThaiNhanVien>(entity =>
        {
            entity.HasKey(e => e.MaTrangThai).HasName("PK__TrangTha__AADE4138DD611F67");
        });

        modelBuilder.Entity<VaiTro>(entity =>
        {
            entity.HasKey(e => e.MaVaiTro).HasName("PK__VaiTro__C24C41CFE039AB67");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
