-- ============================================================
-- SPARK SNEAKER SHOP - SEED DATA SCRIPT
-- Chạy script này trong SQL Server Management Studio (SSMS)
-- Database: SneakerShopDB
-- Tất cả ảnh đã có sẵn trong wwwroot/images/
-- ============================================================

USE SneakerShopDB;
GO

-- ============================================================
-- BƯỚC 1: Thêm Sizes còn thiếu
-- ============================================================
IF NOT EXISTS (SELECT 1 FROM Sizes WHERE Id = 5)
    INSERT INTO Sizes (Id, Number) VALUES (5, 44);
IF NOT EXISTS (SELECT 1 FROM Sizes WHERE Id = 6)
    INSERT INTO Sizes (Id, Number) VALUES (6, 45);
GO

-- ============================================================
-- BƯỚC 2: Thêm Colors còn thiếu
-- ============================================================
IF NOT EXISTS (SELECT 1 FROM Colors WHERE Name = 'Dark Gray')
    INSERT INTO Colors (Name) VALUES ('Dark Gray');
IF NOT EXISTS (SELECT 1 FROM Colors WHERE Name = 'Gold')
    INSERT INTO Colors (Name) VALUES ('Gold');
IF NOT EXISTS (SELECT 1 FROM Colors WHERE Name = 'Obsidian')
    INSERT INTO Colors (Name) VALUES ('Obsidian');
GO

-- ============================================================
-- BƯỚC 3: Thêm Category còn thiếu
-- ============================================================
IF NOT EXISTS (SELECT 1 FROM Category WHERE Name = 'Training')
    INSERT INTO Category (Name, Description) VALUES ('Training', 'Training and gym shoes');
IF NOT EXISTS (SELECT 1 FROM Category WHERE Name = 'Lifestyle')
    INSERT INTO Category (Name, Description) VALUES ('Lifestyle', 'Lifestyle and street fashion');
GO

-- ============================================================
-- BƯỚC 4: Thêm images + variants cho sản phẩm gốc (Id=1, Id=2)
-- ============================================================

-- Thêm images cho Product 1 (Nike Air Force 1)
IF NOT EXISTS (SELECT 1 FROM ProductImages WHERE ProductId = 1 AND ImageUrl = 'Nike/af1-1.jpg')
    INSERT INTO ProductImages (ProductId, ImageUrl) VALUES (1, 'Nike/af1-1.jpg');

-- Thêm variants cho Product 1 nếu chưa có
IF NOT EXISTS (SELECT 1 FROM ProductVariants WHERE ProductId = 1)
BEGIN
    DECLARE @W1 INT = (SELECT Id FROM Colors WHERE Name = 'White');
    DECLARE @B1 INT = (SELECT Id FROM Colors WHERE Name = 'Black');
    INSERT INTO ProductVariants (ProductId, SizeId, ColorId, Stock) VALUES
        (1, 1, @W1, 20), (1, 2, @W1, 25), (1, 3, @W1, 18), (1, 4, @W1, 15),
        (1, 1, @B1, 10), (1, 2, @B1, 14), (1, 3, @B1, 12), (1, 4, @B1, 8);
END
GO

-- Thêm images cho Product 2 (Adidas Run Falcon 5)
IF NOT EXISTS (SELECT 1 FROM ProductImages WHERE ProductId = 2 AND ImageUrl = 'Adidas/Adidas Mens Run Falcon 5 R.jpg')
    INSERT INTO ProductImages (ProductId, ImageUrl) VALUES
        (2, 'Adidas/Adidas Mens Run Falcon 5 R.jpg'),
        (2, 'Adidas/Adidas Mens Run Falcon 5 OL.jpg'),
        (2, 'Adidas/Adidas Mens Run Falcon 5 W.jpg');

-- Thêm variants cho Product 2 nếu chưa có
IF NOT EXISTS (SELECT 1 FROM ProductVariants WHERE ProductId = 2)
BEGIN
    DECLARE @W2 INT = (SELECT Id FROM Colors WHERE Name = 'White');
    DECLARE @B2 INT = (SELECT Id FROM Colors WHERE Name = 'Black');
    DECLARE @G2 INT = (SELECT Id FROM Colors WHERE Name = 'Grey');
    INSERT INTO ProductVariants (ProductId, SizeId, ColorId, Stock) VALUES
        (2, 1, @W2, 20), (2, 2, @W2, 25), (2, 3, @W2, 18), (2, 4, @W2, 15),
        (2, 1, @B2, 12), (2, 2, @B2, 18), (2, 3, @B2, 16),
        (2, 1, @G2, 10), (2, 2, @G2, 14), (2, 3, @G2, 12);
