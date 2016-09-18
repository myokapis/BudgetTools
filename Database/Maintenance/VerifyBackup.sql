CREATE PROCEDURE dbo.VerifyBackup
	@db_name				nvarchar(255),
	@file_path				nvarchar(255),
	@file_name				nvarchar(255)
AS

DECLARE @backup_set_id		int
DECLARE @path				nvarchar(4000)
DECLARE @msg				nvarchar(1000)

SELECT @path = @file_path + '\' + @file_name

SELECT @backup_set_id = position 
FROM msdb.dbo.backupset 
WHERE database_name = @db_name
	AND backup_set_id = (
		SELECT MAX(backup_set_id) 
		FROM msdb.dbo.backupset 
		WHERE database_name = @db_name)

IF @backup_set_id IS NULL 
  BEGIN
	SELECT @msg = 'Verify failed. Backup information for database, ' + @db_name + ', not found.'
	RAISERROR(@msg, 16, 1) 
  END

RESTORE VERIFYONLY 
FROM DISK = @path
WITH FILE = @backup_set_id,  NOUNLOAD,  NOREWIND
GO