using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using BusinessLayer.Interfaces;
using BusinessLayer.Service;
using BusinessLayer.Services;
using MMS.Models.Inventory;
using SharedLayer.AB_Common;

namespace MMS.Controllers
{
    public class FAController : BaseController
    {
        private IItemService _itemService;
        private IFAService _faService;
        public FAController(IItemService itemService, IFAService faService ,ILogHistoryService logService)
            : base(logService)
        {
            _itemService = itemService;
            _faService = faService;
        }
        public async Task<ActionResult> Index()
        {
            var faEntities = await _faService.GetAllAssetsAsync();
            var viewModel = faEntities.Select(asset => new FAVM
            {
                SerialNo = asset.SerialNo,
                AssetCode = asset.AssetCode,
                AssetDescription = asset.AssetDescription,
                Quantity = asset.Quantity,
                BookValue = asset.BookValue,
                PurchaseDate = asset.PurchaseDate,
                Branch = asset.Branch,
                DepartmentId = asset.DepartmentId,
                CrtBy = asset.CrtBy,
                CrtDt = asset.CrtDt
            }).ToList();
            return View(viewModel);
        }
        public async Task<ActionResult> Create()
        {
            var faItems = await _itemService.GetAllItemsAsync();
            var faItem = faItems.Where(i => i.Clamable == false && i.CatType == 0.ToString()).ToList();

            ViewBag.Items = new SelectList(faItems, "ProdCode", "ProdDesc");
            return PartialView("_AddFAModal");
        }
    }
}