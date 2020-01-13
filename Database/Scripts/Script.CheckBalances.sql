USE BudgetTools;

DECLARE @PeriodID int = 201712;
EXEC dbo.uspUpdatePeriodBalances @PeriodID, 0
GO

DECLARE @PeriodID int = 201611;
SELECT *
FROM dbo.PeriodBalances
WHERE PeriodID = @PeriodID
GO

DECLARE @PeriodID int = 201610;
SELECT BankAccountID, SUM(Balance)
FROM dbo.PeriodBalances
WHERE PeriodID = @PeriodID
GROUP BY BankAccountID;

/*
DECLARE @PeriodID int = 201803;
DECLARE @ClosePeriod bit = 1;

EXEC dbo.uspUpdatePeriodBalances @PeriodID, @ClosePeriod;

-- run this to create allocations for the next period
DECLARE @PeriodID int = 201804;
EXEC dbo.uspCreateAllocations @PeriodID

SELECT *
FROM dbo.PeriodBalances
WHERE PeriodID = @PeriodID

*/
