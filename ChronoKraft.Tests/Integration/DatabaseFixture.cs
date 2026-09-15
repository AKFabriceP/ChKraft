using ChronoKraft.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace ChronoKraft.Tests.Integration;
public class DatabaseFixture : IDisposable
{
    private const string ConnectionString =
        "Host=localhost;Port=5432;Database=chronok_db_test;Username=chronok_adm;Password=K53N0wadm";
    public CKDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<CKDbContext>()
            .UseNpgsql(ConnectionString, npgsqlOptions =>
                npgsqlOptions.ConfigureDataSource(dataSourceBuilder =>
                    dataSourceBuilder.EnableDynamicJson()))
            .Options;
        var context = new CKDbContext(options);
        context.Database.EnsureCreated();
        return context;
    }

    public void Dispose()
    {
        using var context = CreateContext();
        context.WorkerSkills.RemoveRange(context.WorkerSkills);
        context.Workers.RemoveRange(context.Workers);
        context.ResourceTypes.RemoveRange(context.ResourceTypes);
        context.Contractors.RemoveRange(context.Contractors);
        context.SaveChanges();
    }
}