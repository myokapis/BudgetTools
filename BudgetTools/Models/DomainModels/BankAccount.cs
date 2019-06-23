namespace BudgetTools.Models.DomainModels
{

    public class BankAccount
    {
        public int BankAccountId { get; set; }
        public string BankAccountName { get; set; }
        public string BankAccountType { get; set; }
        public string BankName { get; set; }
        public string BankRoutingNumber { get; set; }
        public string BankAccountNumber { get; set; }
        public bool IsActive { get; set; }
        public string UploadValidator { get; set; }
    }

}