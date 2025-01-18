using System.Web.Mvc;
using System.Web.Security;
using static MMS.FilterConfig;
namespace MMS.Controllers
{
    //[SessionTimeout]
    //[SingleDeviceLogin]
    public class HomeController : Controller
    {
        //IMstUserServvie _userRepository;
        //HomeService _HomeService = new HomeService();

        public ActionResult Indexk()
        {
            return View();
        }
        public ActionResult Home()
        {
            return View();
        }

        [HttpGet]
        public ActionResult Index()
        {
            if (Session["UserName"] != null && Session["UserID"] != null)
            {
                ViewBag.ActiveMenu = "Dashboard";
                return View();
            }
            else
            {
                return RedirectToAction("Login");//
            }

        }     
       
        public ActionResult SignOut()
        {
            FormsAuthentication.SignOut();
            Session.Abandon(); 
            return RedirectToAction("Login");
        }
    }
}
