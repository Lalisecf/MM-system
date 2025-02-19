using BusinessLayer.Interfaces;
using SharedLayer.Models.Inventory;
using System;
using System.Threading.Tasks;
using System.Web.Mvc;
using MMS.Models.Inventory;
using SharedLayer.AB_Common;
using MMS.Models;
using System.Collections.Generic;
using System.Linq;
using SharedLayer.Models;
using static MMS.Models.Inventory.PurchaseVM;
using SharedLayer.DTO;
using BusinessLayer.Service;

namespace MMS.Controllers
{
    public class PurchaseController : BaseController
    {
        private readonly IPurchaseService _purchaseService;
        private readonly IItemService _itemsService;
        private readonly ICompanyService _companyService;
        private readonly ICommonService _commonService;
        private readonly IIpAddressService _ipAddressService;
        private int tranCode = Convert.ToInt32(TransactionType.Purchase);
        private int status = Convert.ToInt32(RequisitionStatus.Requested);
        private readonly IPurchaseReturnService _purchaseReturnService;
        SystemTools _SystemTools = new SystemTools();

        public PurchaseController(ILogHistoryService logService, IPurchaseService purchaseService, IItemService itemsService, ICompanyService companyService, ICommonService commonService, IIpAddressService ipAddressService, IPurchaseReturnService purchaseReturnService) : base(logService)
        {
            _purchaseService = purchaseService;
            _itemsService = itemsService;
            _companyService = companyService;
            _commonService = commonService;
            _ipAddressService = ipAddressService;
            _purchaseReturnService = purchaseReturnService;
        }

        // List all items
        public async Task<ActionResult> Index()
        {
            try
            {
                var purchaseData = await _purchaseService.GetPurchasesByTransactionAndJournalAsync();
                var purchaseVMList = new List<PurchaseVM>();

                foreach (var d in purchaseData)
                {
                    var purchaseVM = new PurchaseVM
                    {
                        RefNo = d.RefNo,
                        ProdCode = d.ProdCode,
                        StoreCode = d.StoreCode,
                        Items = new List<PurchaseItem>()
                    };

                    var brand = await _purchaseService.GetBrandByIdAsync(d.ItemBrandId);

                    var purchaseItem = new PurchaseItem
                    {
                        MainItem = d.ProdCode,
                        ItemBrandId = d.ItemBrandId,
                        Brand = brand?.Brand ?? "Unknown",
                        Type = brand?.Type ?? "Unknown",
                        Qty = d.Qty,
                        UnitCost = d.UnitCost,
                        VatAmount = d.VatAmount, 
                        TotalAmount = d.TotalAmount
                    };

                    purchaseVM.Items.Add(purchaseItem);
                    purchaseVMList.Add(purchaseVM);
                }

                return View(purchaseVMList);
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                TempData["Error"] = "An error occurred while fetching purchases.";
                return RedirectToAction("Index");
            }
        }



        public async Task<ActionResult> AddPurchase()
        {
            try
            {
                ViewBag.Item = new SelectList(_SystemTools.GetItems(), "ItemID", "Description");
                ViewBag.MainItem = new SelectList(await _itemsService.GetAllItemsAsync(), "ProdCode", "ProdDesc");
                ViewBag.Supplier = new SelectList(await _purchaseService.GetAllSupplierAsync(), "customerNumber", "customerName");

                var brands = await _purchaseService.GetAllBrandAsync();
                ViewBag.ItemBrandId = new SelectList(brands.Select(b => new SelectListItem
                {
                    Value = b.ItemBrandId.ToString(),
                    Text = $"{b.Brand} - {b.Type}"
                }), "Value", "Text");

                //var d = await _purchaseService.GetDefaultPurchaseItemAsync();


                var model = new PurchaseVM
                {
                    Items = new List<PurchaseItem>
                {
                    new PurchaseItem
                    {
                       
                        Qty = 0,  
                        UnitCost = 0,   
                        TotalAmount = 0
                    }
                }
                };

                return View(model);
            }
            catch (Exception ex)
            {
                LogException(ex, "Error while preparing the create form.");
                TempData["Error"] = "An error occurred while preparing the create form.";
                return RedirectToAction("Index");
            }
        }




        [HttpPost]

