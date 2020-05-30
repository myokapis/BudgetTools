IF OBJECT_ID('dbo.StagedTransactions', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.StagedTransactions
    (
	    StagedTransactionId int IDENTITY(1, 1) NOT NULL,
	    BankAccountId int NOT NULL,
	    TransactionNo varchar(255) NOT NULL,
	    TransactionDate datetime NOT NULL,
	    TransactionDesc varchar(255) NOT NULL,
	    CheckNo int NULL,
	    Amount float NOT NULL,
      CONSTRAINT PK_dbo_StagedTransactions PRIMARY KEY NONCLUSTERED(StagedTransactionId)
    );

    CREATE CLUSTERED INDEX ixcl_dbo_StagedTransactions ON dbo.StagedTransactions(BankAccountId, TransactionNo);
END
GO

IF NOT EXISTS
(
    SELECT TOP 1 1
    FROM sys.columns c
    WHERE c.object_id = OBJECT_ID('dbo.StagedTransactions', 'U')
    AND c.name = 'RowNo'
)
BEGIN

    DROP TABLE IF EXISTS dbo.StagedTransactions;

    CREATE TABLE dbo.StagedTransactions
    (
	    StagedTransactionId int IDENTITY(1, 1) NOT NULL,
	    BankAccountId int NOT NULL,
	    TransactionNo varchar(255) NOT NULL,
	    TransactionDate datetime NOT NULL,
	    TransactionDesc varchar(255) NOT NULL,
	    CheckNo int NULL,
	    Amount float NOT NULL,
        Balance money not null default(0.0),
        RowNo int not null,
      CONSTRAINT PK_dbo_StagedTransactions PRIMARY KEY NONCLUSTERED(StagedTransactionId)
    );

    CREATE CLUSTERED INDEX ixcl_dbo_StagedTransactions ON dbo.StagedTransactions(BankAccountId, TransactionNo);

END
GO