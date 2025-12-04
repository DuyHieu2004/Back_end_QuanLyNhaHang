USE [master];
GO

IF DB_ID('QL_NhaHang_DoAn_Test2') IS NOT NULL
BEGIN
    ALTER DATABASE [QL_NhaHang_DoAn_Test2] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE [QL_NhaHang_DoAn_Test2];
END
GO

CREATE DATABASE [QL_NhaHang_DoAn_Test2]
GO
USE [QL_NhaHang_DoAn_Test2]
GO

-- =============================================
-- 1. CÁC BẢNG DANH MỤC (VARCHAR 25)
-- =============================================
CREATE TABLE [dbo].[TrangThaiBanAn]( [MaTrangThai] [varchar](25) NOT NULL PRIMARY KEY, [TenTrangThai] [nvarchar](50) NOT NULL ) 
GO
CREATE TABLE [dbo].[TrangThaiDonHang]( [MaTrangThai] [varchar](25) NOT NULL PRIMARY KEY, [TenTrangThai] [nvarchar](100) NOT NULL ) 
GO
CREATE TABLE [dbo].[DanhMucMonAn]( [MaDanhMuc] [varchar](25) NOT NULL PRIMARY KEY, [TenDanhMuc] [nvarchar](255) NOT NULL ) 
GO
CREATE TABLE [dbo].[NguyenLieu]( [MaNguyenLieu] [varchar](25) NOT NULL PRIMARY KEY, [TenNguyenLieu] [nvarchar](100) NOT NULL, [DonViTinh] [nvarchar](50) NULL, [SoLuongTonKho] [int] NOT NULL DEFAULT 0, [GiaBan] [decimal](10, 2) NOT NULL DEFAULT 0 CHECK ([GiaBan] >= 0), [TrangThaiTonKho] [varchar](25) NOT NULL DEFAULT 'BINH_THUONG' ) 
GO
CREATE TABLE [dbo].[NhaCungCap]( [MaNhaCungCap] [varchar](25) NOT NULL PRIMARY KEY, [TenNhaCungCap] [nvarchar](255) NOT NULL, [SoDienThoai] [nvarchar](15) NOT NULL, [DiaChi] [nvarchar](255) NULL ) 
GO
CREATE TABLE [dbo].[VaiTro]( [MaVaiTro] [varchar](25) NOT NULL PRIMARY KEY, [TenVaiTro] [nvarchar](50) NOT NULL ) 
GO
CREATE UNIQUE NONCLUSTERED INDEX [UQ_VaiTro_Ten] ON [dbo].[VaiTro]([TenVaiTro]); 
GO
CREATE TABLE [dbo].[PhienBanMonAn]( [MaPhienBan] [varchar](25) NOT NULL PRIMARY KEY, [TenPhienBan] [nvarchar](100) NOT NULL, [MaTrangThai] [varchar](25) NOT NULL DEFAULT 'CON_HANG', [ThuTu] [int] NULL ) 
GO
CREATE TABLE [dbo].[Tang]( [MaTang] [varchar](25) NOT NULL PRIMARY KEY, [TenTang] [nvarchar](50) NOT NULL )
GO
CREATE TABLE [dbo].[LoaiMenu]( [MaLoaiMenu] [varchar](25) NOT NULL, [TenLoaiMenu] [nvarchar](100) NOT NULL, [MoTa] [nvarchar](500) NULL, CONSTRAINT [PK_LoaiMenu] PRIMARY KEY CLUSTERED ([MaLoaiMenu] ASC) ); 
GO
CREATE TABLE [dbo].[TrangThaiMenu]( [MaTrangThai] [varchar](25) NOT NULL, [TenTrangThai] [nvarchar](50) NOT NULL, CONSTRAINT [PK_TrangThaiMenu] PRIMARY KEY CLUSTERED ([MaTrangThai] ASC) ); 
GO
CREATE TABLE [dbo].[TrangThaiNhapHang]( [MaTrangThai] [varchar](25) NOT NULL PRIMARY KEY, [TenTrangThai] [nvarchar](50) NOT NULL ) 
GO

-- =============================================
-- 2. CÁC BẢNG CHÍNH (VARCHAR 25)
-- =============================================

CREATE TABLE [dbo].[BanAn](
    [MaBan] [varchar](25) NOT NULL PRIMARY KEY,
    [TenBan] [nvarchar](50) NOT NULL,
    [MaTrangThai] [varchar](25) NOT NULL,
    [MaTang] [varchar](25) NULL,
    [SucChua] [int] NOT NULL DEFAULT 4,
    [IsShow] [bit] DEFAULT 1
)
GO

CREATE TABLE [dbo].[KhachHang](
    [MaKhachHang] [varchar](25) NOT NULL PRIMARY KEY,
    [HoTen] [nvarchar](100) NOT NULL,
    [SoDienThoai] [nvarchar](15) NOT NULL,
    [Email] [nvarchar](100) NULL,
    [HinhAnh] [nvarchar](max) NULL,
    [NoShowCount] [int] DEFAULT 0,
    [NgayCuoiCungTichLuy] [DATETIME] NULL, -- Giữ nguyên cột này theo Code 1
    [NgayTao] [DATETIME] DEFAULT GETDATE()
)
GO
CREATE INDEX IX_KhachHang_SoDienThoai ON KhachHang(SoDienThoai);
CREATE UNIQUE NONCLUSTERED INDEX [IX_KhachHang_Email_Unique] ON [dbo].[KhachHang]([Email]) WHERE [Email] IS NOT NULL;
GO

CREATE TABLE [dbo].[NhanVien](
    [MaNhanVien] [varchar](25) NOT NULL PRIMARY KEY,
    [HoTen] [nvarchar](100) NOT NULL,
    [TenDangNhap] [nvarchar](50) NOT NULL,
    [MatKhau] [nvarchar](256) NOT NULL,
    [MaVaiTro] [varchar](25) NOT NULL,
    [Email] [nvarchar](100) NULL,
    [SoDienThoai] [nvarchar](15) NULL,
    [HinhAnh] [nvarchar](max) NULL
)
GO
CREATE UNIQUE NONCLUSTERED INDEX [UQ_NhanVien_TenDangNhap] ON [dbo].[NhanVien]([TenDangNhap]);
CREATE UNIQUE NONCLUSTERED INDEX [IX_NhanVien_Email_Unique] ON [dbo].[NhanVien]([Email]) WHERE [Email] IS NOT NULL;
GO

CREATE TABLE [dbo].[MonAn](
    [MaMonAn] [varchar](25) NOT NULL PRIMARY KEY,
    [TenMonAn] [nvarchar](100) NOT NULL,
    [MaDanhMuc] [varchar](25) NULL,
    [IsShow] [bit] DEFAULT 1
)
GO

CREATE TABLE [dbo].[KhuyenMai](
    [MaKhuyenMai] [varchar](25) NOT NULL PRIMARY KEY,
    [TenKhuyenMai] [nvarchar](255) NOT NULL,
    [MoTa] [nvarchar](max) NULL,
    [LoaiKhuyenMai] [varchar](25) NOT NULL, 
    [GiaTri] [decimal](10, 2) NOT NULL, 
    [ApDungToiThieu] [decimal](10, 2) NULL, 
    [NgayBatDau] [datetime] NOT NULL,
    [NgayKetThuc] [datetime] NOT NULL,
    [TrangThai] [varchar](25) NOT NULL DEFAULT 'DANG_HOAT_DONG'
)
GO

CREATE TABLE [dbo].[DonHang](
    [MaDonHang] [varchar](25) NOT NULL PRIMARY KEY,
    [MaNhanVien] [varchar](25) NULL,
    [MaKhachHang] [varchar](25) NOT NULL DEFAULT 'KH_VANG_LAI',
    [MaTrangThaiDonHang] [varchar](25) NOT NULL DEFAULT 'CHO_XAC_NHAN',
    [ThoiGianDatHang] [datetime] NULL,
    [TGDatDuKien] [datetime] NULL,
    [TGNhanBan] [datetime] NULL,
    [ThanhToan] [bit] NOT NULL DEFAULT 0,
    [ThoiGianKetThuc] [datetime] NULL,
    [SoLuongNguoiDK] [int] NOT NULL DEFAULT 1,
    [TienDatCoc] [decimal](10, 2) NULL DEFAULT 0,
    [GhiChu] [nvarchar](500) NULL,
    [TenNguoiNhan] [nvarchar](100) NULL,
    [SDTNguoiNhan] [varchar](20) NULL,
    [EmailNguoiNhan] [nvarchar](100) NULL,
    [MaKhuyenMai] [varchar](25) NULL,
    [TienGiamGia] [decimal](10, 2) NULL DEFAULT 0
)
GO

-- =============================================
-- 3. TẠO CÁC BẢNG TRUNG GIAN & CHI TIẾT
-- =============================================

CREATE TABLE [dbo].[ChiTietMonAn](
    [MaCT] [varchar](25) NOT NULL PRIMARY KEY,
    [TenCT] [nvarchar](100) NOT NULL,
    [MaMonAn] [varchar](25) NOT NULL
)
GO

CREATE TABLE [dbo].[CongThucNauAn](
    [MaCongThuc] [varchar](25) NOT NULL PRIMARY KEY,
    [MaCT] [varchar](25) NOT NULL,
    [MaPhienBan] [varchar](25) NOT NULL,
    [Gia] [decimal](10, 2) NOT NULL CHECK ([Gia] >= 0)
)
GO

-- [BẢNG CHITIETDONHANG] - SỬA LẠI THÀNH BIGINT IDENTITY(1,1) CHO ĐÚNG LOGIC
-- 2. TẠO BẢNG ChiTietDonHang (VARCHAR 25)
CREATE TABLE [dbo].[ChiTietDonHang](
    [MaChiTietDonHang] [varchar](25) NOT NULL PRIMARY KEY, -- ĐÃ SỬA THÀNH VARCHAR
    [MaDonHang] [varchar](25) NOT NULL, 
    [MaPhienBan] [varchar](25) NOT NULL,
    [MaCongThuc] [varchar](25) NOT NULL,
    [SoLuong] [int] NOT NULL
)
GO

-- 3. TẠO BẢNG BanAnDonHang (VARCHAR 25)
CREATE TABLE [dbo].[BanAnDonHang] (
    [MaBanAnDonHang] VARCHAR(25) NOT NULL PRIMARY KEY, 
    [MaDonHang] VARCHAR(25) NOT NULL,
    [MaBan] VARCHAR(25) NOT NULL,
    [MaChiTietDonHang] VARCHAR(25) NULL -- ĐÃ SỬA THÀNH VARCHAR ĐỂ KHỚP
)
GO

CREATE TABLE [dbo].[ChiTietCongThuc](
    [MaChiTietCongThuc] [bigint] IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [MaCongThuc] [varchar](25) NOT NULL,
    [MaNguyenLieu] [varchar](25) NOT NULL,
    [SoLuongCanDung] [int] NOT NULL
)
GO

CREATE TABLE [dbo].[CungUng]( [MaCungUng] [varchar](25) NOT NULL PRIMARY KEY, [MaNguyenLieu] [varchar](25) NULL, [MaNhaCungCap] [varchar](25) NULL ) 
GO

CREATE TABLE [dbo].[NhapHang]( [MaNhapHang] [varchar](25) NOT NULL PRIMARY KEY, [MaNhanVien] [varchar](25) NOT NULL, [NgayNhapHang] [datetime] NOT NULL, [TongTien] [decimal](10, 2) NOT NULL, [NgayLapPhieu] [datetime] NOT NULL, [MaTrangThai] [varchar](25) NOT NULL DEFAULT 'MOI_TAO', [MaNhaCungCap] [varchar](25) NULL ) 
GO

CREATE TABLE [dbo].[ChiTietNhapHang](
    [MaChiTietNhapHang] [bigint] IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [MaNhapHang] [varchar](25) NOT NULL,
    [MaNguyenLieu] [varchar](25) NOT NULL, 
    [SoLuong] [int] NOT NULL,
    [GiaNhap] [decimal](10, 2) NOT NULL
)
GO

CREATE TABLE [dbo].[HinhAnhMonAn](
    [Id] [int] IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [MaMonAn] [varchar](25) NOT NULL,
    [URLHinhAnh] [nvarchar](max) NOT NULL
)
GO

CREATE TABLE [dbo].[KhuyenMaiApDungSanPham](
    [Id] [bigint] IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [MaKhuyenMai] [varchar](25) NOT NULL,
    [MaCongThuc] [varchar](25) NULL, 
    [MaDanhMuc] [varchar](25) NULL
)
GO

CREATE TABLE [dbo].[Menu](
    [MaMenu] [varchar](25) NOT NULL,
    [TenMenu] [nvarchar](200) NOT NULL,
    [MaLoaiMenu] [varchar](25) NOT NULL,
    [MaTrangThai] [varchar](25) NOT NULL,
    [GiaMenu] [decimal](10, 2) NOT NULL CHECK ([GiaMenu] >= 0),
    [GiaGoc] [decimal](10, 2) NULL CHECK ([GiaGoc] IS NULL OR [GiaGoc] >= 0),
    [MoTa] [nvarchar](1000) NULL,
    [HinhAnh] [nvarchar](max) NULL,
    [NgayBatDau] [datetime] NULL,
    [NgayKetThuc] [datetime] NULL,
    [IsShow] [bit] NOT NULL DEFAULT(1),
    [ThuTu] [int] NULL, 
    [NgayTao] [datetime] NOT NULL DEFAULT(GETDATE()),
    [NgayCapNhat] [datetime] NULL,
    CONSTRAINT [PK_Menu] PRIMARY KEY CLUSTERED ([MaMenu] ASC)
);
GO

CREATE TABLE [dbo].[ChiTietMenu](
    [MaChiTietMenu] [bigint] IDENTITY(1,1) NOT NULL,
    [MaMenu] [varchar](25) NOT NULL,
    [MaCongThuc] [varchar](25) NOT NULL,
    [SoLuong] [int] NOT NULL DEFAULT(1) CHECK ([SoLuong] > 0),
    [GhiChu] [nvarchar](500) NULL,
    [ThuTu] [int] NULL,
    CONSTRAINT [PK_ChiTietMenu] PRIMARY KEY CLUSTERED ([MaChiTietMenu] ASC)
);
GO

-- =====================================================
-- 4. THÊM KHÓA NGOẠI (FOREIGN KEYS) - CHUẨN XÁC
-- =====================================================

-- 1. BanAnDonHang -> DonHang (Nhiều bàn thuộc 1 đơn)
ALTER TABLE [dbo].[BanAnDonHang] WITH CHECK ADD CONSTRAINT [FK_BanAnDonHang_DonHang] FOREIGN KEY([MaDonHang]) REFERENCES [dbo].[DonHang] ([MaDonHang]) ON DELETE CASCADE
ALTER TABLE [dbo].[BanAnDonHang] WITH CHECK ADD CONSTRAINT [FK_BanAnDonHang_BanAn] FOREIGN KEY([MaBan]) REFERENCES [dbo].[BanAn] ([MaBan])
GO

-- 2. BanAnDonHang -> ChiTietDonHang (Nhiều bàn thuộc 1 chi tiết món)
ALTER TABLE [dbo].[BanAnDonHang] WITH CHECK ADD CONSTRAINT [FK_BanAnDonHang_ChiTietDonHang] FOREIGN KEY([MaChiTietDonHang]) REFERENCES [dbo].[ChiTietDonHang] ([MaChiTietDonHang])
GO

-- 3. ChiTietDonHang -> DonHang (Nhiều chi tiết thuộc 1 đơn)
ALTER TABLE [dbo].[ChiTietDonHang] WITH CHECK ADD CONSTRAINT [FK_ChiTietDonHang_DonHang] FOREIGN KEY([MaDonHang]) REFERENCES [dbo].[DonHang] ([MaDonHang]) -- Không Cascade ở đây để tránh cycle, hoặc có thể thêm nếu muốn
ALTER TABLE [dbo].[ChiTietDonHang] WITH CHECK ADD CONSTRAINT [FK_ChiTietDonHang_PhienBan] FOREIGN KEY([MaPhienBan]) REFERENCES [dbo].[PhienBanMonAn] ([MaPhienBan])
ALTER TABLE [dbo].[ChiTietDonHang] WITH CHECK ADD CONSTRAINT [FK_ChiTietDonHang_CongThuc] FOREIGN KEY([MaCongThuc]) REFERENCES [dbo].[CongThucNauAn] ([MaCongThuc])
GO

