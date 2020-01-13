if object_id('dbo.uspImportTransactions', 'P') is null
    exec('create procedure dbo.uspImportTransactions as select 1;');
go

alter procedure dbo.uspImportTransactions
    @BankAccountId int
as

set nocount on;

begin try

    begin transaction;

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
    and t.BankAccountId IS NULL;

    delete st
    from dbo.StagedTransactions st
    where BankAccountId = @BankAccountId;

    commit transaction;

end try
begin catch
    if @@trancount > 0 rollback transaction;
    throw;
end catch
go
