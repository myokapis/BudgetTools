IF OBJECT_ID('dbo.uspUpdatePeriodBalances', 'P') IS NULL
    EXEC('CREATE PROCEDURE dbo.uspUpdatePeriodBalances AS SELECT 1;');
GO

ALTER PROCEDURE dbo.uspUpdatePeriodBalances
    @PeriodId int = null,
    @ClosePeriod bit = 0
AS
-- TODO: refactor proc to allow any open period to have its balances updated
SET NOCOUNT ON

DECLARE @BeginTranCount                 int;
DECLARE @CurrentPeriodID                int
DECLARE @PreviousPeriodID               int
DECLARE @CurrentPeriodStartDate         date
DECLARE @CurrentPeriodEndDate           date
DECLARE @CashBudgetLineID               int
DECLARE @DefaultAccountId               int
DECLARE @DefaultMoney                   money;
DECLARE @ErrorCount                     int = 0;
DECLARE @ErrorMessage			              nvarchar(4000)
DECLARE @ErrorSeverity			            int

DECLARE @Errors TABLE
(
    ErrorText                           varchar(255)
);

IF OBJECT_ID('tempdb.dbo.#Actuals') IS NOT NULL
  DROP TABLE #Actuals;

CREATE TABLE #Actuals
(
  BankAccountId int NOT NULL,
  BudgetLineId int NOT NULL,
  TransactionTypeCode char(1) NOT NULL,
  DebitCreditFlag char(1) NOT NULL,
  ActualAmount money NOT NULL,
  PRIMARY KEY(BankAccountId, BudgetLineId, TransactionTypeCode, DebitCreditFlag)
);

IF OBJECT_ID('tempdb.dbo.#Allocations') IS NOT NULL
  DROP TABLE #Allocations;

CREATE TABLE #Allocations
(
  BankAccountID int NOT NULL,
  BudgetLineID int NOT NULL,
  PlannedAmount money NOT NULL,
  AllocatedAmount money NOT NULL,
  AccruedAmount money NOT NULL,
  PRIMARY KEY(BankAccountId, BudgetLineId)
);

IF OBJECT_ID('tempdb.dbo.#LineSummary') IS NOT NULL
  DROP TABLE #LineSummary;

CREATE TABLE #LineSummary
(
  BankAccountId int NOT NULL,
  BudgetLineId int NOT NULL,
  DebitCreditFlag char(1) NOT NULL,
  BudgetGroupName varchar(50) NOT NULL,
  IsAccrued bit NOT NULL,
  IsCashOffset bit NOT NULL,
  TransferAmount money NOT NULL,
  StandardAmount money NOT NULL,
  ActualAmount money NOT NULL,
  PlannedAmount money NOT NULL,
  AllocatedAmount money NOT NULL,
  AccruedAmount money NOT NULL,
  PRIMARY KEY(BankAccountId, BudgetLineId, DebitCreditFlag, BudgetGroupName)
);

IF OBJECT_ID('tempdb.dbo.#PeriodBalances') IS NOT NULL
  DROP TABLE #PeriodBalances;

CREATE TABLE #PeriodBalances
(
    BankAccountID	                    int,
    BudgetLineID	                    int,
    Balance	                          money
);

IF OBJECT_ID('tempdb.dbo.#Adjustments') IS NOT NULL
  DROP TABLE #Adjustments;

CREATE TABLE #Adjustments
(
    BankAccountID                   int,
    BudgetLineID                    int,
    AdjustmentTypeCode              char(1),
    CashAdjustment                  money,
    LineAdjustment                  money,
    ProjectedCashAdjustment         money,
    ProjectedLineAdjustment         money
);

IF OBJECT_ID('tempdb.dbo.#Consolidated') IS NOT NULL
  DROP TABLE #Consolidated;

CREATE TABLE #Consolidated
(
  BankAccountID                   int,
  BudgetLineID                    int,
  AdjustmentTypeCode              char(1),
  TotalAmount                     money,
  ProjectedAmount                 money,
  PRIMARY KEY(BankAccountID, BudgetLineID, AdjustmentTypeCode)
);

----IF OBJECT_ID('tempdb.dbo.#BudgetSummary') IS NOT NULL
----  DROP TABLE #BudgetSummary;

