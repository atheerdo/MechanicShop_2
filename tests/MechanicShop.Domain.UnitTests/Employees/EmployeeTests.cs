using MechanicShop.Domain.Employees;
using MechanicShop.Domain.Identity;
using MechanicShop.Tests.Common.Employees;
using Xunit;

namespace MechanicShop.Domain.UnitTests.Employees;

public class EmployeeTests
{
    [Fact]
    public void Create_WithValidData_ShouldSuccess()
    {
        var id = Guid.NewGuid();
        const string firstName = "John";
        const string lastName = "Doe";
        const Role role = Role.Labor;

        var result = EmployeeFactory.CreateEmployee(id: id, firstName: firstName, lastName: lastName, role: role);

        Assert.True(result.IsSuccess);
        var employee = result.Value;
        Assert.Equal(id, employee.Id);
        Assert.Equal(firstName, employee.FirstName);
        Assert.Equal(lastName, employee.LastName);
        Assert.Equal(role, employee.Role);
        Assert.Equal("John Doe", employee.FullName);
    }

    [Fact]
    public void Create_WithEmptyId_ShouldFail()
    {
        var result = EmployeeFactory.CreateManager(Guid.Empty, "John", "Doe");

        Assert.True(result.IsError);
        Assert.Equal(EmployeeErrors.IdRequired.Code, result.TopError.Code);
        Assert.Equal(EmployeeErrors.IdRequired.Description, result.TopError.Description);
    }

    [Fact]
    public void Create_WithEmptyFirstName_ShouldFail()
    {
        var result = EmployeeFactory.CreateEmployee(
            id: Guid.NewGuid(),
            firstName: string.Empty,
            lastName: "Doe",
            role: Role.Labor);

        Assert.True(result.IsError);
        Assert.Equal(EmployeeErrors.FirstNameIsRequired.Code, result.TopError.Code);
        Assert.Equal(EmployeeErrors.FirstNameIsRequired.Description, result.TopError.Description);
    }

    [Fact]
    public void Create_WithEmptyLastName_ShouldFail()
    {
        var result = EmployeeFactory.CreateEmployee(
            id: Guid.NewGuid(),
            firstName: "John",
            lastName: string.Empty,
            role: Role.Labor);

        Assert.True(result.IsError);
        Assert.Equal(EmployeeErrors.LastNameIsRequired.Code, result.TopError.Code);
        Assert.Equal(EmployeeErrors.LastNameIsRequired.Description, result.TopError.Description);
    }

    [Fact]
    public void Create_WithInvalidRole_ShouldFail()
    {
        var result = EmployeeFactory.CreateEmployee(
            id: Guid.NewGuid(),
            firstName: "John",
            lastName: "Doe",
            role: (Role)999);

        Assert.True(result.IsError);
        Assert.Equal(EmployeeErrors.RoleInvalid.Code, result.TopError.Code);
        Assert.Equal(EmployeeErrors.RoleInvalid.Description, result.TopError.Description);
    }
}