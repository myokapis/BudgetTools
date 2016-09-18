CREATE VIEW dbo.vwPeriodBalanceLines
AS

SELECT a.BankAccountID, b.BudgetLineID, b.IsAccrued, b.IsAsset    
FROM dbo.BankAccounts a (NOLOCK), dbo.vwBudgetGroupCategoryLine b (NOLOCK)
WHERE b.IsAsset = 1
    OR b.IsAccrued = 1
GO
