using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Features.Customers.Dtos;
using MechanicShop.Application.Features.Customers.Mappers;
using MechanicShop.Domain.Common.Errors;
using MechanicShop.Domain.Common.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MechanicShop.Application.Features.Customers.Queries.GetCustomerById;

public class GetCustomerByIdQueryHandler(
            ILogger<GetCustomerByIdQueryHandler> logger,
            IAppDbContext context)
            : IRequestHandler<GetCustomerByIdQuery, Result<CustomerDto>>
{
    private readonly ILogger<GetCustomerByIdQueryHandler> _logger = logger;
    private readonly IAppDbContext _context = context;

    public async Task<Result<CustomerDto>> Handle(GetCustomerByIdQuery command, CancellationToken ct)
    {
        var customer = await _context.Customers.AsTracking().Include(c => c.Vehicles)
                         .FirstOrDefaultAsync(c => c.Id == command.CustomerId, ct);

        if (customer is null)
        {
            _logger.LogError("Customer with id: '{CustomerId}' does not exist", command.CustomerId);

            return ApplicationErrors.CustomerNotFound;
        }

        return customer.ToDto();
    }
}