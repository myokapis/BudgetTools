CREATE TABLE dbo.AdjustmentTypes
(
	AdjustmentTypeCode char(1) NOT NULL,
	AdjustmentTypeDesc varchar(255) NOT NULL,
  CONSTRAINT PK_AdjustmentTypes PRIMARY KEY CLUSTERED(AdjustmentTypeCode)
);