-- ... (Các khóa ngoại khác giữ nguyên)
ALTER TABLE [dbo].[BanAn] WITH CHECK ADD CONSTRAINT [FK_BanAn_TrangThaiBanAn] FOREIGN KEY([MaTrangThai]) REFERENCES [dbo].[TrangThaiBanAn] ([MaTrangThai])
ALTER TABLE [dbo].[BanAn] WITH CHECK ADD CONSTRAINT [FK_BanAn_Tang] FOREIGN KEY([MaTang]) REFERENCES [dbo].[Tang] ([MaTang])
GO
ALTER TABLE [dbo].[DonHang] WITH CHECK ADD CONSTRAINT [FK_DonHang_KhachHang] FOREIGN KEY([MaKhachHang]) REFERENCES [dbo].[KhachHang] ([MaKhachHang])
ALTER TABLE [dbo].[DonHang] WITH CHECK ADD CONSTRAINT [FK_DonHang_NhanVien] FOREIGN KEY([MaNhanVien]) REFERENCES [dbo].[NhanVien] ([MaNhanVien])
ALTER TABLE [dbo].[DonHang] WITH CHECK ADD CONSTRAINT [FK_DonHang_TrangThai] FOREIGN KEY([MaTrangThaiDonHang]) REFERENCES [dbo].[TrangThaiDonHang] ([MaTrangThai])
ALTER TABLE [dbo].[DonHang] WITH CHECK ADD CONSTRAINT [FK_DonHang_KhuyenMai] FOREIGN KEY([MaKhuyenMai]) REFERENCES [dbo].[KhuyenMai] ([MaKhuyenMai])
GO
ALTER TABLE [dbo].[ChiTietMonAn] WITH CHECK ADD CONSTRAINT [FK_ChiTietMonAn_MonAn] FOREIGN KEY([MaMonAn]) REFERENCES [dbo].[MonAn] ([MaMonAn])
ALTER TABLE [dbo].[CongThucNauAn] WITH CHECK ADD CONSTRAINT [FK_CongThucNauAn_ChiTietMonAn] FOREIGN KEY([MaCT]) REFERENCES [dbo].[ChiTietMonAn] ([MaCT])
ALTER TABLE [dbo].[CongThucNauAn] WITH CHECK ADD CONSTRAINT [FK_CongThucNauAn_PhienBan] FOREIGN KEY([MaPhienBan]) REFERENCES [dbo].[PhienBanMonAn] ([MaPhienBan])
ALTER TABLE [dbo].[ChiTietCongThuc] WITH CHECK ADD CONSTRAINT [FK_ChiTietCongThuc_CongThuc] FOREIGN KEY([MaCongThuc]) REFERENCES [dbo].[CongThucNauAn] ([MaCongThuc])
ALTER TABLE [dbo].[ChiTietCongThuc] WITH CHECK ADD CONSTRAINT [FK_ChiTietCongThuc_NguyenLieu] FOREIGN KEY([MaNguyenLieu]) REFERENCES [dbo].[NguyenLieu] ([MaNguyenLieu])
ALTER TABLE [dbo].[MonAn] WITH CHECK ADD CONSTRAINT [FK_MonAn_DanhMuc] FOREIGN KEY([MaDanhMuc]) REFERENCES [dbo].[DanhMucMonAn] ([MaDanhMuc])
ALTER TABLE [dbo].[HinhAnhMonAn] WITH CHECK ADD CONSTRAINT [FK_HinhAnhMonAn_MonAn] FOREIGN KEY([MaMonAn]) REFERENCES [dbo].[MonAn] ([MaMonAn])
GO
ALTER TABLE [dbo].[CungUng] WITH CHECK ADD CONSTRAINT [FK_CungUng_NguyenLieu] FOREIGN KEY([MaNguyenLieu]) REFERENCES [dbo].[NguyenLieu] ([MaNguyenLieu])
ALTER TABLE [dbo].[CungUng] WITH CHECK ADD CONSTRAINT [FK_CungUng_NhaCungCap] FOREIGN KEY([MaNhaCungCap]) REFERENCES [dbo].[NhaCungCap] ([MaNhaCungCap])
ALTER TABLE [dbo].[NhapHang] WITH CHECK ADD CONSTRAINT [FK_NhapHang_NhanVien] FOREIGN KEY([MaNhanVien]) REFERENCES [dbo].[NhanVien] ([MaNhanVien])
ALTER TABLE [dbo].[NhapHang] WITH CHECK ADD CONSTRAINT [FK_NhapHang_TrangThai] FOREIGN KEY([MaTrangThai]) REFERENCES [dbo].[TrangThaiNhapHang] ([MaTrangThai])
ALTER TABLE [dbo].[NhapHang] WITH CHECK ADD CONSTRAINT [FK_NhapHang_NhaCungCap] FOREIGN KEY([MaNhaCungCap]) REFERENCES [dbo].[NhaCungCap] ([MaNhaCungCap]);
ALTER TABLE [dbo].[ChiTietNhapHang] WITH CHECK ADD CONSTRAINT [FK_ChiTietNhapHang_NguyenLieu] FOREIGN KEY([MaNguyenLieu]) REFERENCES [dbo].[NguyenLieu] ([MaNguyenLieu])
ALTER TABLE [dbo].[ChiTietNhapHang] WITH CHECK ADD CONSTRAINT [FK_ChiTietNhapHang_NhapHang] FOREIGN KEY([MaNhapHang]) REFERENCES [dbo].[NhapHang] ([MaNhapHang])
GO
ALTER TABLE [dbo].[NhanVien] WITH CHECK ADD CONSTRAINT [FK_NhanVien_VaiTro] FOREIGN KEY([MaVaiTro]) REFERENCES [dbo].[VaiTro] ([MaVaiTro])
GO
ALTER TABLE [dbo].[KhuyenMaiApDungSanPham] WITH CHECK ADD CONSTRAINT [FK_KMAP_KhuyenMai] FOREIGN KEY([MaKhuyenMai]) REFERENCES [dbo].[KhuyenMai] ([MaKhuyenMai])
ALTER TABLE [dbo].[KhuyenMaiApDungSanPham] WITH CHECK ADD CONSTRAINT [FK_KMAP_CongThucNauAn] FOREIGN KEY([MaCongThuc]) REFERENCES [dbo].[CongThucNauAn] ([MaCongThuc])
ALTER TABLE [dbo].[KhuyenMaiApDungSanPham] WITH CHECK ADD CONSTRAINT [FK_KMAP_DanhMucMonAn] FOREIGN KEY([MaDanhMuc]) REFERENCES [dbo].[DanhMucMonAn] ([MaDanhMuc])
ALTER TABLE [dbo].[KhuyenMaiApDungSanPham] ADD CONSTRAINT CK_KMAP_OnlyOneTarget CHECK (([MaCongThuc] IS NULL AND [MaDanhMuc] IS NOT NULL) OR ([MaCongThuc] IS NOT NULL AND [MaDanhMuc] IS NULL));
ALTER TABLE [dbo].[Menu] WITH CHECK ADD CONSTRAINT [FK_Menu_LoaiMenu] FOREIGN KEY([MaLoaiMenu]) REFERENCES [dbo].[LoaiMenu] ([MaLoaiMenu]);
ALTER TABLE [dbo].[Menu] WITH CHECK ADD CONSTRAINT [FK_Menu_TrangThaiMenu] FOREIGN KEY([MaTrangThai]) REFERENCES [dbo].[TrangThaiMenu] ([MaTrangThai]);
ALTER TABLE [dbo].[ChiTietMenu] WITH CHECK ADD CONSTRAINT [FK_ChiTietMenu_Menu] FOREIGN KEY([MaMenu]) REFERENCES [dbo].[Menu] ([MaMenu]) ON DELETE CASCADE;
ALTER TABLE [dbo].[ChiTietMenu] WITH CHECK ADD CONSTRAINT [FK_ChiTietMenu_CongThucNauAn] FOREIGN KEY([MaCongThuc]) REFERENCES [dbo].[CongThucNauAn] ([MaCongThuc]);
GO

-- ============================================================
-- CHÈN DỮ LIỆU (PHẦN CHUNG & KHÔNG TRÙNG LẶP)
-- ============================================================

EXEC sp_MSforeachtable 'ALTER TABLE ? NOCHECK CONSTRAINT ALL'
GO
INSERT INTO [dbo].[KhuyenMai] ([MaKhuyenMai], [TenKhuyenMai], [MoTa], [LoaiKhuyenMai], [GiaTri], [ApDungToiThieu], [NgayBatDau], [NgayKetThuc], [TrangThai]) 
VALUES ('KM_TICHLUY_VIP', N'Tri ân khách hàng thân thiết', N'Tự động giảm giá cho khách đã ăn đủ 10 lần', 'PERCENT', 10, 0, GETDATE(), '2099-12-31', 'ACTIVE');

INSERT INTO [dbo].[TrangThaiBanAn] ([MaTrangThai], [TenTrangThai]) VALUES
('TTBA001', N'Trống'), ('TTBA002', N'Đang phục vụ'),
('TTBA003', N'Đã đặt'), ('TTBA004', N'Bảo trì');

INSERT INTO [dbo].[TrangThaiDonHang] (MaTrangThai, TenTrangThai) VALUES
('CHO_XAC_NHAN', N'Chờ xác nhận'), ('DA_XAC_NHAN', N'Đã xác nhận'),
('DA_HUY', N'Đã hủy'), ('DA_HOAN_THANH', N'Đã hoàn thành'),
('NO_SHOW', N'Vắng mặt (No-Show)'), 
('CHO_THANH_TOAN', N'Chờ thanh toán');

INSERT INTO [dbo].[TrangThaiNhapHang] ([MaTrangThai], [TenTrangThai]) VALUES
('MOI_TAO', N'Mới tạo/Bản nháp'), 
('DA_GUI_NCC', N'Đã gửi Nhà Cung Cấp'), 
('DA_HOAN_TAT', N'Đã nhập kho/Hoàn tất');

INSERT INTO [dbo].[VaiTro] ([MaVaiTro], [TenVaiTro]) VALUES
('VT001', N'Quản lý'), 
('VT002', N'Nhân viên trực quầy');
GO

INSERT INTO [dbo].[DanhMucMonAn] ([MaDanhMuc], [TenDanhMuc]) VALUES
('DM001', N'Món khai vị'), ('DM002', N'Lẩu'), ('DM003', N'Tráng miệng'),
('DM004', N'Thức uống'), ('DM005', N'Món nướng'), ('DM006', N'Món chay'),
('DM007', N'Cơm'), ('DM008', N'Hải sản');

INSERT INTO [dbo].[Tang] ([MaTang], [TenTang]) VALUES
('T001', N'Tầng trệt'), ('T002', N'Tầng 1'), ('T003', N'Tầng 2');

INSERT INTO [dbo].[LoaiMenu] ([MaLoaiMenu], [TenLoaiMenu], [MoTa]) VALUES
('LM001', N'Menu Set', N'Menu combo gồm nhiều món với giá ưu đãi'),
('LM002', N'Menu Buffet', N'Menu buffet ăn thỏa thích'),
('LM003', N'Menu theo ngày', N'Menu đặc biệt theo từng ngày trong tuần'),
('LM004', N'Menu sự kiện', N'Menu đặc biệt cho các dịp lễ, sự kiện'),
('LM005', N'Menu gia đình', N'Menu dành cho gia đình, nhóm đông người'),
('LM006', N'Menu tiệc', N'Menu dành cho tiệc, hội nghị');

INSERT INTO [dbo].[TrangThaiMenu] ([MaTrangThai], [TenTrangThai]) VALUES
('DANG_AP_DUNG', N'Đang áp dụng'), ('HET_HAN', N'Hết hạn'),
('TAM_NGUNG', N'Tạm ngưng'), ('CHUA_AP_DUNG', N'Chưa áp dụng');

INSERT INTO [dbo].[NhaCungCap] ([MaNhaCungCap], [TenNhaCungCap], [SoDienThoai], [DiaChi]) VALUES
('NCC001', N'Công ty rau củ Đà Lạt Xanh', '090111222', N'123, Lâm Đồng'),
('NCC002', N'Vựa hải sản Vũng Tàu', '090222333', N'456, Vũng Tàu'),
('NCC003', N'Công ty thịt bò CP', '090333444', N'789, Đồng Nai'),
('NCC004', N'Đại lý bia Sài Gòn', '090444555', N'101, Q.1, TPHCM'),
('NCC005', N'Tổng kho gia vị', '090555666', '102, Q.5, TPHCM'),
('NCC006', N'Trang trại gà sạch Ba Vì', '090666777', '103, Ba Vì, Hà Nội'),
('NCC007', N'Công ty nước giải khát Pepsico', '090777888', '104, Hóc Môn, TPHCM'),
('NCC008', N'Nhà cung cấp gạo miền Tây', '090888999', '105, Cần Thơ'),
('NCC009', N'Công ty TNHH Nấm Việt', '090999000', '106, Q.12, TPHCM'),
('NCC010', N'Lò mổ heo Vissan', '091111222', '107, Long An'),
('NCC011', N'Trang trại rau hữu cơ', '0912121212', '111, Hóc Môn'),
('NCC012', N'Công ty Nước mắm Phú Quốc', '0912121213', '222, Kiên Giang'),
('NCC013', N'Nhà phân phối rượu Vang Đà Lạt', '0912121214', '333, Lâm Đồng'),
('NCC014', N'Lò bánh mì ABC', '0912121215', '444, Q.10, TPHCM'),
('NCC015', N'Công ty Cà phê Trung Nguyên', '0912121216', '555, Đắk Lắk'),
('NCC016', N'Vựa trái cây Cái Bè', '0912121217', '666, Tiền Giang'),
('NCC017', N'Công ty Sữa Vinamilk', '0912121218', '777, Q.7, TPHCM'),
('NCC018', N'Hợp tác xã nấm sạch', '0912121219', '888, Đồng Nai'),
('NCC019', N'Đại lý trứng gia cầm Ba Huân', '0912121220', '999, Bình Chánh'),
('NCC020', N'Nhà cung cấp đồ khô', '0912121221', '121, Q.5, TPHCM');

-- THAY ĐỔI LỚN: Sửa cột SoLanAnTichLuy thành NgayCuoiCungTichLuy (DATE/DATETIME)
-- Giả định các KH đã ăn được set ngày tích lũy gần nhất (2025-10-08)
INSERT INTO [dbo].[KhachHang] ([MaKhachHang], [HoTen], [SoDienThoai], [Email], [HinhAnh], [NoShowCount], [NgayCuoiCungTichLuy], [NgayTao]) VALUES
('KH_VANG_LAI', N'Khách Vãng Lai', '0000000000', NULL, NULL, 0, NULL, GETDATE()),
('KH001', N'Nguyễn Văn An', '0912345678', 'an.nguyen@gmail.com', 'an.jpg', 0, '2025-10-08 20:00:00', GETDATE()),
('KH002', N'Trần Thị Bình', '0912345679', 'binh.tran@gmail.com', 'binh.jpg', 0, '2025-10-08 21:00:00', GETDATE()),
('KH003', N'Lê Văn Cường', '0912345680', 'cuong.le@gmail.com', 'cuong.jpg', 0, '2025-10-09 12:00:00', GETDATE()),
('KH004', N'Phạm Thị Dung', '0912345681', 'dung.pham@gmail.com', 'dung.jpg', 0, '2025-10-09 13:00:00', GETDATE()),
('KH005', N'Hoàng Văn Giang', '0912345682', 'giang.hoang@gmail.com', 'giang.jpg', 0, '2025-10-10 20:30:00', GETDATE()),
('KH006', N'Vũ Thị Hương', '0912345683', 'huong.vu@gmail.com', 'huong.jpg', 0, '2025-10-11 21:30:00', GETDATE()),
('KH007', N'Đặng Văn Long', '0912345684', 'long.dang@gmail.com', 'long.jpg', 0, '2025-10-12 21:00:00', GETDATE()),
('KH008', N'Bùi Thị Mai', '0912345685', 'mai.bui@gmail.com', 'mai.jpg', 0, '2025-10-13 18:00:00', GETDATE()),
('KH009', N'Ngô Văn Nam', '0912345686', 'nam.ngo@gmail.com', 'nam.jpg', 0, '2025-10-14 21:00:00', GETDATE()),
('KH010', N'Dương Thị Oanh', '0912345687', 'oanh.duong@gmail.com', 'oanh.jpg', 0, '2025-10-15 20:00:00', GETDATE()),
('KH011', N'Trần Văn Phát', '0911111111', 'phat.tran@gmail.com', 'phat.jpg', 0, '2025-11-01 20:00:00', GETDATE()),
('KH012', N'Lê Thị Quyên', '0922222222', 'quyen.le@gmail.com', 'quyen.jpg', 0, '2025-11-01 19:30:00', GETDATE()),
('KH013', N'Đỗ Bá Rừng', '0933333333', 'rung.do@gmail.com', 'rung.jpg', 0, '2025-11-02 21:00:00', GETDATE()),
('KH014', N'Hồ Thị Sen', '0944444444', 'sen.ho@gmail.com', 'sen.jpg', 0, '2025-11-03 13:00:00', GETDATE()),
('KH015', N'Ngô Văn Tùng', '0955555555', 'tung.ngo@gmail.com', 'tung.jpg', 0, '2025-11-03 13:15:00', GETDATE()),
('KH016', N'Dương Văn Út', '0966666666', 'ut.duong@gmail.com', 'ut.jpg', 0, '2025-11-05 21:00:00', GETDATE()),
('KH017', N'Phan Thị Vân', '0977777777', 'van.phan@gmail.com', 'van.jpg', 0, '2025-11-06 20:30:00', GETDATE()),
('KH018', N'Lý Văn Xuân', '0988888888', 'xuan.ly@gmail.com', 'xuan.jpg', 0, '2025-11-07 12:00:00', GETDATE()),
('KH019', N'Võ Thị Yến', '0999999999', 'yen.vo@gmail.com', 'yen.jpg', 0, '2025-11-07 12:30:00', GETDATE()),
('KH020', N'Trịnh Hoài An', '0901234567', 'an.trinh@gmail.com', 'an_trinh.jpg', 0, '2025-11-08 20:45:00', GETDATE()),
('KH021', N'Nguyễn Hữu Ái', '0901112233', 'ai.nguyen@gmail.com', 'ai.jpg', 0, '2025-11-08 21:00:00', GETDATE()),
('KH022', N'Võ Tấn Bằng', '0901112244', 'bang.vo@gmail.com', 'bang.jpg', 0, '2025-11-09 13:00:00', GETDATE()),
('KH023', N'Huỳnh Ngọc Châu', '0901112255', 'chau.huynh@gmail.com', 'chau.jpg', 0, '2025-11-09 13:30:00', GETDATE()),
('KH024', N'Trương Minh Đức', '0901112266', 'duc.truong@gmail.com', 'duc.jpg', 0, NULL, GETDATE()),
('KH025', N'Hà Thị Giang', '0901112277', 'giang.ha@gmail.com', 'giang_ha.jpg', 0, NULL, GETDATE()),
('KH026', N'Đinh Quốc Huy', '0901112288', 'huy.dinh@gmail.com', 'huy.jpg', 0, '2025-11-10 21:00:00', GETDATE()),
('KH027', N'Lương Yến Khanh', '0901112299', 'khanh.luong@gmail.com', 'khanh.jpg', 0, '2025-11-11 20:30:00', GETDATE()),
('KH028', N'Mai Đức Lợi', '0901113300', 'loi.mai@gmail.com', 'loi.jpg', 0, NULL, GETDATE()),
('KH029', N'Đoàn Văn Mẫn', '0901113311', 'man.doan@gmail.com', 'man.jpg', 0, '2025-11-12 13:15:00', GETDATE()),
('KH030', N'Hoàng Thị Ngân', '0901113322', 'ngan.hoang@gmail.com', 'ngan.jpg', 1, NULL, GETDATE()),
('KH031', N'Phạm Gia Phú', '0901113333', 'phu.pham@gmail.com', 'phu.jpg', 0, '2025-11-14 21:00:00', GETDATE()),
('KH032', N'Tô Hoài Sang', '0901113344', 'sang.to@gmail.com', 'sang.jpg', 0, NULL, GETDATE()),
('KH033', N'Lê Minh Thông', '0901113355', 'thong.le@gmail.com', 'thong.jpg', 0, '2025-11-16 19:00:00', GETDATE()),
('KH034', N'VươngGia Uy', '0901113366', 'uy.vuong@gmail.com', 'uy.jpg', 0, '2025-11-17 13:00:00', GETDATE()),
('KH035', N'Nguyễn Thanh Vi', '0901113377', 'vi.nguyen@gmail.com', 'vi.jpg', 0, '2025-11-18 13:00:00', GETDATE()),
('KH036', N'Đặng Minh Vũ', '0901113388', 'vu.dang@gmail.com', 'vu.jpg', 0, '2025-11-19 21:00:00', GETDATE()),
('KH037', N'Tống Phước Lộc', '0901113399', 'loc.tong@gmail.com', 'loc.jpg', 0, '2025-11-20 21:30:00', GETDATE()),
('KH038', N'Triệu Thị Mỹ', '0901114400', 'my.trieu@gmail.com', 'my.jpg', 0, '2025-11-21 13:00:00', GETDATE()),
('KH039', N'Uông Văn Tài', '0901114411', 'tai.uong@gmail.com', 'tai.jpg', 0, '2025-11-22 20:00:00', GETDATE()),
('KH040', N'Cù Minh Tâm', '0901114422', 'tam.cu@gmail.com', 'tam.jpg', 0, '2025-11-23 21:00:00', GETDATE());
GO

