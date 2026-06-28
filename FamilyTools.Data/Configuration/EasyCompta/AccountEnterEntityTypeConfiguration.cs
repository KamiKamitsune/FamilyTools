using FamilyTools.Data.Models.EasyCompta;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FamilyTools.Data.Configuration.EasyCompta;

public class AccountEnterEntityTypeConfiguration : IEntityTypeConfiguration<AccountEnter>
{
    public void Configure(EntityTypeBuilder<AccountEnter> builder)
    {
        builder.ToTable("AccountEnters");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Name).IsRequired();
        builder.Property(e => e.TotalValue).IsRequired();
        builder.Property(e => e.Date).IsRequired();
        builder.Property(e => e.IsDisabled).IsRequired();
        builder.Property(e => e.CreationDate).IsRequired().HasDefaultValueSql("getdate()");
        builder.Property(e => e.UpdateDate);

        // Relation avec AccountLines
        builder.HasMany(e => e.Lines)
            .WithOne(e => e.Enter)
            .HasForeignKey(e => e.EnterId)
            .IsRequired();

        builder.Navigation(e => e.Lines).AutoInclude();

        // Relation avec AccountTag
        builder.HasOne(e => e.Tag)
            .WithMany()
            .HasForeignKey("TagId")
            .OnDelete(DeleteBehavior.Restrict);

        builder.Navigation(e => e.Tag).AutoInclude();

        builder.HasOne(e => e.OperationType)
            .WithMany()
            .HasForeignKey("OperationTypeId")
            .OnDelete(DeleteBehavior.Restrict);

        builder.Navigation(e => e.OperationType).AutoInclude();

    }
}