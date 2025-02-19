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
using System.Web.Http.Results;
using System.ServiceModel.Security;
using System.Drawing.Design;
using System.Drawing.Drawing2D;
using System.Data;


namespace MMS.Controllers
{
    public class IssueReturnController : BaseController
    {
        private string title = "Issue Return ";
        private readonly IIssueReturnService _issueReturnService;
        private readonly IPurchaseReturnService _purchaseReturnService;
        private readonly IPurchaseService _purchaseService;
        private readonly ICommonService _commonService;
        private readonly IItemService _itemsService;
        private readonly ICompanyService _companyService;
        private readonly IIpAddressService _ipAddressService;
        private DataTable tblTranHdCtd;
        private DataTable tblTranDtCtd;
        
        private int tranCode = Convert.ToInt32(TransactionType.IssueReturn);
        SystemTools _SystemTools = new SystemTools();


        public IssueReturnController(ILogHistoryService logService, IPurchaseService purchaseService, ICommonService commonService, IItemService itemsService, ICompanyService companyService, IPurchaseReturnService purchaseReturnService, IIpAddressService ipAddressService, IIssueReturnService issueReturnService) : base(logService)
        {
            _issueReturnService = issueReturnService;
            _purchaseReturnService = purchaseReturnService;
            _purchaseService = purchaseService;
            _commonService = commonService;
            _itemsService = itemsService;
            _companyService = companyService;
            _ipAddressService = ipAddressService;

        }

        public async Task<ActionResult> Index(bool? YtCP)
        {
            try
            {
                ViewBag.YtCP = YtCP;
                var username = Session["UserName"].ToString();

                var IssueReturnVMList = new List<IssueReturnVM>();

                if (YtCP.HasValue)
                {
                    if (YtCP.Value)
                    {
                        var SIVRefNoYtd = await _issueReturnService.GetSIVRefNoWithYtcp();

                        foreach (var r in SIVRefNoYtd)
                        {
                            var Refno = await _commonService.GetTranHdYdtByIdAsync(r.RefNo);
                            var store = await _purchaseService.GetstoreByIdAsync(r.StoreCode);
                            Session["storec"] = r.StoreCode;

                            IssueReturnVMList.Add(new IssueReturnVM
                            {
                                RefNo = r.RefNo,
                                StoreName = store.StoreName,
                                StoreCode = store.StoreName

                            });
                        }
                    }
                    else
                    {
                        var SIVRefNo = await _issueReturnService.GetSIVRefNoWithoutYtcp();
                        foreach (var r in SIVRefNo)
                        {
                            var Refno = await _commonService.GetTranHdByIdAsync(r.RefNo);
                            var store = await _purchaseService.GetstoreByIdAsync(r.StoreCode);

                           
                            Session["storec"] = r.StoreCode;

                            IssueReturnVMList.Add(new IssueReturnVM
                            {
                                RefNo = r.RefNo,
                                StoreName = store.StoreName,
                                StoreCode = store.StoreName
                            });
                        }
                    }
                }
                else
                {
                    var SIVRefNo = await _issueReturnService.GetSIVRefNoWithoutYtcp();

                    foreach (var r in SIVRefNo)
                    {
                        var Refno = await _commonService.GetTranHdByIdAsync(r.RefNo);
                        var store = await _purchaseService.GetstoreByIdAsync(r.StoreCode);
                        Session["storec"] = r.StoreCode;

                        IssueReturnVMList.Add(new IssueReturnVM
                        {
                            RefNo = r.RefNo,
                            StoreName = store.StoreName,
                            StoreCode = store.StoreName
                        });
                    }
                }

                return View(IssueReturnVMList);
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                TempData["Error"] = "An error occurred while fetching Issue.";
                return RedirectToAction("Index");
            }
        }

        public async Task<ActionResult> GetIssueDetails(string refNo, bool Ytcp)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(refNo))
                    return new HttpStatusCodeResult(400, "Invalid reference number.");

                var username = Session["UserName"].ToString();

                dynamic issueDetails = null;

                if (!Ytcp)
                {
                    issueDetails = await _purchaseReturnService.GetTranDtByIdAsync(refNo);
                }
                else
                {
                    issueDetails = await _purchaseReturnService.GettblTranDtYtdByIdAsync(refNo);
                }


                if (issueDetails == null)
                {
                    TempData["Error"] = "No issue details found for the provided reference number.";
                    Log.Warn($"No purchase entries found for RefNo: {refNo}");
                    return RedirectToAction("Index");
                }

                var Transactiontype = await _commonService.GetTransactionTypeByTranCodeAsync(tranCode);

