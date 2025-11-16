USE [master];
GO

---- Xóa DB cũ nếu tồn tại (Để chạy lại từ đầu cho sạch)
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
-- TẠO CÁC BẢNG
-- =============================================

/****** Object:  Table [dbo].[BanAn] ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[BanAn](
    [MaBan] [varchar](25) NOT NULL,
    [TenBan] [nvarchar](50) NOT NULL,
    [MaTrangThai] [varchar](25) NOT NULL,
    [SucChua] [int] NOT NULL,
PRIMARY KEY CLUSTERED 
(
    [MaBan] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

/****** Object:  Table [dbo].[CheBienMonAn] ******/
--CREATE TABLE [dbo].[CheBienMonAn](
--    [MaCheBien] [varchar](25) NOT NULL,
--    [NgayNau] [datetime] NULL,
--    [MaPhienBan] [varchar](25) NOT NULL,
--    [SoLuong] [int] NULL,
--PRIMARY KEY CLUSTERED 
--(
--    [MaCheBien] ASC
--) ON [PRIMARY]
--) ON [PRIMARY]
--GO

/****** Object:  Table [dbo].[ChiTietDonHang] ******/
CREATE TABLE [dbo].[ChiTietDonHang](
    [MaChiTietDonHang] [bigint] IDENTITY(1,1) NOT NULL,
    [MaDonHang] [varchar](25) NOT NULL,
    [MaPhienBan] [varchar](25) NOT NULL,
    [MaCongThuc] [varchar](25) NOT NULL,
    [SoLuong] [int] NOT NULL,
PRIMARY KEY CLUSTERED 
(
    [MaChiTietDonHang] ASC
) ON [PRIMARY]
) ON [PRIMARY]
GO

/****** Object:  Table [dbo].[ChiTietNhapHang] ******/
CREATE TABLE [dbo].[ChiTietNhapHang](
    [MaChiTietNhapHang] [bigint] IDENTITY(1,1) NOT NULL,
    [MaNhapHang] [varchar](25) NOT NULL,
    [MaCungUng] [varchar](25) NOT NULL,
    [SoLuong] [int] NOT NULL,
    [GiaNhap] [decimal](10, 2) NOT NULL,
PRIMARY KEY CLUSTERED 
(
    [MaChiTietNhapHang] ASC
) ON [PRIMARY]
) ON [PRIMARY]
GO

/****** Object:  Table [dbo].[ChiTietMonAn] ******/
CREATE TABLE [dbo].[ChiTietMonAn](
    [MaCT] [varchar](25) NOT NULL,
    [TenCT] [nvarchar](100) NOT NULL,
    [MaMonAn] [varchar](25) NOT NULL,
PRIMARY KEY CLUSTERED 
(
    [MaCT] ASC
) ON [PRIMARY]
) ON [PRIMARY]
GO

/****** Object:  Table [dbo].[CongThucNauAn] ******/
CREATE TABLE [dbo].[CongThucNauAn](
    [MaCongThuc] [varchar](25) NOT NULL,
    [MaCT] [varchar](25) NOT NULL,
    [MaPhienBan] [varchar](25) NOT NULL,
    [Gia] [decimal](10, 2) NOT NULL,
PRIMARY KEY CLUSTERED 
(
    [MaCongThuc] ASC
) ON [PRIMARY]
) ON [PRIMARY]
GO

/****** Object:  Table [dbo].[ChiTietCongThuc] ******/
CREATE TABLE [dbo].[ChiTietCongThuc](
    [MaChiTietCongThuc] [bigint] IDENTITY(1,1) NOT NULL,
    [MaCongThuc] [varchar](25) NOT NULL,
    [MaNguyenLieu] [varchar](25) NOT NULL,
    [SoLuongCanDung] [int] NOT NULL,
PRIMARY KEY CLUSTERED 
(
    [MaChiTietCongThuc] ASC
) ON [PRIMARY]
) ON [PRIMARY]
GO

/****** Object:  Table [dbo].[CungUng] ******/
CREATE TABLE [dbo].[CungUng](
    [MaCungUng] [varchar](25) NOT NULL,
    [MaNguyenLieu] [varchar](25) NULL,
    [MaNhaCungCap] [varchar](25) NULL,
PRIMARY KEY CLUSTERED 
(
    [MaCungUng] ASC
) ON [PRIMARY]
) ON [PRIMARY]
GO

/****** Object:  Table [dbo].[DanhMucMonAn] ******/
CREATE TABLE [dbo].[DanhMucMonAn](
    [MaDanhMuc] [varchar](25) NOT NULL,
    [TenDanhMuc] [nvarchar](255) NOT NULL,
PRIMARY KEY CLUSTERED 
(
    [MaDanhMuc] ASC
) ON [PRIMARY]
) ON [PRIMARY]
GO

/****** Object:  Table [dbo].[DonHang] ******/
CREATE TABLE [dbo].[DonHang](
    [MaDonHang] [varchar](25) NOT NULL,
    --[MaBan] [varchar](25) NOT NULL,
    [MaNhanVien] [varchar](25) NULL,
    [MaKhachHang] [varchar](25) NOT NULL,
    [MaTrangThaiDonHang] [varchar](25) NOT NULL,
    [ThoiGianDatHang] [datetime] NULL,
    [TGDatDuKien] [datetime] NULL,
    [TGNhanBan] [datetime] NULL,
    [ThanhToan] [bit] NOT NULL DEFAULT(0),
    [ThoiGianKetThuc] [datetime] NULL,
    [SoLuongNguoiDK] [int] NOT NULL,
    [TienDatCoc] [decimal](10, 2) NULL,
    [GhiChu] [nvarchar](500) NULL,
PRIMARY KEY CLUSTERED 
(
    [MaDonHang] ASC
) ON [PRIMARY]
) ON [PRIMARY]
GO