----CREATE TABLE #BudgetSummary
----(
----  BankAccountID                   int NOT NULL,
----  BudgetLineID                    int NOT NULL,
----  BudgetGroupName                 varchar(50),
----  IsAccrued                       bit NOT NULL,
----  IsCashOffset                    bit NOT NULL,
----  PlannedAmount                   money NOT NULL,
----  AllocatedAmount                 money NOT NULL,
----  AccruedAmount                   money NOT NULL,
----  ActualAmount                    money NOT NULL,
----  CAccruedAmount                  money NOT NULL,
----  PRIMARY KEY(BankAccountID, BudgetLineID, BudgetGroupName)
----);

BEGIN TRY

------------------------------------------------------------------------------
-- setup
------------------------------------------------------------------------------

  -- set default money value
  SELECT @DefaultMoney = 0.00,
    @BeginTranCount = @@TRANCOUNT;

  -- get current and previous periods
  SELECT @CurrentPeriodID = MIN(CASE WHEN IsOpen = 1 THEN PeriodID ELSE 999999 END),
      @PreviousPeriodID = MAX(CASE WHEN IsOpen = 0 THEN PeriodID ELSE 0 END)
  FROM dbo.Periods (NOLOCK);

    -- can't update balances on a closed period
    IF @PeriodID < @CurrentPeriodID
    BEGIN
        INSERT INTO @Errors(ErrorText)
        VALUES('Unable to update balances on a closed period.')

        SELECT @ErrorCount = @ErrorCount + 1;
        GOTO ExitProc;
    END

    -- current and previous period must be valid
    IF @CurrentPeriodID IS NULL OR @PreviousPeriodID IS NULL
    BEGIN
        INSERT INTO @Errors(ErrorText)
        VALUES('Unable to update balances on an invalid period.')

        SELECT @ErrorCount = @ErrorCount + 1;
        GOTO ExitProc;
    END

  -- get the starting and ending dates for the current period
  SELECT @CurrentPeriodStartDate = PeriodStartDate,
    @CurrentPeriodEndDate = PeriodEndDate
  FROM dbo.Periods
  WHERE PeriodId = @CurrentPeriodID;

  -- get the cash account
  SELECT @CashBudgetLineID = dbo.GetParameter('Cash');

  -- get the default account
  SELECT @DefaultAccountId = dbo.GetParameter('DefaultAccountId');

------------------------------------------------------------------------------
-- validations
------------------------------------------------------------------------------

  -- check for unbalanced transfers
  INSERT INTO @Errors(ErrorText)
  SELECT 'Transactions for the current period are not balanced.'
  FROM dbo.Transactions (NOLOCK)
  WHERE TransactionDate >= @CurrentPeriodStartDate
    AND TransactionDate <= @CurrentPeriodEndDate
    AND TransactionTypeCode = 'X'
  HAVING SUM(Amount) != 0

  SELECT @ErrorCount = @ErrorCount + @@ROWCOUNT;

  -- check for unbalanced transactions
  WITH
  Summary AS
  (
    SELECT t.TransactionId--, SUM(t.Amount) AS TransactionAmount, SUM(ISNULL(m.Amount, 0)) AS MappedAmount
    FROM dbo.Transactions t
      LEFT JOIN dbo.MappedTransactions m ON t.TransactionId = m.TransactionId
    WHERE TransactionDate >= @CurrentPeriodStartDate
      AND TransactionDate <= @CurrentPeriodEndDate
    GROUP BY t.TransactionId, t.Amount
    HAVING t.Amount != SUM(m.Amount)
  )
  INSERT INTO @Errors(ErrorText)
  SELECT 'Mapped amounts for transaction, ' + CAST(TransactionId AS varchar(21))
    + ', do not match the transaction amount.'
  FROM Summary;

  WITH
  actuals AS
  (
    SELECT t.BankAccountID, t.TransactionTypeCode, SUM(m.Amount) AS TotalAmount
    FROM dbo.Transactions t (NOLOCK)
      INNER JOIN dbo.MappedTransactions m (NOLOCK) ON t.TransactionId = m.TransactionId
    WHERE t.TransactionDate >= @CurrentPeriodStartDate
      AND t.TransactionDate <= @CurrentPeriodEndDate
    GROUP BY t.BankAccountID, t.TransactionTypeCode
  ),
  trans AS
  (
      SELECT BankAccountID, TransactionTypeCode,
          SUM(Amount) AS TotalAmount
      FROM dbo.Transactions (NOLOCK)
      WHERE TransactionDate >= @CurrentPeriodStartDate
        AND TransactionDate <= @CurrentPeriodEndDate
      GROUP BY BankAccountID, TransactionTypeCode
  ),
  diffs AS
  (
      SELECT ISNULL(a.BankAccountID, b.BankAccountID) AS BankAccountID,
          ISNULL(a.TransactionTypeCode, b.TransactionTypeCode) AS TransactionTypeCode
      FROM actuals a
          FULL OUTER JOIN trans b ON a.BankAccountID = b.BankAccountID
              AND a.TransactionTypeCode = b.TransactionTypeCode
      WHERE ISNULL(a.TotalAmount, 0) != ISNULL(b.TotalAmount, 0)
  )
  INSERT INTO @Errors(ErrorText)
  SELECT 'Transactions for account, ' + b.BankAccountName + ', do not match '
      + 'the period actuals for transaction type ' + a.TransactionTypeCode + '.'
  FROM diffs a
      INNER JOIN dbo.BankAccounts b (NOLOCK) ON a.BankAccountID = b.BankAccountID

  SELECT @ErrorCount = @ErrorCount + @@ROWCOUNT

  IF @ErrorCount > 0 GOTO ExitProc;

