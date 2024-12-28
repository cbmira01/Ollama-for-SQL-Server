

/**

    This script populates tables used to showcase CLR functions:
    - Support emails used for sentiment analysis
    - Customer and sales data to demonstrate (hopefully) script construction
        from a natual language prompt
    - A stash of large prompts
    - A current schema of the TEST database for consumption by 
        the locally-hosted language models
**/

USE [TEST];

-------------------------------------------------------
-- Recreate the sentiment analysis support emails
-------------------------------------------------------
IF OBJECT_ID('support_emails', 'U') IS NOT NULL
    DROP TABLE support_emails;
GO

CREATE TABLE support_emails (
    id INT IDENTITY(1,1) PRIMARY KEY,
    email_content NVARCHAR(2000),
    sentiment NVARCHAR(20),
    sentiment_level INT
);
GO

INSERT INTO support_emails (email_content, sentiment, sentiment_level) VALUES ('The new washing machine is fantastic! It''s really quiet and efficient.', 'glad', 4);
INSERT INTO support_emails (email_content, sentiment, sentiment_level) VALUES ('The oven I bought isn''t heating up properly. Frustrated with the experience.', 'angry', 3);
INSERT INTO support_emails (email_content, sentiment, sentiment_level) VALUES ('My fridge arrived with a scratch on the door. Can this be resolved?', 'neutral', 2);
INSERT INTO support_emails (email_content, sentiment, sentiment_level) VALUES ('Thank you for the prompt service with the dryer repair! Appreciated.', 'glad', 3);
INSERT INTO support_emails (email_content, sentiment, sentiment_level) VALUES ('I need assistance setting up my dishwasher. The instructions are unclear.', 'neutral', 1);
INSERT INTO support_emails (email_content, sentiment, sentiment_level) VALUES ('The microwave isn''t working, and I''ve only had it a week. Really disappointed.', 'sad', 4);
INSERT INTO support_emails (email_content, sentiment, sentiment_level) VALUES ('Very unhappy! The dryer is making a loud noise and seems defective.', 'angry', 5);
INSERT INTO support_emails (email_content, sentiment, sentiment_level) VALUES ('The vacuum cleaner is performing well. No complaints here!', 'glad', 2);
INSERT INTO support_emails (email_content, sentiment, sentiment_level) VALUES ('I haven''t received my order confirmation. Is there a delay?', 'neutral', 2);
INSERT INTO support_emails (email_content, sentiment, sentiment_level) VALUES ('The freezer has been perfect. Thanks for the recommendation!', 'glad', 5);
INSERT INTO support_emails (email_content, sentiment, sentiment_level) VALUES ('The stove is not what I expected. Feel let down.', 'sad', 3);
INSERT INTO support_emails (email_content, sentiment, sentiment_level) VALUES ('Can someone explain the warranty terms for my washing machine?', 'neutral', 1);
INSERT INTO support_emails (email_content, sentiment, sentiment_level) VALUES ('Thanks for helping me with the installation issues. Much appreciated!', 'glad', 4);
INSERT INTO support_emails (email_content, sentiment, sentiment_level) VALUES ('The dishwasher is already leaking. Very disappointed with the quality.', 'angry', 4);
INSERT INTO support_emails (email_content, sentiment, sentiment_level) VALUES ('I’m pleased with the service team’s response time. Great job!', 'glad', 3);
INSERT INTO support_emails (email_content, sentiment, sentiment_level) VALUES ('Still no resolution on my broken fridge. Very frustrated.', 'angry', 5);
INSERT INTO support_emails (email_content, sentiment, sentiment_level) VALUES ('I''m a bit confused by the setup instructions for the oven.', 'neutral', 2);
INSERT INTO support_emails (email_content, sentiment, sentiment_level) VALUES ('Your customer support really went above and beyond. Thanks!', 'glad', 5);
INSERT INTO support_emails (email_content, sentiment, sentiment_level) VALUES ('The air conditioner broke down after two months. Extremely upset.', 'angry', 4);
INSERT INTO support_emails (email_content, sentiment, sentiment_level) VALUES ('My order arrived promptly. Appreciate the efficient service.', 'glad', 3);
GO

PRINT 'Support emails created'

----------------------------------------------------------
-- Recreate tables used to demonstrate 'QueryFromPrompt'
----------------------------------------------------------
IF OBJECT_ID('Sales', 'U') IS NOT NULL
    DROP TABLE Sales;
GO

IF OBJECT_ID('Items', 'U') IS NOT NULL
    DROP TABLE Items;
GO

