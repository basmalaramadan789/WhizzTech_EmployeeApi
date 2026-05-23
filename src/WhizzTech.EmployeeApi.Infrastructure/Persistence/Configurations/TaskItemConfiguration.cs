using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WhizzTech.EmployeeApi.Domain.Entities;

namespace WhizzTech.EmployeeApi.Infrastructure.Persistence.Configurations;

public class TaskItemConfiguration : IEntityTypeConfiguration<TaskItem>
{
    public void Configure(EntityTypeBuilder<TaskItem> builder)
    {
        builder.ToTable("Tasks");
        builder.HasKey(t => t.Id);
        builder.Property(t => t.Id).HasColumnName("Id").ValueGeneratedNever();
        builder.Property(t => t.Name).HasColumnName("Name").HasMaxLength(200).IsRequired();
        builder.Property(t => t.Description).HasColumnName("Description").HasMaxLength(1000);
        builder.Property(t => t.DueDate).HasColumnName("DueDate").IsRequired();
    }
}