END
GO

-- ============================================================
-- BƯỚC 5: Thêm sản phẩm Nike Air Max Torch 4 - Dark Gray
-- Ảnh: Nike/Nike Men's Air Max Torch 4 Running Shoe Dark Gray.jpg
-- ============================================================
IF NOT EXISTS (SELECT 1 FROM Products WHERE Name = 'Nike Air Max Torch 4 Dark Gray')
BEGIN
    DECLARE @CatRun INT = (SELECT Id FROM Category WHERE Name = 'Running');
    DECLARE @DG INT = (SELECT Id FROM Colors WHERE Name = 'Dark Gray');

    INSERT INTO Products (Name, Brand, CategoryId, Price, Description, MainImage, IsActive, SoldCount, Rating)
    VALUES (
        'Nike Air Max Torch 4 Dark Gray',
        'Nike',
        @CatRun,
        2850000,
        N'Nike Air Max Torch 4 với đệm khí Max Air giúp giảm chấn tối đa. Thiết kế năng động màu Dark Gray phù hợp chạy bộ và tập gym hàng ngày.',
        'Nike/Nike Men''s Air Max Torch 4 Running Shoe Dark Gray.jpg',
        1, 87, 4.5
    );

    DECLARE @P3 INT = SCOPE_IDENTITY();

    INSERT INTO ProductImages (ProductId, ImageUrl) VALUES
        (@P3, 'Nike/Nike Men''s Air Max Torch 4 Running Shoe Dark Gray 1.jpg'),
        (@P3, 'Nike/Nike Men''s Air Max Torch 4 Running Shoe Dark Gray 2.jpg');

    INSERT INTO ProductVariants (ProductId, SizeId, ColorId, Stock) VALUES
        (@P3, 1, @DG, 10), (@P3, 2, @DG, 15), (@P3, 3, @DG, 8), (@P3, 4, @DG, 12);
END
GO

-- ============================================================
-- BƯỚC 6: Nike Air Max Torch 4 - Metallic Gold
-- Ảnh: Nike/Nike Men's Air Max Torch 4 Running Shoe Dark Gray metalic gold.jpg
-- ============================================================
IF NOT EXISTS (SELECT 1 FROM Products WHERE Name = 'Nike Air Max Torch 4 Metallic Gold')
BEGIN
    DECLARE @CatRun2 INT = (SELECT Id FROM Category WHERE Name = 'Running');
    DECLARE @Gold INT = (SELECT Id FROM Colors WHERE Name = 'Gold');
    DECLARE @DG2 INT  = (SELECT Id FROM Colors WHERE Name = 'Dark Gray');

    INSERT INTO Products (Name, Brand, CategoryId, Price, Description, MainImage, IsActive, SoldCount, Rating)
    VALUES (
        'Nike Air Max Torch 4 Metallic Gold',
        'Nike',
        @CatRun2,
        3200000,
        N'Phiên bản đặc biệt của Nike Air Max Torch 4 với điểm nhấn Metallic Gold sang trọng. Công nghệ Max Air tối ưu hiệu suất vận động.',
        'Nike/Nike Men''s Air Max Torch 4 Running Shoe Dark Gray metalic gold.jpg',
        1, 54, 4.7
    );

    DECLARE @P4 INT = SCOPE_IDENTITY();

    INSERT INTO ProductImages (ProductId, ImageUrl) VALUES
        (@P4, 'Nike/Nike Men''s Air Max Torch 4 Running Shoe Dark Gray metalic gold 1.jpg'),
        (@P4, 'Nike/Nike Men''s Air Max Torch 4 Running Shoe Dark Gray metalic gold 2.jpg');

    INSERT INTO ProductVariants (ProductId, SizeId, ColorId, Stock) VALUES
        (@P4, 2, @Gold, 5), (@P4, 3, @Gold, 8), (@P4, 4, @Gold, 6),
        (@P4, 2, @DG2, 10), (@P4, 3, @DG2, 12);
END
GO

