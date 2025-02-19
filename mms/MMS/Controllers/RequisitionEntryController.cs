using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.Optimization;
using BusinessLayer.Interfaces;
using BusinessLayer.Service;
using MMS.Models;
using MMS.Models.Inventory;
using SharedLayer.AB_Common;
using SharedLayer.Models;
using SharedLayer.Models.Inventory;
using static SharedLayer.AB_Common.SystemTools;

namespace MMS.Controllers
{
    public class RequisitionEntryController : BaseController
    {
        private readonly IRequestService _requestService;
        private IDepartmentService _departmentService;
        private IUserBranchService _userBranchService;
        private IMstUserService _userService;
        private IItemService _itemService;
        private IRequestDetailService _requestDetailService;
        private ICategoryService _categoryService;
        private IUserStoreService _userStoreService;
        SystemTools _SystemTools = new SystemTools();

        public RequisitionEntryController(IRequestService requestService, IUserBranchService userBranchService, IDepartmentService departmentService, IMstUserService userService, IItemService itemService, IRequestDetailService requestDetailService, ICategoryService categoryService, IUserStoreService userStoreService, ILogHistoryService logService)
            : base(logService)
        {
            _requestService = requestService;
            _departmentService = departmentService;
            _userBranchService = userBranchService;
            _userService = userService;
            _itemService = itemService;
            _requestDetailService = requestDetailService;
            _categoryService = categoryService;
            _userStoreService = userStoreService;
        }

        public async Task<ActionResult> Index()
        {
            try
            {
                if (!int.TryParse(Session["UserId"]?.ToString(), out int userId))
                {
                    TempData["Error"] = "User session has expired. Please log in again.";
                    return RedirectToAction("Login", "Login");
                }

                var requests = await _requestService.GetRequestsByMainPGAndUserIdAsync(userId);
                if (requests == null || !requests.Any())
                {
                    TempData["Error"] = "No requests found for the specified criteria.";
                    return View(new List<MMS.Models.Inventory.RequestVM>());
                }

                var user = await _userService.GetUserByIdAsync(userId);
                var userName = Session["UserName"]?.ToString();
                var branches = await _userBranchService.GetAssignedDeptRightsByUserIdAsync(userId);
                var branchNames = await _departmentService.GetDepartmentsByIdAsync(branches.Select(b => b.DeptCode).ToArray());
                var requestVMList = new List<MMS.Models.Inventory.RequestVM>();
                foreach (var r in requests)
                {
                    try
                    {
                        var departmentName = branchNames.FirstOrDefault(b => b.DeptCode == long.Parse(r.ReqDept))?.Name ?? "Unknown Department";
                        var category = await _categoryService.GetCategoryByIdAsync(r.BatchNo);
                        requestVMList.Add(new MMS.Models.Inventory.RequestVM
                        {
                            FullName = $"{user.FullName}",
                            ReqDate = r.ReqDate,
                            ReqNo = r.ReqNo,
                            BatchNo = category.CatDesc,
                            ReqDept = departmentName
                        });
                    }
                    catch (Exception ex)
                    {
                        TempData["Error"] = $"Error processing request: {ex.Message}";
                    }
                }

                return View(requestVMList);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"An error occurred: {ex.Message}";
                return View(new List<MMS.Models.Inventory.RequestVM>());
            }
        }

