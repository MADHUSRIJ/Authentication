﻿using System.ComponentModel.DataAnnotations;

namespace JwtAuth.Models
{
    public class AuthUserModel
    {
        [Key]
        [Required]
        public int UserId{ get; set; }
        [Required]
        public string? UserName { get; set; }
        [Required] 
        public string? Password { get; set; }

        public string? token { get; set; }
    }
}
