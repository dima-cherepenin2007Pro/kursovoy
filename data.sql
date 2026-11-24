USE ShopDB;

-- ============================================
-- 1. Статусы заказов
-- ============================================

INSERT INTO Statuses (StatusName) VALUES
('Продано'),
('Отменено'),
('В обработке');


-- ============================================
-- 2. Роли сотрудников
-- ============================================

INSERT INTO Roles (RoleName) VALUES
('Системный администратор'),
('Продавец'),
('Менеджер');


-- ============================================
-- 3. Категории товаров
-- ============================================

INSERT INTO Categories (CategoryName) VALUES
('Семена'),
('Садовый инвентарь'),
('Удобрения'),
('Горшки и кашпо'),
('Средства защиты растений'),
('Саженцы');


-- ============================================
-- 4. Сотрудники
-- ============================================

INSERT INTO Users
    (FirstName, LastName, RoleID, Login, Password)
VALUES
    ('Иван', 'Петров', 
        (SELECT RoleID FROM Roles WHERE RoleName = 'Системный администратор'),
        'admin',
        'admin123'),

    ('Анна', 'Смирнова',
        (SELECT RoleID FROM Roles WHERE RoleName = 'Продавец'),
        'seller',
        'seller123'),

    ('Дмитрий', 'Иванов',
        (SELECT RoleID FROM Roles WHERE RoleName = 'Менеджер'),
        'manager',
        'manager123');


-- ============================================
-- 5. Покупатели
-- ============================================

INSERT INTO Customers
    (FirstName, MiddleName, LastName, CustomerPhone,
     CustomerEmail, CustomerAddress, CustomerBirthDate)
VALUES
    ('Алексей', 'Сергеевич', 'Кузнецов',
     '+7-900-111-22-33',
     'kuznetsov@mail.ru',
     'г. Москва, ул. Садовая, д. 10',
     '1990-05-15'),

    ('Мария', 'Александровна', 'Соколова',
     '+7-900-222-33-44',
     'sokolova@mail.ru',
     'г. Москва, ул. Лесная, д. 25',
     '1988-09-20'),

    ('Сергей', 'Иванович', 'Попов',
     '+7-900-333-44-55',
     'popov@mail.ru',
     'г. Москва, ул. Центральная, д. 7',
     '1995-02-10'),

    ('Ольга', 'Петровна', 'Васильева',
     '+7-900-444-55-66',
     'vasileva@mail.ru',
     'г. Москва, ул. Цветочная, д. 15',
     '1992-07-03'),

    ('Николай', 'Андреевич', 'Морозов',
     '+7-900-555-66-77',
     'morozov@mail.ru',
     'г. Москва, ул. Полевая, д. 42',
     '1985-11-28');


-- ============================================
-- 6. Товары
-- ============================================

INSERT INTO Products
    (Name, Description, CategoryID, StockQuantity, Price)
VALUES

-- Семена
(
    'Семена огурца',
    'Семена огурца для выращивания в открытом грунте',
    (SELECT CategoryID FROM Categories WHERE CategoryName = 'Семена'),
    100,
    120.00
),

(
    'Семена томата',
    'Семена раннеспелого томата',
    (SELECT CategoryID FROM Categories WHERE CategoryName = 'Семена'),
    80,
    150.00
),

(
    'Семена моркови',
    'Семена моркови для дачного участка',
    (SELECT CategoryID FROM Categories WHERE CategoryName = 'Семена'),
    120,
    90.00
),

-- Садовый инвентарь
(
    'Лопата садовая',
    'Стальная садовая лопата с деревянным черенком',
    (SELECT CategoryID FROM Categories WHERE CategoryName = 'Садовый инвентарь'),
    30,
    850.00
),

(
    'Грабли садовые',
    'Металлические грабли для ухода за садом',
    (SELECT CategoryID FROM Categories WHERE CategoryName = 'Садовый инвентарь'),
    25,
    650.00
),

(
    'Секатор',
    'Ручной секатор для обрезки веток',
    (SELECT CategoryID FROM Categories WHERE CategoryName = 'Садовый инвентарь'),
    40,
    750.00
),

-- Удобрения
(
    'Удобрение универсальное',
    'Комплексное удобрение для садовых растений',
    (SELECT CategoryID FROM Categories WHERE CategoryName = 'Удобрения'),
    50,
    450.00
),

(
    'Удобрение для томатов',
    'Минеральное удобрение для выращивания томатов',
    (SELECT CategoryID FROM Categories WHERE CategoryName = 'Удобрения'),
    35,
    520.00
),

