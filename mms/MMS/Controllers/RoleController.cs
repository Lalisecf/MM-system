using BusinessLayer.Interfaces;
using MMS.Controllers;
using MMS.Models.Inventory;
using SharedLayer.Models;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace ManageRoles.Controllers
{
    public class RoleController : BaseController
    {
        private readonly IRoleService _roleService;
        private readonly IApplicationService _applicationService;


        public RoleController(IRoleService roleService, IApplicationService applicationService, ILogHistoryService logService)
            : base(logService)
        {
            _roleService = roleService;
            _applicationService = applicationService;
        }

        // GET: Role/Index
        public async Task<ActionResult> Index()
        {
            try
            {
                var roles = await _roleService.GetAllRolesAsync();
                var roleViewModels = roles.Select(r => new MstRoleVM
                {
                    RoleID = r.RoleID,
                    RoleName = r.RoleName,
                    ApplicationName = r.Applications?.Description ?? "N/A",
                    ForUnitTypeName = r.ForUnitTypes?.Name ?? "N/A",
                    Status = r.Status
                }).ToList();
                return View(roleViewModels);
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                TempData["Error"] = "An error occurred while loading roles.";
                return RedirectToAction("Index");
            }
        }
        public async Task<ActionResult> Create()
        {
            try
            {
                ViewBag.Applications = new SelectList(await _applicationService.GetAllApplicationsAsync(), "AppId", "Description");
                ViewBag.UnitTypes = new SelectList(await _applicationService.GetAllWorkUnitAsync(), "UnitTypeID", "Name");
                ViewBag.Statuses = new SelectList(new[]
                {
                    new { Value = true, Text = "Enable" },
                    new { Value = false, Text = "Disable" }
                }, "Value", "Text");

                return PartialView("_AddRoleModal", new MstRoleVM());
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                TempData["Error"] = "An error occurred while preparing the create form.";
                return RedirectToAction("Index");
            }
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(MstRoleVM role, string UserName)
        {
            try
            {
                var newRole = new MstRole
                {
                    RoleName = role.RoleName,
                    Application = role.Application,
                    ForUnitType = role.ForUnitType,
                    Status = role.Status
                };
                var checkrole = await _roleService.GetRoleByNameAsync(role.RoleName);

                if (checkrole != null)
                {
                    TempData["Error"] = "Role already Exists.";
                    return RedirectToAction("Index");
                }
                await _roleService.AddRoleAsync(newRole);

                // Log the new role creation details
                string logMessage = $"Created a new role: '{role.RoleName}'" +
                      $" | Application: '{(await _applicationService.GetApplicationByIdAsync(role.Application)).Description}'" +
                      $" | Work Unit Type: '{(await _applicationService.GetWorkUnitTypeByIdAsync(role.ForUnitType)).Name}'" +
                      $" | Status: '{(role.Status ? "Enabled" : "Disabled")}'" +
                      $" | Created by: '{UserName}'" +
                      $" | Date and Time: '{DateTime.Now}'";

                await LogActionAsync(newRole.RoleID.ToString(), "Create Role", logMessage, UserName);

                TempData["Success"] = "Role created successfully.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["Error"] = "An error occurred while creating the role.";
                Log.Error(ex.Message);
                return RedirectToAction("Index");
            }
        }

        [HttpGet]
        public async Task<ActionResult> Edit(int id)
        {
            try
            {
                var roles = await _roleService.GetRolesByIdsAsync(new int[] { id });
                var role = roles.FirstOrDefault();
                // Map entity to view model
                var roleViewModel = new MstRoleVM
                {
                    RoleID = role.RoleID,
                    RoleName = role.RoleName,
                    Application = role.Application ?? 0,
                    ForUnitType = role.ForUnitType ?? 0,
                    Status = role.Status
                };

                // Populate dropdowns
                ViewBag.Applications = new SelectList(await _applicationService.GetAllApplicationsAsync(), "AppId", "Description", role.Application);
                ViewBag.UnitTypes = new SelectList(await _applicationService.GetAllWorkUnitAsync(), "UnitTypeID", "Name", role.ForUnitType);
                ViewBag.Statuses = new SelectList(new[]
                {
                    new { Value = true, Text = "Enable" },
                    new { Value = false, Text = "Disable" }
                }, "Value", "Text", role.Status);

                return PartialView("_EditRoleModal", roleViewModel);
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                TempData["Error"] = "An error occurred while loading the role.";
                return View();
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(MstRoleVM role, string userName)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var existingRoles = await _roleService.GetRolesByIdsAsync(new int[] { role.RoleID });
                    var existingRole= existingRoles.FirstOrDefault();

                    var existingapplication = await _applicationService.GetApplicationByIdAsync(role.Application);
                    var existingforunittype = await _applicationService.GetWorkUnitTypeByIdAsync(role.ForUnitType);
                    if (existingRole.RoleName != role.RoleName)
                    {
                        var checkRole = await _roleService.GetRoleByNameAsync(role.RoleName);
                        if (checkRole != null)
                        {
                            TempData["Error"] = "Role name already exists.";
                            return RedirectToAction("Index");
                        }
                    }

                    if (existingRole != null)
                    {
                        // Compare existing values with new values for logging
                        string logMessage = $"Updated role: '{existingRole.RoleName}' to '{role.RoleName}'" +
                       $" | Application: '{existingapplication.Description}' → '{(await _applicationService.GetApplicationByIdAsync(role.Application)).Description}'" +
                       $" | Work Unit Type: '{existingforunittype.Name}' → '{(await _applicationService.GetWorkUnitTypeByIdAsync(role.ForUnitType)).Name}'" +
                       $" | Status: '{(existingRole.Status ? "Enabled" : "Disabled")}' → '{(role.Status ? "Enabled" : "Disabled")}'" +
                       $" | Updated by: '{userName}'" +
                       $" | Date and Time: '{DateTime.Now}'";

                        await LogActionAsync(
                            role.RoleID.ToString(),
                            "Edit Role",
                            logMessage,
                            userName
                        );

                        // Update role properties
                        existingRole.RoleName = role.RoleName;
                        existingRole.Application = role.Application;
                        existingRole.ForUnitType = role.ForUnitType;
                        existingRole.Status = role.Status;

                        await _roleService.UpdateRoleAsync(existingRole);
                    }

                    TempData["Success"] = "Role updated successfully.";
                    return RedirectToAction("Index");

                }
                catch (Exception ex)
                {
                    Log.Error(ex.Message);
                    TempData["Error"] = "An error occurred while updating the role.";
                }
            }

            // Reload dropdowns for invalid model
            ViewBag.Applications = new SelectList(await _applicationService.GetAllApplicationsAsync(), "AppId", "Description", role.Application);
            ViewBag.UnitTypes = new SelectList(await _applicationService.GetAllWorkUnitAsync(), "UnitTypeID", "Name", role.ForUnitType);
            ViewBag.Statuses = new SelectList(new[]
            {
                new { Value = true, Text = "Enable" },
                new { Value = false, Text = "Disable" }
            }, "Value", "Text", role.Status);

            return View(role);
        }


    }
}
