using MechanicShop.Application.Features.RepairTasks.Mappers;
using MechanicShop.Domain.RepairTasks;
using MechanicShop.Domain.RepairTasks.Parts;
using MechanicShop.Tests.Common.RepairTasks;
using Microsoft.Identity.Client;
using Xunit;

namespace MechanicShop.Application.UnitTests.Mappers;

    public class RepairTaskMapperTest
    {
    [Fact]
    public void ToDto_RepairTaskMapper_MappingIsCorrect()
    {
        // Arrange
        var repairTask = RepairTaskFactory.CreateRepairTask().Value;

        // Act
        var repairTaskDto = repairTask.ToDto();

        // Assert
        Assert.NotNull(repairTaskDto);

        Assert.Equal(repairTask.Id, repairTaskDto.RepairTaskId);
        Assert.Equal(repairTask.Name, repairTaskDto.Name);
        Assert.Equal(repairTask.LaborCost, repairTaskDto.LaborCost);
        Assert.Equal(repairTask.TotalCost, repairTaskDto.TotalCost);
        Assert.Equal(repairTask.EstimatedDurationInMins, repairTaskDto.EstimatedDurationInMins);
        Assert.Equal(repairTask.Parts.Count(), repairTaskDto.Parts.Count());

        var part = repairTask.Parts.First();
        var partDto = repairTaskDto.Parts.First();

        Assert.Equal(part.Id, partDto.PartId);
        Assert.Equal(part.Name, partDto.Name);
        Assert.Equal(part.Cost, partDto.Cost);
        Assert.Equal(part.Quantity, partDto.Quantity);
    }

    [Fact]
    public void ToDtos_RepairTaskMapper_MappingIsCorrect()
    {
         // Arrange
        var repairTask = RepairTaskFactory.CreateRepairTask().Value;
        var repairTasks = new List<RepairTask> { repairTask };

        // Act
        var repairTaskDtos = repairTasks.ToDtos();

        // Assert
        Assert.NotNull(repairTaskDtos);
        Assert.Single(repairTaskDtos);

        var dto = repairTaskDtos[0];
        Assert.Equal(repairTask.Id, dto.RepairTaskId);
        Assert.Equal(repairTask.Name, dto.Name);
        Assert.Equal(repairTask.LaborCost, dto.LaborCost);
        Assert.Equal(repairTask.TotalCost, dto.TotalCost);
        Assert.Equal(repairTask.EstimatedDurationInMins, dto.EstimatedDurationInMins);
        Assert.Equal(repairTask.Parts.Count(), dto.Parts.Count());

        var part = repairTask.Parts.First();
        var partDto = dto.Parts.First();

        Assert.Equal(part.Id, partDto.PartId);
        Assert.Equal(part.Name, partDto.Name);
        Assert.Equal(part.Cost, partDto.Cost);
        Assert.Equal(part.Quantity, partDto.Quantity);
    }

    [Fact]
    public void ToDtos_Parts_MappingIsCorrect()
    {
        // Arrange
        var part = PartFactory.CreatePart().Value;
        var parts = new List<Part> { part };

        // Act
        var partsDtos = parts.ToDtos();

        // Assert
        Assert.NotNull(partsDtos);
        Assert.Single(partsDtos);

        var dto = partsDtos[0];
        Assert.NotNull(dto);
        Assert.Equal(part.Id, dto.PartId);
        Assert.Equal(part.Name, dto.Name);
        Assert.Equal(part.Cost, dto.Cost);
        Assert.Equal(part.Quantity, dto.Quantity);
    }
}