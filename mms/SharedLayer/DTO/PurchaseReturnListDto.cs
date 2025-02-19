namespace SharedLayer.DTO
{
    public class PurchaseReturnListDto
    {
        public string RefNo { get; set; }
        public bool Ytd { get; set; }

        public string ProdCode { get; set; }
        public string ProdDesc { get; set; }
        public int ItemBrandId { get; set; }
        public string Brand { get; set; }
        public string Type { get; set; }

        public int Qty { get; set; }
        public decimal QtyReturned { get; set; }
        public decimal? QtyIssued { get; set; }

        public decimal UnitCost { get; set; }

        public decimal TotalAmount { get; set; }
        public string ReturnRefNo { get; set; }

        public string StoreCode { get; set; }
        public string ShortCode { get; set; }
        public string Remark { get; set; }
        public string Supplier { get; set; }
    }
}
