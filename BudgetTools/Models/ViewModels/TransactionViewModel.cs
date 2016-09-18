using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace BudgetTools.Models.ViewModels
{
  public class TransactionViewModel
  {
    public int TransactionId { get; set; }
    public string TransactionTypeCode { get; set; }
    public string Recipient { get; set; }
    public string Notes { get; set; }
    public int BudgetLine1Id { get; set; }
    public decimal Amount1 { get; set; }
    public int BudgetLine2Id { get; set; }
    public decimal Amount2 { get; set; }
    public int BudgetLine3Id { get; set; }
    public decimal Amount3 { get; set; }
    public decimal TotalAmount { get; set; }
  }
}