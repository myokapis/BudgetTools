if object_id('dbo.uspPeriodBudgetGet', 'P') is null
    exec('create procedure dbo.uspPeriodBudgetGet as select 1;');
go

alter procedure dbo.uspPeriodBudgetGet
    @PeriodId int,
    @BankAccountId int
as

declare @PeriodStartDate datetime;
declare @PeriodEndDate datetime;
declare @PreviousPeriodId int;

declare @Transactions table
(
    BudgetLineId int not null primary key,
    Amount money not null
);

-- lookup period dates
select @PeriodStartDate = PeriodStartDate,
    @PeriodEndDate = PeriodEndDate
from dbo.Periods
where PeriodId = @PeriodId;

-- lookup previous period
select @PreviousPeriodId = max(PeriodId)
from dbo.Periods
where PeriodId < @PeriodId;

-- summarize transactions
insert into @Transactions(BudgetLineId, Amount)
select BudgetLineId, sum(-1.0 * mt.Amount) as Amount
from dbo.Transactions t
inner join dbo.MappedTransactions mt on t.TransactionId = mt.TransactionId
where t.TransactionDate >= @PeriodStartDate
and t.TransactionDate < dateadd(day, 1, @PeriodEndDate)
and t.BankAccountId = @BankAccountId
group by BudgetLineId;

-- return the budget for the period
select a.BudgetLineId, bl.BudgetLineName, bl.BudgetCategoryName,
    a.PlannedAmount, a.AllocatedAmount, a.AccruedAmount,
    isnull(t.Amount, 0.0) as ActualAmount,
    a.PlannedAmount - isnull(t.Amount, 0.0) as RemainingAmount,
    isnull(pb.Balance, 0.0) as AccruedBalance,
    case
        when bl.BudgetGroupName != 'Expenses' then a.AllocatedAmount
        when bl.IsAccrued = 0 then a.AllocatedAmount
        when a.AccruedAmount >= 0.0 then a.AllocatedAmount
        else a.AllocatedAmount + -a.AccruedAmount
    end as TotalCashAmount,
    bl.IsAccrued
from dbo.Allocations a
inner join dbo.vwBudgetGroupCategoryLine bl on a.BudgetLineId = bl.BudgetLineId
left join dbo.PeriodBalances pb on a.BudgetLineId = pb.BudgetLineId
    and a.BankAccountId = pb.BankAccountId
    and pb.PeriodId = @PreviousPeriodId
left join @Transactions t on a.BudgetLineId = t.BudgetLineId
where a.PeriodId = @PeriodId
and a.BankAccountId = @BankAccountId;

go

grant execute, view definition on dbo.uspPeriodBudgetGet to exec_procs;
go
