namespace ChronoKraft.Tests.Unit;

[Trait("Category", "Unit")]
public class DomainIsolationTests
{
    [Fact]
    public void Domain_Does_not_reference_Infrastructure_Packages()
    {
        // Prepare
        var domainAssembly = typeof(ChronoKraft.Domain.Entities.Project).Assembly;
        var referencedAssemblyNames = domainAssembly.GetReferencedAssemblies()
            .Select(a => a.Name)
            .Where(name => name != null)
            .ToList();
        // Act
        string[] forbiddenPrefixes = { "Npgsql", "Serilog", "Microsoft.EntityFrameworkCore", "Microsoft.AspNetCore" };
        // Assert
        foreach (var forbiddenPrefix in forbiddenPrefixes)
        {
            Assert.DoesNotContain(referencedAssemblyNames, name => name!.StartsWith(forbiddenPrefix));
        }
    }
}
