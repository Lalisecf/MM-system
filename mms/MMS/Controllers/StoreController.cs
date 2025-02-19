using BusinessLayer.Interfaces;
using MMS.Models.Inventory;
using SharedLayer.Models.Inventory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace MMS.Controllers
{
    public class StoreController : BaseController
    {
        private readonly IStoreService _storeService;

        public StoreController(IStoreService storeService, ILogHistoryService logService)
            : base(logService)
        {
            _storeService = storeService;
        }

        // GET: Store
        public async Task<ActionResult> Index()
        {
            try
            {
                var stores = await _storeService.GetAllStoresAsync();
                var storeVMs = stores.Select(r => new StoreVM
                {
                    StoreCode = r.StoreCode,
                    StoreName = r.StoreName,
                    Address = r.Address,
                    AccountNo = r.AccountNo,
                    BranchCode = r.BranchCode,
                    ShortCode = r.ShortCode,
                    StockCc = r.StockCc
                }).ToList();
                return View(storeVMs);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "An error occurred while loading stores.";
                return RedirectToAction("Index");
            }
        }

        // GET: Store/Create
        public ActionResult Create()
        {
            return PartialView("_AddStoreModal", new StoreVM());
        }

        // POST: Store/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(StoreVM storeVM)
        {
            if (!ModelState.IsValid)
                return View(storeVM);

            try
            {
                var store = new tblStore
                {
                    StoreCode = storeVM.StoreCode,
                    StoreName = storeVM.StoreName,
                    Address = storeVM.Address,
                    AccountNo = storeVM.AccountNo,
                    BranchCode = storeVM.BranchCode,
                    ShortCode = storeVM.ShortCode,
                    StockCc = storeVM.StockCc,
                    IsTransit = storeVM.IsTransit,
                    IsDamaged = storeVM.IsDamaged
                };

                await _storeService.AddStoreAsync(store);
                string logMessage = $"A new store was created by '{Session["UserName"]}':\n" +
                                  $"- StoreCode: '{store.StoreCode}'\n" +
                                  $"- StoreName: '{store.StoreName}'\n" +
                                  $"- StockCc: '{store.StockCc}'\n" +
                                  $"- ShortCode: '{store.ShortCode}'\n" +
                                  $"- BranchCode: '{store.BranchCode}'";
                await LogActionAsync(store.StoreCode, "Create Store", logMessage, Session["UserName"].ToString());
                TempData["Success"] = "Store created successfully.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["Error"] = "An error occurred while creating the store.";
                return View(storeVM);
            }
        }

        // GET: Store/Edit/{id}
        public async Task<ActionResult> Edit(string storeCode)
        {
            try
            {
                if (string.IsNullOrEmpty(storeCode))
                {
                    TempData["Error"] = "Invalid StoreCode.";
                    return RedirectToAction("Index");
                }

                var store = await _storeService.GetStoreByNameAsync(storeCode);
                if (store == null)
                {
                    TempData["Error"] = "Store not found.";
                    return RedirectToAction("Index");
                }

                var storeVM = new StoreVM
                {
                    StoreCode = store.StoreCode,
                    StoreName = store.StoreName,
                    Address = store.Address,
                    AccountNo = store.AccountNo,
                    BranchCode = store.BranchCode,
                    ShortCode = store.ShortCode,
                    StockCc = store.StockCc,
                    IsSatellite = store.IsSatellite ?? false,
                    IsTransit = store.IsTransit ?? false,
                    IsDamaged = store.IsDamaged ?? false
                };

                return PartialView("_EditStoreModal", storeVM);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "An error occurred while loading the store for editing.";
                return RedirectToAction("Index");
            }
        }

        // POST: Store/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(StoreVM model)
        {
            try
            {
                var store = await _storeService.GetStoreByNameAsync(model.StoreCode);
                var changes = new List<string>();

                if (store.StoreName != model.StoreName)
                {
                    changes.Add($"StoreName changed from '{store.StoreName}' to '{model.StoreName}'");
                    store.StoreName = model.StoreName;
                }

                if (store.Address != model.Address)
                {
                    changes.Add($"Address changed from '{store.Address}' to '{model.Address}'");
                    store.Address = model.Address;
                }

                if (store.AccountNo != model.AccountNo)
                {
                    changes.Add($"AccountNo changed from '{store.AccountNo}' to '{model.AccountNo}'");
                    store.AccountNo = model.AccountNo;
                } 
                if (store.StockCc != model.StockCc)
                {
                    changes.Add($"StockCc changed from '{store.StockCc}' to '{model.StockCc}'");
                    store.StockCc = model.StockCc;
                } if (store.ShortCode != model.ShortCode)
                {
                    changes.Add($"ShortCode changed from '{store.ShortCode}' to '{model.AccountNo}'");
                    store.AccountNo = model.ShortCode;
                }

                await _storeService.UpdateStoreAsync(store);

                if (changes.Any())
                {
                    string logMessage = $"Store '{store.StoreCode}' updated by '{Session["UserName"]}':\n" +
                                        string.Join("\n", changes);
                    await LogActionAsync(store.StoreCode, "Edit Store", logMessage,  Session["UserName"].ToString());
                }

                TempData["Success"] = "Store updated successfully.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                LogException(ex, "Error while updating the store.");
                TempData["Error"] = "An error occurred while updating the store.";
                return PartialView("_EditStoreModal", model);
            }
        }
        private void LogException(Exception ex, string message)
        {
            TempData["Error"] = message;
        }
    }
}
