USE BudgetTools;
GO

IF NOT EXISTS(SELECT * FROM sys.tables WHERE object_id = OBJECT_ID('dbo.BankAccounts'))
BEGIN

  CREATE TABLE dbo.BankAccounts
  (
	  BankAccountID int IDENTITY(1, 1) NOT NULL,
	  BankAccountName varchar(50) NOT NULL,
	  BankAccountType char(1) NOT NULL,
	  BankName varchar(50) NOT NULL,
	  BankRoutingNumber varchar(10) NOT NULL,
	  BankAccountNumber varchar(20) NOT NULL,
    CONSTRAINT PK_BankAccounts PRIMARY KEY CLUSTERED(BankAccountID)
  );

  CREATE UNIQUE INDEX ixu_dbo_BankAccounts_BankAccountName ON dbo.BankAccounts(BankAccountName);

END

GO

IF NOT EXISTS(SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.BankAccounts') AND name = 'IsActive')
BEGIN

  ALTER TABLE dbo.BankAccounts
  ADD IsActive bit NOT NULL DEFAULT(1);

END

GO