------------------------------------------------------------------------------
-- gather allocations and actuals
------------------------------------------------------------------------------

-- get the period balances for all asset and accrued lines
INSERT INTO #PeriodBalances(BankAccountID, BudgetLineID, Balance)
SELECT a.BankAccountID, a.BudgetLineID,
  ISNULL(b.Balance, 0) AS Balance
FROM dbo.vwPeriodBalanceLines a (NOLOCK)
  LEFT JOIN dbo.PeriodBalances b (NOLOCK) ON a.BankAccountID = b.BankAccountID
    AND a.BudgetLineID = b.BudgetLineID
    AND b.PeriodID = @PreviousPeriodID;

-- get actuals by bank account and transaction type
WITH
BaseData AS
(
  SELECT t.BankAccountID, t.BudgetLineID, t.TransactionTypeCode,
    CASE WHEN ISNULL(t.Amount, 0) <= 0 THEN 'D' ELSE 'C' END AS DebitCreditFlag,
    t.Amount AS ActualAmount
  FROM dbo.vwTransactions t
  WHERE t.TransactionTypeCode IN('S', 'X')
    AND t.PeriodID = @CurrentPeriodID
)
INSERT INTO #Actuals(BankAccountID, BudgetLineID, TransactionTypeCode,
  DebitCreditFlag, ActualAmount)
SELECT BankAccountID, BudgetLineID, TransactionTypeCode, DebitCreditFlag,
  SUM(ActualAmount) AS ActualAmount
FROM BaseData
GROUP BY BankAccountID, BudgetLineID, TransactionTypeCode, DebitCreditFlag;

-- get allocations by bank account
INSERT INTO #Allocations(BankAccountID, BudgetLineID, PlannedAmount, AllocatedAmount, AccruedAmount)
SELECT BankAccountID, BudgetLineID,
  SUM(PlannedAmount) AS PlannedAmount,
  SUM(AllocatedAmount) AS AllocatedAmount,
  SUM(AccruedAmount) AS AccruedAmount
FROM dbo.Allocations
WHERE PeriodID = @CurrentPeriodID
GROUP BY BankAccountID, BudgetLineID;

-- get a summary for each line by account and transaction type
INSERT INTO #LineSummary(BankAccountId, BudgetLineId, DebitCreditFlag,
  BudgetGroupName, IsAccrued, IsCashOffset, TransferAmount, StandardAmount,
  ActualAmount, PlannedAmount, AllocatedAmount, AccruedAmount)
SELECT ba.BankAccountId, 
  CASE WHEN bl.IsAccrued = 1 THEN bl.BudgetLineID ELSE @CashBudgetLineID END AS BudgetLineId,
  ISNULL(DebitCreditFlag, 'D') AS DebitCreditFlag,
  BudgetGroupName, IsAccrued, 0 AS IsCashOffset, 
  SUM(CASE WHEN ac.TransactionTypeCode = 'X' THEN ac.ActualAmount ELSE @DefaultMoney END) AS TransferAmount,
  SUM(CASE WHEN ac.TransactionTypeCode = 'S' THEN ac.ActualAmount ELSE @DefaultMoney END) AS StandardAmount,
  SUM(ISNULL(ac.ActualAmount, @DefaultMoney)) AS ActualAmount,
  SUM(ISNULL(a.PlannedAmount, @DefaultMoney)) AS PlannedAmount,
  SUM(ISNULL(a.AllocatedAmount, @DefaultMoney)) AS AllocatedAmount,
  SUM(ISNULL(a.AccruedAmount, @DefaultMoney)) AS AccruedAmount
