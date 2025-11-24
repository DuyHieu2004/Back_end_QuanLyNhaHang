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

    public virtual DbSet<BanAnDonHang> BanAnDonHangs { get; set; }

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

    public virtual DbSet<TrangThaiNhapHang> TrangThaiNhapHangs { get; set; }

    public virtual DbSet<VNguyenLieuCanNhapThem> VNguyenLieuCanNhapThems { get; set; }

    public virtual DbSet<VaiTro> VaiTros { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=LAPTOP-EA4M1P0G;Database=QL_NhaHang_DoAn_Test2;Trusted_Connection=True;TrustServerCertificate=True");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<BanAn>(entity =>
        {
            entity.HasKey(e => e.MaBan).HasName("PK__BanAn__3520ED6C51A59FD7");

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

        modelBuilder.Entity<BanAnDonHang>(entity =>
        {
            entity.HasKey(e => e.MaBanAnDonHang).HasName("PK__BanAnDon__05A963CFE814AFF9");

            entity.ToTable("BanAnDonHang");

            entity.Property(e => e.MaBanAnDonHang)
                .HasMaxLength(25)
                .IsUnicode(false);
            entity.Property(e => e.MaBan)
                .HasMaxLength(25)
                .IsUnicode(false);
            entity.Property(e => e.MaDonHang)
                .HasMaxLength(25)
                .IsUnicode(false);

            entity.HasOne(d => d.MaBanNavigation).WithMany(p => p.BanAnDonHangs)
                .HasForeignKey(d => d.MaBan)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_BanAnDonHang_BanAn");

            entity.HasOne(d => d.MaDonHangNavigation).WithMany(p => p.BanAnDonHangs)
                .HasForeignKey(d => d.MaDonHang)
                .HasConstraintName("FK_BanAnDonHang_DonHang");
        });

        modelBuilder.Entity<ChiTietCongThuc>(entity =>
        {
            entity.HasKey(e => e.MaChiTietCongThuc).HasName("PK__ChiTietC__ADE8086451315223");

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
                .HasConstraintName("FK_ChiTietCongThuc_CongThuc");

            entity.HasOne(d => d.MaNguyenLieuNavigation).WithMany(p => p.ChiTietCongThucs)
                .HasForeignKey(d => d.MaNguyenLieu)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ChiTietCongThuc_NguyenLieu");
        });

        modelBuilder.Entity<ChiTietDonHang>(entity =>
        {
            entity.HasKey(e => e.MaChiTietDonHang).HasName("PK__ChiTietD__4B0B45DD5332D744");

            entity.ToTable("ChiTietDonHang");

            entity.Property(e => e.MaBanAnDonHang)
                .HasMaxLength(25)
                .IsUnicode(false);
            entity.Property(e => e.MaCongThuc)
                .HasMaxLength(25)
                .IsUnicode(false);
            entity.Property(e => e.MaPhienBan)
                .HasMaxLength(25)
                .IsUnicode(false);

            entity.HasOne(d => d.MaBanAnDonHangNavigation).WithMany(p => p.ChiTietDonHangs)
                .HasForeignKey(d => d.MaBanAnDonHang)
                .HasConstraintName("FK_ChiTietDonHang_BanAnDonHang");

            entity.HasOne(d => d.MaCongThucNavigation).WithMany(p => p.ChiTietDonHangs)
                .HasForeignKey(d => d.MaCongThuc)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ChiTietDonHang_CongThuc");

            entity.HasOne(d => d.MaPhienBanNavigation).WithMany(p => p.ChiTietDonHangs)
                .HasForeignKey(d => d.MaPhienBan)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ChiTietDonHang_PhienBan");
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
            entity.HasKey(e => e.MaCt).HasName("PK__ChiTietM__27258E74AAFA7D3A");

            entity.ToTable("ChiTietMonAn");

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
                .HasConstraintName("FK_ChiTietMonAn_MonAn");
        });

        modelBuilder.Entity<ChiTietNhapHang>(entity =>
        {
            entity.HasKey(e => e.MaChiTietNhapHang).HasName("PK__ChiTietN__9CC62AA82B02E238");

            entity.ToTable("ChiTietNhapHang", tb => tb.HasTrigger("trg_NhapHang_UniqueNCC"));

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
                .HasConstraintName("FK_ChiTietNhapHang_CungUng");

            entity.HasOne(d => d.MaNhapHangNavigation).WithMany(p => p.ChiTietNhapHangs)
                .HasForeignKey(d => d.MaNhapHang)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ChiTietNhapHang_NhapHang");
        });

        modelBuilder.Entity<CongThucNauAn>(entity =>
        {
            entity.HasKey(e => e.MaCongThuc).HasName("PK__CongThuc__6E223AF746F16236");

            entity.ToTable("CongThucNauAn");

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
                .HasConstraintName("FK_CongThucNauAn_ChiTietMonAn");

            entity.HasOne(d => d.MaPhienBanNavigation).WithMany(p => p.CongThucNauAns)
                .HasForeignKey(d => d.MaPhienBan)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CongThucNauAn_PhienBan");
        });

        modelBuilder.Entity<CungUng>(entity =>
        {
            entity.HasKey(e => e.MaCungUng).HasName("PK__CungUng__CB2EB549E3A016FE");

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
                .HasConstraintName("FK_CungUng_NguyenLieu");

            entity.HasOne(d => d.MaNhaCungCapNavigation).WithMany(p => p.CungUngs)
                .HasForeignKey(d => d.MaNhaCungCap)
                .HasConstraintName("FK_CungUng_NhaCungCap");
        });

        modelBuilder.Entity<DanhMucMonAn>(entity =>
        {
            entity.HasKey(e => e.MaDanhMuc).HasName("PK__DanhMucM__B37508877313F015");

            entity.ToTable("DanhMucMonAn");

            entity.Property(e => e.MaDanhMuc)
                .HasMaxLength(25)
                .IsUnicode(false);
            entity.Property(e => e.TenDanhMuc).HasMaxLength(255);
        });

        modelBuilder.Entity<DonHang>(entity =>
        {
            entity.HasKey(e => e.MaDonHang).HasName("PK__DonHang__129584AD772A7426");

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
                .HasConstraintName("FK_DonHang_KhachHang");

            entity.HasOne(d => d.MaNhanVienNavigation).WithMany(p => p.DonHangs)
                .HasForeignKey(d => d.MaNhanVien)
                .HasConstraintName("FK_DonHang_NhanVien");

            entity.HasOne(d => d.MaTrangThaiDonHangNavigation).WithMany(p => p.DonHangs)
                .HasForeignKey(d => d.MaTrangThaiDonHang)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DonHang_TrangThai");
        });

        modelBuilder.Entity<HinhAnhMonAn>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__HinhAnhM__3214EC07622D3751");

            entity.ToTable("HinhAnhMonAn");

            entity.Property(e => e.MaMonAn)
                .HasMaxLength(25)
                .IsUnicode(false);
            entity.Property(e => e.URLHinhAnh).HasColumnName("URLHinhAnh");

            entity.HasOne(d => d.MaMonAnNavigation).WithMany(p => p.HinhAnhMonAns)
                .HasForeignKey(d => d.MaMonAn)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_HinhAnhMonAn_MonAn");
        });

        modelBuilder.Entity<KhachHang>(entity =>
        {
            entity.HasKey(e => e.MaKhachHang).HasName("PK__KhachHan__88D2F0E586DCD8B8");

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
            entity.HasKey(e => e.MaMonAn).HasName("PK__MonAn__B1171625D7092DC4");

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
                .HasConstraintName("FK_MonAn_DanhMuc");
        });

        modelBuilder.Entity<NguyenLieu>(entity =>
        {
            entity.HasKey(e => e.MaNguyenLieu).HasName("PK__NguyenLi__C75193556279AA69");

            entity.ToTable("NguyenLieu", tb => tb.HasTrigger("trg_NguyenLieu_GiaBanLonHonGiaNhap"));

            entity.Property(e => e.MaNguyenLieu)
                .HasMaxLength(25)
                .IsUnicode(false);
            entity.Property(e => e.DonViTinh).HasMaxLength(50);
            entity.Property(e => e.GiaBan).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.TenNguyenLieu).HasMaxLength(100);
        });

        modelBuilder.Entity<NhaCungCap>(entity =>
        {
            entity.HasKey(e => e.MaNhaCungCap).HasName("PK__NhaCungC__53DA92050F57B474");

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
            entity.HasKey(e => e.MaNhanVien).HasName("PK__NhanVien__77B2CA473CD2E431");

            entity.ToTable("NhanVien");

            entity.HasIndex(e => e.Email, "IX_NhanVien_Email_Unique")
                .IsUnique()
                .HasFilter("([Email] IS NOT NULL)");

            entity.HasIndex(e => e.TenDangNhap, "UQ_NhanVien_TenDangNhap").IsUnique();

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
                .HasConstraintName("FK_NhanVien_VaiTro");
        });

        modelBuilder.Entity<NhapHang>(entity =>
        {
            entity.HasKey(e => e.MaNhapHang).HasName("PK__NhapHang__42ECBDEAD5FBB824");

            entity.ToTable("NhapHang", tb => tb.HasTrigger("trg_NhapHang_CapNhatTonKho"));

            entity.Property(e => e.MaNhapHang)
                .HasMaxLength(25)
                .IsUnicode(false);
            entity.Property(e => e.MaNhanVien)
                .HasMaxLength(25)
                .IsUnicode(false);
            entity.Property(e => e.MaTrangThai)
                .HasMaxLength(25)
                .IsUnicode(false)
                .HasDefaultValue("MOI_TAO");
            entity.HasOne(d => d.MaNhaCungCapNavigation)
                .WithMany(p => p.NhapHangs)
                .HasForeignKey(d => d.MaNhaCungCap)
                .HasConstraintName("FK_NhapHang_NhaCungCap");
            entity.Property(e => e.NgayLapPhieu).HasColumnType("datetime");
            entity.Property(e => e.NgayNhapHang).HasColumnType("datetime");
            entity.Property(e => e.TongTien).HasColumnType("decimal(10, 2)");

            entity.HasOne(d => d.MaNhanVienNavigation).WithMany(p => p.NhapHangs)
                .HasForeignKey(d => d.MaNhanVien)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_NhapHang_NhanVien");

            entity.HasOne(d => d.MaTrangThaiNavigation).WithMany(p => p.NhapHangs)
                .HasForeignKey(d => d.MaTrangThai)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_NhapHang_TrangThai");
        });

        modelBuilder.Entity<PhienBanMonAn>(entity =>
        {
            entity.HasKey(e => e.MaPhienBan).HasName("PK__PhienBan__DB433AD40016B3FE");

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
            entity.HasKey(e => e.MaTang).HasName("PK__Tang__9D73A49D6FB99DAF");

            entity.ToTable("Tang");

            entity.Property(e => e.MaTang)
                .HasMaxLength(25)
                .IsUnicode(false);
            entity.Property(e => e.TenTang).HasMaxLength(50);
        });

        modelBuilder.Entity<TrangThaiBanAn>(entity =>
        {
            entity.HasKey(e => e.MaTrangThai).HasName("PK__TrangTha__AADE4138A1DE2910");

            entity.ToTable("TrangThaiBanAn");

            entity.Property(e => e.MaTrangThai)
                .HasMaxLength(25)
                .IsUnicode(false);
            entity.Property(e => e.TenTrangThai).HasMaxLength(50);
        });

        modelBuilder.Entity<TrangThaiDonHang>(entity =>
        {
            entity.HasKey(e => e.MaTrangThai).HasName("PK__TrangTha__AADE41381782980D");

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

        modelBuilder.Entity<TrangThaiNhapHang>(entity =>
        {
            entity.HasKey(e => e.MaTrangThai).HasName("PK__TrangTha__AADE41380CB04782");

            entity.ToTable("TrangThaiNhapHang");

            entity.Property(e => e.MaTrangThai)
                .HasMaxLength(25)
                .IsUnicode(false);
            entity.Property(e => e.TenTrangThai).HasMaxLength(50);
        });

        modelBuilder.Entity<VNguyenLieuCanNhapThem>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("V_NguyenLieuCanNhapThem");

            entity.Property(e => e.DonViTinh).HasMaxLength(50);
            entity.Property(e => e.MaNguyenLieu)
                .HasMaxLength(25)
                .IsUnicode(false);
            entity.Property(e => e.TenNguyenLieu).HasMaxLength(100);
        });

        modelBuilder.Entity<VaiTro>(entity =>
        {
            entity.HasKey(e => e.MaVaiTro).HasName("PK__VaiTro__C24C41CF8AF44A3C");

            entity.ToTable("VaiTro");

            entity.HasIndex(e => e.TenVaiTro, "UQ_VaiTro_Ten").IsUnique();

            entity.Property(e => e.MaVaiTro)
                .HasMaxLength(25)
                .IsUnicode(false);
            entity.Property(e => e.TenVaiTro).HasMaxLength(50);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}