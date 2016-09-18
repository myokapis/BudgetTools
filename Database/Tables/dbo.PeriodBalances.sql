CREATE TABLE dbo.PeriodBalances
(
	PeriodID int NOT NULL,
	BankAccountID int NOT NULL,
	BudgetLineID int NOT NULL,
	Balance money NOT NULL,
  CONSTRAINT FK_PeriodBalancesBankAccounts_BankAccountID FOREIGN KEY(BankAccountID) REFERENCES dbo.BankAccounts(BankAccountID),
  CONSTRAINT FK_PeriodBalancesBudgetLines_BudgetLineID FOREIGN KEY(BudgetLineID) REFERENCES dbo.BudgetLines(BudgetLineID),
  CONSTRAINT FK_PeriodBalancesPeriod_PeriodID FOREIGN KEY(PeriodID) REFERENCES dbo.Periods(PeriodID),
  CONSTRAINT PK_PeriodBalances PRIMARY KEY CLUSTERED(PeriodID, BankAccountID, BudgetLineID)
);
GO
