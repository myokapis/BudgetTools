CREATE TABLE dbo.MappedTransactions
(
  MappedTransactionid int NOT NULL IDENTITY(1, 1),
  TransactionId int NOT NULL,
  BudgetLineId int NOT NULL,
  Amount money NOT NULL,
  CONSTRAINT PK_dbo_MappedTransactions PRIMARY KEY NONCLUSTERED(MappedTransactionId),
  CONSTRAINT FK_dbo_MappedTransactions_dbo_Transactions FOREIGN KEY(TransactionId) REFERENCES dbo.Transactions(TransactionId),
  CONSTRAINT FK_dbo_MappedTransactions_dbo_BudgetLines FOREIGN KEY(BudgetLineId) REFERENCES dbo.BudgetLines(BudgetLineId)
);

CREATE UNIQUE CLUSTERED INDEX ixclu_dbo_MappedTransactions ON dbo.MappedTransactions(TransactionId, BudgetLineId);
