using DAL.Validation;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace DAL.Models
{
    [Index(nameof(Username), IsUnique = true)]
    public class User
    {
        [Key]
        public long Id { get; set; }

        [Required(ErrorMessage = "Username is required")]
        [UniqueUsername(ErrorMessage = "Username is taken")]
        [MaxLength(100)]
        public required string Username { get; set; }

        [Required(ErrorMessage = "Full Name is required")]
        [MaxLength(200)]
        public required string FullName { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        [MaxLength(200)]
        public required string Email { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; }
    }
}
