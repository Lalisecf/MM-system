using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace MMS.Models.Inventory
{
    public class StoreVM
    {
        [Required]
        [StringLength(20)]
        public string StoreCode { get; set; }

        [Required]
        
        public string StoreName { get; set; }

        
        public string Address { get; set; }

        public bool InventoryUpdate { get; set; } = false;

        [StringLength(25)]
        public string AccountNo { get; set; }

        [StringLength(25)]
        public string ShortOverAccount { get; set; }

        public long? BranchCode { get; set; }

        [StringLength(20)]
        public string ShortCode { get; set; }

        public bool IsSatellite { get; set; } = false;

        [StringLength(10)]
        public string StockCc { get; set; }

        public bool IsTransit { get; set; }

        public bool IsDamaged { get; set; }
    }
}