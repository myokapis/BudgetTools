namespace BudgetToolsBLL.Models
{

    public class PeriodBalance
    {
        public int PeriodId { get; set; }
        public int BankAccountId { get; set; }
        public int BudgetLineId { get; set; }
        public string BankAccountName { get; set; }
        public string BudgetGroupName { get; set; }
        public string BudgetCategoryName { get; set; }
        public string BudgetLineName { get; set; }
        public decimal PreviousBalance { get; set; }
        public decimal Balance { get; set; }
        public decimal ProjectedBalance { get; set; }
        public int Level { get; set; }
    }

}
