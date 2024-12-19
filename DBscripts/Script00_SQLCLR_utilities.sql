

-------------------------------------------------------------
-- List all user-defined assemblies and all CLR functions
-------------------------------------------------------------
SELECT * FROM sys.assemblies WHERE is_user_defined = 1;
GO

SELECT 
    asm.name AS AssemblyName,
    asm.permission_set_desc AS AssemblyPermissionSet,
    obj.name AS FunctionName,
    obj.type_desc AS ObjectType,
    mod.assembly_class AS AssemblyClass,
    mod.assembly_method AS AssemblyMethod
FROM sys.assembly_modules mod
JOIN sys.objects obj ON mod.object_id = obj.object_id
JOIN sys.assemblies asm ON mod.assembly_id = asm.assembly_id
--WHERE obj.type IN ('FN', 'TF', 'IF'); -- FN = Scalar function, TF = Table-valued function, IF = Inline function
GO


-------------------------------------------------
-- Trusted assembly cleanup script
-------------------------------------------------

-- This script removes all entries from sys.trusted_assemblies 
-- by iterating through each trusted assembly and calling sp_drop_trusted_assembly.
--
-- Ensure you have the appropriate permissions to run sp_drop_trusted_assembly.
-- This script removes all trusted assemblies, so use it with caution.
-- To use this script, remove the block comment markers.

/****
DECLARE @hash VARBINARY(64);

DECLARE hash_cursor CURSOR LOCAL FORWARD_ONLY READ_ONLY FOR
SELECT [hash]
FROM sys.trusted_assemblies;

OPEN hash_cursor;
FETCH NEXT FROM hash_cursor INTO @hash;

WHILE @@FETCH_STATUS = 0
BEGIN
    EXEC sys.sp_drop_trusted_assembly @hash = @hash;
    FETCH NEXT FROM hash_cursor INTO @hash;
END

CLOSE hash_cursor;
DEALLOCATE hash_cursor;

-- Verify all trusted assemblies have been removed
SELECT * FROM sys.trusted_assemblies;

***/
