using Domain.Models;
using Domain.Models.Read;
using Domain.Repositories;

namespace Infrastructure.Services
{
    public class ProductService : IProductService
    {
        private readonly IDummyJsonService _dummyJsonService;
        private readonly IUserFavoriteRepository _favoriteRepository;

        public ProductService(IDummyJsonService dummyJsonService, IUserFavoriteRepository favoriteRepository)
        {
            _dummyJsonService = dummyJsonService;
            _favoriteRepository = favoriteRepository;
        }

        public async Task<PagedResult<ProductDTO>> GetProductsAsync(ProductQueryParameters parameters, string? userId = null)
        {
            PagedResult<Product> result;

            // Determine which API endpoint to use based on parameters
            if (!string.IsNullOrEmpty(parameters.SearchTerm))
            {
                result = await _dummyJsonService.SearchProductsAsync(parameters.SearchTerm, parameters);
            }
            else if (!string.IsNullOrEmpty(parameters.Category))
            {
                result = await _dummyJsonService.GetProductsByCategoryAsync(parameters.Category, parameters);
            }
            else
            {
                result = await _dummyJsonService.GetProductsAsync(parameters);
            }

            // Get user favorites
            var favoriteIds = userId != null 
                ? await _favoriteRepository.GetUserFavoriteProductIdsAsync(userId)
                : Enumerable.Empty<int>();

            // Convert to DTOs
            var productDtos = result.Data.Select(p => new ProductDTO
            {
                Id = p.Id,
                Title = p.Title,
                Description = p.Description,
                Category = p.Category,
                Price = p.Price,
                Rating = p.Rating,
                Brand = p.Brand,
                Thumbnail = p.Thumbnail,
                IsFavorite = favoriteIds.Contains(p.Id)
            });

            return new PagedResult<ProductDTO>
            {
                Data = productDtos,
                TotalCount = result.TotalCount,
                PageNumber = result.PageNumber,
                PageSize = result.PageSize,
                TotalPages = result.TotalPages,
                HasPreviousPage = result.HasPreviousPage,
                HasNextPage = result.HasNextPage
            };
        }

        public async Task<ProductDTO?> GetProductByIdAsync(int id, string? userId = null)
        {
            var product = await _dummyJsonService.GetProductByIdAsync(id);
            if (product == null) return null;

            var isFavorite = userId != null && await _favoriteRepository.IsFavoriteAsync(userId, id);

            return new ProductDTO
            {
                Id = product.Id,
                Title = product.Title,
                Description = product.Description,
                Category = product.Category,
                Price = product.Price,
                Rating = product.Rating,
                Brand = product.Brand,
                Thumbnail = product.Thumbnail,
                IsFavorite = isFavorite
            };
        }

        public async Task<List<CategoryDTO>> GetCategoriesAsync()
        {
            return await _dummyJsonService.GetCategoriesAsync();
        }
    }
}