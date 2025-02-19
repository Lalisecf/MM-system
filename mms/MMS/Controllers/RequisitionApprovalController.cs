using BusinessLayer.Interfaces;
using MMS.Models.Inventory;
using SharedLayer.Models.Inventory;
using SharedLayer.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using System;
using BusinessLayer.Service;
using BusinessLayer.Services;
using MMS.Controllers;
using static SharedLayer.AB_Common.SystemTools;
using MMS.Models;
using System.Diagnostics;
using System.Web.Security;

public class RequisitionApprovalController : BaseController
{
    private readonly IRequestService _requestService;
    private readonly IMstUserService _userService;
    private readonly IMstUserRoleService _userRoleService;
    private readonly IDepartmentService _departmentService;
    private readonly ICategoryService _categoryService;
    private readonly IRequestDetailService _requestDetailService;
    private readonly IItemService _itemService;

    public RequisitionApprovalController(IRequestService requestService, IMstUserService userService,
                              IDepartmentService departmentService, ICategoryService categoryService, IRequestDetailService requestDetailService, IItemService itemService, IMstUserRoleService userRoleService, ILogHistoryService logService)
            : base(logService)
    {
        _requestService = requestService;
        _userService = userService;
        _departmentService = departmentService;
        _categoryService = categoryService;
        _requestDetailService = requestDetailService;
        _itemService = itemService;
        _userRoleService = userRoleService;
    }

