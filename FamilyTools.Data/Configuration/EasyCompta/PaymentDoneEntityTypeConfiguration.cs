using FamilyTools.Data.Models.EasyCompta;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FamilyTools.Data.Configuration.EasyCompta;

public class PaymentDoneEntityTypeConfiguration : IEntityTypeConfiguration<PaymentDone>
{
    public void Configure(EntityTypeBuilder<PaymentDone> builder)
    {
        builder.ToTable("PaymentDones");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Total).IsRequired();
        builder.Property(e => e.PaymentIsDone).IsRequired();
        builder.Property(e => e.CreationDate).IsRequired().HasDefaultValueSql("getdate()");
        builder.Property(e => e.UpdateDate);

        // Relation avec User
        builder.HasOne(e => e.User)
            .WithMany()
            .HasForeignKey(e => e.UserId)
            .IsRequired();

        builder.Navigation(e => e.User).AutoInclude();
    }
}