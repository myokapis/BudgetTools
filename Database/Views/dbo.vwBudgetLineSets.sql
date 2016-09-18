CREATE VIEW dbo.vwBudgetLineSets
AS

SELECT s.BudgetLineSetId, s.EffInDate, s.EffOutDate, bl.BudgetLineId, bl.BudgetLineName,
  bl.BudgetLineDesc, sbl.DisplayName, bl.IsAccrued, bc.BudgetCategoryId, bc.BudgetCategoryName,
  bc.BudgetCategoryDesc, bg.BudgetGroupId, bg.BudgetGroupName, bg.BudgetGroupDesc
FROM dbo.BudgetLineSetBudgetLines sbl
  INNER JOIN dbo.BudgetLineSets s ON sbl.BudgetLineSetId = s.BudgetLineSetId
  INNER JOIN dbo.BudgetLines bl ON sbl.BudgetLineId = bl.BudgetLineId
  INNER JOIN dbo.BudgetCategories bc ON bl.BudgetCategoryId = bc.BudgetCategoryId
  INNER JOIN dbo.BudgetGroups bg ON bc.BudgetGroupId = bg.BudgetGroupId;
