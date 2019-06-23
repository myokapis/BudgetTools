//using System.ComponentModel.DataAnnotations;
//using System.ComponentModel.DataAnnotations.Schema;
//using System.Collections.Generic;

//namespace BudgetTools.Models.DomainModels
//{
//    // TODO: rename this after conflict has been removed
//    public class PeriodBudgetLine
//    {
//        public int BudgetLineId { get; set; }
//        public string BudgetLineName { get; set; }
//        //public string BudgetCategoryName { get; set; }

//        [Column(TypeName = "money")]
//        public decimal PlannedAmount { get; set; }

//        [Column(TypeName = "money")]
//        public decimal AllocatedAmount { get; set; }

//        [Column(TypeName = "money")]
//        public decimal AccruedAmount { get; set; }

//        [Column(TypeName = "money")]
//        public decimal ActualAmount { get; set; }

//        [Column(TypeName = "money")]
//        public decimal RemainingAmount { get; set; }

//        [Column(TypeName = "money")]
//        public decimal AccruedBalance { get; set; }

//        public bool IsAccrued { get; set; }
//    }

//    public class PeriodBudgetCategory
//    {
//        public int BudgetCategoryId { get; set; }
//        public string BudgetCategoryName { get; set; }

//        [Column(TypeName = "money")]
//        public decimal PlannedAmount { get; set; }

//        [Column(TypeName = "money")]
//        public decimal AllocatedAmount { get; set; }

//        [Column(TypeName = "money")]
//        public decimal AccruedAmount { get; set; }

//        [Column(TypeName = "money")]
//        public decimal ActualAmount { get; set; }

//        [Column(TypeName = "money")]
//        public decimal RemainingAmount { get; set; }

//        [Column(TypeName = "money")]
//        public decimal AccruedBalance { get; set; }

//        public IEnumerable<PeriodBudgetLine> BudgetLines { get; set; }
//    }

//}