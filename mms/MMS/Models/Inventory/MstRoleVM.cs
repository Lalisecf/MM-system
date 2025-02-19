using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MMS.Models.Inventory
{
    public class MstRoleVM
    {
        public int RoleID { get; set; } 
        public int Application { get; set; } 
        public string ApplicationName { get; set; } 
        public int ForUnitType { get; set; } 
        public string ForUnitTypeName { get; set; } 
        public string RoleName { get; set; } 
        public bool Status { get; set; } 
    }
}