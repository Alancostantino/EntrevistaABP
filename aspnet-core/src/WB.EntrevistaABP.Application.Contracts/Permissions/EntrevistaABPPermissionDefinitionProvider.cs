using WB.EntrevistaABP.Localization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;

namespace WB.EntrevistaABP.Permissions;

public class EntrevistaABPPermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
       var group = context.AddGroup(EntrevistaABPPermissions.GroupName); //Registra el grupo de Permisos.

    //Agrega permisos y definirlo en el sistema (en memoria)
        group.AddPermission(EntrevistaABPPermissions.Viajes.Default);
        group.AddPermission(EntrevistaABPPermissions.Viajes.Create);
        group.AddPermission(EntrevistaABPPermissions.Viajes.Update);
        group.AddPermission(EntrevistaABPPermissions.Viajes.Delete);
        group.AddPermission(EntrevistaABPPermissions.Viajes.ManagePassengers);
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<EntrevistaABPResource>(name);
    }
}
