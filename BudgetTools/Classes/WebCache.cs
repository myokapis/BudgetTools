using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Caching;
using TemplateEngine;
using BudgetToolsBLL.Services;

namespace BudgetTools.Classes
{

    public interface IWebCache
    {
        FieldDefinitions BudgetLineDefinitions();
    }

    public sealed class WebCache : IWebCache
    {
        private IBudgetService budgetService;
        private Cache cache;
        private int defaultExpirationMinutes = 5;

        public WebCache(Cache cache, IBudgetService budgetService)
        {
            this.budgetService = budgetService;
            this.cache = cache;
        }

        public FieldDefinitions BudgetLineDefinitions()
        {

            // lookup the cached item
            var key = "budgetLineDefinitions";
            var definitions = this.cache[key] as FieldDefinitions;

            if (definitions == null)
            {
                // get the current active budget lines
                var budgetLines = new List<Option>() { new Option() { Text = "", Value = "0" } };
                budgetLines.AddRange(budgetService.GetBudgetLineSet<Option>().OrderBy(o => o.Text));

                // set up the dropdown definition
                var dropdown = new DropdownDefinition("BUDGET_LINES", "BudgetLineId", budgetLines);

                definitions = new FieldDefinitions();
                definitions.SetDropdowns(dropdown);

                // cache the created item
                var expiration = DateTime.Now.AddMinutes(defaultExpirationMinutes);
                cache.Insert(key, definitions, null, expiration, Cache.NoSlidingExpiration);
            }

            return definitions;
            
        }

    }

}