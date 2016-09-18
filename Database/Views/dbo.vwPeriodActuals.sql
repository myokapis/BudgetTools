CREATE VIEW dbo.vwPeriodActuals
AS

SELECT p.PeriodID, t.TransactionID, t.TransactionTypeCode, g.BudgetGroupID,
  g.BudgetGroupName, c.BudgetCategoryName, b.BudgetLineID, b.BudgetLineName,
  b.BudgetLineDesc, m.Amount, t.TransactionDate,
  t.Notes, t.BankAccountID, b.IsAccrued
FROM dbo.Transactions t
  INNER JOIN dbo.MappedTransactions m ON t.TransactionId = m.TransactionId
  INNER JOIN dbo.BudgetLines b ON m.BudgetLineID = b.BudgetLineID
  INNER JOIN dbo.BudgetCategories c ON b.BudgetCategoryID = c.BudgetCategoryID
  INNER JOIN dbo.BudgetGroups g ON c.BudgetGroupID = g.BudgetGroupID
  INNER JOIN dbo.Periods p ON t.TransactionDate BETWEEN p.PeriodStartDate AND p.PeriodEndDate
GO


