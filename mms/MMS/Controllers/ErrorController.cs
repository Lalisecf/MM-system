using System;
using System.Web;
using System.Web.Mvc;

namespace MMS.Controllers
{

    public class ErrorController : Controller
    {
        // GET: Error
        public ActionResult Error2()  
        {
            Response.Cache.SetCacheability(HttpCacheability.NoCache);
            Response.Cache.SetExpires(DateTime.Now.AddSeconds(-1));
            Response.Cache.SetNoStore();

            HttpCookie Cookies = new HttpCookie("WebTime");
            Cookies.Value = "";
            Cookies.Expires = DateTime.Now.AddHours(-1);
            Response.Cookies.Add(Cookies);
            HttpContext.Session.Clear();
            Session.Abandon();
            return RedirectToAction("Login", "Login");
        }
        public ActionResult Error() 
        {
            Response.Cache.SetCacheability(HttpCacheability.NoCache);
            Response.Cache.SetExpires(DateTime.Now.AddSeconds(-1));
            Response.Cache.SetNoStore();

            HttpCookie Cookies = new HttpCookie("WebTime");
            Cookies.Value = "";
            Cookies.Expires = DateTime.Now.AddHours(-1);
            Response.Cookies.Add(Cookies);
            HttpContext.Session.Clear();
            Session.Abandon();
            return View();
        }
    }
}