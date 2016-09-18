DECLARE @CurrentPeriodStartDate date = '20141201';
DECLARE @CurrentPeriodEndDate date = '20141231';

WITH
actuals AS
(
  SELECT t.BankAccountID, t.TransactionTypeCode, SUM(m.Amount) AS TotalAmount
  FROM dbo.Transactions t (NOLOCK)
    INNER JOIN dbo.MappedTransactions m (NOLOCK) ON t.TransactionId = m.TransactionId
  WHERE t.TransactionDate >= @CurrentPeriodStartDate
    AND t.TransactionDate <= @CurrentPeriodEndDate
  GROUP BY t.BankAccountID, t.TransactionTypeCode
),
trans AS
(
    SELECT BankAccountID, TransactionTypeCode,
        SUM(Amount) AS TotalAmount
    FROM dbo.Transactions (NOLOCK)
    WHERE TransactionDate >= @CurrentPeriodStartDate
      AND TransactionDate <= @CurrentPeriodEndDate
    GROUP BY BankAccountID, TransactionTypeCode
)
SELECT ISNULL(a.BankAccountID, b.BankAccountID) AS BankAccountID,
  ISNULL(a.TransactionTypeCode, b.TransactionTypeCode) AS TransactionTypeCode,
*
FROM actuals a
  FULL OUTER JOIN trans b ON a.BankAccountID = b.BankAccountID
      AND a.TransactionTypeCode = b.TransactionTypeCode
WHERE ISNULL(a.TotalAmount, 0) != ISNULL(b.TotalAmount, 0)