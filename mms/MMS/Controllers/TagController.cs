using BusinessLayer.Interfaces;
using MMS.Models.Inventory;
using SharedLayer.AB_Common;
using SharedLayer.DTO;
using SharedLayer.Models;
using SharedLayer.Models.Inventory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace MMS.Controllers
{
    public class TagController : BaseController
    {
        private readonly ITagService _tagService;
        private readonly IItemService _itemService;
        private readonly ICommonService _commonService;
        private readonly ICategoryService _categoryService;
        private readonly IIpAddressService _ipadressService;
        private readonly IDepartmentService _departmentService;
        private readonly SystemTools _systemTools = new SystemTools();

        public TagController(ILogHistoryService logService, ICommonService commonService, ITagService tagService, IItemService itemService, ICategoryService categoryService, IIpAddressService ipAddressService, IDepartmentService departmentService) : base(logService)
        {
            _tagService = tagService;
            _commonService = commonService;
            _itemService = itemService;
            _categoryService = categoryService;
            _ipadressService = ipAddressService;
            _departmentService = departmentService;
        }

        // GET: tag
        public async Task<ActionResult> GetTagEntries()
        {
            try
            {
                var tagList = await _tagService.GetTagEntriesAsync();
                var tagVMList = tagList.Select(entry => new TagDto
                {
                    RefNo = entry.RefNo,
                    ReqNo = entry.ReqNo,
                    Name = entry.Name
                }).ToList();

                return View(tagVMList);
            }
            catch (Exception ex)
            {
                Log.Error($"Error in GetTagEntries: {ex.Message}", ex);
                return RedirectToAction("GetTagEntries");
            }
        }

        // GET: Journal/Details/{refNo}
        public async Task<ActionResult> GetTagDetails(string refNo)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(refNo))
                    return new HttpStatusCodeResult(400, "Invalid reference number.");

                var tagDetails = await _tagService.GetTagIssueEntriesAsync(refNo);
                if (tagDetails == null || !tagDetails.Any())
                {
                    TempData["Error"] = "No tag details found for the provided reference number.";
                    Log.Warn($"No tag entries found for RefNo: {refNo}");
                    return RedirectToAction("GetTagEntries");
                }
                var tagDetailsVM = tagDetails.Select(detail => new TagIssueListDto
                {
                    ProdCode = detail.ProdCode,
                    ProdDesc = detail.ProdDesc,
                    RefNo = detail.RefNo,
                    GRV = detail.GRV,
                    DepartmentName = detail.DepartmentName,
                    Qty = detail.Qty,
                    UnitCost = detail.UnitCost,
                    TotalAmount = detail.TotalAmount,
                    IssuedItems = detail.IssuedItems,
                    TaggedItems = detail.TaggedItems,
                    status = detail.status
                }).ToList();

                return View("GetTagDetails", tagDetailsVM);
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                return new HttpStatusCodeResult(500, "An error occurred while fetching tag details.");
            }
        }

        [HttpPost]
        public async Task<ActionResult> SaveTagTransaction(string refNo, TagIssueListDto[] tagIssueListDto)
        {
            try
            {
                var issueTranHdTask= await _commonService.GetTranHdByIdAsync(refNo);
                var issueTranHdYdt =await _commonService.GetTranHdYdtByIdAsync(refNo);
           
                var getTrandtCs = await _commonService.GetTranDtCsByIdAsync(refNo);
                var IssueTranHd = issueTranHdTask != null ? issueTranHdTask : new tblTranHd
                {
                    RefNo = issueTranHdYdt.RefNo,
                    Branch = issueTranHdYdt.Branch,
                    RefDate = (DateTime)(issueTranHdYdt.RefDate)
                };              

                foreach (var getTrandt in tagIssueListDto)
                {
                    var issued = getTrandt.IssuedItems;
                    var tagged = getTrandt.TaggedItems;
                    var grnNo = getTrandt.GRV;
                    var GetItem = await _itemService.GetItemByCodeAsync(getTrandt.ProdCode);
                    var Getcategory = await _categoryService.GetCategoryByIdAsync(GetItem.ProductGroup);
                    var storeCode = GetStoreCode(Getcategory.MainPG);

                    if (IssueTranHd == null || string.IsNullOrEmpty(IssueTranHd.Branch) || !long.TryParse(IssueTranHd.Branch, out long branchId))
                    {
                       
                    }

                    var departmentName = await _departmentService.GetDepartmentByIdAsync(Convert.ToInt64(IssueTranHd.Branch));
                    if (departmentName == null)
                    {
                      
                    }

                    var gettagprefix = _systemTools.GetTagPrefix(Getcategory.MainPG, departmentName.Name, GetItem.OldMainPG, GetItem.TagCode,getTrandt.ProdCode);
                    int tagsToAdd = issued - tagged;

                    for (int i = 0; i < tagsToAdd; i++)
                    {
                        var nextNumber = _systemTools.UpdateTagNextNumber(getTrandt.ProdCode);

                        var FaMaster = new tblFaMaster
                        {
                            MainPG = Getcategory.MainPG,
                            OldMainPG = GetItem.OldMainPG,
                            SerialNo = $"{GetItem.ProductGroup}-{nextNumber}",
                            TagNo = $"{gettagprefix}-{nextNumber}",
                            Branch = Convert.ToInt64(IssueTranHd.Branch),
                            AssetCode = getTrandt.ProdCode,
                            Quantity = 1,
                            DepartmentId = Convert.ToInt64(IssueTranHd.Branch),
                            UnitCost = getTrandt.UnitCost,
                            CostImpairment = 0.00000m,
                            DepPercent = Getcategory.FirstYearDep,
                            AssetDescription = GetItem.ProdDesc,
                            CategoryId = GetItem.ProductGroup,
                            GRNNo = grnNo,
                            NewlyAdded = true,
                            CrtBy = Session["UserName"].ToString(),
                            CrtDt = DateTime.Now,
                            CrtWs = _ipadressService.GetIpAddress(),
                            Period = _systemTools.GetCurrentPeriod(),
                            NewPeriod = _systemTools.GetCurrentPeriod(),
                            IsPaid = false,
                            FirstYearDep = Getcategory.FirstYearDep,
                            storeCode = storeCode,
                            IssueNo = refNo,
                            SivDate = IssueTranHd.RefDate,
                            AICUnitcost = getTrandtCs?.UnitCost ?? 0
                        };

                        await _tagService.AddFAMasterAsync(FaMaster);

                        string logSuccess = $"Tag transaction for RefNo '{refNo}', AssetCode '{FaMaster.AssetCode}' saved by '{Session["UserName"]}' on {DateTime.Now}\n" +
                                            $"SerialNo: {FaMaster.SerialNo}\n" +
                                            $"TagNo: {FaMaster.TagNo}";

                        await LogActionAsync(FaMaster.TagNo, "Tag Entry", logSuccess, Session["UserName"].ToString());
                        tagged++;
                    }
                }

                TempData["Success"] = $"{refNo} is Tagged Successfully";
                return RedirectToAction("GetTagEntries");
            }
            catch (Exception ex)
            {
                Log.Error($"Error while saving tag transaction for RefNo '{refNo}': {ex.Message}", ex);
                TempData["Error"] =ex.Message;
                return RedirectToAction("GetTagEntries");
            }
        }



        private string GetStoreCode(string mainPG)
        {
            if (new[] { "AB", "BF", "CP", "GX", "DE", "DT" }.Contains(mainPG))
            {
                return "STR-01";
            }
            else if (new[] { "OB", "OF", "OM" }.Contains(mainPG))
            {
                return "STR-11";
            }
            else
            {
                return null;
            }
        }
        private void LogException(Exception ex, string Success)
        {
            Log.Error(Success, ex);
            TempData["Error"] = Success;
        }
    }
}
