using MechanicShop.Application.Features.RepairTasks.Queries.GetRepairTaskById;
using Xunit;

namespace MechanicShop.Application.SubcutaneousTests.Features.RepairTasks.Queries.GetRepairTaskById;

public class GetRepairTaskByIdQueryValidatorTests
{
    private readonly GetRepairTaskByIdQueryValidator _validator;

    public GetRepairTaskByIdQueryValidatorTests()
    {
        _validator = new GetRepairTaskByIdQueryValidator();
    }

    [Fact]
    public void Validate_When_RepairTaskId_Is_Empty_Should_Return_Error()
    {
        var command = new GetRepairTaskByIdQuery(Guid.Empty);

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "RepairTaskId");
    }

    [Fact]
    public void Validate_When_RepairTaskId_Is_Valid_Should_Success()
    {
        var command = new GetRepairTaskByIdQuery(Guid.NewGuid());

        var result = _validator.Validate(command);

        Assert.True(result.IsValid);
    }
}