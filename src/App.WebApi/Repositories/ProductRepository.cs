using Microsoft.Data.Sqlite;
using App.Shared.Models;
using App.WebApi.Data;

namespace App.WebApi.Repositories;

public class ProductRepository
{
    public async Task<List<Product>> GetAllAsync()
    {
        var products = new List<Product>();
        
        using var connection = DatabaseHelper.GetConnection();
        await connection.OpenAsync();
        
        var query = "SELECT * FROM Products ORDER BY Name";
        using var command = new SqliteCommand(query, connection);
        using var reader = await command.ExecuteReaderAsync();
        
        while (await reader.ReadAsync())
        {
            products.Add(new Product
            {
                Id = Convert.ToInt32(reader["Id"]),
                Name = reader["Name"].ToString() ?? "",
                Count = Convert.ToInt32(reader["Count"]),
                Price = Convert.ToDecimal(reader["Price"]),
                Photo = reader["Photo"]?.ToString(),
                Description = reader["Description"]?.ToString(),
                CategoryId = Convert.ToInt32(reader["CategoryId"]),
                CategoryName = reader["CategoryName"]?.ToString() ?? "",
                Provider = reader["Provider"]?.ToString() ?? "",
                Brand = reader["Brand"]?.ToString() ?? "",
                Discount = Convert.ToDecimal(reader["Discount"]),
                Unit = reader["Unit"]?.ToString() ?? "пара"
            });
        }
        
        return products;
    }
    
    public async Task<Product?> GetByIdAsync(int id)
    {
        using var connection = DatabaseHelper.GetConnection();
        await connection.OpenAsync();
        
        var query = "SELECT * FROM Products WHERE Id = @id";
        using var command = new SqliteCommand(query, connection);
        command.Parameters.AddWithValue("@id", id);
        
        using var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return new Product
            {
                Id = Convert.ToInt32(reader["Id"]),
                Name = reader["Name"].ToString() ?? "",
                Count = Convert.ToInt32(reader["Count"]),
                Price = Convert.ToDecimal(reader["Price"]),
                Photo = reader["Photo"]?.ToString(),
                Description = reader["Description"]?.ToString(),
                CategoryId = Convert.ToInt32(reader["CategoryId"]),
                CategoryName = reader["CategoryName"]?.ToString() ?? "",
                Provider = reader["Provider"]?.ToString() ?? "",
                Brand = reader["Brand"]?.ToString() ?? "",
                Discount = Convert.ToDecimal(reader["Discount"]),
                Unit = reader["Unit"]?.ToString() ?? "пара"
            };
        }
        return null;
    }
    
    public async Task<int> AddAsync(Product product)
    {
        using var connection = DatabaseHelper.GetConnection();
        await connection.OpenAsync();
        
        var query = @"
            INSERT INTO Products (Name, Count, Price, Photo, Description, CategoryId, CategoryName, Provider, Brand, Discount, Unit)
            VALUES (@Name, @Count, @Price, @Photo, @Description, @CategoryId, @CategoryName, @Provider, @Brand, @Discount, @Unit);
            SELECT last_insert_rowid();
        ";
        
        using var command = new SqliteCommand(query, connection);
        command.Parameters.AddWithValue("@Name", product.Name);
        command.Parameters.AddWithValue("@Count", product.Count);
        command.Parameters.AddWithValue("@Price", product.Price);
        command.Parameters.AddWithValue("@Photo", product.Photo ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@Description", product.Description ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@CategoryId", product.CategoryId);
        command.Parameters.AddWithValue("@CategoryName", product.CategoryName);
        command.Parameters.AddWithValue("@Provider", product.Provider);
        command.Parameters.AddWithValue("@Brand", product.Brand);
        command.Parameters.AddWithValue("@Discount", product.Discount);
        command.Parameters.AddWithValue("@Unit", product.Unit);
        
        return Convert.ToInt32(await command.ExecuteScalarAsync());
    }
    
    public async Task<bool> UpdateAsync(Product product)
    {
        using var connection = DatabaseHelper.GetConnection();
        await connection.OpenAsync();
        
        var query = @"
            UPDATE Products SET 
                Name = @Name,
                Count = @Count,
                Price = @Price,
                Photo = @Photo,
                Description = @Description,
                CategoryId = @CategoryId,
                CategoryName = @CategoryName,
                Provider = @Provider,
                Brand = @Brand,
                Discount = @Discount,
                Unit = @Unit
            WHERE Id = @Id
        ";
        
        using var command = new SqliteCommand(query, connection);
        command.Parameters.AddWithValue("@Id", product.Id);
        command.Parameters.AddWithValue("@Name", product.Name);
        command.Parameters.AddWithValue("@Count", product.Count);
        command.Parameters.AddWithValue("@Price", product.Price);
        command.Parameters.AddWithValue("@Photo", product.Photo ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@Description", product.Description ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@CategoryId", product.CategoryId);
        command.Parameters.AddWithValue("@CategoryName", product.CategoryName);
        command.Parameters.AddWithValue("@Provider", product.Provider);
        command.Parameters.AddWithValue("@Brand", product.Brand);
        command.Parameters.AddWithValue("@Discount", product.Discount);
        command.Parameters.AddWithValue("@Unit", product.Unit);
        
        return await command.ExecuteNonQueryAsync() > 0;
    }
    
    public async Task<bool> DeleteAsync(int id)
    {
        using var connection = DatabaseHelper.GetConnection();
        await connection.OpenAsync();
        
        // Временно отключаем проверку внешних ключей
        using var pragmaCmd = new SqliteCommand("PRAGMA foreign_keys = OFF;", connection);
        await pragmaCmd.ExecuteNonQueryAsync();
        
        try
        {
            var query = "DELETE FROM Products WHERE Id = @Id";
            using var command = new SqliteCommand(query, connection);
            command.Parameters.AddWithValue("@Id", id);
            var result = await command.ExecuteNonQueryAsync();
            
            // Включаем проверку обратно
            using var enableCmd = new SqliteCommand("PRAGMA foreign_keys = ON;", connection);
            await enableCmd.ExecuteNonQueryAsync();
            
            return result > 0;
        }
        catch
        {
            // Включаем проверку обратно в случае ошибки
            using var enableCmd = new SqliteCommand("PRAGMA foreign_keys = ON;", connection);
            await enableCmd.ExecuteNonQueryAsync();
            throw;
        }
    }
    
    public async Task<List<string>> GetAllProvidersAsync()
    {
        var providers = new List<string>();
        
        using var connection = DatabaseHelper.GetConnection();
        await connection.OpenAsync();
        
        var query = "SELECT DISTINCT Provider FROM Products ORDER BY Provider";
        using var command = new SqliteCommand(query, connection);
        using var reader = await command.ExecuteReaderAsync();
        
        while (await reader.ReadAsync())
        {
            providers.Add(reader["Provider"].ToString() ?? "");
        }
        
        return providers;
    }
    
    public async Task<List<CategoryDto>> GetAllCategoriesAsync()
    {
        var categories = new List<CategoryDto>();
        
        using var connection = DatabaseHelper.GetConnection();
        await connection.OpenAsync();
        
        var query = "SELECT Id, Name FROM Categories ORDER BY Name";
        using var command = new SqliteCommand(query, connection);
        using var reader = await command.ExecuteReaderAsync();
        
        while (await reader.ReadAsync())
        {
            categories.Add(new CategoryDto
            {
                Id = Convert.ToInt32(reader["Id"]),
                Name = reader["Name"].ToString() ?? ""
            });
        }
        
        return categories;
    }
}

public class CategoryDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}