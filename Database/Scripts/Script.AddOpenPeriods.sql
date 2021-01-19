declare @LastPeriodStartDate date;
declare @MonthsToAdd int = 12;

select top 1 @LastPeriodStartDate = PeriodStartDate
from dbo.Periods
order by PeriodId desc;

with
Seq as
(
    select top(@MonthsToAdd) row_number() over(order by id) as Num
    from dbo.sysobjects
)
insert into dbo.Periods(PeriodId, IsOpen)
select convert(char(6), dateadd(month, Num, @LastPeriodStartDate), 112) as PeriodId,
    1 as IsOpen
from Seq;
