CREATE TABLE dbo.BudgetLineSetBudgetLines
(
	BudgetLineSetId int NOT NULL,
	BudgetLineId int NOT NULL,
	DisplayName varchar(100) NOT NULL,
  CONSTRAINT FK_dbo_BudgetLineSetBudgetLines_dbo_BudgetLineSets FOREIGN KEY(BudgetLineSetId) REFERENCES dbo.BudgetLineSets(BudgetLineSetId),
  CONSTRAINT FK_dbo_BudgetLineSetBudgetLines_dbo_BudgetLines FOREIGN KEY(BudgetLineId) REFERENCES dbo.BudgetLines(BudgetLineId),
  CONSTRAINT PK_dbo_BudgetLineSetBudgetLines PRIMARY KEY CLUSTERED(BudgetLineSetId, BudgetLineId)
);