-- Горшки
(
    'Горшок пластиковый 5 л',
    'Пластиковый горшок для комнатных и садовых растений',
    (SELECT CategoryID FROM Categories WHERE CategoryName = 'Горшки и кашпо'),
    60,
    300.00
),

(
    'Кашпо декоративное',
    'Декоративное кашпо для цветов',
    (SELECT CategoryID FROM Categories WHERE CategoryName = 'Горшки и кашпо'),
    25,
    900.00
),

-- Средства защиты
(
    'Средство от вредителей',
    'Средство для защиты растений от насекомых',
    (SELECT CategoryID FROM Categories WHERE CategoryName = 'Средства защиты растений'),
    30,
    600.00
),

-- Саженцы
(
    'Саженец яблони',
    'Саженец яблони для посадки в саду',
    (SELECT CategoryID FROM Categories WHERE CategoryName = 'Саженцы'),
    20,
    1200.00
),

(
    'Саженец розы',
    'Саженец садовой розы',
    (SELECT CategoryID FROM Categories WHERE CategoryName = 'Саженцы'),
    30,
    800.00
);


-- ============================================
-- 7. Заказы
-- ============================================

-- Заказ №1
INSERT INTO Orders
    (UserID, StatusID, CustomerID)
VALUES
(
    (SELECT UserID FROM Users WHERE Login = 'seller'),
    (SELECT StatusID FROM Statuses WHERE StatusName = 'Продано'),
    (SELECT CustomerID FROM Customers WHERE CustomerEmail = 'kuznetsov@mail.ru')
);

-- Заказ №2
INSERT INTO Orders
    (UserID, StatusID, CustomerID)
VALUES
(
    (SELECT UserID FROM Users WHERE Login = 'manager'),
    (SELECT StatusID FROM Statuses WHERE StatusName = 'Продано'),
    (SELECT CustomerID FROM Customers WHERE CustomerEmail = 'sokolova@mail.ru')
);

-- Заказ №3
INSERT INTO Orders
    (UserID, StatusID, CustomerID)
VALUES
(
    (SELECT UserID FROM Users WHERE Login = 'seller'),
    (SELECT StatusID FROM Statuses WHERE StatusName = 'Отменено'),
    (SELECT CustomerID FROM Customers WHERE CustomerEmail = 'popov@mail.ru')
);

-- Заказ №4
INSERT INTO Orders
    (UserID, StatusID, CustomerID)
VALUES
(
    (SELECT UserID FROM Users WHERE Login = 'manager'),
    (SELECT StatusID FROM Statuses WHERE StatusName = 'В обработке'),
    (SELECT CustomerID FROM Customers WHERE CustomerEmail = 'vasileva@mail.ru')
);


-- ============================================
-- 8. Товары в заказах
-- ============================================

-- Товары заказа №1
INSERT INTO OrderItems
    (OrderID, ProductID, Quantity, PriceAtPurchase)
VALUES
(
    1,
    (SELECT ProductID FROM Products WHERE Name = 'Семена огурца'),
    2,
    120.00
),
(
    1,
    (SELECT ProductID FROM Products WHERE Name = 'Лопата садовая'),
    1,
    850.00
);


-- Товары заказа №2
INSERT INTO OrderItems
    (OrderID, ProductID, Quantity, PriceAtPurchase)
VALUES
(
    2,
    (SELECT ProductID FROM Products WHERE Name = 'Семена томата'),
    3,
    150.00
),
(
    2,
    (SELECT ProductID FROM Products WHERE Name = 'Удобрение для томатов'),
    1,
    520.00
),
(
    2,
    (SELECT ProductID FROM Products WHERE Name = 'Горшок пластиковый 5 л'),
    2,
    300.00
);


-- Товары отменённого заказа №3
INSERT INTO OrderItems
    (OrderID, ProductID, Quantity, PriceAtPurchase)
VALUES
(
    3,
    (SELECT ProductID FROM Products WHERE Name = 'Саженец яблони'),
    1,
    1200.00
);


-- Товары заказа №4
INSERT INTO OrderItems
    (OrderID, ProductID, Quantity, PriceAtPurchase)
VALUES
(
    4,
    (SELECT ProductID FROM Products WHERE Name = 'Секатор'),
    1,
    750.00
),
(
    4,
    (SELECT ProductID FROM Products WHERE Name = 'Саженец розы'),
    2,
    800.00
);