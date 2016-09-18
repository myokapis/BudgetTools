CREATE TABLE dbo.BudgetGroups
(
	BudgetGroupID int IDENTITY(1, 1) NOT NULL,
	BudgetGroupName varchar(50) NOT NULL,
	BudgetGroupDesc varchar(255) NOT NULL,
  CONSTRAINT PK_BudgetGroups PRIMARY KEY CLUSTERED(BudgetGroupID)
);
