using Volo.Abp.Modularity;

namespace WB.EntrevistaABP;

[DependsOn(
    typeof(EntrevistaABPApplicationModule),
    typeof(EntrevistaABPDomainTestModule)
    )]
public class EntrevistaABPApplicationTestModule : AbpModule
{

}
