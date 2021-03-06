--
-- Drop UdtTestDb, Northwind and associated UDTS, tables and procedures.
--
PRINT N'============================================================================='
GO
PRINT N'Dropping UdtTestDb and Northwind'
GO
PRINT N'============================================================================='
GO
SET NOCOUNT ON
print 'Dropping UdtTestDb database.'
if  exists (select name from [master].[dbo].[sysdatabases] where name = N'UdtTestDb')
    drop database [UdtTestDb]
go

print 'Dropping Northwind database.'
if  exists (select name from [master].[dbo].[sysdatabases] where name = N'Northwind')
    drop database [Northwind]
go