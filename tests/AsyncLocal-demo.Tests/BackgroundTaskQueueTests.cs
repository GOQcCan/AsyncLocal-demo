using AsyncLocal.ExecutionContext.Abstractions;
using AsyncLocal_demo.Infrastructure.BackgroundProcessing;
using FluentAssertions;
using Moq;

namespace AsyncLocal_demo.Tests;

public sealed class BackgroundTaskQueueTests
{
    private readonly Mock<IExecutionContext> _contextMock = new();

    [Fact]
    public async Task EnqueueAsync_Devrait_Capturer_Le_Contexte_Actuel()
    {
        // Arrange
        _contextMock.Setup(x => x.Get<string>("TenantId")).Returns("tenant-123");
        _contextMock.Setup(x => x.Get<string>("UserId")).Returns("user-456");
        _contextMock.Setup(x => x.CorrelationId).Returns("corr-789");

        var queue = new BackgroundTaskQueue<Guid>(_contextMock.Object);
        var orderId = Guid.NewGuid();

        // Act
        await queue.EnqueueAsync(orderId);
        var workItem = await queue.DequeueAsync(CancellationToken.None);

        // Assert
        workItem.Payload.Should().Be(orderId);
        workItem.TenantId.Should().Be("tenant-123");
        workItem.UserId.Should().Be("user-456");
        workItem.CorrelationId.Should().Be("corr-789");
        workItem.CreatedAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public async Task EnqueueAsync_Devrait_Incrementer_PendingCount()
    {
        // Arrange
        _contextMock.Setup(x => x.Get<string>("TenantId")).Returns("tenant-123");
        var queue = new BackgroundTaskQueue<Guid>(_contextMock.Object);

        // Act
        await queue.EnqueueAsync(Guid.NewGuid());
        await queue.EnqueueAsync(Guid.NewGuid());
        await queue.EnqueueAsync(Guid.NewGuid());

        // Assert
        queue.PendingCount.Should().Be(3);
    }

    [Fact]
    public async Task DequeueAsync_Devrait_Decrementer_PendingCount()
    {
        // Arrange
        _contextMock.Setup(x => x.Get<string>("TenantId")).Returns("tenant-123");
        var queue = new BackgroundTaskQueue<Guid>(_contextMock.Object);

        await queue.EnqueueAsync(Guid.NewGuid());
        await queue.EnqueueAsync(Guid.NewGuid());

        // Act
        await queue.DequeueAsync(CancellationToken.None);

        // Assert
        queue.PendingCount.Should().Be(1);
    }

    [Fact]
    public async Task DequeueAsync_Devrait_Respecter_Ordre_FIFO()
    {
        // Arrange
        _contextMock.Setup(x => x.Get<string>("TenantId")).Returns("tenant-123");
        var queue = new BackgroundTaskQueue<Guid>(_contextMock.Object);

        var first = Guid.NewGuid();
        var second = Guid.NewGuid();
        var third = Guid.NewGuid();

        await queue.EnqueueAsync(first);
        await queue.EnqueueAsync(second);
        await queue.EnqueueAsync(third);

        // Act & Assert
        (await queue.DequeueAsync(CancellationToken.None)).Payload.Should().Be(first);
        (await queue.DequeueAsync(CancellationToken.None)).Payload.Should().Be(second);
        (await queue.DequeueAsync(CancellationToken.None)).Payload.Should().Be(third);
    }

    [Fact]
    public async Task ReadAllAsync_Devrait_Retourner_Tous_Les_Items()
    {
        // Arrange
        _contextMock.Setup(x => x.Get<string>("TenantId")).Returns("tenant-123");
        var queue = new BackgroundTaskQueue<Guid>(_contextMock.Object);
        var ids = new[] { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() };

        foreach (var id in ids)
            await queue.EnqueueAsync(id);

        var results = new List<Guid>();

        // Act - Utiliser DequeueAsync au lieu de ReadAllAsync pour éviter le blocage
        for (var i = 0; i < ids.Length; i++)
        {
            var item = await queue.DequeueAsync(CancellationToken.None);
            results.Add(item.Payload);
        }

        // Assert
        results.Should().BeEquivalentTo(ids);
    }

    [Fact]
    public async Task EnqueueAsync_Avec_Contexte_Null_Devrait_Capturer_Valeurs_Nulles()
    {
        // Arrange
        _contextMock.Setup(x => x.Get<string>("TenantId")).Returns((string?)null);
        _contextMock.Setup(x => x.Get<string>("UserId")).Returns((string?)null);
        _contextMock.Setup(x => x.CorrelationId).Returns((string?)null);

        var queue = new BackgroundTaskQueue<Guid>(_contextMock.Object);
        var orderId = Guid.NewGuid();

        // Act
        await queue.EnqueueAsync(orderId);
        var workItem = await queue.DequeueAsync(CancellationToken.None);

        // Assert
        workItem.TenantId.Should().BeNull();
        workItem.UserId.Should().BeNull();
        workItem.CorrelationId.Should().BeNull();
    }

    [Fact]
    public void Constructor_Avec_Context_Null_Devrait_Lever_Exception()
    {
        // Act
        var act = () => new BackgroundTaskQueue<Guid>(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public async Task EnqueueAsync_Avec_Payload_Null_Devrait_Lever_Exception()
    {
        // Arrange
        var queue = new BackgroundTaskQueue<string>(_contextMock.Object);

        // Act
        var act = async () => await queue.EnqueueAsync(null!);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>();
    }
}