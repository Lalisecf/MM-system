using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using BusinessLayer.Interfaces;
using MMS.Models.Inventory;
using SharedLayer.Models.Inventory;
using SharedLayer.Models;
using BusinessLayer.Service;
using SharedLayer.AB_Common;
using System.Xml.Linq;
using System.Web.Http.Results;
using System.Net;
using MMS.Models;

namespace MMS.Controllers
{
    public class FAIssueByRequisitionController : BaseController
    {
        private readonly IRequestService _requestService;
        private readonly IMstUserService _userService;
        private readonly IMstUserRoleService _userRoleService;
        private readonly IDepartmentService _departmentService;
        private readonly ICategoryService _categoryService;
        private readonly IRequestDetailService _requestDetailService;
        private readonly IItemService _itemService;
        private readonly ICommonService _commonervice;
        private readonly IStoreService _storeService;
        private readonly IFAIssueByRequisitionService _faIssueByRequisitionService;
        private readonly SystemTools _systemTools = new SystemTools();
        private readonly IIpAddressService _ipAddressService;
        public FAIssueByRequisitionController(IRequestService requestService, IMstUserService userService,
                          IDepartmentService departmentService, ICategoryService categoryService, IRequestDetailService requestDetailService, IItemService itemService, IMstUserRoleService userRoleService, ICommonService commonervice, IStoreService storeService, IFAIssueByRequisitionService faIssueByRequisitionService, IIpAddressService ipAddressService, ILogHistoryService logService)
        : base(logService)
        {
            _requestService = requestService;
            _userService = userService;
            _departmentService = departmentService;
            _categoryService = categoryService;
            _requestDetailService = requestDetailService;
            _itemService = itemService;
            _userRoleService = userRoleService;
            _commonervice = commonervice;
            _storeService = storeService;
            _faIssueByRequisitionService = faIssueByRequisitionService;
            _ipAddressService = ipAddressService;
        }

        public async Task<ActionResult> Index(int page = 1, string searchTerm = null, int pageSize = 100)
        {
            try
            {
                if (!int.TryParse(Session["UserId"]?.ToString(), out int userId))
                {
                    TempData["Error"] = "User session has expired. Please log in again.";
                    return RedirectToAction("Login", "Login");
                }

                int skip = (page - 1) * pageSize;
                var result = await _requestService.GetRequesForFAIssueAsync(userId, skip, pageSize, searchTerm);
                var requests = result.requests;
                var totalRequestsCount = result.totalCount;

                if (requests == null || !requests.Any())
                {
                    TempData["Error"] = "No requests found.";
                    return View(new List<RequestVM>());
                }

                var requestVMList = await MapRequestsToViewModel(requests);

                int totalPages = (int)Math.Ceiling((double)totalRequestsCount / pageSize);

                ViewBag.CurrentPage = page;
                ViewBag.TotalPages = totalPages;
                ViewBag.PaginationInfo = $"Page: {page} of {totalPages} from {totalRequestsCount} total records";
                ViewBag.SearchTerm = searchTerm;

                return View(requestVMList);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"An error occurred: {ex.Message}";
                return View(new List<RequestVM>());
            }
        }


        private async Task<List<RequestVM>> MapRequestsToViewModel(IEnumerable<tblRequest> requests)
        {
            var userIds = requests.Select(r => r.ReqBy ?? 0).Distinct();
            var deptIds = requests.Select(r => long.TryParse(r.ReqDept, out var deptId) ? deptId : 0).Distinct();
            var batchNos = requests.Select(r => r.BatchNo).Distinct();

            var userDict = new Dictionary<int, MstUser>();
            foreach (var userId in userIds)
            {
                var user = await _userService.GetUserByIdAsync(userId);
                if (user != null)
                {
                    userDict[userId] = user;
                }
            }

            var departmentDict = new Dictionary<long, tblDepartment>();
            foreach (var deptId in deptIds)
            {
                var department = await _departmentService.GetDepartmentByIdAsync(deptId);
                if (department != null)
                {
                    departmentDict[deptId] = department;
                }
            }

            var categoryDict = new Dictionary<string, tblCategory>();
            foreach (var batchNo in batchNos)
            {
                var category = await _categoryService.GetCategoryByIdAsync(batchNo);
                if (category != null)
                {
                    categoryDict[batchNo] = category;
                }
            }

            return requests.Select(r => new RequestVM
            {
                FullName = userDict.TryGetValue(r.ReqBy ?? 0, out var user) ? user.FullName : "Unknown User",
                ReqDate = r.ReqDate,
                ReqNo = r.ReqNo,
                BatchNo = categoryDict.TryGetValue(r.BatchNo, out var category) ? category.CatDesc : "Unknown Category",
                ReqDept = departmentDict.TryGetValue(long.Parse(r.ReqDept), out var department) ? department.Name : "Unknown Department"
            }).ToList();
        }
        public class IssuedItem
        {
            public string ItemName { get; set; }
            public string prodCode { get; set; }
            public int QtyToIssue { get; set; }
            public string GRVRefNo { get; set; }
            public decimal UnitCost { get; set; }
        }

