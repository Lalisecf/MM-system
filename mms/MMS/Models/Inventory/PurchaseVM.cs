using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace MMS.Models.Inventory
{
    public class PurchaseVM
    {
        public string RefNo { get; set; }
        public string ProdCode { get; set; }
       public string ProdDesc { get; set; }
        
        public string StoreCode { get; set; }
        public string CatType { get; set; }
        public string CatDesc { get; set; }
        public string SuppInvNo { get; set; }
        public string SuppInvDate { get; set; }
        public string Remark { get; set; }
        public string Supplier { get; set; }
        public bool IsPrinted { get; set; }
        public DateTime RefDate { get; set; }
        public string Item { get; set; }
        public string CrtBy { get; set; }
        public string MainItem { get; set; }
        public int ItemBrandId { get; set; }
        public string Brand { get; set; }
        public string Type { get; set; }
        public string Description { get; set; }
        public string SupplierAcct { get; set; }
        public string MainPG { get; set; }
        public string StoreName { get; set; }
        public string SupplierName { get; set; }


        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be ≥ 1")]
        public int Qty { get; set; }

        [Range(0.01, double.MaxValue, ErrorMessage = "Unit cost must be > 0")]
        public decimal UnitCost { get; set; }

        public decimal VatAmount { get; set; }
        public decimal TotalAmount { get; set; }

        public List<PurchaseItem> Items { get; set; } = new List<PurchaseItem>();
    }

    public class PurchaseItem
    {
        public string MainItem { get; set; }
        public int ItemBrandId { get; set; }
        public string Brand { get; set; }
        public string Type { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be ≥ 1")] 
        public int Qty { get; set; }

        [Range(0.01, double.MaxValue, ErrorMessage = "Unit cost must be > 0")] 
        public decimal UnitCost { get; set; }

        public decimal VatAmount { get; set; }
        public decimal TotalAmount { get; set; }
        public string ProdCode { get; set; }
        public string ProdDesc { get; set; }
        public string MainPG { get; set; }
    }
}
