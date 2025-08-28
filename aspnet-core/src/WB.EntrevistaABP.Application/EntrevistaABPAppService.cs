using System;
using System.Collections.Generic;
using System.Text;
using WB.EntrevistaABP.Localization;
using Volo.Abp.Application.Services;

namespace WB.EntrevistaABP;

/* Inherit your application services from this class.
 */
public abstract class EntrevistaABPAppService : ApplicationService
{
    protected EntrevistaABPAppService()
    {
        LocalizationResource = typeof(EntrevistaABPResource);
    }
}
