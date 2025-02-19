using System.Web.Mvc;
using SharedLayer.Models.Inventory;
using System;
using BusinessLayer.Interfaces;

namespace MMS.Controllers
{
    public class CompanyController : Controller
    {
        private readonly ICompanyService _companyService;
        public CompanyController(ICompanyService companyService)
        {
            _companyService = companyService;
        }

        // GET: Company
        public ActionResult Index()
        {
            var company = _companyService.GetCompany();
            return View(company);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Save(tblCompany company)
        {
            try
            {
                if (company == null) return View("Error");

                _companyService.SaveCompany(company);

                TempData["Success"] = "Company Profile updated successfully!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error: " + ex.Message;
                return View("Error");
            }
        }
    }
}
