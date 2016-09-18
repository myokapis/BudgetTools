------------------------------------------------------------------------------
-- summarizes account balances for a period
------------------------------------------------------------------------------

DECLARE @PeriodID int = 201402;
DECLARE @UpdateBalances bit = 1;
DECLARE @ClosePeriod bit = 0;

DECLARE @PeriodBalances TABLE
(
  PeriodID int,
  BankAccountID int,
  BankAccountName varchar(100),
  BudgetGroupName varchar(100),
  BudgetCategoryName varchar(100),
  BudgetLineID int,
  BudgetLineName varchar(100),
  Balance money,
  SortIndex int
);

IF EXISTS(SELECT 1 FROM dbo.Periods WHERE PeriodID = @PeriodID AND IsOpen = 0)
  SELECT @UpdateBalances = 0;

IF @UpdateBalances = 1
  EXEC dbo.uspUpdatePeriodBalances @PeriodID, @ClosePeriod;

-- get a snapshot of the data
INSERT INTO @PeriodBalances(PeriodID, BankAccountID, BankAccountName,
  BudgetGroupName, BudgetCategoryName, BudgetLineID, BudgetLineName,
  Balance, SortIndex)
SELECT PeriodID, BankAccountID, BankAccountName, BudgetGroupName,
  BudgetCategoryName, BudgetLineID, BudgetLineName, Balance,
  RANK() OVER(ORDER BY BudgetLineName) AS SortIndex
FROM dbo.vwPeriodBalances
WHERE PeriodID = @PeriodID
  AND BankAccountID <= 3;
------------------------------------------------------------------------------
-- summarize the data
------------------------------------------------------------------------------

WITH
BaseData AS
(
  -- grand totals
  SELECT PeriodID, 'All Accounts' AS BankAccountName,
    'Overall Balance' AS BudgetLineName,
    SUM(Balance) AS Balance,
    0 AS SortKey
  FROM @PeriodBalances
  GROUP BY PeriodID
  -- bank account totals
  UNION ALL
  SELECT PeriodID, BankAccountName,
    'Account Balance' AS BudgetLineName,
    SUM(Balance) AS Balance,
    BankAccountID * 1000 AS SortKey
  FROM @PeriodBalances
  GROUP BY PeriodID, BankAccountID, BankAccountName
  -- budget line totals by bank account
  UNION ALL
  SELECT PeriodID, BankAccountName, BudgetLineName,
    SUM(Balance) AS Balance,
    BankAccountID * 1100 + SortIndex AS SortKey
  FROM @PeriodBalances
  GROUP BY PeriodID, BankAccountID, BankAccountName, BudgetLineName, SortIndex
)
SELECT PeriodID, BankAccountName, BudgetLineName, Balance
FROM BaseData
ORDER BY SortKey;
