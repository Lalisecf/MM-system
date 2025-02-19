using BusinessLayer.Interfaces;
using MMS.Models.Master;
using SharedLayer.AB_Common;
using SharedLayer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace MMS.Controllers
{
    public class UserController : BaseController
    {
        private readonly IMstUserService _userService;
        private readonly IMstUserRoleService _userRoleService;
        private readonly IRoleService _roleService;
        private readonly IDepartmentService _departmentService;
        private readonly IUserBranchService _userDeptService;
        private readonly IUserStoreService _userStoreService;
        private readonly IStoreService _storeService;

        public UserController(IMstUserService userService,IMstUserRoleService userRoleService,IRoleService roleService,
            IDepartmentService departmentService,
            IUserBranchService userDeptService,
            IUserStoreService userStoreService,
            IStoreService storeService,
            ILogHistoryService logService)
            : base(logService)        
        {            
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            _userRoleService = userRoleService ?? throw new ArgumentNullException(nameof(userRoleService));
            _roleService = roleService ?? throw new ArgumentNullException(nameof(roleService));
            _departmentService = departmentService ?? throw new ArgumentNullException(nameof(departmentService));
            _userDeptService = userDeptService ?? throw new ArgumentNullException(nameof(userDeptService));
            _userStoreService=userStoreService?? throw new ArgumentNullException(nameof(userStoreService));
            _storeService = storeService ?? throw new ArgumentNullException(nameof(storeService));
        }

        public async Task<ActionResult> Index()
        {
            try
            {
                var users = await _userService.GetAllUsersAsync();

                var viewModels = users.ToList();


                return View(viewModels);
            }
            catch (Exception ex)
            {
                LogError("Error loading users", ex);
                TempData["Error"] = "An error occurred while loading the users.";
                return View(new List<MstUsersVM>());
            }
        }


        public async Task<ActionResult> Create()
        {
            try
            {
                var stores = await _storeService.GetAllStoresAsync();
                ViewBag.Stores = new SelectList(stores, "StoreCode", "StoreName");
                await LoadRolesAndDepartmentsForViewBag();
                return PartialView("_AddUserModal", new MstUsersVM());
            }
            catch (Exception ex)
            {
                LogError("Error preparing create form", ex);
                TempData["Error"] = "An error occurred while preparing the create form.";
                return RedirectToAction("Index");
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(MstUser user, int[] roleIds, long[] DeptCodeIds, string[] storeCodes)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    await LoadRolesAndDepartmentsForViewBag(roleIds, DeptCodeIds);
                    TempData["Error"] = "Invalid data. Please correct the errors and try again.";
                    return PartialView("_AddUserModal", user);
                }

                var roles = await _roleService.GetRolesByIdsAsync(roleIds);
                if (roles != null && roles.Any(role => role.RoleName == "Approver") && roles.Any(role => role.RoleName == "Requester"))
                {
                    TempData["Error"] = "Assigning both Approver and Requester roles to a user is forbidden.";
                    return RedirectToAction("Index");
                }

                if (await _userService.UserExistsAsync(user.UserName))
                {
                    TempData["Error"] = $"User with username '{user.UserName}' already exists.";
                    return RedirectToAction("Index");
                }
                if (await _userService.EmployeeIdExistsAsync(user.EmployeeId))
                {
                    TempData["Error"] = $"User with EmployeeId '{user.EmployeeId}' already exists.";
                    return RedirectToAction("Index");
                }

                user.CrtBy = Session["UserName"].ToString();
                await _userService.AddUserAsync(user);

                if (roleIds != null && roleIds.Any())
                {
                    await _userRoleService.AddRoleToUserAsync(user.UserId, roleIds);
                }
                if (storeCodes != null && storeCodes.Any())
                {
                     _userStoreService.AddUserStore(user.UserId.ToString(), storeCodes);
                }

                if (DeptCodeIds != null && DeptCodeIds.Any())
                {
                    await _userDeptService.AddDeptRightToUserAsync(user.UserId, DeptCodeIds);
                }
                string strMsg = "";
                var istrue = "and";
                string logMessage = $"A new user was created by '{Session["UserName"]}' on {DateTime.Now}:\n" +
                      $"- Username: '{user.UserName}'\n" +
                      $"- Full Name: '{user.FullName}'\n" +
                      $"- Employee ID: '{user.EmployeeId}'\n" +
                      $"- Is Active: {user.IsActive}\n" +
                      $"- Assigned Roles: {string.Join(", ", (await _roleService.GetRolesByIdsAsync(roleIds)).Select(r => r.RoleName))}\n" +
                      $"- Assigned Departments: {string.Join(", ", (await _departmentService.GetDepartmentsByIdAsync(DeptCodeIds)).Select(d => d.Name))}";
                if (storeCodes != null && storeCodes.Any())
                 {
                     logMessage = logMessage + $"- Assigned Stores: {string.Join(", ", (await _storeService.GetStoresByCodeAsync(storeCodes)).Select(s => s.StoreName))}";
                    strMsg = " and stores ";
                    istrue = ",";
                 }
             
              await LogActionAsync(user.UserId.ToString(), "Create User", logMessage, Session["UserName"].ToString());

                TempData["Success"] = $"User created, roles{istrue} departments {strMsg} assigned successfully.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                LogError("Error creating user", ex);
                await LoadRolesAndDepartmentsForViewBag(roleIds, DeptCodeIds);
                TempData["Error"] = "An error occurred while creating the user.";
                return RedirectToAction("Index");
            }
        }

        public async Task<ActionResult> Edit(int id)
        {
            try
            {
                var user = await _userService.GetUserByIdAsync(id);

                if (user == null)
                {
                    TempData["Error"] = "User not found.";
                    return RedirectToAction("Index");
                }

                var assignedStores = await _userStoreService.GetAssignedStoresByUserIdAsync(id.ToString());
                var storeCodes = assignedStores.Select(r => r.StoreCode).ToArray();
                await LoadStoresForViewBag(storeCodes);

                var assignedRoles = await _userRoleService.GetAssignedRolesByUserIdAsync(id);
                var assignedBranches = await _userDeptService.GetAssignedDeptRightsByUserIdAsync(id);

                // Combine both roles and departments into the ViewBag
                await LoadRolesAndDepartmentsForViewBag(
                    selectedRoleIds: assignedRoles.Select(r => r.RoleId).ToArray(),
                    selectedDeptCodeIds: assignedBranches.Select(b => b.DeptCode).ToArray()
                );

                // Prepare the user view model
                var viewModel = new MstUsersVM
                {
                    UserId = user.UserId,
                    UserName = user.UserName,
                    FullName = user.FullName,
                    EmployeeId = user.EmployeeId,
                    IsActive = user.IsActive
                };

                return PartialView("_EditUserModal", viewModel);
            }
            catch (Exception ex)
            {
                LogError("Error loading user for editing", ex);
                TempData["Error"] = "An error occurred while loading the user for editing.";
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(MstUsersVM model, int[] roleIds, long[] DeptCodeIds, string[] storeCodes = null)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    await LoadRolesAndDepartmentsForViewBag(roleIds, DeptCodeIds);
                    TempData["Error"] = "Invalid data. Please correct the errors and try again.";
                    return PartialView("_EditUserModal", model);
                }
                var roles = await _roleService.GetRolesByIdsAsync(roleIds);
                if (roles != null && roles.Any(role => role.RoleName == "Approver") && roles.Any(role => role.RoleName == "Requester"))
                {
                    TempData["Error"] = "Assigning both Approver and Requester roles to a user is forbidden.";
                    return RedirectToAction("Index");
                }
                var user = await _userService.GetUserByIdAsync(model.UserId);
                if (user == null)
                {
                    TempData["Error"] = "User not found.";
                    return RedirectToAction("Index");
                }

                var changes = new List<string>();

                if (user.FullName != model.FullName)
                {
                    changes.Add($"FullName changed from '{user.FullName}' to '{model.FullName}'");
                    user.FullName = model.FullName;
                }

                if (user.EmployeeId != model.EmployeeId)
                {
                    changes.Add($"EmployeeId changed from '{user.EmployeeId}' to '{model.EmployeeId}'");
                    user.EmployeeId = model.EmployeeId;
                }

                if (user.IsActive != model.IsActive)
                {
                    changes.Add($"IsActive changed from '{user.IsActive}' to '{model.IsActive}'");
                    user.IsActive = model.IsActive;
                }

                if (roleIds != null && roleIds.Any())
                {
                    await _userRoleService.UpdateRolesForUserAsync(user.UserId, roleIds);
                    var assignedRoles = await _roleService.GetRolesByIdsAsync(roleIds);
                    changes.Add($"Assigned Roles updated to: {string.Join(", ", assignedRoles.Select(r => r.RoleName))}");
                }

                if (DeptCodeIds != null && DeptCodeIds.Any())
                {
                    await _userDeptService.UpdateDeptRightsForUserAsync(user.UserId, DeptCodeIds);
                    var assignedDepartments = await _departmentService.GetDepartmentsByIdAsync(DeptCodeIds);
                    changes.Add($"Assigned Departments updated to: {string.Join(", ", assignedDepartments.Select(d => d.Name))}");
                }
                if (storeCodes != null && storeCodes.Any())
                {
                    await _userStoreService.UpdateStoressForUserAsync(user.UserId.ToString(), storeCodes);
                    var assignedStores = await _storeService.GetStoresByCodeAsync(storeCodes);
                    changes.Add($"Assigned Stores updated to: {string.Join(", ", assignedStores.Select(d => d.StoreName))}");
                }
                await _userStoreService.UpdateStoressForUserAsync(user.UserId.ToString(), storeCodes);
                await _userService.UpdateUserAsync(user);

                string logMessage = $"User '{user.UserName}' (ID: {user.UserId}) updated by '{Session["UserName"]}' on {DateTime.Now}\n" +
                                    "Changes made:\n" +
                                    string.Join("\n", changes);

                await LogActionAsync(user.UserId.ToString(), "Edit User", logMessage, Session["UserName"].ToString());

                TempData["Success"] = "User updated successfully.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                LogError("Error updating user", ex);
                await LoadRolesAndDepartmentsForViewBag(roleIds, DeptCodeIds);
                TempData["Error"] = "An error occurred while updating the user.";
                return RedirectToAction("Index");
            }
        }


        private async Task LoadRolesAndDepartmentsForViewBag(int[] selectedRoleIds = null,long[] selectedDeptCodeIds=null)
        {
            var roles = await _roleService.GetAllRolesAsync();
            var departments = await _departmentService.GetAllDepartmentsAsync();
            ViewBag.Roles = new MultiSelectList(roles, "RoleId", "RoleName", selectedRoleIds);
            ViewBag.Branches = new MultiSelectList(departments, "DeptCode", "Name", selectedDeptCodeIds);
        }
        private async Task LoadStoresForViewBag(string[] selectedStoresCodeeIds = null)
        {
            var stores = await _storeService.GetAllStoresAsync();
            ViewBag.Stores = new MultiSelectList(stores, "StoreCode", "StoreName", selectedStoresCodeeIds);
        }
        private void LogError(string message, Exception ex)
        {
            TempData["Error"] = message;
            // Add your logging logic here
        }
    }
}