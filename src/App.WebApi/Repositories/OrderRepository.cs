using Microsoft.Data.Sqlite;
using App.Shared.Models;
using App.WebApi.Data;

namespace App.WebApi.Repositories;

public class OrderRepository
{
    public async Task<(bool Success, string Message, int OrderId)> CreateOrderAsync(CreateOrderRequest request)
    {
        using var connection = DatabaseHelper.GetConnection();
        await connection.OpenAsync();
        
        // Используем синхронную транзакцию
        using var transaction = connection.BeginTransaction();
        
        try
        {
            // 1. Проверяем остатки по каждому товару
            foreach (var item in request.Items)
            {
                var checkStock = "SELECT Count, Name FROM Products WHERE Id = @ProductId";
                using var checkCmd = new SqliteCommand(checkStock, connection, transaction);
                checkCmd.Parameters.AddWithValue("@ProductId", item.ProductId);
                using var reader = await checkCmd.ExecuteReaderAsync();
                
                if (await reader.ReadAsync())
                {
                    var stock = reader.GetInt32(0);
                    var productName = reader.GetString(1);
                    
                    if (stock < item.Quantity)
                    {
                        await transaction.RollbackAsync();
                        return (false, $"Товара \"{productName}\" недостаточно! В наличии: {stock}, запрошено: {item.Quantity}", 0);
                    }
                }
                else
                {
                    await transaction.RollbackAsync();
                    return (false, $"Товар с ID {item.ProductId} не найден", 0);
                }
            }
            
            // 2. Вычисляем общую стоимость
            decimal totalCost = 0;
            foreach (var item in request.Items)
            {
                var getPrice = "SELECT Price, Discount FROM Products WHERE Id = @ProductId";
                using var priceCmd = new SqliteCommand(getPrice, connection, transaction);
                priceCmd.Parameters.AddWithValue("@ProductId", item.ProductId);
                using var reader = await priceCmd.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    var price = reader.GetDecimal(0);
                    var discount = reader.GetDecimal(1);
                    var finalPrice = price - (price * discount / 100);
                    totalCost += finalPrice * item.Quantity;
                }
            }
            
            // 3. Создаем заказ
            var insertOrder = @"
                INSERT INTO Orders (UserId, TotalCost, Status) 
                VALUES (@UserId, @TotalCost, 'Новый');
                SELECT last_insert_rowid();
            ";
            
            using var orderCmd = new SqliteCommand(insertOrder, connection, transaction);
            orderCmd.Parameters.AddWithValue("@UserId", request.UserId);
            orderCmd.Parameters.AddWithValue("@TotalCost", totalCost);
            var orderId = Convert.ToInt32(await orderCmd.ExecuteScalarAsync());
            
            // 4. Добавляем товары в заказ и обновляем остатки
            foreach (var item in request.Items)
            {
                var getPrice = "SELECT Price, Discount FROM Products WHERE Id = @ProductId";
                using var priceCmd = new SqliteCommand(getPrice, connection, transaction);
                priceCmd.Parameters.AddWithValue("@ProductId", item.ProductId);
                using var reader = await priceCmd.ExecuteReaderAsync();
                decimal price = 0, discount = 0;
                if (await reader.ReadAsync())
                {
                    price = reader.GetDecimal(0);
                    discount = reader.GetDecimal(1);
                }
                
                var finalPrice = price - (price * discount / 100);
                
                var insertItem = @"
                    INSERT INTO OrderItems (OrderId, ProductId, Quantity, Price, Discount) 
                    VALUES (@OrderId, @ProductId, @Quantity, @Price, @Discount)
                ";
                using var itemCmd = new SqliteCommand(insertItem, connection, transaction);
                itemCmd.Parameters.AddWithValue("@OrderId", orderId);
                itemCmd.Parameters.AddWithValue("@ProductId", item.ProductId);
                itemCmd.Parameters.AddWithValue("@Quantity", item.Quantity);
                itemCmd.Parameters.AddWithValue("@Price", finalPrice);
                itemCmd.Parameters.AddWithValue("@Discount", discount);
                await itemCmd.ExecuteNonQueryAsync();
                
                var updateStock = "UPDATE Products SET Count = Count - @Quantity WHERE Id = @ProductId";
                using var stockCmd = new SqliteCommand(updateStock, connection, transaction);
                stockCmd.Parameters.AddWithValue("@Quantity", item.Quantity);
                stockCmd.Parameters.AddWithValue("@ProductId", item.ProductId);
                await stockCmd.ExecuteNonQueryAsync();
            }
            
            await transaction.CommitAsync();
            return (true, "Заказ успешно оформлен!", orderId);
        }
        catch (Exception ex)
        {
            try
            {
                await transaction.RollbackAsync();
            }
            catch { }
            return (false, $"Ошибка: {ex.Message}", 0);
        }
    }
}
