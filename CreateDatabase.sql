USE master;
GO

IF EXISTS (SELECT name FROM sys.databases WHERE name = 'PartnersDB')
BEGIN
    ALTER DATABASE PartnersDB SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE PartnersDB;
END
GO

CREATE DATABASE PartnersDB;
GO

USE PartnersDB;
GO

-- =============================================
-- СОЗДАНИЕ ТАБЛИЦ
-- =============================================

-- 1. Таблица типов партнеров
CREATE TABLE PartnerTypes (
    TypeID int IDENTITY(1,1) PRIMARY KEY,
    TypeName nvarchar(50) NOT NULL
);

-- 2. Таблица партнеров
CREATE TABLE Partners (
    PartnerID int IDENTITY(1,1) PRIMARY KEY,
    PartnerName nvarchar(100) NOT NULL,
    PartnerTypeID int NOT NULL,
    DirectorName nvarchar(100),
    Phone nvarchar(20),
    Email nvarchar(100),
    Address nvarchar(200),
    Rating int DEFAULT 0,
    CONSTRAINT FK_Partners_PartnerTypes 
        FOREIGN KEY (PartnerTypeID) REFERENCES PartnerTypes(TypeID)
);

-- 3. Таблица продуктов
CREATE TABLE Products (
    ProductID int IDENTITY(1,1) PRIMARY KEY,
    ProductName nvarchar(100) NOT NULL,
    Price decimal(10,2) NOT NULL
);

-- 4. Таблица истории продаж
CREATE TABLE SalesHistory (
    SaleID int IDENTITY(1,1) PRIMARY KEY,
    PartnerID int NOT NULL,
    ProductID int NOT NULL,
    SaleDate datetime DEFAULT GETDATE(),
    Quantity int NOT NULL,
    CONSTRAINT FK_SalesHistory_Partners 
        FOREIGN KEY (PartnerID) REFERENCES Partners(PartnerID) ON DELETE CASCADE,
    CONSTRAINT FK_SalesHistory_Products 
        FOREIGN KEY (ProductID) REFERENCES Products(ProductID)
);

-- 5. Таблица пользователей
CREATE TABLE Users (
    UserID int IDENTITY(1,1) PRIMARY KEY,
    Login nvarchar(50) NOT NULL UNIQUE,
    Password nvarchar(50) NOT NULL,
    IsAdmin bit DEFAULT 0
);

-- =============================================
-- ВСТАВКА ТЕСТОВЫХ ДАННЫХ
-- =============================================

-- Типы партнеров
INSERT INTO PartnerTypes (TypeName) VALUES 
('ЗАО'),
('ООО'), 
('ПАО'),
('ОАО'),
('ИП');

-- Партнеры
INSERT INTO Partners (PartnerName, PartnerTypeID, DirectorName, Phone, Email, Address, Rating) VALUES 
('ООО Альфа', 2, 'Иванов Иван Иванович', '+7(123)456-78-90', 'alpha@example.com', 'г. Москва, ул. Ленина, д. 1', 5),
('ИП Бета', 5, 'Петров Петр Петрович', '+7(987)654-32-10', 'beta@example.com', 'г. СПб, пр. Невский, д. 25', 4),
('Гамма Трейд', 3, 'Сидоров Сидор Сидорович', '+7(555)123-45-67', 'gamma@example.com', 'г. Екатеринбург, ул. Мира, д. 10', 3),
('Дельта Групп', 4, 'Козлов Константин Константинович', '+7(444)987-65-43', 'delta@example.com', 'г. Новосибирск, ул. Советская, д. 5', 5),
('Эпсилон ЛТД', 1, 'Морозов Михаил Михайлович', '+7(333)111-22-33', 'epsilon@example.com', 'г. Казань, ул. Баумана, д. 15', 4),
('Зета Компани', 2, 'Волков Владимир Владимирович', '+7(222)333-44-55', 'zeta@example.com', 'г. Ростов-на-Дону, пр. Ворошиловский, д. 30', 3),
('Эта Сервис', 1, 'Лебедев Леонид Леонидович', '+7(111)222-33-44', 'eta@example.com', 'г. Уфа, ул. Ленина, д. 50', 4);

