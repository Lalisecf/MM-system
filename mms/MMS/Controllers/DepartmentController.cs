using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web.ApplicationServices;
using System.Web.Mvc;
using System.Web.Security;
using BusinessLayer.Interfaces;
using BusinessLayer.Service;
using MMS.Models.Inventory;
using SharedLayer.Models.Inventory;

namespace MMS.Controllers
{
    public class DepartmentController : BaseController
    {
        private readonly IDepartmentService _departmentService;

        public DepartmentController(IDepartmentService departmentService, ILogHistoryService logService)
            : base(logService)
        {
            _departmentService = departmentService;
        }

        public async Task<ActionResult> Index()
        {
            try
            {
                var departments = await _departmentService.GetAllDepartmentsAsync();

                var viewModels = departments.Select(department => new DepartmentVM
                {
                    MainCostCenter = department.MainCostCenter,
                    BranchCode = department.BranchCode,
                    DeptCode = department.DeptCode,
                    Name = department.Name,
                    DeptCc = department.DeptCc,
                    ParentCode = department.ParentCode,
                    DeptLevel = department.DeptLevel,
                    DeptCodeHr = department.DeptCodeHr,
                    ShorCode = department.ShorCode,
                    Furniture = department.Furniture,
                    OfficeEquip = department.OfficeEquip,
                    MotorVehicle = department.MotorVehicle,
                    DeptType = department.DeptType,
                    DeptTypeName = department.TblDeptTypes.BranchType?? "N/A",
                    DeptLevelName = department.TblDeprtLevels.LevelName?? "N/A"  // Handle null
                }).ToList();
                return View(viewModels);
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
            var deptLevel =  await _departmentService.GetAllDeprtLevelsAsync();
            var deptType = await _departmentService.GetAllDeptTypesAsync();
            var parentCode= await _departmentService.GetAllDepartmentsAsync(); 

            ViewBag.DeptLevel = new SelectList(deptLevel, "ID", "LevelName");
            ViewBag.DeptType = new SelectList(deptType, "ID", "BranchType");
            ViewBag.ParentCode = new SelectList(parentCode, "DeptCode", "Name");
            return PartialView("_createDepartment");
        }

        [HttpPost]
        public async Task<ActionResult> Create(tblDepartment department)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    long departmentCode = await _departmentService.GetMaxDeptCodeAsync()+1;
                    if (departmentCode <= 0)
                    {
                        departmentCode = 1; 
                    }
                    var model = new tblDepartment
                    {
                        DeptCode = departmentCode,  
                        Name = department.Name,
                        DeptLevel = department.DeptLevel,
                        MainCostCenter = department.MainCostCenter,
                        Furniture = department.Furniture,
                        OfficeEquip = department.OfficeEquip,
                        MotorVehicle = department.MotorVehicle,
                        DeptType = department.DeptType,
                        DeptCc = department.DeptCc,
                        ShorCode = department.ShorCode,
                        ParentCode = department.ParentCode,
                        BranchCode = department.BranchCode,
                        DeptCodeHr = department.DeptCodeHr,
                        IsActive = department.IsActive
                    };
                    if (await _departmentService.DepartmentExistsAsync(department.Name))
                    {
                        TempData["Error"] = "The Department is already Exist.";
                        return RedirectToAction("Index");
                    }
                    await _departmentService.AddDepartmentAsync(model);
                    //Log
                    string logMessage = $"A new Department was created by '{Session["UserName"]}':\n" +
                            $"- DeptName: '{model.Name}'\n" +
                            $"- DeptType: '{model.DeptType}'\n" +
                            $"- deptLevel: '{model.DeptLevel}'\n" +
                            $"- Is Active: {model.IsActive}\n" +
                            $"- DeptCode: {model.DeptCode}";
                   await  LogActionAsync(model.DeptCode.ToString(), "Create Department", logMessage, Session["UserName"].ToString());
                    TempData["Success"] = "Department created successfully!";
                    return RedirectToAction("Index", "Department");
                }
                catch (Exception ex)
                {
                    TempData["Error"] = "An error occurred while creating the Department: " + ex.Message;
                    return RedirectToAction("Index", "Department");
                }
            }
            return RedirectToAction("Index", "Department");
        }
        [HttpGet]
        public async Task<ActionResult> Edit(long id)
        {
            try
            {
                var departments = await _departmentService.GetDepartmentsByIdAsync(new long[] { id });
                var department = departments.FirstOrDefault();
                if (department == null)
                {
                    TempData["Error"] = "Department not found.";
                    return RedirectToAction("Index");
                }
                await LoadRolesAndDepartmentsForViewBag(
                   //selectedRoleIds: assignedRoles.Select(r => r.RoleId).ToArray(),
                   selectedDeptCodeIds: departments.Select(b => b.DeptCode).ToArray()
               );
                var departmentVM = new DepartmentVM
                {
                    MainCostCenter = department.MainCostCenter,
                    BranchCode = department.BranchCode,
                    DeptCode = department.DeptCode,
                    Name = department.Name,
                    DeptCc = department.DeptCc,
                    ParentCode = department.ParentCode, // Show old value
                    DeptLevel = department.DeptLevel,   // Show old value
                    DeptCodeHr = department.DeptCodeHr,
                    ShorCode = department.ShorCode,
                    Furniture = department.Furniture,
                    OfficeEquip = department.OfficeEquip,
                    MotorVehicle = department.MotorVehicle,
                    DeptType = department.DeptType,    // Show old value
                    IsActive = department.IsActive
                };

                // Populate dropdowns and pre-select the old values
                var deptLevel = await _departmentService.GetAllDeprtLevelsAsync();
                var deptType = await _departmentService.GetAllDeptTypesAsync();
              

                // Pass old value as the selected item in the dropdown
                ViewBag.DeptLevel = new SelectList(deptLevel, "ID", "LevelName", department.DeptLevel);
                ViewBag.DeptType = new SelectList(deptType, "ID", "BranchType", department.DeptType);
               

                return PartialView("_EditDepartmentModal", departmentVM);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "An error occurred while loading the department for editing.";
                return RedirectToAction("Index");
            }
        }



        [HttpPost]
        public async Task<ActionResult> Edit(DepartmentVM model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var department = new tblDepartment
                    {
                        MainCostCenter = model.MainCostCenter,
                        BranchCode = model.BranchCode,
                        DeptCode = model.DeptCode,
                        Name = model.Name,
                        DeptCc = model.DeptCc,
                        ParentCode = model.ParentCode,
                        DeptLevel = model.DeptLevel,
                        DeptCodeHr = model.DeptCodeHr,
                        ShorCode = model.ShorCode,
                        Furniture = model.Furniture,
                        OfficeEquip = model.OfficeEquip,
                        MotorVehicle = model.MotorVehicle,
                        DeptType = model.DeptType,
                        IsActive = model.IsActive
                    };

                    await _departmentService.UpdateDepartmentAsync(department);

                    TempData["Success"] = "Department updated successfully.";
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    TempData["Error"] = "An error occurred while updating the department.";
                }
            }
            return View(model);
        }
        private async Task LoadRolesAndDepartmentsForViewBag(int[] selectedRoleIds = null, long[] selectedDeptCodeIds = null)
        {
            var parentCode = await _departmentService.GetAllDepartmentsAsync();
            ViewBag.ParentCode = new SelectList(parentCode, "DeptCode", "Name", selectedDeptCodeIds);
        }
        private void LogException(Exception ex, string message)
        {
            TempData["Error"] = message;
        }
    }
}