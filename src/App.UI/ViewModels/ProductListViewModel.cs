using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using App.Shared.Models;

namespace AppUI.ViewModels
{
    public class ProductListViewModel : INotifyPropertyChanged
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;
        private List<Product> _allProducts = new List<Product>();
        private ObservableCollection<Product> _filteredProducts = new ObservableCollection<Product>();
        private string _searchText = string.Empty;
        private string _selectedProvider = "Все поставщики";
        private int _sortOption;
        private List<string> _providers = new List<string>();
        private bool _isLoading;

        public ObservableCollection<Product> FilteredProducts
        {
            get => _filteredProducts;
            set
            {
                _filteredProducts = value;
                OnPropertyChanged();
            }
        }

        public string SearchText
        {
            get => _searchText;
            set
            {
                if (_searchText != value)
                {
                    _searchText = value;
                    OnPropertyChanged();
                    ApplyFilters();
                }
            }
        }

        public string SelectedProvider
        {
            get => _selectedProvider;
            set
            {
                if (_selectedProvider != value)
                {
                    _selectedProvider = value;
                    OnPropertyChanged();
                    ApplyFilters();
                }
            }
        }

        public int SortOption
        {
            get => _sortOption;
            set
            {
                if (_sortOption != value)
                {
                    _sortOption = value;
                    OnPropertyChanged();
                    ApplyFilters();
                }
            }
        }

        public List<string> Providers
        {
            get => _providers;
            set
            {
                _providers = value;
                OnPropertyChanged();
            }
        }

        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                _isLoading = value;
                OnPropertyChanged();
            }
        }

        public int TotalCount => _allProducts.Count;
        public int FilteredCount => _filteredProducts.Count;

        public ProductListViewModel(HttpClient httpClient, string baseUrl)
        {
            _httpClient = httpClient;
            _baseUrl = baseUrl;
        }

        public async Task LoadProductsAsync()
        {
            try
            {
                IsLoading = true;
                _allProducts = await _httpClient.GetFromJsonAsync<List<Product>>($"{_baseUrl}/api/products") 
                              ?? new List<Product>();
                
                // Загружаем поставщиков
                var providers = await _httpClient.GetFromJsonAsync<List<string>>($"{_baseUrl}/api/products/providers");
                Providers = new List<string> { "Все поставщики" };
                if (providers != null)
                {
                    Providers.AddRange(providers);
                }
                SelectedProvider = "Все поставщики";
                
                ApplyFilters();
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Ошибка загрузки: {ex.Message}", "Ошибка", 
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void ApplyFilters()
        {
            var filtered = _allProducts.AsEnumerable();

            // Поиск
            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                var search = SearchText.ToLower();
                filtered = filtered.Where(p =>
                    (p.Name?.ToLower().Contains(search) ?? false) ||
                    (p.Brand?.ToLower().Contains(search) ?? false) ||
                    (p.Description?.ToLower().Contains(search) ?? false) ||
                    (p.CategoryName?.ToLower().Contains(search) ?? false) ||
                    (p.Provider?.ToLower().Contains(search) ?? false)
                );
            }

            // Фильтр по поставщику
            if (!string.IsNullOrWhiteSpace(SelectedProvider) && SelectedProvider != "Все поставщики")
            {
                filtered = filtered.Where(p => p.Provider == SelectedProvider);
            }

            // Сортировка
            switch (SortOption)
            {
                case 1: // По количеству (возрастание)
                    filtered = filtered.OrderBy(p => p.Count);
                    break;
                case 2: // По количеству (убывание)
                    filtered = filtered.OrderByDescending(p => p.Count);
                    break;
                case 3: // По цене (возрастание)
                    filtered = filtered.OrderBy(p => p.Price);
                    break;
                case 4: // По цене (убывание)
                    filtered = filtered.OrderByDescending(p => p.Price);
                    break;
                default:
                    filtered = filtered.OrderBy(p => p.Name);
                    break;
            }

            FilteredProducts = new ObservableCollection<Product>(filtered);
            OnPropertyChanged(nameof(FilteredCount));
            OnPropertyChanged(nameof(TotalCount));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
