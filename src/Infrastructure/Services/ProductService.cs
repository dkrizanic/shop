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
            // Debug logging for all parameters
            Console.WriteLine($"ProductService: SearchTerm='{parameters.SearchTerm}', Category='{parameters.Category}', MinPrice={parameters.MinPrice}, MaxPrice={parameters.MaxPrice}");

            // For filtering and sorting, we need to get more products from DummyJSON
            // DummyJSON has 100+ products available, so let's fetch a larger set to work with
            var fetchParameters = new ProductQueryParameters
            {
                PageNumber = 1,
                PageSize = 100, // Always fetch 100 items to have a good dataset for filtering/sorting
                SearchTerm = parameters.SearchTerm,
                Category = parameters.Category,
                SortBy = parameters.SortBy,
                SortOrder = parameters.SortOrder
            };

            PagedResult<Product> result;

            // Determine which API endpoint to use based on parameters
            if (!string.IsNullOrEmpty(parameters.SearchTerm))
            {
                result = await _dummyJsonService.SearchProductsAsync(parameters.SearchTerm, fetchParameters);
            }
            else if (!string.IsNullOrEmpty(parameters.Category))
            {
                result = await _dummyJsonService.GetProductsByCategoryAsync(parameters.Category, fetchParameters);
            }
            else
            {
                result = await _dummyJsonService.GetProductsAsync(fetchParameters);
            }

            // Apply client-side price filtering (DummyJSON API doesn't support price filtering)
            var filteredProducts = result.Data.AsEnumerable();

            if (parameters.MinPrice.HasValue)
            {
                filteredProducts = filteredProducts.Where(p => p.Price >= parameters.MinPrice.Value);
            }

            if (parameters.MaxPrice.HasValue)
            {
                filteredProducts = filteredProducts.Where(p => p.Price <= parameters.MaxPrice.Value);
            }

            var allFilteredProducts = filteredProducts.ToList();
            Console.WriteLine($"ProductService: Original count={result.Data.Count()}, Filtered count={allFilteredProducts.Count}");

            // Apply sorting (DummyJSON API doesn't support custom sorting)
            if (!string.IsNullOrEmpty(parameters.SortBy))
            {
                var isDescending = parameters.SortOrder?.ToLowerInvariant() == "desc";

                allFilteredProducts = parameters.SortBy.ToLowerInvariant() switch
                {
                    "title" => isDescending
                        ? allFilteredProducts.OrderByDescending(p => p.Title).ToList()
                        : allFilteredProducts.OrderBy(p => p.Title).ToList(),
                    "price" => isDescending
                        ? allFilteredProducts.OrderByDescending(p => p.Price).ToList()
                        : allFilteredProducts.OrderBy(p => p.Price).ToList(),
                    "rating" => isDescending
                        ? allFilteredProducts.OrderByDescending(p => p.Rating).ToList()
                        : allFilteredProducts.OrderBy(p => p.Rating).ToList(),
                    "brand" => isDescending
                        ? allFilteredProducts.OrderByDescending(p => p.Brand).ToList()
                        : allFilteredProducts.OrderBy(p => p.Brand).ToList(),
                    _ => allFilteredProducts
                };
            }

            // Apply pagination to filtered results
            var skip = (parameters.PageNumber - 1) * parameters.PageSize;
            var pagedProducts = allFilteredProducts.Skip(skip).Take(parameters.PageSize).ToList();

            // Get user favorites
            var favoriteIds = userId != null
                ? await _favoriteRepository.GetUserFavoriteProductIdsAsync(userId)
                : Enumerable.Empty<int>();

            // Convert to DTOs
            var productDtos = pagedProducts.Select(p => new ProductDTO
            {
                Id = p.Id,
                Title = p.Title,
                Description = p.Description,
                Category = p.Category,
                Price = p.Price,
                Rating = p.Rating,
                Brand = p.Brand,
                Thumbnail = p.Thumbnail,
                Stock = p.Stock,
                IsFavorite = favoriteIds.Contains(p.Id)
            });

            // Calculate pagination based on filtered results
            var totalFilteredCount = allFilteredProducts.Count;
            var totalPages = (int)Math.Ceiling((double)totalFilteredCount / parameters.PageSize);

            return new PagedResult<ProductDTO>
            {
                Data = productDtos,
                TotalCount = totalFilteredCount,
                PageNumber = parameters.PageNumber,
                PageSize = parameters.PageSize,
                TotalPages = totalPages,
                HasPreviousPage = parameters.PageNumber > 1,
                HasNextPage = parameters.PageNumber < totalPages
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
                Stock = product.Stock,
                IsFavorite = isFavorite
            };
        }

        public async Task<List<CategoryDTO>> GetCategoriesAsync()
        {
            return await _dummyJsonService.GetCategoriesAsync();
        }
    }
}