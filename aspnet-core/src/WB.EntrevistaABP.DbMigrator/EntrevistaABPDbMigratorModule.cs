using WB.EntrevistaABP.EntityFrameworkCore;
using Volo.Abp.Autofac;
using Volo.Abp.Modularity;

namespace WB.EntrevistaABP.DbMigrator;

[DependsOn(
    typeof(AbpAutofacModule),
    typeof(EntrevistaABPEntityFrameworkCoreModule),
    typeof(EntrevistaABPApplicationContractsModule)
    )]
public class EntrevistaABPDbMigratorModule : AbpModule
{
}
