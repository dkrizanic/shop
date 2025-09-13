using Domain.Models;
using Domain.Models.Read;

namespace Domain.Repositories
{
    public interface IDummyJsonService
    {
        Task<PagedResult<Product>> GetProductsAsync(ProductQueryParameters parameters);
        Task<PagedResult<Product>> SearchProductsAsync(string searchTerm, ProductQueryParameters parameters);
        Task<PagedResult<Product>> GetProductsByCategoryAsync(string category, ProductQueryParameters parameters);
        Task<Product?> GetProductByIdAsync(int id);
        Task<List<CategoryDTO>> GetCategoriesAsync();
    }
}