-- TODO: add mapped transaction records
--       add 'M' period adjustment records
--       exclude internal transfers from the GUI

CREATE PROCEDURE dbo.CreateInternalTransfer
    @PeriodID                   int,
    @BankAccountID              int,
    @Amount                     money,
    @LineFromName               varchar(50),
    @LineToName                 varchar(50),
    @Note                       varchar(1024) = NULL
AS

SET NOCOUNT ON

DECLARE @LineFromID             int
DECLARE @LineToID               int
DECLARE @AccountChar            varchar(10)
DECLARE @DebitLineChar          varchar(10)
DECLARE @CreditLineChar         varchar(10)
DECLARE @ErrorMessage           varchar(1024)

BEGIN TRY

-- lookup from line
SELECT @LineFromID = BudgetLineID
FROM dbo.BudgetLines (NOLOCK)
WHERE BudgetLineName = @LineFromName

-- lookup to line
SELECT @LineToID = BudgetLineID
FROM dbo.BudgetLines (NOLOCK)
WHERE BudgetLineName = @LineToName

-- check for error
IF @LineToID IS NULL OR @LineFromID IS NULL
    RAISERROR('Invalid budget line(s)', 15, 1)

SELECT @AccountChar = CAST(@BankAccountID AS varchar)
SELECT @DebitLineChar = CAST(@LineFromID AS varchar)
SELECT @CreditLineChar = CAST(@LineToID AS varchar)

BEGIN TRANSACTION

-- write debit transaction
INSERT INTO dbo.Transactions(BankAccountId, TransactionNo, TransactionDate,
  TransactionDesc, Amount, TransactionTypeCode, Recipient, Notes, IsMapped)
SELECT @BankAccountID, 
  'IntXfer' + CONVERT(char(8), GETDATE(), 112) + '>' + @AccountChar
    + '-' + @AccountChar + ':' + @DebitLineChar + '-' + @CreditLineChar + ':D',
  CONVERT(char(8), GETDATE(), 112),
  'Internal Transfer', -@Amount, 'I', 'Internal Transfer',
  @Note, 1 AS IsMapped;

-- write credit transaction
INSERT INTO dbo.Transactions(BankAccountId, TransactionNo, TransactionDate,
  TransactionDesc, Amount, TransactionTypeCode, Recipient, Notes, IsMapped)
SELECT @BankAccountID,
  'IntXfer' + CONVERT(char(8), GETDATE(), 112) + '>' + @AccountChar
    + '-' + @AccountChar + ':' + @DebitLineChar + '-' + @CreditLineChar + ':C',
  CONVERT(char(8), GETDATE(), 112),
  'Internal Transfer', @Amount, 'I', 'Internal Transfer',
  @Note, 1 AS IsMapped;

COMMIT TRANSACTION
SELECT @ErrorMessage = 'Transfer successful'

END TRY
BEGIN CATCH
    SELECT @ErrorMessage = ERROR_MESSAGE()
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION
END CATCH

SELECT @ErrorMessage AS ErrorMessage
