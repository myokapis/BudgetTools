alter VIEW dbo.vwPeriodPlanned
AS

SELECT a.PeriodID, a.BudgetLineID, b.BudgetLineName, b.BudgetLineDesc, a.PlannedAmount,
  a.AllocatedAmount, a.AccruedAmount, b.IsAccrued, BankAccountID
FROM dbo.Allocations a
  INNER JOIN dbo.BudgetLines b ON a.BudgetLineID = b.BudgetLineID
GO


