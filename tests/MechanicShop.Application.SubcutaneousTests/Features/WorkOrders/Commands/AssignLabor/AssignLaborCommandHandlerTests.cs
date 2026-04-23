using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Features.WorkOrders.Commands.AssignLabor;
using MechanicShop.Application.SubcutaneousTests.Common;
using MechanicShop.Domain.Common.Results;
using MechanicShop.Tests.Common.Employees;
using MechanicShop.Tests.Common.WorkOrders;
using MediatR;
using Xunit;

namespace MechanicShop.Application.SubcutaneousTests.Features.WorkOrders.Commands.AssignLabor;

[Collection(WebAppFactoryCollection.CollectionName)]
public class AssignLaborCommandHandlerTests(WebAppFactory factory)
{
    private readonly IMediator _mediator = factory.CreateMediator();
    private readonly IAppDbContext _context = factory.CreateAppDbContext();

    [Fact]
    public async Task Handle_WithValidData_ShouldSucceed()
    {
        // Arrange
        var workOrder = WorkOrderFactory.CreateWorkOrder().Value;
        var labor = EmployeeFactory.CreateLabor().Value;

        await _context.WorkOrders.AddAsync(workOrder);
        await _context.Employees.AddAsync(labor);
        await _context.SaveChangesAsync(default);

        var command = new AssignLaborCommand(
            WorkOrderId: workOrder.Id,
            LaborId: labor.Id);

         // Act
        var result = await _mediator.Send(command);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.IsType<Updated>(result.Value);

        var updatedWorkOrder = await _context.WorkOrders.FindAsync(workOrder.Id);
        Assert.Equal(labor.Id, updatedWorkOrder!.LaborId);
    }

    [Fact]
    public async Task Handle_WithInvalidLaborId_Should_Return_LaborId_Required()
    {
        // Arrange
        var workOrder = WorkOrderFactory.CreateWorkOrder().Value;
        await _context.WorkOrders.AddAsync(workOrder);
        await _context.SaveChangesAsync(default);

        var command = new AssignLaborCommand(
            WorkOrderId: workOrder.Id,
            LaborId: Guid.Empty);

        // Act
        var result = await _mediator.Send(command);

        // Assert
        Assert.True(result.IsError);
        Assert.Contains(result.Errors, e => e.Code == "LaborId_Required");
    }

    [Fact]
    public async Task Handle_WithInvalidWorkOrderId_Should_Return_WorkOrderId_Required()
    {
        // Arrange
        var labor = EmployeeFactory.CreateEmployee().Value;
        await _context.Employees.AddAsync(labor);
        await _context.SaveChangesAsync(default);

        var command = new AssignLaborCommand(
            WorkOrderId: Guid.Empty,
            LaborId: labor.Id);

        // Act
        var result = await _mediator.Send(command);

        // Assert
        Assert.True(result.IsError);
        Assert.Contains(result.Errors, e => e.Code == "WorkOrderId_Required");
    }

    [Fact]
    public async Task Handle_When_Labor_Is_Occupied_Should_Return_LaborOccupied_Error()
    {
        // Arrange
        var labor = EmployeeFactory.CreateLabor().Value;
        var workOrder1 = WorkOrderFactory.CreateWorkOrder(laborId: labor.Id).Value;

        var workOrder2 = WorkOrderFactory.CreateWorkOrder().Value;

        await _context.Employees.AddAsync(labor);
        await _context.WorkOrders.AddRangeAsync(workOrder1, workOrder2);
        await _context.SaveChangesAsync(default);

        var command = new AssignLaborCommand(
            WorkOrderId: workOrder2.Id,
            LaborId: labor.Id);

        // Act
        var result = await _mediator.Send(command);

        // Assert
        Assert.True(result.IsError);
        Assert.Contains(result.Errors, e => e.Code == "LaborOccupied");
    }
}