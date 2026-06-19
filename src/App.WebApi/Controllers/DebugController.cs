using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;

namespace App.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DebugController : ControllerBase
{
    [HttpGet]
    public IActionResult Index()
    {
        var result = new List<object>();
        
        using var connection = new SqliteConnection("Data Source=shoestore.db");
        connection.Open();
        
        var cmd = new SqliteCommand("SELECT Id, Name, Count, Price, Discount FROM Products", connection);
        using var reader = cmd.ExecuteReader();
        
        while (reader.Read())
        {
            result.Add(new
            {
                Id = reader["Id"],
                Name = reader["Name"],
                Count = reader["Count"],
                Price = reader["Price"],
                Discount = reader["Discount"]
            });
        }
        
        return Ok(result);
    }
}
