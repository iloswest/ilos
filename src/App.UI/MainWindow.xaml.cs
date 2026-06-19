using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using App.Shared.Models;

namespace AppUI;

public partial class MainWindow : Window
{
    private readonly HttpClient _httpClient = new HttpClient();
    private readonly string _baseUrl = "http://localhost:5271";
    private User? _currentUser;
    private List<Product> _allProducts = new List<Product>();
    private ObservableCollection<CartItem> _cartItems = new ObservableCollection<CartItem>();

    public bool IsAdmin => _currentUser?.Role == "admin";

    public MainWindow()
    {
        InitializeComponent();
        LoginBox.Text = "admin";
        DataContext = this;

        AdminPanel.Visibility = Visibility.Collapsed;
        CartButton.Visibility = Visibility.Collapsed;

        LoginButton.Click += LoginButton_Click;
        GuestButton.Click += GuestButton_Click;
        CartButton.Click += CartButton_Click;
        SearchBox.TextChanged += (s, e) => ApplyFilters();
        ProviderFilter.SelectionChanged += (s, e) => ApplyFilters();
        SortByCombo.SelectionChanged += (s, e) => ApplyFilters();
        AddProductButton.Click += AddProductButton_Click;
        RefreshButton.Click += async (s, e) => await LoadProducts();

        Loaded += async (s, e) => await LoadProducts();
        LoadProviders();
    }

