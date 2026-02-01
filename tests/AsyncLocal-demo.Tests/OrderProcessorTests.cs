using AsyncLocal_demo.Application.Orders;
using AsyncLocal_demo.Core.Context;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace AsyncLocal_demo.Tests;

public sealed class OrderProcessorTests
{
    private readonly Mock<IOrderRepository> _repositoryMock = new();
    private readonly OrderProcessor _sut;

    public OrderProcessorTests()
    {
        _sut = new OrderProcessor(
            _repositoryMock.Object,
            Mock.Of<ILogger<OrderProcessor>>());
    }

    [Fact]
    public async Task ProcessAsync_Devrait_Retourner_NotFound_Quand_Commande_Inexistante()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var userId = "user-123";

        _repositoryMock.Setup(x => x.GetByIdAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Order?)null);

        // Act
        var result = await _sut.ProcessAsync(orderId, userId);

        // Assert
        result.Status.Should().Be(OrderProcessingStatus.NotFound);
        result.OrderId.Should().Be(orderId);
    }

    [Fact]
    public async Task ProcessAsync_Devrait_Traiter_Et_Mettre_A_Jour_Commande()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var userId = "user-123";
        var order = new Order
        {
            Id = orderId,
            TenantId = "tenant-123",
            CreatedBy = "user-456",
            CorrelationId = "corr-789",
            Status = OrderProcessingStatus.Pending
        };

        _repositoryMock.Setup(x => x.GetByIdAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        // Act
        var result = await _sut.ProcessAsync(orderId, userId);

        // Assert
        result.Status.Should().Be(OrderProcessingStatus.Completed);
        result.OrderId.Should().Be(orderId);

        _repositoryMock.Verify(x => x.UpdateAsync(
            It.Is<Order>(o => 
                o.Status == OrderProcessingStatus.Completed && 
                o.ProcessedBy == userId &&
                o.ProcessedAt != null),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ProcessAsync_Devrait_Retourner_Processing_Quand_Commande_En_Cours()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var tenantId = "tenant-123";
        var userId = "user-123";
        var order = new Order
        {
            Id = orderId,
            TenantId = tenantId,
            Status = OrderProcessingStatus.Processing
        };

        _repositoryMock
            .Setup(r => r.GetByIdAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        // Act
        var result = await _sut.ProcessAsync(orderId, userId);

        // Assert
        result.Status.Should().Be(OrderProcessingStatus.Processing);
        result.OrderId.Should().Be(orderId);

        _repositoryMock.Verify(r => r.UpdateAsync(
            It.IsAny<Order>(), 
            It.IsAny<CancellationToken>()), Times.Never);
    }
}

public sealed class BackgroundWorkItemTests
{
    [Fact]
    public void FromContext_Devrait_Capturer_Toutes_Les_Proprietes()
    {
        // Arrange
        var contextMock = new Mock<IExecutionContext>();
        contextMock.Setup(x => x.TenantId).Returns("tenant-123");
        contextMock.Setup(x => x.UserId).Returns("user-456");
        contextMock.Setup(x => x.CorrelationId).Returns("corr-789");

        var payload = Guid.NewGuid();

        // Act
        var workItem = AsyncLocal_demo.Infrastructure.BackgroundProcessing.BackgroundWorkItem<Guid>
            .FromContext(payload, contextMock.Object);

        // Assert
        workItem.Payload.Should().Be(payload);
        workItem.TenantId.Should().Be("tenant-123");
        workItem.UserId.Should().Be("user-456");
        workItem.CorrelationId.Should().Be("corr-789");
        workItem.CreatedAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void FromContext_Avec_Payload_Null_Devrait_Lever_Exception()
    {
        // Arrange
        var contextMock = new Mock<IExecutionContext>();

        // Act
        var act = () => AsyncLocal_demo.Infrastructure.BackgroundProcessing.BackgroundWorkItem<string>
            .FromContext(null!, contextMock.Object);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void FromContext_Avec_Context_Null_Devrait_Lever_Exception()
    {
        // Arrange & Act
        var act = () => AsyncLocal_demo.Infrastructure.BackgroundProcessing.BackgroundWorkItem<Guid>
            .FromContext(Guid.NewGuid(), null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }
}

public sealed class OrderProcessingResultTests
{
    [Fact]
    public void Success_Devrait_Creer_Resultat_Completed()
    {
        // Act
        var result = OrderProcessingResult.Success(Guid.NewGuid(), "Test message");

        // Assert
        result.Status.Should().Be(OrderProcessingStatus.Completed);
        result.Message.Should().Be("Test message");
    }

    [Fact]
    public void Failed_Devrait_Creer_Resultat_Failed()
    {
        // Act
        var result = OrderProcessingResult.Failed(Guid.NewGuid(), "Error message");

        // Assert
        result.Status.Should().Be(OrderProcessingStatus.Failed);
        result.Message.Should().Be("Error message");
    }

    [Fact]
    public void NotFound_Devrait_Creer_Resultat_NotFound()
    {
        // Act
        var result = OrderProcessingResult.NotFound(Guid.NewGuid());

        // Assert
        result.Status.Should().Be(OrderProcessingStatus.NotFound);
        result.Message.Should().Be("Commande introuvable");
    }

    [Fact]
    public void InProgress_Devrait_Creer_Resultat_Processing()
    {
        // Act
        var result = OrderProcessingResult.InProgress(Guid.NewGuid());

        // Assert
        result.Status.Should().Be(OrderProcessingStatus.Processing);
        result.Message.Should().Be("Traitement en cours");
    }
}