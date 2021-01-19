using AutoMapper;
using AutoMapper.Features;
using AutoMapper.Mappers;
using BudgetTools.Models;
using TemplateEngine;

using DALModels = BudgetToolsDAL.Models;
using BLLModels = BudgetToolsBLL.Models;

namespace BudgetTools.Classes
{

    public class AutoMapProfile : Profile
    {

        public AutoMapProfile()
        {

            CreateMap<BLLModels.StagedTransaction, DALModels.StagedTransaction>();

            CreateMap<DALModels.Period, Option>()
                .ForMember(o => o.Text, c => c.MapFrom(s => s.PeriodId.ToString()))
                .ForMember(o => o.Value, c => c.MapFrom(s => s.PeriodId.ToString()));

            CreateMap<DALModels.BankAccount, Option>()
                .ForMember(o => o.Text, c => c.MapFrom(s => s.BankAccountName))
                .ForMember(o => o.Value, c => c.MapFrom(s => s.BankAccountId.ToString()));

            CreateMap<DALModels.BudgetLineSet, Option>()
                .ForMember(o => o.Text, c => c.MapFrom(d => d.DisplayName))
                .ForMember(o => o.Value, c => c.MapFrom(d => d.BudgetLineId.ToString()));

            CreateMap<DALModels.Transaction, Transaction>();

            CreateMap<MappedTransaction, DALModels.MappedTransaction>()
                .ForMember(m => m.BudgetLine, x => x.Ignore())
                .ReverseMap();

            CreateMap<BLLModels.PeriodBalance, PeriodBalance>();
                //.ForSourceMember(s => s.BudgetGroupName, x => x.Ignore());

            CreateMap<DALModels.BudgetLine, BudgetLineBalance>()
                .ForMember(o => o.BudgetLineName, c => c.MapFrom(d => d.DisplayName))
                .ForMember(o => o.IsSource, c => c.MapFrom(d => d.Balance > 0m || d.BudgetGroupName == "Assets"));

            CreateMap<DALModels.Message, Option>()
                .ForMember(o => o.Text, c => c.MapFrom(d => d.MessageText))
                .ForMember(o => o.Value, c => c.MapFrom(d => d.ErrorLevel));

            CreateMap<DALModels.Message, Message>()
                .ForMember(o => o.Text, c => c.MapFrom(d => d.MessageText))
                .ForMember(o => o.Value, c => c.MapFrom(d => d.ErrorLevel));

            CreateMap<BLLModels.DataScope, PageScope>().ReverseMap();

            CreateMap<DALModels.PeriodBudgetLine, BLLModels.PeriodBudgetLine>()
                .ForMember(b => b.IsDetail, ex => ex.MapFrom(v => true));

            CreateMap<BLLModels.PeriodBudgetLine, PeriodBudgetLine>();
            //.ForMember(m => m.IsDetail, ex => ex.MapFrom(v => true));

            CreateMap<DALModels.BankAccount, BankAccount>();

            CreateMap<DALModels.PeriodBalance, BLLModels.PeriodBalance>()
                .ForMember(d => d.Level, s => s.MapFrom(x => 2));

            CreateMap<BLLModels.PeriodBalance, PeriodBalance>();

            CreateMap<BLLModels.MappedTransaction, MappedTransaction>()
                .ForMember(b => b.MappedTransactionId, x => x.Ignore())
                .ReverseMap();
        }

    }

}
