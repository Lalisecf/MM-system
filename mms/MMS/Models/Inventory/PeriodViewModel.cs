using System;
using System.ComponentModel.DataAnnotations;

namespace MMS.Models.Inventory
{
    public class PeriodViewModel
    {
        [Required]
        [Range(2004, int.MaxValue, ErrorMessage = "Fiscal Year must be a valid year starting from 2004.")]
        public int FiscalYear { get; set; } = 2004;

        [Required]
        public int PeriodId { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime EndDate { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Period must be a positive number.")]
        public int? Period { get; set; }

        public bool IsActive { get; set; } = false;

        public bool DepCalCulated { get; set; } = false;

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Number of Days must be a positive number.")]
        public int NoOfDays { get; set; } = 30;

        public bool? Final { get; set; }

        public bool? Draft { get; set; }

        [Required]
        public bool Closed { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Per Period must be a positive number.")]
        public int? PerPeriod { get; set; }
    }
}
