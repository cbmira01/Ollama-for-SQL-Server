-------------------------------------------------------------------------------------
PRINT '[CHECK]: scriptName is ''populate-demo-data.sql'' ';
-------------------------------------------------------------------------------------

-------------------------------------------------------------------------------------
PRINT '[STEP]: Determine if the AI_Lab database has been established';
-------------------------------------------------------------------------------------
USE [master];

IF NOT EXISTS (SELECT 1 FROM sys.databases WHERE name = 'AI_Lab')
BEGIN
    PRINT '[ERROR]: The AI_Lab database does not exist; it must be established.';
    SET NOEXEC ON;
END

GO

USE [AI_Lab];

-------------------------------------------------------------------------------------
PRINT '[STEP]: Recreate support emails for sentiment analysis';
-------------------------------------------------------------------------------------
IF OBJECT_ID('support_emails', 'U') IS NOT NULL
    DROP TABLE support_emails;

CREATE TABLE support_emails (
    id INT IDENTITY(1,1) PRIMARY KEY,
    email_content NVARCHAR(2000),
    sentiment NVARCHAR(20),
    sentiment_level INT
);

INSERT INTO support_emails (email_content, sentiment, sentiment_level) VALUES 
    ('The new washing machine is fantastic! It''s really quiet and efficient.', 'glad', 4),
    ('The oven I bought isn''t heating up properly. Frustrated with the experience.', 'angry', 3),
    ('My fridge arrived with a scratch on the door. Can this be resolved?', 'neutral', 2),
    ('Thank you for the prompt service with the dryer repair! Appreciated.', 'glad', 3),
    ('I need assistance setting up my dishwasher. The instructions are unclear.', 'neutral', 1),
    ('The microwave isn''t working, and I''ve only had it a week. Really disappointed.', 'sad', 4),
    ('Very unhappy! The dryer is making a loud noise and seems defective.', 'angry', 5),
    ('The vacuum cleaner is performing well. No complaints here!', 'glad', 2),
    ('I haven''t received my order confirmation. Is there a delay?', 'neutral', 2),
    ('The freezer has been perfect. Thanks for the recommendation!', 'glad', 5),
    ('The stove is not what I expected. Feel let down.', 'sad', 3),
    ('Can someone explain the warranty terms for my washing machine?', 'neutral', 1),
    ('Thanks for helping me with the installation issues. Much appreciated!', 'glad', 4),
    ('The dishwasher is already leaking. Very disappointed with the quality.', 'angry', 4),
    ('I’m pleased with the service team’s response time. Great job!', 'glad', 3),
    ('Still no resolution on my broken fridge. Very frustrated.', 'angry', 5),
    ('I''m a bit confused by the setup instructions for the oven.', 'neutral', 2),
    ('Your customer support really went above and beyond. Thanks!', 'glad', 5),
    ('The air conditioner broke down after two months. Extremely upset.', 'angry', 4),
    ('My order arrived promptly. Appreciate the efficient service.', 'glad', 3);

-------------------------------------------------------------------------------------
PRINT '[STEP]: Recreate tables used for query generation tests';
-------------------------------------------------------------------------------------
IF OBJECT_ID('Sales', 'U') IS NOT NULL
    DROP TABLE Sales;

IF OBJECT_ID('Items', 'U') IS NOT NULL
    DROP TABLE Items;

IF OBJECT_ID('Customers', 'U') IS NOT NULL
    DROP TABLE Customers;

CREATE TABLE Customers (
    CustomerID INT IDENTITY(1,1) PRIMARY KEY,
    FirstName NVARCHAR(50) NOT NULL,
    LastName NVARCHAR(50) NOT NULL,
    Email NVARCHAR(100),
    Phone NVARCHAR(15),
    Address NVARCHAR(255)
);

CREATE TABLE Items (
    ItemID INT IDENTITY(1,1) PRIMARY KEY,
    ItemName NVARCHAR(100) NOT NULL,
    Description NVARCHAR(255),
    Price DECIMAL(10,2) NOT NULL,
    StockQuantity INT NOT NULL
);

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

INSERT INTO Customers (FirstName, LastName, Email, Phone, Address) VALUES
    ('John', 'Doe', 'john.doe@example.com', '123-456-7890', '123 Elm St'),
    ('Jane', 'Smith', 'jane.smith@example.com', '234-567-8901', '456 Oak St'),
    ('Alice', 'Johnson', 'alice.johnson@example.com', '345-678-9012', '789 Pine St'),
    ('Bob', 'Williams', 'bob.williams@example.com', '456-789-0123', '321 Maple St'),
    ('Eve', 'Brown', 'eve.brown@example.com', '567-890-1234', '654 Cedar St');

INSERT INTO Items (ItemName, Description, Price, StockQuantity) VALUES
    ('Laptop', 'High performance laptop', 999.99, 50),
    ('Smartphone', 'Latest model smartphone', 799.99, 100),
    ('Tablet', '10-inch screen tablet', 399.99, 30),
    ('Headphones', 'Noise-cancelling headphones', 199.99, 75),
    ('Smartwatch', 'Waterproof smartwatch', 249.99, 40);

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

PRINT 'Goodbye...';
