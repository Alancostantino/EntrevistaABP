using System.Threading.Tasks;
using Volo.Abp.Authorization.Permissions;   
using Volo.Abp.Data;                        
using Volo.Abp.DependencyInjection;        
using Volo.Abp.Guids;                      
using Volo.Abp.Identity;                    
using Volo.Abp.PermissionManagement;        
using Volo.Abp.Uow;                         
using WB.EntrevistaABP.Permissions;
using Microsoft.AspNetCore.Identity;        

namespace WB.EntrevistaABP
{


    // Crea roles "admin" y "client" si no existen.
    // Concede permisos de Viajes a cada rol
  
    public class DataSeedContributor : IDataSeedContributor, ITransientDependency
    {
        private readonly IdentityRoleManager _roleManager;   // Crear/consultar roles
        private readonly IPermissionManager _permissionMgr;  // Grabar grants (rol → permiso) en BD
        private readonly IGuidGenerator _guid;               // Genera GUIDs según convención ABP

        public DataSeedContributor(
            IdentityRoleManager roleManager,
            IPermissionManager permissionManager,
            IGuidGenerator guid)
        {
            _roleManager = roleManager;
            _permissionMgr = permissionManager;
            _guid = guid;
        }

        

       
        [UnitOfWork]
        public async Task SeedAsync(DataSeedContext context)
        {
            // Crear roles si no existen
            var adminRole  = await _roleManager.FindByNameAsync("admin");
            if (adminRole == null)
            {
                adminRole = new IdentityRole(_guid.Create(), "admin");
                (await _roleManager.CreateAsync(adminRole)).CheckErrors();
            }

            var clientRole = await _roleManager.FindByNameAsync("client");
            if (clientRole == null)
            {
                clientRole = new IdentityRole(_guid.Create(), "client");
                (await _roleManager.CreateAsync(clientRole)).CheckErrors();
            }

            // Conceder permisos a roles (tabla AbpPermissionGrants)
            //    Admin: todos los permisos de viajes
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
                    RolePermissionValueProvider.ProviderName, // proveedor "por rol"
                    adminRole.Name,                           // clave = nombre del rol
                    perm,                                     // permiso a conceder
                    true                                      // conceder = true
                );
            }

            //    Client: solo ver/listar (Default)
            await _permissionMgr.SetAsync(
                RolePermissionValueProvider.ProviderName,
                clientRole.Name,
                EntrevistaABPPermissions.Viajes.Default,
                true
            );
        }
    }
}