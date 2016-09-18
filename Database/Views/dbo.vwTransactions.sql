CREATE VIEW dbo.vwTransactions
AS

SELECT p.PeriodID, t.TransactionID, t.BankAccountId, t.TransactionNo, t.TransactionDate,
  t.TransactionDesc, t.CheckNo, t.Amount AS TotalTransactionAmount, t.TransactionTypeCode,
  t.Recipient, t.Notes, t.IsMapped, m.MappedTransactionId, m.BudgetLineId, m.Amount
FROM dbo.Transactions t
  INNER JOIN dbo.MappedTransactions m ON t.TransactionId = m.TransactionId
  INNER JOIN dbo.Periods p ON t.TransactionDate BETWEEN p.PeriodStartDate AND p.PeriodEndDate
