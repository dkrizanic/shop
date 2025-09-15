namespace Domain.Models.Read;

public class CheckoutValidationResult
{
    public bool IsValid { get; set; }
    public string Message { get; set; } = string.Empty;
    public List<CheckoutValidationError> Errors { get; set; } = new();
    public ShoppingCartDTO? UpdatedCart { get; set; }
}

public class CheckoutValidationError
{
    public int ProductId { get; set; }
    public string ProductTitle { get; set; } = string.Empty;
    public int RequestedQuantity { get; set; }
    public int AvailableStock { get; set; }
    public int AdjustedQuantity { get; set; }
    public CheckoutErrorType ErrorType { get; set; }
    public string Message { get; set; } = string.Empty;
}

public enum CheckoutErrorType
{
    OutOfStock,
    InsufficientStock,
    ProductNotFound
}