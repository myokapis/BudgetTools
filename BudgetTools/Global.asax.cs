using System;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Data.Entity;
using AutoMapper;
using BudgetTools.Classes;
using BudgetToolsDAL.Contexts;
using BudgetToolsDAL.Helpers;
using BudgetTools.Models;
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
            Database.SetInitializer<BudgetToolsDBContext>(null);

            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
        }

        protected void Session_Start(Object sender, EventArgs e)
        {
            // kluge for now
            var budgetService = DependencyResolver.Current.GetService<IBudgetService>();
            var pageScope = new PageScope();
            budgetService.SetPageScope<PageScope>(ref pageScope);
            Session["pageScope"] = pageScope;
        }

        protected override IKernel CreateKernel()
        {
            var kernel = new StandardKernel();
            RegisterServices(kernel);
            return kernel;
        }

        private void RegisterServices(IKernel kernel)
        {
            // TODO: inject the mapper as an instance
            // TODO: move the mapping to the MapperConfig.cs file
            Mapper.Initialize(cfg => {
                // TODO: move mapping to profiles
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
                cfg.CreateMap<BLLModels.PeriodBalance, PeriodBalance>()
                    .ForSourceMember(s => s.BudgetGroupName, x => x.Ignore());
                cfg.CreateMap<DALModels.BudgetLine, BudgetLineBalance>()
                    .ForMember(o => o.BudgetLineName, c => c.MapFrom(d => d.DisplayName))
                    .ForMember(o => o.IsSource, c => c.MapFrom(d => d.Balance > 0m || d.BudgetGroupName == "Assets"));
                cfg.CreateMap<DALModels.Message, Option>()
                    .ForMember(o => o.Text, c => c.MapFrom(d => d.MessageText))
                    .ForMember(o => o.Value, c => c.MapFrom(d => d.ErrorLevel));
                cfg.CreateMap<DALModels.Message, Message>()
                .ForMember(o => o.Text, c => c.MapFrom(d => d.MessageText))
                .ForMember(o => o.Value, c => c.MapFrom(d => d.ErrorLevel));
            });

            // TODO: be sure to add unit tests to check the mappings

            kernel.Bind<IBudgetPresenter>().To<BudgetPresenter>();
            kernel.Bind<ITransactionsPresenter>().To<TransactionsPresenter>();
            kernel.Bind<IBalancesPresenter>().To<BalancesPresenter>();
            kernel.Bind<IAdminPresenter>().To<AdminPresenter>();
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
            kernel.Bind<IImportService>().To<ImportService>().InSingletonScope();

        }

        private void LoadModules(IKernel kernel)
        {

        }

    }

}
