using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Caching;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using System.Data.Entity;
using AutoMapper;
using BudgetTools.AutoMapper;
using BudgetTools.Classes;
using BudgetToolsDAL.Contexts;
using BudgetToolsDAL.Helpers;
using BudgetTools.Models.DomainModels;
using BudgetToolsDAL.Accessors;
using BudgetToolsBLL.Services;
using BudgetTools.Presenters;
using Ninject;
using Ninject.Web.Common.WebHost;
using TemplateEngine;

// TODO: move this out when mapper config gets moved out
using DALModels = BudgetToolsDAL.Models;
using BLLModels = BudgetToolsBLL.Models;

namespace BudgetTools
{

    public class MvcApplication : NinjectHttpApplication
    {

        protected override void OnApplicationStarted()
        {
            base.OnApplicationStarted();

            // the second call is probably redundant
            DataHelpers.DisableInitializers();
            Database.SetInitializer<BudgetToolsDAL.Contexts.BudgetToolsDBContext>(null);

            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            
            //ValueProviderFactories.Factories.Add(new JsonValueProviderFactory());
            //Template.TemplatePath = Server.MapPath(@"./Templates");
        }

        protected void Session_Start(Object sender, EventArgs e)
        {
            var pageScope = new PageScope();
            Session.Contents["pageScope"] = pageScope;
        }

        protected override IKernel CreateKernel()
        {
            var kernel = new StandardKernel();
            RegisterServices(kernel);
            return kernel;
        }

        private void RegisterServices(IKernel kernel)
        {

            // TODO: move the mapping to the MapperConfig.cs file
            Mapper.Initialize(cfg => {
                // TODO: move mapping to profiles
                //cfg.AddProfile<AppProfile>();
                cfg.CreateMissingTypeMaps = true;
                cfg.CreateMap<BLLModels.StagedTransaction, DALModels.StagedTransaction>();
                cfg.CreateMap<DALModels.Period, Option>()
                    .ForMember(o => o.Text, c => c.MapFrom(s => s.PeriodId.ToString()))
                    .ForMember(o => o.Value, c => c.MapFrom(s => s.PeriodId.ToString()));
                cfg.CreateMap<DALModels.BankAccount, Option>()
                    .ForMember(o => o.Text, c => c.MapFrom(s => s.BankAccountName))
                    .ForMember(o => o.Value, c => c.MapFrom(s => s.BankAccountId.ToString()));
                cfg.CreateMap<DALModels.BudgetLineSet, Option>()
                    .ForMember(o => o.Text, c => c.MapFrom(d => d.DisplayName))
                    .ForMember(o => o.Value, c => c.MapFrom(d => d.BudgetLineId.ToString()));
                cfg.CreateMap<DALModels.Transaction, Transaction>();
                cfg.CreateMap<MappedTransaction, DALModels.MappedTransaction>()
                    .ForMember(m => m.BudgetLine, x => x.Ignore());
            });

            // TODO: be sure to add unit tests to check the mappings

            kernel.Bind<IBudgetPresenter>().To<BudgetPresenter>();
            kernel.Bind<ITransactionsPresenter>().To<TransactionsPresenter>();
            kernel.Bind<IBudgetToolsAccessor>().To<BudgetToolsAccessor>();
            kernel.Bind<IBudgetService>().To<BudgetService>();
            kernel.Bind<IBudgetToolsDBContext>().To<BudgetToolsDBContext>()
                .WithConstructorArgument("nameOrConnectionString", "DefaultConnection");
            kernel.Bind<IPageScope>().ToMethod(m => (PageScope)HttpContext.Current.Session["pageScope"]);
            kernel.Bind<IImportPresenter>().To<ImportPresenter>();
            kernel.Bind<ITemplateLoader>().To<TemplateLoader>().InSingletonScope()
                .WithConstructorArgument("templateDirectory", Server.MapPath(@"./Templates"));
            kernel.Bind<ITemplateCache>().To<TemplateCache>().InSingletonScope();
            kernel.Bind<IWebCache>().To<WebCache>().InSingletonScope()
                .WithConstructorArgument("cache", HttpRuntime.Cache);
        }

        private void LoadModules(IKernel kernel)
        {

        }

    }

}
