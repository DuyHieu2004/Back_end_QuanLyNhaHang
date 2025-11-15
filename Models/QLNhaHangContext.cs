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

    public virtual DbSet<TrangThaiDonHang> TrangThaiDonHangs { get; set; }

    public virtual DbSet<TrangThaiNhanVien> TrangThaiNhanViens { get; set; }

    public virtual DbSet<TrangThaiPhienBanMonAn> TrangThaiPhienBanMonAns { get; set; }

    public virtual DbSet<VaiTro> VaiTros { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=LAPTOP-1RKGC1HF\\SQLEXPRESS;Database=QL_NhaHang_DoAn_Test2;Trusted_Connection=True;TrustServerCertificate=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<BanAn>(entity =>
        {
            entity.HasKey(e => e.MaBan).HasName("PK__BanAn__3520ED6C23C60157");

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

            entity.Property(e => e.MaTang)
                .HasMaxLength(25)
                .IsUnicode(false)
                .HasColumnName("MaTang");

            // Cấu hình khóa ngoại trỏ sang bảng Tang
            entity.HasOne(d => d.MaTangNavigation)
                .WithMany(p => p.BanAns)
                .HasForeignKey(d => d.MaTang)
                .OnDelete(DeleteBehavior.ClientSetNull) // Hoặc Cascade tùy bạn
                .HasConstraintName("FK_BanAn_Tang");

            entity.HasOne(d => d.MaTrangThaiNavigation).WithMany(p => p.BanAns)
                .HasForeignKey(d => d.MaTrangThai)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_BanAn_TrangThaiBanAn");
        });

        modelBuilder.Entity<CheBienMonAn>(entity =>
        {
            entity.HasKey(e => e.MaCheBien).HasName("PK__CheBienM__B16F8A8F8FB45FA1");

            entity.ToTable("CheBienMonAn");

            entity.Property(e => e.MaCheBien)
                .HasMaxLength(25)
                .IsUnicode(false);
            entity.Property(e => e.MaPhienBan)
                .HasMaxLength(25)
                .IsUnicode(false);
            entity.Property(e => e.NgayNau).HasColumnType("datetime");

            entity.HasOne(d => d.MaPhienBanNavigation).WithMany(p => p.CheBienMonAns)
                .HasForeignKey(d => d.MaPhienBan)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__CheBienMo__MaPhi__6754599E");
        });

        modelBuilder.Entity<ChiTietDonHang>(entity =>
        {
            entity.HasKey(e => e.MaChiTietDonHang).HasName("PK__ChiTietD__4B0B45DD6A2570FA");

            entity.ToTable("ChiTietDonHang");

            entity.Property(e => e.MaDonHang)
                .HasMaxLength(25)
                .IsUnicode(false);
            entity.Property(e => e.MaPhienBan)
                .HasMaxLength(25)
                .IsUnicode(false);

            entity.HasOne(d => d.MaDonHangNavigation).WithMany(p => p.ChiTietDonHangs)
                .HasForeignKey(d => d.MaDonHang)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ChiTietDo__MaDon__68487DD7");

            entity.HasOne(d => d.MaPhienBanNavigation).WithMany(p => p.ChiTietDonHangs)
                .HasForeignKey(d => d.MaPhienBan)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ChiTietDo__MaPhi__693CA210");
        });

        modelBuilder.Entity<ChiTietNhapNguyenLieu>(entity =>
        {
            entity.HasKey(e => e.MaChiTietNhapHang).HasName("PK__ChiTietN__9CC62AA81E931BCD");

            entity.ToTable("ChiTietNhapNguyenLieu");

            entity.Property(e => e.GiaNhap).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.MaCungUng)
                .HasMaxLength(25)
                .IsUnicode(false);
            entity.Property(e => e.MaNhapHang)
                .HasMaxLength(25)
                .IsUnicode(false);

            entity.HasOne(d => d.MaCungUngNavigation).WithMany(p => p.ChiTietNhapNguyenLieus)
                .HasForeignKey(d => d.MaCungUng)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ChiTietNh__MaCun__6A30C649");

            entity.HasOne(d => d.MaNhapHangNavigation).WithMany(p => p.ChiTietNhapNguyenLieus)
                .HasForeignKey(d => d.MaNhapHang)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ChiTietNh__MaNha__6B24EA82");
        });

        modelBuilder.Entity<CongThucNauAn>(entity =>
        {
            entity.HasKey(e => e.MaCongThuc).HasName("PK__CongThuc__6E223AF75B607B6D");

            entity.ToTable("CongThucNauAn");

            entity.Property(e => e.MaCongThuc)
                .HasMaxLength(25)
                .IsUnicode(false);
            entity.Property(e => e.MaNguyenLieu)
                .HasMaxLength(25)
                .IsUnicode(false);
            entity.Property(e => e.MaPhienBan)
                .HasMaxLength(25)
                .IsUnicode(false);

            entity.HasOne(d => d.MaNguyenLieuNavigation).WithMany(p => p.CongThucNauAns)
                .HasForeignKey(d => d.MaNguyenLieu)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__CongThucN__MaNgu__6C190EBB");

            entity.HasOne(d => d.MaPhienBanNavigation).WithMany(p => p.CongThucNauAns)
                .HasForeignKey(d => d.MaPhienBan)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__CongThucN__MaPhi__6D0D32F4");
        });

        modelBuilder.Entity<CungUng>(entity =>
        {
            entity.HasKey(e => e.MaCungUng).HasName("PK__CungUng__CB2EB5493ADD1FFE");

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
                .HasConstraintName("FK__CungUng__MaNguye__6E01572D");

            entity.HasOne(d => d.MaNhaCungCapNavigation).WithMany(p => p.CungUngs)
                .HasForeignKey(d => d.MaNhaCungCap)
                .HasConstraintName("FK__CungUng__MaNhaCu__6EF57B66");
        });

        modelBuilder.Entity<DanhMucMonAn>(entity =>
        {
            entity.HasKey(e => e.MaDanhMuc).HasName("PK__DanhMucM__B37508879BEAE0F6");

            entity.ToTable("DanhMucMonAn");

            entity.Property(e => e.MaDanhMuc)
                .HasMaxLength(25)
                .IsUnicode(false);
            entity.Property(e => e.TenDanhMuc).HasMaxLength(255);
        });

        modelBuilder.Entity<DonHang>(entity =>
        {
            entity.HasKey(e => e.MaDonHang).HasName("PK__DonHang__129584AD43FFC4F4");

            entity.ToTable("DonHang");

            entity.Property(e => e.MaDonHang)
                .HasMaxLength(25)
                .IsUnicode(false);
            entity.Property(e => e.EmailNguoiDat).HasMaxLength(100);
            entity.Property(e => e.GhiChu).HasMaxLength(500);
            entity.Property(e => e.MaBan)
                .HasMaxLength(25)
                .IsUnicode(false);
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
            entity.Property(e => e.SDTNguoiDat)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("SDTNguoiDat");
            entity.Property(e => e.SoLuongNguoi).HasDefaultValue(1);
            entity.Property(e => e.TenNguoiDat).HasMaxLength(100);
            entity.Property(e => e.ThoiGianBatDau).HasColumnType("datetime");
            entity.Property(e => e.ThoiGianDatHang).HasColumnType("datetime");
            entity.Property(e => e.ThoiGianKetThuc).HasColumnType("datetime");
            entity.Property(e => e.TienDatCoc)
                .HasDefaultValue(0m)
                .HasColumnType("decimal(10, 2)");

            entity.HasOne(d => d.MaBanNavigation).WithMany(p => p.DonHangs)
                .HasForeignKey(d => d.MaBan)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__DonHang__MaBan__6FE99F9F");

            entity.HasOne(d => d.MaKhachHangNavigation).WithMany(p => p.DonHangs)
                .HasForeignKey(d => d.MaKhachHang)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__DonHang__MaKhach__70DDC3D8");

            entity.HasOne(d => d.MaNhanVienNavigation).WithMany(p => p.DonHangs)
                .HasForeignKey(d => d.MaNhanVien)
                .HasConstraintName("FK__DonHang__MaNhanV__71D1E811");

            entity.HasOne(d => d.MaTrangThaiDonHangNavigation).WithMany(p => p.DonHangs)
                .HasForeignKey(d => d.MaTrangThaiDonHang)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DonHang_TrangThaiDonHang");
        });

        modelBuilder.Entity<HinhAnhMonAn>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__HinhAnhM__3214EC07E086E9BB");

            entity.ToTable("HinhAnhMonAn");

            entity.Property(e => e.MaMonAn)
                .HasMaxLength(25)
                .IsUnicode(false);
            entity.Property(e => e.URLHinhAnh).HasColumnName("URLHinhAnh");

            entity.HasOne(d => d.MaMonAnNavigation).WithMany(p => p.HinhAnhMonAns)
                .HasForeignKey(d => d.MaMonAn)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__HinhAnhMo__MaMon__73BA3083");
        });

        modelBuilder.Entity<KhachHang>(entity =>
        {
            entity.HasKey(e => e.MaKhachHang).HasName("PK__KhachHan__88D2F0E510C00775");

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

        modelBuilder.Entity<MonAn>(entity =>
        {
            entity.HasKey(e => e.MaMonAn).HasName("PK__MonAn__B1171625C5260BAF");

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
                .HasConstraintName("FK__MonAn__MaDanhMuc__74AE54BC");
        });

        modelBuilder.Entity<NguyenLieu>(entity =>
        {
            entity.HasKey(e => e.MaNguyenLieu).HasName("PK__NguyenLi__C751935593B4CCFD");

            entity.ToTable("NguyenLieu");

            entity.Property(e => e.MaNguyenLieu)
                .HasMaxLength(25)
                .IsUnicode(false);
            entity.Property(e => e.DonViTinh).HasMaxLength(50);
            entity.Property(e => e.TenNguyenLieu).HasMaxLength(100);
        });

        modelBuilder.Entity<NhaCungCap>(entity =>
        {
            entity.HasKey(e => e.MaNhaCungCap).HasName("PK__NhaCungC__53DA92058328A115");

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
            entity.HasKey(e => e.MaNhanVien).HasName("PK__NhanVien__77B2CA470FE77F78");

            entity.ToTable("NhanVien");

            entity.HasIndex(e => e.Email, "IX_NhanVien_Email_Unique")
                .IsUnique()
                .HasFilter("([Email] IS NOT NULL)");

            entity.HasIndex(e => e.TenDangNhap, "UQ__NhanVien__55F68FC05A7A7098").IsUnique();

            entity.Property(e => e.MaNhanVien)
                .HasMaxLength(25)
                .IsUnicode(false);
            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.HoTen).HasMaxLength(100);
            entity.Property(e => e.MaTrangThai)
                .HasMaxLength(25)
                .IsUnicode(false);
            entity.Property(e => e.MaVaiTro)
                .HasMaxLength(25)
                .IsUnicode(false);
            entity.Property(e => e.MatKhau).HasMaxLength(256);
            entity.Property(e => e.SoDienThoai).HasMaxLength(15);
            entity.Property(e => e.TenDangNhap).HasMaxLength(50);

            entity.HasOne(d => d.MaTrangThaiNavigation).WithMany(p => p.NhanViens)
                .HasForeignKey(d => d.MaTrangThai)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__NhanVien__MaTran__75A278F5");

            entity.HasOne(d => d.MaVaiTroNavigation).WithMany(p => p.NhanViens)
                .HasForeignKey(d => d.MaVaiTro)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__NhanVien__MaVaiT__76969D2E");
        });

        modelBuilder.Entity<NhapNguyenLieu>(entity =>
        {
            entity.HasKey(e => e.MaNhapHang).HasName("PK__NhapNguy__42ECBDEAC9C5DE97");

            entity.ToTable("NhapNguyenLieu");

            entity.Property(e => e.MaNhapHang)
                .HasMaxLength(25)
                .IsUnicode(false);
            entity.Property(e => e.MaNhanVien)
                .HasMaxLength(25)
                .IsUnicode(false);
            entity.Property(e => e.NgayNhapHang).HasColumnType("datetime");
            entity.Property(e => e.TongTien).HasColumnType("decimal(10, 2)");

            entity.HasOne(d => d.MaNhanVienNavigation).WithMany(p => p.NhapNguyenLieus)
                .HasForeignKey(d => d.MaNhanVien)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__NhapNguye__MaNha__778AC167");
        });

        modelBuilder.Entity<PhienBanMonAn>(entity =>
        {
            entity.HasKey(e => e.MaPhienBan).HasName("PK__PhienBan__DB433AD45111DC01");

            entity.ToTable("PhienBanMonAn");

            entity.HasIndex(e => e.MaMonAn, "IX_PhienBanMonAn_MaMonAn");

            entity.Property(e => e.MaPhienBan)
                .HasMaxLength(25)
                .IsUnicode(false);
            entity.Property(e => e.Gia).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.IsShow).HasDefaultValue(true);
            entity.Property(e => e.MaMonAn)
                .HasMaxLength(25)
                .IsUnicode(false);
            entity.Property(e => e.MaTrangThai)
                .HasMaxLength(25)
                .IsUnicode(false)
                .HasDefaultValue("CON_HANG");
            entity.Property(e => e.TenPhienBan).HasMaxLength(100);

            entity.HasOne(d => d.MaMonAnNavigation).WithMany(p => p.PhienBanMonAns)
                .HasForeignKey(d => d.MaMonAn)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__PhienBanM__MaMon__787EE5A0");

            entity.HasOne(d => d.MaTrangThaiNavigation).WithMany(p => p.PhienBanMonAns)
                .HasForeignKey(d => d.MaTrangThai)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PhienBanMonAn_TrangThaiPhienBanMonAn");
        });

        modelBuilder.Entity<TrangThaiBanAn>(entity =>
        {
            entity.HasKey(e => e.MaTrangThai).HasName("PK__TrangTha__AADE41387574A9F4");

            entity.ToTable("TrangThaiBanAn");

            entity.Property(e => e.MaTrangThai)
                .HasMaxLength(25)
                .IsUnicode(false);
            entity.Property(e => e.TenTrangThai).HasMaxLength(50);
        });

        modelBuilder.Entity<TrangThaiDonHang>(entity =>
        {
            entity.HasKey(e => e.MaTrangThai).HasName("PK__TrangTha__AADE4138DFCAAC5F");

            entity.ToTable("TrangThaiDonHang");

            entity.Property(e => e.MaTrangThai)
                .HasMaxLength(25)
                .IsUnicode(false);
            entity.Property(e => e.TenTrangThai).HasMaxLength(100);
        });

        modelBuilder.Entity<TrangThaiNhanVien>(entity =>
        {
            entity.HasKey(e => e.MaTrangThai).HasName("PK__TrangTha__AADE4138E0517566");

            entity.ToTable("TrangThaiNhanVien");

            entity.Property(e => e.MaTrangThai)
                .HasMaxLength(25)
                .IsUnicode(false);
            entity.Property(e => e.TenTrangThai).HasMaxLength(50);
        });

        modelBuilder.Entity<TrangThaiPhienBanMonAn>(entity =>
        {
            entity.HasKey(e => e.MaTrangThai).HasName("PK__TrangTha__AADE4138B8F5915B");

            entity.ToTable("TrangThaiPhienBanMonAn");

            entity.Property(e => e.MaTrangThai)
                .HasMaxLength(25)
                .IsUnicode(false);
            entity.Property(e => e.TenTrangThai).HasMaxLength(50);
        });

        modelBuilder.Entity<VaiTro>(entity =>
        {
            entity.HasKey(e => e.MaVaiTro).HasName("PK__VaiTro__C24C41CF3DF75FEF");

            entity.ToTable("VaiTro");

            entity.HasIndex(e => e.TenVaiTro, "UQ__VaiTro__1DA558141CFF9D96").IsUnique();

            entity.Property(e => e.MaVaiTro)
                .HasMaxLength(25)
                .IsUnicode(false);
            entity.Property(e => e.TenVaiTro).HasMaxLength(50);
        });

        modelBuilder.Entity<Tang>(entity =>
        {
            entity.HasKey(e => e.MaTang);
            entity.ToTable("Tang");
            entity.Property(e => e.MaTang).HasMaxLength(25).IsUnicode(false).HasColumnName("MaTang");
            entity.Property(e => e.TenTang).HasMaxLength(50).HasColumnName("TenTang");
        });


        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
