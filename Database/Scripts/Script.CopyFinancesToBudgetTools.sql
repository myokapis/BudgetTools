-- loads transaction and mapped transaction data from Finances to BudgetTools

-- load transactions
SET IDENTITY_INSERT dbo.Transactions ON;
WITH
BaseData AS
(
  SELECT t.TransactionId, BankAccountId, TransactionNo, t.TransactionDate, TransactionDesc,
    CheckNumber AS CheckNo, t.Amount, t.TransactionTypeCode, Recipient, t.Note AS Notes,
    IsAssigned AS IsMapped
  FROM Finances.dbo.Transactions t
    INNER JOIN Finances.dbo.PeriodActuals a ON t.TransactionId = a.TransactionId
)
INSERT INTO dbo.Transactions(TransactionId, BankAccountId, TransactionNo, TransactionDate,
  TransactionDesc, CheckNo, Amount, TransactionTypeCode, Recipient, Notes, IsMapped)
SELECT DISTINCT b.TransactionId, b.BankAccountId, b.TransactionNo, b.TransactionDate, b.TransactionDesc,
  b.CheckNo, b.Amount, b.TransactionTypeCode, b.Recipient, b.Notes, b.IsMapped
FROM BaseData b
  LEFT JOIN dbo.Transactions t ON b.TransactionId = t.TransactionId
WHERE t.TransactionId IS NULL;
SET IDENTITY_INSERT dbo.Transactions OFF;

-- delete budgettools.dbo.transactions
-- truncate table budgettools.dbo.mappedtransactions

-- load mapped transactions
WITH
BaseData AS
(
  SELECT TransactionId, BudgetLineId, Amount
  FROM Finances.dbo.PeriodActuals
)
INSERT INTO dbo.MappedTransactions(TransactionId, BudgetLineId, Amount)
SELECT b.TransactionId, b.BudgetLineId, b.Amount
FROM BaseData b
  LEFT JOIN dbo.MappedTransactions m ON b.TransactionId = m.TransactionId
    AND b.BudgetLineId = m.BudgetLineId
WHERE m.TransactionId IS NULL;
