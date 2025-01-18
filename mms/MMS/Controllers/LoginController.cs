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
using DataAccessLayer;
using System.Data.Entity;
using MMS.Models;


namespace MMS.Controllers
{

    public class LoginController : Controller
    {
        private IMstUserRepository _iMstUser;
        private IMstUserRoleRepository _savedUserRoles;
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

        public LoginController(IMstUserRepository MstUser, IMstUserRoleRepository savedAssignedRoles)
        {
            _iMstUser = MstUser;
            _savedUserRoles = savedAssignedRoles;
        }

        public ActionResult Login()
        {
            return View();
        }
        [HttpPost]
        public async Task<ActionResult> Login(LoginViewModells loginViewModel)
        {
            string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();

            try
            {
                if (!db.Database.Exists())
                {
                    Log.Error("Database Not Exists");
                    ModelState.AddModelError("", "Please Contact System Admin");
                    return View(loginViewModel);
                }

                Session["UserName"] = loginViewModel.UserName;
                var userExists = await _iMstUser.UserNameExistsAsync(loginViewModel.UserName);
                if (!userExists)
                {
                    ModelState.AddModelError("", "Invalid Credentials");
                    return View(loginViewModel);
                }

                var usermasterModel = _iMstUser.GetByUsername(loginViewModel.UserName);

                if (await IsUserLockedOut(usermasterModel.UserName))
                {
                    ModelState.AddModelError("", "Account locked due to unsuccessful login attempts. Contact system admin.");
                    return View(loginViewModel);
                }

                if (!AuthenticateUserWithAD(loginViewModel.UserName, loginViewModel.Password, loginViewModel))
                {
                    return View(loginViewModel);
                }

                await AssignRoleToUserIfNeeded(usermasterModel.UserId);

                var qq = _iMstUser.UpdateLogin(usermasterModel);
                pl.ResetFailedAttempts(usermasterModel.UserName);
                Session["SessionID"] = qq.SessionID;

                await AssignUserRoleToSession(usermasterModel.UserId);

                //if (!HasRolePermission())
                //{
                //    ModelState.AddModelError("", "Access Denied!");
                //    Log.Error($"{Session["UserName"]} Access Denied!");
                //    return View(loginViewModel);
                //}

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
                ModelState.AddModelError("", $"{ex.InnerException?.Message} ({loginViewModel.attempt} trial of 5)");
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

        private async Task AssignUserRoleToSession(long userId)
        {
            var assignedRole = await _savedUserRoles.GetAssignedRolesbyUserIdAsync(userId);
            if (assignedRole != null)
            {
                Session["Role"] = assignedRole.RoleId;
                int roleId = Convert.ToInt32(Session["Role"]);
                var role = await db.MstRoles.Where(c => c.RoleID == roleId).Select(c => c.RoleName).SingleOrDefaultAsync();
                Session["Roleer"] = role;
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



        public bool Update(LoginViewModells objMstUsers)
        {
            string ConnectionStringg = ConfigurationManager.ConnectionStrings["MasterDataClassesDataContext"].ConnectionString;
            SqlConnection connection = new SqlConnection(ConnectionStringg);
            AesAlgorithm aesAlgorithm = new AesAlgorithm();

            string strUpdate = @"update [PasswordMaster]   SET Password = @Password
                                from MstUser as AA where [PasswordMaster].MstUserId = AA.MstUserId AND [PasswordMaster].MstUserId=@MstUserID ";


            SqlCommand command = new SqlCommand() { CommandText = strUpdate, CommandType = CommandType.Text };
            command.Connection = connection;

            try
            {
                command.Parameters.Add(new SqlParameter("@UserID", objMstUsers.UserId));
                command.Parameters.Add(new SqlParameter("@Password", aesAlgorithm.EncryptString(objMstUsers.Password)));

                connection.Open();
                int _rowsAffected = command.ExecuteNonQuery();
                return true;
            }
            catch (Exception ex)
            {
                Exception inner = ex.InnerException ?? ex;
                while (inner.InnerException != null)
                {
                    inner = inner.InnerException;

                }
                Log.Error(Session["UserName"].ToString() + " " + inner.Message);
                throw new Exception("MstUsers::Update::Error!" + ex.Message, ex);
            }
            finally
            {
                connection.Close();
                command.Dispose();
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
            if (usr.attempt <= 4)
            {
                return false;
            }
            else
            {
                return true;
            }

        }
    }
}
