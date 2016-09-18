CREATE PROCEDURE dbo.BackupDatabase
	@db_name		nvarchar(255),
	@file_path		nvarchar(255),
	@file_name		nvarchar(255)
AS

DECLARE @path		nvarchar(4000)

SELECT @path = @file_path + '\' + @file_name

BACKUP DATABASE @db_name 
TO DISK = @path
WITH NOFORMAT, NOINIT, NAME = @file_name, SKIP, REWIND, NOUNLOAD, STATS = 10
GO