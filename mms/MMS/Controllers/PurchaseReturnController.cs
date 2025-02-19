using BusinessLayer.Interfaces;
using SharedLayer.Models.Inventory;
using System;
using System.Threading.Tasks;
using System.Web.Mvc;
using MMS.Models.Inventory;
using SharedLayer.AB_Common;
using MMS.Models;
using BusinessLayer.Service;
using System.Xml.Linq;
using BusinessLayer.Services;
using System.Net;
using System.Collections.Generic;
using System.Linq;
using SharedLayer.Models;
using System.Runtime.CompilerServices;
using static SharedLayer.AB_Common.SystemTools;
using System.ComponentModel;
using SharedLayer.DTO;


namespace MMS.Controllers
{
    public class PurchaseReturnController : BaseController
    {
        private string title = "Purchase Return ";
        private readonly IPurchaseReturnService _purchaseReturnService;
        private readonly IPurchaseService _purchaseService;
        private readonly ICommonService _commonService;
        private readonly IItemService _itemsService;
        private readonly ICompanyService _companyService;
        private readonly IIpAddressService _ipAddressService;
        private int tranCode = Convert.ToInt32(TransactionType.PurchaseReturn);
        SystemTools _SystemTools = new SystemTools();


        public PurchaseReturnController(ILogHistoryService logService, IPurchaseService purchaseService, ICommonService commonService, IItemService itemsService, ICompanyService companyService, IPurchaseReturnService purchaseReturnService, IIpAddressService ipAddressService) : base(logService)
        {
            _purchaseReturnService = purchaseReturnService;
            _purchaseService = purchaseService;
            _commonService = commonService;
            _itemsService = itemsService;
            _companyService = companyService;
            _ipAddressService = ipAddressService;

        }

        public async Task<ActionResult> Index(bool? Ytd)
        {
            try
            {
                ViewBag.Ytd = Ytd;

                var PurchaseReturnVMList = new List<PurchaseReturnVM>();

                if (Ytd.HasValue)
                {
                    if (Ytd.Value)
                    {
                        var SRVRefNoYtd = await _purchaseReturnService.GetSRVRefNoWithYtd();

                        foreach (var r in SRVRefNoYtd)
                        {
                            var Refno = await _commonService.GetTranHdYdtByIdAsync(r.RefNo);
                            var supplier = await _purchaseService.GetSupplierByIdAsync(Refno.Supplier);
                            var store = await _purchaseService.GetstoreByIdAsync(r.StoreCode);


                            PurchaseReturnVMList.Add(new PurchaseReturnVM
                            {
                                RefNo = r.RefNo,
                                Supplier = supplier.customerName,
                                StoreCode = store.StoreName

                            });
                        }
                    }
                    else
                    {
                        var SRVRefNo = await _purchaseReturnService.GetSRVRefNoWithoutYtd();
                        foreach (var r in SRVRefNo)
                        {
                            var Refno = await _commonService.GetTranHdByIdAsync(r.RefNo);
                            var supplier = await _purchaseService.GetSupplierByIdAsync(Refno.Supplier);
                            var store = await _purchaseService.GetstoreByIdAsync(r.StoreCode);


                            PurchaseReturnVMList.Add(new PurchaseReturnVM
                            {
                                RefNo = r.RefNo,
                                Supplier = supplier.customerName,
                                StoreCode = store.StoreName
                            });
                        }
                    }
                }
                else
                {
                    var SRVRefNo = await _purchaseReturnService.GetSRVRefNoWithoutYtd();

                    foreach (var r in SRVRefNo)
                    {
                        var Refno = await _commonService.GetTranHdByIdAsync(r.RefNo);
                        var supplier = await _purchaseService.GetSupplierByIdAsync(Refno.Supplier);
                        var store = await _purchaseService.GetstoreByIdAsync(r.StoreCode);

                        PurchaseReturnVMList.Add(new PurchaseReturnVM
                        {
                            RefNo = r.RefNo,
                            Supplier = supplier.customerName,
                            StoreCode = store.StoreName
                        });
                    }
                }

                return View(PurchaseReturnVMList);
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                TempData["Error"] = "An error occurred while fetching purchase.";
                return RedirectToAction("Index");
            }
        }


