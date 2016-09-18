USE BudgetTools;

DECLARE @PeriodID int = 201406;

EXEC dbo.uspUpdatePeriodBalances @PeriodID, 0

DECLARE @PeriodID int = 201406;
SELECT *
FROM dbo.PeriodBalances
WHERE PeriodID = @PeriodID

DECLARE @PeriodID int = 201406;
SELECT BankAccountID, SUM(Balance)
FROM dbo.PeriodBalances
WHERE PeriodID = @PeriodID
GROUP BY BankAccountID;

/*
DECLARE @PeriodID int = 201601;
DECLARE @ClosePeriod bit = 1;

EXEC dbo.uspUpdatePeriodBalances @PeriodID, @ClosePeriod;

-- run this to create allocations for the next period
DECLARE @PeriodID int = 201602;
EXEC dbo.uspCreateAllocations @PeriodID

SELECT *
FROM dbo.PeriodBalances
WHERE PeriodID = @PeriodID

*/
