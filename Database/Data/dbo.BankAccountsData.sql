with
BaseData(BankAccountID, UploadValidator) as
(
    select 1, '920156438K78'
    union all select 2, '920156438S00'
    union all select 3, '920156438K77'
    union all select 6, '920336884K75'
    union all select 7, '920336884S00'
    union all select 9, '920448085S00'
)
update a
set UploadValidator = b.UploadValidator
from dbo.BankAccounts a
inner join BaseData b on a.BankAccountId = b.BankAccountId
where a.UploadValidator is null;
