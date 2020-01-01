DECLARE @BudgetLineID int = 75;
DECLARE @BudgetCategoryID int = 8;
DECLARE @BudgetLineName varchar(255) = 'Wish List';
DECLARE @BankAccountID int = 1;
DECLARE @IsAccrued bit = 0;

BEGIN TRANSACTION

SET IDENTITY_INSERT dbo.BudgetLines ON;

INSERT INTO dbo.BudgetLines(BudgetLineId, BudgetCategoryId, BudgetLineName,
  BudgetLineDesc, IsAccrued, IsCashOffset)
VALUES(@BudgetLineID, @BudgetCategoryID, @BudgetLineName, @BudgetLineName, @IsAccrued, 0);

SET IDENTITY_INSERT dbo.BudgetLines OFF;

INSERT INTO dbo.BudgetLineSetBudgetLines(BudgetLineSetId, BudgetLineId, DisplayName)
VALUES(1, @BudgetLineID, @BudgetLineName);

INSERT INTO dbo.AllocationTemplate(BudgetLineId, BankAccountId, PlannedAmount,
  AllocatedAmount, AccruedAmount, IsActive)
VALUES(@BudgetLineID, @BankAccountID, 0.00, 0.00, 0.00, 1);

INSERT INTO dbo.Allocations(PeriodId, BudgetLineId, PlannedAmount, AllocatedAmount,
  AccruedAmount, BankAccountID)
SELECT CONVERT(char(6), GETDATE(), 112), @BudgetLineID, 0.00, 0.00, 0.00, @BankAccountID;

-- COMMIT -- ROLLBACK

/*

select *
from dbo.BudgetLines

select *
from dbo.BudgetCategories

select *
from dbo.vwBudgetGroupCategoryLine
where BudgetCategoryName = 'Giving'

*/
