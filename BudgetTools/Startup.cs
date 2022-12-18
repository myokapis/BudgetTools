using System.IO;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TemplateEngine;
using LazyCache;
using AutoMapper;
//using AutoMapper.Extensions.Microsoft.DependencyInjection;
using BudgetTools.Classes;
using BudgetToolsDAL.Contexts;
using BudgetTools.Models;
using BudgetToolsDAL.Accessors;
using BudgetToolsBLL.Services;
using BudgetTools.Presenters;
using System;
using Microsoft.AspNetCore.Http;

//[assembly: OwinStartupAttribute(typeof(BudgetTools.Startup))]
namespace BudgetTools
{

    public partial class Startup
    {
        public IConfiguration Configuration { get; }
        public IWebHostEnvironment WebHostEnvironment { get; }

        public Startup(IConfiguration configuration, IWebHostEnvironment env)
        {
            Configuration = configuration;
            WebHostEnvironment = env;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            // setup database contexts
            services.AddDbContextPool<BudgetToolsDBContext>(options =>
                {
                    options.UseSqlServer(Configuration.GetConnectionString("Default"));
                    options.EnableDetailedErrors();
                }); // TODO: make this match the config

            

            //// setup authentication/authorization
            //services.AddDefaultIdentity<IdentityUser>(options =>
            //{
            //    options.SignIn.RequireConfirmedAccount = false;
            //    options.SignIn.RequireConfirmedEmail = false;
            //    options.SignIn.RequireConfirmedPhoneNumber = false;
            //})
            //.AddRoles<IdentityRole>()
            //.AddEntityFrameworkStores<AuthDbContext>()
            //.AddDefaultTokenProviders();

            //services.AddAuthorization(options => options.AddPolicy("RequireAdminRole", policy => policy.RequireRole("Admin")));
            //services.ConfigureApplicationCookie(opts =>
            //{
            //    opts.LoginPath = "/Identity/Login";
            //    opts.AccessDeniedPath = "/Identity/Login/AccessDenied";
            //});

            // setup controllers and pages
            services.AddControllersWithViews();

            // TODO: check the correct implementation of this
            services.AddAntiforgery();
            services.Configure<IISServerOptions>(options => options.MaxRequestBodySize = null);

            // setup templates
            var templateDirectory = Path.Join(WebHostEnvironment.ContentRootPath, "Templates");
            services.AddSingleton<ITemplateLoader>(new TemplateLoader(templateDirectory));
            services.AddSingleton<IAppCache>(new CachingService());
            services.AddSingleton<ITemplateCache>((sp) => new TemplateCache(sp.GetService<IAppCache>(), sp.GetService<ITemplateLoader>()));

            // setup presenters
            services.AddScoped<IBudgetPresenter, BudgetPresenter>();
            services.AddScoped<ITransactionsPresenter, TransactionsPresenter>();
            services.AddScoped<IBalancesPresenter, BalancesPresenter>();
            services.AddScoped<IAdminPresenter, AdminPresenter>();
            services.AddScoped<IImportPresenter, ImportPresenter>();

            // TODO: add startup helpers in BLL and DAL to set up dependency injection

            // setup data services
            services.AddScoped<IBudgetToolsDBContext, BudgetToolsDBContext>();
            services.AddScoped<IBudgetService, BudgetService>();
            services.AddScoped<IBudgetToolsAccessor, BudgetToolsAccessor>();
            services.AddSingleton<IImportService, ImportService>();

            // setup misc
            services.AddSingleton<ITemplateCache, TemplateCache>();
            services.AddLazyCache();
            services.AddAutoMapper(config => config.AddProfile(new AutoMapProfile()));
            //services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
            services.AddSingleton<ParserFactory>();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                //app.UseDatabaseErrorPage();
            }
            else
            {
                // TODO: setup the error page the way i want it
                app.UseExceptionHandler("/Home/Error");
                // TODO: check correct implementation of this
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();

            //app.UseAuthentication();
            //app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Budget}/{action=Index}/{id?}");
            });
        }

    }

}
