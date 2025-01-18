using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MMS.Models
{
    [NotMapped]
    public class LoginViewModells
    {
        [Required(ErrorMessage = "Username Required")]
        public string UserName { get; set; }
        public string Password { get; set; }
        [Display(Name = "Employee ID")]
        public string EmpId { get; set; }
        [Display(Name = "Email Address")]
        [RegularExpression(@".*@awashbank.com", ErrorMessage = "must be a @awashbank.com email address")]
        public string EmailId { get; set; }        
        public long UserId { get; set; }
        [Display(Name = "Mobile No")]
        [MinLength(10, ErrorMessage = "Mobile No minimum length is 10 digit")]
        [MaxLength(12, ErrorMessage = "Mobile No maximum length is 12 digit")]
        [RegularExpression("^251?[1-9][0-9]{8}$", ErrorMessage = "Mobile No must be start with 2519xxxx,number only and 12 digit")]
        public string MobileNo { get; set; }
        public string OTP { get; set; }
        public bool RememberMeSet { get; set; }
        public DateTime PasswordChangedDate { get; set; }
        public int attempt { get; set; }
        public Guid? SessionID { get; set; }
    }

}
