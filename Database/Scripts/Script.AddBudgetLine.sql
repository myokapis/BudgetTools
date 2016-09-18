DECLARE @BudgetLineID int = 67;
DECLARE @BudgetCategoryID int = 1;
DECLARE @BudgetLineName varchar(255) = 'Judy';
DECLARE @BankAccountID int = 3;
DECLARE @IsAccrued bit = 1;

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

