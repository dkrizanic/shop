using System.Text.Json;
using Domain.Models;
using Domain.Models.Read;
using Domain.Repositories;

namespace Infrastructure.Services
{
    public class DummyJsonService : IDummyJsonService
    {
        private readonly HttpClient _httpClient;
        private readonly JsonSerializerOptions _jsonOptions;

        public DummyJsonService(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _httpClient.BaseAddress = new Uri("https://dummyjson.com/");

            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                PropertyNameCaseInsensitive = true
            };
        }

        public async Task<PagedResult<Product>> GetProductsAsync(ProductQueryParameters parameters)
        {
            var skip = (parameters.PageNumber - 1) * parameters.PageSize;
            var url = $"products?limit={parameters.PageSize}&skip={skip}";
            Console.WriteLine($"DummyJsonService: Calling {url}");
            var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
                throw new HttpRequestException($"Failed to fetch products: {response.StatusCode}");

            var json = await response.Content.ReadAsStringAsync();
            var dummyResponse = JsonSerializer.Deserialize<DummyJsonProductsResponse>(json, _jsonOptions);

            return new PagedResult<Product>
            {
                Data = dummyResponse!.Products,
                TotalCount = dummyResponse.Total,
                PageNumber = parameters.PageNumber,
                PageSize = parameters.PageSize,
                TotalPages = (int)Math.Ceiling((double)dummyResponse.Total / parameters.PageSize),
                HasPreviousPage = parameters.PageNumber > 1,
                HasNextPage = skip + parameters.PageSize < dummyResponse.Total
            };
        }

        public async Task<PagedResult<Product>> SearchProductsAsync(string searchTerm, ProductQueryParameters parameters)
        {
            var skip = (parameters.PageNumber - 1) * parameters.PageSize;
            var url = $"products/search?q={Uri.EscapeDataString(searchTerm)}&limit={parameters.PageSize}&skip={skip}";
            Console.WriteLine($"DummyJsonService: Searching with term '{searchTerm}', calling {url}");
            var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
                throw new HttpRequestException($"Failed to search products: {response.StatusCode}");

            var json = await response.Content.ReadAsStringAsync();
            var dummyResponse = JsonSerializer.Deserialize<DummyJsonProductsResponse>(json, _jsonOptions);

            return new PagedResult<Product>
            {
                Data = dummyResponse!.Products,
                TotalCount = dummyResponse.Total,
                PageNumber = parameters.PageNumber,
                PageSize = parameters.PageSize,
                TotalPages = (int)Math.Ceiling((double)dummyResponse.Total / parameters.PageSize),
                HasPreviousPage = parameters.PageNumber > 1,
                HasNextPage = skip + parameters.PageSize < dummyResponse.Total
            };
        }

        public async Task<PagedResult<Product>> GetProductsByCategoryAsync(string category, ProductQueryParameters parameters)
        {
            var skip = (parameters.PageNumber - 1) * parameters.PageSize;
            var response = await _httpClient.GetAsync($"products/category/{Uri.EscapeDataString(category)}?limit={parameters.PageSize}&skip={skip}");

            if (!response.IsSuccessStatusCode)
                throw new HttpRequestException($"Failed to fetch products by category: {response.StatusCode}");

            var json = await response.Content.ReadAsStringAsync();
            var dummyResponse = JsonSerializer.Deserialize<DummyJsonProductsResponse>(json, _jsonOptions);

            return new PagedResult<Product>
            {
                Data = dummyResponse!.Products,
                TotalCount = dummyResponse.Total,
                PageNumber = parameters.PageNumber,
                PageSize = parameters.PageSize,
                TotalPages = (int)Math.Ceiling((double)dummyResponse.Total / parameters.PageSize),
                HasPreviousPage = parameters.PageNumber > 1,
                HasNextPage = skip + parameters.PageSize < dummyResponse.Total
            };
        }

        public async Task<Product?> GetProductByIdAsync(int id)
        {
            var response = await _httpClient.GetAsync($"products/{id}");

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                return null;

            if (!response.IsSuccessStatusCode)
                throw new HttpRequestException($"Failed to fetch product: {response.StatusCode}");

            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<Product>(json, _jsonOptions);
        }

        public async Task<List<CategoryDTO>> GetCategoriesAsync()
        {
            var response = await _httpClient.GetAsync("products/categories");

            if (!response.IsSuccessStatusCode)
                throw new HttpRequestException($"Failed to fetch categories: {response.StatusCode}");

            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<List<CategoryDTO>>(json, _jsonOptions) ?? new List<CategoryDTO>();
        }
    }
}