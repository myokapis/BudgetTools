using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.SqlClient;
using System.Data.Entity;

namespace BudgetTools.Models.DomainModels
{
  // DBContext stored procedure calls
  public partial class BudgetToolsDBContext : DbContext
  {

    public void ImportTransactions()
    {
      Database.ExecuteSqlCommand("dbo.uspImportTransactions", new object[] { });
    }

    public void TruncateStagedTransactions()
    {
      Database.ExecuteSqlCommand("TRUNCATE TABLE dbo.StageTransactions;", new object[] { });
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

  }
}