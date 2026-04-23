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

        [Required]
        [MaxLength(100)]
        public required string Username { get; set; }

        [Required]
        [MaxLength(200)]
        public required string FullName { get; set; }

        [Required]
        [MaxLength(200)]
        public required string Email { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; }
    }
}
