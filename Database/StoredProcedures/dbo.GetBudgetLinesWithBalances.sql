IF OBJECT_ID('dbo.GetBudgetLinesWithBalances', 'P') IS NULL
    EXEC('CREATE PROCEDURE dbo.GetBudgetLinesWithBalances AS SELECT 1;');
GO

ALTER PROCEDURE dbo.GetBudgetLinesWithBalances
    @BankAccountId int
AS

DECLARE @CurrentPeriodId int;

SELECT @CurrentPeriodID = MIN(CASE WHEN IsOpen = 1 THEN PeriodID ELSE NULL END)
FROM dbo.Periods WITH(NOLOCK);

IF @CurrentPeriodID IS NULL
    RAISERROR('Invalid Period', 16, 1);

SELECT bl.BudgetLineId, sbl.DisplayName, bl.IsAccrued, bc.BudgetCategoryId, bc.BudgetCategoryName,
    bc.BudgetCategoryDesc, bg.BudgetGroupId, bg.BudgetGroupName, bg.BudgetGroupDesc,
    ISNULL(b.Balance, 0.0) AS Balance
FROM dbo.BudgetLineSetBudgetLines sbl
INNER JOIN dbo.BudgetLineSets s ON sbl.BudgetLineSetId = s.BudgetLineSetId
INNER JOIN dbo.BudgetLines bl ON sbl.BudgetLineId = bl.BudgetLineId
INNER JOIN dbo.BudgetCategories bc ON bl.BudgetCategoryId = bc.BudgetCategoryId
INNER JOIN dbo.BudgetGroups bg ON bc.BudgetGroupId = bg.BudgetGroupId
LEFT JOIN dbo.PeriodBalances b ON bl.BudgetLineId = b.BudgetLineId
    AND b.BankAccountId = @BankAccountId
    AND b.PeriodId = @CurrentPeriodID
WHERE s.EffInDate <= GETDATE()
AND (s.EffOutDate IS NULL OR s.EffOutDate < GETDATE())
AND (bl.IsAccrued = 1 OR bg.BudgetGroupName = 'Assets');

GO

GRANT EXECUTE, VIEW DEFINITION ON dbo.GetBudgetLinesWithBalances TO exec_procs;
GO
