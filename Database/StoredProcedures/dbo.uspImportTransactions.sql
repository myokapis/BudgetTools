create or alter procedure dbo.uspImportTransactions
    @BankAccountId int,
    @IsSortDesc bit = 1
as

set nocount on;
set xact_abort on;

insert into dbo.Transactions(BankAccountId, TransactionNo, TransactionDate,
    TransactionDesc, CheckNo, Amount, TransactionTypeCode, IsMapped, Balance)
select st.BankAccountId, st.TransactionNo, st.TransactionDate,
    st.TransactionDesc, st.CheckNo, st.Amount,
    'S' as TransactionTypeCode,
    0 as IsMapped,
    st.Balance
from dbo.StagedTransactions st
left join dbo.Transactions t on st.BankAccountId = t.BankAccountId
    and st.TransactionNo = t.TransactionNo
where st.BankAccountId = @BankAccountId
and t.BankAccountId IS NULL
order by iif(@IsSortDesc = 1, -st.RowNo, st.RowNo);

go
