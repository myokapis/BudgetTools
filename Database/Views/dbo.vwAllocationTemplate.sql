CREATE VIEW dbo.vwAllocationTemplate
AS

SELECT b.BudgetLineID, a.BankAccountID, b.BudgetLineName, b.BudgetLineDesc, b.IsAccrued,
  b.BudgetCategoryID, a.PlannedAmount, a.AllocatedAmount, a.AccruedAmount
FROM dbo.AllocationTemplate a
  INNER JOIN dbo.BudgetLines b ON a.BudgetLineID = b.BudgetLineID
