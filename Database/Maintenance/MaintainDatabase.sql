DECLARE @DateString varchar(8)
DECLARE @BackupName varchar(255)
DECLARE @BackupLogName varchar(255)

BEGIN TRY
  SELECT @DateString = CONVERT(varchar(8), GETDATE(), 112)
  SELECT @BackupName = DB_NAME() + '_' + @DateString + '.bak'
  SELECT @BackupLogName = DB_NAME() + 'Log_' + @DateString + '.bak'

  EXEC dbo.BackupDatabase 'BudgetTools', 'C:\Users\busin\Documents\Finances\Backups', @BackupName

  ----EXEC dbo.BackupDatabaseLog 'Finances', 'C:\Workspace\Finances\Backups', @BackupLogName
  	
  EXEC dbo.VerifyBackup 'BudgetTools', 'C:\Users\busin\Documents\Finances\Backups', @BackupName

  ----EXEC dbo.VerifyBackup 'Finances', 'C:\Workspace\Finances\Backups', @BackupLogName

  EXEC dbo.RebuildIndexes 'BudgetTools'

  EXEC dbo.UpdateStatistics 'BudgetTools'
END TRY
BEGIN CATCH
  SELECT ERROR_MESSAGE()
END CATCH