-- Bảng BanAn (Giữ nguyên)
INSERT INTO [dbo].[BanAn] ([MaBan], [TenBan], [MaTrangThai], [SucChua]) VALUES
('B001', N'Bàn 1', 'TTBA001', 4), ('B002', N'Bàn 2', 'TTBA001', 4),
('B003', N'Bàn 3', 'TTBA001', 6), ('B004', N'Bàn 4', 'TTBA001', 6),
('B005', N'Bàn 5', 'TTBA001', 8), ('B006', N'Bàn 6', 'TTBA001', 8),
('B007', N'Bàn 7', 'TTBA001', 10), ('B008', N'Bàn 8', 'TTBA001', 12),
('B009', N'Bàn 9', 'TTBA001', 4), ('B010', N'Bàn 10', 'TTBA001', 4),
('B011', N'Bàn 11', 'TTBA001', 4), ('B012', N'Bàn 12', 'TTBA001', 4),
('B013', N'Bàn 13', 'TTBA001', 6), ('B014', N'Bàn 14', 'TTBA001', 6),
('B015', N'Bàn 15', 'TTBA001', 10), ('B016', N'Bàn 16', 'TTBA001', 10),
('B017', N'Bàn 17', 'TTBA001', 2), ('B018', N'Bàn 18', 'TTBA001', 2),
('B019', N'Bàn 19', 'TTBA001', 15), ('B020', N'Bàn 20', 'TTBA001', 20),
('B021', N'Bàn 21', 'TTBA001', 2), ('B022', N'Bàn 22', 'TTBA001', 2),
('B023', N'Bàn 23', 'TTBA001', 4), ('B024', N'Bàn 24', 'TTBA001', 4),
('B025', N'Bàn 25', 'TTBA001', 6), ('B026', N'Bàn 26', 'TTBA001', 6),
('B027', N'Bàn 27', 'TTBA001', 8), ('B028', N'Bàn 28', 'TTBA001', 8),
('B029', N'Bàn 29', 'TTBA001', 10), ('B030', N'Bàn 30', 'TTBA001', 10),
('B031', N'Bàn 31', 'TTBA001', 4), ('B032', N'Bàn 32', 'TTBA001', 4),
('B033', N'Bàn 33', 'TTBA001', 2), ('B034', N'Bàn 34', 'TTBA001', 2),
('B035', N'Bàn 35', 'TTBA001', 12), ('B036', N'Bàn 36', 'TTBA001', 12),
('B037', N'Bàn 37', 'TTBA001', 6), ('B038', N'Bàn 38', 'TTBA001', 6),
('B039', N'Bàn 39', 'TTBA001', 8), ('B040', N'Bàn 40', 'TTBA001', 8);
GO

-- 1. CHÈN DỮ LIỆU NGUYÊN LIỆU (Giữ nguyên)
INSERT INTO [dbo].[NguyenLieu] ([MaNguyenLieu], [TenNguyenLieu], [DonViTinh], [SoLuongTonKho], [GiaBan], [TrangThaiTonKho]) VALUES
('NL001', N'Thịt bò thăn', N'kg', 50, 0, 'BINH_THUONG'), ('NL002', N'Tôm sú (loại 1)', N'kg', 30, 0, 'BINH_THUONG'),
('NL003', N'Gà ta', N'con', 40, 0, 'BINH_THUONG'), ('NL004', N'Cá hồi fillet', N'kg', 20, 0, 'BINH_THUONG'),
('NL005', N'Rau muống', N'bó', 100, 0, 'BINH_THUONG'), ('NL006', N'Nấm kim châm', N'gói', 80, 0, 'BINH_THUONG'),
('NL007', N'Gạo ST25', N'kg', 200, 0, 'BINH_THUONG'), ('NL008', N'Bia Sài Gòn (thùng)', N'thùng', 50, 0, 'BINH_THUONG'),
('NL009', N'Sườn heo non', N'kg', 60, 0, 'BINH_THUONG'), ('NL010', N'Đậu hũ non', N'miếng', 150, 0, 'BINH_THUONG'),
('NL011', N'Súp lơ xanh', N'kg', 50, 0, 'BINH_THUONG'), ('NL012', N'Cà rốt', N'kg', 100, 0, 'BINH_THUONG'),
('NL013', N'Trứng gà', N'quả', 300, 0, 'BINH_THUONG'), ('NL014', N'Sữa đặc', N'hộp', 50, 0, 'BINH_THUONG'),
('NL015', N'Bánh mì sandwich', N'gói', 30, 0, 'BINH_THUONG'), ('NL016', N'Vang đỏ Đà Lạt', N'chai', 20, 0, 'BINH_THUONG'),
('NL017', N'Cà phê hạt', N'kg', 40, 0, 'BINH_THUONG'), ('NL018', N'Nước mắm', N'lít', 100, 0, 'BINH_THUONG'),
('NL019', N'Đường cát', N'kg', 200, 0, 'BINH_THUONG'), ('NL020', N'Bột chiên giòn', N'gói', 80, 0, 'BINH_THUONG'),
('NL021', N'Hạt dưa', N'kg', 10, 0, 'BINH_THUONG'), ('NL022', N'Đậu phộng', N'kg', 15, 0, 'BINH_THUONG'),
('NL023', N'Bánh tráng', N'xấp', 50, 0, 'BINH_THUONG'), ('NL024', N'Dâu tây', N'kg', 5, 0, 'CAN_CANH_BAO'),
('NL025', N'Nho', N'kg', 10, 0, 'BINH_THUONG'), ('NL026', N'Gói lẩu thái', N'gói', 30, 0, 'BINH_THUONG'),
('NL027', N'Lá giang', N'bó', 20, 0, 'BINH_THUONG'), ('NL028', N'Giấm gạo', N'chai', 15, 0, 'BINH_THUONG'),
('NL029', N'Hạt sen', N'kg', 10, 0, 'BINH_THUONG'), ('NL030', N'Sữa chua', N'lốc', 20, 0, 'BINH_THUONG'),
('NL031', N'Nước lọc Aquafina', N'thùng', 30, 0, 'BINH_THUONG'), ('NL032', N'Pepsi (lon)', N'thùng', 25, 0, 'BINH_THUONG'),
('NL033', N'Coca (lon)', N'thùng', 25, 0, 'BINH_THUONG'), ('NL034', N'Hàu sữa', N'con', 100, 0, 'BINH_THUONG'),
('NL035', N'Phô mai', N'kg', 10, 0, 'BINH_THUONG'), ('NL036', N'Tôm hùm', N'con', 10, 0, 'BINH_THUONG'),
('NL037', N'Cua thịt', N'con', 20, 0, 'BINH_THUONG'), ('NL038', N'Ghẹ xanh', N'con', 20, 0, 'BINH_THUONG'),
('NL039', N'Mực ống', N'kg', 30, 0, 'BINH_THUONG'), ('NL040', N'Sò điệp', N'kg', 15, 0, 'BINH_THUONG');
GO

-- 2. CHÈN DỮ LIỆU NHÂN VIÊN (Giữ nguyên)
DECLARE @PasswordQL VARCHAR(100) = '$2a$12$.DiUe2Yopuy/TrhqcMMUI.ne2heyeFpdQgBG2OBA3NUz0Hu4WCZjS';
DECLARE @PasswordNV VARCHAR(100) = '$2a$12$PtG6aCE5oyTOJ3Y05Z2VK.RdMXLX7hI5n9oVgwecuef5FRYEhbcU2';

INSERT INTO [dbo].[NhanVien] ([MaNhanVien], [HoTen], [TenDangNhap], [MatKhau], [MaVaiTro], [Email], [SoDienThoai], [HinhAnh]) VALUES
('NV001', N'Nguyễn Văn Quản Lý', 'manager1', @PasswordQL, 'VT001', 'quanly@email.com', '0987654321', 'anh_a.jpg'),
('NV011', N'Phan Thanh Quản Trị', 'admin2', @PasswordQL, 'VT001', 'admin2@email.com', '0911111101', 'anh_nv11.jpg'),
('NV030', N'Hồng Thất Công', 'manager2', @PasswordQL, 'VT001', 'cong.hong@email.com', '0911111120', 'anh_nv30.jpg'),
('NV002', N'Trần Thị Thu Ngân', 'cashier1', @PasswordNV, 'VT002', 'thungan1@email.com', '0987654322', 'anh_b.jpg'),
('NV009', N'Ngô Văn Thu Ngân', 'cashier2', @PasswordNV, 'VT002', 'thungan2@email.com', '0987654329', 'anh_i.jpg'),
('NV016', N'Hà Thị Thu Ngân 3', 'cashier3', @PasswordNV, 'VT002', 'cashier3@email.com', '0911111106', 'anh_nv16.jpg'),
('NV003', N'Lê Văn Phục Vụ', 'staff1', @PasswordNV, 'VT002', 'phucvu1@email.com', '0987654323', 'anh_c.jpg'),
('NV004', N'Phạm Thị Phục Vụ', 'staff2', @PasswordNV, 'VT002', 'phucvu2@email.com', '0987654324', 'anh_d.jpg'),
('NV005', N'Hoàng Văn Bếp Trưởng', 'chef1', @PasswordNV, 'VT002', 'beptruong@email.com', '0987654325', 'anh_e.jpg'),
('NV006', N'Vũ Thị Bếp Phó', 'chef2', @PasswordNV, 'VT002', 'beppho@email.com', '0987654326', 'anh_f.jpg'),
('NV007', N'Đặng Văn Phục Vụ', 'staff3', @PasswordNV, 'VT002', 'phucvu3@email.com', '0987654327', 'anh_g.jpg'),
('NV008', N'Bùi Thị Phục Vụ', 'staff4', @PasswordNV, 'VT002', 'phucvu4@email.com', '0987654328', 'anh_h.jpg'),
('NV010', N'Dương Thị Nghỉ Việc', 'old_staff', @PasswordNV, 'VT002', 'nghiviec@email.com', '0987654330', 'anh_k.jpg'),
('NV012', N'Lê Thị Bảo Vệ', 'security1', @PasswordNV, 'VT002', 'security1@email.com', '0911111102', 'anh_nv12.jpg'),
('NV013', N'Trần Văn Phục Vụ Mới', 'staff5', @PasswordNV, 'VT002', 'staff5@email.com', '0911111103', 'anh_nv13.jpg'),
('NV014', N'Ngô Thị Tạp Vụ', 'cleaner1', @PasswordNV, 'VT002', 'cleaner1@email.com', '0911111104', 'anh_nv14.jpg'),
('NV015', N'Vũ Hữu Bếp Phụ', 'chef3', @PasswordNV, 'VT002', 'chef3@email.com', '0911111105', 'anh_nv15.jpg'),
('NV017', N'Đặng Văn Thực Tập', 'intern1', @PasswordNV, 'VT002', 'intern1@email.com', '0911111107', 'anh_nv17.jpg'),
('NV018', N'Nguyễn Thị Phục Vụ 6', 'staff6', @PasswordNV, 'VT002', 'staff6@email.com', '0911111108', 'anh_nv18.jpg'),
('NV019', N'Lý Văn Phục Vụ 7', 'staff7', @PasswordNV, 'VT002', 'staff7@email.com', '0911111109', 'anh_nv19.jpg'),
('NV020', N'Bùi Thanh Nghỉ Phép', 'staff8', @PasswordNV, 'VT002', 'staff8@email.com', '0911111110', 'anh_nv20.jpg'),
('NV021', N'Trần Hữu Danh', 'staff9', @PasswordNV, 'VT002', 'danh.tran@email.com', '0911111111', 'anh_nv21.jpg'),
('NV022', N'Lê Thị Kiều', 'staff10', @PasswordNV, 'VT002', 'kieu.le@email.com', '0911111112', 'anh_nv22.jpg'),
('NV023', N'Phạm Văn Mách', 'security2', @PasswordNV, 'VT002', 'mach.pham@email.com', '0911111113', 'anh_nv23.jpg'),
('NV024', N'Đỗ Thị Nở', 'cleaner2', @PasswordNV, 'VT002', 'no.do@email.com', '0911111114', 'anh_nv24.jpg'),
('NV025', N'Quách Tĩnh', 'chef4', @PasswordNV, 'VT002', 'tinh.quach@email.com', '0911111115', 'anh_nv25.jpg'),
('NV026', N'Hoàng Dung', 'chef5', @PasswordNV, 'VT002', 'dung.hoang@email.com', '0911111116', 'anh_nv26.jpg'),
('NV027', N'Dương Khang', 'staff11', @PasswordNV, 'VT002', 'khang.duong@email.com', '0911111117', 'anh_nv27.jpg'),
('NV028', N'Mục Niệm Từ', 'staff12', @PasswordNV, 'VT002', 'tu.muc@email.com', '0911111118', 'anh_nv28.jpg'),
('NV029', N'Âu Dương Phong', 'chef_master', @PasswordNV, 'VT002', 'phong.au@email.com', '0911111119', 'anh_nv29.jpg'),
('NV000', N'Nhân Viên Tạm Thời', 'manager000', @PasswordNV, 'VT002', 'tamthoi@email.com', '0911111120', 'anh_nv30.jpg');
GO

-- 3. CHÈN DỮ LIỆU MÓN ĂN (Giữ nguyên)
INSERT INTO [dbo].[MonAn] ([MaMonAn], [TenMonAn], [MaDanhMuc]) VALUES
('MA001', N'Hạt dưa', 'DM001'), ('MA002', N'Đậu phộng', 'DM001'),
('MA003', N'Chả giò', 'DM001'), ('MA004', N'Mức dâu', 'DM001'),
('MA005', N'Salad trái cây', 'DM001'), ('MA006', N'Lẩu Thái hải sản', 'DM002'),
('MA007', N'Lẩu gà lá giang', 'DM002'), ('MA008', N'Lẩu bò nhúng giấm', 'DM002'),
('MA009', N'Lẩu cá hồi', 'DM002'), ('MA010', N'Lẩu tôm', 'DM002'),
('MA011', N'Bánh flan', 'DM003'), ('MA012', N'Chè hạt sen', 'DM003'),
('MA013', N'Sữa chua nếp cẩm', 'DM003'), ('MA014', N'Kem tươi', 'DM003'),
('MA015', N'Sương sáo hột é', 'DM003'), ('MA016', N'Nước lọc', 'DM004'),
('MA017', N'Nước ép trái cây', 'DM004'), ('MA018', N'Pepsi', 'DM004'),
('MA019', N'Bia Sài Gòn', 'DM004'), ('MA020', N'Coca', 'DM004'),
('MA021', N'Sườn nướng BBQ', 'DM005'), ('MA022', N'Hàu nướng phô mai', 'DM005'),
('MA023', N'Tôm nướng muối ớt', 'DM005'), ('MA024', N'Ba chỉ bò nướng', 'DM005'),
('MA025', N'Gà nướng muối tiêu', 'DM005'), ('MA026', N'Cơm chay đậu hũ', 'DM006'),
('MA027', N'Đậu hũ chiên sả', 'DM006'), ('MA028', N'Canh chua chay', 'DM006'),
('MA029', N'Mì xào chay', 'DM006'), ('MA030', N'Nấm hấp xả', 'DM006'),
('MA031', N'Cơm tấm sườn bì chả', 'DM007'), ('MA032', N'Cơm chiên dương châu', 'DM007'),
('MA033', N'Cơm rang hải sản', 'DM007'), ('MA034', N'Cơm gà xối mỡ', 'DM007'),
('MA035', N'Cơm bò lúc lắc', 'DM007'), ('MA036', N'Tôm hùm nướng bơ tỏi', 'DM008'),
('MA037', N'Cua rang me', 'DM008'), ('MA038', N'Ghẹ hấp bia', 'DM008'),
('MA039', N'Mực chiên giòn', 'DM008'), ('MA040', N'Sò điệp nướng phô mai', 'DM008');
GO

-- 4. CHÈN DỮ LIỆU CHI TIẾT MÓN ĂN (Giữ nguyên)
INSERT INTO [dbo].[ChiTietMonAn] ([MaCT], [TenCT], [MaMonAn]) VALUES
('CT001', N'Chi tiết 1', 'MA001'), ('CT002', N'Chi tiết 1', 'MA002'),
('CT003', N'Chi tiết 1', 'MA003'), ('CT004', N'Chi tiết 1', 'MA004'),
('CT005', N'Chi tiết 1', 'MA005'), ('CT006', N'Chi tiết 1', 'MA006'),
('CT007', N'Chi tiết 1', 'MA007'), ('CT008', N'Chi tiết 1', 'MA008'),
('CT009', N'Chi tiết 1', 'MA009'), ('CT010', N'Chi tiết 1', 'MA010'),
('CT011', N'Chi tiết 1', 'MA011'), ('CT012', N'Chi tiết 1', 'MA012'),
('CT013', N'Chi tiết 1', 'MA013'), ('CT014', N'Chi tiết 1', 'MA014'),
('CT015', N'Chi tiết 1', 'MA015'), ('CT016', N'Chi tiết 1', 'MA016'),
('CT017', N'Chi tiết 1', 'MA017'), ('CT018', N'Chi tiết 1', 'MA018'),
('CT019', N'Chi tiết 1', 'MA019'), ('CT020', N'Chi tiết 1', 'MA020'),
('CT021', N'Chi tiết 1', 'MA021'), ('CT022', N'Chi tiết 1', 'MA022'),
('CT023', N'Chi tiết 1', 'MA023'), ('CT024', N'Chi tiết 1', 'MA024'),
('CT025', N'Chi tiết 1', 'MA025'), ('CT026', N'Chi tiết 1', 'MA026'),
('CT027', N'Chi tiết 1', 'MA027'), ('CT028', N'Chi tiết 1', 'MA028'),
('CT029', N'Chi tiết 1', 'MA029'), ('CT030', N'Chi tiết 1', 'MA030'),
('CT031', N'Chi tiết 1', 'MA031'), ('CT032', N'Chi tiết 1', 'MA032'),
('CT033', N'Chi tiết 1', 'MA033'), ('CT034', N'Chi tiết 1', 'MA034'),
('CT035', N'Chi tiết 1', 'MA035'), ('CT036', N'Chi tiết 1', 'MA036'),
('CT037', N'Chi tiết 1', 'MA037'), ('CT038', N'Chi tiết 1', 'MA038'),
('CT039', N'Chi tiết 1', 'MA039'), ('CT040', N'Chi tiết 1', 'MA040');
GO

-- 5. CHÈN DỮ LIỆU PHIÊN BẢN MÓN ĂN (Giữ nguyên)
INSERT INTO [dbo].[PhienBanMonAn] ([MaPhienBan], [TenPhienBan], [MaTrangThai], [ThuTu]) 
VALUES 
('PB001', N'Size S', 'CON_HANG', 1),
('PB002', N'Size M', 'CON_HANG', 2),
('PB003', N'Size L', 'CON_HANG', 3);
GO