        public async Task<ActionResult> Detail(string reqNo)
        {
            ViewBag.ReqNo = reqNo;

            try
            {
                if (string.IsNullOrEmpty(reqNo))
                {
                    TempData["Error"] = "Request number is missing";
                    return RedirectToAction("Index");
                }

                var requestDetails = await _requestDetailService.GetRequestDetailsByReqNoAsync(reqNo);

                if (requestDetails == null || !requestDetails.Any())
                {
                    TempData["Error"] = "No issued item found.";
                    return View(new List<RquestDetailVM>());
                }

                var requestVMList = new List<MMS.Models.Inventory.RquestDetailVM>();

                foreach (var detail in requestDetails)
                {
                    var item = await _itemService.GetItemByCodeAsync(detail.ProdCode);
                    var qtyOnHand = await _itemService.GetQtyOnHandByStoreCodeAndProdCodeAsync(
                        detail.Requests.StoreCode,
                        detail.ProdCode
                    );

                    var transactionType = (int)TransactionType.Purchase;
                    var getDetailGRV = await _commonervice.GetDetailGRVAsync(detail.ProdCode, transactionType);
                    var isGRV_OR_GRN = getDetailGRV != null && getDetailGRV.Any();

                    if (item != null)
                    {
                        var trandtDetails = getDetailGRV?.Select(t => new TrandtVM
                        {
                            RefNo = t.RefNo,
                            UnitCost = t.UnitCost,
                            Qty = t.Qty,
                            QtyIssued = t.QtyIssued,
                            RefDate = t.postDate,
                        }).ToList() ?? new List<TrandtVM>();

                        requestVMList.Add(new MMS.Models.Inventory.RquestDetailVM
                        {
                            IdNo = detail.IdNo.ToString(),
                            ProdDesc = item?.ProdDesc ?? "Unknown Item",
                            ProdCode = detail.ProdCode,
                            QtyReq = (int)detail.QtyReq,
                            QtyOnHand = qtyOnHand,
                            QtyIssued = (int)detail.QtyIssued,
                            UnitCost = detail.UnitCost,
                            TotalCost = detail.TotalCost,
                            isGRV_OR_GRN = isGRV_OR_GRN,
                            trandt = trandtDetails
                        });
                    }
                }

                return View("RequestDetails", requestVMList);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"An error occurred: {ex.Message}";
                return RedirectToAction("Index");
            }
        }



