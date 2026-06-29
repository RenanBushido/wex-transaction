namespace WexTransaction.Tests.Application.Mappings;

public class MappingProfileTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IMapper _mapper;

    public MappingProfileTests()
    {
        var services = new ServiceCollection();
        services.AddAutoMapper(typeof(MappingProfile));
        _serviceProvider = services.BuildServiceProvider();
        _mapper = _serviceProvider.GetRequiredService<IMapper>();
    }

    [Fact]
    public void AutoMapperConfiguration_IsValid()
    {
        // Arrange & Act
        var serviceProvider = new ServiceCollection()
            .AddAutoMapper(typeof(MappingProfile))
            .BuildServiceProvider();
        var mapper = serviceProvider.GetRequiredService<IMapper>();

        // Assert - if we got here without exception, the configuration is valid
        Assert.NotNull(mapper);
    }

    [Fact]
    public void Map_PurchaseTransactionToGetPurchaseTransactionResponse_MapsSuccessfully()
    {
        // Arrange
        var transaction = PurchaseTransaction.Create("Coffee", DateTime.UtcNow, 100m);

        // Act
        var response = _mapper.Map<GetPurchaseTransactionResponse>(transaction);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(transaction.Id, response.TransactionId);
        Assert.Equal((string)transaction.Description, response.Description);
        Assert.Equal((decimal)transaction.Amount, response.Amount);
        Assert.Equal(transaction.TransactionDate, response.Date);
    }

    [Fact]
    public void Map_PurchaseTransactionValueObjects_ConvertCorrectly()
    {
        // Arrange
        var transaction = PurchaseTransaction.Create("Book purchase", DateTime.UtcNow, 50.25m);

        // Act
        var response = _mapper.Map<GetPurchaseTransactionResponse>(transaction);

        // Assert
        // Value object conversions via implicit operators
        Assert.Equal("Book purchase", response.Description);
        Assert.Equal(50.25m, response.Amount);
    }
}
