using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Windows;
using System.Windows.Controls;
using App.Shared.Models;

namespace AppUI;

public partial class ProductEditWindow : Window
{
    private readonly HttpClient _httpClient;
    private readonly string _baseUrl;
    private readonly Product? _editingProduct;
    
    public ProductEditWindow(HttpClient httpClient, string baseUrl, Product? product = null)
    {
        InitializeComponent();
        _httpClient = httpClient;
        _baseUrl = baseUrl;
        _editingProduct = product;
        
        LoadCategories();
        
        if (product != null)
        {
            Title = "Редактирование товара";
            LoadProductData(product);
        }
        else
        {
            Title = "Добавление товара";
            IdBox.Text = "Новый товар";
        }
        
        SaveButton.Click += SaveButton_Click;
        CancelButton.Click += (s, e) => DialogResult = false;
    }
    
    private async void LoadCategories()
    {
        try
        {
            var categories = await _httpClient.GetFromJsonAsync<CategoryDto[]>($"{_baseUrl}/api/products/categories");
            CategoryBox.ItemsSource = categories;
            CategoryBox.DisplayMemberPath = "Name";
            CategoryBox.SelectedValuePath = "Id";
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка загрузки категорий: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
    
    private void LoadProductData(Product product)
    {
        IdBox.Text = product.Id.ToString();
        NameBox.Text = product.Name;
        DescriptionBox.Text = product.Description ?? "";
        BrandBox.Text = product.Brand;
        ProviderBox.Text = product.Provider;
        PriceBox.Text = product.Price.ToString();
        CountBox.Text = product.Count.ToString();
        DiscountBox.Text = product.Discount.ToString();
        
        if (CategoryBox.ItemsSource != null)
        {
            foreach (CategoryDto cat in CategoryBox.ItemsSource)
            {
                if (cat.Id == product.CategoryId)
                {
                    CategoryBox.SelectedItem = cat;
                    break;
                }
            }
        }
        
        foreach (ComboBoxItem item in UnitBox.Items)
        {
            if (item.Content.ToString() == product.Unit)
            {
                UnitBox.SelectedItem = item;
                break;
            }
        }
    }
    
    private async void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(NameBox.Text))
            {
                MessageBox.Show("Введите наименование товара!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            
            if (CategoryBox.SelectedItem == null)
            {
                MessageBox.Show("Выберите категорию!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            
            if (string.IsNullOrWhiteSpace(BrandBox.Text))
            {
                MessageBox.Show("Введите производителя!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            
            if (string.IsNullOrWhiteSpace(ProviderBox.Text))
            {
                MessageBox.Show("Введите поставщика!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            
            if (!decimal.TryParse(PriceBox.Text, out decimal price) || price < 0)
            {
                MessageBox.Show("Введите корректную цену (неотрицательное число)!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            
            if (!int.TryParse(CountBox.Text, out int count) || count < 0)
            {
                MessageBox.Show("Введите корректное количество (неотрицательное целое число)!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            
            if (!decimal.TryParse(DiscountBox.Text, out decimal discount) || discount < 0 || discount > 100)
            {
                MessageBox.Show("Введите корректную скидку (0-100)!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            
            var category = CategoryBox.SelectedItem as CategoryDto;
            
            var product = new Product
            {
                Id = _editingProduct?.Id ?? 0,
                Name = NameBox.Text,
                Description = DescriptionBox.Text,
                CategoryId = category?.Id ?? 1,
                CategoryName = category?.Name ?? "",
                Brand = BrandBox.Text,
                Provider = ProviderBox.Text,
                Price = price,
                Count = count,
                Discount = discount,
                Unit = (UnitBox.SelectedItem as ComboBoxItem)?.Content.ToString() ?? "пара",
                Photo = _editingProduct?.Photo
            };
            
            HttpResponseMessage response;
            
            if (_editingProduct != null)
            {
                response = await _httpClient.PutAsJsonAsync($"{_baseUrl}/api/products/{product.Id}", product);
            }
            else
            {
                response = await _httpClient.PostAsJsonAsync($"{_baseUrl}/api/products", product);
            }
            
            if (response.IsSuccessStatusCode)
            {
                DialogResult = true;
                Close();
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                MessageBox.Show($"Ошибка сохранения: {error}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}

public class CategoryDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}