        public async Task<ActionResult> Create()
        {
            ViewBag.Category = new SelectList(_SystemTools.GetCategories(), "CatID", "Description");
            return PartialView("_AddRequestModal");
        }
        [HttpGet]
        public async Task<ActionResult> addItem(string reqNo)
        {
            if (string.IsNullOrWhiteSpace(reqNo))
            {
                TempData["Error"] = "ReqNo is required to add an item.";
                return RedirectToAction("Index");
            }

            try
            {
                var requests = new RequestVM
                {
                    ReqNo = reqNo
                };
                var request = await _requestService.GetRequestByNumberAsync(reqNo);
                if (request == null)
                {
                    TempData["Error"] = "Request not found.";
                    return RedirectToAction("Index");
                }

                var items = await _itemService.GetItemsByCatCodeAsync(request.BatchNo);
                ViewBag.Items = new SelectList(items, "ProdCode", "ProdDesc");

                return PartialView("_AddItemModal", requests);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "An error occurred while processing your request: " + ex.Message;
                return RedirectToAction("Index");
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(RequestVM request)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "There were errors in the form submission.";
                return RedirectToAction("Index");
            }

            if (request.CategoryType == 0)
            {
                TempData["Error"] = "Please select a valid category.";
                return RedirectToAction("Index");
            }
            try
            {
                int userId = Convert.ToInt32(Session["UserId"]);
                var branches = await _userBranchService.GetAssignedDeptRightsByUserIdAsync(userId);
                var branche = branches.FirstOrDefault();
                var deptCode = branche.DeptCode;
                var department = (await _departmentService.GetDepartmentsByIdAsync(new long[] { long.Parse(deptCode.ToString()) })).FirstOrDefault();
                bool claim = false;
                int catType = 0;
                string StoreCode = "STR-01";
                switch (request.CategoryType)
                {
                    case 1: claim = false; catType = 0; StoreCode = "STR-01"; break;
                    case 2: claim = true; catType = 1; StoreCode = "STR-02"; break;
                    case 3: claim = true; catType = 3; StoreCode = "STR-11"; break;
                    case 4: claim = false; catType = 5; StoreCode = "STR-12"; break;
                }
                request.ReqDateEth = EthiopianDateConverter.ConvertToEthiopianDate(DateTime.Now);
                var latestRequestNumber = int.Parse(await _requestService.GetLatestRequestNumberAsync());
                var reqNo = $"MR-{request.MainPG}-{latestRequestNumber + 1}";

                var model = new tblRequest
                {
                    ReqNo = reqNo,
                    ReqDept = deptCode.ToString(),
                    ReqDateEth = request.ReqDateEth,
                    ReqBy = userId,
                    ReqDate = DateTime.Now,
                    Reason = request.Reason,
                    Status = 1,
                    IsDocPrinted = false,
                    IsForBatch = false,
                    BatchNo = request.MainPG,
                    StoreCode = StoreCode,
                    Clamable = claim,
                    CatType = catType.ToString(),
                };

                if (await _requestService.RequestExistsAsync(model.ReqNo))
                {
                    TempData["Error"] = "The Request already exists.";
                    return RedirectToAction("Index");
                }
                await _requestService.SaveRequestAsync(model);
                string logMessage = $"A new Request was created by '{Session["UserName"]}' on {DateTime.Now}:\n" +
                                    $"- ReqNo: '{reqNo}'\n" +
                                    $"- BatchNo: '{request.BatchNo}'\n" +
                                    $"- ProdCode: '{request.ItemitemValue}'\n" +
                                    $"- CostCenter: '{department?.MainCostCenter}'\n" +
                                    $"- Request DeptCode: '{deptCode.ToString()}'\n";
                await LogActionAsync(reqNo, "Create Request", logMessage, Session["UserName"].ToString());
                TempData["Success"] = "Request created successfully!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["Error"] = "An error occurred while creating the Request: " + ex.Message;
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> addItem(RequestVM request)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "There were errors in the form submission.";
                return RedirectToAction("Index");
            }
            if (string.IsNullOrEmpty(request.ItemitemValue) && request.QtyReq > 0)
            {
                TempData["Error"] = "You entered a quantity but did not select an item.";
                return RedirectToAction("Index");
            }

            if (!string.IsNullOrEmpty(request.ItemitemValue) && request.QtyReq <= 0)
            {
                TempData["Error"] = "You selected an item but did not enter a quantity.";
                return RedirectToAction("Index");
            }

            try
            {
                var item = await _itemService.GetItemByCodeAsync(request.ItemitemValue);
                int userId = Convert.ToInt32(Session["UserId"]);
                var req = await _requestService.GetRequestByNumberAsync(request.ReqNo);
                var department = (await _departmentService.GetDepartmentsByIdAsync(new long[] { long.Parse(req.ReqDept) })).FirstOrDefault();
                var detail = new tblRequestDetail
                {
                    ReqNo = request.ReqNo,
                    QtyReq = request.QtyReq,
                    ProdCode = request.ItemitemValue,
                    CostCenter = department?.MainCostCenter,
                    UnitCost = item?.UnitCost ?? 0,
                    TotalCost = request.QtyReq * (item?.UnitCost ?? 0),
                    StockAcct = item?.StockAccountSegment,
                    CostAcct = item?.ExpenseAccountSegment,
                    QtyCancel = Convert.ToDecimal(request.QtyReq),
                    QtyIssued = 0,
                };

                await _requestDetailService.AddRequestDetailAsync(detail);
                string logMessage = $"A new Item request was created by '{Session["UserName"]}' on {DateTime.Now}:\n" +
                                    $"- ReqNo: '{request.ReqNo}'\n" +
                                    $"- QtyReq: '{request.QtyReq}'\n" +
                                    $"- UnitCost: '{item?.UnitCost ?? 0}'\n" +
                                    $"- TotalCost: '{request.QtyReq * (item?.UnitCost ?? 0)}'\n" +
                                    $"- StockAcct: '{item?.StockAccountSegment}'\n" +
                                    $"- CostAcct: '{item?.ExpenseAccountSegment}'\n" +
                                    $"- CostCenter: '{department?.MainCostCenter}'\n";
                await LogActionAsync(request.ReqNo, "Create Request", logMessage, Session["UserName"].ToString());
                TempData["Success"] = "Item request created successfully!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["Error"] = "An error occurred while creating the Request: " + ex.Message;
                return RedirectToAction("Index");
            }
        }
        public async Task<ActionResult> Detail(string reqNo)
        {
            ViewBag.ReqNo = reqNo;
            try
            {
                if (string.IsNullOrEmpty(reqNo))
                {
                    return Content("Request number is missing.", "text/plain");
                }
                var requestDetails = await _requestDetailService.GetRequestDetailsByReqNoAsync(reqNo);

                var requestVMList = new List<MMS.Models.Inventory.RquestDetailVM>();

                foreach (var detail in requestDetails)
                {
                    var item = await _itemService.GetItemByCodeAsync(detail.ProdCode);
                    requestVMList.Add(new MMS.Models.Inventory.RquestDetailVM
                    {
                        IdNo = detail.IdNo.ToString(),
                        ProdCode = item?.ProdDesc ?? "Unknown Product",
                        QtyReq = (int)detail.QtyReq,
                        Reason = detail.Requests.Reason,
                        ReqDept = detail.Requests.ReqDept,
                        ReqDateEth = detail.Requests.ReqDateEth,
                        BatchNo = detail.Requests.BatchNo,
                        StoreCode = detail.Requests.StoreCode,
                    });
                }
                return View("RequestDetails", requestVMList);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"An error occurred: {ex.Message}";
                return RedirectToAction("Index");
            }
        }
        [HttpPost]
        public async Task<ActionResult> Delete(string reqNo)
        {
            if (string.IsNullOrEmpty(reqNo))
            {
                TempData["Error"] = "ReqNo is Required for Delete Request";
                return RedirectToAction("Index");

            }

            try
            {
                var request = await _requestService.GetRequestByNumberAsync(reqNo);
                var success = await _requestService.DeleteRequestAsync(reqNo);
                string logMessage = $"A Request was Deleted by '{Session["UserName"]}' on {DateTime.Now}:\n" +
                                    $"- ReqNo: '{reqNo}'\n" +
                                    $"- BatchNo: '{request.BatchNo}'\n" +
                                    $"- Request DeptCode: '{request.ReqDept}'\n";
                await LogActionAsync(reqNo, "Delete Request", logMessage, Session["UserName"].ToString());
                if (success)
                {
                    TempData["Success"] = "Request Deleted successfully!";
                    return RedirectToAction("Index");
                }
                else
                {
                    TempData["Error"] = "Request not deleted";
                    return RedirectToAction("Index");
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = "An error occurred while Deleting the Request: " + ex.Message;

                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        public async Task<ActionResult> deleteDetail(string idNo)
        {
            if (string.IsNullOrWhiteSpace(idNo))
            {
                TempData["Error"] = "ID is required to delete the request detail.";
                return RedirectToAction("Index");
            }

            try
            {
                var reqDetail =await _requestDetailService.GetRequestDetailsByIdNoAsync(idNo);
                var success = await _requestDetailService.DeleteRequestDetailAsync(idNo);
                string logMessage = $"A Request Detail was Deleted by '{Session["UserName"]}' on {DateTime.Now}:\n" +
                          $"- IdNo: '{idNo}'\n" +
                          $"- QtyReq: '{reqDetail.QtyReq}'\n" +
                          $"- ProdCode: '{reqDetail.ProdCode}'\n";
                await LogActionAsync(idNo, "Delete Item Request", logMessage, Session["UserName"].ToString());
                if (success)
                {
                    TempData["Success"] = "Request detail deleted successfully!";
                    return RedirectToAction("Index");
                }
                else
                {
                    TempData["Error"] = "Request detail not deleted";
                    return RedirectToAction("Index");
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = "An error occurred while deleting the request detail: " + ex.Message;
            }

            return RedirectToAction("Index"); 
        }
        [HttpGet]
        public async Task<ActionResult> Edit(string reqNo)
        {
            if (string.IsNullOrEmpty(reqNo))
            {
                TempData["Error"] = "ReqNo is required for editing.";
                return RedirectToAction("Index");
            }

            var request = await _requestService.GetRequestByNumberAsync(reqNo);
            if (request == null)
            {
                TempData["Error"] = "Request not found.";
                return RedirectToAction("Index");
            }
            var requests = new RequestVM
            {
                Reason = request.Reason,
                ReqDate = request.ReqDate,
                ReqDept = request.ReqDept,
                ReqNo = request.ReqNo,
                StoreCode = request.StoreCode
            };

            int CategoryType = 0;
            if (request.Clamable == false && request.CatType == "0")
            {
                CategoryType = 1;
            }
            else if (request.Clamable == true && request.CatType == "1")
            {
                CategoryType = 2;
            }
            else if (request.Clamable == true && request.CatType == "3")
            {
                CategoryType = 3;
            }
            else if (request.Clamable == false && request.CatType == "5")
            {
                CategoryType = 4;
            }
            var categories = _SystemTools.GetCategories();
            foreach(var category in categories)
            {

            }
            ViewBag.Category = new SelectList(categories, "CatID", "Description", CategoryType);
            ViewBag.MainCategory = new SelectList(await _categoryService.GetAllCategoriesAsync(), "CatCode", "CatDesc", request.BatchNo);
            return PartialView("_EditRequestModal", requests);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(RequestVM model)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "There were errors in the form submission.";
                return RedirectToAction("Edit", new { reqNo = model.ReqNo });
            }

            var request = await _requestService.GetRequestByNumberAsync(model.ReqNo);
            if (request == null)
            {
                TempData["Error"] = "Request not found.";
                return RedirectToAction("Index");
            }

            bool claim = false;
            int catType = 0;
            string StoreCode = "STR-01";
            switch (model.CategoryType)
            {
                case 1: claim = false; catType = 0; StoreCode = "STR-01"; break;
                case 2: claim = true; catType = 1; StoreCode = "STR-02"; break;
                case 3: claim = true; catType = 3; StoreCode = "STR-11"; break;
                case 4: claim = false; catType = 5; StoreCode = "STR-12"; break;
            }

            var changes = new List<string>();

            if (request.Reason != model.Reason)
            {
                changes.Add($"Reason changed from '{request.Reason}' to '{model.Reason}'");
                request.Reason = model.Reason;
            }

            if (request.ReqDate !=DateTime.Now)
            {
                changes.Add($"ReqDate changed from '{request.ReqDate}' to '{DateTime.Now}'");
                request.ReqDate = DateTime.Now;
                request.ReqDateEth = EthiopianDateConverter.ConvertToEthiopianDate(DateTime.Now);
            }

            if (request.BatchNo != model.MainPG)
            {
                changes.Add($"BatchNo changed from '{request.BatchNo}' to '{model.MainPG}'");
                request.BatchNo = model.MainPG;
            }

            if (request.StoreCode != StoreCode)
            {
                changes.Add($"StoreCode changed from '{request.StoreCode}' to '{StoreCode}'");
                request.StoreCode = StoreCode;
            }

            if (request.CatType != catType.ToString())
            {
                changes.Add($"CatType changed from '{request.CatType}' to '{catType}'");
                request.CatType = catType.ToString();
            }

            if (request.Clamable != claim)
            {
                changes.Add($"Clamable changed from '{request.Clamable}' to '{claim}'");
                request.Clamable = claim;
            }
            try
            {
                await _requestService.UpdateRequestAsync(request);
                string logMessage = $"Request '{request.ReqNo}' updated by '{Session["UserName"]}' on {DateTime.Now}\n" +
                                    "Changes made:\n" +
                                    string.Join("\n", changes);

                await LogActionAsync(request.ReqNo, "Edit Request", logMessage, Session["UserName"].ToString());

                TempData["Success"] = "Request updated successfully.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["Error"] = "An error occurred while updating the Request: " + ex.Message;
                return RedirectToAction("Index");
            }
        }
        [HttpGet]
        public async Task<ActionResult> EditItem(string idNo)
        {
            if (string.IsNullOrEmpty(idNo))
            {
                TempData["Error"] = "IdNo is required for editing.";
                return RedirectToAction("Index");
            }

            var requestDetail = await _requestDetailService.GetRequestDetailsByIdNoAsync(idNo);
            if (requestDetail == null)
            {
                TempData["Error"] = "Request Detail not found.";
                return RedirectToAction("Index");
            }
            var requests = new RquestDetailVM
            {
                IdNo = requestDetail.IdNo.ToString(),
                QtyReq =(int) requestDetail.QtyReq,
            };
            var items = await _itemService.GetItemsByCatCodeAsync(requestDetail.Requests.BatchNo);
            ViewBag.Items = new SelectList(items, "ProdCode", "ProdDesc", requestDetail.ProdCode);
            return PartialView("_EditItemModal", requests);

        }
        [HttpPost]
        public async Task<ActionResult> EditItem(RquestDetailVM detail)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    TempData["Error"] = "There were errors in the form submission.";
                    return RedirectToAction("Index");
                }
                var requestDetail = await _requestDetailService.GetRequestDetailsByIdNoAsync(detail.IdNo);
                if (requestDetail == null)
                {
                    TempData["Error"] = "Request Detail not found.";
                    return RedirectToAction("Index");
                }
                var item = await _itemService.GetItemByCodeAsync(detail.ItemitemValue);
                var department = (await _departmentService.GetDepartmentsByIdAsync(new long[] { long.Parse(requestDetail.Requests.ReqDept) })).FirstOrDefault();
                var changes = new List<string>();
                if (requestDetail.QtyReq != detail.QtyReq)
                {
                    changes.Add($"QtyReq changed from '{requestDetail.QtyReq}' to '{detail.QtyReq}'");
                    requestDetail.QtyReq = detail.QtyReq;
                }
                if (requestDetail.ProdCode != detail.ItemitemValue)
                {
                    changes.Add($"ProdCode changed from '{requestDetail.ProdCode}' to '{detail.ItemitemValue}'");
                    requestDetail.ProdCode = detail.ItemitemValue;
                    requestDetail.CostCenter = department?.MainCostCenter;
                    requestDetail.UnitCost = item?.UnitCost ?? 0;
                    requestDetail.TotalCost = detail.QtyReq * (item?.UnitCost ?? 0);
                    requestDetail.StockAcct = item?.StockAccountSegment;
                    requestDetail.CostAcct = item?.ExpenseAccountSegment;
                    requestDetail.QtyCancel = Convert.ToDecimal(detail.QtyReq);
                }
                await _requestDetailService.UpdateRequestDetailAsync(requestDetail);
                string logMessage = $"Request Detail '{requestDetail.IdNo}' updated by '{Session["UserName"]}' on {DateTime.Now}\n" +
                                   "Changes made:\n" +
                                   string.Join("\n", changes);

                await LogActionAsync(detail.IdNo, "Edit Request Detail", logMessage, Session["UserName"].ToString());
                TempData["Success"] = "Request Detail updated successfully.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                TempData["Error"] = $"Error updating quantity: {ex.Message}";
                return RedirectToAction("Index");
            }
        }
    }
}
