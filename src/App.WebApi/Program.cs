using App.WebApi.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddCors();

var app = builder.Build();

// Инициализация базы данных
DatabaseHelper.InitializeDatabase();

// НАСТРОЙКА СТАТИЧЕСКИХ ФАЙЛОВ (для изображений)
app.UseStaticFiles();

app.UseCors(x => x.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
app.MapControllers();

app.Run();
