using System.Threading.Tasks;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;

namespace WB.EntrevistaABP
{
    internal class DataSeedContributor : IDataSeedContributor, ITransientDependency
    {
        public DataSeedContributor()
        {
        }
        
        public async Task SeedAsync(DataSeedContext context)
        {
            // Datos a rellenar en backend
            // Pista: Aca se pueden crear usuarios, roles, asignar permisos, etc.
        }
    }
}