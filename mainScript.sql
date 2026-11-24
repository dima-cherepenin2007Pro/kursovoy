CREATE DATABASE ShopDB;
USE ShopDB;

-- Создание таблицы Statuses
CREATE TABLE Statuses (
    StatusID INT PRIMARY KEY AUTO_INCREMENT,
    StatusName VARCHAR(50) NOT NULL UNIQUE
);

-- Создание таблицы Roles
CREATE TABLE Roles (
    RoleID INT PRIMARY KEY AUTO_INCREMENT,
    RoleName VARCHAR(50) NOT NULL UNIQUE
);

-- Создание таблицы Customers
CREATE TABLE Customers (
    CustomerID INT PRIMARY KEY AUTO_INCREMENT,
    FirstName VARCHAR(50) NOT NULL,
    MiddleName VARCHAR(50),
    LastName VARCHAR(50) NOT NULL,
    CustomerPhone VARCHAR(50) NOT NULL,
    CustomerEmail VARCHAR(150) NOT NULL,
    CustomerAddress VARCHAR(150) NOT NULL,
    CustomerBirthDate DATE NOT NULL
);

-- Создание таблицы Categories
CREATE TABLE Categories (
    CategoryID INT PRIMARY KEY AUTO_INCREMENT,
    CategoryName VARCHAR(100) NOT NULL UNIQUE
);

-- Создание таблицы Users
CREATE TABLE Users (
    UserID INT PRIMARY KEY AUTO_INCREMENT,
    FirstName VARCHAR(50) NOT NULL,
    LastName VARCHAR(50) NOT NULL,
    RoleID INT NOT NULL,
    Login VARCHAR(50) NOT NULL UNIQUE,
    Password VARCHAR(255) NOT NULL,
    FOREIGN KEY (RoleID) REFERENCES Roles(RoleID)
);

-- Создание таблицы Products
CREATE TABLE Products (
    ProductID INT PRIMARY KEY AUTO_INCREMENT,
    Name VARCHAR(200) NOT NULL,
    Description TEXT,
    CategoryID INT NOT NULL,
    StockQuantity INT NOT NULL DEFAULT 0,
    Price DECIMAL(10, 2) NOT NULL,
    FOREIGN KEY (CategoryID) REFERENCES Categories(CategoryID)
);

-- Создание таблицы Orders
CREATE TABLE Orders (
    OrderID INT PRIMARY KEY AUTO_INCREMENT,
    UserID INT NOT NULL,
    OrderDate DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    StatusID INT NOT NULL,
    CustomerID INT NOT NULL,
    FOREIGN KEY (UserID) REFERENCES Users(UserID),
    FOREIGN KEY (StatusID) REFERENCES Statuses(StatusID),
    FOREIGN KEY (CustomerID) REFERENCES Customers(CustomerID)
);

-- Создание таблицы OrderItems
CREATE TABLE OrderItems (
    OrderItemsID INT PRIMARY KEY AUTO_INCREMENT,
    OrderID INT NOT NULL,
    ProductID INT NOT NULL,
    Quantity INT NOT NULL CHECK (Quantity > 0),
    PriceAtPurchase DECIMAL(10, 2) NOT NULL,
    FOREIGN KEY (OrderID) REFERENCES Orders(OrderID),
    FOREIGN KEY (ProductID) REFERENCES Products(ProductID)
);