CREATE TABLE dbo.BudgetCategories
(
	BudgetCategoryID int IDENTITY(1, 1) NOT NULL,
	BudgetCategoryName varchar(50) NOT NULL,
	BudgetCategoryDesc varchar(255) NOT NULL,
	BudgetGroupID int NOT NULL,
  CONSTRAINT FK_BudgetCategoriesBudgetGroup_BudgetGroupID FOREIGN KEY(BudgetGroupID) REFERENCES dbo.BudgetGroups (BudgetGroupID),
  CONSTRAINT PK_BudgetCategories PRIMARY KEY CLUSTERED(BudgetCategoryID)
);
