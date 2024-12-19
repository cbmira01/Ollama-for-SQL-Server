


USE [TEST];

-----------------------------------------------------------------------------
-- Drop and recreate the schema table (required for QueryFromPrompt)
-----------------------------------------------------------------------------
IF OBJECT_ID('dbo.DB_Schema', 'U') IS NOT NULL
    DROP TABLE dbo.DB_Schema;
GO

CREATE TABLE DB_Schema (
    ID INT PRIMARY KEY IDENTITY(1,1), 
    SchemaJson NVARCHAR(MAX)
);
GO

INSERT INTO DB_Schema (SchemaJson)
SELECT 
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
    ) AS schemaJson;

SELECT TOP 1 SchemaJson 
FROM DB_Schema
ORDER BY ID DESC;
GO