IF OBJECT_ID('Customers', 'U') IS NOT NULL
    DROP TABLE Customers;
GO

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

PRINT 'Customer sales data created'

-----------------------------------------------------------------------------
-- Recreate the KeyValuePairs table
-----------------------------------------------------------------------------
IF OBJECT_ID('dbo.KeyValuePairs', 'U') IS NOT NULL
    DROP TABLE dbo.KeyValuePairs;

CREATE TABLE KeyValuePairs (
    [ID] INT PRIMARY KEY IDENTITY(1,1), 
    [Key] NVARCHAR(20),
    [Value] NVARCHAR(MAX)
);
GO

-----------------------------------------------------------------------------
-- Stash large prompts (required for 'QueryFromPrompt')
-----------------------------------------------------------------------------
DECLARE @SqlPreamble NVARCHAR(MAX) ='
You are an expert in converting natural-language queries into SQL queries in the SQL Server idiom. 
All queries must strictly adhere to SQL Server syntax.
';

DECLARE @SqlGuidelines NVARCHAR(MAX) = '
Follow these guidelines to address the key differences between SQL Server and MySQL:

1. **Row Limiting**: Use `TOP n` instead of `LIMIT n` for limiting rows.
   - Example:
     - MySQL: `SELECT * FROM employees LIMIT 5;`
     - SQL Server: `SELECT TOP 5 * FROM employees;`

2. **Date and Time Functions**: Use `GETDATE()` or `SYSDATETIME()` instead of `NOW()`.
   - Example:
     - MySQL: `SELECT NOW();`
     - SQL Server: `SELECT GETDATE();`

