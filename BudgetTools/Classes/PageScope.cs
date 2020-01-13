namespace BudgetTools.Classes
{

    public interface IPageScope
    {
        int BankAccountId { get; set; }
        int PeriodId { get; set; }
        int CurrentPeriodId { get; set; }
    }

    public class PageScope : IPageScope
    {
        public int BankAccountId { get; set; } = 0;
        public int PeriodId { get; set; } = 0;
        public int CurrentPeriodId { get; set; } = 0;
    }
}