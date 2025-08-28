using WB.EntrevistaABP.EntityFrameworkCore;
using Volo.Abp.Modularity;

namespace WB.EntrevistaABP;

[DependsOn(
    typeof(EntrevistaABPEntityFrameworkCoreTestModule)
    )]
public class EntrevistaABPDomainTestModule : AbpModule
{

}