        public async Task<ActionResult> AddPurchase(PurchaseVM model)
        {
            try
            {
                foreach (var item in model.Items)
                {
                    Console.WriteLine($"Main Item: {item.MainItem}, Brand: {item.ItemBrandId}, Qty: {item.Qty}, Unit Cost: {item.UnitCost}, Total Amount: {item.TotalAmount}");
                }

                if (model.Item == "1")
                {
                    model.StoreCode = "STR-01";
                }
                else if (model.Item == "2")
                {
                    model.StoreCode = "STR-02";
                }
                else if (model.Item == "3")
                {
                    model.StoreCode = "STR-11";
                }
                else if (model.Item == "4")
                {
                    model.StoreCode = "STR-12";
                }
                else
                {
                    model.StoreCode = "";
                }

                var username = Session["UserName"].ToString();
                var store = await _purchaseService.GetstoreByIdAsync(model.StoreCode);
                var TransactionType = await _commonService.GetTransactionTypeByTranCodeAsync(tranCode);
                var newRefNo = await _purchaseService.GenerateNewRefNoAsync(username, store.ShortCode, TransactionType.Prefix);
                var Period = _companyService.GetCompany();
                var crtws = _ipAddressService.GetIpAddress();
                var customer = await _purchaseService.GetSupplierByIdAsync(model.Supplier);

                foreach (var items in model.Items)
                {
                    var result1 = await _purchaseService.Getresult1(items.MainItem);
                    var result2 = await _purchaseService.Getresult2(items.MainItem);

                    var item = await _itemsService.GetItemByCodeAsync(items.MainItem);

                    if (items.ItemBrandId != 0)
                    {
                        var brand = await _purchaseService.GetBrandByIdAsync(items.ItemBrandId);
                        items.Brand = brand.Brand;
                        items.Type = brand.Type;
                    }
                    else
                    {
                        items.Brand = "No Brand found";
                    }

                    if (result1.Any() || result2.Any())
                    {
                        TempData["Error"] = "This Software Already Exists. Please Do Cost Buildup!";
                        return RedirectToAction("Index");
                    }

                    var existingPurchase = await _commonService.GetTranDtByIdAsync(newRefNo);
                    bool isNewTranDt = existingPurchase == null;

                    var existingTranHd = await _commonService.GetTranHdByIdAsync(newRefNo);
                    bool isNewTranHd = existingTranHd == null;

                    decimal TotalAmount = items.UnitCost * items.Qty;
                    decimal VatAmount = TotalAmount * 0.15m;

                    if (customer.IsWihholding == true)
                    {
                        VatAmount = (TotalAmount - VatAmount) * Convert.ToDecimal("0.02");
                    }

                    var createdDate = DateTime.UtcNow;
                    var createdDateEth = EthiopianDateConverter.ConvertToEthiopianDate(createdDate);

                    var tranhd = new tblTranHd
                    {
                        RefNo = newRefNo,
                        RefDate = createdDate,
                        RefDateEth = createdDateEth,
                        Supplier = model.Supplier,
                        StoreCode = model.StoreCode,
                        Branch = store.BranchCode?.ToString(),
                        SuppInvNo = model.SuppInvNo,
                        SuppInvDate = DateTime.Today,
                        SuppInvDateEth = createdDateEth,
                        Remark = "",
                        IsPrinted = model.IsPrinted,
                        TranCode = tranCode,
                        Period = Convert.ToInt32(Period.CurrentPeriod.ToString()),
                        Clamable = item.Clamable,
                        CatType = item.CatType,
                    };
                    
                        var purchase = new tblTranDt
                        {
                            RefNo = newRefNo,
                            TranCode = tranCode,
                            QtyReturned = 0,
                            WithHolding = 0,
                            aiborAic = "NET",
                            Journalize = false,
                            StoreCode = model.StoreCode,
                            Qty = items.Qty,
                            UnitCost = items.UnitCost,
                            QtyIssued = 0,
                            QtyRecived = 0,
                            CrtBy = username,
                            CrtWs = crtws,
                            StockAccountNo = await _purchaseService.GetStockAccountAsync(model.StoreCode, items.MainItem),
                            CostAccountNo = await _purchaseService.GetSuppAccountNoAsync(Convert.ToString(model.Supplier)),
                            Stockcc = await _purchaseService.GetCostCenterFromStoreCodeAsync(model.StoreCode),
                            ProdCode = items.MainItem,
                            TotalAmount = TotalAmount,
                            VatAmount = VatAmount,
                            ItemBrandId = items.ItemBrandId,
                        };

                        await _commonService.AddTranHdandTranDtAsync(tranhd, purchase);

                        bool failed = !await _purchaseService.ProcessItemPurchaseAsync(purchase, tranhd, "", tranCode.ToString(), isNewTranHd, isNewTranDt, false, false, false);

                        if (!failed)
                        {
                            if (model.StoreCode == "STR-12")
                            {
                                var costBuildup = new tblCostBuildup
                                {
                                    RefNo = newRefNo,
                                    Description = "Initial Cost payment for <-> " + items.MainItem,
                                    ProdCode = items.MainItem,
                                    Cost = items.UnitCost,
                                    CrtDt = createdDate,
                                    CrtBy = Session["UserName"].ToString(),
                                    CrtWs = crtws
                                };

                                await _purchaseService.AddtblCostBuildups(costBuildup);

                                var reqDt = new tblRequestDetail
                                {
                                    ReqNo = "MR - " + items.MainItem,
                                    ProdCode = items.MainItem,
                                    QtyReq = Convert.ToDecimal(1),
                                    UnitCost = items.UnitCost,
                                    TotalCost = 1 * items.UnitCost,
                                    StockAcct = "",
                                    CostAcct = "",
                                    CostCenter = "000",
                                    QtyCancel = Convert.ToDecimal(1),
                                    QtyIssued = 0
                                };

                                await _purchaseService.AddtblRequestDetails(reqDt);

                                var request = new tblRequest
                                {
                                    Clamable = false,
                                    ReqNo = "MR - " + items.MainItem,
                                    ReqBy = Convert.ToInt32(Session["UserId"]),
                                    ReqDate = DateTime.UtcNow,
                                    ReqDateEth = EthiopianDateConverter.ConvertToEthiopianDate(createdDate),
                                    IsDocPrinted = false,
                                    BatchNo = "SO",
                                    ReqDept = "",
                                    IsForBatch = false,
                                    Reason = "for work",
                                    Status = status,
                                    CatType = "5",
                                    StoreCode = "STR-12"
                                };

                                await _purchaseService.AddtblRequests(request);


                                string logMessage = $"A new Purchase was created by '{Session["UserName"]}':\n" +
                                             $"- RefNo: '{newRefNo}'\n" +
                                             $"- Description: '{model.CatDesc}'\n" +
                                             $"- StoreCode: '{model.StoreCode}'\n" +
                                             $"- ProdCode: {items.MainItem}\n";
                                await LogActionAsync(newRefNo, "Create Purchase", logMessage, Session["UserName"].ToString());
                            }

                           
                        }
                        else
                        {
                            TempData["Error"] = "Purchase entry couldn't be performed!";
                      }
                }
                TempData["Success"] = "Item purchase completed successfully!";
                return RedirectToAction("PrintPreview", new { refNo = newRefNo });

            }
            catch (Exception ex)
            {
                LogException(ex, "Error while creating the Purchase.");
                TempData["Error"] = "An error occurred while creating the Purchase.";
                return View("AddPurchase", model);
            }  
        }


        public async Task<JsonResult> GetBrandsByMainItem(string mainItemId)
        {
            try
            {
                var brands = await _purchaseService.GetBrandsByMainItem(mainItemId);


                IEnumerable<SelectListItem> brandSelectList = brands.Select(b => new SelectListItem
                {
                    Value = b.ItemBrandId.ToString(),
                    Text = $"{b.Brand} - {b.Type}"
                });

                ViewBag.ItemBrandId = new SelectList(brandSelectList, "Value", "Text");

                return Json(brandSelectList, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error fetching Brands" });
            }
        }


        public ActionResult PrintPreview(string RefNo)
        {
            return Redirect($"~/ReportViewers/PurchaseReportViewer.aspx?Id={RefNo}");
        }


        private void LogException(Exception ex, string message)
        {
           Log.Error(message, ex);
        }
    }
}