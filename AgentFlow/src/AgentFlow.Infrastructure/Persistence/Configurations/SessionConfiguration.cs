using AgentFlow.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgentFlow.Infrastructure.Persistence.Configurations
{
    public sealed class SessionConfiguration : IEntityTypeConfiguration<Session>
    {
        /// <summary>
        /// Configures EF Core mappings for the Session entity.
        /// </summary>
        public void Configure(EntityTypeBuilder<Session> builder)
        {
            builder.ToTable("Sessions");
            builder.HasKey(s => s.Id);
            builder.Property(s => s.Prompt)
                .IsRequired()
                .HasMaxLength(100000);
            builder.Property(s => s.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("CURRENT_TIMESTAMP");
            builder.Property(x => x.State)
                .HasConversion<string>()
                .HasMaxLength(30)
                .IsRequired();

            builder.Property(x => x.UpdatedAt)
                .IsRequired();

            builder.Property(x => x.FailureReason)
                .HasMaxLength(2000);
        }
    }
}