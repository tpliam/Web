﻿using System.ComponentModel.DataAnnotations;

namespace Web.Models
{
    public class Color
    {
        [Key]
        public int ColorId {  get; set; }
        [StringLength(30)]
        public string? ColorName { get; set; }
    }
}
