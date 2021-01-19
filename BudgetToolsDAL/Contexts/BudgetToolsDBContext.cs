using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using BudgetToolsDAL.Models;
using Microsoft.AspNetCore.SignalR;

namespace BudgetToolsDAL.Contexts
{
    // TODO: move the stored procedure calls and queries into the repository

    public interface IBudgetToolsDBContext
    {
        DbSet<Transaction> Transactions { get; set; }
        DbSet<TransactionType> TransactionTypes { get; set; }
        DbSet<StagedTransaction> StagedTransactions { get; set; }
        DbSet<BankAccount> BankAccounts { get; set; }
        DbSet<MappedTransaction> MappedTransactions { get; set; }
        DbSet<Period> Periods { get; set; }
        DbSet<Allocation> Allocations { get; set; }
        DbSet<BudgetGroup> BudgetGroups { get; set; }
        DbSet<BudgetCategory> BudgetCategories { get; set; }
        DbSet<BudgetLine> BudgetLines { get; set; }
        DbSet<BudgetLineSet> BudgetLineSets { get; set; }
        DbSet<PeriodBalance> PeriodBalances { get; set; }
        DbSet<PeriodBudgetLine> PeriodBudgetLines { get; set; }

        int SaveChanges();
    }

    public partial class BudgetToolsDBContext : DbContext, IBudgetToolsDBContext
    {

        public BudgetToolsDBContext(DbContextOptions<BudgetToolsDBContext> options) : base(options)
        {
        }

        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<TransactionType> TransactionTypes { get; set; }
        public DbSet<BudgetLine> BudgetLines { get; set; }
        public DbSet<BudgetLineSet> BudgetLineSets { get; set; }
        public DbSet<StagedTransaction> StagedTransactions { get; set; }
        public DbSet<BankAccount> BankAccounts { get; set; }
        public DbSet<MappedTransaction> MappedTransactions { get; set; }
        public DbSet<Period> Periods { get; set; }
        public DbSet<Allocation> Allocations { get; set; }
        public DbSet<BudgetCategory> BudgetCategories { get; set; }
        public DbSet<PeriodBalance> PeriodBalances { get; set; }
        public DbSet<BudgetGroup> BudgetGroups { get; set; }
        public DbSet<PeriodBudgetLine> PeriodBudgetLines { get; set; }

        // TODO: refactor the tables having multiple key columns to include a single column non-clustered primary key
        // TODO: remove properties from models where those properties are being set here - OR - switch to using properties
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<PeriodBalance>().HasNoKey();
            modelBuilder.Entity<Message>().HasNoKey();
            modelBuilder.Entity<Allocation>().HasKey(a => new { a.PeriodId, a.BudgetLineId, a.BankAccountId });
            modelBuilder.Entity<BudgetLineSet>().HasKey(b => new { b.BudgetLineSetId, b.BudgetLineId });

        }

    }

}