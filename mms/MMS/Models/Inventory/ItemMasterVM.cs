using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MMS.Models.Inventory
{
    public class ItemMasterVM
    {
        public string ProdCode { get; set; }

        public string ProdDesc { get; set; }

        public string ProductGroup { get; set; }

        public string UnitMeas { get; set; } = "PCS";

        [Range(0, double.MaxValue)]
        public decimal QtyOnHand { get; set; } = 0;

        [Range(0, double.MaxValue)]
        public decimal UnitCost { get; set; } = 0;

        [Range(0, double.MaxValue)]
        public decimal TotalCost { get; set; } = 0;

        [Range(0, double.MaxValue)]
        public decimal QtyOnOrder { get; set; } = 0;

        public DateTime? LastActivityDate { get; set; }

        public string LastActivityDateEth { get; set; }

        public DateTime? LastRecieveDate { get; set; }

        public string LastRecieveDateEth { get; set; }

        [Range(0, double.MaxValue)]
        public decimal ReserveQty { get; set; } = 0;

        [Range(0, double.MaxValue)]
        public decimal ReserveCost { get; set; } = 0;

        [Range(0, double.MaxValue)]
        public decimal ReorderLevel { get; set; } = 0;

        [Range(0, double.MaxValue)]
        public decimal InitialQty { get; set; } = 0;

        [Range(0, double.MaxValue)]
        public decimal InitialTotalCost { get; set; } = 0;

        [Range(0, double.MaxValue)]
        public decimal BBFQty { get; set; } = 0;

        [Range(0, double.MaxValue)]
        public decimal BBFTotalCost { get; set; } = 0;

        [Range(0, double.MaxValue)]
        public decimal QtyCounted { get; set; } = 0;

        public string StockAccountSegment { get; set; }

        public string ExpenseAccountSegment { get; set; }

        public string IFBExpenseAccount { get; set; }

        public string PriceVarianceAccountSegment { get; set; }

        public bool IsVatable { get; set; } = true;

        public bool IsOrder { get; set; } = false;

        public int LeadTime { get; set; } = 90;

        [Range(0, double.MaxValue)]
        public decimal SafetyFactor { get; set; } = 0;

        public decimal? IncDemandPercent { get; set; }

        public decimal? MinimumLevel { get; set; }

        public decimal? Maximumlevel { get; set; }

        public string CrtBy { get; set; }

        public DateTime? CrtDt { get; set; } = DateTime.Now;

        public string CrtWs { get; set; } = Environment.MachineName;

        public bool IsItemObsulte { get; set; }

        public bool? Clamable { get; set; }

        public string OldCode { get; set; }

        public string MainPG { get; set; }

        public string OldMainPG { get; set; }

        public int? NoPerPad { get; set; }

        public decimal? QtyBeforUpdate { get; set; }

        public string CatType { get; set; }
        public string CatCode { get; set; }
        public string CatDesc { get; set; }
        public string newProdCode { get; set; }

        public string CategoryType { get; set; }
        public string ItemType { get; set; }
        public string MainIT { get; set; }
        public string mainItems { get; set; }
        public string TagCode { get; set; }
        public virtual ICollection<ItemDetailVM> ItemDetails { get; set; }
    }
}