        // GET: Purchase/Details/{refNo}
        public async Task<ActionResult> GetPurchaseDetails(string refNo, bool Ytd)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(refNo))
                    return new HttpStatusCodeResult(400, "Invalid reference number.");

                var username = Session["UserName"].ToString();

                var purchase = await _purchaseReturnService.GetTranDtByIdAsync(refNo);

                if (purchase == null)
                {
                    TempData["Error"] = "No purchase details found for the provided reference number.";
                    Log.Warn($"No purchase entries found for RefNo: {refNo}");
                    return RedirectToAction("Index");
                }

                var Transactiontype = await _commonService.GetTransactionTypeByTranCodeAsync(tranCode);

                tblStore results = null;

                if (!Ytd)
                {
                    var result = await _commonService.GetTranHdByIdAsync(refNo);
                    results = await _purchaseService.GetstoreByIdAsync(result.StoreCode);
                }
                else
                {
                    var result = await _commonService.GetTranHdYdtByIdAsync(refNo);
                    results = await _purchaseService.GetstoreByIdAsync(result.StoreCode);
                }

                var returnRefNo = await _purchaseReturnService.GenerateNewReturnRefNoAsync(Transactiontype.Prefix, username.ToUpper(), results.ShortCode);
                ViewBag.ReturnRefNo = returnRefNo;

                var purchasedDetails = await _purchaseReturnService.GetTranDtByIdAsync(refNo); ;

                var purchaseDetailsVM = new List<PurchaseReturnListDto>();

                foreach (var purchased in purchasedDetails)
                {
                    var item = await _itemsService.GetItemByCodeAsync(purchased.ProdCode);
                    string brand = "No Brand found";
                    string type = "No type found";

                    if (purchased.ItemBrandId != 0)
                    {
                        var brandData = await _purchaseService.GetBrandByIdAsync(purchased.ItemBrandId);
                        brand = brandData?.Brand ?? "No Brand found";
                        type = brandData?.Type ?? "No Type found";
                    }

                    var purchaseDetailDto = new PurchaseReturnListDto
                    {
                        ReturnRefNo = returnRefNo,
                        RefNo = refNo,
                        ProdCode = purchased.ProdCode,
                        ProdDesc = item.ProdDesc, 
                        ItemBrandId = purchased.ItemBrandId,
                        QtyReturned = purchased.QtyReturned,
                        QtyIssued = purchased.QtyIssued,
                        Qty = purchased.Qty,
                        UnitCost = purchased.UnitCost,
                        TotalAmount = purchased.TotalAmount,
                        Brand = brand,
                        Type = type
                    };

                    purchaseDetailsVM.Add(purchaseDetailDto);
                }