                tblStore results = null;

                var IssuereturnRefNo = await _issueReturnService.GenerateNewIssueReturnRefNoAsync(Transactiontype.Prefix, username.ToUpper());
                ViewBag.IssuereturnRefNo = IssuereturnRefNo;

                //var purchasedDetails = await _purchaseReturnService.GetTranDtByIdAsync(refNo); ;

                var issueDetailsVM = new List<IssueReturnListDto>();

                foreach (var issued in issueDetails)
                {
                    var item = await _itemsService.GetItemByCodeAsync(issued.ProdCode);
                    string brand = "No Brand found";
                    string type = "No type found";

                    if (issued.ItemBrandId != 0)
                    {
                        var brandData = await _purchaseService.GetBrandByIdAsync(issued.ItemBrandId);
                        brand = brandData?.Brand ?? "No Brand found";
                        type = brandData?.Type ?? "No Type found";
                    }

                    var IssueDetailDto = new IssueReturnListDto
                    {
                        ReturnRefNo = IssuereturnRefNo,
                        RefNo = refNo,
                        Ytcp = Ytcp,
                        ProdCode = issued.ProdCode,
                        ProdDesc = item.ProdDesc,
                        ItemBrandId = issued.ItemBrandId,
                        QtyReturned = issued.QtyReturned,
                        QtyIssued = issued.QtyIssued,
                        Qty = issued.Qty,
                        UnitCost = issued.UnitCost,
                        TotalAmount = issued.TotalAmount,
                        Brand = brand,
                        Type = type
                    };

                    issueDetailsVM.Add(IssueDetailDto);
                }

