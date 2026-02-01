using AsyncLocal_demo.Application.Orders;
using AsyncLocal_demo.Core.BackgroundProcessing;
using AsyncLocal_demo.Core.Context;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace AsyncLocal_demo.Tests;

public sealed class OrderServiceTests
{
    private readonly Mock<IExecutionContext> _contextMock = new();
    private readonly Mock<IOrderRepository> _repositoryMock = new();
    private readonly Mock<IBackgroundTaskQueue<Guid>> _queueMock = new();
    private readonly OrderService _sut;

    public OrderServiceTests()
    {
        _sut = new OrderService(
            _contextMock.Object,
            _repositoryMock.Object,
            _queueMock.Object,
            Mock.Of<ILogger<OrderService>>());
    }

    [Fact]
    public async Task CreerAsync_Devrait_Creer_Commande_Avec_Contexte()
    {
        _contextMock.Setup(x => x.TenantId).Returns("tenant-123");
        _contextMock.Setup(x => x.UserId).Returns("user-456");
        _contextMock.Setup(x => x.CorrelationId).Returns("corr-789");

        var command = new CreateOrderCommand
        {
            Items = [new OrderItemCommand("PROD-1", "Laptop", 2, 999.99m)]
        };

        var result = await _sut.CreateAsync(command);

        result.TenantId.Should().Be("tenant-123");
        result.CreatedBy.Should().Be("user-456");
        result.CorrelationId.Should().Be("corr-789");
        result.ItemCount.Should().Be(1);
        result.TotalAmount.Should().Be(1999.98m);

        _repositoryMock.Verify(x => x.AddAsync(
            It.Is<Order>(o =>
                o.TenantId == "tenant-123" &&
                o.CreatedBy == "user-456" &&
                o.CorrelationId == "corr-789"),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreerAsync_Sans_TenantId_Devrait_Lever_Exception()
    {
        _contextMock.Setup(x => x.TenantId).Returns((string?)null);
        _contextMock.Setup(x => x.CorrelationId).Returns("corr-123");

        var command = new CreateOrderCommand
        {
            Items = [new OrderItemCommand("PROD-1", "Test", 1, 10m)]
        };

        var act = () => _sut.CreateAsync(command);

        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("*TenantId*");
    }

    [Fact]
    public async Task CreerAsync_Sans_CorrelationId_Devrait_Lever_Exception()
    {
        _contextMock.Setup(x => x.TenantId).Returns("tenant-123");
        _contextMock.Setup(x => x.CorrelationId).Returns((string?)null);

        var command = new CreateOrderCommand
        {
            Items = [new OrderItemCommand("PROD-1", "Test", 1, 10m)]
        };

        var act = () => _sut.CreateAsync(command);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*CorrelationId*");
    }

    [Fact]
    public async Task ObtenirParIdAsync_Devrait_Retourner_Null_Quand_Non_Trouve()
    {
        _contextMock.Setup(x => x.TenantId).Returns("tenant-123");
        _contextMock.Setup(x => x.CorrelationId).Returns("corr-123");
        _repositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Order?)null);

        var result = await _sut.GetByIdAsync(Guid.NewGuid());

        result.Should().BeNull();
    }

    [Fact]
    public async Task ObtenirParIdAsync_Devrait_Retourner_Commande_Quand_Trouvee()
    {
        _contextMock.Setup(x => x.TenantId).Returns("tenant-123");
        _contextMock.Setup(x => x.CorrelationId).Returns("corr-123");

        var orderId = Guid.NewGuid();
        var order = new Order
        {
            Id = orderId,
            TenantId = "tenant-123",
            CreatedBy = "user-456",
            CorrelationId = "corr-123",
            Items = [new OrderItem { Id = Guid.NewGuid(), ProductId = "P1", ProductName = "Test", Quantity = 1, UnitPrice = 10m }],
            CreatedAt = DateTime.UtcNow
        };

        _repositoryMock.Setup(x => x.GetByIdAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        var result = await _sut.GetByIdAsync(orderId);

        result.Should().NotBeNull();
        result!.Id.Should().Be(orderId);
        result.Items.Should().HaveCount(1);
    }
}