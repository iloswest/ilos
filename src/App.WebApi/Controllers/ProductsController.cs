using Microsoft.AspNetCore.Mvc;
using App.Shared.Models;
using App.WebApi.Repositories;

namespace App.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly ProductRepository _repository;
    
    public ProductsController()
    {
        _repository = new ProductRepository();
    }
    
    [HttpGet]
    public async Task<ActionResult<List<Product>>> GetAll()
    {
        var products = await _repository.GetAllAsync();
        return Ok(products);
    }
    
    [HttpGet("{id}")]
    public async Task<ActionResult<Product>> GetById(int id)
    {
        var product = await _repository.GetByIdAsync(id);
        if (product == null)
            return NotFound();
        return Ok(product);
    }
    
    [HttpPost]
    public async Task<ActionResult<int>> AddProduct([FromBody] Product product)
    {
        var id = await _repository.AddAsync(product);
        return Ok(id);
    }
    
    [HttpPut("{id}")]
    public async Task<ActionResult> UpdateProduct(int id, [FromBody] Product product)
    {
        if (id != product.Id)
            return BadRequest();
        
        var existing = await _repository.GetByIdAsync(id);
        if (existing == null)
            return NotFound();
        
        await _repository.UpdateAsync(product);
        return Ok();
    }
    
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteProduct(int id)
    {
        var existing = await _repository.GetByIdAsync(id);
        if (existing == null)
            return NotFound();
        
        await _repository.DeleteAsync(id);
        return Ok();
    }
    
    [HttpGet("providers")]
    public async Task<ActionResult<List<string>>> GetProviders()
    {
        var providers = await _repository.GetAllProvidersAsync();
        return Ok(providers);
    }
    
    [HttpGet("categories")]
    public async Task<ActionResult<List<CategoryDto>>> GetCategories()
    {
        var categories = await _repository.GetAllCategoriesAsync();
        return Ok(categories);
    }
}

public class CategoryDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}
