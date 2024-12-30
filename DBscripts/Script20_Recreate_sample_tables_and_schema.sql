

/**

    This script populates tables used to showcase CLR functions:
    - Support emails used for sentiment analysis;
    - Customer and sales data to demonstrate script construction from a natual language prompt;
    - A stash of large prompts for the QueryFromPrompt feature;
    - A current schema of the TEST database for consumption by the locally-hosted language models;
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
DECLARE @SqlPreamble NVARCHAR(MAX) = '
You are an expert in converting natural-language queries into SQL queries in the SQL Server idiom. 
Strictly adhere to SQL Server syntax.
';

DECLARE @SqlGuidelines NVARCHAR(MAX) = '
Follow these SQL Server guidelines. Always adhere strictly to these rules:

1. **Row Limiting**: Use `TOP n` instead of `LIMIT n`.
   - Example: `SELECT TOP 5 * FROM employees;`

2. **Date and Time Functions**: Use `GETDATE()` or `SYSDATETIME()` instead of `NOW()`.

3. **Identifier Quoting**: Use square brackets `[ ]` instead of backticks `` ` `` or double quotes `" "`.

4. **String Concatenation**: Use `+` for concatenation instead of `||`.
   - Example: `SELECT first_name + '' '' + last_name AS full_name;`

5. **Boolean Values**: Use `1` and `0` for `TRUE` and `FALSE`.

6. **Joins**: Avoid `USING` in joins. Use explicit `ON` conditions.
   - Example: `SELECT * FROM a JOIN b ON a.id = b.id;`

7. **Aggregates in Conditions**: Avoid mixing aggregates (e.g., `COUNT`, `SUM`) with row-level conditions in `WHERE` or `CASE`. 
        Use `HAVING` for filtering aggregated results or subqueries to isolate aggregation.
- Examples:
   - Invalid WHERE clause: `SELECT department FROM employees WHERE COUNT(*) > 10;`  
   - Correct WHERE clause:  `SELECT department FROM employees GROUP BY department HAVING COUNT(*) > 10;`
   - Invalid CASE Statement:   `SELECT CASE WHEN COUNT(*) > 1 THEN ''Multiple''   ELSE ''Single''   END AS result FROM employees;`
   - Correct Subquery: `SELECT CASE WHEN (SELECT COUNT(*) FROM employees) > 1 THEN ''Multiple''  ELSE  ''Single'' END AS result;`

Use aggregates only in `HAVING`, subqueries, or after `GROUP BY`.

8. **Fully Qualify Column References (MANDATORY)**: Always use table names with column names in **all clauses** (`SELECT`, `WHERE`, `GROUP BY`, `HAVING`, `ORDER BY`, etc.). This ensures clarity and avoids ambiguity in queries involving multiple tables.
   - Example:
     - Ambiguous: `SELECT CustomerID, FirstName FROM Customers;`
     - Correct: `SELECT Customers.CustomerID, Customers.FirstName FROM Customers;`

9. **Proper Joins**: Always explicitly join tables using `ON`.
   - Example: `SELECT COUNT(*) FROM Table1 INNER JOIN Table2 ON Table1.CustomerID = Table2.CustomerID;`

10. **ORDER BY in Subqueries and Views**: Use `TOP`, `OFFSET`, or `FOR XML` with `ORDER BY` in subqueries.
    - Example: `SELECT column FROM (SELECT TOP 100 PERCENT * FROM table ORDER BY column) AS Subquery;`

11. **Do Not Omit Table Names**: Table names **must** be included with all column references, even if only one table is queried. 
    - Example:
      - Incorrect: `SELECT FirstName FROM Customers;`
      - Correct: `SELECT Customers.FirstName FROM Customers;`

12. **Consistency in Aliases (Optional)**: Use aliases when table names are long, but aliases must be consistent and applied to all columns.
    - Example:
      - Correct: `SELECT c.CustomerID, c.FirstName FROM Customers AS c;`
';

DECLARE @SchemaPreamble NVARCHAR(MAX) = '
Use the provided database schema to generate SQL queries.
You must fully qualify all column names with their table names in all clauses, regardless of ambiguity.
';

DECLARE @SqlPostscript NVARCHAR(MAX) = '
Generate SQL code only. Make no other commentary about the query.

If you cannot create valid SQL following these guidelines, reply with ''No Reply''.

Write a query for the following prompt:
';

DECLARE @DoubleCheck NVARCHAR(MAX) = '
The SQL code you provided had a runtime error. Try again.
Double-check your response against established SQL code guidelines and the database schema.
MANDATORY! Reply with SQL code only, do not provide any other commentary.
';

INSERT INTO KeyValuePairs ([Key], [Value]) VALUES
(N'sqlPreamble', @SqlPreamble),
(N'sqlGuidelines', @SqlGuidelines),
(N'schemaPreamble', @SchemaPreamble),
(N'sqlPostscript', @SqlPostscript),
(N'doubleCheck', @DoubleCheck);
GO

PRINT 'Large prompts have been stashed'
-----------------------------------------------------------------------------
-- Recreate the database schema (required for 'QueryFromPrompt')
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

PRINT 'Key-value pairs created'