-- 6. CHÈN HÌNH ẢNH MÓN ĂN (Giữ nguyên)
DELETE FROM [dbo].[HinhAnhMonAn];
DBCC CHECKIDENT ('[dbo].[HinhAnhMonAn]', RESEED, 0);
GO
INSERT INTO HinhAnhMonAn (MaMonAn, URLHinhAnh) VALUES
('MA024', 'images/monans/bachibo/bachibo1.jpg'), ('MA024', 'images/monans/bachibo/bachibo2.jpg'),
('MA011', 'images/monans/banhflan/flan1.jpg'), ('MA011', 'images/monans/banhflan/flan2.jpg'),
('MA019', 'images/monans/biasg/bia.jpg'), ('MA028', 'images/monans/canhchuachay/canhchua1.jpg'),
('MA028', 'images/monans/canhchuachay/canhchua2.jpg'), ('MA003', 'images/monans/chagio/chagio1.jpg'),
('MA003', 'images/monans/chagio/chagio2.jpg'), ('MA003', 'images/monans/chagio/chagio3.jpg'),
('MA012', 'images/monans/chehatsen/chehatsen1.jpg'), ('MA012', 'images/monans/chehatsen/chehatsen2.jpg'),
('MA020', 'images/monans/coca/coca.jpg'), ('MA035', 'images/monans/comboluclac/boluclac1.jpg'),
('MA035', 'images/monans/comboluclac/boluclac2.jpg'), ('MA035', 'images/monans/comboluclac/boluclac3.jpg'),
('MA026', 'images/monans/comchaydauhu/dauhu1.jpg'), ('MA026', 'images/monans/comchaydauhu/dauhu2.jpg'),
('MA032', 'images/monans/comchienduongchau/comchien1.jpg'), ('MA032', 'images/monans/comchienduongchau/comchien2.jpg'),
('MA034', 'images/monans/comgaxoimo/comga1.jpg'), ('MA034', 'images/monans/comgaxoimo/comga2.jpg'),
('MA034', 'images/monans/comgaxoimo/comga3.jpg'), ('MA034', 'images/monans/comgaxoimo/comga4.jpg'),
('MA033', 'images/monans/comranghaisan/comrang1.jpg'), ('MA033', 'images/monans/comranghaisan/comrang2.jpg'),
('MA033', 'images/monans/comranghaisan/comrang3.jpg'), ('MA031', 'images/monans/comtamsuonbicha/comtam1.jpg'),
('MA031', 'images/monans/comtamsuonbicha/comtam2.jpg'), ('MA037', 'images/monans/cuarangme/cua1.jpg'),
('MA037', 'images/monans/cuarangme/cua2.jpg'), ('MA027', 'images/monans/dauhuchienxa/dauhuchien1.jpg'),
('MA027', 'images/monans/dauhuchienxa/dauhuchien2.jpg'), ('MA002', 'images/monans/dauphong/dauphong1.jpg'),
('MA002', 'images/monans/dauphong/dauphong2.jpg'), ('MA002', 'images/monans/dauphong/dauphong3.jpg'),
('MA025', 'images/monans/ganuongmuoitieu/ganuong1.jpg'), ('MA025', 'images/monans/ganuongmuoitieu/ganuong2.jpg'),
('MA038', 'images/monans/ghehapbia/ghe1.jpg'), ('MA038', 'images/monans/ghehapbia/ghe2.jpg'),
('MA001', 'images/monans/hatdua/dua1.jpg'), ('MA001', 'images/monans/hatdua/dua2.jpg'),
('MA022', 'images/monans/haunuong/haunuong1.jpg'), ('MA022', 'images/monans/haunuong/haunuong2.jpg'),
('MA014', 'images/monans/kemtuoi/kem1.jpg'), ('MA008', 'images/monans/laubo/laubo1.jpg'),
('MA008', 'images/monans/laubo/laubo2.jpg'), ('MA009', 'images/monans/laucahoi/laucahoi1.jpg'),
('MA009', 'images/monans/laucahoi/laucahoi2.jpg'), ('MA006', 'images/monans/lauthai/lauthai1.jpg'),
('MA006', 'images/monans/lauthai/lauthai2.jpg'), ('MA010', 'images/monans/lautom/lautom1.jpg'),
('MA010', 'images/monans/lautom/lautom2.jpg'), ('MA029', 'images/monans/mixaochay/mixaochay1.jpg'),
('MA029', 'images/monans/mixaochay/mixaochay2.jpg'), ('MA039', 'images/monans/mucchiengion/mucchien1.jpg'),
('MA039', 'images/monans/mucchiengion/mucchien2.jpg'), ('MA004', 'images/monans/mutdau/mutdau1.jpg'),
('MA004', 'images/monans/mutdau/mutdau2.jpg'), ('MA030', 'images/monans/namhapxa/namhapxa1.jpg'),
('MA030', 'images/monans/namhapxa/namhapxa2.jpg'), ('MA017', 'images/monans/nuocep/nuocep1.jpg'),
('MA017', 'images/monans/nuocep/nuocep2.jpg'), ('MA016', 'images/monans/nuocsuoi/nuocsuoi1.jpg'),
('MA018', 'images/monans/pepsi/ppsi.jpg'), ('MA005', 'images/monans/salad/salad1.jpg'),
('MA005', 'images/monans/salad/salad2.jpg'), ('MA040', 'images/monans/sodiepnuongphomai/sodiep1.jpg'),
('MA040', 'images/monans/sodiepnuongphomai/sodiep2.jpg'), ('MA013', 'images/monans/suachuanepcam/suachua1.jpg'),
('MA013', 'images/monans/suachuanepcam/suachua2.jpg'), ('MA015', 'images/monans/suongsaohate/suongsao1.jpg'),
('MA015', 'images/monans/suongsaohate/suongsao2.jpg'), ('MA021', 'images/monans/suonnuong/suon1.jpg'),
('MA021', 'images/monans/suonnuong/suon2.jpg'), ('MA036', 'images/monans/tomhumnuongbo/tomhum1.jpg'),
('MA036', 'images/monans/tomhumnuongbo/tomhum2.jpg'), ('MA036', 'images/monans/tomhumnuongbo/tomhum3.jpg'),
('MA023', 'images/monans/tomnuong/tomnuong1.jpg'), ('MA023', 'images/monans/tomnuong/tomnuong2.jpg'),
('MA007', 'images/monans/laugalagiang/lauga.jpg');
GO

-- 7. CHÈN DỮ LIỆU CUNG ỨNG (Giữ nguyên)
INSERT INTO [dbo].[CungUng] ([MaCungUng], [MaNguyenLieu], [MaNhaCungCap]) VALUES
('CU001', 'NL001', 'NCC001'), ('CU002', 'NL002', 'NCC002'), ('CU003', 'NL003', 'NCC003'), ('CU004', 'NL004', 'NCC004'),
('CU005', 'NL005', 'NCC005'), ('CU006', 'NL006', 'NCC006'), ('CU007', 'NL007', 'NCC007'), ('CU008', 'NL008', 'NCC008'),
('CU009', 'NL009', 'NCC009'), ('CU010', 'NL010', 'NCC010'), ('CU011', 'NL011', 'NCC011'), ('CU012', 'NL012', 'NCC012'),
('CU013', 'NL013', 'NCC013'), ('CU014', 'NL014', 'NCC014'), ('CU015', 'NL015', 'NCC015'), ('CU016', 'NL016', 'NCC016'),
('CU017', 'NL017', 'NCC017'), ('CU018', 'NL018', 'NCC018'), ('CU019', 'NL019', 'NCC019'), ('CU020', 'NL020', 'NCC020'),
('CU021', 'NL021', 'NCC001'), ('CU022', 'NL022', 'NCC002'), ('CU023', 'NL023', 'NCC003'), ('CU024', 'NL024', 'NCC004'),
('CU025', 'NL025', 'NCC005'), ('CU026', 'NL026', 'NCC006'), ('CU027', 'NL027', 'NCC007'), ('CU028', 'NL028', 'NCC008'),
('CU029', 'NL029', 'NCC009'), ('CU030', 'NL030', 'NCC010'), ('CU031', 'NL031', 'NCC011'), ('CU032', 'NL032', 'NCC012'),
('CU033', 'NL033', 'NCC013'), ('CU034', 'NL034', 'NCC014'), ('CU035', 'NL035', 'NCC015'), ('CU036', 'NL036', 'NCC016'),
('CU037', 'NL037', 'NCC017'), ('CU038', 'NL038', 'NCC018'), ('CU039', 'NL039', 'NCC019'), ('CU040', 'NL040', 'NCC020');
GO

-- 8. CHÈN DỮ LIỆU NHẬP HÀNG (Giữ nguyên)
INSERT INTO [dbo].[NhapHang] ([MaNhapHang], [MaNhanVien], [NgayNhapHang], [TongTien], [NgayLapPhieu], [MaTrangThai], [MaNhaCungCap]) VALUES
('NH001', 'NV001', '2025-10-01 08:00:00', 0, '2025-09-30', 'DA_HOAN_TAT', 'NCC001'), 
('NH002', 'NV001', '2025-10-02 09:00:00', 0, '2025-10-01', 'DA_HOAN_TAT', 'NCC002'),
('NH003', 'NV001', '2025-10-03 08:00:00', 0, '2025-10-02', 'DA_HOAN_TAT', 'NCC003'), 
('NH004', 'NV001', '2025-10-04 08:00:00', 0, '2025-10-03', 'DA_HOAN_TAT', 'NCC004'),
('NH005', 'NV001', '2025-10-05 08:00:00', 0, '2025-10-04', 'DA_HOAN_TAT', 'NCC005'), 
('NH006', 'NV011', '2025-10-06 08:00:00', 0, '2025-10-05', 'DA_HOAN_TAT', 'NCC006'),
('NH007', 'NV011', '2025-10-06 09:00:00', 0, '2025-10-05', 'DA_HOAN_TAT', 'NCC007'), 
('NH008', 'NV001', '2025-10-07 08:00:00', 0, '2025-10-06', 'DA_HOAN_TAT', 'NCC008'),
('NH009', 'NV001', '2025-10-07 09:00:00', 0, '2025-10-06', 'DA_HOAN_TAT', 'NCC009'), 
('NH010', 'NV011', '2025-10-08 08:00:00', 0, '2025-10-07', 'DA_HOAN_TAT', 'NCC010'),
('NH011', 'NV011', '2025-10-08 09:00:00', 0, '2025-10-07', 'DA_HOAN_TAT', 'NCC011'), 
('NH012', 'NV001', '2025-10-09 08:00:00', 0, '2025-10-08', 'DA_HOAN_TAT', 'NCC012'),
('NH013', 'NV001', '2025-10-09 09:00:00', 0, '2025-10-08', 'DA_HOAN_TAT', 'NCC013'), 
('NH014', 'NV011', '2025-10-10 08:00:00', 0, '2025-10-09', 'DA_HOAN_TAT', 'NCC014'),
('NH015', 'NV011', '2025-10-10 09:00:00', 0, '2025-10-09', 'DA_HOAN_TAT', 'NCC015'), 
('NH016', 'NV001', '2025-10-11 08:00:00', 0, '2025-10-10', 'DA_HOAN_TAT', 'NCC016'),
('NH017', 'NV001', '2025-10-12 08:00:00', 0, '2025-10-11', 'DA_HOAN_TAT', 'NCC017'), 
('NH018', 'NV001', '2025-10-13 08:00:00', 0, '2025-10-12', 'DA_HOAN_TAT', 'NCC018'),
('NH019', 'NV011', '2025-10-14 08:00:00', 0, '2025-10-13', 'DA_HOAN_TAT', 'NCC019'), 
('NH020', 'NV011', '2025-10-15 08:00:00', 0, '2025-10-14', 'DA_HOAN_TAT', 'NCC020'),
('NH021', 'NV001', '2025-10-16 08:00:00', 0, '2025-10-15', 'DA_HOAN_TAT', 'NCC001'), 
('NH022', 'NV001', '2025-10-17 08:00:00', 0, '2025-10-16', 'DA_HOAN_TAT', 'NCC002'),
('NH023', 'NV011', '2025-10-18 08:00:00', 0, '2025-10-17', 'DA_HOAN_TAT', 'NCC003'), 
('NH024', 'NV011', '2025-10-19 08:00:00', 0, '2025-10-18', 'DA_HOAN_TAT', 'NCC004'),
('NH025', 'NV001', '2025-10-20 08:00:00', 0, '2025-10-19', 'DA_HOAN_TAT', 'NCC005'), 
('NH026', 'NV001', '2025-10-21 08:00:00', 0, '2025-10-20', 'DA_HOAN_TAT', 'NCC006'),
('NH027', 'NV011', '2025-10-22 08:00:00', 0, '2025-10-21', 'DA_HOAN_TAT', 'NCC007'), 
('NH028', 'NV001', '2025-10-23 08:00:00', 0, '2025-10-22', 'DA_HOAN_TAT', 'NCC008'),
('NH029', 'NV011', '2025-10-24 08:00:00', 0, '2025-10-23', 'DA_HOAN_TAT', 'NCC009'), 
('NH030', 'NV001', '2025-10-25 08:00:00', 0, '2025-10-24', 'DA_HOAN_TAT', 'NCC010'),
('NH031', 'NV011', '2025-10-26 08:00:00', 0, '2025-10-25', 'DA_HOAN_TAT', 'NCC011'), 
('NH032', 'NV001', '2025-10-27 08:00:00', 0, '2025-10-26', 'DA_HOAN_TAT', 'NCC012'),
('NH033', 'NV011', '2025-10-28 08:00:00', 0, '2025-10-27', 'DA_HOAN_TAT', 'NCC013'), 
('NH034', 'NV001', '2025-10-29 08:00:00', 0, '2025-10-28', 'DA_HOAN_TAT', 'NCC014'),
('NH035', 'NV011', '2025-10-30 08:00:00', 0, '2025-10-29', 'DA_HOAN_TAT', 'NCC015'), 
('NH036', 'NV001', '2025-10-31 08:00:00', 0, '2025-10-30', 'DA_HOAN_TAT', 'NCC016'),
('NH037', 'NV011', '2025-11-01 08:00:00', 0, '2025-10-31', 'DA_HOAN_TAT', 'NCC017'), 
('NH038', 'NV001', '2025-11-02 08:00:00', 0, '2025-11-01', 'DA_HOAN_TAT', 'NCC018'),
('NH039', 'NV011', '2025-11-03 08:00:00', 0, '2025-11-02', 'DA_HOAN_TAT', 'NCC019'), 
('NH040', 'NV001', '2025-11-04 08:00:00', 0, '2025-11-03', 'DA_HOAN_TAT', 'NCC020');
GO

-- 9. CHÈN CHI TIẾT NHẬP HÀNG (Giữ nguyên)
INSERT INTO [dbo].[ChiTietNhapHang] ([MaNhapHang], [MaNguyenLieu], [SoLuong], [GiaNhap]) VALUES
('NH001', 'NL001', 50, 180000.00), ('NH001', 'NL002', 30, 250000.00),
('NH002', 'NL003', 40, 100000.00), ('NH002', 'NL005', 100, 10000.00),
('NH003', 'NL010', 150, 5000.00), ('NH003', 'NL006', 80, 15000.00),
('NH004', 'NL008', 50, 300000.00), ('NH004', 'NL032', 50, 150000.00),
('NH005', 'NL009', 60, 120000.00), ('NH005', 'NL007', 200, 18000.00),
('NH006', 'NL011', 50, 30000.00), ('NH006', 'NL012', 100, 15000.00),
('NH007', 'NL013', 300, 3000.00), ('NH007', 'NL014', 50, 20000.00),
('NH008', 'NL015', 30, 25000.00), ('NH008', 'NL017', 40, 150000.00),
('NH009', 'NL018', 100, 40000.00), ('NH009', 'NL019', 200, 20000.00),
('NH010', 'NL020', 80, 18000.00), ('NH010', 'NL016', 20, 200000.00),
('NH011', 'NL021', 10, 25000.00), ('NH011', 'NL022', 15, 20000.00),
('NH012', 'NL023', 50, 30000.00), ('NH012', 'NL024', 5, 80000.00),
('NH013', 'NL025', 10, 60000.00), ('NH013', 'NL026', 30, 20000.00),
('NH014', 'NL027', 20, 10000.00), ('NH014', 'NL028', 15, 30000.00),
('NH015', 'NL029', 10, 50000.00), ('NH015', 'NL030', 20, 40000.00),
('NH016', 'NL031', 30, 100000.00), ('NH016', 'NL033', 25, 120000.00),
('NH017', 'NL034', 100, 15000.00), ('NH017', 'NL035', 10, 100000.00),
('NH018', 'NL036', 10, 350000.00), ('NH018', 'NL037', 20, 200000.00),
('NH019', 'NL038', 20, 180000.00), ('NH019', 'NL039', 30, 130000.00),
('NH020', 'NL040', 15, 220000.00), ('NH020', 'NL001', 20, 180000.00),
('NH021', 'NL002', 20, 250000.00), ('NH021', 'NL003', 20, 100000.00),
('NH022', 'NL004', 10, 350000.00), ('NH022', 'NL005', 50, 10000.00),
('NH023', 'NL006', 40, 15000.00), ('NH023', 'NL007', 100, 18000.00),
('NH024', 'NL009', 30, 120000.00), ('NH024', 'NL010', 100, 5000.00),
('NH025', 'NL011', 20, 30000.00), ('NH025', 'NL012', 30, 15000.00),
('NH026', 'NL013', 200, 3000.00), ('NH026', 'NL014', 20, 20000.00),
('NH027', 'NL021', 10, 25000.00), ('NH027', 'NL022', 10, 20000.00),
('NH028', 'NL023', 50, 30000.00), ('NH028', 'NL024', 5, 80000.00),
('NH029', 'NL025', 10, 60000.00), ('NH029', 'NL026', 20, 20000.00),
('NH030', 'NL027', 20, 10000.00), ('NH030', 'NL028', 10, 30000.00),
('NH031', 'NL029', 10, 50000.00), ('NH031', 'NL030', 15, 40000.00),
('NH032', 'NL034', 100, 15000.00), ('NH032', 'NL035', 10, 100000.00),
('NH033', 'NL036', 5, 350000.00), ('NH033', 'NL037', 10, 200000.00),
('NH034', 'NL038', 10, 180000.00), ('NH034', 'NL039', 20, 130000.00),
('NH035', 'NL040', 10, 220000.00), ('NH035', 'NL008', 20, 300000.00),
('NH036', 'NL032', 20, 150000.00), ('NH036', 'NL031', 20, 100000.00),
('NH037', 'NL016', 10, 200000.00), ('NH037', 'NL017', 20, 150000.00),
('NH038', 'NL018', 50, 40000.00), ('NH038', 'NL019', 100, 20000.00),
('NH039', 'NL020', 50, 18000.00), ('NH039', 'NL001', 30, 180000.00),
('NH040', 'NL002', 15, 250000.00), ('NH040', 'NL003', 25, 100000.00);
GO

-- 10. CẬP NHẬT GIÁ BÁN NGUYÊN LIỆU (Giữ nguyên)
WITH MinGiaNhap AS (
    SELECT 
        CTNH.MaNguyenLieu,
        MIN(CTNH.GiaNhap) AS MinPrice
    FROM [dbo].[ChiTietNhapHang] CTNH
    GROUP BY CTNH.MaNguyenLieu
)
UPDATE NL
SET NL.GiaBan = MGS.MinPrice * 2
FROM [dbo].[NguyenLieu] NL
JOIN MinGiaNhap MGS ON NL.MaNguyenLieu = MGS.MaNguyenLieu
WHERE MGS.MinPrice IS NOT NULL AND MGS.MinPrice > 0;
GO

UPDATE [dbo].[NguyenLieu]
SET [GiaBan] = 10000 
WHERE [GiaBan] IS NULL OR [GiaBan] = 0;
GO

