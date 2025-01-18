using System.Data.Entity;
using System.Threading.Tasks;
using System.Net;
using System.Web.Mvc;
using SharedLayer.AB_Common;
using SharedLayer.Models.Inventory;
using DataAccessLayer;

namespace MMS.Controllers
{
    public class tblCompanyController : Controller
    {
        private readonly ItblCompanyRepository _companyRepository;

        public tblCompanyController(ItblCompanyRepository userRepository)
        {
            _companyRepository = userRepository;
        }
        // GET: tblCompany
        public async Task<ActionResult> Index()
        {
            var company = await _companyRepository.GetTblCompanies();
            return View(company);            
        }        
    }
}