        public async Task<ActionResult> GRV_or_GRN(string idNo)
        {
            try
            {
                if (string.IsNullOrEmpty(idNo))
                {
                    return Content("Request number is missing.", "text/plain");
                }

                int transactionType = (int)TransactionType.Purchase;
                var requestDetail = await _requestDetailService.GetRequestDetailsByIdNoAsync(idNo);
                var requestDetails = await _commonervice.GetDetailGRVAsync(requestDetail.ProdCode, transactionType);
                var item = await _itemService.GetItemByCodeAsync(requestDetail.ProdCode);
                ViewBag.item = item.ProdDesc;
                ViewBag.ReqNo = requestDetail.ReqNo;
                ViewBag.qtyApproved = requestDetail.QtyReq;
                ViewBag.idNo = requestDetail.IdNo;
                if (requestDetails == null || !requestDetails.Any())
                {
                    TempData["Error"] = "No issue found.";
                    return View(new List<TrandtVM>());
                }

                var requestVMList = new List<MMS.Models.Inventory.TrandtVM>();

                foreach (var detail in requestDetails)
                {
                    requestVMList.Add(new MMS.Models.Inventory.TrandtVM
                    {
                        RefNo = detail.RefNo,
                        Qty = detail.Qty,
                        UnitCost = detail.UnitCost,
                        QtyIssued = detail.QtyIssued,
                        QtyToBeIssued = 0,
                        RefDate = detail.postDate,
                    });

                }

                return PartialView("issueFromDetailsModal", requestVMList);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"An error occurred: {ex.Message}";
                return RedirectToAction("Index");
            }
        }
        public async Task<ActionResult> IssueFrom(List<IssuedItem> issuedItems, string ReqNo)
        {
            var issueTranCode = (int)TransactionType.ItemIssue;
            try
            {
                if (issuedItems == null || !issuedItems.Any())
                {
                    return Json(new { success = false, message = "No items to issue." });
                }

                string _shortCode = "";
                var storeCode = "STR-01";
                var refNo = "";
                var result = await _storeService.GetStoreByNameAsync(storeCode);
                if (result != null)
                {
                    _shortCode = result.ShortCode;
                }
                var tranCodeOf = (int)TransactionType.ItemIssue;
                string uName = Session["UserName"].ToString().Substring(0, 2).ToUpper();
                refNo = await _faIssueByRequisitionService.GetNextNumberAsync(tranCodeOf, uName, _shortCode);
                var request = await _requestService.GetRequestByNumberAsync(ReqNo);

                // Create the header
                var tranHD = new tblTranHd
                {
                    RefNo = refNo,
                    RefDate = DateTime.Today,
                    RefDateEth = EthiopianDateConverter.ConvertToEthiopianDate(DateTime.Now),
                    Department = request.ReqDept,
                    StoreCode = request.StoreCode,
                    Branch = request.ReqDept,
                    Remark = request.Reason,
                    TranCode = issueTranCode,
                    Period = _systemTools.GetCurrentPeriod(),
                    Clamable = request.Clamable,
                    ReqNo = request.ReqNo,
                    CatType = "0"
                };
                await _faIssueByRequisitionService.InsertTranHDAsync(tranHD);

                List<tblTranDt> tranDts = new List<tblTranDt>();

                foreach (var item in issuedItems)
                {
                    bool isValid = await _commonervice.ValidateRefNoProdCodeAsync(refNo, item.prodCode, item.GRVRefNo);
                    if (isValid)
                    {
                        var itemMaster = await _itemService.GetItemByCodeAsync(item.prodCode);
                        var tranDt = new tblTranDt
                        {
                            TranCode = issueTranCode,
                            CostAccountNo = "",
                            QtyReturned = 0,
                            VatAmount = 0,
                            Journalize = false,
                            Branch = request.ReqDept,
                            RefNo = tranHD.RefNo,
                            StoreCode = request.StoreCode,
                            Qty = item.QtyToIssue,
                            UnitCost = item.UnitCost,
                            TotalAmount = item.QtyToIssue * item.UnitCost,
                            ProdCode = item.prodCode,
                            CrtBy = Session["UserName"].ToString(),
                            CrtDt = SystemTools.GetServerDate(),
                            QtyIssued = 0,
                            CatType = "0",
                            CrtWs = _ipAddressService.GetIpAddress(),
                            PstWs = _ipAddressService.GetIpAddress(),
                            aiborAic = "NET"
                        };

                        tranDt.StockAccountNo = SystemTools.GetStockAccount(request.StoreCode, item.prodCode);
                        tranDt.CostAccountNo = SystemTools.GetCostAccount1(request.ReqDept, item.prodCode, itemMaster.MainPG);
                        tranDt.Stockcc = SystemTools.GetCostCenterFromStoreCode(request.StoreCode);
                        tranDt.CostCenter = SystemTools.GetCostCenter1(request.ReqDept, itemMaster.MainPG);
                        tranDts.Add(tranDt);

                        bool issueSuccess = await _faIssueByRequisitionService.ItemIssueFAAsync(tranDt, tranHD, ((int)TransactionType.ItemIssue).ToString(), true, item.GRVRefNo);
                        if (issueSuccess)
                        {
                            if (request.ReqDept == "10117")
                            {
                                await _faIssueByRequisitionService.HandleCostSplitAsync(tranDt);
                            }
                        }
                        else
                        {
                            TempData["Error"] = "Failed to perform item issue.";
                            return RedirectToAction("ErrorPage");
                        }
                        await _faIssueByRequisitionService.InsertTranDTAsync(tranDt);
                    }
                }

                SystemTools.UpdateNextNumber(issueTranCode);
                TempData["Success"] = "Item Issue performed successfully!";
                return RedirectToAction("Detail", "FAIssueByRequisition", new { reqNo = ReqNo });
            }
            catch (Exception ex)
            {
                TempData["Error"] = "An error occurred while processing the request. Please try again.";
                return RedirectToAction("ErrorPage");
            }
        }


    }
}