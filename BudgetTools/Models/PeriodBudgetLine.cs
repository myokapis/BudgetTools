using TemplateEngine.Formats;

namespace BudgetTools.Models
{

    public class PeriodBudgetLine
    {
        public int BudgetLineId { get; set; }
        public string BudgetLineName { get; set; }
        public string BudgetCategoryName { get; set; }
        [FormatCurrency(2)]
        public decimal PlannedAmount { get; set; }
        [FormatCurrency(2)]
        public decimal AllocatedAmount { get; set; }
        [FormatCurrency(2)]
        public decimal AccruedAmount { get; set; }
        [FormatCurrency(2)]
        public decimal ActualAmount { get; set; }
        [FormatCurrency(2)]
        public decimal RemainingAmount { get; set; }
        [FormatCurrency(2)]
        public decimal AccruedBalance { get; set; }
        [FormatCurrency(2)]
        public decimal TotalCashAmount { get; set; }
        public bool IsAccrued { get; set; }
        public bool IsDetail { get; set; }
    }

}