CREATE PROCEDURE dbo.uspImportTransactions
AS

SET NOCOUNT ON;
--SET XACT_ABORT ON;

BEGIN TRY

  BEGIN TRANSACTION;

  INSERT INTO dbo.Transactions(BankAccountId, TransactionNo, TransactionDate,
    TransactionDesc, CheckNo, Amount, TransactionTypeCode, IsMapped)
  SELECT st.BankAccountId, st.TransactionNo, st.TransactionDate,
    st.TransactionDesc, st.CheckNo, st.Amount, 'S' AS TransactionTypeCode, 0 AS IsMapped
  FROM dbo.StageTransactions st
    LEFT JOIN dbo.Transactions t ON st.BankAccountId = t.BankAccountId
      AND st.TransactionNo = t.TransactionNo
  WHERE t.BankAccountId IS NULL;

  TRUNCATE TABLE dbo.StageTransactions;

  COMMIT TRANSACTION;

END TRY
BEGIN CATCH
  IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
  THROW;
END CATCH
GO