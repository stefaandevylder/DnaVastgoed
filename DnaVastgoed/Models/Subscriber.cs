using Microsoft.AspNetCore.Mvc;
using System;
using System.ComponentModel.DataAnnotations;

namespace DnaVastgoed.Models {

    public class Subscriber {

        [Key]
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [MaxLength(500)]
        public string Firstname { get; set; }

        [Required]
        [MaxLength(500)]
        public string Lastname { get; set; }

        [Required]
        [MaxLength(500)]
        public string Telephone { get; set; }

        [Required]
        [MaxLength(500)]
        public string Postalcode { get; set; }

        [Required]
        [Range(0, 100)]
        public int RadiusInKM { get; set; }

        [Required]
        [Range(0, 10000000)]
        public int MinPrice { get; set; }

        [Required]
        [Range(0, 10000000)]
        public int MaxPrice { get; set; }

        [Required]
        [MaxLength(100)]
        public string Status { get; set; }

        [Required]
        [MaxLength(100)]
        public string Type { get; set; }

        [Required]
        [Range(0, 1000)]
        public int Bedrooms { get; set; }

        public DateTime? Suppressed { get; set; }
    }

}
