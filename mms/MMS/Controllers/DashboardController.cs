using DataAccessLayer;
using SharedLayer.AB_Common;
using SharedLayer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using static MMS.FilterConfig;

namespace MMS.Controllers
{
    [SessionTimeout]
    [SingleDeviceLogin]
    public class DashboardController : Controller
    {
        private readonly IMstMenuDefWebRepository _menuRepository;

        public DashboardController(IMstMenuDefWebRepository menuRepository)
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
