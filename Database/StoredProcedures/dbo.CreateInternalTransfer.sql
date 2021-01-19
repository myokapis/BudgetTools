CREATE OR ALTER PROCEDURE dbo.CreateInternalTransfer
    @BankAccountID int,
    @BudgetLineFromID int,
    @BudgetLineToID int,
    @Amount money,
    @Note varchar(1024) = NULL
AS

SET NOCOUNT ON

DECLARE @CurrentPeriodID int;
DECLARE @CurrentPeriodEndDate date;
DECLARE @TransactionDate date;
DECLARE @AccountChar varchar(10);
DECLARE @DebitLineChar varchar(10);
DECLARE @CreditLineChar varchar(10);
DECLARE @TransactionDateChar char(36);

DECLARE @ErrorMessages table
(
    ErrorLevel int not null,
    MessageText varchar(1024)
);

BEGIN TRY

    -- verify bank account exists
    IF NOT EXISTS
    (
        SELECT TOP 1 1
        FROM dbo.BankAccounts
        WHERE BankAccountId = @BankAccountID
    )
        INSERT INTO @ErrorMessages(ErrorLevel, MessageText)
        VALUES(16, 'Transfer failed. The bank account is invalid.');

    -- verify lines are different
    IF COALESCE(@BudgetLineFromID, @BudgetLineToID + 1, -1) = COALESCE(@BudgetLineToID, @BudgetLineFromID + 1, -1)
        INSERT INTO @ErrorMessages(ErrorLevel, MessageText)
        VALUES(16, 'Cannot transfer from and to the same budget line.');

    -- verify budget lines exist
    IF 2 != (
        SELECT COUNT(DISTINCT BudgetLineId)
        FROM dbo.BudgetLines
        WHERE BudgetLineId IN(@BudgetLineFromID, @BudgetLineToID)
    )
        INSERT INTO @ErrorMessages(ErrorLevel, MessageText)
        VALUES(16, 'Transfer failed. A budget line was invalid.');

    -- ensure the amount is > 0
    IF ISNULL(@Amount, 0.0) <= 0
        INSERT INTO @ErrorMessages(ErrorLevel, MessageText)
        VALUES(16, 'Transfer failed. The transfer amount must be greater than zero.');

    IF 0 < (SELECT COUNT(0) FROM @ErrorMessages)
        GOTO ExitProc;

    -- lookup current period
    SELECT @CurrentPeriodId = MIN(PeriodId),
        @CurrentPeriodEndDate = MIN(PeriodEndDate)
    FROM dbo.Periods
    WHERE IsOpen = 1;

    -- set transaction date to be within the current period
    SELECT @TransactionDate =
        CASE
            WHEN GETDATE() > @CurrentPeriodEndDate THEN @CurrentPeriodEndDate
            ELSE GETDATE()
        END;

    -- convert values
    SELECT @AccountChar = CAST(@BankAccountID AS varchar);
    SELECT @DebitLineChar = CAST(@BudgetLineFromID AS varchar);
    SELECT @CreditLineChar = CAST(@BudgetLineToID AS varchar);
    SELECT @TransactionDateChar = NEWID();

    BEGIN TRANSACTION

    -- write debit transaction
    INSERT INTO dbo.Transactions(BankAccountId, TransactionNo, TransactionDate,
      TransactionDesc, Amount, TransactionTypeCode, Recipient, Notes, IsMapped)
    SELECT @BankAccountID, 
        'IntXfer' + @TransactionDateChar + '>' + @AccountChar + '-' + @AccountChar
            + ':' + @DebitLineChar + '-' + @CreditLineChar + ':D',
        @TransactionDate, 'Internal Transfer', -@Amount, 'I', 'Internal Transfer', @Note,
        1 AS IsMapped;

    -- add debit mapped transaction
    INSERT INTO dbo.MappedTransactions(TransactionId, BudgetLineId, Amount)
    SELECT SCOPE_IDENTITY(), @BudgetLineFromID, -@Amount;

    -- write credit transaction
    INSERT INTO dbo.Transactions(BankAccountId, TransactionNo, TransactionDate,
      TransactionDesc, Amount, TransactionTypeCode, Recipient, Notes, IsMapped)
    SELECT @BankAccountID,
        'IntXfer' + @TransactionDateChar + '>' + @AccountChar + '-' + @AccountChar
            + ':' + @DebitLineChar + '-' + @CreditLineChar + ':C',
        @TransactionDate, 'Internal Transfer', @Amount, 'I', 'Internal Transfer', @Note,
        1 AS IsMapped;

    -- add credit mapped transaction
    INSERT INTO dbo.MappedTransactions(TransactionId, BudgetLineId, Amount)
    SELECT SCOPE_IDENTITY(), @BudgetLineToID, @Amount;
    
    -- only allow one adjustment of this type per period and bank account
    WITH
    CurrentPeriodAdjustments AS
    (
        SELECT PeriodID, BankAccountID, BudgetLineID, AdjustmentTypeCode, Amount
        FROM dbo.PeriodAdjustments
        WHERE PeriodId = @CurrentPeriodId
        AND AdjustmentTypeCode = 'M'
        AND BankAccountId = @BankAccountID
        AND BudgetLineID IN(@BudgetLineFromID, @BudgetLineToID)
    ),
    Adjustments AS
    (
        SELECT @BudgetLineFromID AS BudgetLineID, -@Amount AS Amount
        UNION ALL SELECT @BudgetLineToID, @Amount
    )
    MERGE INTO CurrentPeriodAdjustments ca
    USING Adjustments a
    ON ca.BudgetLineID = a.BudgetLineID
    WHEN MATCHED THEN
        UPDATE
        SET Amount = ca.Amount + a.Amount
    WHEN NOT MATCHED BY TARGET THEN
        INSERT(PeriodID, BankAccountID, BudgetLineID, AdjustmentTypeCode, Amount)
        VALUES(@CurrentPeriodId, @BankAccountID, a.BudgetLineId, 'M', a.Amount);

    COMMIT TRANSACTION

    INSERT INTO @ErrorMessages(ErrorLevel, MessageText)
    VALUES(0, 'Transfer succeeded.');

    ExitProc:

END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;

    INSERT INTO @ErrorMessages(ErrorLevel, MessageText)
    VALUES(16, 'Transfer failed. A database error occurred.');
END CATCH

SELECT ErrorLevel, MessageText
FROM @ErrorMessages;

RETURN ISNULL((SELECT MAX(ErrorLevel) FROM @ErrorMessages), 0);

GO

grant execute, view definition on dbo.CreateInternalTransfer to exec_procs;
go

