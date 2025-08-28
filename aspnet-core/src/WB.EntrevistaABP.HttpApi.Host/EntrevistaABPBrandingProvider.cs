using Volo.Abp.DependencyInjection;
using Volo.Abp.Ui.Branding;

namespace WB.EntrevistaABP;

[Dependency(ReplaceServices = true)]
public class EntrevistaABPBrandingProvider : DefaultBrandingProvider
{
    public override string AppName => "EntrevistaABP";
}
