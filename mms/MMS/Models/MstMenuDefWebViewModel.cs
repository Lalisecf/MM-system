using System;
using System.Collections.Generic;

namespace MMS.Models
{
    public class MstMenuDefWebViewModel
    {
        public int MenuCode { get; set; }
        public string Description { get; set; }
        public string NavigateUrl { get; set; }
        public int? ParentCode { get; set; }
        public int? MenuLevel { get; set; }
        public int? AppId { get; set; }
        public int? ReportGroup { get; set; }
        public int? GroupCode { get; set; }
        public int? OrderBy { get; set; }
        public string ParentMenuDescription { get; set; } // To display the description of the Parent Menu
        public string ApplicationName { get; set; } // To display the application name

        public List<MstMenuDefWebViewModel> SubMenus { get; set; }
    }
}
