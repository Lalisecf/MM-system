using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using Unity;

namespace MMS
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            var container = new UnityContainer();
            container.AddExtension(new Diagnostic());
            log4net.Config.XmlConfigurator.Configure();
        }
    }
}
