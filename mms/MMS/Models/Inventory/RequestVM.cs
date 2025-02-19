using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace MMS.Models.Inventory
{
    public class RequestVM
    {
        public string ReqNo { get; set; }
        [Range(0, int.MaxValue, ErrorMessage = "Quantity must be a non-negative integer.")]
        public int QtyReq { get; set; }
        public int? ReqBy { get; set; }
        public string FullName { get; set; }

        public string ReqDept { get; set; }

        public DateTime? ReqDate { get; set; }

        public string ReqDateEth { get; set; }

        public string Reason { get; set; }

        public int? Status { get; set; }

        public int? AppBy { get; set; }

        public DateTime? AppDate { get; set; }

        public string AppDateEth { get; set; }

        [Required]
        public bool IsDocPrinted { get; set; } = false;

        public string BatchNo { get; set; }

        [Required]
        public bool IsForBatch { get; set; } = false;

        public int? JobOrderNumber { get; set; }

        public int? CancelBy { get; set; }

        public DateTime? CancelDate { get; set; }

        public string CancelDateEth { get; set; }

        public string rollBackBy { get; set; }

        public DateTime? rollBackDate { get; set; }

        public string rollBackDateEth { get; set; }

        public string StoreCode { get; set; }

        public string PlateNo { get; set; }

        public string DriverName { get; set; }

        public string SatStoreCode { get; set; }

        public decimal? remTankAmt { get; set; } = 0;

        public bool? Clamable { get; set; }

        public string CatType { get; set; }
        public int CategoryType { get; set; }
        public string MainPG { get; set; }
        public string ItemitemValue { get; set; }
    public List<string> ItemValue { get; set; } = new List<string>();
}
}