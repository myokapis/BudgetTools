CREATE TABLE dbo.BudgetLineSets
(
	BudgetLineSetId int IDENTITY(1, 1) NOT NULL,
	EffInDate date NOT NULL,
	EffOutDate date NULL,
  CONSTRAINT PK_dbo_BudgetLineSets PRIMARY KEY CLUSTERED(BudgetLineSetId)
);
GO