-- Продукты
INSERT INTO Products (ProductName, Price) VALUES 
('Товар А', 1000.00),
('Товар Б', 2500.50),
('Товар В', 750.25),
('Товар Г', 3200.00),
('Товар Д', 1850.75),
('Товар Е', 950.00),
('Товар Ж', 4100.25),
('Товар З', 675.50);

-- История продаж
INSERT INTO SalesHistory (PartnerID, ProductID, SaleDate, Quantity) VALUES 
-- Продажи для ООО Альфа (ID=1)
(1, 1, '2024-01-15', 10),
(1, 2, '2024-01-20', 5),
(1, 3, '2024-02-10', 15),
(1, 4, '2024-02-25', 3),
-- Продажи для ИП Бета (ID=2)
(2, 2, '2024-01-18', 8),
(2, 5, '2024-02-05', 12),
(2, 1, '2024-02-20', 6),
-- Продажи для Гамма Трейд (ID=3)
(3, 3, '2024-01-25', 20),
(3, 6, '2024-02-15', 4),
(3, 7, '2024-03-01', 2),
-- Продажи для Дельта Групп (ID=4)
(4, 4, '2024-01-30', 7),
(4, 8, '2024-02-12', 25),
(4, 1, '2024-03-05', 9),
-- Продажи для Эпсилон ЛТД (ID=5)
(5, 5, '2024-02-01', 14),
(5, 2, '2024-02-18', 11),
(5, 6, '2024-03-10', 5);

-- Пользователи системы
INSERT INTO Users (Login, Password, IsAdmin) VALUES 
('admin', 'admin123', 1),
('user', 'user123', 0),
('manager', 'manager123', 0),
('director', 'director123', 1);

-- =============================================
-- ПРОВЕРОЧНЫЕ ЗАПРОСЫ
-- =============================================

-- Проверка созданных таблиц
SELECT 'PartnerTypes' as TableName, COUNT(*) as RecordCount FROM PartnerTypes
UNION ALL
SELECT 'Partners', COUNT(*) FROM Partners  
UNION ALL
SELECT 'Products', COUNT(*) FROM Products
UNION ALL
SELECT 'SalesHistory', COUNT(*) FROM SalesHistory
UNION ALL
SELECT 'Users', COUNT(*) FROM Users;

-- Проверка связей - партнеры с типами
SELECT p.PartnerName, pt.TypeName, p.DirectorName, p.Rating
FROM Partners p
JOIN PartnerTypes pt ON p.PartnerTypeID = pt.TypeID
ORDER BY p.Rating DESC;

-- Проверка продаж
SELECT p.PartnerName, pr.ProductName, sh.Quantity, sh.SaleDate,
       (sh.Quantity * pr.Price) as TotalAmount
FROM SalesHistory sh
JOIN Partners p ON sh.PartnerID = p.PartnerID
JOIN Products pr ON sh.ProductID = pr.ProductID
ORDER BY sh.SaleDate DESC;

-- Итоговая статистика по партнерам
SELECT p.PartnerName, 
       COUNT(sh.SaleID) as SalesCount,
       SUM(sh.Quantity * pr.Price) as TotalRevenue
FROM Partners p
LEFT JOIN SalesHistory sh ON p.PartnerID = sh.PartnerID
LEFT JOIN Products pr ON sh.ProductID = pr.ProductID
GROUP BY p.PartnerID, p.PartnerName
ORDER BY TotalRevenue DESC;

PRINT 'База данных PartnersDB успешно создана и заполнена тестовыми данными!';
PRINT 'Доступные пользователи:';
PRINT '- admin / admin123 (Администратор)';
PRINT '- user / user123 (Пользователь)';
PRINT '- manager / manager123 (Пользователь)';  
PRINT '- director / director123 (Администратор)';