using MechanicShop.Application.Features.Labors.Mappers;
using MechanicShop.Domain.Employees;
using MechanicShop.Tests.Common.Employees;
using Xunit;

namespace MechanicShop.Application.UnitTests.Mappers;

public class LaborMapperTest
{
    [Fact]
    public void ToDto_Should_Map_Employee_To_LaborDto()
    {
        // Arrange
        var labor = EmployeeFactory.CreateLabor().Value;

        // Act
        var laborDto = labor.ToDto();

        // Assert
        Assert.Equal(labor.Id, laborDto.LaborId);
        Assert.Equal(labor.FullName, laborDto.Name);
    }

    [Fact]
    public void ToDtos_Should_Map_Employees_To_LaborDtos()
    {
        // Arrange
        var labor = EmployeeFactory.CreateLabor().Value;
        var labors = new List<Employee> { labor };

        // Act
        var laborDtos = labors.ToDtos();

        // Assert
        Assert.NotNull(laborDtos);
        Assert.Single(laborDtos);
        var laborDto = laborDtos[0];

        Assert.Equal(labor.Id, laborDto.LaborId);
        Assert.Equal(labor.FullName, laborDto.Name);
    }
}