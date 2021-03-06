CREATE PROCEDURE dbo.RebuildIndexes
	@databasename			varchar(255), 
	@maxfrag				float = 10.0,
	@maxdensity				float = 75.0
AS 

DECLARE @SQL                nvarchar(4000)
DECLARE @DB_ID              int
DECLARE @Table varchar(255)
DECLARE @Tables TABLE(name varchar(255))

-- lookup database id
SELECT @DB_ID = database_id
FROM sys.databases 
WHERE name = @databasename

-- build query to find fragmented tables
SELECT @SQL = 'SELECT distinct b.Name '
+ 'FROM sys.dm_db_index_physical_stats (' + CAST(@DB_ID AS varchar) + ', NULL, NULL, NULL, NULL) a '
+ 'inner join sys.tables b (nolock) on a.object_id = b.object_id '
+ 'where a.avg_fragmentation_in_percent > ' + CAST(@maxfrag AS varchar)

-- get a list of tables to defrag 
INSERT INTO @Tables(name)
EXEC sp_executesql @SQL

-- loop through the list of tables to defrag and rebuild all indexes 
WHILE 1 = 1 
  BEGIN

    SELECT TOP 1 @Table = name
    FROM @Tables

    IF @@ROWCOUNT != 1 BREAK

    SELECT @SQL = 'ALTER INDEX ALL ON ' + @databasename + '.dbo.' + @Table + ' REBUILD '
    EXEC sp_executesql @SQL

    DELETE a
    FROM @Tables a 
    WHERE name = @Table

  END