FROM dbo.vwBudgetGroupCategoryLine bl
  CROSS JOIN dbo.BankAccounts ba
  LEFT JOIN #Actuals ac ON ba.BankAccountId = ac.BankAccountId
    AND bl.BudgetLineId = ac.BudgetLineId
  LEFT JOIN #Allocations a ON ba.BankAccountId = a.BankAccountId
    AND bl.BudgetLineId = a.BudgetLineId
GROUP BY ba.BankAccountId,
  CASE WHEN bl.IsAccrued = 1 THEN bl.BudgetLineID ELSE @CashBudgetLineID END,
  ISNULL(DebitCreditFlag, 'D'),
  BudgetGroupName, IsAccrued; -- IsCashOffset;

------------------------------------------------------------------------------
-- calculate adjustments
------------------------------------------------------------------------------

-- add expense adjustments
INSERT INTO #Adjustments(BankAccountID, BudgetLineID, AdjustmentTypeCode,
  CashAdjustment, LineAdjustment, ProjectedCashAdjustment, ProjectedLineAdjustment)
SELECT BankAccountID, BudgetLineID,
  CASE IsAccrued WHEN 1 THEN 'R' ELSE 'E' END AS AdjustmentTypeCode,
  0 AS CashAdjustment,
  StandardAmount AS LineAdjustment,
  --CASE
  --  WHEN IsAccrued = 1 THEN 0.0
  --  WHEN AllocatedAmount > - ActualAmount THEN -ActualAmount - AllocatedAmount
  --  ELSE 0.0
  --END
  0.0 AS ProjectedCashAdjustment,
  --CASE
  --  WHEN IsAccrued = 0 THEN 0.0
  --  WHEN AccruedAmount > -ActualAmount THEN -ActualAmount - AccruedAmount 
  --  ELSE 0.0
  --END
  0.0 AS ProjectedLineAdjustment
FROM #LineSummary
WHERE BudgetGroupName = 'Expenses'
  AND DebitCreditFlag = 'D'
  AND StandardAmount != 0;

-- add credit adjustments
INSERT INTO #Adjustments(BankAccountID, BudgetLineID, AdjustmentTypeCode,
  CashAdjustment, LineAdjustment, ProjectedCashAdjustment, ProjectedLineAdjustment)
SELECT BankAccountID, BudgetLineID,
  'C' AS AdjustmentTypeCode,
  0.0 AS CashAdjustment,
  StandardAmount AS LineAdjustment,
  0.0 AS ProjectedCashAdjustment,
  0.0 AS ProjectedLineAdjustment
FROM #LineSummary
WHERE BudgetGroupName = 'Expenses'
  AND DebitCreditFlag = 'C'
  AND StandardAmount != 0;

 -- add income adjustments
INSERT INTO #Adjustments(BankAccountID, BudgetLineID, AdjustmentTypeCode,
  CashAdjustment, LineAdjustment, ProjectedCashAdjustment, ProjectedLineAdjustment)
SELECT BankAccountID, BudgetLineID,
  'I' AS AdjustmentTypeCode,
  StandardAmount AS CashAdjustment,
  0.0 AS LineAdjustment,
  CASE
    WHEN @ClosePeriod = 1 THEN 0.0
    WHEN -AllocatedAmount > StandardAmount THEN -AllocatedAmount - StandardAmount
    ELSE 0.0
  END AS ProjectedCashAdjustment,
  0.0 AS ProjectedLineAdjustment
FROM #LineSummary
WHERE IsAccrued = 0
  AND ((BudgetGroupName = 'Income' AND StandardAmount != 0)
  OR (BudgetGroupName = 'Assets' AND StandardAmount > 0));

-- add transfer adjustments
INSERT INTO #Adjustments(BankAccountID, BudgetLineID, AdjustmentTypeCode,
  CashAdjustment, LineAdjustment, ProjectedCashAdjustment, ProjectedLineAdjustment)
SELECT BankAccountID, BudgetLineID,
  'X' AS AdjustmentTypeCode,
  0.0 AS CashAdjustment,
  TransferAmount AS LineAdjustment,
  0.0 AS ProjectedCashAdjustment,
  0.0 AS ProjectedLineAdjustment