-----------------------------------------------------------------
-- SỬA LẠI DỮ LIỆU CHO BẢNG CÔNG THỨC NẤU ĂN--
-----------------------------------------------------------------
-- XÓA DỮ LIỆU CŨ trong CongThucNauAn để chèn lại (Giữ nguyên)
DELETE FROM [dbo].[CongThucNauAn];
GO
--CHÈN DỮ LIỆU CÔNG THỨC MỚI (Giữ nguyên)
INSERT INTO [dbo].[CongThucNauAn] ([MaCongThuc], [MaCT], [MaPhienBan], [Gia]) VALUES
-- MA001 (Hạt dưa)
('CT001_S', 'CT001', 'PB001', 30000), ('CT001_M', 'CT001', 'PB002', 39000), ('CT001_L', 'CT001', 'PB003', 54000),
-- MA002 (Đậu phộng)
('CT002_S', 'CT002', 'PB001', 30000), ('CT002_M', 'CT002', 'PB002', 39000), ('CT002_L', 'CT002', 'PB003', 54000),
-- MA003 (Chả giò)
('CT003_S', 'CT003', 'PB001', 35000), ('CT003_M', 'CT003', 'PB002', 45000), ('CT003_L', 'CT003', 'PB003', 63000),
-- MA004 (Mứt dâu)
('CT004_S', 'CT004', 'PB001', 35000), ('CT004_M', 'CT004', 'PB002', 45000), ('CT004_L', 'CT004', 'PB003', 63000),
-- MA005 (Salad trái cây)
('CT005_S', 'CT005', 'PB001', 40000), ('CT005_M', 'CT005', 'PB002', 52000), ('CT005_L', 'CT005', 'PB003', 72000),
-- MA006 (Lẩu Thái hải sản)
('CT006_S', 'CT006', 'PB001', 250000), ('CT006_M', 'CT006', 'PB002', 350000), ('CT006_L', 'CT006', 'PB003', 450000),
-- MA007 (Lẩu gà lá giang)
('CT007_S', 'CT007', 'PB001', 200000), ('CT007_M', 'CT007', 'PB002', 280000), ('CT007_L', 'CT007', 'PB003', 380000),
-- MA008 (Lẩu bò nhúng giấm)
('CT008_S', 'CT008', 'PB001', 280000), ('CT008_M', 'CT008', 'PB002', 390000), ('CT008_L', 'CT008', 'PB003', 500000),
-- MA009 (Lẩu cá hồi)
('CT009_S', 'CT009', 'PB001', 300000), ('CT009_M', 'CT009', 'PB002', 420000), ('CT009_L', 'CT009', 'PB003', 540000),
-- MA010 (Lẩu tôm)
('CT010_S', 'CT010', 'PB001', 220000), ('CT010_M', 'CT010', 'PB002', 300000), ('CT010_L', 'CT010', 'PB003', 400000),
-- MA011 (Bánh flan)
('CT011_S', 'CT011', 'PB001', 25000), ('CT011_M', 'CT011', 'PB002', 32000), ('CT011_L', 'CT011', 'PB003', 45000),
-- MA012 (Chè hạt sen)
('CT012_S', 'CT012', 'PB001', 30000), ('CT012_M', 'CT012', 'PB002', 39000), ('CT012_L', 'CT012', 'PB003', 54000),
-- MA013 (Sữa chua nếp cẩm)
('CT013_S', 'CT013', 'PB001', 25000), ('CT013_M', 'CT013', 'PB002', 32000), ('CT013_L', 'CT013', 'PB003', 45000),
-- MA014 (Kem tươi)
('CT014_S', 'CT014', 'PB001', 40000), ('CT014_M', 'CT014', 'PB002', 52000), ('CT014_L', 'CT014', 'PB003', 72000),
-- MA015 (Sương sáo hột é)
('CT015_S', 'CT015', 'PB001', 35000), ('CT015_M', 'CT015', 'PB002', 45000), ('CT015_L', 'CT015', 'PB003', 63000),
-- MA016 (Nước lọc)
('CT016_S', 'CT016', 'PB001', 10000), ('CT016_M', 'CT016', 'PB002', 13000), ('CT016_L', 'CT016', 'PB003', 18000),
-- MA017 (Nước ép trái cây)
('CT017_S', 'CT017', 'PB001', 30000), ('CT017_M', 'CT017', 'PB002', 39000), ('CT017_L', 'CT017', 'PB003', 54000),
-- MA018 (Pepsi)
('CT018_S', 'CT018', 'PB001', 25000), ('CT018_M', 'CT018', 'PB002', 32000), ('CT018_L', 'CT018', 'PB003', 45000),
-- MA019 (Bia Sài Gòn)
('CT019_S', 'CT019', 'PB001', 30000), ('CT019_M', 'CT019', 'PB002', 39000), ('CT019_L', 'CT019', 'PB003', 54000),
-- MA020 (Coca)
('CT020_S', 'CT020', 'PB001', 25000), ('CT020_M', 'CT020', 'PB002', 32000), ('CT020_L', 'CT020', 'PB003', 45000),
-- MA021 (Sườn nướng BBQ)
('CT021_S', 'CT021', 'PB001', 150000), ('CT021_M', 'CT021', 'PB002', 200000), ('CT021_L', 'CT021', 'PB003', 270000),
-- MA022 (Hàu nướng phô mai)
('CT022_S', 'CT022', 'PB001', 220000), ('CT022_M', 'CT022', 'PB002', 300000), ('CT022_L', 'CT022', 'PB003', 400000),
-- MA023 (Tôm nướng muối ớt)
('CT023_S', 'CT023', 'PB001', 200000), ('CT023_M', 'CT023', 'PB002', 280000), ('CT023_L', 'CT023', 'PB003', 360000),
-- MA024 (Ba chỉ bò nướng)
('CT024_S', 'CT024', 'PB001', 180000), ('CT024_M', 'CT024', 'PB002', 250000), ('CT024_L', 'CT024', 'PB003', 320000),
-- MA025 (Gà nướng muối tiêu)
('CT025_S', 'CT025', 'PB001', 200000), ('CT025_M', 'CT025', 'PB002', 280000), ('CT025_L', 'CT025', 'PB003', 360000),
-- MA026 (Cơm chay đậu hũ)
('CT026_S', 'CT026', 'PB001', 60000), ('CT026_M', 'CT026', 'PB002', 78000), ('CT026_L', 'CT026', 'PB003', 108000),
-- MA027 (Đậu hũ chiên sả)
('CT027_S', 'CT027', 'PB001', 40000), ('CT027_M', 'CT027', 'PB002', 52000), ('CT027_L', 'CT027', 'PB003', 72000),
-- MA028 (Canh chua chay)
('CT028_S', 'CT028', 'PB001', 45000), ('CT028_M', 'CT028', 'PB002', 58000), ('CT028_L', 'CT028', 'PB003', 81000),
-- MA029 (Mì xào chay)
('CT029_S', 'CT029', 'PB001', 55000), ('CT029_M', 'CT029', 'PB002', 71000), ('CT029_L', 'CT029', 'PB003', 99000),
-- MA030 (Nấm hấp xả)
('CT030_S', 'CT030', 'PB001', 50000), ('CT030_M', 'CT030', 'PB002', 65000), ('CT030_L', 'CT030', 'PB003', 90000),
-- MA031 (Cơm tấm sườn bì chả)
('CT031_S', 'CT031', 'PB001', 75000), ('CT031_M', 'CT031', 'PB002', 97000), ('CT031_L', 'CT031', 'PB003', 135000),
-- MA032 (Cơm chiên dương châu)
('CT032_S', 'CT032', 'PB001', 70000), ('CT032_M', 'CT032', 'PB002', 91000), ('CT032_L', 'CT032', 'PB003', 126000),
-- MA033 (Cơm rang hải sản)
('CT033_S', 'CT033', 'PB001', 85000), ('CT033_M', 'CT033', 'PB002', 110000), ('CT033_L', 'CT033', 'PB003', 153000),
-- MA034 (Cơm gà xối mỡ)
('CT034_S', 'CT034', 'PB001', 75000), ('CT034_M', 'CT034', 'PB002', 97000), ('CT034_L', 'CT034', 'PB003', 135000),
-- MA035 (Cơm bò lúc lắc)
('CT035_S', 'CT035', 'PB001', 80000), ('CT035_M', 'CT035', 'PB002', 104000), ('CT035_L', 'CT035', 'PB003', 144000),
-- MA036 (Tôm hùm nướng bơ tỏi)
('CT036_S', 'CT036', 'PB001', 450000), ('CT036_M', 'CT036', 'PB002', 600000), ('CT036_L', 'CT036', 'PB003', 800000),
-- MA037 (Cua rang me)
('CT037_S', 'CT037', 'PB001', 300000), ('CT037_M', 'CT037', 'PB002', 420000), ('CT037_L', 'CT037', 'PB003', 540000),
-- MA038 (Ghẹ hấp bia)
('CT038_S', 'CT038', 'PB001', 280000), ('CT038_M', 'CT038', 'PB002', 390000), ('CT038_L', 'CT038', 'PB003', 500000),
-- MA039 (Mực chiên giòn)
('CT039_S', 'CT039', 'PB001', 180000), ('CT039_M', 'CT039', 'PB002', 250000), ('CT039_L', 'CT039', 'PB003', 320000),
-- MA040 (Sò điệp nướng phô mai)
('CT040_S', 'CT040', 'PB001', 250000), ('CT040_M', 'CT040', 'PB002', 350000), ('CT040_L', 'CT040', 'PB003', 450000);
GO


-- 12. CHÈN CHI TIẾT CÔNG THỨC (Sửa lại để khớp với mã công thức CTxxx_S/M/L)
-- Quy tắc số lượng: S < M < L (S = x1, M = x1.5 làm tròn, L = x2)
INSERT INTO [dbo].[ChiTietCongThuc] ([MaCongThuc], [MaNguyenLieu], [SoLuongCanDung]) VALUES
-- CT001 (Hạt dưa) - S:1, M:2, L:2
('CT001_S', 'NL021', 1), ('CT001_M', 'NL021', 2), ('CT001_L', 'NL021', 2),
-- CT002 (Đậu phộng) - S:1, M:2, L:2
('CT002_S', 'NL022', 1), ('CT002_M', 'NL022', 2), ('CT002_L', 'NL022', 2),
-- CT003 (Chả giò) - S:1, M:2, L:2
('CT003_S', 'NL023', 1), ('CT003_M', 'NL023', 2), ('CT003_L', 'NL023', 2),
('CT003_S', 'NL009', 1), ('CT003_M', 'NL009', 2), ('CT003_L', 'NL009', 2),
-- CT004 (Mứt dâu) - S:1, M:2, L:2
('CT004_S', 'NL024', 1), ('CT004_M', 'NL024', 2), ('CT004_L', 'NL024', 2),
-- CT005 (Salad trái cây) - S:1, M:2, L:2
('CT005_S', 'NL025', 1), ('CT005_M', 'NL025', 2), ('CT005_L', 'NL025', 2),
('CT005_S', 'NL024', 1), ('CT005_M', 'NL024', 2), ('CT005_L', 'NL024', 2),
-- CT006 (Lẩu Thái hải sản) - S:1, M:2, L:2
('CT006_S', 'NL026', 1), ('CT006_M', 'NL026', 2), ('CT006_L', 'NL026', 2),
('CT006_S', 'NL002', 1), ('CT006_M', 'NL002', 2), ('CT006_L', 'NL002', 2),
('CT006_S', 'NL004', 1), ('CT006_M', 'NL004', 2), ('CT006_L', 'NL004', 2),
('CT006_S', 'NL039', 1), ('CT006_M', 'NL039', 2), ('CT006_L', 'NL039', 2),
('CT006_S', 'NL005', 2), ('CT006_M', 'NL005', 3), ('CT006_L', 'NL005', 4),
('CT006_S', 'NL006', 1), ('CT006_M', 'NL006', 2), ('CT006_L', 'NL006', 2),
-- CT007 (Lẩu gà lá giang) - S:1, M:2, L:2
('CT007_S', 'NL003', 1), ('CT007_M', 'NL003', 2), ('CT007_L', 'NL003', 2),
('CT007_S', 'NL027', 1), ('CT007_M', 'NL027', 2), ('CT007_L', 'NL027', 2),
('CT007_S', 'NL006', 1), ('CT007_M', 'NL006', 2), ('CT007_L', 'NL006', 2),
-- CT008 (Lẩu bò nhúng giấm) - S:2, M:3, L:4
('CT008_S', 'NL001', 2), ('CT008_M', 'NL001', 3), ('CT008_L', 'NL001', 4),
('CT008_S', 'NL028', 1), ('CT008_M', 'NL028', 2), ('CT008_L', 'NL028', 2),
('CT008_S', 'NL005', 2), ('CT008_M', 'NL005', 3), ('CT008_L', 'NL005', 4),
('CT008_S', 'NL012', 1), ('CT008_M', 'NL012', 2), ('CT008_L', 'NL012', 2),
-- CT009 (Lẩu cá hồi) - S:1, M:2, L:2
('CT009_S', 'NL004', 1), ('CT009_M', 'NL004', 2), ('CT009_L', 'NL004', 2),
-- CT010 (Lẩu tôm) - S:1, M:2, L:2
('CT010_S', 'NL002', 1), ('CT010_M', 'NL002', 2), ('CT010_L', 'NL002', 2),
-- CT011 (Bánh flan) - S:2, M:3, L:4
('CT011_S', 'NL013', 2), ('CT011_M', 'NL013', 3), ('CT011_L', 'NL013', 4),
('CT011_S', 'NL014', 1), ('CT011_M', 'NL014', 2), ('CT011_L', 'NL014', 2),
-- CT012 (Chè hạt sen) - S:1, M:2, L:2
('CT012_S', 'NL029', 1), ('CT012_M', 'NL029', 2), ('CT012_L', 'NL029', 2),
-- CT013 (Sữa chua nếp cẩm) - S:1, M:2, L:2
('CT013_S', 'NL030', 1), ('CT013_M', 'NL030', 2), ('CT013_L', 'NL030', 2),
-- CT014 (Kem tươi) - S:1, M:2, L:2
('CT014_S', 'NL030', 1), ('CT014_M', 'NL030', 2), ('CT014_L', 'NL030', 2),
-- CT015 (Sương sáo hột é) - S:1, M:2, L:2
('CT015_S', 'NL029', 1), ('CT015_M', 'NL029', 2), ('CT015_L', 'NL029', 2),
-- CT016 (Nước lọc) - S:1, M:2, L:2
('CT016_S', 'NL031', 1), ('CT016_M', 'NL031', 2), ('CT016_L', 'NL031', 2),
-- CT017 (Nước ép trái cây) - S:1, M:2, L:2
('CT017_S', 'NL025', 1), ('CT017_M', 'NL025', 2), ('CT017_L', 'NL025', 2),
-- CT018 (Pepsi) - S:1, M:2, L:2
('CT018_S', 'NL032', 1), ('CT018_M', 'NL032', 2), ('CT018_L', 'NL032', 2),
-- CT019 (Bia Sài Gòn) - S:1, M:2, L:2
('CT019_S', 'NL008', 1), ('CT019_M', 'NL008', 2), ('CT019_L', 'NL008', 2),
-- CT020 (Coca) - S:1, M:2, L:2
('CT020_S', 'NL033', 1), ('CT020_M', 'NL033', 2), ('CT020_L', 'NL033', 2),
-- CT021 (Sườn nướng BBQ) - S:1, M:2, L:2
('CT021_S', 'NL009', 1), ('CT021_M', 'NL009', 2), ('CT021_L', 'NL009', 2),
-- CT022 (Hàu nướng phô mai) - S:3, M:5, L:6
('CT022_S', 'NL034', 3), ('CT022_M', 'NL034', 5), ('CT022_L', 'NL034', 6),
('CT022_S', 'NL035', 1), ('CT022_M', 'NL035', 2), ('CT022_L', 'NL035', 2),
-- CT023 (Tôm nướng muối ớt) - S:1, M:2, L:2
('CT023_S', 'NL002', 1), ('CT023_M', 'NL002', 2), ('CT023_L', 'NL002', 2),
-- CT024 (Ba chỉ bò nướng) - S:1, M:2, L:2
('CT024_S', 'NL001', 1), ('CT024_M', 'NL001', 2), ('CT024_L', 'NL001', 2),
-- CT025 (Gà nướng muối tiêu) - S:1, M:2, L:2
('CT025_S', 'NL003', 1), ('CT025_M', 'NL003', 2), ('CT025_L', 'NL003', 2),
-- CT026 (Cơm chay đậu hũ) - S:1, M:2, L:2
('CT026_S', 'NL010', 1), ('CT026_M', 'NL010', 2), ('CT026_L', 'NL010', 2),
('CT026_S', 'NL007', 1), ('CT026_M', 'NL007', 2), ('CT026_L', 'NL007', 2),
-- CT027 (Đậu hũ chiên sả) - S:2, M:3, L:4
('CT027_S', 'NL010', 2), ('CT027_M', 'NL010', 3), ('CT027_L', 'NL010', 4),
-- CT028 (Canh chua chay) - S:1, M:2, L:2
('CT028_S', 'NL005', 1), ('CT028_M', 'NL005', 2), ('CT028_L', 'NL005', 2),
('CT028_S', 'NL010', 1), ('CT028_M', 'NL010', 2), ('CT028_L', 'NL010', 2),
-- CT029 (Mì xào chay) - S:1, M:2, L:2
('CT029_S', 'NL006', 1), ('CT029_M', 'NL006', 2), ('CT029_L', 'NL006', 2),
('CT029_S', 'NL010', 1), ('CT029_M', 'NL010', 2), ('CT029_L', 'NL010', 2),
-- CT030 (Nấm hấp xả) - S:1, M:2, L:2
('CT030_S', 'NL006', 1), ('CT030_M', 'NL006', 2), ('CT030_L', 'NL006', 2),
-- CT031 (Cơm tấm sườn bì chả) - S:1, M:2, L:2
('CT031_S', 'NL009', 1), ('CT031_M', 'NL009', 2), ('CT031_L', 'NL009', 2),
('CT031_S', 'NL007', 1), ('CT031_M', 'NL007', 2), ('CT031_L', 'NL007', 2),
('CT031_S', 'NL013', 1), ('CT031_M', 'NL013', 2), ('CT031_L', 'NL013', 2),
-- CT032 (Cơm chiên dương châu) - S:1, M:2, L:2
('CT032_S', 'NL007', 1), ('CT032_M', 'NL007', 2), ('CT032_L', 'NL007', 2),
('CT032_S', 'NL012', 1), ('CT032_M', 'NL012', 2), ('CT032_L', 'NL012', 2),
-- CT033 (Cơm rang hải sản) - S:1, M:2, L:2
('CT033_S', 'NL007', 1), ('CT033_M', 'NL007', 2), ('CT033_L', 'NL007', 2),
('CT033_S', 'NL002', 1), ('CT033_M', 'NL002', 2), ('CT033_L', 'NL002', 2),
('CT033_S', 'NL039', 1), ('CT033_M', 'NL039', 2), ('CT033_L', 'NL039', 2),
-- CT034 (Cơm gà xối mỡ) - S:1, M:2, L:2
('CT034_S', 'NL007', 1), ('CT034_M', 'NL007', 2), ('CT034_L', 'NL007', 2),
('CT034_S', 'NL003', 1), ('CT034_M', 'NL003', 2), ('CT034_L', 'NL003', 2),
-- CT035 (Cơm bò lúc lắc) - S:1, M:2, L:2
('CT035_S', 'NL007', 1), ('CT035_M', 'NL007', 2), ('CT035_L', 'NL007', 2),
('CT035_S', 'NL001', 1), ('CT035_M', 'NL001', 2), ('CT035_L', 'NL001', 2),
('CT035_S', 'NL012', 1), ('CT035_M', 'NL012', 2), ('CT035_L', 'NL012', 2),
-- CT036 (Tôm hùm nướng bơ tỏi) - S:1, M:2, L:2
('CT036_S', 'NL036', 1), ('CT036_M', 'NL036', 2), ('CT036_L', 'NL036', 2),
-- CT037 (Cua rang me) - S:1, M:2, L:2
('CT037_S', 'NL037', 1), ('CT037_M', 'NL037', 2), ('CT037_L', 'NL037', 2),
-- CT038 (Ghẹ hấp bia) - S:1, M:2, L:2
('CT038_S', 'NL038', 1), ('CT038_M', 'NL038', 2), ('CT038_L', 'NL038', 2),
('CT038_S', 'NL008', 1), ('CT038_M', 'NL008', 2), ('CT038_L', 'NL008', 2),
-- CT039 (Mực chiên giòn) - S:1, M:2, L:2
('CT039_S', 'NL039', 1), ('CT039_M', 'NL039', 2), ('CT039_L', 'NL039', 2),
('CT039_S', 'NL020', 1), ('CT039_M', 'NL020', 2), ('CT039_L', 'NL020', 2),
-- CT040 (Sò điệp nướng phô mai) - S:4, M:6, L:8
('CT040_S', 'NL040', 4), ('CT040_M', 'NL040', 6), ('CT040_L', 'NL040', 8),
('CT040_S', 'NL035', 1), ('CT040_M', 'NL035', 2), ('CT040_L', 'NL035', 2);
GO

