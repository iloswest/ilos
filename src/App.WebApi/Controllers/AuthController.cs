using Microsoft.AspNetCore.Mvc;
using App.Shared.Models;
using App.Shared.Helpers;
using Microsoft.Data.Sqlite;
using App.WebApi.Data;

namespace App.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    [HttpPost("login")]
    public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request)
    {
        try
        {
            // 1. Ищем пользователя в базе данных
            using var connection = DatabaseHelper.GetConnection();
            await connection.OpenAsync();

            var query = "SELECT Id, Login, Password, Name, Role FROM Users WHERE Login = @Login";
            using var command = new SqliteCommand(query, connection);
            command.Parameters.AddWithValue("@Login", request.Login);

            using var reader = await command.ExecuteReaderAsync();

            if (!await reader.ReadAsync())
            {
                return Ok(new LoginResponse
                {
                    Success = false,
                    Message = "Неверный логин или пароль"
                });
            }

            // 2. Получаем хеш пароля из базы
            var passwordHash = reader["Password"].ToString() ?? "";
            var userId = Convert.ToInt32(reader["Id"]);
            var name = reader["Name"].ToString() ?? "";
            var role = reader["Role"].ToString() ?? "client";

            // 3. Проверяем введенный пароль с хешем
            if (!PasswordHelper.VerifyPassword(request.Password, passwordHash))
            {
                return Ok(new LoginResponse
                {
                    Success = false,
                    Message = "Неверный логин или пароль"
                });
            }

            // 4. Успешный вход
            return Ok(new LoginResponse
            {
                Success = true,
                Message = "Вход выполнен",
                User = new User
                {
                    Id = userId,
                    Login = request.Login,
                    Name = name,
                    Role = role
                }
            });
        }
        catch (Exception ex)
        {
            return Ok(new LoginResponse
            {
                Success = false,
                Message = $"Ошибка: {ex.Message}"
            });
        }
    }

    // Регистрация нового пользователя (пример)
    [HttpPost("register")]
    public async Task<ActionResult<LoginResponse>> Register([FromBody] LoginRequest request)
    {
        try
        {
            // Хешируем пароль
            var passwordHash = PasswordHelper.HashPassword(request.Password);

            using var connection = DatabaseHelper.GetConnection();
            await connection.OpenAsync();

            var query = @"
                INSERT INTO Users (Login, Password, Name, Role)
                VALUES (@Login, @Password, @Name, @Role)
            ";

            using var command = new SqliteCommand(query, connection);
            command.Parameters.AddWithValue("@Login", request.Login);
            command.Parameters.AddWithValue("@Password", passwordHash);
            command.Parameters.AddWithValue("@Name", request.Login);
            command.Parameters.AddWithValue("@Role", "client");

            await command.ExecuteNonQueryAsync();

            return Ok(new LoginResponse
            {
                Success = true,
                Message = "Пользователь зарегистрирован"
            });
        }
        catch (Exception ex)
        {
            return Ok(new LoginResponse
            {
                Success = false,
                Message = $"Ошибка: {ex.Message}"
            });
        }
    }
}
