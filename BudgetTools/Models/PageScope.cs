using System.ComponentModel.DataAnnotations;

namespace BudgetTools.Models
{

    public class PageScope
    {
        [Range(1, 100)]
        public int BankAccountId { get; set; }

        [Range(200001, 209912)]
        public int CurrentPeriodId { get; set; }

        [Range(200001, 209912)]
        public int PeriodId { get; set; }
    }

}