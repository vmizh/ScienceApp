using Interfaces;
using MangoDbBaseRepository;
using Microsoft.Extensions.DependencyInjection;
using ScientificApp.RandomHistSerice.Model;
using ScientificApp.RandomHistService.Repositories.RandomExperimentSetRepository;

namespace ScientificApp.RandomHistService;

public static class ServiceRegistration
{
    public static IServiceCollection AddRandomHistServiceDependencies(this IServiceCollection services)
    {
        services.AddScoped<IBaseRepository<RandomExperimentSet>, MongoBaseRepository<RandomExperimentSet>>();
        services.AddScoped<IRandomExperimentSetRepository, RandomExperimentSetRepository>();
       
        return services;
    }
}
