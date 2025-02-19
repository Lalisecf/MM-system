using BusinessLayer.Interfaces;
using SharedLayer.Models.Inventory;
using System.Collections.Generic;
using System.Web.Mvc;
using System.Linq;


public class PeriodController : Controller
{
    private readonly IPeriodService _periodService;

    public PeriodController(IPeriodService periodService)
    {
        _periodService = periodService;
    }

    // GET: Period
    public ActionResult Index()
    {
        var fiscalYears = _periodService.GetFiscalYears(); 
        var firstFiscalYear = fiscalYears.FirstOrDefault(); 

        int selectedFiscalYear = 0;
        if (!string.IsNullOrEmpty(firstFiscalYear))
        {
            selectedFiscalYear = int.Parse(firstFiscalYear); 
        }

        var viewModel = new PeriodViewModel
        {
            FiscalYears = new SelectList(fiscalYears),
            Periods = new List<tblPeriodLu>(), 
            SelectedFiscalYear = selectedFiscalYear 
        };

        return View(viewModel);
    }


    // This method is for binding the grid view data based on the selected fiscal year
    [HttpPost]
    public JsonResult GetPeriods(int fiscalYear)
    {
        var periods = _periodService.GetPeriods(fiscalYear);
        return Json(periods, JsonRequestBehavior.AllowGet);
    }
}
