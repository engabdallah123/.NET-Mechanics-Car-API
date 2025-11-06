using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.DTO.Account
{
    public class RegisterDTO
    {
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        [Compare("Password", ErrorMessage = "Passwords do not match")]
        public string RePassword { get; set; }
    }
}
