USE BudgetTools;
GO

CREATE VIEW dbo.vwPeriodBalances
AS

SELECT b.PeriodID, b.BankAccountID, a.BankAccountName, gcl.BudgetGroupName,
  gcl.BudgetCategoryName, b.BudgetLineID, gcl.BudgetLineName,
  ISNULL(pb.Balance, 0.0) AS PreviousBalance,
  b.Balance, b.ProjectedBalance
FROM dbo.PeriodBalances b
  INNER JOIN dbo.vwBudgetGroupCategoryLine gcl ON b.BudgetLineID = gcl.BudgetLineID
  INNER JOIN dbo.BankAccounts a ON b.BankAccountID = a.BankAccountID
  LEFT JOIN dbo.PeriodBalances pb ON b.BankAccountID = pb.BankAccountID
    AND b.BudgetLineID = pb.BudgetLineID
    AND pb.PeriodID = CAST(CONVERT(char(6), DATEADD(month, -1, CONVERT(date, CAST(b.PeriodID * 100 + 1 AS char(8)), 112)), 112) AS int)

