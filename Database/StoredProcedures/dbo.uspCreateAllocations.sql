IF OBJECT_ID('dbo.uspCreateAllocations', 'P') IS NULL
    EXEC('CREATE PROCEDURE dbo.uspCreateAllocations AS SELECT 1;');
GO

ALTER PROCEDURE dbo.uspCreateAllocations
    @PeriodId int
AS

SET NOCOUNT ON;
DECLARE @PreviousPeriodID int;
DECLARE @ThisPeriodID int;
DECLARE @PrevPeriodID int;
DECLARE @CashBudgetLineId int;

DECLARE @Periods TABLE
(
    PeriodId int NOT NULL PRIMARY KEY,
    IsOpen bit NOT NULL
);

-- get current and previous periods
SELECT @PreviousPeriodID = MAX(PeriodID)
FROM dbo.Periods (NOLOCK)
WHERE IsOpen = 0;

-- get the cash account
SELECT @CashBudgetLineId = BudgetLineId
FROM dbo.BudgetLines
WHERE BudgetLineName = 'Cash';

-- get periods to process
INSERT INTO @Periods(PeriodId, IsOpen)
SELECT PeriodId, IsOpen
FROM dbo.Periods
WHERE PeriodId BETWEEN @PreviousPeriodID AND @PeriodId;

-- add planned amounts
INSERT INTO dbo.Allocations(PeriodId, BudgetLineId, PlannedAmount,
  AllocatedAmount, AccruedAmount, BankAccountID)
SELECT p.PeriodId, t.BudgetLineId, t.PlannedAmount, t.AllocatedAmount,
    t.AccruedAmount, t.BankAccountId
FROM dbo.AllocationTemplate t
CROSS JOIN @Periods p
WHERE t.IsActive = 1
AND p.IsOpen = 1
AND NOT EXISTS
(
    SELECT 1
    FROM dbo.Allocations a
    WHERE a.BudgetLineId = t.BudgetLineId
    AND a.BankAccountId = t.BankAccountId
    AND a.PeriodId = p.PeriodId
);

-- add any budget lines for which a balance exists and that are not in the plan
WITH
BudgetLines AS
(
    SELECT b.BudgetLineId, b.BankAccountId
    FROM dbo.PeriodBalances b
    INNER JOIN @Periods pp ON b.PeriodId = pp.PeriodId
    WHERE Balance != 0.0
    AND BudgetLineId != @CashBudgetLineId
    GROUP BY b.BudgetLineId, b.BankAccountId
)
INSERT INTO dbo.Allocations(PeriodId, BudgetLineId, PlannedAmount,
    AllocatedAmount, AccruedAmount, BankAccountID)
SELECT @PeriodId, bl.BudgetLineId, 0.0 AS PlannedAmount,
    0.0 AS AllocatedAmount, 0.0 AS AccruedAmount, bl.BankAccountId
FROM BudgetLines bl
CROSS JOIN @Periods p
WHERE p.IsOpen = 1
AND NOT EXISTS
(
    SELECT 1
    FROM dbo.Allocations a
    WHERE a.BudgetLineId = bl.BudgetLineId
    AND a.BankAccountId = bl.BankAccountId
    AND a.PeriodId = p.PeriodId
);

GO

grant execute, view definition on dbo.uspCreateAllocations to exec_procs;
go