3. **Identifier Quoting**: Use square brackets `[ ]` instead of backticks `` ` `` or double quotes `" "`.
   - Example:
     - MySQL: `SELECT * FROM "users";`
     - SQL Server: `SELECT * FROM [users];`

4. **String Concatenation**: Use `+` for string concatenation instead of `||`.
   - Example:
     - MySQL: ''SELECT first_name || '' '' || last_name AS full_name;''
     - SQL Server: ''SELECT first_name + '' '' + last_name AS full_name;''

5. **Boolean Values**: Use `1` and `0` for `TRUE` and `FALSE`, respectively.
   - Example:
     - MySQL: `SELECT * FROM employees WHERE is_active = TRUE;`
     - SQL Server: `SELECT * FROM employees WHERE is_active = 1;`

6: **Joins**: Avoid MySQL-specific features such as `USING` in joins. Always write explicit `ON` conditions.
   - Example:
     - MySQL: `SELECT * FROM a JOIN b USING (id);`
     - SQL Server: `SELECT * FROM a JOIN b ON a.id = b.id;`

7. **Aggregates in WHERE Clause**: Aggregates cannot appear in the `WHERE` clause. Use the `HAVING` clause for filtering aggregated results.
   - Example:
     - Invalid:
       - `SELECT department, COUNT(*) FROM employees WHERE COUNT(*) > 10 GROUP BY department;`
     - SQL Server (Correct):
       - `SELECT department, COUNT(*) AS employee_count FROM employees GROUP BY department HAVING COUNT(*) > 10;`

8. **Avoid Ambiguous Column References**: Always qualify column names with their table aliases or table names in queries involving multiple tables to avoid ambiguity.
   - Example:
     - Ambiguous (Error):
       - `SELECT CustomerID, FirstName, LastName FROM Customers INNER JOIN Sales ON Customers.CustomerID = Sales.CustomerID WHERE TotalPrice >= 400;`
     - SQL Server (Correct):
       - `SELECT Customers.CustomerID, Customers.FirstName, Customers.LastName FROM Customers INNER JOIN Sales ON Customers.CustomerID = Sales.CustomerID WHERE Sales.TotalPrice >= 400;`

9. **Ensure Proper Table Joins**: Always explicitly join tables when querying fields from multiple tables. Use the correct `ON` condition to connect related tables.
   - Example:
     - Incorrect:
       - `SELECT COUNT(*) FROM Sales WHERE Customers.FirstName = ''John'';`
     - SQL Server (Correct):
       - `SELECT COUNT(*) FROM Sales INNER JOIN Customers ON Sales.CustomerID = Customers.CustomerID WHERE Customers.FirstName = ''John'';`
';

DECLARE @SchemaPreamble NVARCHAR(MAX) ='
You are able to read database schema information in JSON format.
You can select tables and fields from this schema that you think 
are likely to answer the prompt that will be given to you.

Following is the JSON schema of the database you will examine:
';

DECLARE @SqlPostscript NVARCHAR(MAX) ='
Generate SQL code only and make no other commentary.

Write a query for the following prompt:
';

INSERT INTO KeyValuePairs ([Key], [Value]) VALUES
(N'sqlPreamble', @SqlPreamble),
(N'sqlGuidelines', @SqlGuidelines),
(N'schemaPreamble', @SchemaPreamble),
(N'sqlPostscript', @SqlPostscript);
GO

-----------------------------------------------------------------------------
-- Recreate the recreate the database schema (required for 'QueryFromPrompt')
-----------------------------------------------------------------------------
INSERT INTO KeyValuePairs ([Key], [Value])
SELECT 
    'schemaJson' AS [Key],
    (
        SELECT 
            t.TABLE_NAME AS name,
            (
                SELECT 
                    c.COLUMN_NAME AS name,
                    c.DATA_TYPE AS type,
                    c.CHARACTER_MAXIMUM_LENGTH AS maxLength,
                    CASE 
                        WHEN pk.COLUMN_NAME IS NOT NULL THEN 'true' 
                        ELSE 'false' 
                    END AS primaryKey
                FROM 
                    INFORMATION_SCHEMA.COLUMNS c
                LEFT JOIN 
                    (
                        SELECT 
                            cu.TABLE_NAME, cu.COLUMN_NAME
                        FROM 
                            INFORMATION_SCHEMA.TABLE_CONSTRAINTS tc
                        JOIN 
                            INFORMATION_SCHEMA.KEY_COLUMN_USAGE cu
                        ON 
                            tc.CONSTRAINT_NAME = cu.CONSTRAINT_NAME
                        WHERE 
                            tc.CONSTRAINT_TYPE = 'PRIMARY KEY'
                    ) pk
                ON 
                    c.TABLE_NAME = pk.TABLE_NAME AND c.COLUMN_NAME = pk.COLUMN_NAME
                WHERE 
                    c.TABLE_NAME = t.TABLE_NAME
                FOR JSON PATH
            ) AS columns
        FROM 
            INFORMATION_SCHEMA.TABLES t
        WHERE 
            t.TABLE_TYPE = 'BASE TABLE'
            AND t.TABLE_CATALOG = DB_NAME() -- Use current database dynamically
        FOR JSON PATH, ROOT('tables')
    ) AS [Value];

SELECT TOP 1 
    [Key], [Value]
FROM KeyValuePairs
WHERE [Key] = N'schemaJson'
ORDER BY [ID] DESC;
GO

/**
Typical SchemaJson output: 

{"tables":[{"name":"support_emails","columns":[{"name":"id","type":"int","primaryKey":"true"},{"name":"email_content","type":"nvarchar","maxLength":2000,"primaryKey":"false"},
{"name":"sentiment","type":"nvarchar","maxLength":20,"primaryKey":"false"},{"name":"sentiment_level","type":"int","primaryKey":"false"}]},
{"name":"Customers","columns":[{"name":"CustomerID","type":"int","primaryKey":"true"},{"name":"FirstName","type":"nvarchar","maxLength":50,"primaryKey":"false"},
{"name":"LastName","type":"nvarchar","maxLength":50,"primaryKey":"false"},{"name":"Email","type":"nvarchar","maxLength":100,"primaryKey":"false"},
{"name":"Phone","type":"nvarchar","maxLength":15,"primaryKey":"false"},{"name":"Address","type":"nvarchar","maxLength":255,"primaryKey":"false"}]},
{"name":"Items","columns":[{"name":"ItemID","type":"int","primaryKey":"true"},{"name":"ItemName","type":"nvarchar","maxLength":100,"primaryKey":"false"},
{"name":"Description","type":"nvarchar","maxLength":255,"primaryKey":"false"},{"name":"Price","type":"decimal","primaryKey":"false"},
{"name":"StockQuantity","type":"int","primaryKey":"false"}]},{"name":"Sales","columns":[{"name":"SaleID","type":"int","primaryKey":"true"},
{"name":"CustomerID","type":"int","primaryKey":"false"},{"name":"ItemID","type":"int","primaryKey":"false"},{"name":"SaleDate","type":"datetime","primaryKey":"false"},
{"name":"Quantity","type":"int","primaryKey":"false"},{"name":"TotalPrice","type":"decimal","primaryKey":"false"}]},
{"name":"DB_Schema","columns":[{"name":"ID","type":"int","primaryKey":"true"},{"name":"SchemaJson","type":"nvarchar","maxLength":-1,"primaryKey":"false"}]}]}

**/

PRINT 'Database schema created'
