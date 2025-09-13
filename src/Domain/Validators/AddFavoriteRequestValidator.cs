using FluentValidation;

namespace Domain.Validators
{
    public class AddFavoriteRequest
    {
        public string UserId { get; set; } = string.Empty;
        public int ProductId { get; set; }
    }

    public class AddFavoriteRequestValidator : AbstractValidator<AddFavoriteRequest>
    {
        public AddFavoriteRequestValidator()
        {
            RuleFor(x => x.UserId)
                .NotEmpty()
                .WithMessage("User ID is required")
                .MaximumLength(450)
                .WithMessage("User ID must not exceed 450 characters");

            RuleFor(x => x.ProductId)
                .GreaterThan(0)
                .WithMessage("Product ID must be greater than 0");
        }
    }
}