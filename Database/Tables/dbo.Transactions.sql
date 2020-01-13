IF OBJECT_ID('dbo.Transactions', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Transactions
    (
        TransactionId int IDENTITY(1, 1) NOT NULL,
        BankAccountId int NOT NULL,
        TransactionNo varchar(255) NOT NULL,
        TransactionDate datetime NOT NULL,
        TransactionDesc varchar(255) NOT NULL,
        CheckNo int NULL,
        Amount money NOT NULL,
        TransactionTypeCode char(1) NOT NULL,
        Recipient varchar(255) NULL,
        Notes varchar(255) NULL,
        IsMapped bit NOT NULL,
        CONSTRAINT PK_dbo_Transactions PRIMARY KEY CLUSTERED(TransactionId),
        CONSTRAINT FK_dbo_Transactions_dbo_TransactionTypes FOREIGN KEY(TransactionTypeCode) REFERENCES dbo.TransactionTypes(TransactionTypeCode)
    );

    CREATE UNIQUE INDEX ixu_dbo_Transactions_AltKey ON dbo.Transactions(BankAccountId, TransactionNo);
END
GO

IF EXISTS
(
    SELECT top 1 1
    FROM sys.columns c
    INNER JOIN sys.types t on c.system_type_id = t.system_type_id
        AND c.user_type_id = t.user_type_id
    WHERE c.object_id = OBJECT_ID('dbo.Transactions', 'U')
    AND c.name = 'TransactionDate'
    AND t.name = 'datetime'
)
BEGIN
    ALTER TABLE dbo.Transactions
    ALTER COLUMN TransactionDate date not null;
END
GO

IF NOT EXISTS
(
    SELECT TOP 1 1
    FROM sys.columns c
    WHERE c.object_id = OBJECT_ID('dbo.Transactions', 'U')
    AND c.name = 'Balance'
)
BEGIN
    ALTER TABLE dbo.Transactions
    ADD Balance money not null default(0.0);
END
GO