    private async Task LoadProducts()
    {
        try
        {
            _allProducts = await _httpClient.GetFromJsonAsync<List<Product>>($"{_baseUrl}/api/products") ?? new List<Product>();
            ApplyFilters();
            UpdateButtonsVisibility();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка загрузки: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private async void LoadProviders()
    {
        try
        {
            var providers = await _httpClient.GetFromJsonAsync<List<string>>($"{_baseUrl}/api/products/providers");
            ProviderFilter.Items.Clear();
            ProviderFilter.Items.Add("Все поставщики");
            if (providers != null)
            {
                foreach (var p in providers)
                    ProviderFilter.Items.Add(p);
            }
            ProviderFilter.SelectedIndex = 0;
        }
        catch { }
    }

    private void ApplyFilters()
    {
        var filtered = _allProducts.AsEnumerable();

        var search = SearchBox.Text?.ToLower();
        if (!string.IsNullOrWhiteSpace(search))
        {
            filtered = filtered.Where(p =>
                (p.Name?.ToLower().Contains(search) ?? false) ||
                (p.Brand?.ToLower().Contains(search) ?? false) ||
                (p.Description?.ToLower().Contains(search) ?? false) ||
                (p.CategoryName?.ToLower().Contains(search) ?? false) ||
                (p.Provider?.ToLower().Contains(search) ?? false));
        }

        if (ProviderFilter.SelectedIndex > 0 && ProviderFilter.SelectedItem?.ToString() != "Все поставщики")
        {
            filtered = filtered.Where(p => p.Provider == ProviderFilter.SelectedItem?.ToString());
        }

        switch (SortByCombo.SelectedIndex)
        {
            case 1: filtered = filtered.OrderBy(p => p.Count); break;
            case 2: filtered = filtered.OrderByDescending(p => p.Count); break;
            case 3: filtered = filtered.OrderBy(p => p.Price); break;
            case 4: filtered = filtered.OrderByDescending(p => p.Price); break;
        }

        ProductsList.ItemsSource = filtered.ToList();
        UpdateButtonsVisibility();
    }

    private void UpdateButtonsVisibility()
    {
        // Ждем рендеринга UI
        Dispatcher.BeginInvoke(new Action(() =>
        {
            bool isAdmin = _currentUser?.Role == "admin";

            foreach (var item in ProductsList.Items)
            {
                var container = ProductsList.ItemContainerGenerator.ContainerFromItem(item) as ListBoxItem;
                if (container != null)
                {
                    var editButton = FindButtonInContainer(container, "EditButton");
                    var deleteButton = FindButtonInContainer(container, "DeleteButton");

                    if (editButton != null)
                        editButton.Visibility = isAdmin ? Visibility.Visible : Visibility.Collapsed;

                    if (deleteButton != null)
                        deleteButton.Visibility = isAdmin ? Visibility.Visible : Visibility.Collapsed;
                }
            }
        }), System.Windows.Threading.DispatcherPriority.Background);
    }

    private Button? FindButtonInContainer(DependencyObject parent, string name)
    {
        for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
        {
            var child = VisualTreeHelper.GetChild(parent, i);
            if (child is Button button && button.Name == name)
                return button;

            var result = FindButtonInContainer(child, name);
            if (result != null)
                return result;
        }
        return null;
    }

    private void UpdateUIBasedOnRole()
    {
        bool isAdmin = _currentUser?.Role == "admin";

        AdminPanel.Visibility = isAdmin ? Visibility.Visible : Visibility.Collapsed;
        CartButton.Visibility = (_currentUser != null && (_currentUser.Role == "client" || isAdmin || _currentUser.Role == "manager")) ? Visibility.Visible : Visibility.Collapsed;
        
        DataContext = null;
        DataContext = this;
        
        UpdateButtonsVisibility();
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
                _currentUser = result.User;
                UserInfo.Text = $"Добро пожаловать, {_currentUser?.Name} ({_currentUser?.Role})";
                UpdateUIBasedOnRole();
                await LoadProducts();
                MessageBox.Show($"Вход выполнен! Роль: {_currentUser?.Role}", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
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

    private async void GuestButton_Click(object sender, RoutedEventArgs e)
    {
        _currentUser = new User { Id = 0, Login = "guest", Name = "Гость", Role = "guest" };
        UserInfo.Text = "Вы вошли как гость";
        UpdateUIBasedOnRole();
        await LoadProducts();
        MessageBox.Show("Доступен только просмотр товаров", "Режим гостя", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private void CartButton_Click(object sender, RoutedEventArgs e)
    {
        var cartWindow = new CartWindow(_cartItems, _currentUser);
        cartWindow.Owner = this;
        cartWindow.ShowDialog();
        LoadProducts();
    }

    private void AddToCart_Click(object sender, RoutedEventArgs e)
    {
        var btn = sender as Button;
        var product = btn?.Tag as Product;

        if (product != null)
        {
            var existing = _cartItems.FirstOrDefault(i => i.Product?.Id == product.Id);
            if (existing != null)
                existing.Quantity++;
            else
                _cartItems.Add(new CartItem { Product = product, Quantity = 1 });

            MessageBox.Show($"Товар \"{product.Name}\" добавлен в корзину!", "Корзина", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    private async void AddProductButton_Click(object sender, RoutedEventArgs e)
    {
        var editWindow = new ProductEditWindow(_httpClient, _baseUrl);
        editWindow.Owner = this;
        if (editWindow.ShowDialog() == true)
            await LoadProducts();
    }

    private async void EditProduct_Click(object sender, RoutedEventArgs e)
    {
        var btn = sender as Button;
        if (btn?.Tag is int id)
        {
            var product = _allProducts.FirstOrDefault(p => p.Id == id);
            if (product == null) return;

            var editWindow = new ProductEditWindow(_httpClient, _baseUrl, product);
            editWindow.Owner = this;
            if (editWindow.ShowDialog() == true)
                await LoadProducts();
        }
    }

    private async void DeleteProduct_Click(object sender, RoutedEventArgs e)
    {
        var btn = sender as Button;
        if (btn?.Tag is int id)
        {
            var result = MessageBox.Show("Вы уверены, что хотите удалить этот товар?",
                "Подтверждение удаления", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    await _httpClient.DeleteAsync($"{_baseUrl}/api/products/{id}");
                    await LoadProducts();
                    MessageBox.Show("Товар успешно удален!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}