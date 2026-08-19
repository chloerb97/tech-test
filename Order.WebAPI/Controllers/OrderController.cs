using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Order.Service;
using Order.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace OrderService.WebAPI.Controllers
{
    [ApiController]
    [Route("orders")]
    public class OrderController : ControllerBase
    {
        private readonly IOrderService _orderService;

        public OrderController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]

        // gets orders with optional  status filter
        public async Task<IActionResult> Get([FromQuery] string? status = null)
        {
            var orders = await _orderService.GetOrdersAsync(status);
            return Ok(orders);
        }

        [HttpGet("{orderId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetOrderById(Guid orderId)
        {
            var order = await _orderService.GetOrderByIdAsync(orderId);
            if (order != null)
            {
                return Ok(order);
            }
            else
            {
                return NotFound();
            }
        }

        [HttpPut("{orderId}/status")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateOrderStatus(Guid orderId, [FromBody] UpdateStatusRequest request)
        {
            var success = await _orderService.UpdateOrderStatusAsync(orderId, request.Status);
            if (!success)
            {
                return NotFound();
            }
            return NoContent();
        }

        // Handles HTTP POST requests to create a new order
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var createdOrder = await _orderService.CreateOrderAsync(request);
            return CreatedAtAction(nameof(GetOrderById), new { orderId = createdOrder.Id }, createdOrder);
        }
    }
    public class UpdateStatusRequest
    {
        public string Status { get; set; }
    }

}



