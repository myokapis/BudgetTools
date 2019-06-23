namespace BudgetTools.Classes
{

    public interface IPageScope
    {
        int BankAccountId { get; set; }
        int PeriodId { get; set; }
    }

    public class PageScope : IPageScope
    {

        //public PageScope(int bankAccountId, int periodId)
        //{
        //    this.BankAccountId = bankAccountId;
        //    this.PeriodId = periodId;
        //}

        public int BankAccountId { get; set; } = 0;
        public int PeriodId { get; set; } = 0;
    }
}