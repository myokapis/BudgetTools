IF OBJECT_ID('dbo.CloseCurrentPeriod', 'P') IS NULL
    EXEC('CREATE PROCEDURE dbo.CloseCurrentPeriod AS SELECT 1;');
GO

ALTER PROCEDURE dbo.CloseCurrentPeriod
AS

SET NOCOUNT ON;

DECLARE @PeriodID int;
DECLARE @PeriodStartDate date;
DECLARE @PeriodEndDate date;
DECLARE @ErrorCount int = 0;

DECLARE @FinalBalances TABLE
(
    BankAccountId int,
    TransactionId int,
    Balance money
);

DECLARE @Errors TABLE
(
    ErrorText varchar(255)
);

-- get current period
WITH
CurrentPeriod AS
(
    SELECT MIN(PeriodID) AS PeriodId
    FROM dbo.Periods
    WHERE IsOpen = 1
)
SELECT @PeriodID = p.PeriodId,
    @PeriodStartDate = p.PeriodStartDate,
    @PeriodEndDate = p.PeriodEndDate
FROM dbo.Periods p
INNER JOIN CurrentPeriod cp ON p.PeriodId = cp.PeriodId;
    
-- get final bank account balances for the period
WITH
LastTransactions AS
(
    SELECT BankAccountId, MAX(TransactionId) AS TransactionId
    FROM dbo.Transactions
    WHERE TransactionDate <= @PeriodEndDate
    AND TransactionTypeCode != 'I'
    GROUP BY BankAccountId
)
INSERT INTO @FinalBalances(BankAccountId, TransactionId, Balance)
SELECT t.BankAccountId, t.TransactionId, t.Balance
FROM dbo.Transactions t
INNER JOIN dbo.BankAccounts b on t.BankAccountId = b.BankAccountId
INNER JOIN LastTransactions lt ON t.BankAccountId = lt.BankAccountId
    AND t.TransactionId = lt.TransactionId
WHERE b.IsActive = 1;

-- make sure final bank balances match the calculated balances
INSERT INTO @Errors(ErrorText)
SELECT a.BankAccountName + ', is out of balance. '
    + 'The transaction balance is ' + FORMAT(ISNULL(f.Balance, 0.0), 'C', 'en-us') + '. '
    + 'The period balance is ' + FORMAT(ISNULL(b.Balance, 0.0), 'C', 'en-us') + '.'
FROM dbo.BankAccounts a
LEFT JOIN @FinalBalances f ON a.BankAccountId = f.BankAccountId
LEFT JOIN
(
    SELECT BankAccountId, SUM(Balance) AS Balance
    FROM dbo.PeriodBalances
    WHERE PeriodId = @PeriodId
    GROUP BY BankAccountId
) b ON a.BankAccountId = b.BankAccountId
WHERE ISNULL(f.Balance, 0.0) != ISNULL(b.Balance, 0.0);

-- only the cash account is allowed to be negative
INSERT INTO @Errors(ErrorText)
SELECT l.BudgetLineName + ' in ' + a.BankAccountName + ' has a negative balance.'
FROM dbo.PeriodBalances b
INNER JOIN dbo.BudgetLines l ON b.BudgetLineId = l.BudgetLineId
INNER JOIN dbo.BankAccounts a ON b.BankAccountId = a.BankAccountId
WHERE b.PeriodId = @PeriodId
AND b.Balance < 0.0
AND b.BudgetLineId != dbo.GetParameter('Cash');

WITH
Actuals AS
(
    SELECT t.BankAccountId, m.BudgetLineId,
        -SUM(m.Amount) AS TotalAmount
    FROM dbo.MappedTransactions m
    INNER JOIN dbo.Transactions t ON m.TransactionId = t.TransactionId
    WHERE t.TransactionDate >= @PeriodStartDate
    AND t.TransactionDate <= @PeriodEndDate
    GROUP BY t.BankAccountId, m.BudgetLineId
),
Planned AS
(
    SELECT a.BankAccountId, a.BudgetLineId, v.BudgetLineName,
        a.PlannedAmount
        + CASE
            WHEN v.BudgetGroupName = 'Expenses' AND a.AccruedAmount < 0.0 THEN -a.AccruedAmount
            WHEN v.BudgetGroupName != 'Expenses' AND a.AccruedAmount > 0.0 THEN -a.AccruedAmount
            ELSE 0.0
        END AS PlannedAmount
    FROM dbo.Allocations a
    INNER JOIN dbo.vwBudgetGroupCategoryLine v ON a.BudgetLineId = v.BudgetLineId
    WHERE PeriodId = @PeriodId
)
INSERT INTO @Errors(ErrorText)
SELECT p.BudgetLineName + ' in ' + b.BankAccountName + ' has a remaining balance.'
FROM Planned p
INNER JOIN dbo.BankAccounts b ON p.BankAccountId = b.BankAccountId
LEFT JOIN Actuals a ON p.BankAccountId = a.BankAccountId
    AND p.BudgetLineId = a.BudgetLineId
WHERE p.PlannedAmount != ISNULL(a.TotalAmount, 0.0);

SELECT @ErrorCount = COUNT(0)
FROM @Errors;

IF @ErrorCount = 0
BEGIN

    UPDATE p
    SET IsOpen = 0
    FROM dbo.Periods p
    WHERE PeriodId = @PeriodId;

    INSERT INTO @Errors(ErrorText)
    VALUES('Period successfully closed.');

END

SELECT CASE WHEN @ErrorCount = 0 THEN 0 ELSE 16 END AS ErrorLevel,
    ErrorText AS MessageText
FROM @Errors;

RETURN CASE WHEN @ErrorCount = 0 THEN 0 ELSE 16 END;

GO

grant execute, view definition on dbo.CloseCurrentPeriod to exec_procs;
go


