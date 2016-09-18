CREATE PROCEDURE dbo.UpdateStatistics 
    @databasename               varchar(255) 
AS

DECLARE @SQL                    nvarchar(4000) 

-- update statistics 
SELECT @SQL = @databasename + '.dbo.sp_updatestats' 
EXEC @SQL
GO