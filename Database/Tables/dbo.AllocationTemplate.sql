USE BudgetTools;
GO

IF NOT EXISTS(SELECT * FROM sys.tables WHERE object_id = OBJECT_ID('dbo.AllocationTemplate'))
BEGIN

  CREATE TABLE dbo.AllocationTemplate
  (
    BudgetLineId int NOT NULL,
    BankAccountId int NOT NULL,
    PlannedAmount money NOT NULL,
    AllocatedAmount money NOT NULL,
    AccruedAmount money NOT NULL,
    IsActive bit NOT NULL,
    --CONSTRAINT CK_dbo_AllocationTemplate_PlannedAmount CHECK(PlannedAmount >= 0),
    --CONSTRAINT CK_dbo_AllocationTemplate_AllocatedAmount CHECK(AllocatedAmount >= 0),
    CONSTRAINT CK_dbo_AllocationTemplate_Balanced CHECK(PlannedAmount - AllocatedAmount - AccruedAmount = 0),
    CONSTRAINT FK_dbo_AllocationTemplate_dbo_BudgetLines FOREIGN KEY(BudgetLineId) REFERENCES dbo.BudgetLines(BudgetLineId),
    CONSTRAINT FK_dbo_AllocationTemplate_dbo_BankAccounts FOREIGN KEY(BankAccountId) REFERENCES dbo.BankAccounts(BankAccountId),
    CONSTRAINT PK_dbo_AllocationTemplate PRIMARY KEY CLUSTERED(BudgetLineId)
  );

END

GO

DECLARE @IndexValue int;

SELECT @IndexValue = ISNULL(SUM(CASE WHEN c.name IN('BudgetLineId', 'BankAccountId') THEN 1 ELSE 10 END), 0)
FROM sys.indexes i
  INNER JOIN sys.index_columns ic ON i.object_id = ic.object_id
    AND i.index_id = ic.index_id
  INNER JOIN sys.columns c ON ic.object_id = c.object_id
    AND ic.column_id = c.column_id
WHERE i.object_id = OBJECT_ID('dbo.AllocationTemplate')
  AND i.is_primary_key = 1
GROUP BY c.name;

IF @IndexValue != 2
BEGIN

  IF @IndexValue > 0
    ALTER TABLE dbo.AllocationTemplate
    DROP CONSTRAINT PK_dbo_AllocationTemplate;

  ALTER TABLE dbo.AllocationTemplate
  ADD CONSTRAINT PK_dbo_AllocationTemplate PRIMARY KEY CLUSTERED(BudgetLineId, BankAccountId);

END
