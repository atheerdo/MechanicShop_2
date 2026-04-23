using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Features.Billing.Commands.IssueInvoice;
using MechanicShop.Application.SubcutaneousTests.Common;
using MechanicShop.Domain.WorkOrders;
using MechanicShop.Domain.WorkOrders.Enums;
using MechanicShop.Tests.Common.Employees;
using MechanicShop.Tests.Common.RepairTasks;
using MechanicShop.Tests.Common.WorkOrders;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace MechanicShop.Application.SubcutaneousTests.Features.Billing.Commands.IssueInvoice;

[Collection(WebAppFactoryCollection.CollectionName)]
public class IssueInvoiceCommandHandlerTests(WebAppFactory factory)
{
    private readonly IMediator _mediator = factory.CreateMediator();
    private readonly IAppDbContext _context = factory.CreateAppDbContext();

    [Fact]
    public async Task Handle_When_WorkOrder_Not_Found_Should_Fail()
    {
        // Arrange
        var command = new IssueInvoiceCommand(Guid.NewGuid());

        // Act
        var result = await _mediator.Send(command);

        // Assert
        Assert.True(result.IsError);
        Assert.Contains(result.Errors, e => e.Code == "ApplicationErrors.WorkOrder.NotFound");
    }

    [Fact]
    public async Task Handle_When_WorkOrder_Not_Completed_Should_Return_Error()
    {
        // Arrange
        var employee = EmployeeFactory.CreateEmployee().Value;

        await _context.Employees.AddAsync(employee);
        await _context.SaveChangesAsync(default);

        var repairTask = RepairTaskFactory.CreateRepairTask().Value;

        await _context.RepairTasks.AddAsync(repairTask);
        await _context.SaveChangesAsync(default);

        var workOrder = WorkOrderFactory.CreateWorkOrder(
        laborId: employee.Id,
        repairTasks: new List<MechanicShop.Domain.RepairTasks.RepairTask>
        {
           repairTask
        }).Value;

        await _context.WorkOrders.AddAsync(workOrder);
        await _context.SaveChangesAsync(default);

        var command = new IssueInvoiceCommand(workOrder.Id);

        // Act
        var result = await _mediator.Send(command);

        // Assert
        Assert.True(result.IsError);
    }

    [Fact]
    public async Task Handle_CreateInvoice_When_WorkOrder_Is_Completed()
    {
        // Arrange
        var workOrder = WorkOrderFactory.CreateWorkOrder().Value;

        workOrder.UpdateState(WorkOrderState.InProgress);
        workOrder.UpdateState(WorkOrderState.Completed);

        await _context.WorkOrders.AddAsync(workOrder);
        await _context.SaveChangesAsync(default);

        var command = new IssueInvoiceCommand(workOrder.Id);

        // Act
        var result = await _mediator.Send(command);

        // Assert
        Assert.True(result.IsSuccess);

        var invoiceDto = result.Value;
        Assert.Equal(workOrder.Id, invoiceDto.WorkOrderId);
    }

    [Fact]
    public async Task Handle_When_LineItem_Creation_Fails_Should_Return_Error()
    {
        // Arrange
        var invalidTaskResult = RepairTaskFactory.CreateRepairTask(laborCost: -100);

        Assert.True(invalidTaskResult.IsError);

        var workOrder = WorkOrderFactory.CreateWorkOrder().Value;

        workOrder.UpdateState(WorkOrderState.InProgress);
        workOrder.UpdateState(WorkOrderState.Completed);

        await _context.WorkOrders.AddAsync(workOrder);
        await _context.SaveChangesAsync(default);

        var command = new IssueInvoiceCommand(workOrder.Id);

        // Act
        var result = await _mediator.Send(command);

        // Assert
        Assert.True(result.IsError);
    }
}