FROM #LineSummary
WHERE BudgetGroupName IN('Expenses', 'Assets')
  AND TransferAmount != 0;

-- add projected adjustments
WITH
BaseData AS
(
  SELECT BankAccountID, BudgetLineID,
    CASE
      WHEN @ClosePeriod = 1 THEN 0.0
      WHEN IsAccrued = 1 THEN
        CASE
          WHEN AllocatedAmount + AccruedAmount = 0 THEN 0.0
          WHEN AllocatedAmount + AccruedAmount > 0 AND AllocatedAmount + AccruedAmount > -ActualAmount
          THEN -(AllocatedAmount + AccruedAmount) - ActualAmount
          WHEN AllocatedAmount + AccruedAmount < 0 AND ActualAmount < -(AllocatedAmount + AccruedAmount)
          THEN -ActualAmount - (AllocatedAmount + AccruedAmount)
          ELSE 0.0
        END
      ELSE
        CASE
          WHEN AllocatedAmount <= 0 THEN 0.0
          WHEN AllocatedAmount > 0 AND AllocatedAmount > -ActualAmount
          THEN -AllocatedAmount - ActualAmount
          ELSE 0.0
        END
    END AS ProjectedLineAdjustment
  --,AllocatedAmount, AccruedAmount, ActualAmount
  FROM #LineSummary
  WHERE BudgetGroupName IN('Expenses', 'Assets', 'Income')
)
INSERT INTO #Adjustments(BankAccountID, BudgetLineID, AdjustmentTypeCode,
  CashAdjustment, LineAdjustment, ProjectedCashAdjustment, ProjectedLineAdjustment)
SELECT BankAccountID, BudgetLineID,
  'P' AS AdjustmentTypeCode,
  0.0 AS CashAdjustment,
  0.0 AS LineAdjustment,
  0.0 AS ProjectedCashAdjustment,
  ProjectedLineAdjustment
--,AllocatedAmount, AccruedAmount, ActualAmount
FROM BaseData
WHERE ProjectedLineAdjustment != 0;

-- add accrual adjustments
INSERT INTO #Adjustments(BankAccountID, BudgetLineID, AdjustmentTypeCode,
  CashAdjustment, LineAdjustment, ProjectedCashAdjustment, ProjectedLineAdjustment)
SELECT ls.BankAccountID, ls.BudgetLineID,
  'A' AS AdjustmentTypeCode,
  CASE
    WHEN AllocatedAmount < 0 THEN 0.0
    WHEN @ClosePeriod = 1 THEN -AllocatedAmount
    WHEN AllocatedAmount > 0 AND AllocatedAmount >= -ActualAmount THEN ActualAmount
    ELSE -AllocatedAmount
  END AS CashAdjustment,
  CASE
    WHEN AllocatedAmount < 0 THEN 0.0
    WHEN @ClosePeriod = 1 THEN AllocatedAmount
    WHEN AllocatedAmount > 0 AND AllocatedAmount >= -ActualAmount THEN -ActualAmount
    ELSE AllocatedAmount
  END AS LineAdjustment,
  CASE
    WHEN AllocatedAmount = 0 THEN 0.0
    WHEN @ClosePeriod = 1 THEN 0.0
    WHEN -ActualAmount >= AllocatedAmount THEN 0.0
    ELSE -AllocatedAmount - ActualAmount
  END AS ProjectedCashAdjustment,
  CASE
    WHEN AllocatedAmount = 0 THEN 0.0
    WHEN @ClosePeriod = 1 THEN 0.0
    WHEN -ActualAmount >= AllocatedAmount THEN 0.0
    ELSE AllocatedAmount - -ActualAmount
  END AS ProjectedLineAdjustment
--,AllocatedAmount, AccruedAmount, ActualAMount
FROM #LineSummary ls
WHERE BudgetGroupName = 'Expenses'
  AND IsAccrued = 1;

-- calculate final adjustments
INSERT INTO #Consolidated(BankAccountID, BudgetLineID, AdjustmentTypeCode, TotalAmount, ProjectedAmount)
SELECT BankAccountID, @CashBudgetLineID, AdjustmentTypeCode, SUM(CashAdjustment), SUM(ProjectedCashAdjustment)
FROM #Adjustments
GROUP BY BankAccountID, AdjustmentTypeCode
HAVING SUM(CashAdjustment) != 0 OR SUM(ProjectedCashAdjustment) != 0
UNION ALL
SELECT BankAccountID, BudgetLineID, AdjustmentTypeCode, SUM(LineAdjustment), SUM(ProjectedLineAdjustment)
FROM #Adjustments
GROUP BY BankAccountID, BudgetLineID, AdjustmentTypeCode
HAVING SUM(LineAdjustment) != 0 OR SUM(ProjectedLineAdjustment) != 0;

