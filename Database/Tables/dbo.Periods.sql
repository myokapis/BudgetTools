CREATE TABLE dbo.Periods
(
  PeriodId int NOT NULL,
  PeriodStartDate AS (CONVERT(date,CONVERT(char(6),PeriodId)+'01',(112))) PERSISTED NOT NULL,
  PeriodEndDate  AS (DATEADD(day, -1, DATEADD(month, 1, CONVERT(date, CONVERT(char(6), PeriodId) + '01', 112)))) PERSISTED NOT NULL,
  IsOpen bit NOT NULL,
  CONSTRAINT CK_dbo_Periods_PeriodId CHECK  ((isdate(CONVERT(varchar(50),PeriodId)+'01')=(1))),
  CONSTRAINT PK_dbo_Periods PRIMARY KEY CLUSTERED(PeriodId)
);

--DECLARE @Date date = '2009-12-01';

--WHILE @Date < GETDATE()
--BEGIN

--  INSERT INTO dbo.Periods(PeriodId, IsOpen)
--  SELECT CONVERT(char(6), @Date, 112) AS PeriodId,
--    CASE WHEN @Date < '2014-01-01' THEN 0 ELSE 1 END AS IsOpen;

--  SELECT @Date = DATEADD(month, 1, @Date);

--END

--SELECT *
--FROM dbo.Periods

--alter TABLE dbo.Periods
--add
--  CONSTRAINT CK_dbo_Periods_PeriodId CHECK(ISDATE(CAST(PeriodId AS varchar(50)) + '01') = 1)
