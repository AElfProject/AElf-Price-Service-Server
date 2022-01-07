using System;
using System.ComponentModel.DataAnnotations;

namespace TokenPriceUpdate.Price.DTOs
{
    public class CreateOrUpdateTokenPriceInput
    {
        [Required]
        public string Token { get; set; }
        [Required]
        public string UnderlyingToken { get; set; }
        [Required]
        public decimal Price { get; set; }
        [Required]
        public PriceType PriceType { get; set; }
        [Required]
        public DateTime DateTime { get; set; }
    }
}