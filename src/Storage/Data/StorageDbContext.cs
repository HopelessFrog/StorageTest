using Microsoft.EntityFrameworkCore;
using Storage.Data.Entities;

namespace Storage.Data;

public class StorageDbContext : DbContext
{
    public StorageDbContext(DbContextOptions<StorageDbContext> options) : base(options)
    {
    }

    public DbSet<Resource> Resources { get; set; }
    public DbSet<Unit> Units { get; set; }
    public DbSet<Receipt> Receipts { get; set; }
    public DbSet<IncomeResource> IncomeResources { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        ConfigureResource(modelBuilder);
        ConfigureUnit(modelBuilder);
        ConfigureReceipt(modelBuilder);
        ConfigureIncomeResource(modelBuilder);
    }

    private static void ConfigureResource(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Resource>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Name)
                  .IsRequired()
                  .HasMaxLength(50);

            entity.HasIndex(e => e.Name).IsUnique();
            entity.HasIndex(e => e.State);
        });
    }

    private static void ConfigureUnit(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Unit>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Name)
                  .IsRequired()
                  .HasMaxLength(50);

            entity.HasIndex(e => e.Name).IsUnique();
            entity.HasIndex(e => e.State);
        });
    }

    private static void ConfigureReceipt(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Receipt>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Number)
                  .IsRequired()
                  .HasMaxLength(50);

            entity.HasIndex(e => e.Number).IsUnique();

            entity.HasMany(d => d.IncomeResources)
                  .WithOne(r => r.Receipt)
                  .HasForeignKey(r => r.ReceiptId)
                  .OnDelete(DeleteBehavior.Cascade);
        });
    }

    private static void ConfigureIncomeResource(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<IncomeResource>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.HasOne(ir => ir.Resource)
                  .WithMany()
                  .HasForeignKey(ir => ir.ResourceId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(ir => ir.Unit)
                  .WithMany()
                  .HasForeignKey(ir => ir.UnitId)
                  .OnDelete(DeleteBehavior.Restrict);
            
            entity.Property(e => e.Quantity)
                  .IsRequired()
                  .HasPrecision(16, 3);

            entity.HasIndex(e => e.ReceiptId);
            entity.HasIndex(e => e.ResourceId);
            entity.HasIndex(e => e.UnitId);
        });
    }
}
