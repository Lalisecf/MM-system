using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace MMS.Models.Inventory
{
    public class FAVM
    {
        [Required]
        
        public string SerialNo { get; set; }

        public int AssetId { get; set; }

        
        public string AssetCode { get; set; }

        [StringLength(100)]
        public string AssetDescription { get; set; }

        [Required]
        public decimal Quantity { get; set; } = 1;

        [Required]
        public decimal BookValue { get; set; } = 0;

        public DateTime? TransferDate { get; set; }

        [Required]
        public decimal ACCDepr { get; set; } = 0;

        
        public string Remark { get; set; }

        public long Branch { get; set; }

        public long DepartmentId { get; set; }

        [Required]
        public int AccYear { get; set; }

        [Required]
        public bool Disposed { get; set; }

        
        public string EngineNo { get; set; }

        [Required]
        public decimal UnitCost { get; set; } = 0;

        public DateTime? PurchaseDate { get; set; }

        
        public string Model { get; set; }

        
        public string Brand { get; set; }

        
        public string Condition { get; set; }

        
        public string EqSerialNo { get; set; }

        [Required]
        public bool NewlyAdded { get; set; } = false;

        [Required]
        [StringLength(20)]
        public string CrtBy { get; set; }

        [Required]
        public DateTime CrtDt { get; set; } = DateTime.Now;

        
        public string storeCode { get; set; }

        [Required]
        public decimal AICUnitcost { get; set; } = 0;

        [Required]
        public decimal MaintenanceCost { get; set; } = 0;

        [Required]
        public int UsefullLife { get; set; } = 0;
    }
}