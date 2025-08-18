using Microsoft.AspNetCore.Mvc;
using PaymentSystem.Application.Abstractions;
using PaymentSystem.Application.Models.Orders;

namespace PaymentSystem.Web.Controllers
{
    [Route("api/v1/orders")]
    public class OrdersController(IOrdersService orders) : ApiBaseController
    {
        [HttpPost]
        public async Task<IActionResult> Create(CreateOrderDto request)
        {
            var result = await orders.Create(request);
            return Ok(result);
        }

        [HttpGet("{orderId:long}")]
        public async Task<IActionResult> GetById(long orderId)
        {
            var result = await orders.GetById(orderId);
            return Ok(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await orders.GetAll();
            return Ok(new { orders = result });
        }

        [HttpGet("customers/{customerId:long}")]
        public async Task<IActionResult> GetByUser(long customerId)
        {
            var result = await orders.GetByUser(customerId);
            return Ok(new { orders = result });
        }

        [HttpPost("reject/{orderId:long}")]
        public async Task<IActionResult> Reject(long orderId)
        {
            await orders.Reject(orderId);
            return Ok();
        }
    }
}