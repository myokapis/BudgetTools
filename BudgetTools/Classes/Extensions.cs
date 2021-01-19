using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Text;
using System.Data;


namespace BudgetTools.Classes
{

    public static class Extensions
    {

        #region ISession Extensions

        public static T Get<T>(this ISession session, string key) where T : class
        {
            var item = session.GetString(key);
            if (item == null) return null;

            return JsonConvert.DeserializeObject<T>(item);
        }

        public static T Set<T>(this ISession session, string key, T value) where T : class
        {
            var item = JsonConvert.SerializeObject(value);
            session.SetString(key, item);
            return value;
        }

        public static T Update<T>(this ISession session, string key, Action<T> action) where T : class, new()
        {
            var item = session.Get<T>(key);
            if (item == null) item = new T(); // TODO: decide if this should be default or if consumer should be responsible

            action.Invoke(item);

            session.Set(key, item);
            return item;
        }

        #endregion


        #region IQueryCollection Extensions

        public static T GetValue<T>(this IQueryCollection query, string key)
        {
            query.TryGetValue(key, out var values);
            if (values.Count == 0) return default;
            var item = Convert.ChangeType(values.First(), typeof(T));
            return item.Equals(null) ? default : (T)item;
        }

        public static IEnumerable<T> GetValues<T>(this IQueryCollection query, string key)
        {
            query.TryGetValue(key, out var values);

            foreach(var value in values)
            {
                var item = Convert.ChangeType(values.First(), typeof(T));
                yield return item.Equals(null) ? default : (T)item;
            }
        }

        #endregion

        //#region Controller Extensions

        //public static ContentResult ContentAsHtml(this Controller controller, string content, Encoding contentEncoding = null)
        //{
        //    return controller.Content(content, "text/html", contentEncoding ?? Encoding.UTF8);
        //}

        //#endregion

        //public static bool IsValid(this PageScope pageScope)
        //{
        //    if (pageScope == null) return false;

        //    return (pageScope.BankAccountId > 0)
        //        && (pageScope.CurrentPeriodId > 200000)
        //        && (pageScope.PeriodId > 200000);
        //}

    }

}
