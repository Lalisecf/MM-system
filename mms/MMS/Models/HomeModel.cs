
using SharedLayer.AB_Common;
using System;
using System.ComponentModel.DataAnnotations;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace MMS.Models
{
    public class HomeModel
    {
        string ConnectionString = ConfigurationManager.ConnectionStrings["MasterDBConnectionString"].ConnectionString;

        SystemTools _SystemTools = new SystemTools();
       
    }
    public class LoginModel
    {
        public string UserID { get; set; }

        [Required]
        [Display(Name = "User Name")]
        public string UserName { get; set; }  

        [Required]
        [Display(Name = "Password")]
        public string Password { get; set; }
        public string NewPassword { get; set; } 
        public bool RememberMe { get; set; }

        public int IsArchived { get; set; }
        public int IsActive { get; set; }
        
    }

    public class LoginResultModel
    {
        public int UserID { get; set; }
         [Display(Name = "User Name")]
        public string Email { get; set; }
        public string Firstname { get; set; }
        public string Position { get; set; }
        public byte[] Image { get; set; }
        public bool RememberMe { get; set; }
        public string Password { get; set; }
    }

    public class DashboardModels
    {
        public string UsersList { get; set; }
        public string ActiveUsers { get; set; }
        public string InActiveUsers { get; set; }
        public string ArchivedUsers { get; set; }
        public bool RememberMe { get; set; }
    }
}