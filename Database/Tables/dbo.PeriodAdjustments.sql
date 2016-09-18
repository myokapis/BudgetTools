CREATE TABLE dbo.PeriodAdjustments
(
	PeriodID int NOT NULL,
	BankAccountID int NOT NULL,
	BudgetLineID int NOT NULL,
	AdjustmentTypeCode char(1) NOT NULL,
	Amount money NOT NULL,
  CONSTRAINT FK_PeriodAdjustmentsAdjustmentTypes_AdjustmentTypeCode FOREIGN KEY(AdjustmentTypeCode) REFERENCES dbo.AdjustmentTypes(AdjustmentTypeCode),
  CONSTRAINT FK_PeriodAdjustmentsBankAccounts_BankAccountID FOREIGN KEY(BankAccountID) REFERENCES dbo.BankAccounts(BankAccountID),
  CONSTRAINT FK_PeriodAdjustmentsBudgetLines_BudgetLineID FOREIGN KEY(BudgetLineID) REFERENCES dbo.BudgetLines(BudgetLineID),
  CONSTRAINT FK_PeriodAdjustmentsPeriods_PeriodID FOREIGN KEY(PeriodID) REFERENCES dbo.Periods(PeriodID),
  CONSTRAINT PK_PeriodAdjustments PRIMARY KEY CLUSTERED(PeriodID, BankAccountID, BudgetLineID, AdjustmentTypeCode)
);
GO
