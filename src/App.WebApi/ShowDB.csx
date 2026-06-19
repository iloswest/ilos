using Microsoft.Data.Sqlite;
using var connection = new SqliteConnection("Data Source=shoestore.db");
connection.Open();

Console.WriteLine("=== Products ===");
var cmd = new SqliteCommand("SELECT * FROM Products", connection);
using var reader = cmd.ExecuteReader();
while (reader.Read())
{
    Console.WriteLine($"Id: {reader["Id"]}, Name: {reader["Name"]}, Count: {reader["Count"]}, Price: {reader["Price"]}");
}
