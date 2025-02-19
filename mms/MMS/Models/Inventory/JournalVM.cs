using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace MMS.Models.Inventory
{
    public class JournalVM
    {
        public int DetailId { get; set; }
        public string RefNo { get; set; }
        public string ProdCode { get; set; }
        public string Description { get; set; }
        public int Quantity { get; set; }
        public decimal UnitCost { get; set; }
        public decimal TotalAmount { get; set; }
        public string Department { get; set; }
        public string Supplier { get; set; }
        public string StockAccountNo { get; set; }
        public string CostAccountNo { get; set; }
        public string UserName { get; set; }
        public string StoreName { get; set; }
        public int Period { get; set; }
    }
}
