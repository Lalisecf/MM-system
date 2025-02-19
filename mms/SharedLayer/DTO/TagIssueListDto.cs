namespace SharedLayer.DTO
{
    public class TagIssueListDto
    {
        public string RefNo { get; set; }

        public string ProdCode { get; set; }
        public string ProdDesc { get; set; }

        public int Qty { get; set; }

        public decimal UnitCost { get; set; }

        public decimal TotalAmount { get; set; }

        public string GRV { get; set; }
        public string GIV { get; set; }
        public string status { get; set; }

        public string DepartmentName { get; set; }
        public int IssuedItems { get; set; }
        public int TaggedItems { get; set; }
    }
}
