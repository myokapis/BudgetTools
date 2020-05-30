using System.Collections.Generic;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using BudgetToolsDAL.Models;

namespace BudgetToolsDAL.Contexts
{
    // TODO: move the stored procedure calls and queries into the repository
    
    public interface IBudgetToolsDBContext
    {
        Database Database { get; }
        DbSet<Transaction> Transactions { get; set; }
        DbSet<TransactionType> TransactionTypes { get; set; }
        DbSet<StagedTransaction> StageTransactions { get; set; }
        DbSet<BankAccount> BankAccounts { get; set; }
        DbSet<MappedTransaction> MappedTransactions { get; set; }
        DbSet<Period> Periods { get; set; }
        DbSet<Allocation> Allocations { get; set; }
        DbSet<BudgetGroup> BudgetGroups { get; set; }
        DbSet<BudgetCategory> BudgetCategories { get; set; }
        DbSet<BudgetLine> BudgetLines { get; set; }
        DbSet<BudgetLineSet> BudgetLineSets { get; set; }
        DbSet<PeriodBalance> PeriodBalances { get; set; }

        void ImportTransactions(int bankAccountId, bool isSortDesc = true);
        void DeleteStagedTransactions(int bankAccountId);
        void UpdatePeriodBalances(int PeriodId, bool ClosePeriod);
        Period CurrentPeriod { get; }
        Period PreviousPeriod { get; }

        void SaveChanges();
    }

    public partial class BudgetToolsDBContext : DbContext, IBudgetToolsDBContext
    {
        
        public BudgetToolsDBContext(string nameOrConnectionString) : base(nameOrConnectionString)
        {
        }

        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<TransactionType> TransactionTypes { get; set; }
        public DbSet<BudgetLine> BudgetLines { get; set; }
        public DbSet<BudgetLineSet> BudgetLineSets { get; set; }
        public DbSet<StagedTransaction> StageTransactions { get; set; }
        public DbSet<BankAccount> BankAccounts { get; set; }
        public DbSet<MappedTransaction> MappedTransactions { get; set; }
        public DbSet<Period> Periods { get; set; }
        public DbSet<Allocation> Allocations { get; set; }
        public DbSet<BudgetCategory> BudgetCategories { get; set; }
        public DbSet<PeriodBalance> PeriodBalances { get; set; }
        public DbSet<BudgetGroup> BudgetGroups { get; set; }

        public void ImportTransactions(int bankAccountId, bool isSortDesc = true)
        {
            Database.ExecuteSqlCommand("dbo.uspImportTransactions @BankAccountId", new object[]
            {
                new SqlParameter("@BankAccountId", bankAccountId),
                new SqlParameter("@IsSortDesc", isSortDesc)
            });
        }

        public void DeleteStagedTransactions(int bankAccountId)
        {
            Database.ExecuteSqlCommand("delete dbo.StagedTransactions where BankAccountId = @BankAccountId;", new object[]
            {
                new SqlParameter("@BankAccountId", bankAccountId)
            });
        }

        public void UpdatePeriodBalances(int PeriodId, bool ClosePeriod)
        {
            Database.ExecuteSqlCommand("dbo.uspUpdatePeriodBalances @PeriodID, @ClosePeriod", new object[]
            {
                new SqlParameter("@PeriodID", PeriodId),
                new SqlParameter("@ClosePeriod", ClosePeriod)
            });
        }

        public Period CurrentPeriod
        {
            get
            {
                var period = Periods.Where(p => p.IsOpen == true)
                    .OrderBy(o => o.PeriodId).First();

                return period;
            }
        }

        public Period PreviousPeriod
        {
            get
            {
                var period = Periods.Where(p => p.IsOpen == false)
                    .OrderByDescending(o => o.PeriodId).First();

                return period;
            }
        }

        void IBudgetToolsDBContext.SaveChanges() => SaveChanges();


        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<PeriodBalance>().Map(model => { model.MapInheritedProperties(); model.ToTable("vwPeriodBalances"); });
        }

    }

}