-- 13. CHÈN DỮ LIỆU ĐƠN HÀNG & BÀN ĂN ĐƠN HÀNG (Giữ nguyên)
INSERT INTO [dbo].[DonHang] ([MaDonHang], [MaNhanVien], [MaKhachHang], [MaTrangThaiDonHang], [ThoiGianDatHang], [TGDatDuKien], [TGNhanBan], [ThoiGianKetThuc], [SoLuongNguoiDK], [TienDatCoc], [GhiChu], [ThanhToan]) VALUES
('DH001', 'NV003', 'KH001', 'DA_HOAN_THANH', '2025-10-08 18:00:00', NULL, '2025-10-08 18:15:00', '2025-10-08 20:00:00', 5, 0, N'Đã thanh toán (Tháng 10)', 1),
('DH002', 'NV004', 'KH002', 'DA_HOAN_THANH', '2025-10-08 19:00:00', NULL, '2025-10-08 19:10:00', '2025-10-08 21:00:00', 10, 0, N'Đã thanh toán (Tháng 10)', 1),
('DH003', 'NV007', 'KH003', 'DA_HOAN_THANH', '2025-10-09 11:00:00', NULL, '2025-10-09 11:05:00', '2025-10-09 12:00:00', 4, 0, N'Đã thanh toán (Tháng 10)', 1),
('DH004', 'NV008', 'KH004', 'DA_HOAN_THANH', '2025-10-09 12:00:00', NULL, '2025-10-09 12:05:00', '2025-10-09 13:00:00', 2, 0, N'Đã thanh toán (Tháng 10)', 1),
('DH005', 'NV003', 'KH005', 'DA_HOAN_THANH', '2025-10-10 18:30:00', NULL, '2025-10-10 18:40:00', '2025-10-10 20:30:00', 4, 0, N'Đã thanh toán (Tháng 10)', 1),
('DH006', 'NV004', 'KH006', 'DA_HOAN_THANH', '2025-10-11 19:00:00', NULL, '2025-10-11 19:20:00', '2025-10-11 21:30:00', 9, 0, N'Đã thanh toán (Tháng 10)', 1),
('DH007', 'NV007', 'KH007', 'DA_HOAN_THANH', '2025-10-12 20:00:00', NULL, '2025-10-12 20:05:00', '2025-10-12 21:00:00', 2, 0, N'Đã thanh toán (Tháng 10)', 1),
('DH008', 'NV008', 'KH008', 'DA_HOAN_THANH', '2025-10-13 17:00:00', NULL, '2025-10-13 17:10:00', '2025-10-13 18:00:00', 3, 0, N'Đã thanh toán (Tháng 10)', 1),
('DH009', 'NV003', 'KH009', 'DA_HOAN_THANH', '2025-10-14 19:30:00', NULL, '2025-10-14 19:45:00', '2025-10-14 21:00:00', 6, 0, N'Đã thanh toán (Tháng 10)', 1),
('DH010', 'NV004', 'KH010', 'DA_HOAN_THANH', '2025-10-15 18:00:00', NULL, '2025-10-15 18:10:00', '2025-10-15 20:00:00', 7, 0, N'Đã thanh toán (Tháng 10)', 1),
('DH011', 'NV013', 'KH011', 'DA_HOAN_THANH', '2025-11-01 18:00:00', NULL, '2025-11-01 18:10:00', '2025-11-01 20:00:00', 4, 0, N'Đã thanh toán (Tháng 11)', 1),
('DH012', 'NV018', 'KH012', 'DA_HOAN_THANH', '2025-11-01 18:05:00', NULL, '2025-11-01 18:15:00', '2025-11-01 19:30:00', 3, 0, N'Đã thanh toán (Tháng 11)', 1),
('DH013', 'NV019', 'KH013', 'DA_HOAN_THANH', '2025-11-02 19:00:00', NULL, '2025-11-02 19:15:00', '2025-11-02 21:00:00', 6, 0, N'Đã thanh toán (Tháng 11)', 1),
('DH014', 'NV013', 'KH014', 'DA_HOAN_THANH', '2025-11-03 12:00:00', NULL, '2025-11-03 12:05:00', '2025-11-03 13:00:00', 2, 0, N'Đã thanh toán (Tháng 11)', 1),
('DH015', 'NV018', 'KH015', 'DA_HOAN_THANH', '2025-11-03 12:10:00', NULL, '2025-11-03 12:15:00', '2025-11-03 13:15:00', 2, 0, N'Đã thanh toán (Tháng 11)', 1),
('DH016', 'NV019', 'KH016', 'DA_HOAN_THANH', '2025-11-05 19:00:00', NULL, '2025-11-05 19:10:00', '2025-11-05 21:00:00', 18, 0, N'Đã thanh toán (Tháng 11)', 1),
('DH017', 'NV013', 'KH017', 'DA_HOAN_THANH', '2025-11-06 18:30:00', NULL, '2025-11-06 18:45:00', '2025-11-06 20:30:00', 10, 0, N'Đã thanh toán (Tháng 11)', 1),
('DH018', 'NV018', 'KH018', 'DA_HOAN_THANH', '2025-11-07 11:00:00', NULL, '2025-11-07 11:05:00', '2025-11-07 12:00:00', 1, 0, N'Đã thanh toán (Tháng 11)', 1),
('DH019', 'NV019', 'KH019', 'DA_HOAN_THANH', '2025-11-07 11:05:00', NULL, '2025-11-07 11:10:00', '2025-11-07 12:30:00', 3, 0, N'Đã thanh toán (Tháng 11)', 1),
('DH020', 'NV013', 'KH020', 'DA_HOAN_THANH', '2025-11-08 19:00:00', NULL, '2025-11-08 19:10:00', '2025-11-08 20:45:00', 5, 0, N'Đã thanh toán (Tháng 11)', 1),
('DH021', 'NV003', 'KH021', 'DA_HOAN_THANH', '2025-11-08 19:15:00', NULL, '2025-11-08 19:25:00', '2025-11-08 21:00:00', 4, 0, N'Đã thanh toán (Tháng 11)', 1),
('DH022', 'NV004', 'KH022', 'DA_HOAN_THANH', '2025-11-09 12:00:00', NULL, '2025-11-09 12:05:00', '2025-11-09 13:00:00', 6, 0, N'Đã thanh toán (Tháng 11)', 1),
('DH023', 'NV007', 'KH023', 'DA_HOAN_THANH', '2025-11-09 12:05:00', NULL, '2025-11-09 12:10:00', '2025-11-09 13:30:00', 5, 0, N'Đã thanh toán (Tháng 11)', 1),
('DH024', 'NV008', 'KH024', 'DA_XAC_NHAN', '2025-11-10 18:00:00', NULL, NULL, NULL, 8, 500000.00, N'Đã đặt cọc 500k (B005)', 0),
('DH025', 'NV013', 'KH025', 'DA_XAC_NHAN', '2025-11-10 18:30:00', NULL, NULL, NULL, 15, 1000000.00, N'Đặt cọc 1 triệu (B019)', 0),
('DH026', 'NV018', 'KH026', 'DA_HOAN_THANH', '2025-11-10 19:00:00', NULL, '2025-11-10 19:10:00', '2025-11-10 21:00:00', 7, 0, N'Đã thanh toán (Tháng 11)', 1),
('DH027', 'NV019', 'KH027', 'DA_HOAN_THANH', '2025-11-11 19:15:00', NULL, '2025-11-11 19:25:00', '2025-11-11 20:30:00', 8, 0, N'Đã thanh toán (Tháng 11)', 1),
('DH028', 'NV003', 'KH028', 'DA_HUY', '2025-11-12 11:30:00', NULL, NULL, NULL, 9, 0, N'Khách gọi báo hủy', 0),
('DH029', 'NV004', 'KH029', 'DA_HOAN_THANH', '2025-11-12 12:00:00', NULL, '2025-11-12 12:05:00', '2025-11-12 13:15:00', 10, 0, N'Đã thanh toán (Tháng 11)', 1),
('DH030', 'NV007', 'KH030', 'NO_SHOW', '2025-11-13 19:00:00', NULL, NULL, NULL, 2, 0, N'Khách không đến (No-Show)', 0),
('DH031', 'NV008', 'KH031', 'DA_HOAN_THANH', '2025-11-14 19:00:00', NULL, '2025-11-14 19:10:00', '2025-11-14 21:00:00', 4, 0, N'Đã thanh toán (Tháng 11)', 1),
('DH032', 'NV013', 'KH032', 'CHO_XAC_NHAN', '2025-11-15 20:00:00', NULL, NULL, NULL, 2, 0, N'Đơn mới, chờ gọi xác nhận', 0),
('DH033', 'NV018', 'KH033', 'DA_HOAN_THANH', '2025-11-16 18:00:00', NULL, '2025-11-16 18:10:00', '2025-11-16 19:00:00', 2, 0, N'Đã thanh toán (Tháng 11)', 1),
('DH034', 'NV019', 'KH034', 'DA_HOAN_THANH', '2025-11-17 11:00:00', NULL, '2025-11-17 11:10:00', '2025-11-17 13:00:00', 12, 0, N'Đã thanh toán (Tháng 11)', 1),
('DH035', 'NV003', 'KH035', 'DA_HOAN_THANH', '2025-11-18 11:30:00', NULL, '2025-11-18 11:40:00', '2025-11-18 13:00:00', 11, 0, N'Đã thanh toán (Tháng 11)', 1),
('DH036', 'NV004', 'KH036', 'DA_HOAN_THANH', '2025-11-19 19:00:00', NULL, '2025-11-19 19:10:00', '2025-11-19 21:00:00', 7, 0, N'Đã thanh toán (Tháng 11)', 1),
('DH037', 'NV007', 'KH037', 'DA_HOAN_THANH', '2025-11-20 19:00:00', NULL, '2025-11-20 19:10:00', '2025-11-20 21:30:00', 8, 0, N'Đã thanh toán (Tháng 11)', 1),
('DH038', 'NV008', 'KH038', 'DA_HOAN_THANH', '2025-11-21 12:00:00', NULL, '2025-11-21 12:05:00', '2025-11-21 13:00:00', 3, 0, N'Đã thanh toán (Tháng 11)', 1),
('DH039', 'NV013', 'KH039', 'DA_HOAN_THANH', '2025-11-22 18:00:00', NULL, '2025-11-22 18:10:00', '2025-11-22 20:00:00', 4, 0, N'Đã thanh toán (Tháng 11)', 1),
('DH040', 'NV018', 'KH040', 'DA_HOAN_THANH', '2025-11-23 19:00:00', NULL, '2025-11-23 19:10:00', '2025-11-23 21:00:00', 5, 0, N'Đã thanh toán (Tháng 11)', 1);
GO

INSERT INTO [dbo].[BanAnDonHang] ([MaBanAnDonHang], [MaDonHang], [MaBan], [MaChiTietDonHang])
SELECT 
    'BDH' + RIGHT('000' + CAST(ROW_NUMBER() OVER(ORDER BY CT.MaChiTietDonHang) AS VARCHAR), 3),
    CT.MaDonHang,
    -- Logic gán bàn giả định
    CASE 
        WHEN CT.MaDonHang = 'DH001' THEN 'B003'
        WHEN CT.MaDonHang = 'DH002' THEN 'B008'
        WHEN CT.MaDonHang = 'DH003' THEN 'B001'
        WHEN CT.MaDonHang = 'DH004' THEN 'B002'
        WHEN CT.MaDonHang = 'DH005' THEN 'B004'
        WHEN CT.MaDonHang = 'DH024' THEN 'B005'
        ELSE 'B001'
    END,
    CT.MaChiTietDonHang -- Bây giờ nó là chuỗi 'CTDHxxx' rồi, khớp với kiểu dữ liệu
FROM [dbo].[ChiTietDonHang] CT;
GO


-- 14. CHÈN CHI TIẾT ĐƠN HÀNG
DECLARE @TempChiTiet TABLE (
    MaDonHang varchar(25),
    MaPhienBan varchar(25),
    MaCongThuc varchar(25),
    SoLuong int,
    -- THÊM CỘT MA BAN AN DON HANG ĐỂ TRUY VẤN
    MaBanAnDonHang varchar(25)
);

-- Sửa lại: Chuyển đổi sang sử dụng PB001 (S), PB002 (M), PB003 (L) và mã công thức tương ứng
-- Quy tắc: Sử dụng PB002 (Size M) làm mặc định cho các phiên bản không hợp lệ
INSERT INTO @TempChiTiet (MaDonHang, MaPhienBan, MaCongThuc, SoLuong) VALUES
-- DH001: Lẩu Thái M, Chả giò L, Bia Sài Gòn M
('DH001', 'PB002', 'CT006_M', 1), ('DH001', 'PB003', 'CT003_L', 2), ('DH001', 'PB002', 'CT019_M', 5),
-- DH002: Lẩu bò M, Sườn nướng M, Hạt dưa S
('DH002', 'PB002', 'CT008_M', 2), ('DH002', 'PB002', 'CT021_M', 1), ('DH002', 'PB001', 'CT001_S', 1),
-- DH003: Cơm tấm M, Coca M
('DH003', 'PB002', 'CT031_M', 4), ('DH003', 'PB002', 'CT020_M', 4),
-- DH004: Cơm gà M, Nước lọc M
('DH004', 'PB002', 'CT034_M', 2), ('DH004', 'PB002', 'CT016_M', 2),
-- DH005: Cơm bò lúc lắc M, Đậu hũ chiên sả M, Pepsi M
('DH005', 'PB002', 'CT035_M', 2), ('DH005', 'PB002', 'CT027_M', 1), ('DH005', 'PB002', 'CT018_M', 4),
-- DH006: Tôm hùm M, Cua rang me M, Sò điệp M, Bia Sài Gòn M
('DH006', 'PB002', 'CT036_M', 1), ('DH006', 'PB002', 'CT037_M', 1), ('DH006', 'PB002', 'CT040_M', 2), ('DH006', 'PB002', 'CT019_M', 10),
-- DH007: Tôm nướng M, Bia Sài Gòn M
('DH007', 'PB002', 'CT023_M', 1), ('DH007', 'PB002', 'CT019_M', 4),
-- DH008: Salad M, Bánh flan M, Nước ép M
('DH008', 'PB002', 'CT005_M', 1), ('DH008', 'PB002', 'CT011_M', 2), ('DH008', 'PB002', 'CT017_M', 2),
-- DH009: Lẩu gà M, Ba chỉ bò M, Đậu phộng S
('DH009', 'PB002', 'CT007_M', 1), ('DH009', 'PB002', 'CT024_M', 2), ('DH009', 'PB001', 'CT002_S', 1),
-- DH010: Lẩu cá hồi M, Lẩu tôm M, Hàu nướng M
('DH010', 'PB002', 'CT009_M', 1), ('DH010', 'PB002', 'CT010_M', 1), ('DH010', 'PB002', 'CT022_M', 2),
-- DH011: Gà nướng M, Pepsi M
('DH011', 'PB002', 'CT025_M', 2), ('DH011', 'PB002', 'CT018_M', 4),
-- DH012: Cơm chay M, Canh chua chay M
('DH012', 'PB002', 'CT026_M', 3), ('DH012', 'PB002', 'CT028_M', 1),
-- DH013: Mì xào chay M, Nấm hấp M, Nước lọc M
('DH013', 'PB002', 'CT029_M', 2), ('DH013', 'PB002', 'CT030_M', 2), ('DH013', 'PB002', 'CT016_M', 6),
-- DH014: Cơm tấm M, Cơm chiên M, Coca M
('DH014', 'PB002', 'CT031_M', 1), ('DH014', 'PB002', 'CT032_M', 1), ('DH014', 'PB002', 'CT020_M', 2),
-- DH015: Cơm rang hải sản M, Cơm gà M
('DH015', 'PB002', 'CT033_M', 1), ('DH015', 'PB002', 'CT034_M', 1),
-- DH016: Lẩu Thái M, Lẩu bò M, Sườn nướng M, Bia Sài Gòn M
('DH016', 'PB002', 'CT006_M', 3), ('DH016', 'PB002', 'CT008_M', 3), ('DH016', 'PB002', 'CT021_M', 5), ('DH016', 'PB002', 'CT019_M', 20),
-- DH017: Ghẹ hấp M, Mực chiên M, Sò điệp M
('DH017', 'PB002', 'CT038_M', 2), ('DH017', 'PB002', 'CT039_M', 3), ('DH017', 'PB002', 'CT040_M', 3),
-- DH018: Cơm tấm M
('DH018', 'PB002', 'CT031_M', 1),
-- DH019: Cơm bò lúc lắc M, Nước lọc M
('DH019', 'PB002', 'CT035_M', 3), ('DH019', 'PB002', 'CT016_M', 3),
-- DH020: Lẩu Thái M, Ba chỉ bò M, Chả giò L
('DH020', 'PB002', 'CT006_M', 1), ('DH020', 'PB002', 'CT024_M', 2), ('DH020', 'PB003', 'CT003_L', 2),
-- DH021: Lẩu gà M, Hạt dưa S, Bia Sài Gòn M
('DH021', 'PB002', 'CT007_M', 1), ('DH021', 'PB001', 'CT001_S', 1), ('DH021', 'PB002', 'CT019_M', 6),
-- DH022: Cơm tấm M, Cơm gà M, Cơm chiên M
('DH022', 'PB002', 'CT031_M', 2), ('DH022', 'PB002', 'CT034_M', 2), ('DH022', 'PB002', 'CT032_M', 2),
-- DH023: Cơm bò lúc lắc M, Coca M
('DH023', 'PB002', 'CT035_M', 5), ('DH023', 'PB002', 'CT020_M', 5),
-- DH024: Lẩu cá hồi M, Chả giò L
('DH024', 'PB002', 'CT009_M', 1), ('DH024', 'PB003', 'CT003_L', 2),
-- DH025: Tôm hùm M, Cua rang me M
('DH025', 'PB002', 'CT036_M', 1), ('DH025', 'PB002', 'CT037_M', 2),
-- DH026: Hàu nướng M, Sườn nướng M, Pepsi M
('DH026', 'PB002', 'CT022_M', 3), ('DH026', 'PB002', 'CT021_M', 2), ('DH026', 'PB002', 'CT018_M', 7),
-- DH027: Sò điệp M, Mực chiên M, Bia Sài Gòn M
('DH027', 'PB002', 'CT040_M', 4), ('DH027', 'PB002', 'CT039_M', 2), ('DH027', 'PB002', 'CT019_M', 10),
-- DH028: Cơm tấm M, Nước lọc M
('DH028', 'PB002', 'CT031_M', 9), ('DH028', 'PB002', 'CT016_M', 9),
-- DH029: Nấm hấp M, Coca M
('DH029', 'PB002', 'CT030_M', 10), ('DH029', 'PB002', 'CT020_M', 10),
-- DH030: Tôm nướng M, Hạt dưa S, Bia Sài Gòn M
('DH030', 'PB002', 'CT023_M', 1), ('DH030', 'PB001', 'CT001_S', 1), ('DH030', 'PB002', 'CT019_M', 2),
-- DH031: Ba chỉ bò M, Pepsi M
('DH031', 'PB002', 'CT024_M', 2), ('DH031', 'PB002', 'CT018_M', 4),
-- DH032: Salad M, Bánh flan M
('DH032', 'PB002', 'CT005_M', 1), ('DH032', 'PB002', 'CT011_M', 2),
-- DH033: Chè hạt sen M, Sữa chua nếp cẩm M
('DH033', 'PB002', 'CT012_M', 1), ('DH033', 'PB002', 'CT013_M', 1),
-- DH034: Cơm chiên M, Cơm rang hải sản M, Nước lọc M
('DH034', 'PB002', 'CT032_M', 5), ('DH034', 'PB002', 'CT033_M', 5), ('DH034', 'PB002', 'CT016_M', 10),
-- DH035: Cơm tấm M, Coca M
('DH035', 'PB002', 'CT031_M', 11), ('DH035', 'PB002', 'CT020_M', 11),
-- DH036: Lẩu Thái M, Lẩu gà M, Bia Sài Gòn M
('DH036', 'PB002', 'CT006_M', 1), ('DH036', 'PB002', 'CT007_M', 1), ('DH036', 'PB002', 'CT019_M', 8),
-- DH037: Lẩu bò M, Sườn nướng M, Pepsi M
('DH037', 'PB002', 'CT008_M', 2), ('DH037', 'PB002', 'CT021_M', 3), ('DH037', 'PB002', 'CT018_M', 8),
-- DH038: Cơm gà M, Nước lọc M
('DH038', 'PB002', 'CT034_M', 3), ('DH038', 'PB002', 'CT016_M', 3),
-- DH039: Cơm bò lúc lắc M, Mực chiên M, Coca M
('DH039', 'PB002', 'CT035_M', 2), ('DH039', 'PB002', 'CT039_M', 1), ('DH039', 'PB002', 'CT020_M', 4),
-- DH040: Lẩu Thái M, Hàu nướng M, Bia Sài Gòn M
('DH040', 'PB002', 'CT006_M', 1), ('DH040', 'PB002', 'CT022_M', 2), ('DH040', 'PB002', 'CT019_M', 5);

