using System.Web.Mvc;

namespace MMS.Controllers
{
    public class DirectPostingController : Controller
    {
        // GET: DirectPosting        
        public ActionResult DirectPostingApproval()
        {
            return Redirect($"~/DirectPosting/frmDirectPostApproval.aspx");
        }
        public ActionResult DirectPosting()
        {
            return Redirect($"~/DirectPosting/frmPostToCore.aspx");
        }
    }
}