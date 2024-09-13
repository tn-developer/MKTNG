using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace pcg.Models
{
    public class UsersModel
    {
        public class EmailFormatAttribute : RegularExpressionAttribute
        {
            public EmailFormatAttribute() : base(@"^(?=.*@(yahoo\.com|gmail\.com)$).*$")
            {
                ErrorMessage = "Invalid only @yahoo or @gmail is allowed.";
            }
        }
        [Required(ErrorMessage = "Username is required.")]
        public string Username { get; set; }
        public string Usernamecheck { get; set; }        
        [Required(ErrorMessage = "Password is required")]
        public string Password { get; set; }
        [Required(ErrorMessage = "Please confirm your password")]
        [Compare("Password", ErrorMessage = "Passwords do not match")]
        public string Confirm { get; set; }
        [Required(ErrorMessage = "Name is required.")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Email is required.")]
        [EmailFormat(ErrorMessage = "Invalid Email Address")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Position is required")]
        public string Position { get; set; }

        [Required(ErrorMessage = "Contact number is required")]
        public string ContactNo{ get; set; }
        public string Status { get; set; }
    }
    public class Login
    {
        [Required(ErrorMessage = "Invalid username")]
        public string Username { get; set; }
        [Required(ErrorMessage = "Invalid password")]
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }
    public class ChangeInfo
    {
        public class EmailFormatAttribute : RegularExpressionAttribute
        {
            public EmailFormatAttribute() : base(@"^(?=.*@(yahoo\.com|gmail\.com)$).*$")
            {
                ErrorMessage = "Invalid only @yahoo or @gmail is allowed.";
            }
        }
        public string Name { get; set; }
        public string Password { get; set;}
        [Compare("Password", ErrorMessage = "Passwords do not match.")]
        public string Confirm { get; set; }
        [EmailFormat]
        public string Email { get; set;}
        public string ContactNo { get; set; }
        public string CurName { get; set; }
        public string CurPass { get; set; }
        public string CurEmail { get; set; }
        public string CurContact { get; set; }
    }
}
