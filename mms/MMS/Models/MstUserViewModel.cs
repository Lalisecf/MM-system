using System.ComponentModel.DataAnnotations;

namespace MMS.Models
{
    public class MstUserViewModel
    {
        [MinLength(6, ErrorMessage = "Minimum MstMstUsername must be 6 in charaters")]
        [Required(ErrorMessage = "MstMstUsername Required")]
        public string MstMstUserName { get; set; }

        [Required(ErrorMessage = "Enter FirstName")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Enter LastName")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "EmailID Required")]
        [RegularExpression(@"^([a-zA-Z0-9_\-\.]+)@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([a-zA-Z0-9\-]+\.)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$", ErrorMessage = "Please enter a valid e-mail adress")]
        public string EmailId { get; set; }

        [Required(ErrorMessage = "Mobileno Required")]
        [RegularExpression(@"^(\d{10})$", ErrorMessage = "Wrong Mobileno")]
        [MaxLength(10)]
        public string MobileNo { get; set; }

        [Required(ErrorMessage = "Gender Required")]
        public string Gender { get; set; }
        public bool? Status { get; set; }
        
    }
}
