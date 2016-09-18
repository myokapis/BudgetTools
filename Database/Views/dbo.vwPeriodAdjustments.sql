CREATE VIEW dbo.vwPeriodAdjustments
AS

SELECT a.PeriodID, a.BankAccountID, a.BudgetLineID, b.BudgetLineName, b.BudgetLineDesc,
  b.BudgetCategoryId, b.IsAccrued, a.AdjustmentTypeCode, a.Amount
FROM dbo.PeriodAdjustments a
  INNER JOIN dbo.BudgetLines b ON a.BudgetLineID = b.BudgetLineID
