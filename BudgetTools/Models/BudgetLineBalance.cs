using TemplateEngine.Formats;

namespace BudgetTools.Models
{

    public class BudgetLineBalance
    {
        public int BudgetLineId { get; set; }
        public string BudgetLineName { get; set; }
        public bool IsSource { get; set; }

        [FormatCurrency(2)]
        public decimal Balance { get; set; }
    }

}