                return View(issueDetailsVM);
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                return new HttpStatusCodeResult(500, "An error occurred while fetching issue details.");
            }
        }

        public async Task<ActionResult> ReturnIssue(string refNo, string ProdCode, bool Ytcp)
        {
            try
            {
                var username = Session["UserName"].ToString();
                var user = await _issueReturnService.GetUserByUserNameAsync(username);
                string userid = user?.UserId.ToString();

                dynamic result = null;
                dynamic result11 = null;
                dynamic DetailId = null;
                dynamic YtdId = null;

                IssueReturnListDto issueReturnDto = null;

                if (!Ytcp)
                {
                    result = await _commonService.GetTranHdByIdAsync(refNo);
                    result11 = await _commonService.GetTranDtByIdAsync(refNo);
                    var StoreIssued = await _issueReturnService.GetStoreIssuedByIdAsync(result.StoreCode, userid);
                    var StoreReturned = await _issueReturnService.GetStoreReturnedByIdAsync(result.StoreCode, userid);
                    ViewBag.Issued = new SelectList(StoreIssued, "StoreCode", "StoreName");
                    ViewBag.Returned = new SelectList(StoreReturned, "StoreCode", "StoreName");
                    DetailId = result11.DetailId;
                    YtdId = 0;
                }
                else
                {
                    result = await _commonService.GetTranHdYdtByIdAsync(refNo);
                    result11 = await _commonService.GetTranDtYdtByIdAsync(refNo);
                    var StoreIssued = await _issueReturnService.GetStoreIssuedByIdAsync(result.StoreCode, userid);
                    var StoreReturned = await _issueReturnService.GetStoreReturnedByIdAsync(result.StoreCode, userid);
                    ViewBag.Issued = new SelectList(StoreIssued, "StoreCode", "StoreName");
                    ViewBag.Returned = new SelectList(StoreReturned, "StoreCode", "StoreName");
                    YtdId = result11.YtdId;
                    DetailId = result11.DetailId;
                }

               
                issueReturnDto = new IssueReturnListDto
                {
                  
                    RefNo = refNo,
                    ProdCode = ProdCode,
                    Ytcp = Ytcp,
                    DetailId = DetailId,
                    YtdId = YtdId,
                    Issued = result.StoreCode,
                    ItemBrandId = result11.ItemBrandId,
                    QtyReturned = result11.QtyReturned,
                    QtyIssued = result11.QtyIssued,
                    Qty = result11.Qty,
                    UnitCost = result11.UnitCost,
                    TotalAmount = result11.TotalAmount,
                    Returned = result.StoreCode  
                };

                return View(issueReturnDto);
            }
            catch (Exception ex)
            {
                LogException(ex, "Error while loading the issue for return.");
                TempData["Error"] = "An error occurred while loading the Issue.";
                return RedirectToAction("Index");
            }
        }



        [HttpPost]
        public async Task<ActionResult> SaveReturnIssue(IssueReturnListDto model)
        {
            try
            {
             

                var currentPeriod = _SystemTools.GetCurrentPeriod();

                var username = Session["UserName"].ToString();
                var Remark = "Issue Return";
                var Transactiontype = await _commonService.GetTransactionTypeByTranCodeAsync(tranCode);
                var IssuereturnRefNo = await _issueReturnService.GenerateNewIssueReturnRefNoAsync(Transactiontype.Prefix, username.ToUpper());

                tblTranHd TranHdIssuedCtd = new tblTranHd();
                tblTranHdYtd TranHdIssuedYtd = new tblTranHdYtd();
                tblTranDt TranDtIssuedCtd = new tblTranDt();
                tblTranDtYtd TranDtIssuedYtd = new tblTranDtYtd();

                if (!model.Ytcp)
                {
                    var tranDtList = await _issueReturnService.GetTranDtByDetailIdAsync(model.DetailId);
                    TranDtIssuedCtd = tranDtList.FirstOrDefault();
                }
                else
                {
                    var tranDtYtdList = await _issueReturnService.GetTranDtYtdByDetailIdAsync(model.DetailId, model.YtdId);
                    TranDtIssuedYtd = tranDtYtdList.FirstOrDefault();
                }

                var purchase = await _issueReturnService.GetTranDtByIdAsync(model.RefNo);

                if (await _issueReturnService.ValidateQty(model.QtyReturned, (model.Ytcp == false ? TranDtIssuedCtd.Qty : TranDtIssuedYtd.Qty), (model.Ytcp == false ? TranDtIssuedCtd.QtyReturned : TranDtIssuedYtd.QtyReturned)))
                {
                    var user = await _issueReturnService.GetUserByUserNameAsync(username);
                    string userid = user?.UserId.ToString();
                    var storeCodeReturn = model.Returned;
                    var storeCodeIssue = model.Issued;

                    dynamic IsNew = null;

                    if (await _purchaseReturnService.ValidateRefNo(IssuereturnRefNo))
                    {
                        IsNew = true;
                    }

                    if (!model.Ytcp)
                        TranHdIssuedCtd = await _commonService.GetTranHdByIdAsync(model.RefNo);
                    else
                        TranHdIssuedYtd = await _commonService.GetTranHdYdtByIdAsync(model.RefNo);

                    DataTable tblTranHdCtd = new DataTable();
                    tblTranHdCtd.Columns.Add("refNo"); tblTranHdCtd.Columns.Add("refDate"); tblTranHdCtd.Columns.Add("refDateEth"); tblTranHdCtd.Columns.Add("supplier"); tblTranHdCtd.Columns.Add("Department"); tblTranHdCtd.Columns.Add("storeCode");
                    tblTranHdCtd.Columns.Add("Clamable"); tblTranHdCtd.Columns.Add("remark"); tblTranHdCtd.Columns.Add("TranCode"); tblTranHdCtd.Columns.Add("Period"); tblTranHdCtd.Columns.Add("otherRefNo"); tblTranHdCtd.Columns.Add("Branch");

                    var tranHdCtd = tblTranHdCtd.NewRow();
                    {
                        tranHdCtd["refNo"] = IssuereturnRefNo;
                        tranHdCtd["refDate"] = DateTime.Today;
                        tranHdCtd["refDateEth"] = EthiopianDateConverter.ConvertToEthiopianDate(DateTime.Today);
                        tranHdCtd["supplier"] = model.Ytcp == false ? TranHdIssuedCtd.Supplier : TranHdIssuedYtd.Supplier;
                        tranHdCtd["Department"] = model.Ytcp == false ? TranHdIssuedCtd.Department : TranHdIssuedYtd.Department;
                        tranHdCtd["storeCode"] = storeCodeReturn;
                        tranHdCtd["Clamable"] = model.Ytcp == false ? TranHdIssuedCtd.Clamable : TranHdIssuedYtd.Clamable;
                        tranHdCtd["remark"] = Remark;
                        tranHdCtd["TranCode"] = tranCode;
                        tranHdCtd["Period"] = currentPeriod;
                        tranHdCtd["otherRefNo"] = model.RefNo;
                        tranHdCtd["Branch"] = model.Ytcp == false ? TranHdIssuedCtd.Branch : TranHdIssuedYtd.Branch;
                    }


                    var tranDtCtd = new DataTable();
                    tranDtCtd.Columns.Add("TranCode"); tranDtCtd.Columns.Add("DetailId"); tranDtCtd.Columns.Add("AiborAic");tranDtCtd.Columns.Add("Journalize"); tranDtCtd.Columns.Add("RefNo"); tranDtCtd.Columns.Add("StoreCode");
                    tranDtCtd.Columns.Add("Qty"); tranDtCtd.Columns.Add("UnitCost"); tranDtCtd.Columns.Add("QtyReturned"); tranDtCtd.Columns.Add("TotalAmount"); tranDtCtd.Columns.Add("VatAmount"); tranDtCtd.Columns.Add("WithHolding");
                    tranDtCtd.Columns.Add("StockAccountNo");  tranDtCtd.Columns.Add("CostAccountNo"); tranDtCtd.Columns.Add("CostCenter");tranDtCtd.Columns.Add("Costcc"); tranDtCtd.Columns.Add("Stockcc"); tranDtCtd.Columns.Add("ProdCode"); tranDtCtd.Columns.Add("CrtBy");

                 
                    var tranDtRow = tranDtCtd.NewRow();
                    {
                        tranDtRow["TranCode"] = tranCode;
                        tranDtRow["DetailId"] = model.Ytcp == false ? TranDtIssuedCtd.DetailId : TranDtIssuedYtd.YtdId;
                        tranDtRow["AiborAic"] = "NET";
                        tranDtRow["Journalize"] = false;
                        tranDtRow["RefNo"] = IssuereturnRefNo;
                        tranDtRow["StoreCode"] = storeCodeReturn;
                        tranDtRow["Qty"] = model.QtyReturned;
                        tranDtRow["UnitCost"] = model.Ytcp == false ? TranDtIssuedCtd.UnitCost : TranDtIssuedYtd.UnitCost;
                        tranDtRow["QtyReturned"] = model.QtyReturned;
                        tranDtRow["TotalAmount"] = model.Ytcp == false ? TranDtIssuedCtd.TotalAmount : TranDtIssuedYtd.TotalAmount;
                        tranDtRow["VatAmount"] = model.Ytcp == false ? TranDtIssuedCtd.VatAmount : TranDtIssuedYtd.VatAmount;
                        tranDtRow["WithHolding"] = model.Ytcp == false ? TranDtIssuedCtd.WithHolding : TranDtIssuedYtd.WithHolding;
                        tranDtRow["StockAccountNo"] = model.Ytcp == false ? TranDtIssuedCtd.StockAccountNo : TranDtIssuedYtd.StockAccountNo;
                        tranDtRow["CostAccountNo"] = model.Ytcp == false ? TranDtIssuedCtd.CostAccountNo : TranDtIssuedYtd.CostAccountNo;
                        tranDtRow["Costcc"] = model.Ytcp == false ? TranDtIssuedCtd.Costcc : TranDtIssuedYtd.Costcc;
                        tranDtRow["Stockcc"] = await _issueReturnService.GetCostCenterFromStoreCode(storeCodeReturn);
                        tranDtRow["ProdCode"] = model.Ytcp == false ? TranDtIssuedCtd.ProdCode : TranDtIssuedYtd.ProdCode;
                        tranDtRow["CrtBy"] = username;
                    }


                    bool failed = !await _issueReturnService.ProcessIssueReturnAsync(tranDtRow, tranHdCtd, title, IsNew, storeCodeReturn, storeCodeReturn == storeCodeIssue ? false : true, model.RefNo, tranCode, false, model.Ytcp);

                    if (!failed)
                    {
                        TempData["Success"] = "Issue Return completed successfully!";
                    }
                    else
                    {
                        TempData["Error"] = "Issue Return entry couldn't be performed!";
                        return View("Index");
                    }
                }
                else
                {
                    TempData["Error"] = "Qty Recieved is Invalid!";
                }

                string logMessage = $"A Issue was Returned by '{Session["UserName"]}':\n" +
                                    $"- RefNo: '{model.RefNo}'\n" +
                                    $"- Description: '{Remark}'\n" +
                                    $"- StoreCode: '{model.StoreCode}'\n";
                await LogActionAsync(model.RefNo, "Return Issue", logMessage, Session["UserName"].ToString());

                TempData["Success"] = "Issue Return completed successfully!";
                return RedirectToAction("PrintPreview", new { refNo = model.RefNo });
            }
            catch (Exception ex)
            {
                LogException(ex, "Error while Returning the Issue.");
                TempData["Error"] = "An error occurred while Returning the Issue.";
                return View("GetIssueDetails");
            }
        }


        public ActionResult PrintPreview(string RefNo)
        {
            return Redirect($"~/ReportViewers/IssueReturnReportViewer.aspx?Id={RefNo}");
        }

        private void LogException(Exception ex, string message)
        {
            Log.Error(message, ex);
        }
    }
}