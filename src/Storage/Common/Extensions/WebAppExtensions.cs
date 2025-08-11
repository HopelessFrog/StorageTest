using Microsoft.EntityFrameworkCore;
using Storage.Data;

namespace Storage.Common.Extensions;

public static class WebAppExtensions
{
    public static async Task EnsureDatabaseCreated(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var database = scope.ServiceProvider.GetRequiredService<StorageDbContext>();

        try
        {
            await database.Database.MigrateAsync();
        }
        catch (Exception e)
        {
            await database.Database.EnsureCreatedAsync();
        }

        await DbSeeder.SeedAsync(database);
    }
}