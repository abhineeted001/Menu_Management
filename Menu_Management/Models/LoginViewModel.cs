using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Menu_Management.Models
{
    public class LoginViewModel
    {
        [Required]
        [EmailAddress]
        public string EMAIL_ID { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string PASSWORD { get; set; }
    }
}