/****** Object:  Table [dbo].[HinhAnhMonAn] ******/
CREATE TABLE [dbo].[HinhAnhMonAn](
    [Id] [int] IDENTITY(1,1) NOT NULL,
    [MaMonAn] [varchar](25) NOT NULL,
    [URLHinhAnh] [nvarchar](max) NOT NULL,
PRIMARY KEY CLUSTERED 
(
    [Id] ASC
) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

/****** Object:  Table [dbo].[KhachHang] (ĐÃ SỬA LOGIC UNIQUE) ******/
CREATE TABLE [dbo].[KhachHang](
    [MaKhachHang] [varchar](25) NOT NULL,
    [HoTen] [nvarchar](100) NOT NULL,
    [SoDienThoai] [nvarchar](15) NOT NULL,
    [Email] [nvarchar](100) NULL, -- Cho phép NULL
    [HinhAnh] [nvarchar](max) NULL,
    [NoShowCount] [int] NULL,
PRIMARY KEY CLUSTERED 
(
    [MaKhachHang] ASC
) ON [PRIMARY]
-- Lưu ý: Đã xóa dòng UNIQUE Email ở đây để tạo Index riêng bên dưới
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

-- [QUAN TRỌNG] Tạo Unique Index cho Email nhưng cho phép nhiều NULL
CREATE UNIQUE NONCLUSTERED INDEX [IX_KhachHang_Email_Unique]
ON [dbo].[KhachHang]([Email])
WHERE [Email] IS NOT NULL;
GO

/****** Object:  Table [dbo].[MonAn] ******/
CREATE TABLE [dbo].[MonAn](
    [MaMonAn] [varchar](25) NOT NULL,
    [TenMonAn] [nvarchar](100) NOT NULL,
    [MaDanhMuc] [varchar](25) NULL,
PRIMARY KEY CLUSTERED 
(
    [MaMonAn] ASC
) ON [PRIMARY]
) ON [PRIMARY]
GO

/****** Object:  Table [dbo].[NguyenLieu] ******/
CREATE TABLE [dbo].[NguyenLieu](
    [MaNguyenLieu] [varchar](25) NOT NULL,
    [TenNguyenLieu] [nvarchar](100) NOT NULL,
    [DonViTinh] [nvarchar](50) NULL,
    [SoLuongTonKho] [int] NOT NULL,
PRIMARY KEY CLUSTERED 
(
    [MaNguyenLieu] ASC
) ON [PRIMARY]
) ON [PRIMARY]
GO

/****** Object:  Table [dbo].[NhaCungCap] ******/
CREATE TABLE [dbo].[NhaCungCap](
    [MaNhaCungCap] [varchar](25) NOT NULL,
    [TenNhaCungCap] [nvarchar](255) NOT NULL,
    [SoDienThoai] [nvarchar](15) NOT NULL,
    [DiaChi] [nvarchar](255) NULL,
PRIMARY KEY CLUSTERED 
(
    [MaNhaCungCap] ASC
) ON [PRIMARY]
) ON [PRIMARY]
GO

/****** Object:  Table [dbo].[NhanVien] (ĐÃ SỬA LOGIC UNIQUE) ******/
CREATE TABLE [dbo].[NhanVien](
    [MaNhanVien] [varchar](25) NOT NULL,
    [HoTen] [nvarchar](100) NOT NULL,
    [TenDangNhap] [nvarchar](50) NOT NULL,
    [MatKhau] [nvarchar](256) NOT NULL,
    [MaVaiTro] [varchar](25) NOT NULL,
    [Email] [nvarchar](100) NULL,
    [SoDienThoai] [nvarchar](15) NULL,
    [HinhAnh] [nvarchar](max) NULL,
    --[MaTrangThai] [varchar](25) NOT NULL,
PRIMARY KEY CLUSTERED 
(
    [MaNhanVien] ASC
) ON [PRIMARY],
UNIQUE NONCLUSTERED ([TenDangNhap] ASC)
-- Đã xóa Unique Email và SĐT ở đây để xử lý riêng
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

-- Tạo Index Unique cho Email Nhân viên (Bỏ qua NULL)
CREATE UNIQUE NONCLUSTERED INDEX [IX_NhanVien_Email_Unique]
ON [dbo].[NhanVien]([Email])
WHERE [Email] IS NOT NULL;
GO

/****** Object:  Table [dbo].[NhapHang] ******/
CREATE TABLE [dbo].[NhapHang](
    [MaNhapHang] [varchar](25) NOT NULL,
    [MaNhanVien] [varchar](25) NOT NULL,
    [NgayNhapHang] [datetime] NOT NULL,
    [TongTien] [decimal](10, 2) NOT NULL,
PRIMARY KEY CLUSTERED 
(
    [MaNhapHang] ASC
) ON [PRIMARY]
) ON [PRIMARY]
GO


/****** Object:  Table [dbo].[PhienBanMonAn] ******/
CREATE TABLE [dbo].[PhienBanMonAn](
    [MaPhienBan] [varchar](25) NOT NULL,
    [TenPhienBan] [nvarchar](100) NOT NULL,
    [MaTrangThai] [varchar](25) NOT NULL,
    [ThuTu] [int] NULL,
PRIMARY KEY CLUSTERED 
(
    [MaPhienBan] ASC
) ON [PRIMARY]
) ON [PRIMARY]
GO

/****** Object:  Table [dbo].[TrangThaiBanAn] ******/
CREATE TABLE [dbo].[TrangThaiBanAn](
    [MaTrangThai] [varchar](25) NOT NULL,
    [TenTrangThai] [nvarchar](50) NOT NULL,
PRIMARY KEY CLUSTERED 
(
    [MaTrangThai] ASC
) ON [PRIMARY]
) ON [PRIMARY]
GO

/****** Object:  Table [dbo].[TrangThaiDonHang] ******/
CREATE TABLE [dbo].[TrangThaiDonHang](
    [MaTrangThai] [varchar](25) NOT NULL,
    [TenTrangThai] [nvarchar](100) NOT NULL,
PRIMARY KEY CLUSTERED 
(
    [MaTrangThai] ASC
) ON [PRIMARY]
) ON [PRIMARY]
GO


/****** Object:  Table [dbo].[VaiTro] ******/
CREATE TABLE [dbo].[VaiTro](
    [MaVaiTro] [varchar](25) NOT NULL,
    [TenVaiTro] [nvarchar](50) NOT NULL,
PRIMARY KEY CLUSTERED 
(
    [MaVaiTro] ASC
) ON [PRIMARY],
UNIQUE NONCLUSTERED ([TenVaiTro] ASC)
) ON [PRIMARY]
GO

CREATE TABLE [dbo].[BanAnDonHang](
    -- Khóa Chính (Khóa ngoại tham chiếu đến DonHang)
    [MaDonHang] [varchar](25) NOT NULL,
    
    -- Khóa Chính (Khóa ngoại tham chiếu đến BanAn)
    [MaBan] [varchar](25) NOT NULL,
    
    -- XÓA CỘT [NgayTao] theo yêu cầu của bạn vì thông tin này đã có trong bảng DonHang

PRIMARY KEY CLUSTERED 
(
    [MaDonHang] ASC,
    [MaBan] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO


-- ===== THÊM CÁC GIÁ TRỊ DEFAULT =====
ALTER TABLE [dbo].[BanAn] ADD  DEFAULT ((4)) FOR [SucChua]
GO
ALTER TABLE [dbo].[DonHang] ADD  DEFAULT ((1)) FOR [SoLuongNguoiDK]
GO
ALTER TABLE [dbo].[DonHang] ADD  DEFAULT ('CHO_XAC_NHAN') FOR [MaTrangThaiDonHang]
GO
ALTER TABLE [dbo].[DonHang] ADD  DEFAULT ((0)) FOR [TienDatCoc]
GO
ALTER TABLE [dbo].[KhachHang] ADD  DEFAULT ((0)) FOR [NoShowCount]
GO
ALTER TABLE [dbo].[PhienBanMonAn] ADD  DEFAULT ('CON_HANG') FOR [MaTrangThai]
GO
ALTER TABLE [dbo].[CongThucNauAn] ADD  CONSTRAINT [CK_CongThucNauAn_Gia] CHECK ([Gia] >= 0)
GO

-- =====================================================
-- THÊM TẤT CẢ KHÓA NGOẠI (FOREIGN KEYS)
-- =====================================================
-- (Phần này giữ nguyên như cũ của bạn)


ALTER TABLE [dbo].[BanAn]  WITH CHECK ADD  CONSTRAINT [FK_BanAn_TrangThaiBanAn] FOREIGN KEY([MaTrangThai])
REFERENCES [dbo].[TrangThaiBanAn] ([MaTrangThai])
GO
--ALTER TABLE [dbo].[CheBienMonAn]  WITH CHECK ADD FOREIGN KEY([MaPhienBan])
--REFERENCES [dbo].[PhienBanMonAn] ([MaPhienBan])
GO
ALTER TABLE [dbo].[ChiTietDonHang]  WITH CHECK ADD FOREIGN KEY([MaDonHang])
REFERENCES [dbo].[DonHang] ([MaDonHang])
GO
ALTER TABLE [dbo].[ChiTietDonHang]  WITH CHECK ADD FOREIGN KEY([MaPhienBan])
REFERENCES [dbo].[PhienBanMonAn] ([MaPhienBan])
GO
ALTER TABLE [dbo].[ChiTietDonHang]  WITH CHECK ADD FOREIGN KEY([MaCongThuc])
REFERENCES [dbo].[CongThucNauAn] ([MaCongThuc])
GO
ALTER TABLE [dbo].[ChiTietNhapHang]  WITH CHECK ADD FOREIGN KEY([MaCungUng])
REFERENCES [dbo].[CungUng] ([MaCungUng])
GO
ALTER TABLE [dbo].[ChiTietNhapHang]  WITH CHECK ADD FOREIGN KEY([MaNhapHang])
REFERENCES [dbo].[NhapHang] ([MaNhapHang])
GO
ALTER TABLE [dbo].[ChiTietMonAn]  WITH CHECK ADD FOREIGN KEY([MaMonAn])
REFERENCES [dbo].[MonAn] ([MaMonAn])
GO
ALTER TABLE [dbo].[CongThucNauAn]  WITH CHECK ADD FOREIGN KEY([MaCT])
REFERENCES [dbo].[ChiTietMonAn] ([MaCT])
GO
ALTER TABLE [dbo].[CongThucNauAn]  WITH CHECK ADD FOREIGN KEY([MaPhienBan])
REFERENCES [dbo].[PhienBanMonAn] ([MaPhienBan])
GO
ALTER TABLE [dbo].[ChiTietCongThuc]  WITH CHECK ADD FOREIGN KEY([MaCongThuc])
REFERENCES [dbo].[CongThucNauAn] ([MaCongThuc])
GO
ALTER TABLE [dbo].[ChiTietCongThuc]  WITH CHECK ADD FOREIGN KEY([MaNguyenLieu])
REFERENCES [dbo].[NguyenLieu] ([MaNguyenLieu])
GO
ALTER TABLE [dbo].[CungUng]  WITH CHECK ADD FOREIGN KEY([MaNguyenLieu])
REFERENCES [dbo].[NguyenLieu] ([MaNguyenLieu])
GO
ALTER TABLE [dbo].[CungUng]  WITH CHECK ADD FOREIGN KEY([MaNhaCungCap])
REFERENCES [dbo].[NhaCungCap] ([MaNhaCungCap])
GO

ALTER TABLE [dbo].[DonHang]  WITH CHECK ADD FOREIGN KEY([MaKhachHang])
REFERENCES [dbo].[KhachHang] ([MaKhachHang])
GO
ALTER TABLE [dbo].[DonHang]  WITH CHECK ADD FOREIGN KEY([MaNhanVien])
REFERENCES [dbo].[NhanVien] ([MaNhanVien])
GO
ALTER TABLE [dbo].[DonHang]  WITH CHECK ADD  CONSTRAINT [FK_DonHang_TrangThaiDonHang] FOREIGN KEY([MaTrangThaiDonHang])
REFERENCES [dbo].[TrangThaiDonHang] ([MaTrangThai])
GO
ALTER TABLE [dbo].[HinhAnhMonAn]  WITH CHECK ADD FOREIGN KEY([MaMonAn])
REFERENCES [dbo].[MonAn] ([MaMonAn])
GO
ALTER TABLE [dbo].[MonAn]  WITH CHECK ADD FOREIGN KEY([MaDanhMuc])
REFERENCES [dbo].[DanhMucMonAn] ([MaDanhMuc])
GO
ALTER TABLE [dbo].[NhanVien]  WITH CHECK ADD FOREIGN KEY([MaVaiTro])
REFERENCES [dbo].[VaiTro] ([MaVaiTro])
GO
ALTER TABLE [dbo].[NhapHang]  WITH CHECK ADD FOREIGN KEY([MaNhanVien])
REFERENCES [dbo].[NhanVien] ([MaNhanVien])
GO


ALTER TABLE [dbo].[BanAnDonHang] WITH CHECK ADD CONSTRAINT [FK_BanAnDonHang_DonHang] 
FOREIGN KEY([MaDonHang]) REFERENCES [dbo].[DonHang] ([MaDonHang]) ON DELETE CASCADE
GO

ALTER TABLE [dbo].[BanAnDonHang] WITH CHECK ADD CONSTRAINT [FK_BanAnDonHang_BanAn] 
FOREIGN KEY([MaBan]) REFERENCES [dbo].[BanAn] ([MaBan])
GO

CREATE INDEX [IX_BanAnDonHang_MaBan] ON [dbo].[BanAnDonHang]([MaBan])
GO
CREATE INDEX [IX_ChiTietMonAn_MaMonAn] ON [dbo].[ChiTietMonAn]([MaMonAn])
GO
CREATE INDEX [IX_CongThucNauAn_MaCT] ON [dbo].[CongThucNauAn]([MaCT])
GO
CREATE INDEX [IX_CongThucNauAn_MaPhienBan] ON [dbo].[CongThucNauAn]([MaPhienBan])
GO



-- ============================================================
-- CHÈN DỮ LIỆU (Đã bổ sung trạng thái CHO_THANH_TOAN)
-- ============================================================

-- Vô hiệu hóa TẤT CẢ kiểm tra khóa ngoại để chèn dữ liệu
EXEC sp_MSforeachtable 'ALTER TABLE ? NOCHECK CONSTRAINT ALL'
GO

-- 1. TrangThaiBanAn
INSERT INTO [dbo].[TrangThaiBanAn] ([MaTrangThai], [TenTrangThai]) VALUES
('TTBA001', N'Trống'), ('TTBA002', N'Đang phục vụ'),
('TTBA003', N'Đã đặt'), ('TTBA004', N'Bảo trì')


-- 3. TrangThaiDonHang (CẬP NHẬT THÊM 'CHO_THANH_TOAN')
INSERT INTO [dbo].[TrangThaiDonHang] (MaTrangThai, TenTrangThai) VALUES
('CHO_XAC_NHAN', N'Chờ xác nhận'), ('DA_XAC_NHAN', N'Đã xác nhận'),
('DA_HUY', N'Đã hủy'), ('DA_HOAN_THANH', N'Đã hoàn thành'),
('NO_SHOW', N'Vắng mặt (No-Show)'), 
('CHO_THANH_TOAN', N'Chờ thanh toán');



-- chèn dữ liệu cho: VaiTro
INSERT INTO [dbo].[VaiTro] ([MaVaiTro], [TenVaiTro]) VALUES
('VT001', N'Quản lý'), ('VT002', N'Nhân viên phục vụ'), ('VT003', N'Đầu bếp'),
('VT004', N'Thu ngân'), ('VT005', N'Bảo vệ'), ('VT006', N'Tạp vụ');

-- chèn dữ liệu cho: DanhMucMonAn
INSERT INTO [dbo].[DanhMucMonAn] ([MaDanhMuc], [TenDanhMuc]) VALUES
('DM001', N'Món khai vị'), ('DM002', N'Lẩu'), ('DM003', N'Tráng miệng'),
('DM004', N'Thức uống'), ('DM005', N'Món nướng'), ('DM006', N'Món chay'),
('DM007', N'Cơm'), ('DM008', N'Hải sản');

-- chèn dữ liệu cho: KhachHang
INSERT INTO [dbo].[KhachHang] ([MaKhachHang], [HoTen], [SoDienThoai], [Email], [HinhAnh], [NoShowCount]) VALUES
('KH001', N'Nguyễn Văn An', '0912345678', 'an.nguyen@gmail.com', 'an.jpg', 0),
('KH002', N'Trần Thị Bình', '0912345679', 'binh.tran@gmail.com', 'binh.jpg', 0),
('KH003', N'Lê Văn Cường', '0912345680', 'cuong.le@gmail.com', 'cuong.jpg', 0),
('KH004', N'Phạm Thị Dung', '0912345681', 'dung.pham@gmail.com', 'dung.jpg', 0),
('KH005', N'Hoàng Văn Giang', '0912345682', 'giang.hoang@gmail.com', 'giang.jpg', 0),
('KH006', N'Vũ Thị Hương', '0912345683', 'huong.vu@gmail.com', 'huong.jpg', 0),
('KH007', N'Đặng Văn Long', '0912345684', 'long.dang@gmail.com', 'long.jpg', 0),
('KH008', N'Bùi Thị Mai', '0912345685', 'mai.bui@gmail.com', 'mai.jpg', 0),
('KH009', N'Ngô Văn Nam', '0912345686', 'nam.ngo@gmail.com', 'nam.jpg', 0),
('KH010', N'Dương Thị Oanh', '0912345687', 'oanh.duong@gmail.com', 'oanh.jpg', 0),
('KH011', N'Trần Văn Phát', '0911111111', 'phat.tran@gmail.com', 'phat.jpg', 0),
('KH012', N'Lê Thị Quyên', '0922222222', 'quyen.le@gmail.com', 'quyen.jpg', 0),
('KH013', N'Đỗ Bá Rừng', '0933333333', 'rung.do@gmail.com', 'rung.jpg', 0),
('KH014', N'Hồ Thị Sen', '0944444444', 'sen.ho@gmail.com', 'sen.jpg', 0),
('KH015', N'Ngô Văn Tùng', '0955555555', 'tung.ngo@gmail.com', 'tung.jpg', 0),
('KH016', N'Dương Văn Út', '0966666666', 'ut.duong@gmail.com', 'ut.jpg', 0),
('KH017', N'Phan Thị Vân', '0977777777', 'van.phan@gmail.com', 'van.jpg', 0),
('KH018', N'Lý Văn Xuân', '0988888888', 'xuan.ly@gmail.com', 'xuan.jpg', 0),
('KH019', N'Võ Thị Yến', '0999999999', 'yen.vo@gmail.com', 'yen.jpg', 0),
('KH020', N'Trịnh Hoài An', '0901234567', 'an.trinh@gmail.com', 'an_trinh.jpg', 0),
('KH021', N'Nguyễn Hữu Ái', '0901112233', 'ai.nguyen@gmail.com', 'ai.jpg', 0),
('KH022', N'Võ Tấn Bằng', '0901112244', 'bang.vo@gmail.com', 'bang.jpg', 0),
('KH023', N'Huỳnh Ngọc Châu', '0901112255', 'chau.huynh@gmail.com', 'chau.jpg', 0),
('KH024', N'Trương Minh Đức', '0901112266', 'duc.truong@gmail.com', 'duc.jpg', 0),
('KH025', N'Hà Thị Giang', '0901112277', 'giang.ha@gmail.com', 'giang_ha.jpg', 0),
('KH026', N'Đinh Quốc Huy', '0901112288', 'huy.dinh@gmail.com', 'huy.jpg', 0),
('KH027', N'Lương Yến Khanh', '0901112299', 'khanh.luong@gmail.com', 'khanh.jpg', 0),
('KH028', N'Mai Đức Lợi', '0901113300', 'loi.mai@gmail.com', 'loi.jpg', 0),
('KH029', N'Đoàn Văn Mẫn', '0901113311', 'man.doan@gmail.com', 'man.jpg', 0),
('KH030', N'Hoàng Thị Ngân', '0901113322', 'ngan.hoang@gmail.com', 'ngan.jpg', 1),
('KH031', N'Phạm Gia Phú', '0901113333', 'phu.pham@gmail.com', 'phu.jpg', 0),
('KH032', N'Tô Hoài Sang', '0901113344', 'sang.to@gmail.com', 'sang.jpg', 0),
('KH033', N'Lê Minh Thông', '0901113355', 'thong.le@gmail.com', 'thong.jpg', 0),
('KH034', N'VươngGia Uy', '0901113366', 'uy.vuong@gmail.com', 'uy.jpg', 0),
('KH035', N'Nguyễn Thanh Vi', '0901113377', 'vi.nguyen@gmail.com', 'vi.jpg', 0),
('KH036', N'Đặng Minh Vũ', '0901113388', 'vu.dang@gmail.com', 'vu.jpg', 0),
('KH037', N'Tống Phước Lộc', '0901113399', 'loc.tong@gmail.com', 'loc.jpg', 0),
('KH038', N'Triệu Thị Mỹ', '0901114400', 'my.trieu@gmail.com', 'my.jpg', 0),
('KH039', N'Uông Văn Tài', '0901114411', 'tai.uong@gmail.com', 'tai.jpg', 0),
('KH040', N'Cù Minh Tâm', '0901114422', 'tam.cu@gmail.com', 'tam.jpg', 0);

INSERT INTO [dbo].[BanAn] ([MaBan], [TenBan], [MaTrangThai], [SucChua]) VALUES
('B001', N'Bàn 1', 'TTBA001', 4), ('B002', N'Bàn 2', 'TTBA001', 4),
('B003', N'Bàn 3', 'TTBA002', 6), ('B004', N'Bàn 4', 'TTBA001', 6),
('B005', N'Bàn 5', 'TTBA003', 8), ('B006', N'Bàn 6', 'TTBA001', 8),
('B007', N'Bàn 7', 'TTBA001', 10), ('B008', N'Bàn 8', 'TTBA002', 12),
('B009', N'Bàn 9', 'TTBA001', 4), ('B010', N'Bàn 10', 'TTBA001', 4),
('B011', N'Bàn 11', 'TTBA001', 4), ('B012', N'Bàn 12', 'TTBA001', 4),
('B013', N'Bàn 13', 'TTBA001', 6), ('B014', N'Bàn 14', 'TTBA001', 6),
('B015', N'Bàn 15', 'TTBA001', 10), ('B016', N'Bàn 16', 'TTBA001', 10),
('B017', N'Bàn 17', 'TTBA001', 2), ('B018', N'Bàn 18', 'TTBA001', 2),
('B019', N'Bàn 19', 'TTBA003', 15), ('B020', N'Bàn 20', 'TTBA001', 20),
('B021', N'Bàn 21', 'TTBA001', 2), ('B022', N'Bàn 22', 'TTBA001', 2),
('B023', N'Bàn 23', 'TTBA001', 4), ('B024', N'Bàn 24', 'TTBA001', 4),
('B025', N'Bàn 25', 'TTBA001', 6), ('B026', N'Bàn 26', 'TTBA001', 6),
('B027', N'Bàn 27', 'TTBA001', 8), ('B028', N'Bàn 28', 'TTBA001', 8),
('B029', N'Bàn 29', 'TTBA001', 10), ('B030', N'Bàn 30', 'TTBA001', 10),
('B031', N'Bàn 31', 'TTBA001', 4), ('B032', N'Bàn 32', 'TTBA001', 4),
('B033', N'Bàn 33', 'TTBA001', 2), ('B034', N'Bàn 34', 'TTBA001', 2),
('B035', N'Bàn 35', 'TTBA001', 12), ('B036', N'Bàn 36', 'TTBA001', 12),
('B037', N'Bàn 37', 'TTBA004', 6), ('B038', N'Bàn 38', 'TTBA004', 6),
('B039', N'Bàn 39', 'TTBA001', 8), ('B040', N'Bàn 40', 'TTBA001', 8);

-- chèn dữ liệu cho: NhaCungCap
INSERT INTO [dbo].[NhaCungCap] ([MaNhaCungCap], [TenNhaCungCap], [SoDienThoai], [DiaChi]) VALUES
('NCC001', N'Công ty rau củ Đà Lạt Xanh', '090111222', N'123, Lâm Đồng'),
('NCC002', N'Vựa hải sản Vũng Tàu', '090222333', N'456, Vũng Tàu'),
('NCC003', N'Công ty thịt bò CP', '090333444', N'789, Đồng Nai'),
('NCC004', N'Đại lý bia Sài Gòn', '090444555', N'101, Q.1, TPHCM'),
('NCC005', N'Tổng kho gia vị', '090555666', N'102, Q.5, TPHCM'),
('NCC006', N'Trang trại gà sạch Ba Vì', '090666777', N'103, Ba Vì, Hà Nội'),
('NCC007', N'Công ty nước giải khát Pepsico', '090777888', N'104, Hóc Môn, TPHCM'),
('NCC008', N'Nhà cung cấp gạo miền Tây', '090888999', N'105, Cần Thơ'),
('NCC009', N'Công ty TNHH Nấm Việt', '090999000', N'106, Q.12, TPHCM'),
('NCC010', N'Lò mổ heo Vissan', '091111222', N'107, Long An'),
('NCC011', N'Trang trại rau hữu cơ', '0912121212', N'111, Hóc Môn'),
('NCC012', N'Công ty Nước mắm Phú Quốc', '0912121213', N'222, Kiên Giang'),
('NCC013', N'Nhà phân phối rượu Vang Đà Lạt', '0912121214', N'333, Lâm Đồng'),
('NCC014', N'Lò bánh mì ABC', '0912121215', N'444, Q.10, TPHCM'),
('NCC015', N'Công ty Cà phê Trung Nguyên', '0912121216', N'555, Đắk Lắk'),
('NCC016', N'Vựa trái cây Cái Bè', '0912121217', N'666, Tiền Giang'),
('NCC017', N'Công ty Sữa Vinamilk', '0912121218', N'777, Q.7, TPHCM'),
('NCC018', N'Hợp tác xã nấm sạch', '0912121219', N'888, Đồng Nai'),
('NCC019', N'Đại lý trứng gia cầm Ba Huân', '0912121220', N'999, Bình Chánh'),
('NCC020', N'Nhà cung cấp đồ khô', '0912121221', N'121, Q.5, TPHCM');

-- chèn dữ liệu cho: NguyenLieu
INSERT INTO [dbo].[NguyenLieu] ([MaNguyenLieu], [TenNguyenLieu], [DonViTinh], [SoLuongTonKho]) VALUES
('NL001', N'Thịt bò thăn', N'kg', 50), ('NL002', N'Tôm sú (loại 1)', N'kg', 30),
('NL003', N'Gà ta', N'con', 40), ('NL004', N'Cá hồi fillet', N'kg', 20),
('NL005', N'Rau muống', N'bó', 100), ('NL006', N'Nấm kim châm', N'gói', 80),
('NL007', N'Gạo ST25', N'kg', 200), ('NL008', N'Bia Sài Gòn (thùng)', N'thùng', 50),
('NL009', N'Sườn heo non', N'kg', 60), ('NL010', N'Đậu hũ non', N'miếng', 150),
('NL011', N'Súp lơ xanh', N'kg', 50), ('NL012', N'Cà rốt', N'kg', 100),
('NL013', N'Trứng gà', N'quả', 300), ('NL014', N'Sữa đặc', N'hộp', 50),
('NL015', N'Bánh mì sandwich', N'gói', 30), ('NL016', N'Vang đỏ Đà Lạt', N'chai', 20),
('NL017', N'Cà phê hạt', N'kg', 40), ('NL018', N'Nước mắm', N'lít', 100),
('NL019', N'Đường cát', N'kg', 200), ('NL020', N'Bột chiên giòn', N'gói', 80),
('NL021', N'Hạt dưa', N'kg', 10), ('NL022', N'Đậu phộng', N'kg', 15),
('NL023', N'Bánh tráng', N'xấp', 50), ('NL024', N'Dâu tây', N'kg', 5),
('NL025', N'Nho', N'kg', 10), ('NL026', N'Gói lẩu thái', N'gói', 30),
('NL027', N'Lá giang', N'bó', 20), ('NL028', N'Giấm gạo', N'chai', 15),
('NL029', N'Hạt sen', N'kg', 10), ('NL030', N'Sữa chua', N'lốc', 20),
('NL031', N'Nước lọc Aquafina', N'thùng', 30), ('NL032', N'Pepsi (lon)', N'thùng', 25),
('NL033', N'Coca (lon)', N'thùng', 25), ('NL034', N'Hàu sữa', N'con', 100),
('NL035', N'Phô mai', N'kg', 10), ('NL036', N'Tôm hùm', N'con', 10),
('NL037', N'Cua thịt', N'con', 20), ('NL038', N'Ghẹ xanh', N'con', 20),
('NL039', N'Mực ống', N'kg', 30), ('NL040', N'Sò điệp', N'kg', 15);


INSERT INTO [dbo].[NhanVien] ([MaNhanVien], [HoTen], [TenDangNhap], [MatKhau], [MaVaiTro], [Email], [SoDienThoai], [HinhAnh]) VALUES
('NV001', N'Nguyễn Văn Quản Lý', 'manager1', 'hashed_password_A', 'VT001', 'quanly@email.com', '0987654321', 'anh_a.jpg'),
('NV002', N'Trần Thị Thu Ngân', 'cashier1', 'hashed_password_B', 'VT004', 'thungan1@email.com', '0987654322', 'anh_b.jpg'),
('NV003', N'Lê Văn Phục Vụ', 'staff1', 'hashed_password_C', 'VT002', 'phucvu1@email.com', '0987654323', 'anh_c.jpg'),
('NV004', N'Phạm Thị Phục Vụ', 'staff2', 'hashed_password_D', 'VT002', 'phucvu2@email.com', '0987654324', 'anh_d.jpg'),
('NV005', N'Hoàng Văn Bếp Trưởng', 'chef1', 'hashed_password_E', 'VT003', 'beptruong@email.com', '0987654325', 'anh_e.jpg'),
('NV006', N'Vũ Thị Bếp Phó', 'chef2', 'hashed_password_F', 'VT003', 'beppho@email.com', '0987654326', 'anh_f.jpg'),
('NV007', N'Đặng Văn Phục Vụ', 'staff3', 'hashed_password_G', 'VT002', 'phucvu3@email.com', '0987654327', 'anh_g.jpg'),
('NV008', N'Bùi Thị Phục Vụ', 'staff4', 'hashed_password_H', 'VT002', 'phucvu4@email.com', '0987654328', 'anh_h.jpg'),
('NV009', N'Ngô Văn Thu Ngân', 'cashier2', 'hashed_password_I', 'VT004', 'thungan2@email.com', '0987654329', 'anh_i.jpg'),
('NV010', N'Dương Thị Nghỉ Việc', 'old_staff', 'hashed_password_K', 'VT002', 'nghiviec@email.com', '0987654330', 'anh_k.jpg'),
('NV011', N'Phan Thanh Quản Trị', 'admin2', 'hashed_pass_11', 'VT001', 'admin2@email.com', '0911111101', 'anh_nv11.jpg'),
('NV012', N'Lê Thị Bảo Vệ', 'security1', 'hashed_pass_12', 'VT005', 'security1@email.com', '0911111102', 'anh_nv12.jpg'),
('NV013', N'Trần Văn Phục Vụ Mới', 'staff5', 'hashed_pass_13', 'VT002', 'staff5@email.com', '0911111103', 'anh_nv13.jpg'),
('NV014', N'Ngô Thị Tạp Vụ', 'cleaner1', 'hashed_pass_14', 'VT006', 'cleaner1@email.com', '0911111104', 'anh_nv14.jpg'),
('NV015', N'Vũ Hữu Bếp Phụ', 'chef3', 'hashed_pass_15', 'VT003', 'chef3@email.com', '0911111105', 'anh_nv15.jpg'),
('NV016', N'Hà Thị Thu Ngân 3', 'cashier3', 'hashed_pass_16', 'VT004', 'cashier3@email.com', '0911111106', 'anh_nv16.jpg'),
('NV017', N'Đặng Văn Thực Tập', 'intern1', 'hashed_pass_17', 'VT002', 'intern1@email.com', '0911111107', 'anh_nv17.jpg'),
('NV018', N'Nguyễn Thị Phục Vụ 6', 'staff6', 'hashed_pass_18', 'VT002', 'staff6@email.com', '0911111108', 'anh_nv18.jpg'),
('NV019', N'Lý Văn Phục Vụ 7', 'staff7', 'hashed_pass_19', 'VT002', 'staff7@email.com', '0911111109', 'anh_nv19.jpg'),
('NV020', N'Bùi Thanh Nghỉ Phép', 'staff8', 'hashed_pass_20', 'VT002', 'staff8@email.com', '0911111110', 'anh_nv20.jpg'),
('NV021', N'Trần Hữu Danh', 'staff9', 'hashed_pass_21', 'VT002', 'danh.tran@email.com', '0911111111', 'anh_nv21.jpg'),
('NV022', N'Lê Thị Kiều', 'staff10', 'hashed_pass_22', 'VT002', 'kieu.le@email.com', '0911111112', 'anh_nv22.jpg'),
('NV023', N'Phạm Văn Mách', 'security2', 'hashed_pass_23', 'VT005', 'mach.pham@email.com', '0911111113', 'anh_nv23.jpg'),
('NV024', N'Đỗ Thị Nở', 'cleaner2', 'hashed_pass_24', 'VT006', 'no.do@email.com', '0911111114', 'anh_nv24.jpg'),
('NV025', N'Quách Tĩnh', 'chef4', 'hashed_pass_25', 'VT003', 'tinh.quach@email.com', '0911111115', 'anh_nv25.jpg'),
('NV026', N'Hoàng Dung', 'chef5', 'hashed_pass_26', 'VT003', 'dung.hoang@email.com', '0911111116', 'anh_nv26.jpg'),
('NV027', N'Dương Khang', 'staff11', 'hashed_pass_27', 'VT002', 'khang.duong@email.com', '0911111117', 'anh_nv27.jpg'),
('NV028', N'Mục Niệm Từ', 'staff12', 'hashed_pass_28', 'VT002', 'tu.muc@email.com', '0911111118', 'anh_nv28.jpg'),
('NV029', N'Âu Dương Phong', 'chef_master', 'hashed_pass_29', 'VT003', 'phong.au@email.com', '0911111119', 'anh_nv29.jpg'),
('NV030', N'Hồng Thất Công', 'manager2', 'hashed_pass_30', 'VT001', 'cong.hong@email.com', '0911111120', 'anh_nv30.jpg');
-- chèn dữ liệu cho: MonAn
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

-- chèn dữ liệu cho: ChiTietMonAn (mỗi món ăn có ít nhất 1 chi tiết)
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

-- chèn dữ liệu cho: PhienBanMonAn (đã xóa MaMonAn và Gia)
INSERT INTO [dbo].[PhienBanMonAn] ([MaPhienBan], [TenPhienBan], [MaTrangThai], [ThuTu]) VALUES
('PB001', N'Phần', 'CON_HANG', 1), ('PB002', N'Phần', 'CON_HANG', 1),
('PB003', N'Phần', 'CON_HANG', 1), ('PB004', N'Phần', 'CON_HANG', 1),
('PB005', N'Dĩa', 'CON_HANG', 1), ('PB006', N'Lẩu nhỏ', 'CON_HANG', 1),
('PB007', N'Lẩu nhỏ', 'CON_HANG', 1), ('PB008', N'Lẩu nhỏ', 'CON_HANG', 1),
('PB009', N'Lẩu nhỏ', 'CON_HANG', 1), ('PB010', N'Lẩu nhỏ', 'CON_HANG', 1),
('PB011', N'Phần', 'CON_HANG', 1), ('PB012', N'Chén', 'CON_HANG', 1),
('PB013', N'Phần', 'CON_HANG', 1), ('PB014', N'Viên', 'CON_HANG', 1),
('PB015', N'Ly', 'CON_HANG', 1), ('PB016', N'Chai 500ml', 'CON_HANG', 1),
('PB017', N'Ly', 'CON_HANG', 1), ('PB018', N'Lon', 'CON_HANG', 1),
('PB019', N'Chai', 'CON_HANG', 1), ('PB020', N'Lon', 'CON_HANG', 1),
('PB021', N'Phần 300g', 'CON_HANG', 1), ('PB022', N'Phần 3 con', 'CON_HANG', 1),
('PB023', N'Phần 300g', 'CON_HANG', 1), ('PB024', N'Phần 200g', 'CON_HANG', 1),
('PB025', N'Nửa con', 'CON_HANG', 1), ('PB026', N'Phần', 'CON_HANG', 1),
('PB027', N'Dĩa', 'CON_HANG', 1), ('PB028', N'Tô', 'CON_HANG', 1),
('PB029', N'Dĩa', 'CON_HANG', 1), ('PB030', N'Dĩa', 'CON_HANG', 1),
('PB031', N'Dĩa', 'CON_HANG', 1), ('PB032', N'Dĩa', 'CON_HANG', 1),
('PB033', N'Dĩa', 'CON_HANG', 1), ('PB034', N'Dĩa', 'CON_HANG', 1),
('PB035', N'Phần', 'CON_HANG', 1), ('PB036', N'1 con (1kg)', 'CON_HANG', 1),
('PB037', N'1 con (700g)', 'CON_HANG', 1), ('PB038', N'1 con (600g)', 'CON_HANG', 1),
('PB039', N'Dĩa 300g', 'CON_HANG', 1), ('PB040', N'Phần 4 con', 'CON_HANG', 1);

-- chèn dữ liệu cho: HinhAnhMonAn
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
('MA023', 'images/monans/tomnuong/tomnuong1.jpg'), ('MA023', 'images/monans/tomnuong/tomnuong2.jpg');
GO

-- chèn dữ liệu cho: CungUng
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

-- chèn dữ liệu cho: NhapHang
INSERT INTO [dbo].[NhapHang] ([MaNhapHang], [MaNhanVien], [NgayNhapHang], [TongTien]) VALUES
('NH001', 'NV001', '2025-10-01 08:00:00', 0), ('NH002', 'NV001', '2025-10-02 09:00:00', 0),
('NH003', 'NV001', '2025-10-03 08:00:00', 0), ('NH004', 'NV001', '2025-10-04 08:00:00', 0),
('NH005', 'NV001', '2025-10-05 08:00:00', 0), ('NH006', 'NV011', '2025-10-06 08:00:00', 0),
('NH007', 'NV011', '2025-10-06 09:00:00', 0), ('NH008', 'NV001', '2025-10-07 08:00:00', 0),
('NH009', 'NV001', '2025-10-07 09:00:00', 0), ('NH010', 'NV011', '2025-10-08 08:00:00', 0),
('NH011', 'NV011', '2025-10-08 09:00:00', 0), ('NH012', 'NV001', '2025-10-09 08:00:00', 0),
('NH013', 'NV001', '2025-10-09 09:00:00', 0), ('NH014', 'NV011', '2025-10-10 08:00:00', 0),
('NH015', 'NV011', '2025-10-10 09:00:00', 0), ('NH016', 'NV001', '2025-10-11 08:00:00', 0),
('NH017', 'NV001', '2025-10-12 08:00:00', 0), ('NH018', 'NV001', '2025-10-13 08:00:00', 0),
('NH019', 'NV011', '2025-10-14 08:00:00', 0), ('NH020', 'NV011', '2025-10-15 08:00:00', 0),
('NH021', 'NV001', '2025-10-16 08:00:00', 0), ('NH022', 'NV001', '2025-10-17 08:00:00', 0),
('NH023', 'NV011', '2025-10-18 08:00:00', 0), ('NH024', 'NV011', '2025-10-19 08:00:00', 0),
('NH025', 'NV001', '2025-10-20 08:00:00', 0), ('NH026', 'NV001', '2025-10-21 08:00:00', 0),
('NH027', 'NV011', '2025-10-22 08:00:00', 0), ('NH028', 'NV001', '2025-10-23 08:00:00', 0),
('NH029', 'NV011', '2025-10-24 08:00:00', 0), ('NH030', 'NV001', '2025-10-25 08:00:00', 0),
('NH031', 'NV011', '2025-10-26 08:00:00', 0), ('NH032', 'NV001', '2025-10-27 08:00:00', 0),
('NH033', 'NV011', '2025-10-28 08:00:00', 0), ('NH034', 'NV001', '2025-10-29 08:00:00', 0),
('NH035', 'NV011', '2025-10-30 08:00:00', 0), ('NH036', 'NV001', '2025-10-31 08:00:00', 0),
('NH037', 'NV011', '2025-11-01 08:00:00', 0), ('NH038', 'NV001', '2025-11-02 08:00:00', 0),
('NH039', 'NV011', '2025-11-03 08:00:00', 0), ('NH040', 'NV001', '2025-11-04 08:00:00', 0);

INSERT INTO [dbo].[DonHang] ([MaDonHang], [MaNhanVien], [MaKhachHang], [MaTrangThaiDonHang], [ThoiGianDatHang], [TGDatDuKien], [TGNhanBan], [ThoiGianKetThuc], [SoLuongNguoiDK], [TienDatCoc], [GhiChu], [ThanhToan]) VALUES
('DH001', 'NV003', 'KH001', 'DA_HOAN_THANH', '2025-10-08 18:00:00', 15, '2025-10-08 18:15:00', '2025-10-08 20:00:00', 5, 0, N'Đã thanh toán (Tháng 10)', 1),
('DH002', 'NV004', 'KH002', 'DA_HOAN_THANH', '2025-10-08 19:00:00', 10, '2025-10-08 19:10:00', '2025-10-08 21:00:00', 10, 0, N'Đã thanh toán (Tháng 10)', 1),
('DH003', 'NV007', 'KH003', 'DA_HOAN_THANH', '2025-10-09 11:00:00', 5, '2025-10-09 11:05:00', '2025-10-09 12:00:00', 4, 0, N'Đã thanh toán (Tháng 10)', 1),
('DH004', 'NV008', 'KH004', 'DA_HOAN_THANH', '2025-10-09 12:00:00', 5, '2025-10-09 12:05:00', '2025-10-09 13:00:00', 2, 0, N'Đã thanh toán (Tháng 10)', 1),
('DH005', 'NV003', 'KH005', 'DA_HOAN_THANH', '2025-10-10 18:30:00', 10, '2025-10-10 18:40:00', '2025-10-10 20:30:00', 4, 0, N'Đã thanh toán (Tháng 10)', 1),
('DH006', 'NV004', 'KH006', 'DA_HOAN_THANH', '2025-10-11 19:00:00', 20, '2025-10-11 19:20:00', '2025-10-11 21:30:00', 9, 0, N'Đã thanh toán (Tháng 10)', 1),
('DH007', 'NV007', 'KH007', 'DA_HOAN_THANH', '2025-10-12 20:00:00', 5, '2025-10-12 20:05:00', '2025-10-12 21:00:00', 2, 0, N'Đã thanh toán (Tháng 10)', 1),
('DH008', 'NV008', 'KH008', 'DA_HOAN_THANH', '2025-10-13 17:00:00', 10, '2025-10-13 17:10:00', '2025-10-13 18:00:00', 3, 0, N'Đã thanh toán (Tháng 10)', 1),
('DH009', 'NV003', 'KH009', 'DA_HOAN_THANH', '2025-10-14 19:30:00', 15, '2025-10-14 19:45:00', '2025-10-14 21:00:00', 6, 0, N'Đã thanh toán (Tháng 10)', 1),
('DH010', 'NV004', 'KH010', 'DA_HOAN_THANH', '2025-10-15 18:00:00', 10, '2025-10-15 18:10:00', '2025-10-15 20:00:00', 7, 0, N'Đã thanh toán (Tháng 10)', 1),
('DH011', 'NV013', 'KH011', 'DA_HOAN_THANH', '2025-11-01 18:00:00', 10, '2025-11-01 18:10:00', '2025-11-01 20:00:00', 4, 0, N'Đã thanh toán (Tháng 11)', 1),
('DH012', 'NV018', 'KH012', 'DA_HOAN_THANH', '2025-11-01 18:05:00', 10, '2025-11-01 18:15:00', '2025-11-01 19:30:00', 3, 0, N'Đã thanh toán (Tháng 11)', 1),
('DH013', 'NV019', 'KH013', 'DA_HOAN_THANH', '2025-11-02 19:00:00', 15, '2025-11-02 19:15:00', '2025-11-02 21:00:00', 6, 0, N'Đã thanh toán (Tháng 11)', 1),
('DH014', 'NV013', 'KH014', 'DA_HOAN_THANH', '2025-11-03 12:00:00', 5, '2025-11-03 12:05:00', '2025-11-03 13:00:00', 2, 0, N'Đã thanh toán (Tháng 11)', 1),
('DH015', 'NV018', 'KH015', 'DA_HOAN_THANH', '2025-11-03 12:10:00', 5, '2025-11-03 12:15:00', '2025-11-03 13:15:00', 2, 0, N'Đã thanh toán (Tháng 11)', 1),
('DH016', 'NV019', 'KH016', 'DA_HOAN_THANH', '2025-11-05 19:00:00', 10, '2025-11-05 19:10:00', '2025-11-05 21:00:00', 18, 0, N'Đã thanh toán (Tháng 11)', 1),
('DH017', 'NV013', 'KH017', 'DA_HOAN_THANH', '2025-11-06 18:30:00', 15, '2025-11-06 18:45:00', '2025-11-06 20:30:00', 10, 0, N'Đã thanh toán (Tháng 11)', 1),
('DH018', 'NV018', 'KH018', 'DA_HOAN_THANH', '2025-11-07 11:00:00', 5, '2025-11-07 11:05:00', '2025-11-07 12:00:00', 1, 0, N'Đã thanh toán (Tháng 11)', 1),
('DH019', 'NV019', 'KH019', 'DA_HOAN_THANH', '2025-11-07 11:05:00', 5, '2025-11-07 11:10:00', '2025-11-07 12:30:00', 3, 0, N'Đã thanh toán (Tháng 11)', 1),
('DH020', 'NV013', 'KH020', 'DA_HOAN_THANH', '2025-11-08 19:00:00', 10, '2025-11-08 19:10:00', '2025-11-08 20:45:00', 5, 0, N'Đã thanh toán (Tháng 11)', 1),
('DH021', 'NV003', 'KH021', 'DA_HOAN_THANH', '2025-11-08 19:15:00', 10, '2025-11-08 19:25:00', '2025-11-08 21:00:00', 4, 0, N'Đã thanh toán (Tháng 11)', 1),
('DH022', 'NV004', 'KH022', 'DA_HOAN_THANH', '2025-11-09 12:00:00', 5, '2025-11-09 12:05:00', '2025-11-09 13:00:00', 6, 0, N'Đã thanh toán (Tháng 11)', 1),
('DH023', 'NV007', 'KH023', 'DA_HOAN_THANH', '2025-11-09 12:05:00', 5, '2025-11-09 12:10:00', '2025-11-09 13:30:00', 5, 0, N'Đã thanh toán (Tháng 11)', 1),
('DH024', 'NV008', 'KH024', 'DA_XAC_NHAN', '2025-11-10 18:00:00', NULL, NULL, NULL, 8, 500000.00, N'Đã đặt cọc 500k (B005)', 0),
('DH025', 'NV013', 'KH025', 'DA_XAC_NHAN', '2025-11-10 18:30:00', NULL, NULL, NULL, 15, 1000000.00, N'Đặt cọc 1 triệu (B019)', 0),
('DH026', 'NV018', 'KH026', 'DA_HOAN_THANH', '2025-11-10 19:00:00', 10, '2025-11-10 19:10:00', '2025-11-10 21:00:00', 7, 0, N'Đã thanh toán (Tháng 11)', 1),
('DH027', 'NV019', 'KH027', 'DA_HOAN_THANH', '2025-11-11 19:15:00', 10, '2025-11-11 19:25:00', '2025-11-11 20:30:00', 8, 0, N'Đã thanh toán (Tháng 11)', 1),
('DH028', 'NV003', 'KH028', 'DA_HUY', '2025-11-12 11:30:00', NULL, NULL, NULL, 9, 0, N'Khách gọi báo hủy', 0),
('DH029', 'NV004', 'KH029', 'DA_HOAN_THANH', '2025-11-12 12:00:00', 5, '2025-11-12 12:05:00', '2025-11-12 13:15:00', 10, 0, N'Đã thanh toán (Tháng 11)', 1),
('DH030', 'NV007', 'KH030', 'NO_SHOW', '2025-11-13 19:00:00', NULL, NULL, NULL, 2, 0, N'Khách không đến (No-Show)', 0),
('DH031', 'NV008', 'KH031', 'DA_HOAN_THANH', '2025-11-14 19:00:00', 10, '2025-11-14 19:10:00', '2025-11-14 21:00:00', 4, 0, N'Đã thanh toán (Tháng 11)', 1),
('DH032', 'NV013', 'KH032', 'CHO_XAC_NHAN', '2025-11-15 20:00:00', NULL, NULL, NULL, 2, 0, N'Đơn mới, chờ gọi xác nhận', 0),
('DH033', 'NV018', 'KH033', 'DA_HOAN_THANH', '2025-11-16 18:00:00', 10, '2025-11-16 18:10:00', '2025-11-16 19:00:00', 2, 0, N'Đã thanh toán (Tháng 11)', 1),
('DH034', 'NV019', 'KH034', 'DA_HOAN_THANH', '2025-11-17 11:00:00', 10, '2025-11-17 11:10:00', '2025-11-17 13:00:00', 12, 0, N'Đã thanh toán (Tháng 11)', 1),
('DH035', 'NV003', 'KH035', 'DA_HOAN_THANH', '2025-11-18 11:30:00', 10, '2025-11-18 11:40:00', '2025-11-18 13:00:00', 11, 0, N'Đã thanh toán (Tháng 11)', 1),
('DH036', 'NV004', 'KH036', 'DA_HOAN_THANH', '2025-11-19 19:00:00', 10, '2025-11-19 19:10:00', '2025-11-19 21:00:00', 7, 0, N'Đã thanh toán (Tháng 11)', 1),
('DH037', 'NV007', 'KH037', 'DA_HOAN_THANH', '2025-11-20 19:00:00', 10, '2025-11-20 19:10:00', '2025-11-20 21:30:00', 8, 0, N'Đã thanh toán (Tháng 11)', 1),
('DH038', 'NV008', 'KH038', 'DA_HOAN_THANH', '2025-11-21 12:00:00', 5, '2025-11-21 12:05:00', '2025-11-21 13:00:00', 3, 0, N'Đã thanh toán (Tháng 11)', 1),
('DH039', 'NV013', 'KH039', 'DA_HOAN_THANH', '2025-11-22 18:00:00', 10, '2025-11-22 18:10:00', '2025-11-22 20:00:00', 4, 0, N'Đã thanh toán (Tháng 11)', 1),
('DH040', 'NV018', 'KH040', 'DA_HOAN_THANH', '2025-11-23 19:00:00', 10, '2025-11-23 19:10:00', '2025-11-23 21:00:00', 5, 0, N'Đã thanh toán (Tháng 11)', 1);




---- chèn dữ liệu cho: CheBienMonAn
--INSERT INTO [dbo].[CheBienMonAn] ([MaCheBien], [NgayNau], [MaPhienBan], [SoLuong]) VALUES
--('CB001', '2025-10-08 17:00:00', 'PB006', 5), ('CB002', '2025-10-08 17:05:00', 'PB008', 3),
--('CB003', '2025-10-08 17:10:00', 'PB003', 10), ('CB004', '2025-10-08 17:15:00', 'PB021', 4),
--('CB005', '2025-10-09 10:00:00', 'PB031', 20), ('CB006', '2025-10-09 10:05:00', 'PB034', 15),
--('CB007', '2025-10-10 17:00:00', 'PB036', 2), ('CB008', '2025-10-11 18:00:00', 'PB007', 8),
--('CB009', '2025-10-12 19:00:00', 'PB023', 6), ('CB010', '2025-10-13 16:00:00', 'PB027', 10),
--('CB011', '2025-11-01 17:00:00', 'PB040', 10), ('CB012', '2025-11-01 17:05:00', 'PB039', 5),
--('CB013', '2025-11-02 17:10:00', 'PB037', 5), ('CB014', '2025-11-03 11:00:00', 'PB035', 20),
--('CB015', '2025-11-03 11:05:00', 'PB032', 10), ('CB016', '2025-11-05 18:00:00', 'PB024', 3),
--('CB017', '2025-11-06 17:00:00', 'PB022', 10), ('CB018', '2025-11-07 10:05:00', 'PB031', 5),
--('CB019', '2025-11-08 18:00:00', 'PB006', 2), ('CB020', '2025-11-08 18:05:00', 'PB008', 5),
--('CB021', '2025-11-09 11:00:00', 'PB028', 10), ('CB022', '2025-11-09 11:05:00', 'PB029', 5),
--('CB023', '2025-11-10 18:00:00', 'PB030', 5), ('CB024', '2025-11-11 18:00:00', 'PB001', 10),
--('CB025', '2025-11-12 10:00:00', 'PB002', 10), ('CB026', '2025-11-13 18:00:00', 'PB004', 5),
--('CB027', '2025-11-14 18:00:00', 'PB005', 8), ('CB028', '2025-11-15 19:00:00', 'PB011', 10),
--('CB029', '2025-11-16 17:00:00', 'PB012', 10), ('CB030', '2025-11-17 10:00:00', 'PB013', 15),
--('CB031', '2025-11-18 10:00:00', 'PB014', 10), ('CB032', '2025-11-19 18:00:00', 'PB015', 10),
--('CB033', '2025-11-20 18:00:00', 'PB017', 20), ('CB034', '2025-11-21 11:00:00', 'PB025', 5),
--('CB035', '2025-11-22 17:00:00', 'PB026', 10), ('CB036', '2025-11-23 18:00:00', 'PB033', 5),
--('CB037', '2025-11-23 18:05:00', 'PB038', 4), ('CB038', '2025-11-23 18:10:00', 'PB039', 3),
--('CB039', '2025-11-23 18:15:00', 'PB040', 2), ('CB040', '2025-11-23 18:20:00', 'PB036', 1);

-- chèn dữ liệu cho: ChiTietNhapHang
INSERT INTO [dbo].[ChiTietNhapHang] ([MaNhapHang], [MaCungUng], [SoLuong], [GiaNhap]) VALUES
('NH001', 'CU001', 50, 180000.00), ('NH001', 'CU002', 30, 250000.00),
('NH002', 'CU003', 40, 100000.00), ('NH002', 'CU005', 100, 10000.00),
('NH003', 'CU010', 150, 5000.00), ('NH003', 'CU006', 80, 15000.00),
('NH004', 'CU008', 50, 300000.00), ('NH004', 'CU032', 50, 150000.00),
('NH005', 'CU009', 60, 120000.00), ('NH005', 'CU007', 200, 18000.00),
('NH006', 'CU011', 50, 30000.00), ('NH006', 'CU012', 100, 15000.00),
('NH007', 'CU013', 300, 3000.00), ('NH007', 'CU014', 50, 20000.00),
('NH008', 'CU015', 30, 25000.00), ('NH008', 'CU017', 40, 150000.00),
('NH009', 'CU018', 100, 40000.00), ('NH009', 'CU019', 200, 20000.00),
('NH010', 'CU020', 80, 18000.00), ('NH010', 'CU016', 20, 200000.00),
('NH011', 'CU021', 10, 25000.00), ('NH011', 'CU022', 15, 20000.00),
('NH012', 'CU023', 50, 30000.00), ('NH012', 'CU024', 5, 80000.00),
('NH013', 'CU025', 10, 60000.00), ('NH013', 'CU026', 30, 20000.00),
('NH014', 'CU027', 20, 10000.00), ('NH014', 'CU028', 15, 30000.00),
('NH015', 'CU029', 10, 50000.00), ('NH015', 'CU030', 20, 40000.00),
('NH016', 'CU031', 30, 100000.00), ('NH016', 'CU033', 25, 120000.00),
('NH017', 'CU034', 100, 15000.00), ('NH017', 'CU035', 10, 100000.00),
('NH018', 'CU036', 10, 350000.00), ('NH018', 'CU037', 20, 200000.00),
('NH019', 'CU038', 20, 180000.00), ('NH019', 'CU039', 30, 130000.00),
('NH020', 'CU040', 15, 220000.00), ('NH020', 'CU001', 20, 180000.00),
('NH021', 'CU002', 20, 250000.00), ('NH021', 'CU003', 20, 100000.00),
('NH022', 'CU004', 10, 350000.00), ('NH022', 'CU005', 50, 10000.00),
('NH023', 'CU006', 40, 15000.00), ('NH023', 'CU007', 100, 18000.00),
('NH024', 'CU009', 30, 120000.00), ('NH024', 'CU010', 100, 5000.00),
('NH025', 'CU011', 20, 30000.00), ('NH025', 'CU012', 30, 15000.00),
('NH026', 'CU013', 200, 3000.00), ('NH026', 'CU014', 20, 20000.00),
('NH027', 'CU021', 10, 25000.00), ('NH027', 'CU022', 10, 20000.00),
('NH028', 'CU023', 50, 30000.00), ('NH028', 'CU024', 5, 80000.00),
('NH029', 'CU025', 10, 60000.00), ('NH029', 'CU026', 20, 20000.00),
('NH030', 'CU027', 20, 10000.00), ('NH030', 'CU028', 10, 30000.00),
('NH031', 'CU029', 10, 50000.00), ('NH031', 'CU030', 15, 40000.00),
('NH032', 'CU034', 100, 15000.00), ('NH032', 'CU035', 10, 100000.00),
('NH033', 'CU036', 5, 350000.00), ('NH033', 'CU037', 10, 200000.00),
('NH034', 'CU038', 10, 180000.00), ('NH034', 'CU039', 20, 130000.00),
('NH035', 'CU040', 10, 220000.00), ('NH035', 'CU008', 20, 300000.00),
('NH036', 'CU032', 20, 150000.00), ('NH036', 'CU031', 20, 100000.00),
('NH037', 'CU016', 10, 200000.00), ('NH037', 'CU017', 20, 150000.00),
('NH038', 'CU018', 50, 40000.00), ('NH038', 'CU019', 100, 20000.00),
('NH039', 'CU020', 50, 18000.00), ('NH039', 'CU001', 30, 180000.00),
('NH040', 'CU002', 15, 250000.00), ('NH040', 'CU003', 25, 100000.00);

-- chèn dữ liệu cho: CongThucNauAn (MaCongThuc, MaCT, MaPhienBan, Gia)
-- Mỗi công thức liên kết một ChiTietMonAn với một PhienBanMonAn và có giá
INSERT INTO [dbo].[CongThucNauAn] ([MaCongThuc], [MaCT], [MaPhienBan], [Gia]) VALUES
('CT001', 'CT001', 'PB001', 30000), ('CT002', 'CT002', 'PB002', 30000),
('CT003', 'CT003', 'PB003', 35000), ('CT004', 'CT004', 'PB004', 35000),
('CT005', 'CT005', 'PB005', 40000), ('CT006', 'CT006', 'PB006', 250000),
('CT007', 'CT007', 'PB007', 230000), ('CT008', 'CT008', 'PB008', 280000),
('CT009', 'CT009', 'PB009', 300000), ('CT010', 'CT010', 'PB010', 220000),
('CT011', 'CT011', 'PB011', 25000), ('CT012', 'CT012', 'PB012', 30000),
('CT013', 'CT013', 'PB013', 25000), ('CT014', 'CT014', 'PB014', 40000),
('CT015', 'CT015', 'PB015', 35000), ('CT016', 'CT016', 'PB016', 10000),
('CT017', 'CT017', 'PB017', 30000), ('CT018', 'CT018', 'PB018', 40000),
('CT019', 'CT019', 'PB019', 30000), ('CT020', 'CT020', 'PB020', 25000),
('CT021', 'CT021', 'PB021', 150000), ('CT022', 'CT022', 'PB022', 220000),
('CT023', 'CT023', 'PB023', 200000), ('CT024', 'CT024', 'PB024', 180000),
('CT025', 'CT025', 'PB025', 200000), ('CT026', 'CT026', 'PB026', 60000),
('CT027', 'CT027', 'PB027', 40000), ('CT028', 'CT028', 'PB028', 45000),
('CT029', 'CT029', 'PB029', 55000), ('CT030', 'CT030', 'PB030', 50000),
('CT031', 'CT031', 'PB031', 75000), ('CT032', 'CT032', 'PB032', 70000),
('CT033', 'CT033', 'PB033', 85000), ('CT034', 'CT034', 'PB034', 75000),
('CT035', 'CT035', 'PB035', 80000), ('CT036', 'CT036', 'PB036', 450000),
('CT037', 'CT037', 'PB037', 300000), ('CT038', 'CT038', 'PB038', 280000),
('CT039', 'CT039', 'PB039', 180000), ('CT040', 'CT040', 'PB040', 250000);

-- chèn dữ liệu cho: ChiTietCongThuc (MaCongThuc, MaNguyenLieu, SoLuongCanDung)
-- Mỗi công thức có nhiều nguyên liệu
INSERT INTO [dbo].[ChiTietCongThuc] ([MaCongThuc], [MaNguyenLieu], [SoLuongCanDung]) VALUES
('CT001', 'NL021', 1), ('CT002', 'NL022', 1),
('CT003', 'NL023', 1), ('CT003', 'NL009', 1),
('CT004', 'NL024', 1), ('CT005', 'NL025', 1),
('CT005', 'NL024', 1), ('CT006', 'NL026', 1),
('CT006', 'NL002', 1), ('CT006', 'NL004', 1),
('CT006', 'NL039', 1), ('CT006', 'NL005', 2),
('CT006', 'NL006', 1), ('CT007', 'NL003', 1),
('CT007', 'NL027', 1), ('CT007', 'NL006', 1),
('CT008', 'NL001', 2), ('CT008', 'NL028', 1),
('CT008', 'NL005', 2), ('CT008', 'NL012', 1),
('CT009', 'NL004', 1), ('CT010', 'NL002', 1),
('CT011', 'NL013', 2), ('CT011', 'NL014', 1),
('CT012', 'NL029', 1), ('CT013', 'NL030', 1),
('CT016', 'NL031', 1), ('CT017', 'NL025', 1),
('CT018', 'NL032', 1), ('CT019', 'NL008', 1),
('CT020', 'NL033', 1), ('CT021', 'NL009', 1),
('CT022', 'NL034', 3), ('CT022', 'NL035', 1),
('CT023', 'NL002', 1), ('CT024', 'NL001', 1),
('CT025', 'NL003', 1), ('CT026', 'NL010', 1),
('CT026', 'NL007', 1), ('CT027', 'NL010', 2),
('CT028', 'NL005', 1), ('CT028', 'NL010', 1),
('CT029', 'NL006', 1), ('CT029', 'NL010', 1),
('CT030', 'NL006', 1), ('CT031', 'NL009', 1),
('CT031', 'NL007', 1), ('CT031', 'NL013', 1),
('CT032', 'NL007', 1), ('CT032', 'NL012', 1),
('CT033', 'NL007', 1), ('CT033', 'NL002', 1),
('CT033', 'NL039', 1), ('CT034', 'NL007', 1),
('CT034', 'NL003', 1), ('CT035', 'NL007', 1),
('CT035', 'NL001', 1), ('CT035', 'NL012', 1),
('CT036', 'NL036', 1), ('CT037', 'NL037', 1),
('CT038', 'NL038', 1), ('CT038', 'NL008', 1),
('CT039', 'NL039', 1), ('CT039', 'NL020', 1),
('CT040', 'NL040', 4), ('CT040', 'NL035', 1);

-- chèn dữ liệu cho: ChiTietDonHang (đã thêm MaCongThuc)
-- Mapping: PB001->CT001, PB002->CT002, ..., PB040->CT040
INSERT INTO [dbo].[ChiTietDonHang] ([MaDonHang], [MaPhienBan], [MaCongThuc], [SoLuong]) VALUES
('DH001', 'PB006', 'CT006', 1), ('DH001', 'PB003', 'CT003', 2), ('DH001', 'PB019', 'CT019', 5),
('DH002', 'PB008', 'CT008', 2), ('DH002', 'PB021', 'CT021', 1), ('DH002', 'PB001', 'CT001', 1),
('DH003', 'PB031', 'CT031', 4), ('DH003', 'PB020', 'CT020', 4),
('DH004', 'PB034', 'CT034', 2), ('DH004', 'PB016', 'CT016', 2),
('DH005', 'PB035', 'CT035', 2), ('DH005', 'PB027', 'CT027', 1), ('DH005', 'PB018', 'CT018', 4),
('DH006', 'PB036', 'CT036', 1), ('DH006', 'PB037', 'CT037', 1), ('DH006', 'PB040', 'CT040', 2), ('DH006', 'PB019', 'CT019', 10),
('DH007', 'PB023', 'CT023', 1), ('DH007', 'PB019', 'CT019', 4),
('DH008', 'PB005', 'CT005', 1), ('DH008', 'PB011', 'CT011', 2), ('DH008', 'PB017', 'CT017', 2),
('DH009', 'PB007', 'CT007', 1), ('DH009', 'PB024', 'CT024', 2), ('DH009', 'PB002', 'CT002', 1),
('DH010', 'PB009', 'CT009', 1), ('DH010', 'PB010', 'CT010', 1), ('DH010', 'PB022', 'CT022', 2),
('DH011', 'PB025', 'CT025', 2), ('DH011', 'PB018', 'CT018', 4),
('DH012', 'PB026', 'CT026', 3), ('DH012', 'PB028', 'CT028', 1),
('DH013', 'PB029', 'CT029', 2), ('DH013', 'PB030', 'CT030', 2), ('DH013', 'PB016', 'CT016', 6),
('DH014', 'PB031', 'CT031', 1), ('DH014', 'PB032', 'CT032', 1), ('DH014', 'PB020', 'CT020', 2),
('DH015', 'PB033', 'CT033', 1), ('DH015', 'PB034', 'CT034', 1),
('DH016', 'PB006', 'CT006', 3), ('DH016', 'PB008', 'CT008', 3), ('DH016', 'PB021', 'CT021', 5), ('DH016', 'PB019', 'CT019', 20),
('DH017', 'PB038', 'CT038', 2), ('DH017', 'PB039', 'CT039', 3), ('DH017', 'PB040', 'CT040', 3),
('DH018', 'PB031', 'CT031', 1),
('DH019', 'PB035', 'CT035', 3), ('DH019', 'PB016', 'CT016', 3),
('DH020', 'PB006', 'CT006', 1), ('DH020', 'PB024', 'CT024', 2), ('DH020', 'PB003', 'CT003', 2),
('DH021', 'PB007', 'CT007', 1), ('DH021', 'PB001', 'CT001', 1), ('DH021', 'PB019', 'CT019', 6),
('DH022', 'PB031', 'CT031', 2), ('DH022', 'PB034', 'CT034', 2), ('DH022', 'PB032', 'CT032', 2),
('DH023', 'PB035', 'CT035', 5), ('DH023', 'PB020', 'CT020', 5),
('DH024', 'PB009', 'CT009', 1), ('DH024', 'PB003', 'CT003', 2),
('DH025', 'PB036', 'CT036', 1), ('DH025', 'PB037', 'CT037', 2),
('DH026', 'PB022', 'CT022', 3), ('DH026', 'PB021', 'CT021', 2), ('DH026', 'PB018', 'CT018', 7),
('DH027', 'PB040', 'CT040', 4), ('DH027', 'PB039', 'CT039', 2), ('DH027', 'PB019', 'CT019', 10),
('DH028', 'PB031', 'CT031', 9), ('DH028', 'PB016', 'CT016', 9),
('DH029', 'PB030', 'CT030', 10), ('DH029', 'PB020', 'CT020', 10),
('DH030', 'PB023', 'CT023', 1), ('DH030', 'PB001', 'CT001', 1), ('DH030', 'PB019', 'CT019', 2),
('DH031', 'PB024', 'CT024', 2), ('DH031', 'PB018', 'CT018', 4),
('DH032', 'PB005', 'CT005', 1), ('DH032', 'PB011', 'CT011', 2),
('DH033', 'PB012', 'CT012', 1), ('DH033', 'PB013', 'CT013', 1),
('DH034', 'PB032', 'CT032', 5), ('DH034', 'PB033', 'CT033', 5), ('DH034', 'PB016', 'CT016', 10),
('DH035', 'PB031', 'CT031', 11), ('DH035', 'PB020', 'CT020', 11),
('DH036', 'PB006', 'CT006', 1), ('DH036', 'PB007', 'CT007', 1), ('DH036', 'PB019', 'CT019', 8),
('DH037', 'PB008', 'CT008', 2), ('DH037', 'PB021', 'CT021', 3), ('DH037', 'PB018', 'CT018', 8),
('DH038', 'PB034', 'CT034', 3), ('DH038', 'PB016', 'CT016', 3),
('DH039', 'PB035', 'CT035', 2), ('DH039', 'PB039', 'CT039', 1), ('DH039', 'PB020', 'CT020', 4),
('DH040', 'PB006', 'CT006', 1), ('DH040', 'PB022', 'CT022', 2), ('DH040', 'PB019', 'CT019', 5);

-- Cập nhật tổng tiền
PRINT N'--- Đang cập nhật TONGTIEN cho các phiếu NhapHang ---'
GO
UPDATE NhapHang
SET TongTien = ISNULL(T.Tong, 0)
FROM NhapHang P
LEFT JOIN (
    SELECT MaNhapHang, SUM(SoLuong * GiaNhap) AS Tong
    FROM ChiTietNhapHang
    GROUP BY MaNhapHang
) AS T ON P.MaNhapHang = T.MaNhapHang;
GO


INSERT INTO [dbo].[BanAnDonHang] ([MaDonHang], [MaBan]) VALUES
('DH001', 'B003'),
('DH002', 'B008'),
('DH003', 'B001'),
('DH004', 'B002'),
('DH005', 'B004'),
('DH006', 'B007'),
('DH007', 'B009'),
('DH008', 'B010'),
('DH009', 'B006'),
('DH010', 'B011'),
('DH011', 'B012'),
('DH012', 'B013'),
('DH013', 'B014'),
('DH014', 'B017'),
('DH015', 'B018'),
('DH016', 'B020'),
('DH017', 'B015'),
('DH018', 'B021'),
('DH019', 'B022'),
('DH020', 'B023'),
('DH021', 'B024'),
('DH022', 'B025'),
('DH023', 'B026'),
('DH024', 'B005'), -- Đơn hàng 024 (Bàn chính)
('DH025', 'B019'), -- Đơn hàng 025 (Bàn chính)
('DH026', 'B027'),
('DH027', 'B028'),
('DH028', 'B029'),
('DH029', 'B030'),
('DH030', 'B031'),
('DH031', 'B032'),
('DH032', 'B033'), -- Đơn hàng 032 (Bàn chính)
('DH033', 'B034'),
('DH034', 'B035'),
('DH035', 'B036'),
('DH036', 'B039'),
('DH037', 'B040'),
('DH038', 'B001'),
('DH039', 'B002'),
('DH040', 'B003'),
-- Bổ sung dữ liệu GHÉP BÀN (Minh họa mối quan hệ N:M)
('DH024', 'B006'), -- Ghép thêm Bàn 6 cho Đơn hàng 024
('DH025', 'B020'), -- Ghép thêm Bàn 20 cho Đơn hàng 025
('DH032', 'B035'); -- Ghép thêm Bàn 35 cho Đơn hàng 032
GO

-- Kích hoạt lại khóa ngoại
PRINT N'--- KÍCH HOẠT LẠI TẤT CẢ KHÓA NGOẠI ---'
EXEC sp_MSforeachtable 'ALTER TABLE ? WITH CHECK CHECK CONSTRAINT ALL'
GO

-- =============================================
-- TẠO STORED PROCEDURES
-- =============================================

-- Stored Procedure: Lấy doanh thu theo tháng
CREATE PROCEDURE [dbo].[GetDoanhThuTheoThang]
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
        COALESCE(SUM(CTDH.SoLuong * CTA.Gia), 0) AS DoanhThu
    FROM Thang T
    LEFT JOIN DonHang DH ON MONTH(DH.ThoiGianKetThuc) = T.Thang
                        AND YEAR(DH.ThoiGianKetThuc) = @Nam
                        AND DH.MaTrangThaiDonHang = 'DA_HOAN_THANH'
    LEFT JOIN ChiTietDonHang CTDH ON DH.MaDonHang = CTDH.MaDonHang
    LEFT JOIN CongThucNauAn CTA ON CTDH.MaCongThuc = CTA.MaCongThuc
    GROUP BY T.Thang
    ORDER BY T.Thang;
END;
GO


CREATE PROCEDURE [dbo].[LayHoaDon]
    @MaDonHang VARCHAR(50)
AS
BEGIN
    SELECT
        dh.MaDonHang,
        nv.HoTen AS 'TenNhanVien',
        -- THAY THẾ ba.TenBan BẰNG DANH SÁCH CÁC BÀN GHÉP
        STRING_AGG(ba.TenBan, ', ') WITHIN GROUP (ORDER BY ba.TenBan) AS 'DanhSachBan',
        kh.HoTen AS 'TenKhachHang',
        dh.TGNhanBan,
        ttdh.TenTrangThai AS 'TrangThaiDonHang',
        dh.TienDatCoc,
        ctdh.SoLuong,
        ma.TenMonAn,
        ctma.TenCT AS 'TenChiTietMonAn',
        pb.TenPhienBan,
        cta.Gia,
        (ctdh.SoLuong * cta.Gia) AS 'ThanhTien'
    FROM DonHang dh
    
    -- THÊM BẢNG TRUNG GIAN ĐỂ LẤY THÔNG TIN BÀN
    LEFT JOIN BanAnDonHang badh ON dh.MaDonHang = badh.MaDonHang
    LEFT JOIN BanAn ba ON badh.MaBan = ba.MaBan
    
    -- Giữ nguyên các JOIN khác
    JOIN ChiTietDonHang ctdh ON ctdh.MaDonHang = dh.MaDonHang
    JOIN CongThucNauAn cta ON ctdh.MaCongThuc = cta.MaCongThuc
    JOIN ChiTietMonAn ctma ON cta.MaCT = ctma.MaCT
    JOIN MonAn ma ON ctma.MaMonAn = ma.MaMonAn
    JOIN PhienBanMonAn pb ON ctdh.MaPhienBan = pb.MaPhienBan
    JOIN KhachHang kh ON kh.MaKhachHang = DH.MaKhachHang
    JOIN NhanVien nv ON nv.MaNhanVien = dh.MaNhanVien
    LEFT JOIN TrangThaiDonHang ttdh ON dh.MaTrangThaiDonHang = ttdh.MaTrangThai
    
    WHERE dh.MaDonHang = @MaDonHang
    
    -- CẦN GROUP BY TẤT CẢ CÁC CỘT KHÔNG DÙNG HÀM TỔNG HỢP (STRING_AGG)
    GROUP BY
        dh.MaDonHang, nv.HoTen, kh.HoTen, dh.TGNhanBan, ttdh.TenTrangThai, dh.TienDatCoc, 
        ctdh.SoLuong, ma.TenMonAn, ctma.TenCT, pb.TenPhienBan, cta.Gia, (ctdh.SoLuong * cta.Gia);
END;
GO

ALTER TABLE DonHang ADD TenNguoiNhan nvarchar(100) NULL;
ALTER TABLE DonHang ADD SDTNguoiNhan varchar(20) NULL;
ALTER TABLE DonHang ADD EmailNguoiNhan nvarchar(100) NULL;

--USE [master];
--GO

--IF DB_ID('QL_NhaHang_DoAn_Test2') IS NOT NULL
--BEGIN
--    ALTER DATABASE [QL_NhaHang_DoAn_Test2] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
--    DROP DATABASE [QL_NhaHang_DoAn_Test2];
--END
--GO

CREATE TABLE [dbo].[Tang](
    [MaTang] [varchar](25) NOT NULL,
    [TenTang] [nvarchar](50) NOT NULL,
CONSTRAINT [PK_Tang] PRIMARY KEY CLUSTERED ([MaTang] ASC)
);
GO


ALTER TABLE [dbo].[BanAn]
ADD [MaTang] [varchar](25) NULL,
[IsShow] [bit] NOT NULL DEFAULT(1);
GO


ALTER TABLE [dbo].[BanAn]
ADD CONSTRAINT [FK_BanAn_Tang]
FOREIGN KEY ([MaTang]) REFERENCES [dbo].[Tang]([MaTang]);
GO


ALTER TABLE [dbo].[MonAn]
ADD [IsShow] [bit] NOT NULL DEFAULT(1);
GO


INSERT INTO [dbo].[Tang] ([MaTang], [TenTang]) VALUES
('T001', N'Tầng trệt'),
('T002', N'Tầng 1'),
('T003', N'Tầng 2');
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

-- =============================================
-- THIẾT KẾ BẢNG MENU
-- =============================================

-- Bảng LoaiMenu: Phân loại menu (Menu Set, Menu Buffet, Menu theo ngày, Menu đặc biệt...)
CREATE TABLE [dbo].[LoaiMenu](
    [MaLoaiMenu] [varchar](25) NOT NULL,
    [TenLoaiMenu] [nvarchar](100) NOT NULL,
    [MoTa] [nvarchar](500) NULL,
CONSTRAINT [PK_LoaiMenu] PRIMARY KEY CLUSTERED ([MaLoaiMenu] ASC)
);
GO

-- Bảng TrangThaiMenu: Trạng thái menu
CREATE TABLE [dbo].[TrangThaiMenu](
    [MaTrangThai] [varchar](25) NOT NULL,
    [TenTrangThai] [nvarchar](50) NOT NULL,
CONSTRAINT [PK_TrangThaiMenu] PRIMARY KEY CLUSTERED ([MaTrangThai] ASC)
);
GO

-- Bảng Menu: Thông tin menu chính
CREATE TABLE [dbo].[Menu](
    [MaMenu] [varchar](25) NOT NULL,
    [TenMenu] [nvarchar](200) NOT NULL,
    [MaLoaiMenu] [varchar](25) NOT NULL,
    [MaTrangThai] [varchar](25) NOT NULL,
    [GiaMenu] [decimal](10, 2) NOT NULL, -- Giá menu (có thể giảm giá so với mua lẻ)
    [GiaGoc] [decimal](10, 2) NULL, -- Tổng giá gốc nếu mua lẻ từng món (để tính % giảm giá)
    [MoTa] [nvarchar](1000) NULL,
    [HinhAnh] [nvarchar](max) NULL,
    [NgayBatDau] [datetime] NULL, -- Ngày bắt đầu áp dụng menu
    [NgayKetThuc] [datetime] NULL, -- Ngày kết thúc áp dụng menu
    [IsShow] [bit] NOT NULL DEFAULT(1), -- Hiển thị trên app/website
    [ThuTu] [int] NULL, -- Thứ tự hiển thị
    [NgayTao] [datetime] NOT NULL DEFAULT(GETDATE()),
    [NgayCapNhat] [datetime] NULL,
CONSTRAINT [PK_Menu] PRIMARY KEY CLUSTERED ([MaMenu] ASC)
);
GO

-- Bảng ChiTietMenu: Chi tiết các món trong menu
CREATE TABLE [dbo].[ChiTietMenu](
    [MaChiTietMenu] [bigint] IDENTITY(1,1) NOT NULL,
    [MaMenu] [varchar](25) NOT NULL,
    [MaCongThuc] [varchar](25) NOT NULL, -- Liên kết với CongThucNauAn
    [SoLuong] [int] NOT NULL DEFAULT(1), -- Số lượng món trong menu (ví dụ: 1 phần cơm, 2 phần canh)
    [GhiChu] [nvarchar](500) NULL, -- Ghi chú đặc biệt cho món này trong menu
    [ThuTu] [int] NULL, -- Thứ tự hiển thị món trong menu
CONSTRAINT [PK_ChiTietMenu] PRIMARY KEY CLUSTERED ([MaChiTietMenu] ASC)
);
GO

-- Thêm các ràng buộc và giá trị mặc định

ALTER TABLE [dbo].[Menu] ADD CONSTRAINT [CK_Menu_GiaMenu] CHECK ([GiaMenu] >= 0);
GO
ALTER TABLE [dbo].[Menu] ADD CONSTRAINT [CK_Menu_GiaGoc] CHECK ([GiaGoc] IS NULL OR [GiaGoc] >= 0);
GO
ALTER TABLE [dbo].[ChiTietMenu] ADD CONSTRAINT [CK_ChiTietMenu_SoLuong] CHECK ([SoLuong] > 0);
GO

-- Thêm Foreign Keys
ALTER TABLE [dbo].[Menu] WITH CHECK ADD CONSTRAINT [FK_Menu_LoaiMenu] 
FOREIGN KEY([MaLoaiMenu]) REFERENCES [dbo].[LoaiMenu] ([MaLoaiMenu]);
GO

ALTER TABLE [dbo].[Menu] WITH CHECK ADD CONSTRAINT [FK_Menu_TrangThaiMenu] 
FOREIGN KEY([MaTrangThai]) REFERENCES [dbo].[TrangThaiMenu] ([MaTrangThai]);
GO

ALTER TABLE [dbo].[ChiTietMenu] WITH CHECK ADD CONSTRAINT [FK_ChiTietMenu_Menu] 
FOREIGN KEY([MaMenu]) REFERENCES [dbo].[Menu] ([MaMenu]) ON DELETE CASCADE;
GO

ALTER TABLE [dbo].[ChiTietMenu] WITH CHECK ADD CONSTRAINT [FK_ChiTietMenu_CongThucNauAn] 
FOREIGN KEY([MaCongThuc]) REFERENCES [dbo].[CongThucNauAn] ([MaCongThuc]);
GO

-- Tạo Index để tối ưu truy vấn
CREATE INDEX [IX_Menu_MaLoaiMenu] ON [dbo].[Menu]([MaLoaiMenu]);
GO
CREATE INDEX [IX_Menu_MaTrangThai] ON [dbo].[Menu]([MaTrangThai]);
GO
CREATE INDEX [IX_Menu_IsShow] ON [dbo].[Menu]([IsShow]);
GO
CREATE INDEX [IX_ChiTietMenu_MaMenu] ON [dbo].[ChiTietMenu]([MaMenu]);
GO
CREATE INDEX [IX_ChiTietMenu_MaCongThuc] ON [dbo].[ChiTietMenu]([MaCongThuc]);
GO

-- =============================================
-- CHÈN DỮ LIỆU MẪU CHO MENU
-- =============================================

-- Chèn dữ liệu cho LoaiMenu
INSERT INTO [dbo].[LoaiMenu] ([MaLoaiMenu], [TenLoaiMenu], [MoTa]) VALUES
('LM001', N'Menu Set', N'Menu combo gồm nhiều món với giá ưu đãi'),
('LM002', N'Menu Buffet', N'Menu buffet ăn thỏa thích'),
('LM003', N'Menu theo ngày', N'Menu đặc biệt theo từng ngày trong tuần'),
('LM004', N'Menu sự kiện', N'Menu đặc biệt cho các dịp lễ, sự kiện'),
('LM005', N'Menu gia đình', N'Menu dành cho gia đình, nhóm đông người'),
('LM006', N'Menu tiệc', N'Menu dành cho tiệc, hội nghị');
GO

-- Chèn dữ liệu cho TrangThaiMenu
INSERT INTO [dbo].[TrangThaiMenu] ([MaTrangThai], [TenTrangThai]) VALUES
('DANG_AP_DUNG', N'Đang áp dụng'),
('HET_HAN', N'Hết hạn'),
('TAM_NGUNG', N'Tạm ngưng'),
('CHUA_AP_DUNG', N'Chưa áp dụng');
GO

-- Chèn dữ liệu mẫu cho Menu
-- Menu Set A: Cơm + Canh + Món mặn + Nước
INSERT INTO [dbo].[Menu] ([MaMenu], [TenMenu], [MaLoaiMenu], [MaTrangThai], [GiaMenu], [GiaGoc], [MoTa], [HinhAnh], [NgayBatDau], [NgayKetThuc], [IsShow], [ThuTu]) VALUES
('MENU001', N'Menu Set A - Cơm tấm combo', 'LM001', 'DANG_AP_DUNG', 120000, 150000, N'Bao gồm: 1 phần cơm tấm sườn bì chả + 1 canh chua chay + 1 nước lọc', NULL, '2025-01-01', NULL, 1, 1),
('MENU002', N'Menu Set B - Lẩu combo 2 người', 'LM001', 'DANG_AP_DUNG', 450000, 500000, N'Bao gồm: 1 lẩu Thái hải sản + 2 phần cơm + 2 nước', NULL, '2025-01-01', NULL, 1, 2),
('MENU003', N'Menu Set C - Hải sản combo', 'LM001', 'DANG_AP_DUNG', 600000, 700000, N'Bao gồm: 1 tôm hùm nướng bơ tỏi + 1 cua rang me + 2 nước', NULL, '2025-01-01', NULL, 1, 3),
('MENU004', N'Menu Buffet trưa', 'LM002', 'DANG_AP_DUNG', 250000, NULL, N'Buffet trưa thứ 2-6, từ 11h-14h', NULL, '2025-01-01', NULL, 1, 4),
('MENU005', N'Menu gia đình 4 người', 'LM005', 'DANG_AP_DUNG', 800000, 950000, N'Menu đầy đủ cho gia đình 4 người: 4 phần cơm + 2 món mặn + 1 canh + 4 nước', NULL, '2025-01-01', NULL, 1, 5),
('MENU006', N'Menu Tết Nguyên Đán 2025', 'LM004', 'CHUA_AP_DUNG', 1200000, 1400000, N'Menu đặc biệt dịp Tết, áp dụng từ 28/12 - 5/1', NULL, '2025-12-28', '2026-01-05', 1, 6);
GO

-- Chèn dữ liệu chi tiết cho Menu Set A
INSERT INTO [dbo].[ChiTietMenu] ([MaMenu], [MaCongThuc], [SoLuong], [GhiChu], [ThuTu]) VALUES
('MENU001', 'CT031', 1, N'Cơm tấm sườn bì chả', 1),
('MENU001', 'CT028', 1, N'Canh chua chay', 2),
('MENU001', 'CT016', 1, N'Nước lọc', 3);
GO

-- Chèn dữ liệu chi tiết cho Menu Set B
INSERT INTO [dbo].[ChiTietMenu] ([MaMenu], [MaCongThuc], [SoLuong], [GhiChu], [ThuTu]) VALUES
('MENU002', 'CT006', 1, N'Lẩu Thái hải sản', 1),
('MENU002', 'CT031', 2, N'Cơm tấm (2 phần)', 2),
('MENU002', 'CT016', 2, N'Nước lọc (2 chai)', 3);
GO

-- Chèn dữ liệu chi tiết cho Menu Set C
INSERT INTO [dbo].[ChiTietMenu] ([MaMenu], [MaCongThuc], [SoLuong], [GhiChu], [ThuTu]) VALUES
('MENU003', 'CT036', 1, N'Tôm hùm nướng bơ tỏi', 1),
('MENU003', 'CT037', 1, N'Cua rang me', 2),
('MENU003', 'CT016', 2, N'Nước lọc (2 chai)', 3);
GO

-- Chèn dữ liệu chi tiết cho Menu gia đình 4 người
INSERT INTO [dbo].[ChiTietMenu] ([MaMenu], [MaCongThuc], [SoLuong], [GhiChu], [ThuTu]) VALUES
('MENU005', 'CT031', 4, N'Cơm tấm (4 phần)', 1),
('MENU005', 'CT021', 1, N'Sườn nướng BBQ', 2),
('MENU005', 'CT024', 1, N'Ba chỉ bò nướng', 3),
('MENU005', 'CT028', 1, N'Canh chua chay', 4),
('MENU005', 'CT016', 4, N'Nước lọc (4 chai)', 5);
GO

-- =============================================
-- STORED PROCEDURE: Lấy danh sách menu đang áp dụng
-- =============================================
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

-- =============================================
-- STORED PROCEDURE: Lấy chi tiết menu
-- =============================================
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