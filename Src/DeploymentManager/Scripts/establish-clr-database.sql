
-------------------------------------------------------------------------------------
PRINT '[CHECK]: scriptName is ''establish-clr-database.sql'' ';
PRINT '[CHECK]: Ensure the @RepoRootDirectory symbol is declared in this script.';
-------------------------------------------------------------------------------------

USE [master];

-------------------------------------------------------------------------------------
PRINT '[STEP]: Recreate the AI_Lab database';
-------------------------------------------------------------------------------------
IF EXISTS (SELECT name FROM sys.databases WHERE name = 'AI_Lab')
BEGIN
    -- Set database to single-user mode to terminate active connections
    ALTER DATABASE [AI_Lab] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE [AI_Lab];
END

CREATE DATABASE [AI_Lab];
ALTER DATABASE [AI_Lab] SET TRUSTWORTHY ON;

----------------------------------------------------------------------------------------
PRINT '[STEP]: Enable CLR integration, disable strict security for UNSAFE assemblies';
----------------------------------------------------------------------------------------
EXEC sp_configure 'clr enabled', 1;
RECONFIGURE;

EXEC sp_configure 'show advanced options', 1;
RECONFIGURE;

EXEC sp_configure 'clr strict security', 0; 
RECONFIGURE;

EXEC sp_configure 'show advanced options', 0;
RECONFIGURE;

-------------------------------------------------------------------------------------
PRINT '[STEP]: Delete this project''s trusted assemblies';
-------------------------------------------------------------------------------------
DECLARE @hash VARBINARY(64);

-- Remove OllamaSqlClr trusted assembly if it exists
SELECT @hash = [hash]
FROM sys.trusted_assemblies
WHERE description = 'OllamaSqlClr';

IF @hash IS NOT NULL
BEGIN
    EXEC sys.sp_drop_trusted_assembly @hash = @hash;
END

-- Remove JsonClrLibrary if it exists
SELECT @hash = [hash]
FROM sys.trusted_assemblies
WHERE description = 'JsonClrLibrary';

IF @hash IS NOT NULL
BEGIN
    EXEC sys.sp_drop_trusted_assembly @hash = @hash;
END

-------------------------------------------------------------------------------------
PRINT '[STEP]: Define trusted assemblies for this project';
-------------------------------------------------------------------------------------
DECLARE @OllamaSqlClrRelease NVARCHAR(200) = 'Src\OllamaSqlClr\bin\Release\OllamaSqlClr.dll';
DECLARE @JsonClrLibraryRelease NVARCHAR(200) = 'Src\OllamaSqlClr\bin\Release\JsonClrLibrary.dll';

DECLARE @OllamaSqlClrAssemblyPath NVARCHAR(MAX) = @RepoRootDirectory + '\' + @OllamaSqlClrRelease;
DECLARE @JsonClrLibraryAssemblyPath NVARCHAR(MAX) = @RepoRootDirectory + '\' + @JsonClrLibraryRelease;

DECLARE @AssemblyPath NVARCHAR(MAX);
DECLARE @AssemblyHash VARBINARY(64);
DECLARE @sql NVARCHAR(MAX);

-------------------------------------------------------------------------------------
PRINT '[STEP]: Trust the OllamaSqlClr assembly';
-------------------------------------------------------------------------------------
SET @AssemblyPath = @OllamaSqlClrAssemblyPath;

SET @sql = N'
SELECT @AssemblyHashOut = HASHBYTES(''SHA2_512'', BulkColumn)
FROM OPENROWSET(BULK ''' + @AssemblyPath + ''', SINGLE_BLOB) AS A;
';

-- Execute the dynamic SQL and capture the assembly hash
EXEC sp_executesql @sql, N'@AssemblyHashOut VARBINARY(64) OUTPUT', @AssemblyHashOut=@AssemblyHash OUTPUT;

-- Add the assembly’s hash to the trusted assemblies
EXEC sys.sp_add_trusted_assembly @hash = @AssemblyHash, @description = N'OllamaSqlClr';

-------------------------------------------------------------------------------------
PRINT '[STEP]: Trust the JsonClrLibrary assembly';
-------------------------------------------------------------------------------------
SET @AssemblyPath = @JsonClrLibraryAssemblyPath;

SET @sql = N'
SELECT @AssemblyHashOut = HASHBYTES(''SHA2_512'', BulkColumn)
FROM OPENROWSET(BULK ''' + @AssemblyPath + ''', SINGLE_BLOB) AS A;
';

EXEC sp_executesql @sql, N'@AssemblyHashOut VARBINARY(64) OUTPUT', @AssemblyHashOut=@AssemblyHash OUTPUT;

EXEC sys.sp_add_trusted_assembly @hash = @AssemblyHash, @description = N'JsonClrLibrary';

-------------------------------------------------------------------------------------
PRINT '[STEP]: Verify the assembly is now trusted';
-------------------------------------------------------------------------------------
SELECT
    [description],
    [create_date],
    [created_by]
FROM sys.trusted_assemblies;

GO

USE [AI_Lab];

-------------------------------------------------------------------------------------
PRINT '[STEP]: Create the KeyValuePairs table';
-------------------------------------------------------------------------------------
CREATE TABLE KeyValuePairs (
    [ID] INT PRIMARY KEY IDENTITY(1,1), 
    [Key] NVARCHAR(20),
    [Value] NVARCHAR(MAX)
);

-------------------------------------------------------------------------------------
PRINT '[STEP]: Create the Images table';
-------------------------------------------------------------------------------------
CREATE TABLE Images (
    Id INT PRIMARY KEY IDENTITY,
    FileName NVARCHAR(255) NOT NULL,
    ImageData VARBINARY(MAX) NOT NULL
);

-------------------------------------------------------------------------------------
PRINT '[STEP]: Create the support_emails table';
-------------------------------------------------------------------------------------
CREATE TABLE support_emails (
    id INT IDENTITY(1,1) PRIMARY KEY,
    email_content NVARCHAR(2000),
    sentiment NVARCHAR(20),
    sentiment_level INT
);

-------------------------------------------------------------------------------------
PRINT '[STEP]: Create the tables used to demonstrate query generation';
-------------------------------------------------------------------------------------
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

PRINT 'Goodbye...';
