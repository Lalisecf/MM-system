using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedLayer.DTO
{
    public class IssueReturnListDto
    {
        public string RefNo { get; set; }
        public bool Ytcp { get; set; }

        public string ProdCode { get; set; }
        public string ProdDesc { get; set; }
        public int ItemBrandId { get; set; }
        public string Brand { get; set; }
        public string Type { get; set; }

        public int Qty { get; set; }
        public int QtyReturned { get; set; }
        public int? QtyIssued { get; set; }

        public decimal UnitCost { get; set; }

        public decimal TotalAmount { get; set; }
        public string ReturnRefNo { get; set; }

        public string StoreCode { get; set; }
        public string ShortCode { get; set; }
        public string Remark { get; set; }
        public string Supplier { get; set; }
        public string StoreName { get; set; }
        public string Issued { get; set; }
        public string Returned { get; set; }
        public int DetailId { get; set; }
        public int YtdId { get; set; }
        

    }
}
