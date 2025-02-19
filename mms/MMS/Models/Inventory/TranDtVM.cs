using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace MMS.Models.Inventory
{
    public class TrandtVM
    {

        [Display(Name = "Detail ID")]
        public int DetailId { get; set; }

        [Required]
        [StringLength(20)]
        [Display(Name = "Reference Number")]
        public string RefNo { get; set; }


        [Required]
        [StringLength(20)]
        [Display(Name = "Store Code")]
        public string StoreCode { get; set; }

        [Required]
        [Display(Name = "Transaction Code")]
        public int TranCode { get; set; }

        [Required]
        [StringLength(20)]
        [Display(Name = "Product Code")]
        public string ProdCode { get; set; }

        [Required]
        [Range(0, double.MaxValue)]
        [Display(Name = "Quantity")]
        public decimal Qty { get; set; }

        [Required]
        [Range(0, double.MaxValue)]
        [Display(Name = "Unit Cost")]
        public decimal UnitCost { get; set; }

        [Required]
        [Range(0, double.MaxValue)]
        [Display(Name = "Total Amount")]
        public decimal TotalAmount { get; set; }

        [Required]
        [Range(0, double.MaxValue)]
        [Display(Name = "Quantity Returned")]
        public int QtyReturned { get; set; }

        [StringLength(25)]
        [Display(Name = "Stock Account Number")]
        public string StockAccountNo { get; set; }

        [StringLength(25)]
        [Display(Name = "Cost Account Number")]
        public string CostAccountNo { get; set; }

        [Required]
        [Range(0, double.MaxValue)]
        [Display(Name = "VAT Amount")]
        public decimal VatAmount { get; set; }

        [Required]
        [Range(0, double.MaxValue)]
        [Display(Name = "Withholding Amount")]
        public decimal WithHolding { get; set; }

        [Required]
        [Display(Name = "Journalized")]
        public bool Journalize { get; set; }

        [Required]
        [StringLength(20)]
        [Display(Name = "Created By")]
        public string CrtBy { get; set; }

        [Required]
        [Display(Name = "Created Date")]
        public DateTime CrtDt { get; set; }

        [Required]
        
        [Display(Name = "Created Workstation")]
        public string CrtWs { get; set; }

        [StringLength(20)]
        [Display(Name = "Stock Cost Center")]
        public string Stockcc { get; set; }

        [StringLength(20)]
        [Display(Name = "Cost Center")]
        public string Costcc { get; set; }

        [StringLength(20)]
        [Display(Name = "Other Cost Center")]
        public string CostCenter { get; set; }

        [Display(Name = "Quantity Received")]
        public int QtyRecived { get; set; }

        [Display(Name = "Quantity Issued")]
        public int QtyIssued { get; set; }

        
        [Display(Name = "Branch")]
        public string Branch { get; set; }

        [StringLength(20)]
        [Display(Name = "Other Reference Number")]
        public string otherRefNo { get; set; }

        
        [Display(Name = "Referred From Core")]
        public string referFromCore { get; set; }

        [StringLength(60)]
        [Display(Name = "Approval Status")]
        public string ApprvalStatus { get; set; }

        [StringLength(150)]
        [Display(Name = "Posted By")]
        public string postedBy { get; set; }

        [Display(Name = "Posted Date")]
        public DateTime? postDate { get; set; }

        [Display(Name = "RefDate")]
        public DateTime? RefDate { get; set; }
        [StringLength(10)]
        [Display(Name = "Category Type")]
        public string CatType { get; set; }

        
        [Display(Name = "Posted Workstation")]
        public string PstWs { get; set; }

        
        [Display(Name = "Posted Approved By")]
        public string postedApprovedBy { get; set; }

        [Required]
        
        [Display(Name = "Aibor Aic")]
        public string aiborAic { get; set; } = "NET";
        [Required]
        [Range(0, double.MaxValue)]
        [Display(Name = "Quantity to Issued")]
        public int QtyToBeIssued { get; set; }

    }
}