-- ============================================================
-- BƯỚC 7: Nike Flex Control TR3 (Training)
-- Ảnh có trong folder Adidas/ (nhầm folder nhưng file đã có sẵn)
-- ============================================================
IF NOT EXISTS (SELECT 1 FROM Products WHERE Name = 'Nike Flex Control TR3')
BEGIN
    DECLARE @CatTrain INT = (SELECT Id FROM Category WHERE Name = 'Training');
    DECLARE @BlackFC INT = (SELECT Id FROM Colors WHERE Name = 'Black');
    DECLARE @WhiteFC INT = (SELECT Id FROM Colors WHERE Name = 'White');
    DECLARE @ObsFC   INT = (SELECT Id FROM Colors WHERE Name = 'Obsidian');

    INSERT INTO Products (Name, Brand, CategoryId, Price, Description, MainImage, IsActive, SoldCount, Rating)
    VALUES (
        'Nike Flex Control TR3',
        'Nike',
        @CatTrain,
        2400000,
        N'Nike Flex Control TR3 thiết kế riêng cho tập gym cường độ cao. Đế multi-directional flex linh hoạt, mid-foot lockdown ổn định hoàn hảo.',
        'Adidas/Nike Men''s Flex Control TR3 Sneaker.jpg',
        1, 133, 4.6
    );

    DECLARE @P5 INT = SCOPE_IDENTITY();

    INSERT INTO ProductImages (ProductId, ImageUrl) VALUES
        (@P5, 'Adidas/Nike Men''s Flex Control TR3 Sneaker 1.jpg'),
        (@P5, 'Adidas/Nike Men''s Flex Control TR3 Sneaker white.jpg'),
        (@P5, 'Adidas/Nike Men''s Flex Control TR3 Sneaker white 1.jpg'),
        (@P5, 'Adidas/Nike Men''s Flex Control TR3 Sneaker white 2.jpg'),
        (@P5, 'Adidas/Nike Men''s Flex Control TR3 Sneaker obsidian.jpg'),
        (@P5, 'Adidas/Nike Men''s Flex Control TR3 Sneaker obsidian 1.jpg'),
        (@P5, 'Adidas/Nike Men''s Flex Control TR3 Sneaker obsidian 2.jpg');

    INSERT INTO ProductVariants (ProductId, SizeId, ColorId, Stock) VALUES
        (@P5, 1, @BlackFC, 15), (@P5, 2, @BlackFC, 20), (@P5, 3, @BlackFC, 18), (@P5, 4, @BlackFC, 10),
        (@P5, 1, @WhiteFC, 12), (@P5, 2, @WhiteFC, 16), (@P5, 3, @WhiteFC, 14),
        (@P5, 1, @ObsFC, 8),  (@P5, 2, @ObsFC, 11), (@P5, 3, @ObsFC, 9);
END
GO

-- ============================================================
-- BƯỚC 8: Adidas Lite Racer Adapt 7.0 (Casual)
-- Ảnh: Adidas/adidas Mens Lite Racer Adapt 7.0 Slip On Sneakers Shoes Casual - Grey.jpg
-- ============================================================
IF NOT EXISTS (SELECT 1 FROM Products WHERE Name = 'Adidas Lite Racer Adapt 7.0')
BEGIN
    DECLARE @CatCas2 INT = (SELECT Id FROM Category WHERE Name = 'Casual');
    DECLARE @GreyLR INT  = (SELECT Id FROM Colors WHERE Name = 'Grey');

    INSERT INTO Products (Name, Brand, CategoryId, Price, Description, MainImage, IsActive, SoldCount, Rating)
    VALUES (
        'Adidas Lite Racer Adapt 7.0',
        'Adidas',
        @CatCas2,
        1890000,
        N'Adidas Lite Racer Adapt 7.0 slip-on tiện lợi, không cần buộc dây. Đế CloudFoam siêu êm, lý tưởng cho những ngày dài đứng hoặc đi bộ.',
        'Adidas/adidas Mens Lite Racer Adapt 7.0 Slip On Sneakers Shoes Casual - Grey.jpg',
        1, 201, 4.4
    );

    DECLARE @P7 INT = SCOPE_IDENTITY();

    INSERT INTO ProductImages (ProductId, ImageUrl) VALUES
        (@P7, 'Adidas/adidas Mens Lite Racer Adapt 7.0 Slip On Sneakers Shoes Casual - Grey 1.jpg');

    INSERT INTO ProductVariants (ProductId, SizeId, ColorId, Stock) VALUES
        (@P7, 1, @GreyLR, 18), (@P7, 2, @GreyLR, 22), (@P7, 3, @GreyLR, 20), (@P7, 4, @GreyLR, 14);
