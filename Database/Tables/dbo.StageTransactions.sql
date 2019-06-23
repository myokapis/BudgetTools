USE BudgetTools;
GO

CREATE TABLE dbo.StageTransactions
(
	StageTransactionId int IDENTITY(1, 1) NOT NULL,
	BankAccountId int NOT NULL,
	TransactionNo varchar(255) NOT NULL,
	TransactionDate datetime NOT NULL,
	TransactionDesc varchar(255) NOT NULL,
	CheckNo int NULL,
	Amount float NOT NULL,
  CONSTRAINT PK_dbo_StageTransactions PRIMARY KEY NONCLUSTERED(StageTransactionId)
);

CREATE CLUSTERED INDEX ixcl_dbo_StageTransactions ON dbo.StageTransactions(BankAccountId, TransactionNo);
GO

sp_rename 'dbo.StageTransactions', 'StagedTransactions';
