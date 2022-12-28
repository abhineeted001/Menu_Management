//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Menu_Management.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Web.Mvc;
    using CompareAttribute = System.ComponentModel.DataAnnotations.CompareAttribute;

    public partial class register_user
    {
        public int USER_ID { get; set; }
        [Required]
        [RegularExpression("^[a-zA-Z0-9_\\.-]+@([a-zA-Z0-9-]+\\.)+[a-zA-Z]{2,6}$", ErrorMessage = "Invalid Email-Id")]
        [Remote("EmailExists", "Account", HttpMethod = "POST", ErrorMessage = "You already have an account with us. Please Login.")]
        public string EMAIL_ID { get; set; }
        public string FIRST_NAME { get; set; }
        public string LAST_NAME { get; set; }
        public string GENDER { get; set; }
        [Required]
        [DataType(DataType.Password)]
        [MinLength(8, ErrorMessage = "Password must be 8 characters or more")]
        public string PASSWORD { get; set; }

        [NotMapped]
        [Compare("PASSWORD", ErrorMessage ="Password does not match")]
        public string CONFIRM_PASSWORD { get; set; }
        [MaxLength(10, ErrorMessage = "Phone number cannot be more than 10 digits")]
        public string PHONE { get; set; }
    }
}
