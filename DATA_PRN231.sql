-- Tạo database
CREATE DATABASE BookStore

-- Sử dụng database
USE BookStore;

-- Tạo bảng Danh mục (Category)
CREATE TABLE Categories (
    CategoryID INT IDENTITY(1,1) PRIMARY KEY,
    CategoryName NVARCHAR(50) NOT NULL
);

-- Tạo bảng Sách
CREATE TABLE Books (
    BookID INT IDENTITY(1,1) PRIMARY KEY,
    Title NVARCHAR(255) NOT NULL,
    Author NVARCHAR(255) NOT NULL,
	ImageUrl VARCHAR(255) NOT NULL,
    CategoryID INT NOT NULL,
    Price DECIMAL(10, 2) NOT NULL,
	[Description] NVARCHAR(100) NOT NULL
    FOREIGN KEY (CategoryID) REFERENCES Categories(CategoryID)
);

-- Tạo bảng Khách hàng
CREATE TABLE Users (
    UserID INT IDENTITY(1,1) PRIMARY KEY,
    FullName NVARCHAR(50) NOT NULL,
    Email VARCHAR(100) NOT NULL,
	[phoneNumber] [varchar](50) NOT NULL,
	[Address] NVARCHAR(50) NOT NULL,
	[Password] VARCHAR(100) NOT NULL,
	[Role] VARCHAR(50) NOT NULL
);

-- Tạo bảng Đơn đặt hàng
CREATE TABLE Orders (
    OrderID INT IDENTITY(1,1) PRIMARY KEY,
    UserID INT not null,
    OrderDate date not null,
	[RequiredDate] [date] NULL,
	[ShippedDate] [date] NOT NULL,
	[Freight] [int] NULL
    FOREIGN KEY (UserID) REFERENCES Users(UserID)
);

-- Tạo bảng Chi tiết đơn đặt hàng
CREATE TABLE OrderDetails (
    OrderDetailID INT IDENTITY(1,1) PRIMARY KEY,
    OrderID INT NOT NULL,
    BookID INT NOT NULL,
	UnitPrice INT NOT NULL,
    Quantity INT NOT NULL
    FOREIGN KEY (OrderID) REFERENCES Orders(OrderID),
    FOREIGN KEY (BookID) REFERENCES Books(BookID)
);


INSERT INTO Categories
    (CategoryName)
VALUES
    (N'Kinh Dị'),
	(N'Ngôn Tình'),
	(N'Trinh Thám'),
	(N'Hành Động'),
	(N'Phiêu Lưu');

INSERT INTO Books
    (Title, Author, ImageUrl, CategoryID, Price, [Description])
VALUES
    (N'Ba Hồi Kinh Dị',N'Thế Lữ',N'8935244877212.jpg',1,33.600,N'Nhà Xuất Bản Kim Đồng'),
	(N'Vàng Và Máu',N'Thế Lữ',N'vang-va-mau.jpg',1,38.500,N'Nhà Xuất Bản Kim Đồng'),
	(N'Tết ở làng Địa Ngục',N'Thảo Trang',N'8935212358279.jpg',1,126.750,N'Nhà Xuất Bản Thanh Niên'),
	(N'Bến Xe',N'Thương Thái Vi',N'8935212349208_1.jpg',2,50.920,N'Nhà Xuất Bản Văn Học'),
	(N'Thất Tịch Không Mưa',N'Lâu Vũ Tinh',N'that-tich-khong-mua-bia.jpg',2,79.000,N'Nhà Xuất Bản Công An Nhân Dân'),
	(N'Sherlock Holmes - Toàn Tập',N'Sir Artnur Conan Doyle',N'image_195509_1_10840.jpg',3,244.550,N'Nhà Xuất Bản Hồng Đức'),
	(N'Detective Conan 100+ Plus SDB',N'Gosho Aoyama',N'9784098510993.jpg',3,289.750,N'Nhà Xuất Bản Shogakukan'),
	(N'Tập 1: Buma, Goku Và 7 Viên Ngọc Rồng - Tặng Kèm Postcard Hai Mặt',N'Akira Toriyama',N'dragon_ball_sd_7_vien_ngoc_rong_nhi_bia_tap_1.jpg',4,71.250,N'Nhà Xuất Bản Kim Đồng'),
	(N'One Piece 107',N'Eiichiro Oda',N'9784088837857.jpg',4,143.100,N'Nhà Xuất Bản Shueisha'),
	(N'Doraemon Hoạt Hình Màu - Tập 1',N'Fujiko F Fujio',N'2021_01_13_10_06_18_1-390x510.jpg',5,30.800,N'Nhà Xuất Bản Kim Đồng');

INSERT INTO Users
	(FullName, Email, phoneNumber, [Address], [Password], [Role])
VALUES
	(N'Vương Đắc Khiển',N'khienvd@gmail.com',N'0961186671',N'Quốc Oai',123,N'user'),
	(N'Nguyễn Hữu Hoa',N'hoanh472001@gmail.com',N'0961186670',N'Hà Nội',123,N'admin');