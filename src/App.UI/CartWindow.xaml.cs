using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using App.Shared.Models;

namespace AppUI;

public partial class CartWindow : Window
{
    private ObservableCollection<CartItem> _cartItems;
    private readonly HttpClient _httpClient = new HttpClient();
    private readonly string _baseUrl = "http://localhost:5271";
    private readonly User? _currentUser;

    public CartWindow(ObservableCollection<CartItem> cartItems, User? currentUser = null)
    {
        InitializeComponent();
        _cartItems = cartItems;
        _currentUser = currentUser;
        CartListBox.ItemsSource = _cartItems;
        UpdateTotalPrice();

        ClearCartButton.Click += (s, e) => ClearCart();
        ContinueShoppingButton.Click += (s, e) => { DialogResult = false; Close(); };
        CheckoutButton.Click += async (s, e) => await Checkout();
    }

    private void UpdateTotalPrice()
    {
        var total = _cartItems.Sum(item => item.TotalPrice);
        TotalPriceText.Text = total.ToString("C");
    }

    private void ClearCart()
    {
        if (MessageBox.Show("Вы уверены, что хотите очистить корзину?", "Подтверждение",
            MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
        {
            _cartItems.Clear();
            UpdateTotalPrice();
        }
    }

    private async System.Threading.Tasks.Task Checkout()
    {
        if (_cartItems.Count == 0)
        {
            MessageBox.Show("Корзина пуста!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (_currentUser == null || string.IsNullOrEmpty(_currentUser.Login) || _currentUser.Login == "guest")
        {
            MessageBox.Show("Для оформления заказа необходимо авторизоваться!", "Ошибка",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var result = MessageBox.Show($"Оформить заказ на сумму {TotalPriceText.Text}?",
            "Подтверждение заказа", MessageBoxButton.YesNo, MessageBoxImage.Question);

        if (result != MessageBoxResult.Yes) return;

        try
        {
            var request = new CreateOrderRequest
            {
                UserId = _currentUser.Id,
                Items = _cartItems.Select(item => new OrderItemRequest
                {
                    ProductId = item.Product.Id,
                    Quantity = item.Quantity
                }).ToList()
            };

            var response = await _httpClient.PostAsJsonAsync($"{_baseUrl}/api/orders", request);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                // Парсим ответ
                using var doc = JsonDocument.Parse(responseContent);
                var root = doc.RootElement;
                var message = root.TryGetProperty("message", out var msgProp) ? msgProp.GetString() : "Заказ успешно оформлен!";
                
                MessageBox.Show(message, "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                _cartItems.Clear();
                DialogResult = true;
                Close();
            }
            else
            {
                // Парсим ошибку
                try
                {
                    using var doc = JsonDocument.Parse(responseContent);
                    var root = doc.RootElement;
                    var errorMessage = root.TryGetProperty("message", out var msgProp) ? msgProp.GetString() : "Ошибка оформления заказа";
                    MessageBox.Show(errorMessage, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                catch
                {
                    MessageBox.Show($"Ошибка оформления заказа: {responseContent}", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void IncreaseQuantity_Click(object sender, RoutedEventArgs e)
    {
        var btn = sender as Button;
        var item = btn?.Tag as CartItem;
        if (item != null)
        {
            item.Quantity++;
            UpdateTotalPrice();
            CartListBox.Items.Refresh();
        }
    }

    private void DecreaseQuantity_Click(object sender, RoutedEventArgs e)
    {
        var btn = sender as Button;
        var item = btn?.Tag as CartItem;
        if (item != null && item.Quantity > 1)
        {
            item.Quantity--;
            UpdateTotalPrice();
            CartListBox.Items.Refresh();
        }
        else if (item != null && item.Quantity == 1)
        {
            _cartItems.Remove(item);
            UpdateTotalPrice();
        }
    }

    private void RemoveFromCart_Click(object sender, RoutedEventArgs e)
    {
        var btn = sender as Button;
        var item = btn?.Tag as CartItem;
        if (item != null)
        {
            _cartItems.Remove(item);
            UpdateTotalPrice();
        }
    }
}
