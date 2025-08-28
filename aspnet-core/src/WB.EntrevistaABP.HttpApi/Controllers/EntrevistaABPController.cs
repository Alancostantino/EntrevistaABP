using WB.EntrevistaABP.Localization;
using Volo.Abp.AspNetCore.Mvc;

namespace WB.EntrevistaABP.Controllers;

/* Inherit your controllers from this class.
 */
public abstract class EntrevistaABPController : AbpControllerBase
{
    protected EntrevistaABPController()
    {
        LocalizationResource = typeof(EntrevistaABPResource);
    }
}
