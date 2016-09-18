CREATE TABLE dbo.Parameters
(
	ParameterName varchar(50) NOT NULL,
	ParameterDesc varchar(255) NOT NULL,
	ParameterValue varchar(1024) NULL,
  CONSTRAINT PK_Parameters PRIMARY KEY CLUSTERED(ParameterName)
);
