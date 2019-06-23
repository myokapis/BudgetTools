using AutoMapper;
using Ninject.Activation;
using Ninject.Modules;
using DAL = BudgetToolsDAL.Models;
using DM = BudgetTools.Models.DomainModels;
using VM = BudgetTools.Models.ViewModels;

namespace BudgetTools.AutoMapper
{

    //public class AutoMapperModule : NinjectModule
    //{
    //    public override void Load()
    //    {
    //        Bind<IValueResolver<SourceEntity, DestModel, bool>>().To<MyResolver>();

    //        var mapperConfiguration = CreateConfiguration();
    //        Bind<MapperConfiguration>().ToConstant(mapperConfiguration).InSingletonScope();

    //        // This teaches Ninject how to create automapper instances say if for instance
    //        // MyResolver has a constructor with a parameter that needs to be injected
    //        Bind<IMapper>().ToMethod(ctx =>
    //             new Mapper(mapperConfiguration, type => ctx.Kernel.Get(type)));
    //    }

    //    private MapperConfiguration CreateConfiguration()
    //    {
    //        var config = new MapperConfiguration(cfg =>
    //        {
    //            // Add all profiles in current assembly
    //            cfg.AddProfiles(GetType().Assembly);
    //        });

    //        return config;
    //    }
    //}

    //public class AutoMapperConfig
    //{
    //    public static IMapper GetMapper()
    //    {
    //        return GetMapperConfig().CreateMapper();
    //    }

    //    protected static MapperConfiguration GetMapperConfig()
    //    {

    //        return new MapperConfiguration(cfg => {
    //            // cfg.ConstructServicesUsing(type => context.Kernel.GetType());

    //            // TODO: look into profiles
    //            //cfg.AddProfile<AppProfile>();

    //            // TODO: add map configs
    //            //cfg.CreateMap<DAL., Dest>();
    //        });

    //    }

    //}

}