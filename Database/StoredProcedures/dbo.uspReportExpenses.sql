--IF NOT EXISTS(SELECT * FROM sys.procedures WHERE object_id = OBJECT_ID('dbo.uspReportExpenses'))
--  EXEC('CREATE PROCEDURE dbo.uspReportExpenses');

--ALTER PROCEDURE dbo.uspReportExpenses
declare
  @BeginPeriodID int,
  @EndPeriodID int
--AS

SET NOCOUNT ON;
select @beginperiodid = 201410, @endperiodid = 201412;
DECLARE @Bucket1 money;
DECLARE @Bucket2 money;

DECLARE @ReportLines TABLE
(
  BudgetLineID int NOT NULL,
  BankAccountID int NOT NULL,
  DistributionMethod char(1) NOT NULL,
  BudgetGroupName varchar(50) NOT NULL,
  BudgetCategoryName varchar(50) NOT NULL,
  BudgetLineName varchar(100) NOT NULL,
  PRIMARY KEY(BudgetLineID, BankAccountID)
);

DECLARE @Distributions TABLE
(
  BankAccountID int NOT NULL,
  PeriodID int NOT NULL,
  Bucket1 money NOT NULL,
  Bucket2 money NOT NULL,
  Total money NOT NULL,
  PRIMARY KEY(BankAccountID, PeriodID)
);

-- add lines on which to report for my account
INSERT INTO @ReportLines(BudgetLineID, BankAccountID, DistributionMethod,
  BudgetGroupName, BudgetCategoryName, BudgetLineName)
SELECT BudgetLineID, 1 AS BankAccountID, 'G' AS DistributionMethod,
  BudgetGroupName, BudgetCategoryName, BudgetLineName
FROM dbo.vwBudgetGroupCategoryLine
WHERE BudgetLineID IN(1, 2, 3, 4, 6, 11, 12, 13, 14, 15, 16, 18, 20, 61, 64, 65, 66);

-- add lines on which to report for household account
INSERT INTO @ReportLines(BudgetLineID, BankAccountID, DistributionMethod,
  BudgetGroupName, BudgetCategoryName, BudgetLineName)
SELECT BudgetLineID, 6 AS BankAccountID, 'P' AS DistributionMethod,
  BudgetGroupName, BudgetCategoryName, BudgetLineName
FROM dbo.vwBudgetGroupCategoryLine
WHERE BudgetLineID IN(1, 2, 3, 4, 6, 9, 11, 12, 13, 14, 15, 16, 18, 20, 23, 24, 25, 46, 61, 64, 65, 66);

-- lookup contributions
SELECT p.PeriodID,
  ISNULL(SUM(CASE WHEN BudgetLineID = 1065 THEN Amount ELSE 0.00 END), 0.00),
  ISNULL(SUM(CASE WHEN BudgetLineID = 1064 THEN Amount ELSE 0.00 END), 0.00)
FROM dbo.Periods p
  LEFT JOIN vwPeriodActuals a ON p.PeriodID = a.PeriodID
    AND a.BankAccountID IN(6, 7)
WHERE p.PeriodID BETWEEN @BeginPeriodID AND @EndPeriodID
GROUP BY p.PeriodID;

-- gather expense data
SELECT DATENAME(month, CAST(a.PeriodID * 100 + 1 AS varchar(10)))
  + ' ' + CAST(a.PeriodID / 100 AS char(4)) AS ReportMonth,
  a.BudgetGroupName, a.BudgetCategoryName, a.BudgetLineName,
  SUM(-Amount) AS Amount,
  SUM(
    CASE l.DistributionMethod
      WHEN 'G' THEN -Amount
      WHEN 'P' THEN -Amount * @Bucket1 / (@Bucket1 + @Bucket2)
      ELSE 0.00
    END
  ) AS Bucket1,
  SUM(
    CASE l.DistributionMethod
      WHEN 'C' THEN -Amount
      WHEN 'P' THEN -Amount * @Bucket2 / (@Bucket1 + @Bucket2)
      ELSE 0.00
    END
  ) AS Bucket2
FROM dbo.vwPeriodActuals a
  INNER JOIN @ReportLines l ON a.BudgetLineID = l.BudgetLineID
    AND a.BankAccountID = l.BankAccountID
WHERE PeriodID BETWEEN @BeginPeriodID AND @EndPeriodID
  AND TransactionTypeCode = 'S'
GROUP BY a.PeriodID, a.BudgetGroupName, a.BudgetCategoryName, a.BudgetLineName;
