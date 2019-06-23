using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BudgetTools.Models.ViewModels
{
    public class PeriodBudgetViewModel
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
        public bool IsAccrued { get; set; }
    }
}