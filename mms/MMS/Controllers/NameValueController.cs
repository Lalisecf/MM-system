using BusinessLayer.Interfaces; // Your business layer interface
using SharedLayer.Models.Inventory; // Your model
using System.Collections.Generic;
using System.Web.Mvc;

namespace MMS.Controllers
{
    public class NameValueController : Controller
    {
        private readonly INameValueService _service;

        // Injecting the service that will interact with the data layer
        public NameValueController(INameValueService service)
        {
            _service = service;
        }

        // GET: Index - Display all parameters
        public ActionResult Index()
        {
            // Get all parameters from the service
            IEnumerable<tblNameValue> nameValues = _service.GetAll();
            return View(nameValues);
        }

        // GET: Create - Show the form to add a new parameter
        public ActionResult Create()
        {
            return View();
        }

        // POST: Create - Handle the creation of a new parameter
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(tblNameValue nameValue)
        {
            if (ModelState.IsValid)
            {
                // Save the new parameter
                _service.Insert(nameValue);
                TempData["SuccessMessage"] = "Parameter added successfully!";
                return RedirectToAction("Index");
            }

            TempData["ErrorMessage"] = "There was an error. Please try again.";
            return View(nameValue);
        }

        // GET: Edit - Show the form to edit an existing parameter
        public ActionResult Edit(string id)
        {
            var nameValue = _service.GetByParameterName(id);
            if (nameValue == null)
            {
                return HttpNotFound();
            }

            return View(nameValue);
        }

        // POST: Edit - Handle the update of an existing parameter
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(tblNameValue nameValue)
        {
            if (ModelState.IsValid)
            {
                // Update the parameter
                _service.Update(nameValue);
                TempData["SuccessMessage"] = "Parameter updated successfully!";
                return RedirectToAction("Index");
            }

            TempData["ErrorMessage"] = "There was an error. Please try again.";
            return View(nameValue);
        }
    }
}
