using FamilyTools.Data.Models.EasyCompta;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FamilyTools.Data.Configuration.EasyCompta;

public class AccountPageEntityTypeConfiguration : IEntityTypeConfiguration<AccountPage>
{
    public void Configure(EntityTypeBuilder<AccountPage> builder)
    {
        builder.ToTable("AccountPages");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Name).IsRequired();
        builder.Property(e => e.IsClosing).IsRequired();
        builder.Property(e => e.Date).IsRequired();
        builder.Property(e => e.CreationDate).IsRequired().HasDefaultValueSql("getdate()");
        builder.Property(e => e.UpdateDate);

        // Relation avec AccountEnters
        builder.HasMany(e => e.Enters)
            .WithOne(e => e.Page)
            .HasForeignKey(e => e.PageId)
            .IsRequired();

        builder.Navigation(e => e.Enters).AutoInclude();

        // Configuration du dictionnaire PaymentDone
        builder.HasMany<PaymentDone>(e => e.PaymentDones)
            .WithOne(e => e.Page)
            .HasForeignKey(e => e.PageId)
            .IsRequired();

        builder.Navigation(e => e.PaymentDones).AutoInclude();
    }
}