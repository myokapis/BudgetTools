USE BudgetTools
GO

CREATE TABLE dbo.Transactions
(
	TransactionId int IDENTITY(1, 1) NOT NULL,
	BankAccountId int NOT NULL,
	TransactionNo varchar(255) NOT NULL,
	TransactionDate datetime NOT NULL,
	TransactionDesc varchar(255) NOT NULL,
	CheckNo int NULL,
	Amount money NOT NULL,
  TransactionTypeCode char(1) NOT NULL,
  Recipient varchar(255) NULL,
  Notes varchar(255) NULL,
	IsMapped bit NOT NULL,
  CONSTRAINT PK_dbo_Transactions PRIMARY KEY CLUSTERED(TransactionId),
  CONSTRAINT FK_dbo_Transactions_dbo_TransactionTypes FOREIGN KEY(TransactionTypeCode) REFERENCES dbo.TransactionTypes(TransactionTypeCode)
);
GO

CREATE UNIQUE INDEX ixu_dbo_Transactions_AltKey ON dbo.Transactions(BankAccountId, TransactionNo);
GO
