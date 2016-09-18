MERGE INTO dbo.AdjustmentTypes a
USING
(
  SELECT 'A' AS AdjustmentTypeCode, 'Accrual' AS AdjustmentTypeDesc
  UNION ALL SELECT 'C', 'Credit'
  UNION ALL SELECT 'P', 'Projected'
  UNION ALL SELECT 'E', 'Expenses'
  UNION ALL SELECT 'I', 'Income'
  UNION ALL SELECT 'M', 'Manual Adjustment'
  UNION ALL SELECT 'R', 'Expenses (accrual account)'
  UNION ALL SELECT 'X', 'Transfers'
) b
ON a.AdjustmentTypeCode = b.AdjustmentTypeCode
WHEN MATCHED THEN
  UPDATE
  SET AdjustmentTypeDesc = b.AdjustmentTypeDesc
WHEN NOT MATCHED BY TARGET THEN
  INSERT(AdjustmentTypeCode, AdjustmentTypeDesc)
  VALUES(b.AdjustmentTypeCode, b.AdjustmentTypeDesc);