-- pick up manual adjustments
INSERT INTO #Consolidated(BankAccountID, BudgetLineID, AdjustmentTypeCode, TotalAmount, ProjectedAmount)
SELECT BankAccountID, BudgetLineID, AdjustmentTypeCode, SUM(Amount), 0.00
FROM dbo.PeriodAdjustments pa
WHERE PeriodID = @CurrentPeriodID
  AND AdjustmentTypeCode = 'M'
GROUP BY BankAccountID, BudgetLineID, AdjustmentTypeCode;

------------------------------------------------------------------------------
-- persist data
------------------------------------------------------------------------------

IF @BeginTranCount = 0
  BEGIN TRANSACTION;
ELSE
  SAVE TRANSACTION SavePoint;

-- remove existing balances for this period
DELETE a
FROM dbo.PeriodBalances a (NOLOCK)
WHERE PeriodID = @CurrentPeriodID

-- remove existing adjustments for this period (excluding manual adjustments)
DELETE a
FROM dbo.PeriodAdjustments a
WHERE PeriodID = @CurrentPeriodID
    AND AdjustmentTypeCode != 'M'

-- add period adjustments
INSERT INTO dbo.PeriodAdjustments(PeriodID, BankAccountID, BudgetLineID, AdjustmentTypeCode, Amount)
SELECT @CurrentPeriodID, BankAccountID, BudgetLineID, AdjustmentTypeCode, TotalAmount
FROM #Consolidated
WHERE AdjustmentTypeCode != 'M';

-- insert new balances for this period
WITH
data AS
(
    SELECT BankAccountID, BudgetLineID,
      SUM(TotalAmount) AS CurrentAmount,
      SUM(TotalAmount + ProjectedAmount) AS ProjectedAmount
    FROM #Consolidated
    GROUP BY BankAccountID, BudgetLineID
)
INSERT INTO dbo.PeriodBalances(PeriodID, BankAccountID, BudgetLineID, Balance, ProjectedBalance)
SELECT @CurrentPeriodID, a.BankAccountID, a.BudgetLineID,
  a.Balance + ISNULL(b.CurrentAmount, 0) AS Balance,
  a.Balance + ISNULL(b.ProjectedAmount, 0) AS ProjectedBalance
FROM #PeriodBalances a
  LEFT JOIN data b ON a.BankAccountID = b.BankAccountID
      AND a.BudgetLineID = b.BudgetLineID
WHERE NOT(a.Balance + ISNULL(b.CurrentAmount, 0) = 0
  AND a.Balance + ISNULL(b.ProjectedAmount, 0) = 0);

-- close the period
IF @ClosePeriod = 1
  UPDATE a
  SET IsOpen = 0
  FROM dbo.Periods a
  WHERE PeriodID = @CurrentPeriodID

IF @BeginTranCount = 0
  COMMIT TRANSACTION;

END TRY
BEGIN CATCH
	SELECT @ErrorMessage = 'Procedure: ' + ERROR_PROCEDURE() + ' '
		+ 'Error at line: ' + CAST(ERROR_LINE() AS nvarchar(10)) + ' '
		+ 'Msg: ' + ERROR_MESSAGE(),
		@ErrorSeverity = ERROR_SEVERITY()

  IF @BeginTranCount = 0
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
  ELSE
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION SavePoint;

  RAISERROR(@ErrorMessage, @ErrorSeverity, 1)
END CATCH

ExitProc:

    SELECT CASE WHEN @ErrorCount > 0 THEN 16 ELSE 0 END AS ErrorLevel,
        CASE WHEN @ErrorCount > 0 THEN
            CASE @ClosePeriod
                WHEN 1 THEN 'Period could not be closed.'
                ELSE 'Period balances could not be updated.'
            END
        ELSE
            CASE @ClosePeriod
                WHEN 1 THEN 'Period was closed.'
                ELSE 'Period balances were updated.'
            END
        END AS MessageText
    UNION ALL
    SELECT 16 AS ErrorLevel, ErrorText AS MessageText
    FROM @Errors;

GO

grant execute, view definition on dbo.uspUpdatePeriodBalances to exec_procs;
go
