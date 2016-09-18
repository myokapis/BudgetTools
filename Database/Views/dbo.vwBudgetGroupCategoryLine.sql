CREATE VIEW dbo.vwBudgetGroupCategoryLine
AS

SELECT a.BudgetGroupID, a.BudgetGroupName, a.BudgetGroupDesc,
    b.BudgetCategoryID, b.BudgetCategoryName, b.BudgetCategoryDesc,
    c.BudgetLineID, c.BudgetLineName, c.BudgetLineDesc, c.IsAccrued,
    IsCashOffset,
    CASE WHEN a.BudgetGroupName = 'Assets' THEN 1 ELSE 0 END AS IsAsset
FROM dbo.BudgetGroups a
    INNER JOIN dbo.BudgetCategories b ON a.BudgetGroupID = b.BudgetGroupID
    INNER JOIN dbo.BudgetLines c ON b.BudgetCategoryID = c.BudgetCategoryID
GO
