IF OBJECT_ID('dbo.BudgetLines', 'U') IS NULL
BEGIN

CREATE TABLE dbo.BudgetLines
(
  BudgetLineId int NOT NULL IDENTITY(1, 1),
  BudgetCategoryId int NOT NULL,
  BudgetLineName varchar(100) NOT NULL,
  BudgetLineDesc varchar(255) NOT NULL,
  IsAccrued bit NOT NULL,
  CONSTRAINT FK_dbo_BudgetLines_dbo_BudgetCategories FOREIGN KEY(BudgetCategoryId) REFERENCES dbo.BudgetCategories(BudgetCategoryId),
  CONSTRAINT PK_dbo_BudgetLines PRIMARY KEY CLUSTERED(BudgetLineId)
);

--CREATE TABLE dbo.BudgetLineSets
--(
--  BudgetLineSetId int NOT NULL IDENTITY(1, 1),
--  EffInDate date NOT NULL,
--  EffOutDate date NULL,
--  CONSTRAINT PK_dbo_BudgetLineSets PRIMARY KEY CLUSTERED(BudgetLineSetId)
--);

--CREATE TABLE dbo.BudgetLineSetBudgetLines
--(
--  BudgetLineSetId int NOT NULL,
--  BudgetLineId int NOT NULL,
--  DisplayName varchar(100) NOT NULL,
--  CONSTRAINT PK_dbo_BudgetLineSetBudgetLines PRIMARY KEY CLUSTERED(BudgetLineSetId, BudgetLineId)
--);

END

IF NOT EXISTS(SELECT * FROM sys.columns WHERE name = 'IsCashOffset' AND object_id = OBJECT_ID('dbo.BudgetLines'))
BEGIN

  ALTER TABLE dbo.BudgetLines
  ADD IsCashOffset bit NOT NULL DEFAULT(1);

END