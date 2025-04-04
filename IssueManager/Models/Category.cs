﻿using System.ComponentModel.DataAnnotations;

namespace IssueManager.Models
{
    public class Category
    {
        public int Id { get; set; } 
        [Required]
        [MaxLength(50)]
        public string Name { get; set; } = string.Empty;
    }
}
