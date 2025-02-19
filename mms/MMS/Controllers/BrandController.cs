using BusinessLayer.Interfaces;
using MMS.Models.Inventory;
using SharedLayer.AB_Common;
using SharedLayer.Models;
using SharedLayer.Models.Inventory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace MMS.Controllers
{
    public class BrandController : BaseController
    {
        private readonly IBrandService _brandsService;
        private readonly IItemService _itemsService;
        private readonly ICategoryService _categoryService;
        private IIpAddressService _ipAddressService;
        SystemTools _SystemTools = new SystemTools();
        private readonly ILogHistoryService _logService;

        public BrandController(ILogHistoryService logService, IItemService itemsService, ICategoryService categoryService, IIpAddressService ipAddressService, IBrandService brandsService) : base(logService)
        {
            _itemsService = itemsService;
            _categoryService = categoryService;
            _ipAddressService = ipAddressService;
            _brandsService = brandsService;
        }

        // List all items
        public async Task<ActionResult> Index()
        {
            try
            {
                var brands = await _brandsService.GetAllBrandAsync();

                var brandVMs = new List<ItemBrandVM>();

                foreach (var brand in brands)
                {
                    var productDesc = await _itemsService.GetItemByCodeAsync(brand.MainItem);

                    var brandVM = new ItemBrandVM
                    {
                        ItemBrandId = brand.ItemBrandId,
                        Brand = brand.Brand,
                        Type = brand.Type,
                        MainItem = brand.MainItem,
                        ProdDesc = productDesc?.ProdDesc 
                    };

                    brandVMs.Add(brandVM);
                }

                return View(brandVMs);
            }
            catch (Exception ex)
            {
                LogException(ex, "Error while fetching items.");
                TempData["Error"] = "An error occurred while fetching items.";
                return RedirectToAction("Error", "Home");
            }
        }

        public async Task<ActionResult> AddBrand()
        {
            try
            {
                ViewBag.Item = new SelectList(_SystemTools.GetItems(), "ItemID", "Description");
                ViewBag.MainItem = new SelectList(await _itemsService.GetAllItemsAsync(), "ProdCode", "ProdDesc");

                return View(new ItemBrandVM());
            }
            catch (Exception ex)
            {
                LogException(ex, "Error while preparing the create form.");
                TempData["Error"] = "An error occurred while preparing the create form.";
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        public async Task<ActionResult> AddBrand(ItemBrandVM model)
        {
            try
            {
                var brand = await _brandsService.GetAllBrandAsync();

                var crtws = _ipAddressService.GetIpAddress();
                var itembrand = new tblItemBrand
                {
                    ItemBrandId = model.ItemBrandId,
                    Brand = model.Brand,
                    Type = model.Type,
                    MainItem = model.MainItem,
                    CrtDt = DateTime.Today,
                    CrtBy = Session["UserName"].ToString(),
                    CrtWs = crtws,
                };

                _brandsService.AddBrand(itembrand);

                string logMessage = $"A new item was created by '{Session["UserName"]}':\n" +
                                    $"- ItemBrandId: '{model.ItemBrandId}'\n" +
                                    $"- Description: '{model.Brand}'\n" +
                                    $"- Type: '{model.Type}'\n" +
                                    $"- CrtDt: {DateTime.Today}\n";

                await LogActionAsync(model.MainItem, "Create Item Brand", logMessage, Session["UserName"].ToString());

                TempData["Success"] = "Item Brand created successfully.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {

                LogException(ex, "Error while creating the Brand.");
                TempData["Error"] = "An error occurred while creating the Brand.";
                return View("AddBrand", model);
            }
        }

        public async Task<ActionResult> EditBrand(int ItemBrandId)
        {
            try
            {

                var brand = await _brandsService.GetItemBrandByCodeAsync(ItemBrandId);
                var item = await _itemsService.GetItemByCodeAsync(brand.MainItem);
                var ItemList = _SystemTools.GetItems();
                ViewBag.Item = new SelectList(ItemList, "ItemID", "Description");
                ViewBag.MainItem = new SelectList(await _itemsService.GetAllItemsAsync(), "ProdCode", "ProdDesc", item.ProdDesc);

                var model = new ItemBrandVM
                {
                    ItemBrandId = brand.ItemBrandId,
                    Brand = brand.Brand,
                    Type = brand.Type,
                    MainItem = item.ProdDesc
                };

                return View(model);
            }
            catch (Exception ex)
            {
                LogException(ex, "Error while loading the itemBrand for editing.");
                TempData["Error"] = "An error occurred while loading the itemBrand.";
                return RedirectToAction("Index");
            }
        }

        [HttpPost]

        public async Task<ActionResult> EditBrand(ItemBrandVM model)
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
                var brand = await _brandsService.GetItemBrandByCodeAsync(model.ItemBrandId);
                var item = await _itemsService.GetItemByCodeAsync(brand.MainItem);
                var changes = new List<string>();

                if (brand.Brand != model.Brand)
                {
                    changes.Add($"Brand changed from '{brand.Brand}' to '{model.Brand}'");
                    brand.Brand = model.Brand;
                }

                if (brand.Type != model.Type)
                {
                    changes.Add($"Type changed from '{brand.Type}' to '{model.Type}'");
                    brand.Type = model.Type;
                }

                if (item.ProdCode != model.MainItem)
                {
                    changes.Add($"MainItem changed from '{item.ProdCode}' to '{model.MainItem}'");
                    item.ProdCode = model.MainItem;
                }

                await _brandsService.UpdateItemBrandAsync(brand);

                if (changes.Any())
                {
                    string logMessage = $"Item '{brand.ItemBrandId}' updated by '{Session["UserName"]}' on {DateTime.Now}\n" +
                                        "Changes made:\n" +
                                        string.Join("\n", changes);
                    await LogActionAsync(brand.Brand, "Edit brand", logMessage, Session["UserName"].ToString());
                }

                TempData["Success"] = "brand updated successfully.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                LogException(ex, "Error while updating the brand.");
                TempData["Error"] = "An error occurred while updating the brand.";
                return View(model);
            }
        }



        private void LogException(Exception ex, string message)
        {
            Log.Error(message, ex);
        }
    }
}