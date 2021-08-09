using Microsoft.AspNetCore.Mvc;
using System;
using System.ComponentModel.DataAnnotations;

namespace DnaVastgoed.Models {

    public class Subscriber {

        [Key]
        [Required]
        [EmailAddress]
        [FromForm(Name = "form_fields[Email]")]
        public string Email { get; set; }

        [Required]
        [MaxLength(500)]
        [FromForm(Name = "form_fields[Firstname]")]
        public string Firstname { get; set; }

        [Required]
        [MaxLength(500)]
        [FromForm(Name = "form_fields[Lastname]")]
        public string Lastname { get; set; }

        [Required]
        [MaxLength(500)]
        [FromForm(Name = "form_fields[Telephone]")]
        public string Telephone { get; set; }

        [Required]
        [MaxLength(500)]
        [FromForm(Name = "form_fields[Postalcode]")]
        public string Postalcode { get; set; }

        [Required]
        [Range(0, 100)]
        [FromForm(Name = "form_fields[RadiusInKM]")]
        public int RadiusInKM { get; set; }

        [Required]
        [Range(0, 10000000)]
        [FromForm(Name = "form_fields[MinPrice]")]
        public int MinPrice { get; set; }

        [Required]
        [Range(0, 10000000)]
        [FromForm(Name = "form_fields[MaxPrice]")]
        public int MaxPrice { get; set; }

        [Required]
        [MaxLength(100)]
        [FromForm(Name = "form_fields[Status]")]
        public string Status { get; set; }

        [Required]
        [MaxLength(100)]
        [FromForm(Name = "form_fields[Type]")]
        public string Type { get; set; }

        [Required]
        [Range(0, 1000)]
        [FromForm(Name = "form_fields[Bedrooms]")]
        public int Bedrooms { get; set; }

        public DateTime? Suppressed { get; set; }
    }

}
