
-------------------------------------------------------------------------------------
PRINT '[CHECK]: scriptName is ''populate-config-and-schema.sql'' ';
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
PRINT '[STEP]: Delete the KeyValuePairs table';
-------------------------------------------------------------------------------------
IF EXISTS (SELECT 1 FROM KeyValuePairs)
BEGIN
    PRINT 'The KeyValuePairs table was not empty. Clearing it before inserting new values.';
    DELETE FROM KeyValuePairs;
END

-------------------------------------------------------------------------------------
PRINT '[STEP]: Stash complex prompts required for query generation';
-------------------------------------------------------------------------------------
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

-------------------------------------------------------------------------------------
PRINT '[STEP]: Rebuild the database schema, required for query generation';
-------------------------------------------------------------------------------------
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

PRINT 'Goodbye...';
