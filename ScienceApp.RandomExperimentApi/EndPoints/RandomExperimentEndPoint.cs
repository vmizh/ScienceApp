using MangoDbBaseRepository;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
using ScientificApp.RandomHistSerice.Model;
using ScientificApp.RandomHistService.Repositories.RandomExperimentSetRepository;

namespace ScienceApp.RandomExperimentApi.EndPoints;

public static class RandomExperimentEndPoint
{
    public static void ConfigureRandomExperimentEndPoint(this WebApplication app)
    {
        var pointMap = app.MapGroup("/api/random-experiment");
        pointMap.MapGet("/{id:guid}", GetExperiment).WithName("GetExperiment");
    }

    private static async Task<IResult> GetExperiment(IRandomExperimentSetRepository repository,
        ILogger<RandomExperimentSet> log, Guid id, CancellationToken cancellationToken)
    {
        log.LogInformation($"Получение записи эжксперимента с id - '{id}'");
        var result = await ((MongoBaseRepository<RandomExperimentSet>)repository).GetByIdAsync(id, cancellationToken);
        return Results.Ok(result?.MapToDto());
    }
}
