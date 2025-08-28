using System.Threading.Tasks;

namespace WB.EntrevistaABP.Data;

public interface IEntrevistaABPDbSchemaMigrator
{
    Task MigrateAsync();
}