    public async Task<ActionResult> Index(int page = 1, string searchTerm = null, int pageSize = 100)
    {
        var appId = 0;
        try
        {
            if (!int.TryParse(Session["UserId"]?.ToString(), out int userId))
            {
                TempData["Error"] = "User session has expired. Please log in again.";
                return RedirectToAction("Login", "Login");
            }
            var roles = await _userRoleService.GetAssignedRolesByUserIdAsync(userId);
            if (roles != null && roles.Any(role => role.Roles?.RoleName == "Approver"))
            {
                appId = userId;
            }
            else
            {
                TempData["Error"] = "Does not have permission.";
                return RedirectToAction("Home", "Home");
            }

            int skip = (page - 1) * pageSize;
            var result = await _requestService.GetRequestsForApproveByUserId(appId, skip, pageSize, searchTerm);
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
                    UnitCost = detail.UnitCost,
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
    public async Task<ActionResult> RejectRequest(string reqNo)
    {
        if (string.IsNullOrEmpty(reqNo))
        {
            TempData["Error"] = "ReqNo is Required for Reject Request";
            return RedirectToAction("Index");

        }

        try
        {
            var request = await _requestService.GetRequestByNumberAsync(reqNo);
            var success = await _requestService.DeleteRequestAsync(reqNo);
            string logMessage = $"A Request was Rejected by '{Session["UserName"]}' on {DateTime.Now}:\n" +
                                $"- ReqNo: '{reqNo}'\n" +
                                $"- BatchNo: '{request.BatchNo}'\n" +
                                $"- Request DeptCode: '{request.ReqDept}'\n";
            await LogActionAsync(reqNo, "Rejected Request", logMessage, Session["UserName"].ToString());
            if (success)
            {
                TempData["Message"] = "Request Rejected successfully!";
                _requestDetailService.RejectRequestDetailAsync(reqNo);
                return RedirectToAction("Index");
            }
            else
            {
                TempData["Error"] = "Request not Rejected";
                return RedirectToAction("Index");
            }
        }
        catch (Exception ex)
        {
            TempData["Error"] = "An error occurred while Deleting the Request: " + ex.Message;

            return RedirectToAction("Index");
        }
    }
    public async Task<ActionResult> DeleteRequestDetail(string idNo)
    {
        if (string.IsNullOrEmpty(idNo))
        {
            TempData["Error"] = "IdNo is Required for Delete Request";
            return View("RequestDetails");
        }
        string reqNo = Request.QueryString["reqNo"];
        var reqDetail = await _requestDetailService.GetRequestDetailsByIdNoAsync(idNo);
        if (string.IsNullOrEmpty(reqNo))
            {
                reqNo = reqDetail.ReqNo;
            }
        try
        {
            var success = await _requestDetailService.DeleteRequestDetailAsync(idNo);

            string logMessage = $"A Request Detail was Deleted by '{Session["UserName"]}' on {DateTime.Now}:\n" +
                                $"- IdNo: '{idNo}'\n" +
                                $"- QtyReq: '{reqDetail.QtyReq}'\n" +
                                $"- ProdCode: '{reqDetail.ProdCode}'\n";

            await LogActionAsync(idNo, "Delete Item Request", logMessage, Session["UserName"].ToString());

            if (success)
            {
                TempData["Message"] = "Request detail deleted successfully!";
            }
            else
            {
                TempData["Error"] = "Request detail not deleted.";
            }
        }
        catch (Exception ex)
        {
            TempData["Error"] = "An error occurred while deleting the request detail: " + ex.Message;
        }
        return RedirectToAction("Detail", "RequisitionApproval", new { reqNo = reqNo });
    }
    [HttpPost]
    public async Task<ActionResult> UpdateQuantity(int idNo, int qtyReq)
    {
        string reqNo = Request.QueryString["reqNo"];
        var requestDetail = await _requestDetailService.GetRequestDetailsByIdNoAsync(idNo.ToString());
        if (string.IsNullOrEmpty(reqNo))
        {
            reqNo = requestDetail.ReqNo;
        }
        try
        {
            if (idNo <= 0 || qtyReq <= 0)
            {
                TempData["Error"] = "Invalid ID or quantity value. Please check your input.";
                return RedirectToAction("Detail", "RequisitionApproval", new { reqNo = reqNo });
            }
            if (requestDetail == null)
            {
                TempData["Error"] = $"Request Detail with ID '{idNo}' not found.";
                return RedirectToAction("Detail", "RequisitionApproval", new { reqNo = reqNo });
            }
            if (qtyReq == requestDetail.QtyReq)
            {
                TempData["Error"] = $"Quantity Requested equals to Quantity Approved,please enter correct approve required quantity.";
                return RedirectToAction("Detail", "RequisitionApproval", new { reqNo = reqNo });
            }
            var item = await _itemService.GetItemByCodeAsync(requestDetail.ProdCode);
            if (item == null)
            {
                TempData["Error"] = $"Item with code '{requestDetail.ProdCode}' not found.";
                return RedirectToAction("Detail", "RequisitionApproval", new { reqNo = reqNo });
            }

            var changes = new List<string>();
            if (requestDetail.QtyReq != qtyReq)
            {
                changes.Add($"QtyReq changed from '{requestDetail.QtyReq}' to '{qtyReq}'");
                requestDetail.QtyReq = qtyReq;
                requestDetail.TotalCost = qtyReq * (item?.UnitCost ?? 0);
                requestDetail.QtyCancel = Convert.ToDecimal(qtyReq);
                changes.Add($"TotalCost updated to '{requestDetail.TotalCost}'");
            }

            await _requestDetailService.UpdateRequestDetailAsync(requestDetail);

            try
            {
                string logMessage = $"Request Detail '{requestDetail.IdNo}' QtyReq amended by '{Session["UserName"]}' on {DateTime.Now}\n" +
                                    "Changes made:\n" +
                                    string.Join("\n", changes);
                await LogActionAsync(idNo.ToString(), "QtyReq Amendment", logMessage, Session["UserName"].ToString());
            }
            catch (Exception logEx)
            {
                Log.Error($"Logging failed: {logEx.Message}");
            }

            TempData["Message"] = "Request detail quantity updated successfully.";
            return RedirectToAction("Detail", "RequisitionApproval", new { reqNo = reqNo });
        }
        catch (Exception ex)
        {
            Log.Error($"Error updating quantity: {ex.Message}");
            TempData["Error"] = $"Error updating quantity: {ex.Message}";
            return RedirectToAction("Detail", "RequisitionApproval", new { reqNo = reqNo });
        }
    }


    [HttpPost]
    public async Task<ActionResult> ApproveRequest(string reqNo)
    {
        if (string.IsNullOrEmpty(reqNo))
        {
            TempData["Error"] = "ReqNo required for approve aRequest.";
            return RedirectToAction("Index");
        }

        var request = await _requestService.GetRequestByNumberAsync(reqNo);
        if (request == null)
        {
            TempData["Error"] = "Request not found.";
            return RedirectToAction("Index");
        }
        var changes = new List<string>();

        if (request != null)
        {
            int userId = Convert.ToInt32(Session["UserId"]);
            request.AppDate = DateTime.Now;
            request.AppDateEth = EthiopianDateConverter.ConvertToEthiopianDate(DateTime.Now);
            request.AppBy = userId;
            request.Status = 2;
        }
        try
        {
            await _requestService.UpdateRequestAsync(request);
            string logMessage = $"A Request was Approved by '{Session["UserName"]}' on {DateTime.Now}:\n" +
                                $"- ReqNo: '{reqNo}'\n" +
                                $"- BatchNo: '{request.BatchNo}'\n" +
                                $"- StoreCode: '{request.StoreCode}'\n" +
                                $"- Request DeptCode: '{request.ReqDept}'\n";

            await LogActionAsync(request.ReqNo, "Approve Request", logMessage, Session["UserName"].ToString());

            TempData["Message"] = "Request Approved successfully.";
            return RedirectToAction("Index");
        }
        catch (Exception ex)
        {
            TempData["Error"] = "An error occurred while updating the Request: " + ex.Message;
            return RedirectToAction("Index");
        }
    }
}
