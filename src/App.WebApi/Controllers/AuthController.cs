using Microsoft.AspNetCore.Mvc;
using App.Shared.Models;

namespace App.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    [HttpPost("login")]
    public ActionResult<LoginResponse> Login([FromBody] LoginRequest request)
    {
        // Тестовые пользователи (без БД!)
        if (request.Login == "admin" && request.Password == "123")
        {
            return Ok(new LoginResponse
            {
                Success = true,
                Message = "Вход выполнен",
                User = new User { Id = 1, Login = "admin", Name = "Администратор", Role = "admin" }
            });
        }

        if (request.Login == "manager" && request.Password == "123")
        {
            return Ok(new LoginResponse
            {
                Success = true,
                Message = "Вход выполнен",
                User = new User { Id = 2, Login = "manager", Name = "Иванов Иван", Role = "manager" }
            });
        }

        if (request.Login == "client" && request.Password == "123")
        {
            return Ok(new LoginResponse
            {
                Success = true,
                Message = "Вход выполнен",
                User = new User { Id = 3, Login = "client", Name = "Петров Петр", Role = "client" }
            });
        }

        return Ok(new LoginResponse
        {
            Success = false,
            Message = "Неверный логин или пароль"
        });
    }
}