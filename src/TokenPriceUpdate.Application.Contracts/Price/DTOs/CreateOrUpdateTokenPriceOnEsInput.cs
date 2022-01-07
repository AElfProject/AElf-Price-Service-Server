using System;

namespace TokenPriceUpdate.Price.DTOs
{
    public class CreateOrUpdateTokenPriceOnEsInput: CreateOrUpdateTokenPriceInput
    {
        public Guid Id { get; set; }
    }
}