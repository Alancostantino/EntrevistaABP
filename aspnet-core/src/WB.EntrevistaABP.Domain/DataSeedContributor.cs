using System.Threading.Tasks;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Guids;
using Volo.Abp.Identity;
using Volo.Abp.PermissionManagement;
using Volo.Abp.Uow;
using Volo.Abp.MultiTenancy;
using Microsoft.AspNetCore.Identity;
using WB.EntrevistaABP.Permissions; // ⬅️ para ICurrentTenant

namespace WB.EntrevistaABP
{
    public class DataSeedContributor : IDataSeedContributor, ITransientDependency
    {
        private readonly IdentityRoleManager _roleManager;
        private readonly IPermissionManager _permissionMgr;
        private readonly IGuidGenerator _guid;
        private readonly ICurrentTenant _currentTenant; // ⬅️ inyectá el tenant actual

        public DataSeedContributor(
            IdentityRoleManager roleManager,
            IPermissionManager permissionManager,
            IGuidGenerator guid,
            ICurrentTenant currentTenant)
        {
            _roleManager = roleManager;
            _permissionMgr = permissionManager;
            _guid = guid;
            _currentTenant = currentTenant;
        }

        [UnitOfWork]
        public async Task SeedAsync(DataSeedContext context)
        {
            // aseguro que todo lo de abajo se ejecute en el tenant correcto
            using var change = _currentTenant.Change(context.TenantId);

            // crear roles con TenantId del contexto
            var adminRole = await _roleManager.FindByNameAsync("admin");
            if (adminRole == null)
            {
                adminRole = new IdentityRole(_guid.Create(), "admin", _currentTenant.Id);
                (await _roleManager.CreateAsync(adminRole)).CheckErrors();
            }

            var clientRole = await _roleManager.FindByNameAsync("client");
            if (clientRole == null)
            {
                clientRole = new IdentityRole(_guid.Create(), "client", _currentTenant.Id);
                (await _roleManager.CreateAsync(clientRole)).CheckErrors();
            }

            // permisos de Viajes
            var allViajesPerms = new[]
            {
                EntrevistaABPPermissions.Viajes.Default,
                EntrevistaABPPermissions.Viajes.Create,
                EntrevistaABPPermissions.Viajes.Update,
                EntrevistaABPPermissions.Viajes.Delete,
                EntrevistaABPPermissions.Viajes.ManagePassengers
            };

            foreach (var perm in allViajesPerms)
            {
                await _permissionMgr.SetAsync(
                    RolePermissionValueProvider.ProviderName,
                    adminRole.Name, // "admin"
                    perm,
                    true
                );
            }

            await _permissionMgr.SetAsync(
                RolePermissionValueProvider.ProviderName,
                clientRole.Name, // "client"
                EntrevistaABPPermissions.Viajes.Default,
                true
            );
        }
    }
}