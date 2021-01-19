using System;

namespace BudgetTools.Models
{

    public class Period
    {
        public int PeriodId { get; set; }
        public DateTime PeriodStartDate { get; set; }
        public DateTime PeriodEndDate { get; set; }
        public bool IsOpen { get; set; }
    }

}
