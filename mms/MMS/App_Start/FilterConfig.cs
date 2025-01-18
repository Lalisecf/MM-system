using MMS.Filters;
using SharedLayer.AB_Common;
using System;
using System.Linq;
using System.Web.Mvc;

namespace MMS
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new CustomErrorHandler());
        }
        public class SessionTimeoutAttribute : ActionFilterAttribute
        {
            public override void OnActionExecuting(ActionExecutingContext filterContext)
            {
                var session = Convert.ToString(filterContext.HttpContext.Session["UserID"]);
                if (session == null)
                {
                    filterContext.Result = new RedirectResult("~/Login/Login");
                    return;
                }

                base.OnActionExecuting(filterContext);
            }
        }
        [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = true, AllowMultiple = true)]
        public class SingleDeviceLoginAttribute : ActionFilterAttribute
        {
            public override void OnActionExecuting(ActionExecutingContext filterContext)
            {         
                MasterDataContext db = new MasterDataContext();
                var role = Convert.ToString(filterContext.HttpContext.Session["SessionID"]);

                if (!string.IsNullOrEmpty(role))
                {
                    var roleMasterDetails = db.MstUsers.Where(c => c.SessionID == new Guid(role)).FirstOrDefault();
                    if (roleMasterDetails == null)
                    {
                        filterContext.Result = new RedirectResult("~/Login/Login");
                        return;  
                    }
                }
                base.OnActionExecuting(filterContext);
            }

        }

       
    }
}
