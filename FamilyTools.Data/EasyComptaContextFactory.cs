using FamilyTools.Data.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace FamilyTools.Data;

public class EasyComptaContextFactory : IDesignTimeDbContextFactory<EasyComptaContext>
{
    public EasyComptaContext CreateDbContext(string[] args)
    {
        // Construire la configuration depuis appsettings.Design.json
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.Design.json", false, false)
            .Build();

        var connectionString = configuration.GetConnectionString("DefaultConnection");

        if (string.IsNullOrEmpty(connectionString))
            throw new InvalidOperationException(
                "La chaîne de connexion 'DefaultConnection' est introuvable dans appsettings.Design.json");

        var optionsBuilder = new DbContextOptionsBuilder<EasyComptaContext>();
        optionsBuilder.UseSqlServer(connectionString);

        return new EasyComptaContext(optionsBuilder.Options);
    }
}