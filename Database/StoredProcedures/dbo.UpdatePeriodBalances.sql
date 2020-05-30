CREATE OR ALTER PROCEDURE dbo.UpdatePeriodBalances
    @PeriodId int,
    @SkipValidations bit = 0
AS

SET NOCOUNT ON

DECLARE @CurrentPeriodID int;
DECLARE @PreviousPeriodID int;
DECLARE @ThisPeriodID int;
DECLARE @PrevPeriodID int;
DECLARE @ThisPeriodStartDate date;
DECLARE @ThisPeriodEndDate date;
DECLARE @CashBudgetLineID int;
DECLARE @DefaultAccountId int;
DECLARE @ErrorCount int = 0;

DECLARE @Periods TABLE
(
    PeriodId int NOT NULL PRIMARY KEY,
    PeriodStartDate date NOT NULL,
    PeriodEndDate date NOT NULL
);

DECLARE @Errors TABLE
(
    ErrorText varchar(255)
);

DROP TABLE IF EXISTS #Actuals;

CREATE TABLE #Actuals
(
    BankAccountId int NOT NULL,
    BudgetLineId int NOT NULL,
    TransactionTypeCode char(1) NOT NULL,
    ActualAmount money NOT NULL,
    PRIMARY KEY(BankAccountId, BudgetLineId, TransactionTypeCode)
);

DROP TABLE IF EXISTS #Allocations;

CREATE TABLE #Allocations
(
    BankAccountID int NOT NULL,
    BudgetLineID int NOT NULL,
    PlannedAmount money NOT NULL,
    AllocatedAmount money NOT NULL,
    AccruedAmount money NOT NULL,
    PRIMARY KEY(BankAccountId, BudgetLineId)
);

DROP TABLE IF EXISTS #LineSummary;

CREATE TABLE #LineSummary
(
    BankAccountId int NOT NULL,
    BudgetLineId int NOT NULL,
    BudgetGroupName varchar(50) NOT NULL,
    IsAccrued bit NOT NULL,
    TransferAmount money NOT NULL,
    StandardAmount money NOT NULL,
    ActualAmount money NOT NULL,
    PlannedAmount money NOT NULL,
    AllocatedAmount money NOT NULL,
    AccruedAmount money NOT NULL,
    RemainingAmount money NOT NULL default(0.0),
    ActualLineAmount money NOT NULL default(0.0),
    ActualCashAmount money NOT NULL default(0.0),
    ProjectedLineAmount money NOT NULL default(0.0),
    ProjectedCashAmount money NOT NULL default(0.0),
    PRIMARY KEY(BankAccountId, BudgetLineId, BudgetGroupName)
);

DROP TABLE IF EXISTS #PeriodBalances;

CREATE TABLE #PeriodBalances
(
    BankAccountID	                    int,
    BudgetLineID	                    int,
    Balance	                          money
);

DROP TABLE IF EXISTS #Adjustments;

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

DROP TABLE IF EXISTS #Consolidated;

CREATE TABLE #Consolidated
(
    BankAccountID                   int,
    BudgetLineID                    int,
    AdjustmentTypeCode              char(1),
    TotalAmount                     money,
    ProjectedAmount                 money,
    PRIMARY KEY(BankAccountID, BudgetLineID, AdjustmentTypeCode)
);

