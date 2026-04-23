using MechanicShop.Application.Features.Billing.Mappers;
using MechanicShop.Application.Features.Customers.Mappers;
using MechanicShop.Domain.WorkOrders.Billing;
using MechanicShop.Tests.Common.Billing;
using MechanicShop.Tests.Common.Customers;
using MechanicShop.Tests.Common.WorkOrders;

using Xunit;

namespace MechanicShop.Application.UnitTests.Mappers;

    public class InvoiceMapperTest
    {
    [Fact]
    public void ToDto_WhenInvoiceIsFound_ShouldMapAllPropertiesCorrectly()
    {
    // Arrange
    var customer = CustomerFactory.CreateCustomer().Value;
    var vehicle = VehicleFactory.CreateVehicle().Value;
    var workOrder = WorkOrderFactory.CreateWorkOrder().Value;

    vehicle.Customer = customer;
    workOrder.Vehicle = vehicle;

    var invoice = InvoiceFactory.CreateInvoice(workOrderId: workOrder.Id).Value;
    invoice.WorkOrder = workOrder;

    // Act
    var invoiceDto = invoice.ToDto();

    // Assert
    Assert.NotNull(invoiceDto);

    Assert.Equal(invoice.Id, invoiceDto.InvoiceId);
    Assert.Equal(workOrder.Id, invoiceDto.WorkOrderId);

    // Customer
    Assert.NotNull(invoiceDto.Customer);
    Assert.Equal(customer.Id, invoiceDto.Customer!.CustomerId);
    Assert.Equal(customer.Name, invoiceDto.Customer.Name);
    Assert.Equal(customer.PhoneNumber, invoiceDto.Customer.PhoneNumber);
    Assert.Equal(customer.Email, invoiceDto.Customer.Email);

    // Vehicle
    Assert.NotNull(invoiceDto.Vehicle);
    Assert.Equal(vehicle.Id, invoiceDto.Vehicle!.VehicleId);
    Assert.Equal(vehicle.Make, invoiceDto.Vehicle.Make);
    Assert.Equal(vehicle.Model, invoiceDto.Vehicle.Model);
    Assert.Equal(vehicle.Year, invoiceDto.Vehicle.Year);
    Assert.Equal(vehicle.LicensePlate, invoiceDto.Vehicle.LicensePlate);

    // Money
    Assert.Equal(invoice.TaxAmount, invoiceDto.TaxAmount);
    Assert.Equal(invoice.Subtotal, invoiceDto.Subtotal);
    Assert.Equal(invoice.DiscountAmount, invoiceDto.DiscountAmount);
    Assert.Equal(invoice.Total, invoiceDto.Total);

    // Status
    Assert.Equal(invoice.Status.ToString(), invoiceDto.PaymentStatus);

    // Items
    Assert.Equal(invoice.LineItems.Count, invoiceDto.Items.Count);

    var firstItem = invoice.LineItems.First();
    var firstItemDto = invoiceDto.Items.First();

    Assert.Equal(firstItem.InvoiceId, firstItemDto.InvoiceId);
    Assert.Equal(firstItem.Quantity, firstItemDto.Quantity);
    Assert.Equal(firstItem.UnitPrice, firstItemDto.UnitPrice);
    }

    [Fact]
    public void ToDtos_WhenInvoiceIsFound_ShouldMapAllPropertiesCorrectly()
    {
        // Arrange
        var invoice = InvoiceFactory.CreateInvoice().Value;

        var workOrder = invoice.WorkOrder!;
        var vehicle = workOrder.Vehicle!;
        var customer = vehicle.Customer!;

        var invoices = new List<Invoice> { invoice };

        // Act
        var invoiceDtos = invoices.ToDtos();

        // Assert
        Assert.NotNull(invoiceDtos);
        Assert.Single(invoiceDtos);

        var dto = invoiceDtos[0];
        Assert.NotNull(dto.Customer);
        Assert.NotNull(dto.Vehicle);
        Assert.NotNull(dto.Items);

        Assert.Equal(invoice.Id, dto.InvoiceId);
        Assert.Equal(invoice.WorkOrderId, dto.WorkOrderId);
        Assert.Equal(customer.Id, dto.Customer!.CustomerId);
        Assert.Equal(vehicle.Id, dto.Vehicle!.VehicleId);
        Assert.Equal(invoice.TaxAmount, dto.TaxAmount);
        Assert.Equal(invoice.Subtotal, dto.Subtotal);
        Assert.Equal(invoice.DiscountAmount, dto.DiscountAmount);
        Assert.Equal(invoice.Total, dto.Total);
        Assert.Equal(invoice.Status.ToString(), dto.PaymentStatus);
        Assert.Equal(invoice.LineItems.Count, dto.Items.Count);

        var firstItem = invoice.LineItems.First();
        var firstItemDto = dto.Items.First();

        Assert.Equal(firstItem.InvoiceId, firstItemDto.InvoiceId);
        Assert.Equal(firstItem.Quantity, firstItemDto.Quantity);
        Assert.Equal(firstItem.UnitPrice, firstItemDto.UnitPrice);
    }

    [Fact]
    public void ToDto_WhenInvoiceLineItemIsFound_ShouldMapAllPropertiesCorrectly()
    {
        // Arrange
        var lineItems = InvoiceLineItemFactory.CreateInvoiceLineItem().Value;

        // Act
        var lineItemsDto = lineItems.ToDto();

        // Assert
        Assert.NotNull(lineItemsDto);
        Assert.Equal(lineItems.InvoiceId, lineItemsDto.InvoiceId);
        Assert.Equal(lineItems.LineNumber, lineItemsDto.LineNumber);
        Assert.Equal(lineItems.Description, lineItemsDto.Description);
        Assert.Equal(lineItems.Quantity, lineItemsDto.Quantity);
        Assert.Equal(lineItems.UnitPrice, lineItemsDto.UnitPrice);
        Assert.Equal(lineItems.LineTotal, lineItemsDto.LineTotal);
    }

    [Fact]
    public void ToDtos_WhenInvoiceLineItemIsFound_ShouldMapAllPropertiesCorrectly()
    {
         // Arrange
        var lineItems = InvoiceLineItemFactory.CreateInvoiceLineItem().Value;

        var lineItemsList = new List<InvoiceLineItem> { lineItems };

        // Act
        var lineItemsDtos = lineItemsList.ToDtos();

        // Assert
        Assert.NotNull(lineItemsDtos);
        Assert.Single(lineItemsDtos);

        var dto = lineItemsDtos[0];
        Assert.Equal(lineItems.InvoiceId, dto.InvoiceId);
        Assert.Equal(lineItems.LineNumber, dto.LineNumber);
        Assert.Equal(lineItems.Description, dto.Description);
        Assert.Equal(lineItems.Quantity, dto.Quantity);
        Assert.Equal(lineItems.UnitPrice, dto.UnitPrice);
        Assert.Equal(lineItems.LineTotal, dto.LineTotal);
    }
}