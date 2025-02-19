using SharedLayer.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace MMS.Models.Master
{
    public class MstUserRoleVM
    {
        public int UserId { get; set; }
        public MstUser Users { get; set; }  
        public int RoleId { get; set; }
        public MstRole Roles { get; set; }  
    }


}