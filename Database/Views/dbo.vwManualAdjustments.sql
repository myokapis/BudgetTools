ALTER VIEW dbo.vwManualAdjustments
AS

SELECT PeriodID, BankAccountID, BudgetLineID, AdjustmentTypeCode, Amount
FROM dbo.PeriodAdjustments
WHERE PeriodID = (SELECT MIN(PeriodID) FROM dbo.Periods WHERE IsOpen = 1)
  AND AdjustmentTypeCode = 'M';
