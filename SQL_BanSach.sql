
-- TẠO BẢNG CHỨC VỤ
CREATE TABLE ChucVu (
    MaCV NVARCHAR(50) PRIMARY KEY,
    TenCV NVARCHAR(50)
);

-- TẠO BẢNG NHÂN VIÊN
CREATE TABLE NhanVien (
    MaNhanVien NVARCHAR(50) PRIMARY KEY,
    HoTen NVARCHAR(100),
    SoDienThoai VARCHAR(10),
    Email VARCHAR(255),
    TenTK NVARCHAR(50),
    MatKhau VARCHAR(50),
    MaCV NVARCHAR(50),
    FOREIGN KEY (MaCV) REFERENCES ChucVu(MaCV)
);

-- TẠO BẢNG KHÁCH HÀNG
CREATE TABLE KhachHang (
    MaKH NVARCHAR(50) PRIMARY KEY,
    HoTen NVARCHAR(100),
    SoDienThoai VARCHAR(15),
    MatKhau VARCHAR(50),
    Email VARCHAR(100),
    DiaChi NVARCHAR(255),
    NgaySinh DATE
);

-- TẠO BẢNG NHÀ XUẤT BẢN
CREATE TABLE NhaXuatBan (
    MaNXB NVARCHAR(50) PRIMARY KEY,
    TenNXB NVARCHAR(255),
    DiaChi NVARCHAR(255)
);

-- TẠO BẢNG LOẠI SÁCH
CREATE TABLE Loai (
    MaLoai NVARCHAR(50) PRIMARY KEY,
    TenLoai NVARCHAR(100),
	Status int
);

-- TẠO BẢNG SÁCH
CREATE TABLE Sach (
    MaSach NVARCHAR(50) PRIMARY KEY,
    TenSach NVARCHAR(255),
    Hinh NVARCHAR(255),
    GiaBan DECIMAL(10,2),
    MoTa NVARCHAR(1000),
    MaNXB NVARCHAR(50),
    NgayNhapHang DATE,
    SoLuongTon INT,
    MaLoai NVARCHAR(50),
	Status int,
    FOREIGN KEY (MaNXB) REFERENCES NhaXuatBan(MaNXB),
    FOREIGN KEY (MaLoai) REFERENCES Loai(MaLoai)
);

-- TẠO BẢNG TÁC GIẢ
CREATE TABLE TacGia (
    MaTG NVARCHAR(50) PRIMARY KEY,
    TenTG NVARCHAR(255),
    DiaChi NVARCHAR(255),
    TieuSu NVARCHAR(1000),
    DienThoai VARCHAR(15),
	Status int
);

-- TẠO BẢNG VIẾT SÁCH (LIÊN KẾT SÁCH VỚI TÁC GIẢ)
CREATE TABLE VietSach (
    MaTG NVARCHAR(50),
    MaSach NVARCHAR(50),
    VaiTro NVARCHAR(100),
    PRIMARY KEY (MaTG, MaSach),
    FOREIGN KEY (MaTG) REFERENCES TacGia(MaTG),
    FOREIGN KEY (MaSach) REFERENCES Sach(MaSach)
);

-- TẠO BẢNG ĐƠN ĐẶT HÀNG
CREATE TABLE DonDatHang (
    MaDonHang NVARCHAR(50) PRIMARY KEY,
    HoTen NVARCHAR(100),
    SoDienThoai VARCHAR(15),
	 Email VARCHAR(100),
    DiaChi NVARCHAR(255),
    NgayDat DATE,
    TrangThai NVARCHAR(50),
	PhuongThucThanhToan int,
);

-- TẠO BẢNG CHI TIẾT ĐƠN HÀNG
CREATE TABLE ChiTietDonHang (
    MaDonHang NVARCHAR(50),
    MaSach NVARCHAR(50),
    SoLuong INT,
    DonGia DECIMAL(10,2),
    PRIMARY KEY (MaDonHang, MaSach),
    FOREIGN KEY (MaDonHang) REFERENCES DonDatHang(MaDonHang),
    FOREIGN KEY (MaSach) REFERENCES Sach(MaSach)
);
-- Tạo bảng giỏ hàng
CREATE TABLE GioHang (
    MaGioHang NVARCHAR(50) PRIMARY KEY,
    MaKH NVARCHAR(50),
    NgayTao DATETIME ,
    FOREIGN KEY (MaKH) REFERENCES KhachHang(MaKH)
);

-- tạo bảng chi tiết giỏ hàng
CREATE TABLE ChiTietGioHang (
    MaChiTiet NVARCHAR(50) PRIMARY KEY,
    MaGioHang NVARCHAR(50),
    MaSach NVARCHAR(50),
    SoLuong INT,
    DonGia DECIMAL(18, 2),
    FOREIGN KEY (MaGioHang) REFERENCES GioHang(MaGioHang),
    FOREIGN KEY (MaSach) REFERENCES Sach(MaSach)
);