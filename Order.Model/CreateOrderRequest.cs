using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Order.Model
{
    // DTO representing the expected incoming payload and validation rules for creating a new order
    public class CreateOrderRequest
    {
        [Required(ErrorMessage = "ResellerId is required.")]
        public string ResellerId { get; set; }

        [Required(ErrorMessage = "CustomerId is required.")]
        public string CustomerId { get; set; }

        [Required(ErrorMessage = "At least one order item is required.")]
        [MinLength(1, ErrorMessage = "An order must contain at least one item.")]
        public List<CreateOrderItemDto> Items { get; set; }
    }

    // DTO representing an individual product line item inside the new order request
    public class CreateOrderItemDto
    {
        [Required(ErrorMessage = "ProductId is required.")]
        public string ProductId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1.")]
        public int Quantity { get; set; }
    }
}