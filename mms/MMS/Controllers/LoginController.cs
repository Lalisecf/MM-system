using System;
using System.Configuration;
using System.Web;
using System.Web.Mvc;
using System.Data.SqlClient;
using System.Data;
using System.Threading.Tasks;
using System.Linq;
using SharedLayer.AB_Common;
using SharedLayer.Models;
using System.Data.Entity;
using MMS.Models;
using BusinessLayer.Interfaces;

namespace MMS.Controllers
{
    public class LoginController : Controller
    {
        private IMstUserService _userService;
        private IMstUserRoleService _userRoleService;
        private IRoleService _roleService;
        private IDepartmentService _departmentService;
        private IUserBranchService _userBranchService;
        SystemTools objSystemTools = new SystemTools();
        private MasterDataContext db = new MasterDataContext();
        API pl = new API();

        private string ConnectionString
        {
            get
            {
                return ConfigurationManager.ConnectionStrings["MasterDBConnectionString"].ConnectionString;
            }
        }

        public LoginController(IMstUserService MstUser,
            IMstUserRoleService savedAssignedRoles,
            IUserBranchService userBranchService,
            IRoleService roleService,
            IDepartmentService departmentService)
        {
            _userService = MstUser;
            _userRoleService = savedAssignedRoles;
            _userBranchService = userBranchService;
            _roleService = roleService;
            _departmentService = departmentService;
        }

        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> Login(LoginViewModells loginViewModel)
        {
            try
            {
                if (!db.Database.Exists())
                {
                    Log.Error("Database Not Exists");
                    ModelState.AddModelError("", "Please Contact System Admin");
                    TempData["Error"] = "Network Error";
                    return View(loginViewModel);
                }

                Session["UserName"] = loginViewModel.UserName;
                var userExists = await _userService.UserExistsAsync(loginViewModel.UserName);
                if (!userExists)
                {
                    Log.Error("Invalid Credentials");
                    ModelState.AddModelError("", "Invalid Credentials");
                    return View(loginViewModel);
                }

                var usermasterModel = await _userService.GetUserByUsernameAsync(loginViewModel.UserName);
                var userroleModel = await _userRoleService.GetAssignedRolesByUserIdAsync(usermasterModel.UserId);

                if (await IsUserLockedOut(usermasterModel.UserName))
                {
                    ModelState.AddModelError("", "Account locked due to unsuccessful login attempts. Contact system admin.");
                    return View(loginViewModel);
                }

                await _userService.UpdateLoginAsync(usermasterModel.UserId);
                pl.ResetFailedAttempts(usermasterModel.UserName);
                Session["SessionID"] = usermasterModel.SessionID;
                Session["UserID"] = usermasterModel.UserId;

                await GetAssignedRole(usermasterModel.UserId);
                await GetAssignedBranch(usermasterModel.UserId);

                Log.Info($"On {DateTime.Now} {Session["UserName"]} logged in from IP {Request.ServerVariables["REMOTE_ADDR"]}");

                return RedirectToAction("Home", "Home");
            }
            catch (Exception ex)
            {
                Log.Error(Session["UserName"]?.ToString() + " " + GetInnerExceptionMessage(ex));
                ModelState.AddModelError("", "Please Contact System Admin");
                return View(loginViewModel);
            }
        }

        private bool AuthenticateUserWithAD(string username, string password, LoginViewModells loginViewModel)
        {
            try
            {
                var isAuthenticate = pl.GetADUser(username, password);
                return true;
            }
            catch (Exception ex)
            {
                pl.IncrementFailedAttempts(loginViewModel.UserName);
                TempData["Error"] = $"{ex.InnerException?.Message} ({loginViewModel.attempt} trial of 5)";
                Log.Error($"{Session["UserName"]} {ex.InnerException?.Message}");
                return false;
            }
        }

        private async Task AssignRoleToUserIfNeeded(long userId)
        {
            var authorizemaker = objSystemTools.authorizeMakerRolefromCore(Session["UserName"].ToString());
            var authorizechecker = objSystemTools.authorizeCheckerRolefromCore(Session["UserName"].ToString());

            if (authorizemaker || authorizechecker)
            {
                using (var connection = new SqlConnection(ConnectionString))
                {
                    string strUpdate = @"UPDATE [dbo].[MstUserRoles] SET [RoleId]=@RoleId WHERE [UserId]=@UserId AND RoleId <> 40";
                    SqlCommand command = new SqlCommand(strUpdate, connection);
                    command.Parameters.Add(new SqlParameter("@UserId", userId));
                    command.Parameters.Add(new SqlParameter("@RoleId", authorizemaker ? 2 : 3));

                    await connection.OpenAsync();
                    await command.ExecuteNonQueryAsync();
                }
            }
        }

