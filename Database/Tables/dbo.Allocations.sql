IF NOT EXISTS(SELECT * FROM sys.tables WHERE object_id = OBJECT_ID('dbo.Allocations'))
BEGIN

  CREATE TABLE dbo.Allocations
  (
    PeriodId int NOT NULL,
    BudgetLineId int NOT NULL,
    PlannedAmount money NOT NULL,
    AllocatedAmount money NOT NULL,
    AccruedAmount money NOT NULL,
    --CONSTRAINT CK_dbo_Allocations_PlannedAmount CHECK(PlannedAmount >= 0),
    --CONSTRAINT CK_dbo_Allocations_AllocatedAmount CHECK(AllocatedAmount >= 0),
    CONSTRAINT CK_dbo_Allocations_Balanced CHECK(PlannedAmount - AllocatedAmount - AccruedAmount = 0),
    CONSTRAINT FK_dbo_Allocations_dbo_Periods FOREIGN KEY(PeriodId) REFERENCES dbo.Periods(PeriodId),
    CONSTRAINT FK_dbo_Allocations_dbo_BudgetLines FOREIGN KEY(BudgetLineId) REFERENCES dbo.BudgetLines(BudgetLineId),
    CONSTRAINT PK_dboAllocations PRIMARY KEY CLUSTERED(PeriodId, BudgetLineId)
  );

END

IF NOT EXISTS
(
  SELECT *
  FROM sys.columns c
    INNER JOIN sys.tables t ON c.object_id = t.object_id
  WHERE c.name = 'BankAccountID'
    AND t.object_id = OBJECT_ID('dbo.Allocations')
)
BEGIN

  ALTER TABLE dbo.Allocations
  ADD BankAccountID int NOT NULL DEFAULT(1);

  ALTER TABLE dbo.Allocations
  DROP CONSTRAINT PK_dboAllocations;

  ALTER TABLE dbo.Allocations
  ADD CONSTRAINT PK_dboAllocations PRIMARY KEY CLUSTERED(PeriodId, BudgetLineId, BankAccountID);

END

ALTER TABLE dbo.Allocations
DROP CONSTRAINT CK_dbo_Allocations_Balanced;

ALTER TABLE dbo.Allocations
ADD CONSTRAINT CK_dbo_Allocations_Balanced CHECK(PlannedAmount = AllocatedAmount + AccruedAmount);