BEGIN TRY

    ------------------------------------------------------------------------------
    -- setup
    ------------------------------------------------------------------------------

    -- get current and previous periods
    SELECT @CurrentPeriodID = MIN(CASE WHEN IsOpen = 1 THEN PeriodID ELSE 999999 END),
        @PreviousPeriodID = MAX(CASE WHEN IsOpen = 0 THEN PeriodID ELSE 0 END)
    FROM dbo.Periods (NOLOCK);

    -- get the cash and default accounts
    SELECT @CashBudgetLineID = dbo.GetParameter('Cash'),
        @DefaultAccountId = dbo.GetParameter('DefaultAccountId');

    ------------------------------------------------------------------------------
    -- validations
    ------------------------------------------------------------------------------

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

    ------------------------------------------------------------------------------
    -- period processing
    ------------------------------------------------------------------------------

    -- get all periods from current period to requested period
    INSERT INTO @Periods(PeriodId, PeriodStartDate, PeriodEndDate)
    SELECT PeriodId, PeriodStartDate, PeriodEndDate
    FROM dbo.Periods c
    WHERE PeriodId BETWEEN @CurrentPeriodId AND @PeriodId
    ORDER BY PeriodId;

    SELECT @ThisPeriodId = @CurrentPeriodId,
        @PrevPeriodId = @PreviousPeriodId;

    WHILE 1 = 1
    BEGIN

        -- clean out variables
        SELECT @ThisPeriodId = NULL,
            @ThisPeriodStartDate = NULL,
            @ThisPeriodEndDate = NULL;

        -- get the starting and ending dates for this period
        SELECT TOP 1 @ThisPeriodId = PeriodId,
            @ThisPeriodStartDate = PeriodStartDate,
            @ThisPeriodEndDate = PeriodEndDate
        FROM @Periods
        ORDER BY PeriodId;

        IF @ThisPeriodStartDate IS NULL OR @ThisPeriodEndDate IS NULL OR @@ROWCOUNT = 0
            BREAK;

        ------------------------------------------------------------------------------
        -- current period validations
        ------------------------------------------------------------------------------

        IF @ThisPeriodId = @CurrentPeriodId AND @SkipValidations = 0
        BEGIN

            -- check for unbalanced transfers
            INSERT INTO @Errors(ErrorText)
            SELECT 'Transactions for the current period are not balanced.'
            FROM dbo.Transactions (NOLOCK)
            WHERE TransactionDate >= @ThisPeriodStartDate
            AND TransactionDate <= @ThisPeriodEndDate
            AND TransactionTypeCode = 'X'
            HAVING SUM(Amount) != 0;

            SELECT @ErrorCount = @ErrorCount + @@ROWCOUNT;

            -- check for unbalanced transactions
            WITH
            Summary AS
            (
                SELECT t.TransactionId
                FROM dbo.Transactions t
                LEFT JOIN dbo.MappedTransactions m ON t.TransactionId = m.TransactionId
                WHERE TransactionDate >= @ThisPeriodStartDate
                AND TransactionDate <= @ThisPeriodEndDate
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
                WHERE t.TransactionDate >= @ThisPeriodStartDate
                AND t.TransactionDate <= @ThisPeriodEndDate
                GROUP BY t.BankAccountID, t.TransactionTypeCode
            ),
            trans AS
            (
                SELECT BankAccountID, TransactionTypeCode,
                    SUM(Amount) AS TotalAmount
                FROM dbo.Transactions (NOLOCK)
                WHERE TransactionDate >= @ThisPeriodStartDate
                AND TransactionDate <= @ThisPeriodEndDate
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

            IF @ErrorCount > 0 BREAK;

        END

        ------------------------------------------------------------------------------
        -- gather allocations and actuals
        ------------------------------------------------------------------------------

        -- get the starting period balances
        INSERT INTO #PeriodBalances(BankAccountID, BudgetLineID, Balance)
        SELECT a.BankAccountID, a.BudgetLineID,
            CASE
                WHEN @ThisPeriodId = @CurrentPeriodId THEN ISNULL(b.Balance, 0.0)
                ELSE ISNULL(b.ProjectedBalance, 0.0)
            END AS Balance
        FROM dbo.vwPeriodBalanceLines a (NOLOCK)
        LEFT JOIN dbo.PeriodBalances b (NOLOCK) ON a.BankAccountID = b.BankAccountID
            AND a.BudgetLineID = b.BudgetLineID
            AND b.PeriodID = @PrevPeriodID;

        -- get actuals by bank account and transaction type
        WITH
        BaseData AS
        (
            SELECT t.BankAccountID, t.BudgetLineID, t.TransactionTypeCode,
                CASE WHEN ISNULL(t.Amount, 0.0) <= 0.0 THEN 'D' ELSE 'C' END AS DebitCreditFlag,
                t.Amount AS ActualAmount
            FROM dbo.vwTransactions t
            WHERE t.TransactionTypeCode IN('S', 'X')
            AND t.PeriodID = @ThisPeriodID
        )
        INSERT INTO #Actuals(BankAccountID, BudgetLineID, TransactionTypeCode, ActualAmount)
        SELECT BankAccountID, BudgetLineID, TransactionTypeCode,
            SUM(ActualAmount) AS ActualAmount
        FROM BaseData
        GROUP BY BankAccountID, BudgetLineID, TransactionTypeCode;

        -- get allocations by bank account
        INSERT INTO #Allocations(BankAccountID, BudgetLineID, PlannedAmount, AllocatedAmount, AccruedAmount)
        SELECT BankAccountID, BudgetLineID,
            SUM(PlannedAmount) AS PlannedAmount,
            SUM(AllocatedAmount) AS AllocatedAmount,
            SUM(AccruedAmount) AS AccruedAmount
        FROM dbo.Allocations
        WHERE PeriodID = @ThisPeriodID
        GROUP BY BankAccountID, BudgetLineID;

        -- get a summary for each line by account and transaction type
        INSERT INTO #LineSummary(BankAccountId, BudgetLineId,
            BudgetGroupName, IsAccrued, TransferAmount, StandardAmount,
            ActualAmount, PlannedAmount, AllocatedAmount, AccruedAmount)
        SELECT ba.BankAccountId, bl.BudgetLineId, BudgetGroupName, IsAccrued,
            SUM(CASE WHEN ac.TransactionTypeCode = 'X' THEN ac.ActualAmount ELSE 0.0 END) AS TransferAmount,
            SUM(CASE WHEN ac.TransactionTypeCode = 'S' THEN ac.ActualAmount ELSE 0.0 END) AS StandardAmount,
            SUM(ISNULL(ac.ActualAmount, 0.0)) AS ActualAmount,
            SUM(ISNULL(a.PlannedAmount, 0.0)) AS PlannedAmount,
            SUM(ISNULL(a.AllocatedAmount, 0.0)) AS AllocatedAmount,
            SUM(ISNULL(a.AccruedAmount, 0.0)) AS AccruedAmount
        FROM dbo.vwBudgetGroupCategoryLine bl
        CROSS JOIN dbo.BankAccounts ba
        LEFT JOIN #Actuals ac ON ba.BankAccountId = ac.BankAccountId
            AND bl.BudgetLineId = ac.BudgetLineId
        LEFT JOIN #Allocations a ON ba.BankAccountId = a.BankAccountId
            AND bl.BudgetLineId = a.BudgetLineId
        GROUP BY ba.BankAccountId, bl.BudgetLineId, BudgetGroupName, IsAccrued;

        ------------------------------------------------------------------------------
        -- rules
        ------------------------------------------------------------------------------

        -- calculate remaining amount for expenses and distribute actual amount between line and cash
        UPDATE s
		SET RemainingAmount = PlannedAmount + ActualAmount
				+ IIF(AccruedAmount < 0.0 AND @ThisPeriodId = @CurrentPeriodId, -AccruedAmount, 0.0),
            ActualLineAmount = -AccruedAmount +
				CASE
					WHEN IsAccrued = 0 OR BudgetLineId = @CashBudgetLineId THEN 0.0
					WHEN TransferAmount < 0.0 THEN 0.0
					ELSE TransferAmount
				END,
            ActualCashAmount = ActualAmount + AccruedAmount +
				CASE
					WHEN IsAccrued = 0 OR BudgetLineId = @CashBudgetLineId THEN 0.0
					WHEN TransferAmount < 0.0 THEN 0.0
					ELSE -TransferAmount
				END
        FROM #LineSummary s
        WHERE BudgetGroupName = 'Expenses';

        -- calculate remaining amount for income/assets and distribute actual amount between line and cash
        UPDATE s
        SET RemainingAmount =
            CASE
                WHEN ActualAmount < -PlannedAmount THEN PlannedAmount - -ActualAmount
                ELSE 0.0
            END,
            ActualLineAmount =
            CASE
                WHEN IsAccrued = 0 THEN 0.0
                WHEN ActualAmount > -AccruedAmount AND @ThisPeriodId = @CurrentPeriodId THEN AccruedAmount
                ELSE ActualAmount - IIF(BudgetLineId = @CashBudgetLineId, 0.0, -TransferAmount)
            END,
            ActualCashAmount =
            CASE
				WHEN ActualAmount < 0.0 THEN ActualAmount
					- IIF(@ThisPeriodId = @CurrentPeriodId, -AccruedAmount, 0.0)
                    - IIF(BudgetLineId = @CashBudgetLineId, 0.0, -TransferAmount)
                WHEN ActualAmount > -AccruedAmount THEN ActualAmount
					- IIF(@ThisPeriodId = @CurrentPeriodId, -AccruedAmount, 0.0)
                    - IIF(BudgetLineId = @CashBudgetLineId, 0.0, -TransferAmount)
                ELSE 0.0
            END
        FROM #LineSummary s
        WHERE BudgetGroupName IN('Income', 'Assets');

        -- calculate projected amounts and distribute between line and cash for expenses
        UPDATE s
        SET ProjectedLineAmount =
            CASE
                WHEN RemainingAmount = 0.0 THEN 0.0
                WHEN -ActualAmount > AccruedAmount AND @ThisPeriodId = @CurrentPeriodId THEN 0.0
                ELSE ActualAmount - AccruedAmount
            END,
            ProjectedCashAmount =
            CASE
                WHEN RemainingAmount = 0.0 THEN 0.0
                WHEN -ActualAmount >= AccruedAmount AND @ThisPeriodId = @CurrentPeriodId THEN -RemainingAmount
                ELSE AccruedAmount - ActualAmount
            END
        FROM #LineSummary s
        WHERE BudgetGroupName = 'Expenses';

        -- calculate projected amounts and distribute between line and cash for income and assets
        UPDATE s
        SET ProjectedLineAmount =
            CASE
                WHEN RemainingAmount = 0.0 THEN 0.0
                WHEN AccruedAmount > -RemainingAmount THEN -(AccruedAmount - RemainingAmount)
                ELSE 0.0
            END,
            ProjectedCashAmount =
            CASE
                WHEN RemainingAmount = 0.0 THEN 0.0
                ELSE -(RemainingAmount - AccruedAmount)
            END
        FROM #LineSummary s
        WHERE BudgetGroupName IN('Income', 'Assets');

        ------------------------------------------------------------------------------
        -- calculate adjustments
        ------------------------------------------------------------------------------

        -- add adjustments
        INSERT INTO #Adjustments(BankAccountID, BudgetLineID, AdjustmentTypeCode,
            CashAdjustment, LineAdjustment, ProjectedCashAdjustment, ProjectedLineAdjustment)
        SELECT BankAccountID, BudgetLineID,
            CASE IsAccrued WHEN 1 THEN 'R' ELSE 'E' END AS AdjustmentTypeCode,
            ActualCashAmount AS CashAdjustment,
            ActualLineAmount AS LineAdjustment,
            ProjectedCashAmount AS ProjectedCashAdjustment,
            ProjectedLineAmount AS ProjectedLineAdjustment
        FROM #LineSummary
        WHERE ActualCashAmount != 0.0
        OR ActualLineAmount != 0.0
        OR ProjectedCashAmount != 0.0
        OR ProjectedLineAmount != 0.0;

        -- calculate final adjustments
        INSERT INTO #Consolidated(BankAccountID, BudgetLineID, AdjustmentTypeCode, TotalAmount, ProjectedAmount)
        SELECT BankAccountID, @CashBudgetLineID, AdjustmentTypeCode, SUM(CashAdjustment), SUM(ProjectedCashAdjustment)
        FROM #Adjustments
        GROUP BY BankAccountID, AdjustmentTypeCode
        HAVING SUM(CashAdjustment) != 0.0 OR SUM(ProjectedCashAdjustment) != 0.0
        UNION ALL
        SELECT BankAccountID, BudgetLineID, AdjustmentTypeCode, SUM(LineAdjustment), SUM(ProjectedLineAdjustment)
        FROM #Adjustments
        GROUP BY BankAccountID, BudgetLineID, AdjustmentTypeCode
        HAVING SUM(LineAdjustment) != 0.0 OR SUM(ProjectedLineAdjustment) != 0.0;

        -- pick up manual adjustments
        INSERT INTO #Consolidated(BankAccountID, BudgetLineID, AdjustmentTypeCode, TotalAmount, ProjectedAmount)
        SELECT BankAccountID, BudgetLineID, AdjustmentTypeCode, SUM(Amount), 0.00
        FROM dbo.PeriodAdjustments pa
        WHERE PeriodID = @ThisPeriodID
        AND AdjustmentTypeCode = 'M'
        GROUP BY BankAccountID, BudgetLineID, AdjustmentTypeCode;

        ------------------------------------------------------------------------------
        -- persist data
        ------------------------------------------------------------------------------

        BEGIN TRANSACTION;

        -- remove existing balances for this period
        DELETE a
        FROM dbo.PeriodBalances a (NOLOCK)
        WHERE PeriodID = @ThisPeriodID

        -- remove existing adjustments for this period (excluding manual adjustments)
        DELETE a
        FROM dbo.PeriodAdjustments a
        WHERE PeriodID = @ThisPeriodID
        AND AdjustmentTypeCode != 'M'

        -- add period adjustments
        INSERT INTO dbo.PeriodAdjustments(PeriodID, BankAccountID, BudgetLineID, AdjustmentTypeCode, Amount)
        SELECT @ThisPeriodID, BankAccountID, BudgetLineID, AdjustmentTypeCode, TotalAmount
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
        SELECT @ThisPeriodID, a.BankAccountID, a.BudgetLineID,
            a.Balance + ISNULL(b.CurrentAmount, 0) AS Balance,
            a.Balance + ISNULL(b.ProjectedAmount, 0) AS ProjectedBalance
        FROM #PeriodBalances a
        LEFT JOIN data b ON a.BankAccountID = b.BankAccountID
            AND a.BudgetLineID = b.BudgetLineID
        WHERE NOT(a.Balance + ISNULL(b.CurrentAmount, 0) = 0
        AND a.Balance + ISNULL(b.ProjectedAmount, 0) = 0);


    
       
 
                                       
                                          
                                                             
                      
                                        
 
                                                                     
                                                      
                                                                
                      
                                                     
                                       
                                                    
                                                 
                       
                                          


        COMMIT TRANSACTION;

        ------------------------------------------------------------------------------
        -- cleanup
        ------------------------------------------------------------------------------

        DELETE p
        FROM @Periods p
        WHERE PeriodId = @ThisPeriodId;

        SELECT @PrevPeriodId = @ThisPeriodId;

        TRUNCATE TABLE #PeriodBalances;
        TRUNCATE TABLE #Actuals;
        TRUNCATE TABLE #Allocations;
        TRUNCATE TABLE #LineSummary;
        TRUNCATE TABLE #Adjustments;
        TRUNCATE TABLE #Consolidated;

    END

    ExitProc:

END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;

    INSERT INTO @Errors(ErrorText)
    VALUES('An unexpected error occurred.');

    SELECT @ErrorCount = @ErrorCount + 1;
END CATCH

SELECT CASE WHEN @ErrorCount > 0 THEN 16 ELSE 0 END AS ErrorLevel,
    CASE WHEN @ErrorCount > 0
        THEN 'Period balances could not be updated.'
        ELSE 'Period balances were updated.'
    END AS MessageText
UNION ALL
SELECT 16 AS ErrorLevel, ErrorText AS MessageText
FROM @Errors;

RETURN CASE WHEN @ErrorCount > 0 THEN 16 ELSE 0 END;

GO

grant execute, view definition on dbo.UpdatePeriodBalances to exec_procs;
go
