using Microsoft.AspNetCore.Mvc;
using App.Shared.Models;
using App.WebApi.Repositories;

namespace App.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly OrderRepository _repository;
    
    public OrdersController()
    {
        _repository = new OrderRepository();
    }
    
    [HttpPost]
    public async Task<ActionResult> CreateOrder([FromBody] CreateOrderRequest request)
    {
        if (request == null || request.Items == null || request.Items.Count == 0)
        {
            return BadRequest(new { success = false, message = "Корзина пуста!" });
        }
        
        var result = await _repository.CreateOrderAsync(request);
        
        if (result.Success)
        {
            return Ok(new { success = true, message = result.Message, orderId = result.OrderId });
        }
        else
        {
            return BadRequest(new { success = false, message = result.Message });
        }
    }
}
