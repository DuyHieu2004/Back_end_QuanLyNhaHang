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

    public virtual DbSet<ChiTietCongThuc> ChiTietCongThucs { get; set; }

    public virtual DbSet<ChiTietDonHang> ChiTietDonHangs { get; set; }

    public virtual DbSet<ChiTietMenu> ChiTietMenus { get; set; }

    public virtual DbSet<ChiTietMonAn> ChiTietMonAns { get; set; }

    public virtual DbSet<ChiTietNhapHang> ChiTietNhapHangs { get; set; }

    public virtual DbSet<CongThucNauAn> CongThucNauAns { get; set; }

    public virtual DbSet<CungUng> CungUngs { get; set; }

    public virtual DbSet<DanhMucMonAn> DanhMucMonAns { get; set; }

    public virtual DbSet<DonHang> DonHangs { get; set; }

    public virtual DbSet<HinhAnhMonAn> HinhAnhMonAns { get; set; }

    public virtual DbSet<KhachHang> KhachHangs { get; set; }

    public virtual DbSet<LoaiMenu> LoaiMenus { get; set; }

    public virtual DbSet<Menu> Menus { get; set; }

    public virtual DbSet<MonAn> MonAns { get; set; }

    public virtual DbSet<NguyenLieu> NguyenLieus { get; set; }

    public virtual DbSet<NhaCungCap> NhaCungCaps { get; set; }

    public virtual DbSet<NhanVien> NhanViens { get; set; }

    public virtual DbSet<NhapHang> NhapHangs { get; set; }

    public virtual DbSet<PhienBanMonAn> PhienBanMonAns { get; set; }

    public virtual DbSet<Tang> Tangs { get; set; }

    public virtual DbSet<TrangThaiBanAn> TrangThaiBanAns { get; set; }

    public virtual DbSet<TrangThaiDonHang> TrangThaiDonHangs { get; set; }

    public virtual DbSet<TrangThaiMenu> TrangThaiMenus { get; set; }

    public virtual DbSet<VaiTro> VaiTros { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=LAPTOP-1RKGC1HF\\SQLEXPRESS;Database=QL_NhaHang_DoAn_Test2;Integrated Security=True;Encrypt=True;TrustServerCertificate=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<BanAn>(entity =>
        {
            entity.HasKey(e => e.MaBan).HasName("PK__BanAn__3520ED6C0571B471");

            entity.ToTable("BanAn");

            entity.Property(e => e.MaBan)
                .HasMaxLength(25)
                .IsUnicode(false);
            entity.Property(e => e.IsShow).HasDefaultValue(true);
            entity.Property(e => e.MaTang)
                .HasMaxLength(25)
                .IsUnicode(false);
            entity.Property(e => e.MaTrangThai)
                .HasMaxLength(25)
                .IsUnicode(false);
            entity.Property(e => e.SucChua).HasDefaultValue(4);
            entity.Property(e => e.TenBan).HasMaxLength(50);

            entity.HasOne(d => d.MaTangNavigation).WithMany(p => p.BanAns)
                .HasForeignKey(d => d.MaTang)
                .HasConstraintName("FK_BanAn_Tang");

            entity.HasOne(d => d.MaTrangThaiNavigation).WithMany(p => p.BanAns)
                .HasForeignKey(d => d.MaTrangThai)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_BanAn_TrangThaiBanAn");
        });

        modelBuilder.Entity<ChiTietCongThuc>(entity =>
        {
            entity.HasKey(e => e.MaChiTietCongThuc).HasName("PK__ChiTietC__ADE808647FF12960");

            entity.ToTable("ChiTietCongThuc");

            entity.Property(e => e.MaCongThuc)
                .HasMaxLength(25)
                .IsUnicode(false);
            entity.Property(e => e.MaNguyenLieu)
                .HasMaxLength(25)
                .IsUnicode(false);

            entity.HasOne(d => d.MaCongThucNavigation).WithMany(p => p.ChiTietCongThucs)
                .HasForeignKey(d => d.MaCongThuc)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ChiTietCo__MaCon__6FE99F9F");

            entity.HasOne(d => d.MaNguyenLieuNavigation).WithMany(p => p.ChiTietCongThucs)
                .HasForeignKey(d => d.MaNguyenLieu)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ChiTietCo__MaNgu__70DDC3D8");
        });

        modelBuilder.Entity<ChiTietDonHang>(entity =>
        {
            entity.HasKey(e => e.MaChiTietDonHang).HasName("PK__ChiTietD__4B0B45DDB09AE201");

            entity.ToTable("ChiTietDonHang");

            entity.Property(e => e.MaCongThuc)
                .HasMaxLength(25)
                .IsUnicode(false);
            entity.Property(e => e.MaDonHang)
                .HasMaxLength(25)
                .IsUnicode(false);
            entity.Property(e => e.MaPhienBan)
                .HasMaxLength(25)
                .IsUnicode(false);

            entity.HasOne(d => d.MaCongThucNavigation).WithMany(p => p.ChiTietDonHangs)
                .HasForeignKey(d => d.MaCongThuc)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ChiTietDo__MaCon__6A30C649");

            entity.HasOne(d => d.MaDonHangNavigation).WithMany(p => p.ChiTietDonHangs)
                .HasForeignKey(d => d.MaDonHang)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ChiTietDo__MaDon__68487DD7");

            entity.HasOne(d => d.MaPhienBanNavigation).WithMany(p => p.ChiTietDonHangs)
                .HasForeignKey(d => d.MaPhienBan)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ChiTietDo__MaPhi__693CA210");
        });

        modelBuilder.Entity<ChiTietMenu>(entity =>
        {
            entity.HasKey(e => e.MaChiTietMenu);

            entity.ToTable("ChiTietMenu");

            entity.HasIndex(e => e.MaCongThuc, "IX_ChiTietMenu_MaCongThuc");

            entity.HasIndex(e => e.MaMenu, "IX_ChiTietMenu_MaMenu");

            entity.Property(e => e.GhiChu).HasMaxLength(500);
            entity.Property(e => e.MaCongThuc)
                .HasMaxLength(25)
                .IsUnicode(false);
            entity.Property(e => e.MaMenu)
                .HasMaxLength(25)
                .IsUnicode(false);
            entity.Property(e => e.SoLuong).HasDefaultValue(1);

            entity.HasOne(d => d.MaCongThucNavigation).WithMany(p => p.ChiTietMenus)
                .HasForeignKey(d => d.MaCongThuc)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ChiTietMenu_CongThucNauAn");

            entity.HasOne(d => d.MaMenuNavigation).WithMany(p => p.ChiTietMenus)
                .HasForeignKey(d => d.MaMenu)
                .HasConstraintName("FK_ChiTietMenu_Menu");
        });

        modelBuilder.Entity<ChiTietMonAn>(entity =>
        {
            entity.HasKey(e => e.MaCt).HasName("PK__ChiTietM__27258E74B2914A13");

            entity.ToTable("ChiTietMonAn");

            entity.HasIndex(e => e.MaMonAn, "IX_ChiTietMonAn_MaMonAn");

            entity.Property(e => e.MaCt)
                .HasMaxLength(25)
                .IsUnicode(false)
                .HasColumnName("MaCT");
            entity.Property(e => e.MaMonAn)
                .HasMaxLength(25)
                .IsUnicode(false);
            entity.Property(e => e.TenCt)
                .HasMaxLength(100)
                .HasColumnName("TenCT");

            entity.HasOne(d => d.MaMonAnNavigation).WithMany(p => p.ChiTietMonAns)
                .HasForeignKey(d => d.MaMonAn)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ChiTietMo__MaMon__6D0D32F4");
        });

        modelBuilder.Entity<ChiTietNhapHang>(entity =>
        {
            entity.HasKey(e => e.MaChiTietNhapHang).HasName("PK__ChiTietN__9CC62AA8D5ABCF93");

            entity.ToTable("ChiTietNhapHang");

            entity.Property(e => e.GiaNhap).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.MaCungUng)
                .HasMaxLength(25)
                .IsUnicode(false);
            entity.Property(e => e.MaNhapHang)
                .HasMaxLength(25)
                .IsUnicode(false);

            entity.HasOne(d => d.MaCungUngNavigation).WithMany(p => p.ChiTietNhapHangs)
                .HasForeignKey(d => d.MaCungUng)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ChiTietNh__MaCun__6B24EA82");

            entity.HasOne(d => d.MaNhapHangNavigation).WithMany(p => p.ChiTietNhapHangs)
                .HasForeignKey(d => d.MaNhapHang)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ChiTietNh__MaNha__6C190EBB");
        });

        modelBuilder.Entity<CongThucNauAn>(entity =>
        {
            entity.HasKey(e => e.MaCongThuc).HasName("PK__CongThuc__6E223AF7441CD06A");

            entity.ToTable("CongThucNauAn");

            entity.HasIndex(e => e.MaCt, "IX_CongThucNauAn_MaCT");

            entity.HasIndex(e => e.MaPhienBan, "IX_CongThucNauAn_MaPhienBan");

            entity.Property(e => e.MaCongThuc)
                .HasMaxLength(25)
                .IsUnicode(false);
            entity.Property(e => e.Gia).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.MaCt)
                .HasMaxLength(25)
                .IsUnicode(false)
                .HasColumnName("MaCT");
            entity.Property(e => e.MaPhienBan)
                .HasMaxLength(25)
                .IsUnicode(false);

            entity.HasOne(d => d.MaCtNavigation).WithMany(p => p.CongThucNauAns)
                .HasForeignKey(d => d.MaCt)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__CongThucNa__MaCT__6E01572D");

            entity.HasOne(d => d.MaPhienBanNavigation).WithMany(p => p.CongThucNauAns)
                .HasForeignKey(d => d.MaPhienBan)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__CongThucN__MaPhi__6EF57B66");
        });

        modelBuilder.Entity<CungUng>(entity =>
        {
            entity.HasKey(e => e.MaCungUng).HasName("PK__CungUng__CB2EB549E655561E");

            entity.ToTable("CungUng");

            entity.Property(e => e.MaCungUng)
                .HasMaxLength(25)
                .IsUnicode(false);
            entity.Property(e => e.MaNguyenLieu)
                .HasMaxLength(25)
                .IsUnicode(false);
            entity.Property(e => e.MaNhaCungCap)
                .HasMaxLength(25)
                .IsUnicode(false);

            entity.HasOne(d => d.MaNguyenLieuNavigation).WithMany(p => p.CungUngs)
                .HasForeignKey(d => d.MaNguyenLieu)
                .HasConstraintName("FK__CungUng__MaNguye__71D1E811");

            entity.HasOne(d => d.MaNhaCungCapNavigation).WithMany(p => p.CungUngs)
                .HasForeignKey(d => d.MaNhaCungCap)
                .HasConstraintName("FK__CungUng__MaNhaCu__72C60C4A");
        });

        modelBuilder.Entity<DanhMucMonAn>(entity =>
        {
            entity.HasKey(e => e.MaDanhMuc).HasName("PK__DanhMucM__B3750887016388AD");

            entity.ToTable("DanhMucMonAn");

            entity.Property(e => e.MaDanhMuc)
                .HasMaxLength(25)
                .IsUnicode(false);
            entity.Property(e => e.TenDanhMuc).HasMaxLength(255);
        });

        modelBuilder.Entity<DonHang>(entity =>
        {
            entity.HasKey(e => e.MaDonHang).HasName("PK__DonHang__129584ADCC454E1D");

            entity.ToTable("DonHang", tb => tb.HasTrigger("trg_OnDonHangUpdate_IncrementNoShow"));

            

            entity.Property(e => e.MaDonHang)
                .HasMaxLength(25)
                .IsUnicode(false);
            entity.Property(e => e.EmailNguoiNhan).HasMaxLength(100);
            entity.Property(e => e.GhiChu).HasMaxLength(500);
            entity.Property(e => e.MaKhachHang)
                .HasMaxLength(25)
                .IsUnicode(false);
            entity.Property(e => e.MaNhanVien)
                .HasMaxLength(25)
                .IsUnicode(false);
            entity.Property(e => e.MaTrangThaiDonHang)
                .HasMaxLength(25)
                .IsUnicode(false)
                .HasDefaultValue("CHO_XAC_NHAN");
            entity.Property(e => e.SDTNguoiNhan)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("SDTNguoiNhan");
            entity.Property(e => e.SoLuongNguoiDk)
                .HasDefaultValue(1)
                .HasColumnName("SoLuongNguoiDK");
            entity.Property(e => e.TenNguoiNhan).HasMaxLength(100);
            entity.Property(e => e.TgdatDuKien)
                .HasColumnType("datetime")
                .HasColumnName("TGDatDuKien");
            entity.Property(e => e.TgnhanBan)
                .HasColumnType("datetime")
                .HasColumnName("TGNhanBan");
            entity.Property(e => e.ThoiGianDatHang).HasColumnType("datetime");
            entity.Property(e => e.ThoiGianKetThuc).HasColumnType("datetime");
            entity.Property(e => e.TienDatCoc)
                .HasDefaultValue(0m)
                .HasColumnType("decimal(10, 2)");

            entity.HasOne(d => d.MaKhachHangNavigation).WithMany(p => p.DonHangs)
                .HasForeignKey(d => d.MaKhachHang)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__DonHang__MaKhach__73BA3083");

            entity.HasOne(d => d.MaNhanVienNavigation).WithMany(p => p.DonHangs)
                .HasForeignKey(d => d.MaNhanVien)
                .HasConstraintName("FK__DonHang__MaNhanV__74AE54BC");

            entity.HasOne(d => d.MaTrangThaiDonHangNavigation).WithMany(p => p.DonHangs)
                .HasForeignKey(d => d.MaTrangThaiDonHang)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DonHang_TrangThaiDonHang");

            entity.HasMany(d => d.MaBans).WithMany(p => p.MaDonHangs)
                .UsingEntity<Dictionary<string, object>>(
                    "BanAnDonHang",
                    r => r.HasOne<BanAn>().WithMany()
                        .HasForeignKey("MaBan")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_BanAnDonHang_BanAn"),
                    l => l.HasOne<DonHang>().WithMany()
                        .HasForeignKey("MaDonHang")
                        .HasConstraintName("FK_BanAnDonHang_DonHang"),
                    j =>
                    {
                        j.HasKey("MaDonHang", "MaBan").HasName("PK__BanAnDon__C1C78A7B4CC56DE7");
                        j.ToTable("BanAnDonHang");
                        j.HasIndex(new[] { "MaBan" }, "IX_BanAnDonHang_MaBan");
                        j.IndexerProperty<string>("MaDonHang")
                            .HasMaxLength(25)
                            .IsUnicode(false);
                        j.IndexerProperty<string>("MaBan")
                            .HasMaxLength(25)
                            .IsUnicode(false);
                    });
        });

        modelBuilder.Entity<HinhAnhMonAn>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__HinhAnhM__3214EC0700664A4E");

            entity.ToTable("HinhAnhMonAn");

            entity.Property(e => e.MaMonAn)
                .HasMaxLength(25)
                .IsUnicode(false);
            entity.Property(e => e.URLHinhAnh).HasColumnName("URLHinhAnh");

            entity.HasOne(d => d.MaMonAnNavigation).WithMany(p => p.HinhAnhMonAns)
                .HasForeignKey(d => d.MaMonAn)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__HinhAnhMo__MaMon__76969D2E");
        });

        modelBuilder.Entity<KhachHang>(entity =>
        {
            entity.HasKey(e => e.MaKhachHang).HasName("PK__KhachHan__88D2F0E557BEACA7");

            entity.ToTable("KhachHang");

            entity.HasIndex(e => e.Email, "IX_KhachHang_Email_Unique")
                .IsUnique()
                .HasFilter("([Email] IS NOT NULL)");

            entity.Property(e => e.MaKhachHang)
                .HasMaxLength(25)
                .IsUnicode(false);
            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.HoTen).HasMaxLength(100);
            entity.Property(e => e.NoShowCount).HasDefaultValue(0);
            entity.Property(e => e.SoDienThoai).HasMaxLength(15);
        });

        modelBuilder.Entity<LoaiMenu>(entity =>
        {
            entity.HasKey(e => e.MaLoaiMenu);

            entity.ToTable("LoaiMenu");

            entity.Property(e => e.MaLoaiMenu)
                .HasMaxLength(25)
                .IsUnicode(false);
            entity.Property(e => e.MoTa).HasMaxLength(500);
            entity.Property(e => e.TenLoaiMenu).HasMaxLength(100);
        });

        modelBuilder.Entity<Menu>(entity =>
        {
            entity.HasKey(e => e.MaMenu);

            entity.ToTable("Menu");

            entity.HasIndex(e => e.IsShow, "IX_Menu_IsShow");

            entity.HasIndex(e => e.MaLoaiMenu, "IX_Menu_MaLoaiMenu");

            entity.HasIndex(e => e.MaTrangThai, "IX_Menu_MaTrangThai");

            entity.Property(e => e.MaMenu)
                .HasMaxLength(25)
                .IsUnicode(false);
            entity.Property(e => e.GiaGoc).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.GiaMenu).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.IsShow).HasDefaultValue(true);
            entity.Property(e => e.MaLoaiMenu)
                .HasMaxLength(25)
                .IsUnicode(false);
            entity.Property(e => e.MaTrangThai)
                .HasMaxLength(25)
                .IsUnicode(false);
            entity.Property(e => e.MoTa).HasMaxLength(1000);
            entity.Property(e => e.NgayBatDau).HasColumnType("datetime");
            entity.Property(e => e.NgayCapNhat).HasColumnType("datetime");
            entity.Property(e => e.NgayKetThuc).HasColumnType("datetime");
            entity.Property(e => e.NgayTao)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.TenMenu).HasMaxLength(200);

            entity.HasOne(d => d.MaLoaiMenuNavigation).WithMany(p => p.Menus)
                .HasForeignKey(d => d.MaLoaiMenu)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Menu_LoaiMenu");

            entity.HasOne(d => d.MaTrangThaiNavigation).WithMany(p => p.Menus)
                .HasForeignKey(d => d.MaTrangThai)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Menu_TrangThaiMenu");
        });

        modelBuilder.Entity<MonAn>(entity =>
        {
            entity.HasKey(e => e.MaMonAn).HasName("PK__MonAn__B1171625AA9225CF");

            entity.ToTable("MonAn");

            entity.Property(e => e.MaMonAn)
                .HasMaxLength(25)
                .IsUnicode(false);
            entity.Property(e => e.IsShow).HasDefaultValue(true);
            entity.Property(e => e.MaDanhMuc)
                .HasMaxLength(25)
                .IsUnicode(false);
            entity.Property(e => e.TenMonAn).HasMaxLength(100);

            entity.HasOne(d => d.MaDanhMucNavigation).WithMany(p => p.MonAns)
                .HasForeignKey(d => d.MaDanhMuc)
                .HasConstraintName("FK__MonAn__MaDanhMuc__778AC167");
        });

        modelBuilder.Entity<NguyenLieu>(entity =>
        {
            entity.HasKey(e => e.MaNguyenLieu).HasName("PK__NguyenLi__C751935516B4C656");

            entity.ToTable("NguyenLieu");

            entity.Property(e => e.MaNguyenLieu)
                .HasMaxLength(25)
                .IsUnicode(false);
            entity.Property(e => e.DonViTinh).HasMaxLength(50);
            entity.Property(e => e.TenNguyenLieu).HasMaxLength(100);
        });

        modelBuilder.Entity<NhaCungCap>(entity =>
        {
            entity.HasKey(e => e.MaNhaCungCap).HasName("PK__NhaCungC__53DA92050CD362C8");

            entity.ToTable("NhaCungCap");

            entity.Property(e => e.MaNhaCungCap)
                .HasMaxLength(25)
                .IsUnicode(false);
            entity.Property(e => e.DiaChi).HasMaxLength(255);
            entity.Property(e => e.SoDienThoai).HasMaxLength(15);
            entity.Property(e => e.TenNhaCungCap).HasMaxLength(255);
        });

        modelBuilder.Entity<NhanVien>(entity =>
        {
            entity.HasKey(e => e.MaNhanVien).HasName("PK__NhanVien__77B2CA471BAD3E5B");

            entity.ToTable("NhanVien");

            entity.HasIndex(e => e.Email, "IX_NhanVien_Email_Unique")
                .IsUnique()
                .HasFilter("([Email] IS NOT NULL)");

            entity.HasIndex(e => e.TenDangNhap, "UQ__NhanVien__55F68FC077A7032A").IsUnique();

            entity.Property(e => e.MaNhanVien)
                .HasMaxLength(25)
                .IsUnicode(false);
            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.HoTen).HasMaxLength(100);
            entity.Property(e => e.MaVaiTro)
                .HasMaxLength(25)
                .IsUnicode(false);
            entity.Property(e => e.MatKhau).HasMaxLength(256);
            entity.Property(e => e.SoDienThoai).HasMaxLength(15);
            entity.Property(e => e.TenDangNhap).HasMaxLength(50);

            entity.HasOne(d => d.MaVaiTroNavigation).WithMany(p => p.NhanViens)
                .HasForeignKey(d => d.MaVaiTro)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__NhanVien__MaVaiT__787EE5A0");
        });

        modelBuilder.Entity<NhapHang>(entity =>
        {
            entity.HasKey(e => e.MaNhapHang).HasName("PK__NhapHang__42ECBDEA48EDE74E");

            entity.ToTable("NhapHang");

            entity.Property(e => e.MaNhapHang)
                .HasMaxLength(25)
                .IsUnicode(false);
            entity.Property(e => e.MaNhanVien)
                .HasMaxLength(25)
                .IsUnicode(false);
            entity.Property(e => e.NgayNhapHang).HasColumnType("datetime");
            entity.Property(e => e.TongTien).HasColumnType("decimal(10, 2)");

            entity.HasOne(d => d.MaNhanVienNavigation).WithMany(p => p.NhapHangs)
                .HasForeignKey(d => d.MaNhanVien)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__NhapHang__MaNhan__797309D9");
        });

        modelBuilder.Entity<PhienBanMonAn>(entity =>
        {
            entity.HasKey(e => e.MaPhienBan).HasName("PK__PhienBan__DB433AD4E3B509F9");

            entity.ToTable("PhienBanMonAn");

            entity.Property(e => e.MaPhienBan)
                .HasMaxLength(25)
                .IsUnicode(false);
            entity.Property(e => e.MaTrangThai)
                .HasMaxLength(25)
                .IsUnicode(false)
                .HasDefaultValue("CON_HANG");
            entity.Property(e => e.TenPhienBan).HasMaxLength(100);
        });

        modelBuilder.Entity<Tang>(entity =>
        {
            entity.HasKey(e => e.MaTang);

            entity.ToTable("Tang");

            entity.Property(e => e.MaTang)
                .HasMaxLength(25)
                .IsUnicode(false);
            entity.Property(e => e.TenTang).HasMaxLength(50);
        });

        modelBuilder.Entity<TrangThaiBanAn>(entity =>
        {
            entity.HasKey(e => e.MaTrangThai).HasName("PK__TrangTha__AADE41388641E3CA");

            entity.ToTable("TrangThaiBanAn");

            entity.Property(e => e.MaTrangThai)
                .HasMaxLength(25)
                .IsUnicode(false);
            entity.Property(e => e.TenTrangThai).HasMaxLength(50);
        });

        modelBuilder.Entity<TrangThaiDonHang>(entity =>
        {
            entity.HasKey(e => e.MaTrangThai).HasName("PK__TrangTha__AADE4138D519FEDE");

            entity.ToTable("TrangThaiDonHang");

            entity.Property(e => e.MaTrangThai)
                .HasMaxLength(25)
                .IsUnicode(false);
            entity.Property(e => e.TenTrangThai).HasMaxLength(100);
        });

        modelBuilder.Entity<TrangThaiMenu>(entity =>
        {
            entity.HasKey(e => e.MaTrangThai);

            entity.ToTable("TrangThaiMenu");

            entity.Property(e => e.MaTrangThai)
                .HasMaxLength(25)
                .IsUnicode(false);
            entity.Property(e => e.TenTrangThai).HasMaxLength(50);
        });

        modelBuilder.Entity<VaiTro>(entity =>
        {
            entity.HasKey(e => e.MaVaiTro).HasName("PK__VaiTro__C24C41CF9CB9A5DF");

            entity.ToTable("VaiTro");

            entity.HasIndex(e => e.TenVaiTro, "UQ__VaiTro__1DA5581432A308F7").IsUnique();

            entity.Property(e => e.MaVaiTro)
                .HasMaxLength(25)
                .IsUnicode(false);
            entity.Property(e => e.TenVaiTro).HasMaxLength(50);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
