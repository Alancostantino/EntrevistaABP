using WB.EntrevistaABP.Localization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;

namespace WB.EntrevistaABP.Permissions;

public class EntrevistaABPPermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var myGroup = context.AddGroup(EntrevistaABPPermissions.GroupName);
        //Define your own permissions here. Example:
        //myGroup.AddPermission(EntrevistaABPPermissions.MyPermission1, L("Permission:MyPermission1"));
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<EntrevistaABPResource>(name);
    }
}
