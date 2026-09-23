using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Reflection.Emit;
using TaskScheduler.MinimalAPI.Models.Entities;

namespace TaskScheduler.MinimalAPI.Data;

/// <summary>
/// Контекст базы данных для планировщика задач
/// </summary>
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<TaskEntity> Tasks { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TaskEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasDefaultValueSql("NEWID()");

            entity.Property(e => e.Title)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(e => e.Description)
                .HasMaxLength(1000);

            entity.Property(e => e.Category)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(e => e.Status)
                .IsRequired()
                .HasMaxLength(20)
                .HasDefaultValue("New");

            entity.Property(e => e.Priority)
                .IsRequired()
                .HasDefaultValue(3);

            entity.Property(e => e.IsActive)
                .IsRequired()
                .HasDefaultValue(true);

            entity.Property(e => e.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()");

            entity.Property(e => e.UpdatedAt).IsRequired(false);
            entity.Property(e => e.DeletedAt).IsRequired(false);

            // JSON-колонка для тегов
            entity.Property(e => e.Tags)
                .HasColumnType("nvarchar(max)")
                .HasConversion(
                    v => System.Text.Json.JsonSerializer.Serialize(v, new System.Text.Json.JsonSerializerOptions()),
                    v => System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(v, new System.Text.Json.JsonSerializerOptions())
                         ?? new Dictionary<string, object>());

            // Индексы
            entity.HasIndex(e => e.Category).HasDatabaseName("IX_Tasks_Category");
            entity.HasIndex(e => e.Status).HasDatabaseName("IX_Tasks_Status");
            entity.HasIndex(e => e.DueDate).HasDatabaseName("IX_Tasks_DueDate");
            entity.HasIndex(e => e.DeletedAt).HasDatabaseName("IX_Tasks_DeletedAt");
            entity.HasIndex(e => new { e.Category, e.DeletedAt })
                .HasDatabaseName("IX_Tasks_Category_DeletedAt");

            entity.ToTable("Tasks");
        });

        base.OnModelCreating(modelBuilder);
    }
}