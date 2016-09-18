CREATE TABLE dbo.TransactionTypes
(
  TransactionTypeCode char(1) NOT NULL,
  TransactionTypeDesc varchar(255) NOT NULL,
  CONSTRAINT PK_dbo_TransactionTypes PRIMARY KEY CLUSTERED(TransactionTypeCode)
) 