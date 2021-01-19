namespace BudgetToolsBLL.Models
{

    public class PeriodBudgetLine
    {
        public int BudgetLineId { get; set; }
        public string BudgetLineName { get; set; }
        public string BudgetCategoryName { get; set; }
        public decimal PlannedAmount { get; set; }
        public decimal AllocatedAmount { get; set; }
        public decimal AccruedAmount { get; set; }
        public decimal ActualAmount { get; set; }
        public decimal RemainingAmount { get; set; }
        public decimal AccruedBalance { get; set; }
        public decimal TotalCashAmount { get; set; }
        public bool IsAccrued { get; set; }
        public bool IsDetail { get; set; }
    }

}
