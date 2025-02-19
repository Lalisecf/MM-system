using SharedLayer.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace MMS.Models.Master
{
    public class MstUsersVM
    {
        public int UserId { get; set; }
        public string UserName { get; set; }
        public string FullName { get; set; }
        public bool IsActive { get; set; }
        public bool AlreadyLogin { get; set; }
        public DateTime? LastLogin { get; set; }
        public string EmailAddress { get; set; }
        public string EmployeeId { get; set; }
        public int? DefaultApp { get; set; } 
        public string TelNo { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public int RoleId { get; set; }
        public string RoleName { get; set; }
        public long DeptCode { get; set; }
        public string Name { get; set; }
        [Required(ErrorMessage = "Please select at least one role.")]
        public IEnumerable<int> RoleIds { get; set; }
        [Required(ErrorMessage = "Please select at least one branch.")]
        public IEnumerable<int> DeptCodeIds { get; set; }
        public IEnumerable<string> StoreCodes { get; set; }

    }

}