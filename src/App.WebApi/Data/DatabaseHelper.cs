using Microsoft.Data.Sqlite;
using System.Security.Cryptography;
using System.Text;

namespace App.WebApi.Data;

public static class DatabaseHelper
{
    private const string ConnectionString = "Data Source=shoestore.db";

    public static SqliteConnection GetConnection()
    {
        return new SqliteConnection(ConnectionString);
    }

    private static string HashPassword(string password)
    {
        if (string.IsNullOrEmpty(password))
            return string.Empty;

        using var sha256 = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(password);
        var hashBytes = sha256.ComputeHash(bytes);
        return Convert.ToHexString(hashBytes).ToLower();
    }

    public static void InitializeDatabase()
    {
        using var connection = GetConnection();
        connection.Open();

        var createTableQuery = @"
            CREATE TABLE IF NOT EXISTS Users (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Login TEXT NOT NULL UNIQUE,
                Password TEXT NOT NULL,
                Name TEXT NOT NULL,
                Role TEXT NOT NULL DEFAULT 'client'
            );

            CREATE TABLE IF NOT EXISTS Categories (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Name TEXT NOT NULL UNIQUE
            );

            CREATE TABLE IF NOT EXISTS Products (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Name TEXT NOT NULL,
                Count INTEGER NOT NULL DEFAULT 0,
                Price REAL NOT NULL,
                Photo TEXT,
                Description TEXT,
                CategoryId INTEGER NOT NULL DEFAULT 1,
                CategoryName TEXT NOT NULL DEFAULT '',
                Provider TEXT NOT NULL DEFAULT '',
                Brand TEXT NOT NULL DEFAULT '',
                Discount REAL DEFAULT 0,
                Unit TEXT DEFAULT 'пара'
            );

            CREATE TABLE IF NOT EXISTS Orders (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                UserId INTEGER NOT NULL,
                OrderDate TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
                TotalCost REAL NOT NULL DEFAULT 0,
                Status TEXT DEFAULT 'Новый'
            );

            CREATE TABLE IF NOT EXISTS OrderItems (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                OrderId INTEGER NOT NULL,
                ProductId INTEGER NOT NULL,
                Quantity INTEGER NOT NULL,
                Price REAL NOT NULL,
                Discount REAL DEFAULT 0
            );

            CREATE INDEX IF NOT EXISTS idx_orders_user ON Orders(UserId);
            CREATE INDEX IF NOT EXISTS idx_orderitems_order ON OrderItems(OrderId);
            CREATE INDEX IF NOT EXISTS idx_orderitems_product ON OrderItems(ProductId);
            CREATE INDEX IF NOT EXISTS idx_products_category ON Products(CategoryId);
        ";

        using var command = new SqliteCommand(createTableQuery, connection);
        command.ExecuteNonQuery();

        // Проверка, есть ли пользователи
        var checkUsers = "SELECT COUNT(*) FROM Users";
        using var checkCmd = new SqliteCommand(checkUsers, connection);
        var userCount = Convert.ToInt32(checkCmd.ExecuteScalar());

        if (userCount == 0)
        {
            var adminHash = HashPassword("123");
            var managerHash = HashPassword("123");
            var clientHash = HashPassword("123");

            var insertQuery = @"
                INSERT INTO Users (Login, Password, Name, Role) VALUES 
                ('admin', '" + adminHash + @"', 'Администратор', 'admin'),
                ('manager', '" + managerHash + @"', 'Иванов Иван', 'manager'),
                ('client', '" + clientHash + @"', 'Петров Петр', 'client');

                INSERT INTO Categories (Name) VALUES 
                ('Кроссовки'),
                ('Ботинки'),
                ('Туфли'),
                ('Сандалии'),
                ('Сапоги');

                INSERT INTO Products (Name, Count, Price, CategoryId, CategoryName, Provider, Brand, Discount, Unit) VALUES 
                ('Nike Air Max 270', 25, 8990.00, 1, 'Кроссовки', 'Nike', 'Nike', 15, 'пара'),
                ('Adidas Ultraboost', 12, 12990.00, 1, 'Кроссовки', 'Adidas', 'Adidas', 10, 'пара'),
                ('Columbia Winter Boot', 8, 7990.00, 2, 'Ботинки', 'Columbia', 'Columbia', 0, 'пара'),
                ('ECCO Classic', 0, 9990.00, 3, 'Туфли', 'ECCO', 'ECCO', 5, 'пара'),
                ('Puma Summer Sandals', 30, 2490.00, 4, 'Сандалии', 'Puma', 'Puma', 20, 'пара'),
                ('New Balance 574', 18, 7990.00, 1, 'Кроссовки', 'New Balance', 'New Balance', 10, 'пара'),
                ('Asics Gel-Kayano', 10, 14990.00, 1, 'Кроссовки', 'Asics', 'Asics', 15, 'пара'),
                ('Saucony Guide', 7, 11990.00, 1, 'Кроссовки', 'Saucony', 'Saucony', 5, 'пара'),
                ('Timberland Classic', 15, 15990.00, 2, 'Ботинки', 'Timberland', 'Timberland', 0, 'пара'),
                ('Caterpillar Colorado', 12, 12990.00, 2, 'Ботинки', 'Caterpillar', 'Caterpillar', 10, 'пара'),
                ('Clarks Desert Boot', 20, 8990.00, 3, 'Туфли', 'Clarks', 'Clarks', 0, 'пара'),
                ('Geox Sneakers', 14, 10990.00, 3, 'Туфли', 'Geox', 'Geox', 5, 'пара'),
                ('Teva Hurricane', 25, 3990.00, 4, 'Сандалии', 'Teva', 'Teva', 0, 'пара'),
                ('Crocs Classic', 40, 2990.00, 4, 'Сандалии', 'Crocs', 'Crocs', 0, 'пара'),
                ('Dr. Martens 1460', 9, 17990.00, 5, 'Сапоги', 'Dr. Martens', 'Dr. Martens', 0, 'пара'),
                ('UGG Classic', 11, 13990.00, 5, 'Сапоги', 'UGG', 'UGG', 15, 'пара');
            ";

            using var insertCmd = new SqliteCommand(insertQuery, connection);
            insertCmd.ExecuteNonQuery();
        }
    }
}
