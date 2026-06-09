using Microsoft.AspNetCore.Mvc;
using App.Shared.Models;

namespace App.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    // Тестовые данные прямо в коде (без БД!)
    private static List<Product> _products = new()
    {
        new() { Id = 1, Name = "Nike Air Max", Count = 25, Price = 8990, CategoryId = 1, CategoryName = "Кроссовки", Provider = "Nike", Brand = "Nike", Discount = 15, Unit = "пара" },
        new() { Id = 2, Name = "Adidas Ultraboost", Count = 12, Price = 12990, CategoryId = 1, CategoryName = "Кроссовки", Provider = "Adidas", Brand = "Adidas", Discount = 10, Unit = "пара" },
        new() { Id = 3, Name = "Columbia Winter Boot", Count = 8, Price = 7990, CategoryId = 2, CategoryName = "Ботинки", Provider = "Columbia", Brand = "Columbia", Discount = 0, Unit = "пара" },
        new() { Id = 4, Name = "ECCO Classic", Count = 0, Price = 9990, CategoryId = 3, CategoryName = "Туфли", Provider = "ECCO", Brand = "ECCO", Discount = 5, Unit = "пара" },
        new() { Id = 5, Name = "Puma Summer", Count = 30, Price = 2490, CategoryId = 4, CategoryName = "Сандалии", Provider = "Puma", Brand = "Puma", Discount = 20, Unit = "пара" }
    };

    [HttpGet]
    public ActionResult<List<Product>> GetAll()
    {
        return Ok(_products);
    }

    [HttpGet("{id}")]
    public ActionResult<Product> GetById(int id)
    {
        var product = _products.FirstOrDefault(p => p.Id == id);
        if (product == null)
            return NotFound(new { message = $"Товар с ID {id} не найден" });
        return Ok(product);
    }

    [HttpGet("categories")]
    public ActionResult<object> GetCategories()
    {
        var categories = _products
            .Select(p => new { Id = p.CategoryId, Name = p.CategoryName })
            .Distinct()
            .ToList();
        return Ok(categories);
    }

    [HttpGet("providers")]
    public ActionResult<List<string>> GetProviders()
    {
        var providers = _products.Select(p => p.Provider).Distinct().ToList();
        return Ok(providers);
    }
}