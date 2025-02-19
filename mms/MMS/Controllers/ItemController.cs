using BusinessLayer.Interfaces;
using SharedLayer.Models.Inventory;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using MMS.Models.Inventory;
using System.Collections.Generic;
using SharedLayer.AB_Common;

namespace MMS.Controllers
{
    public class ItemController : BaseController
    {
        private readonly IItemService _itemsService;
        private readonly ICommonService _commonService;
        private readonly ICategoryService _categoryService;
        private IIpAddressService _ipAddressService;
        SystemTools _SystemTools = new SystemTools();
        private readonly ILogHistoryService _logService;

        public ItemController(ILogHistoryService logService, IItemService itemsService, ICategoryService categoryService, IIpAddressService ipAddressService, ICommonService commonService) : base(logService)
        {
            _itemsService = itemsService;
            _categoryService = categoryService;
            _ipAddressService = ipAddressService;
            _commonService = commonService;
        }

        // List all items
        public async Task<ActionResult> Index(string selectedItem)
        {
            try
            {
                var items = _SystemTools.GetItems();
                ViewBag.Items = new SelectList(items, "ItemID", "Description");

                IEnumerable<ItemMasterVM> mainItemsVM = Enumerable.Empty<ItemMasterVM>();

                if (!string.IsNullOrEmpty(selectedItem))
                {
                    IEnumerable<tblItemMaster> mainItems = Enumerable.Empty<tblItemMaster>();
                    switch (selectedItem)
                    {
                        case "1":
                            mainItems = await _itemsService.GetFAMainItemsAsync();
                            break;
                        case "2":
                            mainItems = await _itemsService.GetStationaryMainItemsAsync();
                            break;
                        case "3":
                            mainItems = await _itemsService.GetOtherMainItemsAsync();
                            break;
                        case "4":
                            mainItems = await _itemsService.GetSoftwareMainItemsAsync();
                            break;
                        default:
                            break;
                    }
                    var categories = await _categoryService.GetAllCategoriesAsync();

                    mainItemsVM = mainItems.Select(item => new ItemMasterVM
                    {
                        MainPG = item.MainPG,
                        CatDesc = categories.FirstOrDefault(c => c.MainPG == item.MainPG)?.CatDesc,
                        ProdCode = item.ProdCode,
                        ProdDesc = item.ProdDesc,
                        ProductGroup = item.ProductGroup,
                        IsItemObsulte = item.IsItemObsulte,
                        Maximumlevel = item.Maximumlevel,
                        MinimumLevel = item.MinimumLevel,
                        ReorderLevel = item.ReorderLevel,
                        TagCode = item.TagCode,
                        UnitMeas = item.UnitMeas
                    });
                }

                //ViewBag.Category = new SelectList(_SystemTools.GetCategories(), "CatID", "Description");
                //ViewBag.MainCategory = new SelectList(await _categoryService.GetAllCategoriesAsync(), "CatCode", "CatDesc");
                //ViewBag.MainItem = new SelectList(await _itemsService.GetAllItemsAsync(), "ProdCode", "ProdDesc");

                return View(mainItemsVM);
            }
            catch (Exception ex)
            {
                LogException(ex, "Error while fetching items.");
                TempData["Error"] = "An error occurred while fetching items.";
                return RedirectToAction("Error", "Home");
            }
        }

