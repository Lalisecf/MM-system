using System;
using System.Linq;
using System.Web.Mvc;
using System.Web.Routing;
using SharedLayer.AB_Common;
using SharedLayer.Models;

namespace MMS.Filters
{
    public class AuthorizeUserAttribute : ActionFilterAttribute, IAuthorizationFilter
    {

        public void OnAuthorization(AuthorizationContext filterContext)
        {
            try
            {
                MasterDataContext context = new MasterDataContext();

                var role = Convert.ToString(filterContext.HttpContext.Session["Role"]);

                if (!string.IsNullOrEmpty(role))
                {
                    var roleValue = Convert.ToInt32(role);

                    var roleMasterDetails = (from rolemaster in context.MstRoles 
                                             where rolemaster.RoleID == roleValue
                                             select rolemaster).FirstOrDefault();

                    if (roleMasterDetails != null && !(string.Equals(roleMasterDetails.RoleName.ToLower(), "maker")))
                    {
                        filterContext.HttpContext.Session.Abandon();

                        filterContext.Result = new RedirectToRouteResult
                        (
                            new RouteValueDictionary
                                (new
                                { controller = "Error", action = "Error" }
                            ));
                    }
                }
                else
                {
                    filterContext.HttpContext.Session.Abandon();

                    filterContext.Result = new RedirectToRouteResult
                    (
                        new RouteValueDictionary
                        (new
                        { controller = "Error", action = "Error" }
                        ));
                }
            }
            catch (Exception ex)
            {
                Exception inner = ex.InnerException ?? ex;
                while (inner.InnerException != null)
                {
                    inner = inner.InnerException;
                }
               Log.Error(inner.Message);
                throw;
            }
        }
    }
}