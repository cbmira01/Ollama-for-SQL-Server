

-- Testing 'QueryFromPrompt' feature

DECLARE @modelName NVARCHAR(MAX) = 'llama3.2';
DECLARE @prompt NVARCHAR(MAX) = 'Find all entries in support_emails where sentiment is glad.';

SELECT * FROM dbo.QueryFromPrompt(@modelName, @prompt);
GO

SELECT * FROM support_emails WHERE sentiment = 'glad'


-- Create the Customers table
CREATE TABLE Customers (
    CustomerID INT IDENTITY(1,1) PRIMARY KEY,
    FirstName NVARCHAR(50) NOT NULL,
    LastName NVARCHAR(50) NOT NULL,
    Email NVARCHAR(100),
    Phone NVARCHAR(15),
    Address NVARCHAR(255)
);
GO

-- Create the Items table
CREATE TABLE Items (
    ItemID INT IDENTITY(1,1) PRIMARY KEY,
    ItemName NVARCHAR(100) NOT NULL,
    Description NVARCHAR(255),
    Price DECIMAL(10,2) NOT NULL,
    StockQuantity INT NOT NULL
);
GO

-- Create the Sales table
CREATE TABLE Sales (
    SaleID INT IDENTITY(1,1) PRIMARY KEY,
    CustomerID INT NOT NULL,
    ItemID INT NOT NULL,
    SaleDate DATETIME NOT NULL DEFAULT GETDATE(),
    Quantity INT NOT NULL,
    TotalPrice DECIMAL(10,2) NOT NULL,
    FOREIGN KEY (CustomerID) REFERENCES Customers(CustomerID),
    FOREIGN KEY (ItemID) REFERENCES Items(ItemID)
);
GO

-- Insert test data into Customers
INSERT INTO Customers (FirstName, LastName, Email, Phone, Address) VALUES
('John', 'Doe', 'john.doe@example.com', '123-456-7890', '123 Elm St'),
('Jane', 'Smith', 'jane.smith@example.com', '234-567-8901', '456 Oak St'),
('Alice', 'Johnson', 'alice.johnson@example.com', '345-678-9012', '789 Pine St'),
('Bob', 'Williams', 'bob.williams@example.com', '456-789-0123', '321 Maple St'),
('Eve', 'Brown', 'eve.brown@example.com', '567-890-1234', '654 Cedar St');
GO

-- Insert test data into Items
INSERT INTO Items (ItemName, Description, Price, StockQuantity) VALUES
('Laptop', 'High performance laptop', 999.99, 50),
('Smartphone', 'Latest model smartphone', 799.99, 100),
('Tablet', '10-inch screen tablet', 399.99, 30),
('Headphones', 'Noise-cancelling headphones', 199.99, 75),
('Smartwatch', 'Waterproof smartwatch', 249.99, 40);
GO

-- Insert test data into Sales
INSERT INTO Sales (CustomerID, ItemID, SaleDate, Quantity, TotalPrice) VALUES
(1, 1, '2024-12-01 10:15:00', 1, 999.99),
(2, 2, '2024-12-02 14:30:00', 2, 1599.98),
(3, 3, '2024-12-03 09:45:00', 1, 399.99),
(4, 4, '2024-12-04 16:20:00', 3, 599.97),
(5, 5, '2024-12-05 11:50:00', 2, 499.98),
(1, 2, '2024-12-06 13:10:00', 1, 799.99),
(2, 3, '2024-12-07 08:25:00', 2, 799.98),
(3, 1, '2024-12-08 15:40:00', 1, 999.99),
(4, 5, '2024-12-09 12:00:00', 2, 499.98),
(5, 4, '2024-12-10 18:05:00', 1, 199.99);
GO

