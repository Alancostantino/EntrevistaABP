using WB.EntrevistaABP.Localization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;

namespace WB.EntrevistaABP.Permissions;

public class EntrevistaABPPermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        // Evita duplicados si otro provider ya creó el grupo
        var group = context.GetGroupOrNull(EntrevistaABPPermissions.GroupName)
                   ?? context.AddGroup(EntrevistaABPPermissions.GroupName, L("Permission:EntrevistaABP"));

        // Cuelga los permisos como árbol, con display names localizables
        var viajes = group.AddPermission(EntrevistaABPPermissions.Viajes.Default, L("Permission:Viajes"));
        viajes.AddChild(EntrevistaABPPermissions.Viajes.Create, L("Permission:Create"));
        viajes.AddChild(EntrevistaABPPermissions.Viajes.Update, L("Permission:Update"));
        viajes.AddChild(EntrevistaABPPermissions.Viajes.Delete, L("Permission:Delete"));
        viajes.AddChild(EntrevistaABPPermissions.Viajes.ManagePassengers, L("Permission:ManagePassengers"));
    }

    private static LocalizableString L(string name)
        => LocalizableString.Create<EntrevistaABPResource>(name);
}