using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Features.WorkOrders.EventHandlers;
using MechanicShop.Domain.WorkOrders.Events;
using NSubstitute;
using Xunit;

namespace MechanicShop.Application.SubcutaneousTests.Features.WorkOrders.EventHandlers;

public class WorkOrderCollectionModifiedEventHandlerTests
{
    [Fact]
    public async Task Handle_Should_Notify_WorkOrders_Changed()
    {
        // Arrange
        var notifier = Substitute.For<IWorkOrderNotifier>();
        var handler = new WorkOrderCollectionModifiedEventHandler(notifier);

        var notification = new WorkOrderCollectionModified();
        var ct = new CancellationTokenSource().Token;

        // Act
        await handler.Handle(notification, ct);

        // Assert
        await notifier.Received(1)
            .NotifyWorkOrdersChangedAsync(ct);
    }
}