-- A. Insert ChiTietDonHang (Tự sinh mã chuỗi CTDHxxx) - Sửa lại để khớp với dữ liệu mới
INSERT INTO [dbo].[ChiTietDonHang] ([MaChiTietDonHang], [MaDonHang], [MaPhienBan], [MaCongThuc], [SoLuong])
SELECT 
    'CTDH' + RIGHT('000' + CAST(ROW_NUMBER() OVER(ORDER BY (SELECT 1)) AS VARCHAR), 3),
    T.MaDonHang, T.MaPhienBan, T.MaCongThuc, T.SoLuong
FROM (VALUES
-- DH001: Lẩu Thái M, Chả giò L, Bia Sài Gòn M
('DH001', 'PB002', 'CT006_M', 1), ('DH001', 'PB003', 'CT003_L', 2), ('DH001', 'PB002', 'CT019_M', 5),
-- DH002: Lẩu bò M, Sườn nướng M, Hạt dưa S
('DH002', 'PB002', 'CT008_M', 2), ('DH002', 'PB002', 'CT021_M', 1), ('DH002', 'PB001', 'CT001_S', 1),
-- DH003: Cơm tấm M, Coca M
('DH003', 'PB002', 'CT031_M', 4), ('DH003', 'PB002', 'CT020_M', 4),
-- DH004: Cơm gà M, Nước lọc M
('DH004', 'PB002', 'CT034_M', 2), ('DH004', 'PB002', 'CT016_M', 2),
-- DH005: Cơm bò lúc lắc M, Đậu hũ chiên sả M, Pepsi M
('DH005', 'PB002', 'CT035_M', 2), ('DH005', 'PB002', 'CT027_M', 1), ('DH005', 'PB002', 'CT018_M', 4),
-- DH006: Tôm hùm M, Cua rang me M, Sò điệp M, Bia Sài Gòn M
('DH006', 'PB002', 'CT036_M', 1), ('DH006', 'PB002', 'CT037_M', 1), ('DH006', 'PB002', 'CT040_M', 2), ('DH006', 'PB002', 'CT019_M', 10),
-- DH007: Tôm nướng M, Bia Sài Gòn M
('DH007', 'PB002', 'CT023_M', 1), ('DH007', 'PB002', 'CT019_M', 4),
-- DH008: Salad M, Bánh flan M, Nước ép M
('DH008', 'PB002', 'CT005_M', 1), ('DH008', 'PB002', 'CT011_M', 2), ('DH008', 'PB002', 'CT017_M', 2),
-- DH009: Lẩu gà M, Ba chỉ bò M, Đậu phộng S
('DH009', 'PB002', 'CT007_M', 1), ('DH009', 'PB002', 'CT024_M', 2), ('DH009', 'PB001', 'CT002_S', 1),
-- DH010: Lẩu cá hồi M, Lẩu tôm M, Hàu nướng M
('DH010', 'PB002', 'CT009_M', 1), ('DH010', 'PB002', 'CT010_M', 1), ('DH010', 'PB002', 'CT022_M', 2),
-- DH011: Gà nướng M, Pepsi M
('DH011', 'PB002', 'CT025_M', 2), ('DH011', 'PB002', 'CT018_M', 4),
-- DH012: Cơm chay M, Canh chua chay M
('DH012', 'PB002', 'CT026_M', 3), ('DH012', 'PB002', 'CT028_M', 1),
-- DH013: Mì xào chay M, Nấm hấp M, Nước lọc M
('DH013', 'PB002', 'CT029_M', 2), ('DH013', 'PB002', 'CT030_M', 2), ('DH013', 'PB002', 'CT016_M', 6),
-- DH014: Cơm tấm M, Cơm chiên M, Coca M
('DH014', 'PB002', 'CT031_M', 1), ('DH014', 'PB002', 'CT032_M', 1), ('DH014', 'PB002', 'CT020_M', 2),
-- DH015: Cơm rang hải sản M, Cơm gà M
('DH015', 'PB002', 'CT033_M', 1), ('DH015', 'PB002', 'CT034_M', 1),
-- DH016: Lẩu Thái M, Lẩu bò M, Sườn nướng M, Bia Sài Gòn M
('DH016', 'PB002', 'CT006_M', 3), ('DH016', 'PB002', 'CT008_M', 3), ('DH016', 'PB002', 'CT021_M', 5), ('DH016', 'PB002', 'CT019_M', 20),
-- DH017: Ghẹ hấp M, Mực chiên M, Sò điệp M
('DH017', 'PB002', 'CT038_M', 2), ('DH017', 'PB002', 'CT039_M', 3), ('DH017', 'PB002', 'CT040_M', 3),
-- DH018: Cơm tấm M
('DH018', 'PB002', 'CT031_M', 1),
-- DH019: Cơm bò lúc lắc M, Nước lọc M
('DH019', 'PB002', 'CT035_M', 3), ('DH019', 'PB002', 'CT016_M', 3),
-- DH020: Lẩu Thái M, Ba chỉ bò M, Chả giò L
('DH020', 'PB002', 'CT006_M', 1), ('DH020', 'PB002', 'CT024_M', 2), ('DH020', 'PB003', 'CT003_L', 2),
-- DH021: Lẩu gà M, Hạt dưa S, Bia Sài Gòn M
('DH021', 'PB002', 'CT007_M', 1), ('DH021', 'PB001', 'CT001_S', 1), ('DH021', 'PB002', 'CT019_M', 6),
-- DH022: Cơm tấm M, Cơm gà M, Cơm chiên M
('DH022', 'PB002', 'CT031_M', 2), ('DH022', 'PB002', 'CT034_M', 2), ('DH022', 'PB002', 'CT032_M', 2),
-- DH023: Cơm bò lúc lắc M, Coca M
('DH023', 'PB002', 'CT035_M', 5), ('DH023', 'PB002', 'CT020_M', 5),
-- DH024: Lẩu cá hồi M, Chả giò L
('DH024', 'PB002', 'CT009_M', 1), ('DH024', 'PB003', 'CT003_L', 2),
-- DH025: Tôm hùm M, Cua rang me M
('DH025', 'PB002', 'CT036_M', 1), ('DH025', 'PB002', 'CT037_M', 2),
-- DH026: Hàu nướng M, Sườn nướng M, Pepsi M
('DH026', 'PB002', 'CT022_M', 3), ('DH026', 'PB002', 'CT021_M', 2), ('DH026', 'PB002', 'CT018_M', 7),
-- DH027: Sò điệp M, Mực chiên M, Bia Sài Gòn M
('DH027', 'PB002', 'CT040_M', 4), ('DH027', 'PB002', 'CT039_M', 2), ('DH027', 'PB002', 'CT019_M', 10),
-- DH028: Cơm tấm M, Nước lọc M
('DH028', 'PB002', 'CT031_M', 9), ('DH028', 'PB002', 'CT016_M', 9),
-- DH029: Nấm hấp M, Coca M
('DH029', 'PB002', 'CT030_M', 10), ('DH029', 'PB002', 'CT020_M', 10),
-- DH030: Tôm nướng M, Hạt dưa S, Bia Sài Gòn M
('DH030', 'PB002', 'CT023_M', 1), ('DH030', 'PB001', 'CT001_S', 1), ('DH030', 'PB002', 'CT019_M', 2),
-- DH031: Ba chỉ bò M, Pepsi M
('DH031', 'PB002', 'CT024_M', 2), ('DH031', 'PB002', 'CT018_M', 4),
-- DH032: Salad M, Bánh flan M
('DH032', 'PB002', 'CT005_M', 1), ('DH032', 'PB002', 'CT011_M', 2),
-- DH033: Chè hạt sen M, Sữa chua nếp cẩm M
('DH033', 'PB002', 'CT012_M', 1), ('DH033', 'PB002', 'CT013_M', 1),
-- DH034: Cơm chiên M, Cơm rang hải sản M, Nước lọc M
('DH034', 'PB002', 'CT032_M', 5), ('DH034', 'PB002', 'CT033_M', 5), ('DH034', 'PB002', 'CT016_M', 10),
-- DH035: Cơm tấm M, Coca M
('DH035', 'PB002', 'CT031_M', 11), ('DH035', 'PB002', 'CT020_M', 11),
-- DH036: Lẩu Thái M, Lẩu gà M, Bia Sài Gòn M
('DH036', 'PB002', 'CT006_M', 1), ('DH036', 'PB002', 'CT007_M', 1), ('DH036', 'PB002', 'CT019_M', 8),
-- DH037: Lẩu bò M, Sườn nướng M, Pepsi M
('DH037', 'PB002', 'CT008_M', 2), ('DH037', 'PB002', 'CT021_M', 3), ('DH037', 'PB002', 'CT018_M', 8),
-- DH038: Cơm gà M, Nước lọc M
('DH038', 'PB002', 'CT034_M', 3), ('DH038', 'PB002', 'CT016_M', 3),
-- DH039: Cơm bò lúc lắc M, Mực chiên M, Coca M
('DH039', 'PB002', 'CT035_M', 2), ('DH039', 'PB002', 'CT039_M', 1), ('DH039', 'PB002', 'CT020_M', 4),
-- DH040: Lẩu Thái M, Hàu nướng M, Bia Sài Gòn M
('DH040', 'PB002', 'CT006_M', 1), ('DH040', 'PB002', 'CT022_M', 2), ('DH040', 'PB002', 'CT019_M', 5)
) AS T(MaDonHang, MaPhienBan, MaCongThuc, SoLuong);
GO

-- =============================================
-- 15. TẠO STORED PROCEDURES
-- =============================================

-- 1. DOANH THU THEO THÁNG (Procedure này OK)
CREATE OR ALTER PROCEDURE [dbo].[GetDoanhThuTheoThang]
    @Nam INT
AS
BEGIN
    ;WITH Thang AS (
        SELECT 1 AS Thang UNION ALL SELECT 2 UNION ALL SELECT 3 UNION ALL
        SELECT 4 UNION ALL SELECT 5 UNION ALL SELECT 6 UNION ALL
        SELECT 7 UNION ALL SELECT 8 UNION ALL SELECT 9 UNION ALL
        SELECT 10 UNION ALL SELECT 11 UNION ALL SELECT 12
    )
    SELECT
        T.Thang,
        -- Sử dụng logic tính doanh thu đơn giản và trực tiếp
        COALESCE(SUM(CTDH.SoLuong * CTA.Gia), 0) AS DoanhThu
    FROM Thang T
    LEFT JOIN DonHang DH ON MONTH(DH.ThoiGianKetThuc) = T.Thang
                        AND YEAR(DH.ThoiGianKetThuc) = @Nam
                        AND DH.MaTrangThaiDonHang = 'DA_HOAN_THANH'
                        
    -- JOIN TRỰC TIẾP VỚI CHI TIẾT ĐƠN HÀNG (đã có MaDonHang - Varchar)
    LEFT JOIN ChiTietDonHang CTDH ON DH.MaDonHang = CTDH.MaDonHang
    -- JOIN tới Công thức nấu ăn để lấy giá
    LEFT JOIN CongThucNauAn CTA ON CTDH.MaCongThuc = CTA.MaCongThuc
    GROUP BY T.Thang
    ORDER BY T.Thang;
END;
GO

-- 2. LẤY HÓA ĐƠN (ĐÃ SỬA LỖI LOGIC TÌM BÀN)
CREATE OR ALTER PROCEDURE [dbo].[LayHoaDon]
    @MaDonHang VARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;

    -- Tính tổng tiền trước khi trả về
    DECLARE @TongThanhTien DECIMAL(18, 2);
    
    SELECT @TongThanhTien = ISNULL(SUM(ctdh.SoLuong * cta.Gia), 0)
    FROM DonHang dh_temp
    JOIN ChiTietDonHang ctdh ON ctdh.MaDonHang = dh_temp.MaDonHang
    JOIN CongThucNauAn cta ON ctdh.MaCongThuc = cta.MaCongThuc
    WHERE dh_temp.MaDonHang = @MaDonHang;

    SELECT
        dh.MaDonHang,
        nv.HoTen AS 'TenNhanVien',
        kh.HoTen AS 'TenKhachHang',
        dh.TGNhanBan,
        ttdh.TenTrangThai AS 'TrangThaiDonHang',
        dh.TienDatCoc,
        -- THÊM CÁC TRƯỜNG TỔNG TIỀN VÀ GIẢM GIÁ
        @TongThanhTien AS 'TongTienTruocGiam',
        dh.TienGiamGia AS 'TongTienGiamGia',
        (@TongThanhTien - ISNULL(dh.TienGiamGia, 0)) AS 'TongTienCanThanhToan',

        -- [QUAN TRỌNG] ĐÃ SỬA: LẤY DANH SÁCH BÀN QUA CHI TIẾT ĐƠN HÀNG
        -- Vì bảng BanAnDonHang không còn cột MaDonHang
        (
            SELECT STRING_AGG(b.TenBan, ', ') WITHIN GROUP (ORDER BY b.TenBan)
            FROM BanAnDonHang badh
            JOIN BanAn b ON badh.MaBan = b.MaBan
            -- Join bắc cầu qua ChiTietDonHang (Khóa BIGINT)
            JOIN ChiTietDonHang ctdh_b ON badh.MaChiTietDonHang = ctdh_b.MaChiTietDonHang
            -- So sánh MaDonHang tại bảng ChiTietDonHang
            WHERE ctdh_b.MaDonHang = dh.MaDonHang
        ) AS 'DanhSachBan',

        -- Thông tin món ăn (Chi tiết từng dòng)
        ctdh.SoLuong,
        ma.TenMonAn,
        ctma.TenCT AS 'TenChiTietMonAn',
        pb.TenPhienBan,
        cta.Gia,
        (ctdh.SoLuong * cta.Gia) AS 'ThanhTien'

    FROM DonHang dh
    JOIN ChiTietDonHang ctdh ON ctdh.MaDonHang = dh.MaDonHang
    JOIN CongThucNauAn cta ON ctdh.MaCongThuc = cta.MaCongThuc
    JOIN ChiTietMonAn ctma ON cta.MaCT = ctma.MaCT
    JOIN MonAn ma ON ctma.MaMonAn = ma.MaMonAn
    JOIN PhienBanMonAn pb ON ctdh.MaPhienBan = pb.MaPhienBan
    JOIN KhachHang kh ON kh.MaKhachHang = dh.MaKhachHang
    LEFT JOIN NhanVien nv ON nv.MaNhanVien = dh.MaNhanVien
    LEFT JOIN TrangThaiDonHang ttdh ON dh.MaTrangThaiDonHang = ttdh.MaTrangThai
    
    WHERE dh.MaDonHang = @MaDonHang;
END;
GO

-- Stored Procedure: Lấy danh sách menu đang áp dụng
CREATE PROCEDURE [dbo].[GetMenuDangApDung]
    @MaLoaiMenu VARCHAR(25) = NULL -- NULL = lấy tất cả loại
