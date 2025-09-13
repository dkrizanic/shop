using Domain.Models;
using Domain.Models.Read;

namespace Domain.Repositories
{
    public interface IProductService
    {
        Task<PagedResult<ProductDTO>> GetProductsAsync(ProductQueryParameters parameters, string? userId = null);
        Task<ProductDTO?> GetProductByIdAsync(int id, string? userId = null);
        Task<List<CategoryDTO>> GetCategoriesAsync();
    }
}