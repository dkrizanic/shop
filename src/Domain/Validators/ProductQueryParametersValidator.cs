using FluentValidation;
using Domain.Models;

namespace Domain.Validators
{
    public class ProductQueryParametersValidator : AbstractValidator<ProductQueryParameters>
    {
        public ProductQueryParametersValidator()
        {
            RuleFor(x => x.PageNumber)
                .GreaterThan(0)
                .WithMessage("Page number must be greater than 0");

            RuleFor(x => x.PageSize)
                .InclusiveBetween(1, 100)
                .WithMessage("Page size must be between 1 and 100");

            RuleFor(x => x.SearchTerm)
                .MaximumLength(100)
                .When(x => !string.IsNullOrEmpty(x.SearchTerm))
                .WithMessage("Search term must not exceed 100 characters");

            RuleFor(x => x.Category)
                .MaximumLength(50)
                .When(x => !string.IsNullOrEmpty(x.Category))
                .WithMessage("Category must not exceed 50 characters");
        }
    }
}