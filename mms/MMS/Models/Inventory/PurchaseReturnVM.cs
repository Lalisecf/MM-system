using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MMS.Models.Inventory
{
    public class PurchaseReturnVM
    {
        public string RefNo { get; set; }
        public string ReturnRefNo { get; set; }
        

       public string ProdCode { get; set; }
       public string ProdDesc { get; set; }
        public string StoreCode { get; set; }
        public string ShortCode { get; set; }

        public decimal Qty { get; set; }

        public decimal UnitCost { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal VatAmount { get; set; }
        public string CatType { get; set; }
        public string CatDesc { get; set; }

        public string SRVRefNo { get; set; }
        public string SuppInvDate { get; set; }
        public string Remark { get; set; }
        public string Supplier { get; set; }

        public bool IsPrinted { get; set; }
        public bool Ytd { get; set; }

        public DateTime RefDate { get; set; }
        public string MainItem { get; set; }
        public string ItemType { get; set; }
        public decimal QtyReturned { get; set; } 
        public decimal? QtyIssued { get; set; }
        public int? ItemBrandId { get; set; }
        public string Brand { get; set; }
        public string Type { get; set; }
        public string Period { get; set; }
        public string ReqNo { get; set; }
        public string StoreName { get; set; }
        public string otherStoreName { get; set; }
        public string UnitMeas { get; set; }
        public string Description { get; set; }
        public string CrtBy { get; set; }
        public string DeptDescription { get; set; }
        

        public List<PurchaseReturnList> Items { get; set; }
        public class PurchaseReturnList
        {
            public string RefNo { get; set; }
            public string Ytd { get; set; }

            public string ProdCode { get; set; }
            public string ProdDesc { get; set; }
            public int ItemBrandId { get; set; }

            public int Qty { get; set; }

            public decimal UnitCost { get; set; }

            public decimal TotalAmount { get; set; }
            public string ReturnRefNo { get; set; }

        }

    }
}