using Domain.Validators;
using FluentValidation.TestHelper;
using Xunit;

namespace Shop.UnitTests.Validators
{
    public class AddFavoriteRequestValidatorTests
    {
        private readonly AddFavoriteRequestValidator _validator;

        public AddFavoriteRequestValidatorTests()
        {
            _validator = new AddFavoriteRequestValidator();
        }

        [Fact]
        public void Should_Have_Error_When_UserId_Is_Empty()
        {
            var model = new AddFavoriteRequest { UserId = string.Empty, ProductId = 1 };
            var result = _validator.TestValidate(model);
            result.ShouldHaveValidationErrorFor(x => x.UserId)
                  .WithErrorMessage("User ID is required");
        }

        [Fact]
        public void Should_Have_Error_When_UserId_Is_Null()
        {
            var model = new AddFavoriteRequest { UserId = null!, ProductId = 1 };
            var result = _validator.TestValidate(model);
            result.ShouldHaveValidationErrorFor(x => x.UserId)
                  .WithErrorMessage("User ID is required");
        }

        [Fact]
        public void Should_Have_Error_When_UserId_Is_Too_Long()
        {
            var model = new AddFavoriteRequest
            {
                UserId = new string('a', 451), // 451 characters
                ProductId = 1
            };
            var result = _validator.TestValidate(model);
            result.ShouldHaveValidationErrorFor(x => x.UserId)
                  .WithErrorMessage("User ID must not exceed 450 characters");
        }

        [Fact]
        public void Should_Not_Have_Error_When_UserId_Is_Valid()
        {
            var model = new AddFavoriteRequest { UserId = "user123", ProductId = 1 };
            var result = _validator.TestValidate(model);
            result.ShouldNotHaveValidationErrorFor(x => x.UserId);
        }

        [Fact]
        public void Should_Not_Have_Error_When_UserId_Is_At_Maximum_Length()
        {
            var model = new AddFavoriteRequest
            {
                UserId = new string('a', 450), // exactly 450 characters
                ProductId = 1
            };
            var result = _validator.TestValidate(model);
            result.ShouldNotHaveValidationErrorFor(x => x.UserId);
        }

        [Fact]
        public void Should_Have_Error_When_ProductId_Is_Zero()
        {
            var model = new AddFavoriteRequest { UserId = "user123", ProductId = 0 };
            var result = _validator.TestValidate(model);
            result.ShouldHaveValidationErrorFor(x => x.ProductId)
                  .WithErrorMessage("Product ID must be greater than 0");
        }

        [Fact]
        public void Should_Have_Error_When_ProductId_Is_Negative()
        {
            var model = new AddFavoriteRequest { UserId = "user123", ProductId = -1 };
            var result = _validator.TestValidate(model);
            result.ShouldHaveValidationErrorFor(x => x.ProductId)
                  .WithErrorMessage("Product ID must be greater than 0");
        }

        [Fact]
        public void Should_Not_Have_Error_When_ProductId_Is_Valid()
        {
            var model = new AddFavoriteRequest { UserId = "user123", ProductId = 1 };
            var result = _validator.TestValidate(model);
            result.ShouldNotHaveValidationErrorFor(x => x.ProductId);
        }

        [Fact]
        public void Should_Pass_With_Valid_Model()
        {
            var model = new AddFavoriteRequest
            {
                UserId = "user123",
                ProductId = 42
            };
            var result = _validator.TestValidate(model);
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public void Should_Have_Multiple_Errors_When_Both_Fields_Are_Invalid()
        {
            var model = new AddFavoriteRequest
            {
                UserId = string.Empty,
                ProductId = 0
            };
            var result = _validator.TestValidate(model);

            result.ShouldHaveValidationErrorFor(x => x.UserId);
            result.ShouldHaveValidationErrorFor(x => x.ProductId);
        }
    }
}