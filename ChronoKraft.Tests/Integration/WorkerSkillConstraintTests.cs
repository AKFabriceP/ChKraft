using ChronoKraft.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ChronoKraft.Tests.Integration;

[Trait("Category", "Integration")]
public class WorkerSkillConstraintTests : IClassFixture<DatabaseFixture>
{
    private readonly DatabaseFixture _fixture;

    public WorkerSkillConstraintTests(DatabaseFixture fixture) => _fixture = fixture;

    [Fact]
    public async Task When_WorkerSkill_For_Worker_And_ResourceType_Already_Exist_Then_Cannot_Create_Duplicate_WorkerSkill()
    {
        using var context = _fixture.CreateContext();

        var contractor = new Contractor { Name = "Test Contractor" };
        var resourceType = new ResourceType { Name = "Soudeur Test", ResourceCategory = ResourceCategory.LABOR };
        context.Contractors.Add(contractor);
        context.ResourceTypes.Add(resourceType);
        await context.SaveChangesAsync();

        var worker = new Worker { ContractorId = contractor.Id, FirstName = "Jean", LastName = "Tremblay" };
        context.Workers.Add(worker);
        await context.SaveChangesAsync();

        context.WorkerSkills.Add(new WorkerSkill { WorkerId = worker.Id, ResourceTypeId = resourceType.Id });
        await context.SaveChangesAsync();

        context.WorkerSkills.Add(new WorkerSkill { WorkerId = worker.Id, ResourceTypeId = resourceType.Id });

        await Assert.ThrowsAsync<DbUpdateException>(() => context.SaveChangesAsync());
    }
}