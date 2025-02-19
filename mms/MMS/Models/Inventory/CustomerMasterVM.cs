using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace MMS.Models.Inventory
{
    public class CustomerMasterVM
    {
        public string customerNumber { get; set; }
        public string customerName { get; set; }
        public string customerAddr { get; set; }
        public string CustomerTel { get; set; }
        public string CustomerFax { get; set; }
        public decimal? TotalPurchase { get; set; }
        public decimal? Balance { get; set; }
        public decimal? CreditLimit { get; set; }
        public bool? OnHold { get; set; }
        public bool? AllowCredit { get; set; }
        public bool? Delear { get; set; }
        public int? TransactionNumber { get; set; }
        public int? Category { get; set; }
        public string CostCenter { get; set; }
        public string AccountNo { get; set; }
        public string Contact { get; set; }
        public string email { get; set; }
        public string Comment { get; set; }
        public string SupplierType { get; set; }
        public string SupplierCode { get; set; }
        public long? VatRegistered { get; set; }
        public string VatRegNo { get; set; }
        public DateTime? VatRegDate { get; set; }
        public string TinNo { get; set; }
        public int? CreditTerms { get; set; }
        public int? Status { get; set; }
        public bool? IsCustomer { get; set; }
        public string SuppProcurmentCode { get; set; }
        public string VatRegDateEth { get; set; }
        public bool IsActive { get; set; }
        public string pOBox { get; set; }
        public bool IsWithholding { get; set; }
    }

}