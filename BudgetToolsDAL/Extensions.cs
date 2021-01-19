using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System.Text;
using Microsoft.Data.SqlClient;

namespace BudgetToolsDAL
{

    public static class Extensions
    {

        //// TODO: decide if i want to use this
        //public static IQueryable<T> FromSqlProc<T>(this DbSet<T> dbSet, string procName, SqlParameter returnValue, params SqlParameter[] sqlParameters) where T : class
        //{
        //    var paramNames = (sqlParameters.Length == 0) ? "" :
        //        $" {string.Join(", ", sqlParameters.Select(p => p.ParameterName))}";

        //    var command = $"exec {returnValue.ParameterName} = {procName}{paramNames};";
        //    var sqlParams = sqlParameters.ToList<object>();
        //    sqlParams.Add(returnValue);

        //    return dbSet.FromSqlRaw(command, sqlParams.ToArray());
        //}

    }

}
