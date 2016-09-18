-- use this script to get daily balances which can be added to the starting balance to compare with
--  bank records in order to find missing transactions
SELECT TransactionDate, SUM(Amount)
FROM dbo.Transactions
WHERE BankAccountID = 1
  AND TransactionDate BETWEEN '20140401' AND '20140430'
GROUP BY TransactionDate

SELECT SUM(Balance)
FROM dbo.PeriodBalances
WHERE BankAccountID = 1
  AND PeriodID = 201403
