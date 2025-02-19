using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MMS.Models.Inventory
{
    using System;
    using System.ComponentModel.DataAnnotations;

    public class CompanyVM
    {
        [Required]
        [StringLength(50, ErrorMessage = "Company name cannot exceed 50 characters.")]
        public string CompanyName { get; set; }

        
        public string Country { get; set; }

        
        public string City { get; set; }

        
        public string Region { get; set; }

        
        public string Woreda { get; set; }

        
        public string Kebele { get; set; }

        
        public string HouseNo { get; set; }

        
        public string POBox { get; set; }

        [Phone]
        
        public string Tel1 { get; set; }

        [Phone]
        
        public string Tel2 { get; set; }

        
        public string Fax { get; set; }

        [Url]
        [StringLength(100)]
        public string Website { get; set; }

        [EmailAddress]
        
        public string Email { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Max Period must be a positive number.")]
        public int? MaxPeriod { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Current Period must be a positive number.")]
        public int? CurrentPeriod { get; set; }

        
        public string VATTinNumber { get; set; }

        [DataType(DataType.Date)]
        public DateTime? VATTinDate { get; set; }

        [StringLength(10)]
        public string VATTinDateEth { get; set; }

        public bool? UseEthDate { get; set; }

        [DataType(DataType.Date)]
        public DateTime? YearStart { get; set; }

        [DataType(DataType.Date)]
        public DateTime? YearEnd { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Fiscal Year must be a positive number.")]
        public int? FiscalYear { get; set; }

        public bool DepCalCulated { get; set; } = false;

        [Range(0, (double)decimal.MaxValue, ErrorMessage = "Final Book Value must be a positive number.")]
        public decimal? FinalBookValue { get; set; }

        
        public string DepCalcMethod { get; set; }

        public int? ConsiderFaCalc { get; set; }

        public bool IsBasedBookValue { get; set; } = false;

        public bool CheckChartofAcct { get; set; } = false;

        [StringLength(20)]
        public string DbStart { get; set; }

        [Range(0, (double)decimal.MaxValue, ErrorMessage = "App Value must be a positive number.")]
        public decimal? AppValue { get; set; } = 0;

        
        public string BackupDirOnSrv { get; set; }
    }

}