create or alter procedure dbo.uspPeriodBudgetGet
    @PeriodId int,
    @BankAccountId int
as

declare @PeriodStartDate datetime;
declare @PeriodEndDate datetime;
declare @PreviousPeriodId int;

declare @CashBudgetLineID int = dbo.GetParameter('Cash');

declare @Transactions table
(
    BudgetLineId int not null primary key,
    Amount money not null,
    TransferAdjustment money not null
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

-- add any missing allocations
insert into dbo.Allocations(PeriodId, BudgetLineId, PlannedAmount,
    AllocatedAmount, AccruedAmount, BankAccountID)
select @PeriodId, m.BudgetLineId, 0.0, 0.0, 0.0, @BankAccountId
from dbo.MappedTransactions m
inner join dbo.Transactions t on m.TransactionId = t.TransactionId
where -- can't decide about this
--m.BudgetLineId != @CashBudgetLineID
--and 
t.BankAccountId = @BankAccountId
and t.TransactionDate >= @PeriodStartDate
and t.TransactionDate < dateadd(day, 1, @PeriodEndDate)
and not exists
(
    select 1
    from dbo.Allocations a
    where a.PeriodId = @PeriodId
    and a.BankAccountId = @BankAccountId
    and m.BudgetLineId = a.BudgetLineId
)
group by m.BudgetLineId;

-- summarize transactions
insert into @Transactions(BudgetLineId, Amount, TransferAdjustment)
select BudgetLineId,
    sum(-1.0 * mt.Amount) as Amount,
    sum(iif(t.TransactionTypeCode = 'X' and mt.Amount > 0.0, -mt.Amount, 0.0)) as TransferAdjustment
from dbo.Transactions t
inner join dbo.MappedTransactions mt on t.TransactionId = mt.TransactionId
where t.TransactionDate >= @PeriodStartDate
and t.TransactionDate < dateadd(day, 1, @PeriodEndDate)
and t.BankAccountId = @BankAccountId
and t.TransactionTypeCode in('S', 'X')
group by BudgetLineId;

-- return the budget for the period
select a.BudgetLineId, bl.BudgetLineName, bl.BudgetCategoryName,
    a.PlannedAmount, a.AllocatedAmount, a.AccruedAmount,
    isnull(t.Amount, 0.0) as ActualAmount,
    a.AllocatedAmount - isnull(t.Amount, 0.0)
        + iif(a.AccruedAmount <= 0.0, 0.0, a.AccruedAmount) as RemainingAmount,
    isnull(pb.ProjectedBalance, 0.0) as AccruedBalance,
    case
        when bl.BudgetGroupName != 'Expenses' then a.AllocatedAmount
        -- TODO: enforce a rule that transfers can only be applied to cash, assets, or accrued lines
        when bl.IsAccrued = 0 then a.AllocatedAmount
        when a.AccruedAmount >= 0.0 then a.AllocatedAmount - isnull(t.TransferAdjustment, 0.0)
        else a.AllocatedAmount + -a.AccruedAmount - isnull(t.TransferAdjustment, 0.0)
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
