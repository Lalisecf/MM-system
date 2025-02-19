using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MMS.Models.Inventory
{
    public class ItemBrandVM
    {
        public int ItemBrandId { get; set; }
        public string Brand { get; set; }
        public string Type { get; set; }
        public string MainItem { get; set; }
        public string ProdDesc { get; set; }
        public string ItemType { get; set; }        

    }
}