using Microsoft.AspNetCore.Mvc;
using Domain.Models;
using Domain.Repositories;
using Domain.Validators;
using FluentValidation;

namespace Application.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductController : ControllerBase
    {
        private readonly IProductService _productService;
        private readonly IValidator<ProductQueryParameters> _queryValidator;

        public ProductController(IProductService productService, IValidator<ProductQueryParameters> queryValidator)
        {
            _productService = productService;
            _queryValidator = queryValidator;
        }

        [HttpGet]
        public async Task<IActionResult> GetProducts([FromQuery] ProductQueryParameters parameters)
        {
            var validationResult = await _queryValidator.ValidateAsync(parameters);
            if (!validationResult.IsValid)
            {
                return BadRequest(validationResult.Errors);
            }

            var result = await _productService.GetProductsAsync(parameters);
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetProduct(int id)
        {
            if (id <= 0)
                return BadRequest("Product ID must be greater than 0");

            var product = await _productService.GetProductByIdAsync(id);
            if (product == null)
                return NotFound();

            return Ok(product);
        }

        [HttpGet("search")]
        public async Task<IActionResult> SearchProducts([FromQuery] string q, [FromQuery] ProductQueryParameters parameters)
        {
            parameters.SearchTerm = q;

            var validationResult = await _queryValidator.ValidateAsync(parameters);
            if (!validationResult.IsValid)
            {
                return BadRequest(validationResult.Errors);
            }

            var result = await _productService.GetProductsAsync(parameters);
            return Ok(result);
        }

        [HttpGet("categories")]
        public async Task<IActionResult> GetCategories()
        {
            var categories = await _productService.GetCategoriesAsync();
            return Ok(categories);
        }
    }
}
