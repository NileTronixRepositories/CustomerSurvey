using BuildingBlock.Application.Abstraction.Persistence;
using CustomerSurvey.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.infrastructure.Configurations
{
    internal sealed class OperatorTemplateConfiguration : IEntityTypeConfiguration<OperatorTemplate>, IWriteEntityConfiguration
    {
        public void Configure(EntityTypeBuilder<OperatorTemplate> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.CreatedByApplicationUserId)
                .IsRequired();

            builder.HasIndex(x => x.OperatorId);

            builder.HasIndex(x => x.TemplateId);

            builder.HasIndex(x => new { x.OperatorId, x.TemplateId })
                .IsUnique();

            builder.HasOne(x => x.Template)
                .WithMany()
                .HasForeignKey(x => x.TemplateId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}