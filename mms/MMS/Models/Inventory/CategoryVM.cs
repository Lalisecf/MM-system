using System.ComponentModel.DataAnnotations;

namespace MMS.Models.Inventory
{
    public class CategoryVM
    {
        [Required]
        public string CatCode { get; set; }
        public int CatID { get; set; }
        public string CatDesc { get; set; }
        public string CatDepAcct { get; set; }
        public string CatDepCc { get; set; }
        [Required]
        public decimal DePPercent { get; set; } = 0;
        public string CatDepExpAcct { get; set; }
        [Required]
        public decimal FirstYearDep { get; set; } = 0;
        [Required]
        public decimal YearAfterDep { get; set; } = 0;
        [Required]
        public bool IsPool { get; set; } = false;
        public string Parent { get; set; }
        public string StockAccountSegment { get; set; }
        public string ExpenseAccountSegment { get; set; }
        public string IFBExpenseAccount { get; set; }
        public int? Levels { get; set; }
        public string Prefix { get; set; }
        public int? NextNo { get; set; }
        public bool? Type { get; set; }
        public bool? Clamable { get; set; }
        public string MainPG { get; set; }
        public int? UsefullLife { get; set; }
        public decimal? ScrapValue { get; set; }
        public int? CatType { get; set; }
        public int IncNum { get; set; }
        public int CategoryType { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
