using FamilyTools.Data.Configuration.EasyCompta;
using FamilyTools.Data.Models.EasyCompta;
using FamilyTools.Data.Seed.EasyCompta;
using Microsoft.EntityFrameworkCore;

namespace FamilyTools.Data.Context;

public class EasyComptaContext(DbContextOptions<EasyComptaContext> options) : DbContext(options)
{
    public DbSet<User> Users => this.Set<User>();
    public DbSet<AccountEnter> AccountEnters => this.Set<AccountEnter>();
    public DbSet<AccountLine> AccountLines => this.Set<AccountLine>();
    public DbSet<AccountPage> AccountPages => this.Set<AccountPage>();
    public DbSet<AccountTag> AccountTags => this.Set<AccountTag>();
    public DbSet<Template> Templates => this.Set<Template>();
    public DbSet<PaymentDone> PaymentDones => this.Set<PaymentDone>();
    public DbSet<OperationType> OperationTypes => this.Set<OperationType>();

    public async Task EnsureSeedData()
    {
        ContextInitializer initializer = new();
        await initializer.Seed(this);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Configuration de User
        new UserEntityTypeConfiguration().Configure(modelBuilder.Entity<User>());

        // Configuration de AccountTag
        new AccountTagEntityTypeConfiguration().Configure(modelBuilder.Entity<AccountTag>());

        // Configuration de AccountLine
        new AccountLineEntityTypeConfiguration().Configure(modelBuilder.Entity<AccountLine>());

        // Configuration de AccountEnter
        new AccountEnterEntityTypeConfiguration().Configure(modelBuilder.Entity<AccountEnter>());

        // Configuration de AccountPage
        new AccountPageEntityTypeConfiguration().Configure(modelBuilder.Entity<AccountPage>());

        // Configuration de Template
        new TemplateEntityTypeConfiguration().Configure(modelBuilder.Entity<Template>());

        new PaymentDoneEntityTypeConfiguration().Configure(modelBuilder.Entity<PaymentDone>());

        new OperationTypeEntityTypeConfiguration().Configure(modelBuilder.Entity<OperationType>());

        base.OnModelCreating(modelBuilder);
    }
}