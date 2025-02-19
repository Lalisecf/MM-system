using BusinessLayer.Interfaces;
using System;
using System.Linq;
using System.Web.Mvc;
using MMS.Models.Inventory;
using SharedLayer.Models.Inventory;
using System.Collections.Generic;
using System.Threading.Tasks;
using SharedLayer.AB_Common;
using SharedLayer.Models;

namespace MMS.Controllers
{
    public class CategoryController : BaseController
    {
        private readonly ICategoryService _categoryService;
        SystemTools _SystemTools = new SystemTools();
        public CategoryController(ILogHistoryService logService, ICategoryService categoryService) : base(logService)
        {
            _categoryService = categoryService;
        }
        
        public async Task<ActionResult> Index()
        {
            try
            {
                var categories = await _categoryService.GetAllCategoriesAsync();
                return View(categories);
            }
            catch (Exception ex)
            {
                LogException(ex, "Error while fetching categories.");
                TempData["Error"] = "An error occurred while fetching categories.";
                return RedirectToAction("Error", "Home");
            }
        }

        public async Task<ActionResult> Create()
        {
            try
            {
                ViewBag.Category = new SelectList(_SystemTools.GetCategories(), "CatID", "Description");
                ViewBag.MainCategory = new SelectList(await _categoryService.GetAllCategoriesAsync(), "CatCode", "CatDesc");
                return PartialView("_AddCategoryModal", new CategoryVM());
            }
            catch (Exception ex)
            {
                LogException(ex, "Error while preparing the create form.");
                TempData["Error"] = "An error occurred while preparing the create form.";
                return RedirectToAction("Index"); 
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(CategoryVM model)
        {
            try
            {
                var newCatCode = await _categoryService.GenerateNewCatCodeAsync(model.MainPG);
                var result = await _categoryService.GetCategoryByIdAsync(model.MainPG);
                if (result.CatDesc == model.CatDesc)
                {
                    TempData["Error"] = "This Category is already exists";
                    return RedirectToAction("Index");
                }
                var lastdigit = await _categoryService.GetLastIncrementNumAsync();
                bool clame = false;
                int catype = 0;
                if (model.CategoryType == 1)
                {
                    clame = false;
                    catype = 0;
                }
                else if (model.CategoryType == 2)
                {
                    clame = true;
                    catype = 1;
                }
                else if (model.CategoryType == 3)
                {
                    clame = true;
                    catype = 3;
                }
                else if (model.CategoryType == 4)
                {
                    clame = false;
                    catype = 5;
                }

                var category = new tblCategory
                {
                    CatCode = newCatCode,
                    CatDepAcct = result.CatDepAcct,
                    CatDepExpAcct = result.CatDepExpAcct,
                    CatDesc = model.CatDesc,
                    Clamable = clame,
                    StockAccountSegment = result.StockAccountSegment,
                    ExpenseAccountSegment = result.ExpenseAccountSegment,
                    IFBExpenseAccount = result.IFBExpenseAccount,
                    FirstYearDep = Convert.ToDecimal(result.FirstYearDep),
                    IsPool = false,
                    MainPG = result.MainPG,
                    Levels = (result.Levels + 1),
                    Parent = result.Parent,
                    YearAfterDep = result.YearAfterDep,
                    Prefix = result.CatCode,
                    UsefullLife = result.UsefullLife,
                    IsActive = model.IsActive,
                    IncNum = lastdigit + 1,
                    CatType = catype,
                    Type = result.Type,
                };

                _categoryService.AddCategory(category);
                string logMessage = $"A new category was created by '{Session["UserName"]}':\n" +
                           $"- CatCode: '{newCatCode}'\n" +
                           $"- Description: '{model.CatDesc}'\n" +
                           $"- MainPG: '{model.MainPG}'\n" +
                           $"- Is Active: {model.IsActive}\n";

                await LogActionAsync(newCatCode, "Create Category", logMessage, Session["UserName"].ToString());

                TempData["Success"] = "Category created successfully.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                LogException(ex, "Error while creating the category.");
                TempData["Error"] = "An error occurred while creating the category.";
                return PartialView("_AddCategoryModal", model);
            }
        }
        public async Task<ActionResult> Edit(string id)
        {
            try
            {
                var category = await _categoryService.GetCategoryByIdAsync(id);
                ViewBag.Category = new SelectList(_SystemTools.GetCategories(), "CatID", "Description");
                ViewBag.MainCategory = new SelectList(await _categoryService.GetAllCategoriesAsync(), "CatCode", "CatDesc", category.MainPG);

                var model = new CategoryVM
                {
                    CatCode = category.CatCode,
                    CatDesc = category.CatDesc,
                    StockAccountSegment = category.StockAccountSegment,
                    ExpenseAccountSegment = category.ExpenseAccountSegment,
                    IFBExpenseAccount = category.IFBExpenseAccount,
                    UsefullLife = category.UsefullLife,
                    IsActive = category.IsActive
                };

                return PartialView("_EditCategoryModal", model);
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
        public async Task<ActionResult> Edit(CategoryVM model)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Invalid data submitted.";
                return PartialView("_EditCategoryModal", model);
            }

            try
            {
                var category = await _categoryService.GetCategoryByIdAsync(model.CatCode);
                var changes = new List<string>();

                if (category.CatDesc != model.CatDesc)
                {
                    changes.Add($"CatDesc changed from '{category.CatDesc}' to '{model.CatDesc}'");
                    category.CatDesc = model.CatDesc;
                }
                if (category.StockAccountSegment != model.StockAccountSegment)
                {
                    changes.Add($"StockAccountSegment changed from '{category.StockAccountSegment}' to '{model.StockAccountSegment}'");
                    category.StockAccountSegment = model.StockAccountSegment;
                }
                if (category.ExpenseAccountSegment != model.ExpenseAccountSegment)
                {
                    changes.Add($"ExpenseAccountSegment changed from '{category.ExpenseAccountSegment}' to '{model.ExpenseAccountSegment}'");
                    category.ExpenseAccountSegment = model.ExpenseAccountSegment;
                }
                if (category.IFBExpenseAccount != model.IFBExpenseAccount)
                {
                    changes.Add($"IFBExpenseAccount changed from '{category.IFBExpenseAccount}' to '{model.IFBExpenseAccount}'");
                    category.IFBExpenseAccount = model.IFBExpenseAccount;
                }
                if (category.UsefullLife != model.UsefullLife)
                {
                    changes.Add($"UsefullLife changed from '{category.UsefullLife}' to '{model.UsefullLife}'");
                    category.UsefullLife = model.UsefullLife;
                }
                if (category.IsActive != model.IsActive)
                {
                    changes.Add($"IsActive changed from '{category.IsActive}' to '{model.IsActive}'");
                    category.IsActive = model.IsActive;
                }
                await _categoryService.UpdateCategoryAsync(category);

                if (changes.Any())
                {
                    string logMessage = $"Category '{category.CatCode}' updated by '{Session["UserName"]}' on {DateTime.Now}\n" +
                                        "Changes made:\n" +
                                        string.Join("\n", changes);
                    await LogActionAsync(category.CatCode.ToString(), "Edit Category", logMessage, Session["UserName"].ToString());
                }

                TempData["Success"] = "Category updated successfully.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                LogException(ex, "Error while updating the category.");
                TempData["Error"] = "An error occurred while updating the category.";
                return PartialView("_EditCategoryModal", model);
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

        private void LogException(Exception ex, string message)
        {
            Log.Error(message);
        }
    }
}