                return View(purchaseDetailsVM);
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                return new HttpStatusCodeResult(500, "An error occurred while fetching Purchase details.");
            }
        }



        [HttpPost]
        public async Task<ActionResult> ReturnPurchase(PurchaseReturnVM model)
        {
            try
            {

                if (!ModelState.IsValid)
                {
                    TempData["Error"] = "Invalid model data.";
                    return View("Index");
                }

                bool loopFailed = false;


                var username = Session["UserName"].ToString();
                var purchase = await _commonService.GetTranDtByIdAsync(model.RefNo);
                var Transactiontype = await _commonService.GetTransactionTypeByTranCodeAsync(tranCode);

                bool IsNew = false;

                var Period = _companyService.GetCompany();

                var createdDate = DateTime.UtcNow;
                var crtws = _ipAddressService.GetIpAddress();
                var createdDateEth = EthiopianDateConverter.ConvertToEthiopianDate(createdDate);

                if (!model.Ytd)
                {
                    var result = await _commonService.GetTranHdByIdAsync(model.RefNo);
                    var results = await _purchaseService.GetstoreByIdAsync(result.StoreCode);

                    if (results != null)
                    {
                        model.ShortCode = results.ShortCode;
                        model.ReturnRefNo = await _purchaseReturnService.GenerateNewReturnRefNoAsync(Transactiontype.Prefix, username.ToUpper(), results.ShortCode);
                    }

                    if (await _purchaseReturnService.ValidateRefNo(model.ReturnRefNo))
                    {
                        IsNew = true;

                        var tranhd = new tblTranHd
                        {
                            RefNo = model.ReturnRefNo,
                            RefDate = createdDate,
                            RefDateEth = createdDateEth,
                            Supplier = result.Supplier,
                            StoreCode = result.StoreCode,
                            Branch = result.Branch,
                            Remark = "",
                            TranCode = tranCode,
                            Period = Convert.ToInt32(Period.CurrentPeriod.ToString()),
                            otherRefNo = model.RefNo,
                        };

                        var result1 = await _purchaseReturnService.GetTranDtByIdAsync(model.RefNo);

                        foreach (tblTranDt TranDt in result1)
                        {
                            var itemDetail = await _purchaseReturnService.GetItemDetailByIdAsync(TranDt.ProdCode, TranDt.StoreCode);

                            if (!await _purchaseReturnService.ValidateFA(TranDt.ProdCode, TranDt.RefNo))
                            {
                                TempData["Error"] = "Fixed Asset has been registered with the selected RefNo!!!";
                                return View("GetPurchaseDetails");
                            }

                            if (!await _purchaseReturnService.ValidateQty(itemDetail == null ? 0 : itemDetail.QtyOnHand, TranDt.Qty))
                            {
                                TempData["Error"] = "Purchase Item Code" + itemDetail.ProdCode + " Already Issued!";
                                return View("GetPurchaseDetails");
                            }

                            if (!await _purchaseReturnService.ValidateQtyReturned(TranDt.QtyReturned, TranDt.QtyRecived, TranDt.QtyIssued))
                            {
                                TempData["Error"] = "Purchase Item Code" + itemDetail.ProdCode + " Already has a Transaction!";
                                return View("GetPurchaseDetails");
                            }
                            var trandt = new tblTranDt
                            {
                                TranCode = tranCode,
                                DetailId = TranDt.DetailId,
                                WithHolding = 0,
                                aiborAic = "NET",
                                Journalize = false,
                                RefNo = model.ReturnRefNo,
                                StoreCode = TranDt.StoreCode,
                                Qty = TranDt.Qty,
                                UnitCost = TranDt.UnitCost,
                                QtyReturned = TranDt.Qty,
                                TotalAmount = TranDt.TotalAmount,
                                VatAmount = TranDt.VatAmount,
                                StockAccountNo = TranDt.StockAccountNo,
                                CostAccountNo = TranDt.CostAccountNo,
                                CostCenter = TranDt.CostCenter,
                                Stockcc = TranDt.Stockcc,
                                ProdCode = TranDt.ProdCode,
                                CrtBy = username,
                                CrtDt = createdDate,
                                CrtWs = crtws,
                                ItemBrandId = TranDt.ItemBrandId
                                
                            };
                            bool failed = !await _purchaseReturnService.ProcessPurchaseReturnAsync(trandt, tranhd, title, IsNew, TranDt.StoreCode, false, false);

                            if (!failed)
                            { IsNew = false; }

                            if (failed)
                            { loopFailed = true; }
                        }
                    }
                }

                //region Ytd is true
                if (model.Ytd)
                {
                    var result = await _commonService.GetTranHdYdtByIdAsync(model.RefNo);
                    var results = await _purchaseService.GetstoreByIdAsync(result.StoreCode);

                    if (results != null)
                    {
                        model.ShortCode = results.ShortCode;
                        model.ReturnRefNo = await _purchaseReturnService.GenerateNewReturnRefNoAsync(Transactiontype.Prefix, username.ToUpper(), results.ShortCode);
                    }

                    if (await _purchaseReturnService.ValidateRefNo(model.ReturnRefNo))
                    {
                        IsNew = true;

                        var tranhd = new tblTranHd
                        {
                            RefNo = model.ReturnRefNo,
                            RefDate = createdDate,
                            RefDateEth = createdDateEth,
                            Supplier = result.Supplier,
                            StoreCode = result.StoreCode,
                            Branch = result.Branch,
                            Remark = "",
                            TranCode = tranCode,
                            Period = Convert.ToInt32(Period.CurrentPeriod.ToString()),
                            otherRefNo = model.RefNo,
                        };

                        var result1 = await _purchaseReturnService.GettblTranDtYtdByIdAsync(model.RefNo);

                        foreach (tblTranDtYtd TranDtYtd in result1)
                        {
                            var itemDetail = await _purchaseReturnService.GetItemDetailByIdAsync(TranDtYtd.ProdCode, TranDtYtd.StoreCode);

                            if (!await _purchaseReturnService.ValidateFA(TranDtYtd.ProdCode, TranDtYtd.RefNo))
                            {
                                TempData["Error"] = "Fixed Asset has been registered with the selected RefNo!!!";
                                return View("Index");
                            }

                            if (!await _purchaseReturnService.ValidateQty(itemDetail == null ? 0 : itemDetail.QtyOnHand, TranDtYtd.Qty))
                            {
                                TempData["Error"] = "Purchase Item Code" + itemDetail.ProdCode + " Already Issued!";
                                return View("Index");
                            }

                            if (!await _purchaseReturnService.ValidateQtyReturned(TranDtYtd.QtyReturned, TranDtYtd.QtyRecived, TranDtYtd.QtyIssued))
                            {
                                TempData["Error"] = "Purchase Item Code" + itemDetail.ProdCode + " Already has a Transaction!";
                                return View("Index");
                            }
                       
                            var trandt = new tblTranDt
                            {
                                TranCode = tranCode,
                                DetailId = TranDtYtd.YtdId,
                                WithHolding = 0,
                                aiborAic = "NET",
                                Journalize = false,
                                RefNo = model.ReturnRefNo,
                                StoreCode = TranDtYtd.StoreCode,
                                Qty = TranDtYtd.Qty,
                                UnitCost = TranDtYtd.UnitCost,
                                QtyReturned = TranDtYtd.Qty,
                                TotalAmount = TranDtYtd.TotalAmount,
                                VatAmount = TranDtYtd.VatAmount,
                                StockAccountNo = TranDtYtd.StockAccountNo,
                                CostAccountNo = TranDtYtd.CostAccountNo,
                                CostCenter = TranDtYtd.CostCenter,
                                Stockcc = TranDtYtd.Stockcc,
                                ProdCode = TranDtYtd.ProdCode,
                                CrtBy = username,
                                CrtDt = createdDate,
                                CrtWs = crtws,
                                ItemBrandId = TranDtYtd.ItemBrandId,
                            };

                         
                            bool failed = !await _purchaseReturnService.ProcessPurchaseReturnAsync(trandt, tranhd, title, IsNew, TranDtYtd.StoreCode, false, true);

                            if (!failed)
                            { IsNew = false; }

                            if (failed)
                            { loopFailed = true; }
                        }
                    }
                }

                if (!loopFailed)
                {
                    // Complete transaction, e.g., JournalizeTransaction, etc.
                }
                else
                {
                    TempData["Error"] = "Purchase Return entry couldn't be performed!";
                }

                string logMessage = $"A Purchase was Returned by '{Session["UserName"]}':\n" +
                    $"- RefNo: '{model.ReturnRefNo}'\n" +
                    $"- Description: '{model.Remark}'\n" +
                    $"- StoreCode: '{model.StoreCode}'\n" +
                    $"- ProdCode: {model.ProdCode}\n";
                await LogActionAsync(model.ReturnRefNo, "Return Purchase", logMessage, Session["UserName"].ToString());

                TempData["Success"] = "Purchase Return completed successfully!";
                return RedirectToAction("PrintPreview", new { refNo = model.ReturnRefNo });
               
            }
            catch (Exception ex)
            {
                LogException(ex, "Error while Returning the Purchase.");
                TempData["Error"] = "An error occurred while Returning the Purchase.";
                return View("GetPurchaseDetails");
            }
        }
        public ActionResult PrintPreview(string RefNo)
        {
            return Redirect($"~/ReportViewers/PurchaseReturnReportViewer.aspx?Id={RefNo}");
        }


        private void LogException(Exception ex, string message)
        {
            Log.Error(message, ex);
        }
    }
}