END
GO

-- ============================================================
-- BƯỚC 9: Adidas Superstar Classic (Lifestyle)
-- Ảnh: Adidas/adidas-superstar.jpg
-- ============================================================
IF NOT EXISTS (SELECT 1 FROM Products WHERE Name = 'Adidas Superstar Classic')
BEGIN
    DECLARE @CatLife INT = (SELECT Id FROM Category WHERE Name = 'Lifestyle');
    DECLARE @WhiteSS INT = (SELECT Id FROM Colors WHERE Name = 'White');
    DECLARE @BlackSS INT = (SELECT Id FROM Colors WHERE Name = 'Black');

    INSERT INTO Products (Name, Brand, CategoryId, Price, Description, MainImage, IsActive, SoldCount, Rating)
    VALUES (
        'Adidas Superstar Classic',
        'Adidas',
        @CatLife,
        2700000,
        N'Huyền thoại Adidas Superstar với shell toe đặc trưng ra mắt năm 1969. Phong cách streetwear vượt thời gian, phối đồ cực kỳ linh hoạt.',
        'Adidas/adidas-superstar.jpg',
        1, 312, 4.8
    );

    DECLARE @P8 INT = SCOPE_IDENTITY();

    INSERT INTO ProductVariants (ProductId, SizeId, ColorId, Stock) VALUES
        (@P8, 1, @WhiteSS, 25), (@P8, 2, @WhiteSS, 30), (@P8, 3, @WhiteSS, 22), (@P8, 4, @WhiteSS, 18),
        (@P8, 1, @BlackSS, 15), (@P8, 2, @BlackSS, 20), (@P8, 3, @BlackSS, 17), (@P8, 4, @BlackSS, 12);
END
GO

-- ============================================================
-- BƯỚC 10: PUMA Tazon 6 FM Black (Running)
-- Ảnh: Puma/PUMA Men's Tazon 6 FM Shoes black.jpg
-- ============================================================
IF NOT EXISTS (SELECT 1 FROM Products WHERE Name = 'PUMA Tazon 6 FM')
BEGIN
    DECLARE @CatRun10 INT = (SELECT Id FROM Category WHERE Name = 'Running');
    DECLARE @BlackPuma INT = (SELECT Id FROM Colors WHERE Name = 'Black');

    INSERT INTO Products (Name, Brand, CategoryId, Price, Description, MainImage, IsActive, SoldCount, Rating)
    VALUES (
        'PUMA Tazon 6 FM',
        'Puma',
        @CatRun10,
        1650000,
        N'PUMA Tazon 6 FM với EcoOrthoLite insole thân thiện môi trường giảm áp lực chân. Thiết kế classic màu đen phù hợp đi làm và chạy nhẹ.',
        'Puma/PUMA Men''s Tazon 6 FM Shoes black.jpg',
        1, 168, 4.3
    );

    DECLARE @P9b INT = SCOPE_IDENTITY();

    INSERT INTO ProductImages (ProductId, ImageUrl) VALUES
        (@P9b, 'Puma/PUMA Men''s Tazon 6 FM Shoes black 1.jpg'),
        (@P9b, 'Puma/PUMA Men''s Tazon 6 FM Shoes black 2.jpg');

    INSERT INTO ProductVariants (ProductId, SizeId, ColorId, Stock) VALUES
        (@P9b, 1, @BlackPuma, 20), (@P9b, 2, @BlackPuma, 25),
        (@P9b, 3, @BlackPuma, 22), (@P9b, 4, @BlackPuma, 16);
END
GO

-- ============================================================
-- KIỂM TRA KẾT QUẢ - chạy câu này để xác nhận
-- ============================================================
SELECT 
    p.Id,
    p.Name,
    p.Brand,
    CAST(p.Price AS VARCHAR) AS Price,
    p.MainImage,
    COUNT(DISTINCT pi.Id) AS TotalImages,
    COUNT(DISTINCT pv.Id) AS TotalVariants
FROM Products p
LEFT JOIN ProductImages pi ON pi.ProductId = p.Id
LEFT JOIN ProductVariants pv ON pv.ProductId = p.Id
GROUP BY p.Id, p.Name, p.Brand, p.Price, p.MainImage
ORDER BY p.Brand, p.Name;
GO
