using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;


namespace SmartCut.Shared.Resources.Localization
{
    public static class Localizer
    {
        public static string Get(string key)
       => AppResource.ResourceManager.GetString(key, CultureInfo.CurrentUICulture);
        public static void SetCulture(string cultureCode)
        {
            var culture = new CultureInfo(cultureCode);
            CultureInfo.CurrentCulture = culture;
            CultureInfo.CurrentUICulture = culture;
        }
    }

}
