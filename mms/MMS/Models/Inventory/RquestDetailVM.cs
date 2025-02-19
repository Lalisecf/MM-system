using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using SharedLayer.Models.Inventory;

namespace MMS.Models.Inventory
{
    public class RquestDetailVM
    {
        public string IdNo { get; set; }
        public string ReqNo { get; set; }
        public string ProdCode { get; set; }
        public string ProdDesc { get; set; }
        public int QtyReq { get; set; }
        public int QtyOnHand { get; set; }
        public int QtyIssued { get; set; }
        public DateTime? IssueDate { get; set; }
        public string IssueDateEth { get; set; }
        public string IssueRefNo { get; set; }
        public decimal UnitCost { get; set; }
        public int Status { get; set; }
        public decimal TotalCost { get; set; }
        public decimal QtyCancel { get; set; }
        public string CancelBy { get; set; }
        public DateTime? DateCancel { get; set; }
        public string DateCancelEth { get; set; }
        public string WsCancel { get; set; }
        public string StockAcct { get; set; }
        public string CostAcct { get; set; }
        public string CostCenter { get; set; }
        public decimal QtyReturned { get; set; }
        public string Reason { get; set; }
        public string ReqDept { get; set; }

        public string ReqDateEth { get; set; }

        public int? AppBy { get; set; }

        public DateTime? AppDate { get; set; }

        public string AppDateEth { get; set; }

        public string BatchNo { get; set; }

        public int? JobOrderNumber { get; set; }

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

        public string CatType { get; set; }
        public int CategoryType { get; set; }
        public string MainPG { get; set; }
        public string ItemitemValue { get; set; }
        public virtual tblRequest Requests { get; set; }
        public virtual IEnumerable<TrandtVM> trandt { get; set; }
        public bool isGRV_OR_GRN { get; set; }

    }
}