        private async Task GetAssignedRole(int userId)
        {
            var roles = await _userRoleService.GetAssignedRolesByUserIdAsync(userId);
            if (roles.Any())
            {
                Session["Roles"] = roles.Select(r => r.RoleId).ToList();
                var roleNames = await _roleService.GetRolesByIdsAsync(roles.Select(r => r.RoleId).ToArray());
                Session["RoleName"] = string.Join(", ", roleNames.Select(r => r.RoleName));
            }
            else
            {
                Session["Roles"] = null;
                Session["RoleName"] = null;
            }
        }

        private async Task GetAssignedBranch(int userId)
        {
            var branches = await _userBranchService.GetAssignedDeptRightsByUserIdAsync(userId);
            if (branches.Any())
            {
                Session["Branches"] = branches.Select(b => b.DeptCode).ToList();
                var branchNames = await _departmentService.GetDepartmentsByIdAsync(branches.Select(b => b.DeptCode).ToArray());
                Session["BranchName"] = string.Join(", ", branchNames.Select(b => b.Name));
            }
            else
            {
                Session["Branches"] = null;
                Session["BranchName"] = null;
            }
        }

        private bool HasRolePermission()
        {
            var role = Convert.ToInt32(Session["Role"]);
            return (role == 2 && objSystemTools.authorizeMakerRolefromCore(Session["UserName"].ToString())) ||
                   (role == 3 && objSystemTools.authorizeCheckerRolefromCore(Session["UserName"].ToString()));
        }

        private string GetInnerExceptionMessage(Exception ex)
        {
            Exception inner = ex.InnerException ?? ex;
            while (inner.InnerException != null)
            {
                inner = inner.InnerException;
            }
            return inner.Message;
        }

        public ActionResult Show()
        {
            return View();
        }

        [HttpGet]
        public ActionResult Logout()
        {
            try
            {
                var ipAdd = Request.ServerVariables["REMOTE_ADDR"];
                var desc = "On " + DateTime.Now + " Date " + Session["UserName"].ToString() + " is Logout MMS System from " + ipAdd + " ip address";
                Log.Info(desc);
                Response.Cache.SetCacheability(HttpCacheability.NoCache);
                Response.Cache.SetExpires(DateTime.Now.AddSeconds(-1));
                Response.Cache.SetNoStore();

                HttpCookie Cookies = new HttpCookie("WebTime");
                Cookies.Value = "";
                Cookies.Expires = DateTime.Now.AddHours(-1);
                Response.Cookies.Add(Cookies);
                HttpContext.Session.Clear();
                Session.Abandon();
                return RedirectToAction("Login", "Login");
            }
            catch (Exception ex)
            {
                Exception inner = ex.InnerException ?? ex;
                while (inner.InnerException != null)
                {
                    inner = inner.InnerException;
                }
                Log.Error(Session["UserName"].ToString() + " " + inner.Message);
                throw;
            }
        }

        [NonAction]
        public void remove_Anonymous_Cookies()
        {
            try
            {
                if (Request.Cookies["WebTime"] != null)
                {
                    var option = new HttpCookie("WebTime");
                    option.Expires = DateTime.Now.AddDays(-1);
                    Response.Cookies.Add(option);
                }
            }
            catch (Exception ex)
            {
                Exception inner = ex.InnerException ?? ex;
                while (inner.InnerException != null)
                {
                    inner = inner.InnerException;
                }
                Log.Error(Session["UserName"].ToString() + " " + inner.Message);
                throw;
            }
        }

        [HttpGet]
        public ActionResult VerifyAccount()
        {
            return View();
        }

        [HttpGet]
        public ActionResult ChangePassword()
        {
            return View();
        }

        [HttpPost]
        public ActionResult ChangePassword(LoginViewModells model)
        {
            try
            {
                return View(model);
            }
            catch (Exception e)
            {
                Exception inner = e.InnerException ?? e;
                while (inner.InnerException != null)
                {
                    inner = inner.InnerException;
                }
                Log.Error(Session["UserName"].ToString() + " " + inner.Message);
                return View("Login");
            }
        }

        [HttpGet]
        public ActionResult Resetpass()
        {
            return View();
        }

        private async Task<bool> IsUserLockedOut(string username)
        {
            var usr = await db.MstUsers.FirstOrDefaultAsync(x => x.UserName == username);
            return usr.Attempt > 4;
        }
    }
}

