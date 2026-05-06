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
    internal sealed class QuestionConfiguration : IEntityTypeConfiguration<Question>, IWriteEntityConfiguration
    {
        public void Configure(EntityTypeBuilder<Question> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.TextEn)
                .IsRequired()
                .HasMaxLength(1000);

            builder.Property(x => x.TextAr)
                .IsRequired(false)
                .HasMaxLength(1000);

            builder.Property(x => x.Type)
                .IsRequired();

            builder.Property(x => x.IsActive)
                .IsRequired();

            builder.Property(x => x.CreatedByApplicationUserId)
                .IsRequired();

            builder.HasIndex(x => x.BranchId);

            builder.HasIndex(x => x.GroupId);

            builder.HasOne(x => x.Branch)
                .WithMany()
                .HasForeignKey(x => x.BranchId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}