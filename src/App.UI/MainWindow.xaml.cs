using System.Windows;
using System.Net.Http;
using System.Net.Http.Json;
using App.Shared.Models;
using System.Collections.Generic;

namespace AppUI
{
    public partial class MainWindow : Window
    {
        private readonly HttpClient _httpClient = new();
        private readonly string _baseUrl = "http://localhost:5271";  // ← ВАШ ПОРТ

        public MainWindow()
        {
            InitializeComponent();
            LoadProducts();
        }

        private async void LoadProducts()
        {
            try
            {
                var products = await _httpClient.GetFromJsonAsync<List<Product>>($"{_baseUrl}/api/products");
                ProductsList.ItemsSource = products;
                MessageBox.Show($"Загружено {products?.Count} товаров", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки: {ex.Message}\n\nПроверьте, запущен ли WebApi на {_baseUrl}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            var request = new LoginRequest { Login = LoginBox.Text, Password = PasswordBox.Password };

            try
            {
                var response = await _httpClient.PostAsJsonAsync($"{_baseUrl}/api/auth/login", request);
                var result = await response.Content.ReadFromJsonAsync<LoginResponse>();

                if (result?.Success == true)
                {
                    UserInfo.Text = $"Добро пожаловать, {result.User?.Name}";
                    MessageBox.Show($"Вход выполнен! Роль: {result.User?.Role}", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show(result?.Message ?? "Ошибка входа", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void GuestButton_Click(object sender, RoutedEventArgs e)
        {
            UserInfo.Text = "Вы вошли как гость";
            MessageBox.Show("Доступен только просмотр товаров", "Режим гостя", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}