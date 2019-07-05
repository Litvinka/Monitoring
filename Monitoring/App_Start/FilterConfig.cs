using System.Web;
using System.Web.Mvc;
using Monitoring.Filters;

namespace Monitoring
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }
    }
}
