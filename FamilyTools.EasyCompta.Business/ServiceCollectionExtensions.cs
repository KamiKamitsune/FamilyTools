using FamilyTools.EasyCompta.IBusiness;
using Microsoft.Extensions.DependencyInjection;

namespace FamilyTools.EasyCompta.Business;

public static class ServiceCollectionExtensions
{
    /// <summary>Enregistre la couche métier EasyCompta, partagée par l'API et le worker d'import.</summary>
    public static IServiceCollection AddEasyComptaBusiness(this IServiceCollection services)
    {
        services.AddScoped<IUserBusiness, UserBusiness>();
        services.AddScoped<IPaymentDoneBusiness, PaymentDoneBusiness>();
        services.AddScoped<ITemplateBusiness, TemplateBusiness>();
        services.AddScoped<IAccountEnterBusiness, AccountEnterBusiness>();
        services.AddScoped<IAccountTagBusiness, AccountTagBusiness>();
        services.AddScoped<IAccountPageBusiness, AccountPageBusiness>();
        services.AddScoped<IAccountLineBusiness, AccountLineBusiness>();
        services.AddScoped<IImportCSVBusiness, ImportCSVBusiness>();
        services.AddScoped<IOperationTypeBusiness, OperationTypeBusiness>();

        return services;
    }
}
