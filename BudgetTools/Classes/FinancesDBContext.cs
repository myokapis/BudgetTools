using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;

namespace BudgetTools.Models.DomainModels
{
  // DBContext initializer, DbSet properties, and events
  public partial class BudgetToolsDBContext : DbContext
  {
    // create an initializer that calls the base initializer with the connection string name to be used
    public BudgetToolsDBContext()
      : base("DefaultConnection")
    {
    }

    public DbSet<Transaction> Transactions { get; set; }
    public DbSet<TransactionType> TransactionTypes { get; set; }
    public DbSet<BudgetLine> BudgetLines { get; set; }
    public DbSet<StageTransaction> StageTransactions { get; set; }
    public DbSet<BankAccount> BankAccounts { get; set; }
    public DbSet<MappedTransaction> MappedTransactions { get; set; }
    public DbSet<Period> Periods { get; set; }
    public DbSet<Allocation> Allocations { get; set; }
    public DbSet<BudgetCategory> BudgetCategories { get; set; }
    public DbSet<PeriodBalance> PeriodBalances { get; set; }

    protected override void OnModelCreating(DbModelBuilder modelBuilder)
    {
      modelBuilder.Entity<BudgetLine>().Map(model => { model.MapInheritedProperties(); model.ToTable("vwBudgetLineSets"); });
      modelBuilder.Entity<PeriodBalance>().Map(model => { model.MapInheritedProperties(); model.ToTable("vwPeriodBalances"); });
    }

  }

}