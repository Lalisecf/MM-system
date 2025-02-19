using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MMS.Models.Inventory
{
    public class ItemDetailVM
    {
        public string StoreCode { get; set; }

        public string ProdCode { get; set; }

        public decimal QtyOnHand { get; set; }

        public decimal UnitCost { get; set; }

        public decimal TotalCost { get; set; }

        public DateTime? LastActivityDate { get; set; }

        public string LastActivityDateEth { get; set; }

        public DateTime? LastRecieveDate { get; set; }

        public string LastRecieveDateEth { get; set; }

        public decimal ReserveQty { get; set; }

        public decimal ReserveCost { get; set; }

        public decimal InitialQty { get; set; }

        public decimal InitialTotalCost { get; set; }

        public decimal BBFQty { get; set; }

        public decimal BBFTotalCost { get; set; }

        public decimal? QtyCounted { get; set; }

        public string BinLocation { get; set; }

        public string OldCode { get; set; }

        public string MainPG { get; set; }

        public string OldMainPG { get; set; }

        public int? NoPerPad { get; set; }

        public decimal? QtyBeforUpdate { get; set; }

        public ItemMasterVM ItemMaster { get; set; }
    }
}