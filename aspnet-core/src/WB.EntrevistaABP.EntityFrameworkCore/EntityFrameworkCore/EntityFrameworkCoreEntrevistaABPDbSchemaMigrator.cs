using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using WB.EntrevistaABP.Data;
using Volo.Abp.DependencyInjection;

namespace WB.EntrevistaABP.EntityFrameworkCore;

public class EntityFrameworkCoreEntrevistaABPDbSchemaMigrator
    : IEntrevistaABPDbSchemaMigrator, ITransientDependency
{
    private readonly IServiceProvider _serviceProvider;

    public EntityFrameworkCoreEntrevistaABPDbSchemaMigrator(
        IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task MigrateAsync()
    {
        /* We intentionally resolving the EntrevistaABPDbContext
         * from IServiceProvider (instead of directly injecting it)
         * to properly get the connection string of the current tenant in the
         * current scope.
         */

        await _serviceProvider
            .GetRequiredService<EntrevistaABPDbContext>()
            .Database
            .MigrateAsync();
    }
}