AS
BEGIN
    SELECT 
        m.[MaMenu],
        m.[TenMenu],
        lm.[TenLoaiMenu],
        m.[GiaMenu],
        m.[GiaGoc],
        CASE 
            WHEN m.[GiaGoc] > 0 THEN CAST(((m.[GiaGoc] - m.[GiaMenu]) * 100.0 / m.[GiaGoc]) AS DECIMAL(5,2))
            ELSE 0
        END AS [PhanTramGiamGia],
        m.[MoTa],
        m.[HinhAnh],
        m.[NgayBatDau],
        m.[NgayKetThuc],
        m.[ThuTu]
    FROM [dbo].[Menu] m
    INNER JOIN [dbo].[LoaiMenu] lm ON m.[MaLoaiMenu] = lm.[MaLoaiMenu]
    WHERE m.[MaTrangThai] = 'DANG_AP_DUNG'
        AND m.[IsShow] = 1
        AND (m.[NgayBatDau] IS NULL OR m.[NgayBatDau] <= GETDATE())
        AND (m.[NgayKetThuc] IS NULL OR m.[NgayKetThuc] >= GETDATE())
        AND (@MaLoaiMenu IS NULL OR m.[MaLoaiMenu] = @MaLoaiMenu)
    ORDER BY m.[ThuTu] ASC, m.[TenMenu] ASC;
END;
GO

-- Stored Procedure: Lấy chi tiết menu
CREATE PROCEDURE [dbo].[GetChiTietMenu]
    @MaMenu VARCHAR(25)
AS
BEGIN
    -- Thông tin menu
    SELECT 
        m.[MaMenu],
        m.[TenMenu],
        lm.[TenLoaiMenu],
        ttm.[TenTrangThai],
        m.[GiaMenu],
        m.[GiaGoc],
        m.[MoTa],
        m.[HinhAnh],
        m.[NgayBatDau],
        m.[NgayKetThuc]
    FROM [dbo].[Menu] m
    INNER JOIN [dbo].[LoaiMenu] lm ON m.[MaLoaiMenu] = lm.[MaLoaiMenu]
    INNER JOIN [dbo].[TrangThaiMenu] ttm ON m.[MaTrangThai] = ttm.[MaTrangThai]
    WHERE m.[MaMenu] = @MaMenu;

    -- Chi tiết các món trong menu
    SELECT 
        ctm.[MaChiTietMenu],
        ctm.[SoLuong],
        ctm.[GhiChu],
        ctm.[ThuTu],
        ma.[TenMonAn],
        ctma.[TenCT] AS [TenChiTietMonAn],
        pb.[TenPhienBan],
        cta.[Gia] AS [GiaGoc],
        (ctm.[SoLuong] * cta.[Gia]) AS [ThanhTien]
    FROM [dbo].[ChiTietMenu] ctm
    INNER JOIN [dbo].[CongThucNauAn] cta ON ctm.[MaCongThuc] = cta.[MaCongThuc]
    INNER JOIN [dbo].[ChiTietMonAn] ctma ON cta.[MaCT] = ctma.[MaCT]
    INNER JOIN [dbo].[MonAn] ma ON ctma.[MaMonAn] = ma.[MaMonAn]
    INNER JOIN [dbo].[PhienBanMonAn] pb ON cta.[MaPhienBan] = pb.[MaPhienBan]
    WHERE ctm.[MaMenu] = @MaMenu
    ORDER BY ctm.[ThuTu] ASC;
END;
GO

-- 3. DASHBOARD STATS (Procedure này OK)
CREATE OR ALTER PROCEDURE [dbo].[GetDashboardStats]
    @TimeRange VARCHAR(20) -- 'TODAY', 'WEEK', 'MONTH'
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @StartDate DATETIME;
    DECLARE @EndDate DATETIME = GETDATE();

    -- 1. Xác định thời gian
    IF @TimeRange = 'TODAY' SET @StartDate = CAST(CAST(GETDATE() AS DATE) AS DATETIME);
    ELSE IF @TimeRange = 'WEEK' SET @StartDate = DATEADD(wk, DATEDIFF(wk, 0, GETDATE()), 0);
    ELSE IF @TimeRange = 'MONTH' SET @StartDate = DATEFROMPARTS(YEAR(GETDATE()), MONTH(GETDATE()), 1); 
    ELSE SET @StartDate = CAST(CAST(GETDATE() AS DATE) AS DATETIME);

    DECLARE @TongDoanhThu DECIMAL(18, 2) = 0;
    DECLARE @SoDonHoanThanh INT = 0;
    DECLARE @SoBanDangPhucVu INT = 0;
    DECLARE @TongKhachHang INT = 0;

    -- 2. TÍNH TỔNG DOANH THU
    SELECT @TongDoanhThu = ISNULL(SUM(CTDH.SoLuong * CTA.Gia), 0)
    FROM DonHang DH
    JOIN ChiTietDonHang CTDH ON DH.MaDonHang = CTDH.MaDonHang
    JOIN CongThucNauAn CTA ON CTDH.MaCongThuc = CTA.MaCongThuc
    WHERE DH.MaTrangThaiDonHang = 'DA_HOAN_THANH'
      AND DH.ThoiGianKetThuc >= @StartDate 
      AND DH.ThoiGianKetThuc <= @EndDate;

    -- 3. Các chỉ số khác
    SELECT @SoDonHoanThanh = COUNT(*)
    FROM DonHang
    WHERE MaTrangThaiDonHang = 'DA_HOAN_THANH'
      AND ThoiGianKetThuc >= @StartDate 
      AND ThoiGianKetThuc <= @EndDate;

    SELECT @TongKhachHang = ISNULL(SUM(SoLuongNguoiDK), 0)
    FROM DonHang
    WHERE MaTrangThaiDonHang = 'DA_HOAN_THANH' 
      AND (ThoiGianKetThuc >= @StartDate AND ThoiGianKetThuc <= @EndDate OR ThoiGianDatHang >= @StartDate);

    SELECT @SoBanDangPhucVu = COUNT(*)
    FROM BanAn
    WHERE MaTrangThai = 'TTBA002'; -- Đang phục vụ

    -- 4. Trả về kết quả
    SELECT 
        @TongDoanhThu AS TongDoanhThu,
        @SoDonHoanThanh AS SoDonHoanThanh,
        @SoBanDangPhucVu AS SoBanPhucVu,
        @TongKhachHang AS TongKhachHang;
END
GO

-- 4. INVENTORY STATS (Procedure này OK)
CREATE OR ALTER PROCEDURE [dbo].[GetInventoryStats]
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @MinStock INT = 10;
    
    DECLARE @ItemsNeedAlert INT = 0;
    DECLARE @DraftReceipts INT = 0;
    DECLARE @TotalInventoryValue DECIMAL(18, 2) = 0;

    -- 1. Đếm số lượng Nguyên liệu cần cảnh báo
    SELECT @ItemsNeedAlert = COUNT(MaNguyenLieu)
    FROM [dbo].[NguyenLieu]
    WHERE SoLuongTonKho < @MinStock;

    -- 2. Đếm số lượng Phiếu nhập đang là Bản nháp
    SELECT @DraftReceipts = COUNT(MaNhapHang)
    FROM [dbo].[NhapHang]
    WHERE MaTrangThai = 'MOI_TAO';

    -- 3. Tính tổng giá trị Tồn kho
    SELECT @TotalInventoryValue = ISNULL(SUM(CAST(SoLuongTonKho AS DECIMAL(10, 2)) * GiaBan), 0)
    FROM [dbo].[NguyenLieu]
    WHERE SoLuongTonKho > 0;

    -- 4. Trả về kết quả
    SELECT
        @ItemsNeedAlert AS SoLuongCanCanhBao,
        @DraftReceipts AS SoPhieuNhapNhap,
        @TotalInventoryValue AS TongGiaTriTonKho
END;
GO

-- =============================================
-- 16. TẠO TRIGGERS
-- =============================================

-- Trigger tăng NoShowCount
CREATE OR ALTER TRIGGER [dbo].[trg_OnDonHangUpdate_IncrementNoShow]
ON [dbo].[DonHang]
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    IF NOT UPDATE(MaTrangThaiDonHang) RETURN;

    UPDATE KH
    SET NoShowCount = ISNULL(KH.NoShowCount, 0) + 1
    FROM [dbo].[KhachHang] AS KH
    JOIN inserted AS i ON KH.MaKhachHang = i.MaKhachHang
    JOIN deleted AS d ON i.MaDonHang = d.MaDonHang -- Tối ưu hóa JOIN
    WHERE i.MaTrangThaiDonHang = 'NO_SHOW'
      AND ISNULL(d.MaTrangThaiDonHang, '') <> 'NO_SHOW';
END
GO

-- Trigger cập nhật giá bán nguyên liệu
CREATE OR ALTER TRIGGER [dbo].[trg_NguyenLieu_GiaBanLonHonGiaNhap]
ON [dbo].[NguyenLieu]
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    IF UPDATE(GiaBan)
    BEGIN
        IF EXISTS (
            SELECT 1 
            FROM inserted i
            JOIN (
                SELECT MaNguyenLieu, MAX(GiaNhap) AS MaxGiaNhap
                FROM [dbo].[ChiTietNhapHang]
                GROUP BY MaNguyenLieu
            ) MaxNhap ON i.MaNguyenLieu = MaxNhap.MaNguyenLieu
            WHERE i.GiaBan <= MaxNhap.MaxGiaNhap
        )
        BEGIN
            RAISERROR (N'Lỗi: Giá Bán phải lớn hơn Giá Nhập cao nhất đã có trong Chi Tiết Nhập Hàng.', 16, 1);
            ROLLBACK TRANSACTION;
            RETURN;
        END
    END
END
GO

CREATE OR ALTER TRIGGER [dbo].[trg_NhapHang_CapNhatTonKho]
ON [dbo].[NhapHang]
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    IF UPDATE(MaTrangThai)
    BEGIN
        -- 1. Cập nhật Số lượng tồn kho (Giữ nguyên)
        UPDATE NL
        SET SoLuongTonKho = ISNULL(NL.SoLuongTonKho, 0) + CTNH.SoLuong
        FROM [dbo].[NguyenLieu] NL
        JOIN [dbo].[ChiTietNhapHang] CTNH ON NL.MaNguyenLieu = CTNH.MaNguyenLieu
        JOIN inserted i ON CTNH.MaNhapHang = i.MaNhapHang
        JOIN deleted d ON i.MaNhapHang = d.MaNhapHang
        WHERE i.MaTrangThai = 'DA_HOAN_TAT' 
          AND d.MaTrangThai <> 'DA_HOAN_TAT';

        -- 2. CẬP NHẬT TRẠNG THÁI TỒN KHO DỰA TRÊN SỐ LƯỢNG MỚI (BỔ SUNG)
        UPDATE NL
        SET TrangThaiTonKho = CASE 
                                   WHEN NL.SoLuongTonKho = 0 THEN 'HET_HANG'
                                   WHEN NL.SoLuongTonKho < 10 THEN 'CAN_CANH_BAO'
                                   ELSE 'BINH_THUONG'
                               END
        FROM [dbo].[NguyenLieu] NL
        JOIN [dbo].[ChiTietNhapHang] CTNH ON NL.MaNguyenLieu = CTNH.MaNguyenLieu
        JOIN inserted i ON CTNH.MaNhapHang = i.MaNhapHang
        JOIN deleted d ON i.MaNhapHang = d.MaNhapHang
        WHERE i.MaTrangThai = 'DA_HOAN_TAT' AND d.MaTrangThai <> 'DA_HOAN_TAT';
    END
END
GO


-- Trigger giảm tồn kho khi đơn hàng hoàn tất/thanh toán (ĐÃ SỬA LỖI JOIN)
CREATE OR ALTER TRIGGER [dbo].[trg_DonHang_CapNhatTonKhoGiam]
ON [dbo].[DonHang]
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;

    -- Kiểm tra nếu có cập nhật trạng thái đơn hàng
    IF UPDATE(MaTrangThaiDonHang)
    BEGIN
        -- 1. Xác định các chi tiết đơn hàng cần trừ tồn kho
        SELECT CTDH.MaCongThuc, CTDH.SoLuong
        INTO #DonHangHoanTat
        FROM inserted i
        JOIN deleted d ON i.MaDonHang = d.MaDonHang
        JOIN [dbo].[ChiTietDonHang] CTDH ON i.MaDonHang = CTDH.MaDonHang 
        WHERE i.MaTrangThaiDonHang = 'DA_HOAN_THANH' 
          AND d.MaTrangThaiDonHang <> 'DA_HOAN_THANH'; 

        -- 2. Trừ tồn kho và CẬP NHẬT TRẠNG THÁI TỒN KHO TRONG CÙNG UPDATE
        UPDATE NL
        SET SoLuongTonKho = NL.SoLuongTonKho - ISNULL(T.SoLuongTieuThu, 0),
            -- Cập nhật trạng thái dựa trên SỐ LƯỢNG MỚI (NL.SoLuongTonKho - T.SoLuongTieuThu)
            TrangThaiTonKho = CASE 
                                WHEN (NL.SoLuongTonKho - ISNULL(T.SoLuongTieuThu, 0)) = 0 THEN 'HET_HANG'
                                WHEN (NL.SoLuongTonKho - ISNULL(T.SoLuongTieuThu, 0)) < 10 THEN 'CAN_CANH_BAO'
                                ELSE 'BINH_THUONG'
                              END
        FROM [dbo].[NguyenLieu] NL
        -- Sử dụng bảng tạm để tính tổng lượng tiêu thụ của từng nguyên liệu trong đơn hàng này
        JOIN (
            SELECT 
                CTCT.MaNguyenLieu, 
                SUM(DH.SoLuong * CTCT.SoLuongCanDung) AS SoLuongTieuThu
            FROM #DonHangHoanTat DH 
            JOIN [dbo].[ChiTietCongThuc] CTCT ON DH.MaCongThuc = CTCT.MaCongThuc
            GROUP BY CTCT.MaNguyenLieu
        ) AS T ON NL.MaNguyenLieu = T.MaNguyenLieu
        -- Chỉ cập nhật những nguyên liệu có liên quan đến các món vừa bán
        WHERE NL.MaNguyenLieu IN (SELECT DISTINCT MaNguyenLieu FROM ChiTietCongThuc CTCT JOIN #DonHangHoanTat DH ON CTCT.MaCongThuc = DH.MaCongThuc);
        
        DROP TABLE #DonHangHoanTat;
    END
END
GO

-- CHÈN DỮ LIỆU MẪU CHO MENU
-- Chèn dữ liệu mẫu cho Menu
INSERT INTO [dbo].[Menu] ([MaMenu], [TenMenu], [MaLoaiMenu], [MaTrangThai], [GiaMenu], [GiaGoc], [MoTa], [HinhAnh], [NgayBatDau], [NgayKetThuc], [IsShow], [ThuTu]) VALUES
('MENU001', N'Menu Set A - Cơm tấm combo', 'LM001', 'DANG_AP_DUNG', 120000, 150000, N'Bao gồm: 1 phần cơm tấm sườn bì chả + 1 canh chua chay + 1 nước lọc', 'images\menus\menu1.jpg', '2025-01-01', NULL, 1, 1),
('MENU002', N'Menu Set B - Lẩu combo 2 người', 'LM001', 'DANG_AP_DUNG', 450000, 500000, N'Bao gồm: 1 lẩu Thái hải sản + 2 phần cơm + 2 nước', 'images\menus\menu2.jpg', '2025-01-01', NULL, 1, 2),
('MENU003', N'Menu Set C - Hải sản combo', 'LM001', 'DANG_AP_DUNG', 600000, 700000, N'Bao gồm: 1 tôm hùm nướng bơ tỏi + 1 cua rang me + 2 nước', 'images\menus\menu3.jpg', '2025-01-01', NULL, 1, 3),
('MENU004', N'Menu Buffet trưa', 'LM002', 'DANG_AP_DUNG', 250000, NULL, N'Buffet trưa thứ 2-6, từ 11h-14h', 'images\menus\menu4.jpg', '2025-01-01', NULL, 1, 4),
('MENU005', N'Menu gia đình 4 người', 'LM005', 'DANG_AP_DUNG', 800000, 950000, N'Menu đầy đủ cho gia đình 4 người: 4 phần cơm + 2 món mặn + 1 canh + 4 nước', 'images\menus\menu5.jpg', '2025-01-01', NULL, 1, 5),
('MENU006', N'Menu Tết Nguyên Đán 2025', 'LM004', 'CHUA_AP_DUNG', 1200000, 1400000, N'Menu đặc biệt dịp Tết, áp dụng từ 28/12 - 5/1', 'images\menus\menu6.jpg', '2025-12-28', '2026-01-05', 1, 6);
GO

-- Chèn dữ liệu chi tiết cho Menu Set A (Sử dụng Size M làm mặc định)
INSERT INTO [dbo].[ChiTietMenu] ([MaMenu], [MaCongThuc], [SoLuong], [GhiChu], [ThuTu]) VALUES
('MENU001', 'CT031_M', 1, N'Cơm tấm sườn bì chả', 1),
('MENU001', 'CT028_M', 1, N'Canh chua chay', 2),
('MENU001', 'CT016_M', 1, N'Nước lọc', 3);
GO

-- Chèn dữ liệu chi tiết cho Menu Set B (Sử dụng Size M làm mặc định)
INSERT INTO [dbo].[ChiTietMenu] ([MaMenu], [MaCongThuc], [SoLuong], [GhiChu], [ThuTu]) VALUES
('MENU002', 'CT006_M', 1, N'Lẩu Thái hải sản', 1),
('MENU002', 'CT031_M', 2, N'Cơm tấm (2 phần)', 2),
('MENU002', 'CT016_M', 2, N'Nước lọc (2 chai)', 3);
GO

-- Chèn dữ liệu chi tiết cho Menu Set C (Sử dụng Size M làm mặc định)
INSERT INTO [dbo].[ChiTietMenu] ([MaMenu], [MaCongThuc], [SoLuong], [GhiChu], [ThuTu]) VALUES
('MENU003', 'CT036_M', 1, N'Tôm hùm nướng bơ tỏi', 1),
('MENU003', 'CT037_M', 1, N'Cua rang me', 2),
('MENU003', 'CT016_M', 2, N'Nước lọc (2 chai)', 3);
GO

-- Chèn dữ liệu chi tiết cho Menu gia đình 4 người (Sử dụng Size M làm mặc định)
INSERT INTO [dbo].[ChiTietMenu] ([MaMenu], [MaCongThuc], [SoLuong], [GhiChu], [ThuTu]) VALUES
('MENU005', 'CT031_M', 4, N'Cơm tấm (4 phần)', 1),
('MENU005', 'CT021_M', 1, N'Sườn nướng BBQ', 2),
('MENU005', 'CT024_M', 1, N'Ba chỉ bò nướng', 3),
('MENU005', 'CT028_M', 1, N'Canh chua chay', 4),
('MENU005', 'CT016_M', 4, N'Nước lọc (4 chai)', 5);
GO

-- Gán bàn vào các tầng
UPDATE [dbo].[BanAn]
SET [MaTang] = 'T001'
WHERE [MaBan] BETWEEN 'B001' AND 'B014';
GO


UPDATE [dbo].[BanAn]
SET [MaTang] = 'T002'
WHERE [MaBan] BETWEEN 'B015' AND 'B027';
GO


UPDATE [dbo].[BanAn]
SET [MaTang] = 'T003'
WHERE [MaBan] BETWEEN 'B028' AND 'B040';
GO

CREATE TABLE RefreshTokens (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Token NVARCHAR(MAX) NOT NULL,
    UserId NVARCHAR(450) NOT NULL,
    Expires DATETIME2 NOT NULL,
    IsRevoked BIT NOT NULL DEFAULT 0
);
