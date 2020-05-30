CREATE OR ALTER PROCEDURE dbo.CloseCurrentPeriod
AS

SET NOCOUNT ON;

DECLARE @PeriodID int;
DECLARE @PeriodStartDate date;
DECLARE @PeriodEndDate date;
DECLARE @NextPeriodStartDate date;
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
    @PeriodEndDate = p.PeriodEndDate,
	@NextPeriodStartDate = DATEADD(day, 1, p.PeriodEndDate)
FROM dbo.Periods p
INNER JOIN CurrentPeriod cp ON p.PeriodId = cp.PeriodId;
    
-- get final bank account balances for the period
INSERT INTO @FinalBalances(BankAccountId, TransactionId, Balance)
SELECT b.BankAccountId, xt.TransactionId,
    ISNULL(t.Balance, 0.0) AS Balance
FROM dbo.BankAccounts b
OUTER APPLY
(
    SELECT TOP 1 tr.TransactionId
    FROM dbo.Transactions tr
    WHERE tr.TransactionTypeCode IN('S', 'X')
    AND b.BankAccountId = tr.BankAccountId
	AND tr.TransactionDate < @NextPeriodStartDate
    ORDER BY tr.TransactionId DESC
) xt
LEFT JOIN dbo.Transactions t ON b.BankAccountId = t.BankAccountId
    AND xt.TransactionId = t.TransactionId
WHERE b.IsActive = 1;

-- TODO: reinstate this after i fix the import to keep the transaction order and after i update ending balances in all accounts
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
    AND t.TransactionTypeCode IN('S', 'X')
    GROUP BY t.BankAccountId, m.BudgetLineId
),
Planned AS
(
    SELECT a.BankAccountId, a.BudgetLineId, v.BudgetLineName,
        a.AllocatedAmount + IIF(a.AccruedAmount > 0.0, a.AccruedAmount, 0.0) AS ExpectedActualAmount
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
WHERE p.ExpectedActualAmount != ISNULL(a.TotalAmount, 0.0);

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


