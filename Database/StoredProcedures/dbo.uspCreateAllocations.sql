USE BudgetTools;
GO

CREATE PROCEDURE dbo.uspCreateAllocations
  @PeriodId                   int
AS

SET NOCOUNT ON;
DECLARE @PriorPeriodId int;
DECLARE @CashBudgetLineId int;

-- get the prior period
SELECT @PriorPeriodId = CONVERT(varchar(6), DATEADD(month, -1, CONVERT(date, CAST(@PeriodId * 100 + 1 AS varchar(8)), 112)), 112);

-- get the cash account
SELECT @CashBudgetLineId = BudgetLineId
FROM dbo.BudgetLines
WHERE BudgetLineName = 'Cash';

-- add planned amounts
INSERT INTO dbo.Allocations(PeriodId, BudgetLineId, PlannedAmount,
  AllocatedAmount, AccruedAmount, BankAccountID)
SELECT @PeriodId, at.BudgetLineId, at.PlannedAmount, at.AllocatedAmount,
  at.AccruedAmount, at.BankAccountId
FROM dbo.AllocationTemplate at
  LEFT JOIN dbo.Allocations a ON at.BudgetLineId = a.BudgetLineId
    AND at.BankAccountId = a.BankAccountId
    AND a.PeriodId = @PeriodId
WHERE at.IsActive = 1
  AND a.BudgetLineId IS NULL;

-- add any budget lines for which a balance exists and that are not in the plan
WITH
BudgetLines AS
(
  SELECT BudgetLineId, BankAccountId
  FROM dbo.PeriodBalances
  WHERE PeriodId IN(@PeriodId, @PriorPeriodId)
    AND Balance != 0.0
    AND BudgetLineId != @CashBudgetLineId
  GROUP BY BudgetLineId, BankAccountId
)
INSERT INTO dbo.Allocations(PeriodId, BudgetLineId, PlannedAmount,
  AllocatedAmount, AccruedAmount, BankAccountID)
SELECT @PeriodId, bl.BudgetLineId, 0.0 AS PlannedAmount,
  0.0 AS AllocatedAmount, 0.0 AS AccruedAmount, bl.BankAccountId
FROM BudgetLines bl
  LEFT JOIN dbo.Allocations a ON bl.BudgetLineId = a.BudgetLineId
    AND bl.BankAccountId = a.BankAccountId
    AND a.PeriodId = @PeriodId
WHERE a.BudgetLineId IS NULL;

GO