IF OBJECT_ID('dbo.CreateInternalTransfer', 'P') IS NULL
    EXEC('CREATE PROCEDURE dbo.CreateInternalTransfer AS SELECT 1;');
GO

ALTER PROCEDURE dbo.CreateInternalTransfer
    @BankAccountFromID int,
    @BudgetLineFromID int,
    @BankAccountToID int,
    @BudgetLineToID int,
    @Amount money,
    @Note varchar(1024) = NULL
AS

-- TODO: add mapped transaction records
--       add 'M' period adjustment records
--       exclude internal transfers from the GUI

SET NOCOUNT ON

DECLARE @CurrentPeriodID int;
DECLARE @AccountChar varchar(10);
DECLARE @DebitLineChar varchar(10);
DECLARE @CreditLineChar varchar(10);

DECLARE @ErrorMessages table
(
    ErrorLevel int not null,
    ErrorMessage varchar(1024)
);

BEGIN TRY

    -- verify bank accounts match
    IF COALESCE(@BankAccountFromID, @BankAccountToID + 1, -1) != COALESCE(@BankAccountToID, @BankAccountFromID + 1, -1)
        INSERT INTO @ErrorMessages(ErrorLevel, ErrorMessage)
        VALUES(16, 'Currently transfers are only allowed within the same bank account.');

    -- verify bank account exists
    IF NOT EXISTS
    (
        SELECT TOP 1 1
        FROM dbo.BankAccounts
        WHERE BankAccountId = @BankAccountFromID
    )
        INSERT INTO @ErrorMessages(ErrorLevel, ErrorMessage)
        VALUES(16, 'Transfer failed. The bank account is invalid.');

    -- verify lines are different
    IF COALESCE(@BudgetLineFromID, @BudgetLineToID + 1, -1) != COALESCE(@BudgetLineToID, @BudgetLineFromID + 1, -1)
        INSERT INTO @ErrorMessages(ErrorLevel, ErrorMessage)
        VALUES(16, 'Cannot transfer from and to the same budget line.');

    -- verify budget lines exist
    IF 2 != (
        SELECT COUNT(DISTINCT BudgetLineId)
        FROM dbo.BudgetLines
        WHERE BudgetLineId IN(@BudgetLineFromID, @BudgetLineToID)
    )
        INSERT INTO @ErrorMessages(ErrorLevel, ErrorMessage)
        VALUES(16, 'Transfer failed. A budget line was invalid.');

    -- ensure the amount is > 0
    IF ISNULL(@Amount, 0.0) <= 0
        INSERT INTO @ErrorMessages(ErrorLevel, ErrorMessage)
        VALUES(16, 'Transfer failed. The transfer amount must be greater than zero.');

    IF 0 < (SELECT COUNT(0) FROM @ErrorMessages)
        GOTO ExitProc;

    -- lookup current period
    SELECT @CurrentPeriodId = MIN(PeriodId)
    FROM dbo.Periods
    WHERE IsOpen = 1;

    -- convert values
    SELECT @AccountChar = CAST(@BankAccountFromID AS varchar)
    SELECT @DebitLineChar = CAST(@BudgetLineFromID AS varchar)
    SELECT @CreditLineChar = CAST(@BudgetLineToID AS varchar)

    BEGIN TRANSACTION

    -- write debit transaction
    INSERT INTO dbo.Transactions(BankAccountId, TransactionNo, TransactionDate,
      TransactionDesc, Amount, TransactionTypeCode, Recipient, Notes, IsMapped)
    SELECT @BankAccountFromID, 
      'IntXfer' + CONVERT(char(8), GETDATE(), 112) + '>' + @AccountChar
        + '-' + @AccountChar + ':' + @DebitLineChar + '-' + @CreditLineChar + ':D',
      CONVERT(char(8), GETDATE(), 112),
      'Internal Transfer', -@Amount, 'I', 'Internal Transfer',
      @Note, 1 AS IsMapped;

    -- add debit mapped transaction
    INSERT INTO dbo.MappedTransactions(TransactionId, BudgetLineId, Amount)
    SELECT SCOPE_IDENTITY(), @BudgetLineFromID, -@Amount;

    -- write credit transaction
    INSERT INTO dbo.Transactions(BankAccountId, TransactionNo, TransactionDate,
      TransactionDesc, Amount, TransactionTypeCode, Recipient, Notes, IsMapped)
    SELECT @BankAccountToID,
      'IntXfer' + CONVERT(char(8), GETDATE(), 112) + '>' + @AccountChar
        + '-' + @AccountChar + ':' + @DebitLineChar + '-' + @CreditLineChar + ':C',
      CONVERT(char(8), GETDATE(), 112),
      'Internal Transfer', @Amount, 'I', 'Internal Transfer',
      @Note, 1 AS IsMapped;

    -- add credit mapped transaction
    INSERT INTO dbo.MappedTransactions(TransactionId, BudgetLineId, Amount)
    SELECT SCOPE_IDENTITY(), @BudgetLineToID, @Amount;

    INSERT INTO dbo.PeriodAdjustments(PeriodID, BankAccountID, BudgetLineID, AdjustmentTypeCode, Amount)
    VALUES(@CurrentPeriodId, @BankAccountFromID, @BudgetLineFromID, 'M', -@Amount),
      (@CurrentPeriodId, @BankAccountToID, @BudgetLineToID, 'M', @Amount);

    COMMIT TRANSACTION

    INSERT INTO @ErrorMessages(ErrorLevel, ErrorMessage)
    VALUES(0, 'Transfer succeeded.');

    ExitProc:

END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;

    INSERT INTO @ErrorMessages(ErrorLevel, ErrorMessage)
    VALUES(16, 'Transfer failed. A database error occurred.');
END CATCH

SELECT ErrorLevel, ErrorMessage
FROM @ErrorMessages;

GO

grant execute, view definition on dbo.CreateInternalTransfer to exec_procs;
go

