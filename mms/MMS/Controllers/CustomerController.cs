using BusinessLayer.Interfaces;
using MMS.Models.Inventory;
using SharedLayer.Models;
using SharedLayer.Models.Inventory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace MMS.Controllers
{
    public class CustomerController : BaseController
    {
        private readonly ICustomerService _customerService;

        public CustomerController(ILogHistoryService logService, ICustomerService customerService) : base(logService)
        {
            _customerService = customerService;
        }

        // GET: Customer
        public async Task<ActionResult> Index()
        {
            try
            {
                var customers = await _customerService.GetAllCustomersAsync();
                return View(customers);
            }
            catch (Exception ex)
            {
                LogException(ex, "Error while fetching customers.");
                TempData["Error"] = "An error occurred while fetching customers.";
                return RedirectToAction("Error", "Home");
            }
        }

        // GET: Create Customer
        public async Task<ActionResult> Create()
        {
            try
            {
                ViewBag.SupplierCategory = new SelectList(await _customerService.GetAllCustomerCategoriesAsync(), "CategoryId", "CategoryName");
                return PartialView("_AddCustomerModal", new CustomerMasterVM());
            }
            catch (Exception ex)
            {
                LogException(ex, "Error while preparing the create form.");
                TempData["Error"] = "An error occurred while preparing the create form.";
                return RedirectToAction("Index");
            }
        }

        // POST: Create Customer
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(CustomerMasterVM model)
        {
            try
            {
                string nextSupplierCode = await _customerService.GenerateNewSupplierCodeAsync();
                model.customerNumber = nextSupplierCode;
                var supplier = new CustomerMaster
                {
                    customerNumber = nextSupplierCode,
                    SupplierType = model.SupplierType,
                    customerName = model.customerName,
                    isActive = model.IsActive,
                    IsWihholding = model.IsWithholding,
                    AccountNo = model.AccountNo,
                    Category = model.Category,
                };
                await _customerService.AddCustomerAsync(supplier);

                string logMessage = $"A new Supplier was created by '{Session["UserName"]}':\n" +
                                    $"- SupplierName: '{model.customerName}'\n" +
                                    $"- SupplierCode: '{model.customerNumber}'\n";

                await LogActionAsync(model.customerNumber.ToString(), "Add Supplier", logMessage,  Session["UserName"].ToString());

                TempData["Success"] = "Customer added successfully.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                LogException(ex, "Error while creating the customer.");
                TempData["Error"] = "An error occurred while creating the customer.";
                return View(model);
            }
        }


        // GET: Edit Customer
        public async Task<ActionResult> Edit(string id)
        {
            if (string.IsNullOrEmpty(id))
                return HttpNotFound();

            try
            {
                var customer = await _customerService.GetCustomerByIdAsync(id);
                var model = new CustomerMasterVM
                {
                    customerName = customer.customerName,
                    SupplierType = customer.SupplierType,
                    AccountNo = customer.AccountNo,
                    IsActive = customer.isActive,
                    IsWithholding = customer.IsWihholding,
                };
                ViewBag.SupplierCategory = new SelectList(await _customerService.GetAllCustomerCategoriesAsync(), "CategoryId", "CategoryName", customer.Category);
                return PartialView("_EditCustomerModal", model);
            }
            catch (Exception ex)
            {
                LogException(ex, "Error while loading the customer for editing.");
                TempData["Error"] = "An error occurred while loading the customer.";
                return RedirectToAction("Index");
            }
        }

        // POST: Edit Customer
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(CustomerMaster model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var customer = await _customerService.GetCustomerByIdAsync(model.customerNumber);
            if (customer == null)
                return HttpNotFound();

            var changes = new List<string>();

            // Compare and log changes
            if (customer.customerName != model.customerName)
            {
                changes.Add($"Name changed from '{customer.customerName}' to '{model.customerName}'");
                customer.customerName = model.customerName;
            }

            if (customer.email != model.email)
            {
                changes.Add($"email changed from '{customer.email}' to '{model.email}'");
                customer.email = model.email;
            }

           
            // If there are changes, log them
            if (changes.Any())
            {
                string logMessage = $"Supplier '{customer.customerNumber}' updated by '{Session["UserName"]}' on {DateTime.Now}\n" +
                                    "Changes made:\n" +
                                    string.Join("\n", changes);
                await LogActionAsync(customer.customerNumber.ToString(), "Edit Supplier", logMessage, Session["UserName"].ToString());
            }

            await _customerService.UpdateCustomerAsync(customer);
            TempData["Success"] = "Supplier updated successfully.";
            return RedirectToAction("Index");
        }


        // GET: Delete Customer
        public async Task<ActionResult> Delete(string id)
        {
            if (string.IsNullOrEmpty(id))
                return HttpNotFound();

            try
            {
                var customer = await _customerService.GetCustomerByIdAsync(id);
                if (customer == null)
                    return HttpNotFound();

                return View(customer);
            }
            catch (Exception ex)
            {
                LogException(ex, "Error while loading the customer for deletion.");
                TempData["Error"] = "An error occurred while loading the customer.";
                return RedirectToAction("Index");
            }
        }

        // POST: Confirm Delete Customer
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ConfirmDelete(string id)
        {
            if (string.IsNullOrEmpty(id))
                return HttpNotFound();

            try
            {
                await _customerService.DeleteCustomerAsync(id);
                TempData["Success"] = "Customer deleted successfully.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                LogException(ex, "Error while deleting the customer.");
                TempData["Error"] = "An error occurred while deleting the customer.";
                return RedirectToAction("Index");
            }
        }

        private void LogException(Exception ex, string message)
        {
            TempData["Error"] = message;
            Log.Error(message, ex);
        }
    }
}
