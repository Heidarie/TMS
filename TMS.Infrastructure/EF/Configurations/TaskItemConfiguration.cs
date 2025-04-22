using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TMS.Domain.Kernel.Types;
using TMS.Domain.Tasks.Entities;

namespace TMS.Infrastructure.EF.Configurations;

public class TaskItemConfiguration : IEntityTypeConfiguration<TaskItem>
{
    public void Configure(EntityTypeBuilder<TaskItem> builder)
    {
        builder.HasKey(t => t.Id);

        builder.Property(t => t.Id).HasConversion(x => x.Value, x => new AggregateId(x))
            .UseIdentityColumn();

        builder.Property(t => t.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(t => t.Description)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(t => t.Version)
            .IsConcurrencyToken();

        builder.Property(t => t.Status).IsRequired();
    }
}