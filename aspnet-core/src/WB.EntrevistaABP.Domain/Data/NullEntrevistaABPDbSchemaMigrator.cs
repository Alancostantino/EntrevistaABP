using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;

namespace WB.EntrevistaABP.Data;

/* This is used if database provider does't define
 * IEntrevistaABPDbSchemaMigrator implementation.
 */
public class NullEntrevistaABPDbSchemaMigrator : IEntrevistaABPDbSchemaMigrator, ITransientDependency
{
    public Task MigrateAsync()
    {
        return Task.CompletedTask;
    }
}
