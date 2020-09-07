using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace TaskAuthenticationAuthorization.Models
{
    public class LoginModel 
    {
        [Required(ErrorMessage = "Email not entered")]
        public string Email { get; set; }

         [Required(ErrorMessage = "Password not entered")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

          }
}
