using BusinessLayer.Interfaces;
using SharedLayer.Models.Inventory;
using System;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace MMS.Controllers
{
    public class BaseController : Controller 
    {
        private readonly ILogHistoryService _logHistoryService;        
        public BaseController(ILogHistoryService logHistoryService)
        {
            _logHistoryService = logHistoryService;
        }


        protected async Task LogActionAsync( string referenceId, string  actionMethod, string Details, string createdBy)
        {
            var logHistory = new LogHistory
            {
                referenceId = referenceId,
                actionMethod =actionMethod,
                Details = Details, 
                CrtBy = createdBy,
                CrtDt = DateTime.UtcNow
            };

            await _logHistoryService.LogActionAsync(logHistory);
        }
    }
}
