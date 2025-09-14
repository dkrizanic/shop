using Domain.Models;
using Domain.Validators;
using FluentValidation.TestHelper;
using Xunit;

namespace Shop.UnitTests.Validators
{
    public class ProductQueryParametersValidatorTests
    {
        private readonly ProductQueryParametersValidator _validator;

        public ProductQueryParametersValidatorTests()
        {
            _validator = new ProductQueryParametersValidator();
        }

        [Fact]
        public void Should_Have_Error_When_PageNumber_Is_Zero()
        {
            var model = new ProductQueryParameters { PageNumber = 0 };
            var result = _validator.TestValidate(model);
            result.ShouldHaveValidationErrorFor(x => x.PageNumber)
                  .WithErrorMessage("Page number must be greater than 0");
        }

        [Fact]
        public void Should_Have_Error_When_PageNumber_Is_Negative()
        {
            var model = new ProductQueryParameters { PageNumber = -1 };
            var result = _validator.TestValidate(model);
            result.ShouldHaveValidationErrorFor(x => x.PageNumber)
                  .WithErrorMessage("Page number must be greater than 0");
        }

        [Fact]
        public void Should_Not_Have_Error_When_PageNumber_Is_Valid()
        {
            var model = new ProductQueryParameters { PageNumber = 1, PageSize = 10 };
            var result = _validator.TestValidate(model);
            result.ShouldNotHaveValidationErrorFor(x => x.PageNumber);
        }

        [Fact]
        public void Should_Have_Error_When_PageSize_Is_Zero()
        {
            var model = new ProductQueryParameters { PageNumber = 1, PageSize = 0 };
            var result = _validator.TestValidate(model);
            result.ShouldHaveValidationErrorFor(x => x.PageSize)
                  .WithErrorMessage("Page size must be between 1 and 100");
        }

        [Fact]
        public void Should_Have_Error_When_PageSize_Is_Too_Large()
        {
            var model = new ProductQueryParameters { PageNumber = 1, PageSize = 101 };
            var result = _validator.TestValidate(model);
            result.ShouldHaveValidationErrorFor(x => x.PageSize)
                  .WithErrorMessage("Page size must be between 1 and 100");
        }

        [Fact]
        public void Should_Not_Have_Error_When_PageSize_Is_Valid()
        {
            var model = new ProductQueryParameters { PageNumber = 1, PageSize = 50 };
            var result = _validator.TestValidate(model);
            result.ShouldNotHaveValidationErrorFor(x => x.PageSize);
        }

        [Fact]
        public void Should_Have_Error_When_SearchTerm_Is_Too_Long()
        {
            var model = new ProductQueryParameters
            {
                PageNumber = 1,
                PageSize = 10,
                SearchTerm = new string('a', 101)
            };
            var result = _validator.TestValidate(model);
            result.ShouldHaveValidationErrorFor(x => x.SearchTerm)
                  .WithErrorMessage("Search term must not exceed 100 characters");
        }

        [Fact]
        public void Should_Not_Have_Error_When_SearchTerm_Is_Valid()
        {
            var model = new ProductQueryParameters
            {
                PageNumber = 1,
                PageSize = 10,
                SearchTerm = "valid search term"
            };
            var result = _validator.TestValidate(model);
            result.ShouldNotHaveValidationErrorFor(x => x.SearchTerm);
        }

        [Fact]
        public void Should_Not_Have_Error_When_SearchTerm_Is_Null()
        {
            var model = new ProductQueryParameters
            {
                PageNumber = 1,
                PageSize = 10,
                SearchTerm = null
            };
            var result = _validator.TestValidate(model);
            result.ShouldNotHaveValidationErrorFor(x => x.SearchTerm);
        }

        [Fact]
        public void Should_Not_Have_Error_When_SearchTerm_Is_Empty()
        {
            var model = new ProductQueryParameters
            {
                PageNumber = 1,
                PageSize = 10,
                SearchTerm = string.Empty
            };
            var result = _validator.TestValidate(model);
            result.ShouldNotHaveValidationErrorFor(x => x.SearchTerm);
        }

        [Fact]
        public void Should_Have_Error_When_Category_Is_Too_Long()
        {
            var model = new ProductQueryParameters
            {
                PageNumber = 1,
                PageSize = 10,
                Category = new string('a', 51)
            };
            var result = _validator.TestValidate(model);
            result.ShouldHaveValidationErrorFor(x => x.Category)
                  .WithErrorMessage("Category must not exceed 50 characters");
        }

        [Fact]
        public void Should_Not_Have_Error_When_Category_Is_Valid()
        {
            var model = new ProductQueryParameters
            {
                PageNumber = 1,
                PageSize = 10,
                Category = "electronics"
            };
            var result = _validator.TestValidate(model);
            result.ShouldNotHaveValidationErrorFor(x => x.Category);
        }

        [Fact]
        public void Should_Not_Have_Error_When_Category_Is_Null()
        {
            var model = new ProductQueryParameters
            {
                PageNumber = 1,
                PageSize = 10,
                Category = null
            };
            var result = _validator.TestValidate(model);
            result.ShouldNotHaveValidationErrorFor(x => x.Category);
        }

        [Fact]
        public void Should_Pass_With_Valid_Model()
        {
            var model = new ProductQueryParameters
            {
                PageNumber = 1,
                PageSize = 20,
                SearchTerm = "laptop",
                Category = "electronics"
            };
            var result = _validator.TestValidate(model);
            result.ShouldNotHaveAnyValidationErrors();
        }
    }
}