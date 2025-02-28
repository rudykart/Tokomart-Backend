using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace TokoMart.DTO
{
    public class DiscountDto : IValidatableObject
    {
        [Required(ErrorMessage = "Discount value is required.")]
        [Range(1, 100, ErrorMessage = "Discount value must be between 1 and 100.")]
        public int DiscountValue { get; set; }

        [Required(ErrorMessage = "Start date is required.")]
        public DateTime? StartAt { get; set; }

        [Required(ErrorMessage = "Expiration date is required.")]
        public DateTime? ExpiredAt { get; set; }

        [Required(ErrorMessage = "Product ID is required.")]
        [MinLength(1, ErrorMessage = "Product ID cannot be empty.")]
        public string ProductId { get; set; } = string.Empty; // Foreign key

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var today = DateTime.UtcNow.Date;

            // Validasi StartAt tidak boleh kurang dari hari ini
            if (StartAt.HasValue && StartAt.Value.Date < today)
            {
                yield return new ValidationResult(
                    "Start date cannot be in the past.",
                    new[] { nameof(StartAt) }
                );
            }

            // Validasi ExpiredAt harus lebih besar dari StartAt
            if (StartAt.HasValue && ExpiredAt.HasValue && StartAt.Value >= ExpiredAt.Value)
            {
                yield return new ValidationResult(
                    "Expiration date must be later than the start date.",
                    new[] { nameof(ExpiredAt) }
                );
            }
        }
    }
}
