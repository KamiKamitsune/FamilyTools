using FamilyTools.Data.Models.EasyCompta;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FamilyTools.Data.Configuration.EasyCompta;

public class TemplateEntityTypeConfiguration : IEntityTypeConfiguration<Template>
{
    public void Configure(EntityTypeBuilder<Template> builder)
    {
        builder.ToTable("Templates");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Name).IsRequired();
        builder.Property(e => e.Date).IsRequired();
        builder.Property(e => e.CreationDate).IsRequired().HasDefaultValueSql("getdate()");
        builder.Property(e => e.UpdateDate);

        // Relation avec AccountLines
        builder.HasMany(e => e.Enters)
            .WithMany()
            .UsingEntity(j => j.ToTable("TemplateEnters"));

        builder.Navigation(e => e.Enters).AutoInclude();
    }
}