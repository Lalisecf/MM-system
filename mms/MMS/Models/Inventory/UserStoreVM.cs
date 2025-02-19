using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MMS.Models.Inventory
{
    public class UserStoreVM
    {
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string StoreCode { get; set; }
        public string StoreName { get; set; }
        public string FullName { get; set; }
        public IEnumerable<string> StoreCodes { get; set; } 
    }

}
