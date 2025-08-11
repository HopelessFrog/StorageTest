using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Storage.Common;
using Storage.Data.Entities;

namespace Storage.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(StorageDbContext context)
    {
        if (!context.Units.Any())
        {
            var baseUnits = new[]
            {
                new Unit { Name = "шт", State = ArchiveState.Active },
                new Unit { Name = "кг", State = ArchiveState.Active },
                new Unit { Name = "л", State = ArchiveState.Active },
                new Unit { Name = "м", State = ArchiveState.Active },
                new Unit { Name = "м²", State = ArchiveState.Active },
                new Unit { Name = "м³", State = ArchiveState.Active }
            };

            await context.Units.AddRangeAsync(baseUnits);
            await context.SaveChangesAsync();
        }

        var existingUnitNames = await context.Units.AsNoTracking().Select(u => u.Name).ToHashSetAsync();
        var unitsToAdd = new List<Unit>();
        for (int i = 1; existingUnitNames.Count + unitsToAdd.Count < 100; i++)
        {
            var candidate = $"Unit {i:D3}";
            if (existingUnitNames.Contains(candidate)) continue;
            unitsToAdd.Add(new Unit { Name = candidate, State = ArchiveState.Active });
        }
        if (unitsToAdd.Count > 0)
        {
            await context.Units.AddRangeAsync(unitsToAdd);
            await context.SaveChangesAsync();
        }

        if (!context.Resources.Any())
        {
            var baseResources = new[]
            {
                new Resource { Name = "A4 paper", State = ArchiveState.Active },
                new Resource { Name = "Ballpoint pens", State = ArchiveState.Active },
                new Resource { Name = "Pencils", State = ArchiveState.Active }
            };

            await context.Resources.AddRangeAsync(baseResources);
            await context.SaveChangesAsync();
        }

        var existingResourceNames = await context.Resources.AsNoTracking().Select(r => r.Name).ToHashSetAsync();
        var resourcesToAdd = new List<Resource>();
        for (int i = 1; existingResourceNames.Count + resourcesToAdd.Count < 100; i++)
        {
            var candidate = $"Resource {i:D3}";
            if (existingResourceNames.Contains(candidate)) continue;
            resourcesToAdd.Add(new Resource { Name = candidate, State = ArchiveState.Active });
        }
        if (resourcesToAdd.Count > 0)
        {
            await context.Resources.AddRangeAsync(resourcesToAdd);
            await context.SaveChangesAsync();
        }

        var rng = new Random(123);
        var unitIds = await context.Units.AsNoTracking().Select(u => u.Id).ToListAsync();
        var resourceIds = await context.Resources.AsNoTracking().Select(r => r.Id).ToListAsync();

        var existingReceiptNumbers = await context.Receipts.AsNoTracking().Select(r => r.Number).ToHashSetAsync();
        int currentReceipts = existingReceiptNumbers.Count;

        var receiptsToCreate = new List<Receipt>();
        var incomeResourcesToCreate = new List<IncomeResource>();

        for (int i = currentReceipts + 1; i <= 100; i++)
        {
            string number;
            int attempt = 0;
            do
            {
                number = $"Receipt-{i + attempt:D6}";
                attempt++;
            } while (existingReceiptNumbers.Contains(number));
            existingReceiptNumbers.Add(number);

            var baseDateUtc = new DateTimeOffset(DateTime.UtcNow.Date, TimeSpan.Zero);
            var date = baseDateUtc
                .AddDays(-rng.Next(0, 180))
                .AddHours(rng.Next(0, 24))
                .AddMinutes(rng.Next(0, 60));

            var receipt = new Receipt
            {
                Number = number,
                Date = date
            };
            receiptsToCreate.Add(receipt);

            int lines = rng.Next(1, 6);
            var pickedResources = new HashSet<int>();
            for (int j = 0; j < lines; j++)
            {
                if (resourceIds.Count == 0 || unitIds.Count == 0) break;

                int resourceId;
                int guard = 0;
                do
                {
                    resourceId = resourceIds[rng.Next(resourceIds.Count)];
                    guard++;
                } while (pickedResources.Contains(resourceId) && guard < 10);
                pickedResources.Add(resourceId);

                int unitId = unitIds[rng.Next(unitIds.Count)];

                var quantity = Math.Max(0.001m, Math.Round((decimal)rng.NextDouble() * 100m, 3));

                incomeResourcesToCreate.Add(new IncomeResource
                {
                    ResourceId = resourceId,
                    UnitId = unitId,
                    Quantity = quantity,
                    Receipt = receipt
                });
            }
        }

        if (receiptsToCreate.Count > 0)
        {
            await context.Receipts.AddRangeAsync(receiptsToCreate);
            await context.IncomeResources.AddRangeAsync(incomeResourcesToCreate);
            await context.SaveChangesAsync();
        }
    }
}