using Volo.Abp.Account;
using Volo.Abp.FeatureManagement;
using Volo.Abp.Identity;
using Volo.Abp.Modularity;
using Volo.Abp.ObjectExtending;
using Volo.Abp.PermissionManagement;
using Volo.Abp.SettingManagement;
using Volo.Abp.TenantManagement;
using Volo.Abp.Authorization.Permissions;

namespace WB.EntrevistaABP;

[DependsOn(
    typeof(EntrevistaABPDomainSharedModule),
    typeof(AbpAccountApplicationContractsModule),
    typeof(AbpFeatureManagementApplicationContractsModule),
    typeof(AbpIdentityApplicationContractsModule),
    typeof(AbpPermissionManagementApplicationContractsModule),
    typeof(AbpSettingManagementApplicationContractsModule),
    typeof(AbpTenantManagementApplicationContractsModule),
    typeof(AbpObjectExtendingModule)
)]
public class EntrevistaABPApplicationContractsModule : AbpModule
{
    public override void PreConfigureServices(ServiceConfigurationContext context)
    {
        EntrevistaABPDtoExtensions.Configure();
    } 

//Agregar permissionProvider al modulo 
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<AbpPermissionOptions>(options =>
        {
            var providerType = typeof(WB.EntrevistaABP.Permissions.EntrevistaABPPermissionDefinitionProvider);
             if (!options.DefinitionProviders.Contains(providerType))
            {
                options.DefinitionProviders.Add(providerType);
            }
        });
    }
}
