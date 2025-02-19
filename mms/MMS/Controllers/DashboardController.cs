using BusinessLayer;
using SharedLayer.Models;
using System;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace MMS.Controllers
{
    //[SessionTimeout]
    //[SingleDeviceLogin]
    public class DashboardController : Controller
    {
        private readonly IMstMenuDefWebService _menuRepository;

        public DashboardController(IMstMenuDefWebService menuRepository)
        {
            _menuRepository = menuRepository;
        }

        public ActionResult ShowMenus()
        {
            try
            {
                var menuHierarchy = CommonSub.ShowMainMenu();
                return PartialView("ShowMenu", menuHierarchy);
            }
            catch (Exception ex)
            {
                Exception inner = ex.InnerException ?? ex;
                while (inner.InnerException != null)
                {
                    inner = inner.InnerException;
                }
                Log.Error(Session["Username"].ToString() + " " + inner.Message);
                throw;
            }
        }

    }
}
