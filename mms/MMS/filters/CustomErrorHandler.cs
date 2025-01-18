using System;
using System.Web.Mvc;
using System.Web.Routing;

public class CustomErrorHandler : HandleErrorAttribute
{
    public override void OnException(ExceptionContext filterContext)
    {
        // Capture the exception
        Exception ex = filterContext.Exception;

        // Prevent the default error handling
        filterContext.ExceptionHandled = true;

        // Log the exception details
        Exception innerException = ex;
        while (innerException.InnerException != null)
        {
            innerException = innerException.InnerException;
        }

        // Assuming you have a logging setup, e.g., Log.Error(innerException.Message);
        System.Diagnostics.Debug.WriteLine(innerException.Message); // Use actual logging

        // Redirect to a custom error page
        filterContext.Result = new RedirectToRouteResult(new RouteValueDictionary
        {
            { "controller", "Error" },
            { "action", "Error" }
        });
    }
}