        public async Task<ActionResult> PadSize()
        {
            try
            {
                var main = await _itemsService.GetAllItemsAsync();
                var pads = main.Select(r => new ItemMasterVM
                {
                    ProdCode = r.ProdCode,
                    ProdDesc = r.ProdDesc,
                    ProductGroup = r.ProductGroup,
                    NoPerPad = r.NoPerPad,
                }).ToList();
                return View(pads);
            }
            catch (Exception ex)
            {
                LogException(ex, "Error while fetching items.");
                TempData["Error"] = "An error occurred while fetching items.";
                return RedirectToAction("Error", "Home");
            }
        }
        public async Task<ActionResult> EditPadSize(string id)
        {
            try
            {
                var prodcode = await _itemsService.GetItemByCodeAsync(id);
                var model = new ItemMasterVM
                {
                    ProdCode = prodcode.ProdCode,
                    ProdDesc = prodcode.ProdDesc,
                    NoPerPad = prodcode.NoPerPad,
                };
                return PartialView("_EditPadSizeModal", model);
            }
            catch (Exception ex)
            {
                LogException(ex, "Error while loading the category for editing.");
                TempData["Error"] = "An error occurred while loading the category.";
                return RedirectToAction("Index");
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> EditPadSize(ItemMasterVM model)
        {
            try
            {
                var prodcodes = await _itemsService.GetItemByCodeAsync(model.ProdCode);
                var changes = new List<string>();

                if (prodcodes.NoPerPad != model.NoPerPad)
                {
                    changes.Add($"PadSize from '{prodcodes.NoPerPad}' to '{model.NoPerPad}'");
                    prodcodes.NoPerPad = model.NoPerPad;
                }

                await _itemsService.UpdateItemAsync(prodcodes);

                if (changes.Any())
                {
                    string logMessage = $"PadSize '{prodcodes.ProdCode}' updated by '{Session["UserName"]}' on {DateTime.Now}\n" +
                                        "Changes made:\n" +
                                        string.Join("\n", changes);
                    await LogActionAsync(prodcodes.ProdCode.ToString(), "Edit PadSize", logMessage, Session["UserName"].ToString());
                }

                TempData["Success"] = "Securable updated successfully.";
                return RedirectToAction("PadSize");
            }
            catch (Exception ex)
            {
                LogException(ex, "Error while updating the category.");
                TempData["Error"] = "An error occurred while updating the padsize.";
                return PartialView("_EditCategoryModal", model);
            }
        }
        public async Task<ActionResult> AddItem()
        {
            try
            {

                ViewBag.UnitMeas = new SelectList(await _itemsService.GetAllUnitMeasAsync(), "UnitMeas", "UnitMeas");
                ViewBag.OldMainPG = new SelectList(await _commonService.GetOldMainPG(), "OldMainPG", "OldMainPG");

                ViewBag.Category = new SelectList(_SystemTools.GetCategories(), "CatID", "Description");
                ViewBag.MainCategory = new SelectList(await _categoryService.GetAllCategoriesAsync(), "CatCode", "CatDesc");

                return View(new ItemMasterVM());
            }
            catch (Exception ex)
            {
                LogException(ex, "Error while preparing the create form.");
                TempData["Error"] = "An error occurred while preparing the create form.";
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        public async Task<ActionResult> AddItem(ItemMasterVM model)
        {
            try
            {
                var newProdCode = await _itemsService.GenerateNewItemCodeAsync(model.MainPG);
                var result = await _categoryService.GetCategoryByIdAsync(model.MainPG);
                //var result1 = await _itemsService.GetFaByCodeAsync(model.OldMainPG);
                var crtws = _ipAddressService.GetIpAddress();
                var item = new tblItemMaster
                {
                    ProdCode = newProdCode,
                    ProductGroup = model.MainPG,
                    ProdDesc = model.ProdDesc,
                    Clamable = result.Clamable,
                    CatType = result.CatType?.ToString(),
                    QtyOnHand = 0,
                    UnitCost = 0,
                    TotalCost = 0,
                    QtyOnOrder = 0,
                    InitialQty = 0,
                    InitialTotalCost = 0,
                    BBFQty = 0,
                    BBFTotalCost = 0,
                    ReserveQty = 0,
                    ReserveCost = 0,
                    QtyCounted = 0,
                    IsItemObsulte = model.IsItemObsulte,
                    OldCode = "",
                    LeadTime = 0,
                    UnitMeas = model.UnitMeas,
                    MainPG = result.MainPG,
                    OldMainPG = model.OldMainPG,
                    SafetyFactor = 0,
                    IncDemandPercent = 0,
                    IsVatable = true,
                    CrtDt = DateTime.Today,
                    CrtWs = crtws,
                    CrtBy = Session["UserName"].ToString(),
                    StockAccountSegment = result.StockAccountSegment,
                    PriceVarianceAccountSegment = result.StockAccountSegment,
                    ExpenseAccountSegment = result.ExpenseAccountSegment,
                    IFBExpenseAccount = result.IFBExpenseAccount,
                    
                    ReorderLevel = model.ReorderLevel,
                    TagCode = model.TagCode,
                    MinimumLevel = model.MinimumLevel,
                    Maximumlevel = model.Maximumlevel
                };

                _itemsService.AddItem(item);

                string logMessage = $"A new item was created by '{Session["UserName"]}':\n" +
                                    $"- ProdCode: '{newProdCode}'\n" +
                                    $"- Description: '{model.ProdDesc}'\n" +
                                    $"- MainPG: '{model.MainPG}'\n" +
                                    $"- Is Active: {model.IsItemObsulte}\n";

                await LogActionAsync(newProdCode, "Create Item", logMessage, Session["UserName"].ToString());

                TempData["Success"] = "Item created successfully.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {

                LogException(ex, "Error while creating the item.");
                TempData["Error"] = "An error occurred while creating the item.";
                return View("AddItem", model);
            }
        }

        public async Task<ActionResult> EditItem(string ProdCode)
        {
            try
            {
                var item = await _itemsService.GetItemByCodeAsync(ProdCode);
                var category = await _categoryService.GetCategoryByIdAsync(item.ProductGroup);
                var categories = _SystemTools.GetCategories();
                var categoryType = categories.FirstOrDefault()?.Description;


                ViewBag.Category = new SelectList(_SystemTools.GetCategories(), "CatID", "Description", categoryType);
                ViewBag.MainCategory = new SelectList(await _categoryService.GetAllCategoriesAsync(), "CatCode", "CatDesc", category.MainPG);

                var unitMeasList = await _itemsService.GetAllUnitMeasAsync();
                ViewBag.unitMeas = new SelectList(unitMeasList, "UnitMeas", "UnitMeas", item.UnitMeas);

                ViewBag.OldMainPG = new SelectList(await _commonService.GetOldMainPG(), "OldMainPG", "OldMainPG", item.OldMainPG);
                var model = new ItemMasterVM
                {
                    CategoryType = categoryType,
                    ProdCode = item.ProdCode,
                    ProdDesc = item.ProdDesc,
                    Maximumlevel = item.Maximumlevel,
                    MinimumLevel = item.MinimumLevel,
                    ProductGroup=item.ProductGroup,
                    ReorderLevel = item.ReorderLevel,
                    IsItemObsulte = item.IsItemObsulte,
                    UnitMeas = item.UnitMeas,
                    OldMainPG = item.OldMainPG,
                    TagCode = item.TagCode
                };

                return View(model);
            }
            catch (Exception ex)
            {
                LogException(ex, "Error while loading the item for editing.");
                TempData["Error"] = "An error occurred while loading the item.";
                return RedirectToAction("Index");
            }
        }

        [HttpPost]

        public async Task<ActionResult> EditItem(ItemMasterVM model)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Invalid data submitted.";
                foreach (var modelError in ModelState.Values.SelectMany(v => v.Errors))
                {
                    Console.WriteLine(modelError.ErrorMessage);
                }
                return View(model);
            }

            try
            {
                var item = await _itemsService.GetItemByCodeAsync(model.ProdCode);
                var changes = new List<string>();

                if (item.ProdDesc != model.ProdDesc)
                {
                    changes.Add($"ProdDesc changed from '{item.ProdDesc}' to '{model.ProdDesc}'");
                    item.ProdDesc = model.ProdDesc;
                }

                if (item.Maximumlevel != model.Maximumlevel)
                {
                    changes.Add($"Maximumlevel changed from '{item.Maximumlevel}' to '{model.Maximumlevel}'");
                    item.Maximumlevel = model.Maximumlevel;
                }

                if (item.MinimumLevel != model.MinimumLevel)
                {
                    changes.Add($"MinimumLevel changed from '{item.MinimumLevel}' to '{model.MinimumLevel}'");
                    item.MinimumLevel = model.MinimumLevel;
                }

                if (item.ReorderLevel != model.ReorderLevel)
                {
                    changes.Add($"ReorderLevel changed from '{item.ReorderLevel}' to '{model.ReorderLevel}'");
                    item.ReorderLevel = model.ReorderLevel;
                }


                if (item.IsItemObsulte != model.IsItemObsulte)
                {
                    changes.Add($"IsItemObsulte changed from '{item.IsItemObsulte}' to '{model.IsItemObsulte}'");
                    item.IsItemObsulte = model.IsItemObsulte;
                }
                if (item.TagCode != model.TagCode)
                {
                    changes.Add($"TagCode changed from '{item.TagCode}' to '{model.TagCode}'");
                    item.TagCode = model.TagCode;
                }

                await _itemsService.UpdateItemAsync(item);

                if (changes.Any())
                {
                    string logMessage = $"Item '{item.ProdCode}' updated by '{Session["UserName"]}' on {DateTime.Now}\n" +
                                        "Changes made:\n" +
                                        string.Join("\n", changes);
                    await LogActionAsync(item.ProdCode.ToString(), "Edit Item", logMessage, Session["UserName"].ToString());
                }

                TempData["Success"] = "Item updated successfully.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                LogException(ex, "Error while updating the item.");
                TempData["Error"] = "An error occurred while updating the item.";
                return View(model);
            }
        }



        public async Task<JsonResult> GetMainItems(int? itemType)
        {
            try
            {
                IEnumerable<SelectListItem> mainItems;

                switch (itemType)
                {
                    case 1:
                        mainItems = (await _itemsService.GetFAMainItemsAsync())
                            .Select(i => new SelectListItem { Value = i.ProdCode.ToString(), Text = i.ProdDesc });
                        break;
                    case 2:
                        mainItems = (await _itemsService.GetStationaryMainItemsAsync())
                            .Select(i => new SelectListItem { Value = i.ProdCode.ToString(), Text = i.ProdDesc });
                        break;
                    case 3:
                        mainItems = (await _itemsService.GetOtherMainItemsAsync())
                            .Select(i => new SelectListItem { Value = i.ProdCode.ToString(), Text = i.ProdDesc });
                        break;
                    case 4:
                        mainItems = (await _itemsService.GetSoftwareMainItemsAsync())
                            .Select(i => new SelectListItem { Value = i.ProdCode.ToString(), Text = i.ProdDesc });
                        break;
                    default:
                        mainItems = Enumerable.Empty<SelectListItem>();
                        break;
                }

                return Json(mainItems, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error fetching Items" });
            }
        }

        public async Task<JsonResult> GetMainCategories(int categoryType)
        {
            try
            {
                IEnumerable<SelectListItem> mainCategories;

                switch (categoryType)
                {
                    case 1:
                        mainCategories = (await _categoryService.GetFAMainCategoriesAsync())
                            .Select(c => new SelectListItem { Value = c.CatCode.ToString(), Text = c.CatDesc });
                        break;
                    case 2:
                        mainCategories = (await _categoryService.GetStationaryMainCategoriesAsync())
                            .Select(c => new SelectListItem { Value = c.CatCode.ToString(), Text = c.CatDesc });
                        break;
                    case 3:
                        mainCategories = (await _categoryService.GetOtherMainCategoriesAsync())
                            .Select(c => new SelectListItem { Value = c.CatCode.ToString(), Text = c.CatDesc });
                        break;
                    case 4:
                        mainCategories = (await _categoryService.GetSoftwareMainCategoriesAsync())
                            .Select(c => new SelectListItem { Value = c.CatCode.ToString(), Text = c.CatDesc });
                        break;
                    default:
                        mainCategories = Enumerable.Empty<SelectListItem>();
                        break;
                }

                return Json(mainCategories, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error fetching categories" });
            }
        }

        public async Task<JsonResult> GetItemsByCatCode(string CatCode)
        {
            try
            {
                IEnumerable<SelectListItem> mainItems = (await _itemsService.GetItemsByCatCodeAsync(CatCode))
                            .Select(i => new SelectListItem { Value = i.ProdCode.ToString(), Text = i.ProdDesc });
                return Json(mainItems, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error fetching Items" });
            }
        }

        private void LogException(Exception ex, string message)
        {
            TempData["Error"] = message;

        }
    }
}
