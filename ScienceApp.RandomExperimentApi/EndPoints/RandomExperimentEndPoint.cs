using CommonHelper;
using MangoDbBaseRepository;
using ScienceApp.Dto.RandomExperiment;
using ScientificApp.RandomHistSerice.Model;
using ScientificApp.RandomHistService.Repositories.RandomExperimentSetRepository;

namespace ScienceApp.RandomExperimentApi.EndPoints;

public static class RandomExperimentEndPoint
{
    private static readonly string _mapName = "/api/random-experiment";
    public static void ConfigureRandomExperimentEndPoint(this WebApplication app)
    {
        var pointMap = app.MapGroup(_mapName);
        pointMap.MapGet("/{id:guid}", GetExperiment).WithName("GetExperiment");
        pointMap.MapGet("/period", GetResultForPeriod).WithName("GetResultForPeriod");
        pointMap.MapPost("/execute", ExecuteExperiment).WithName("ExecuteExperiment");
        pointMap.MapPut("/calcaggregate", CalcAggregate).WithName("CalcAggregate");
    }

    private static async Task<IResult> CalcAggregate(IRandomExperimentSetRepository repository,
        ILogger<RandomExperimentSet> log, DateTime start, DateTime end, int rangeValue, CancellationToken cancellationToken)
    {
        log.LogInformation(
            $"Расчет агрегатов для результатов за период с '{start}' по '{end}' {_mapName}/GetResultForPeriod");
        try
        {
            var res = await repository.CalcAggregate(start, end, rangeValue);
            return Results.Ok($"Расчет выполнен для {res} экспериментов.");
        }
        catch (Exception ex)
        {
            var msg = ex.GetAllMessages();
            log.LogError($"Ошибка в {_mapName}/ExecuteExperiment {msg}");
            return Results.InternalServerError(msg);
        }
    }

    private static async Task<IResult> GetResultForPeriod(IRandomExperimentSetRepository repository,
        ILogger<RandomExperimentSet> log, DateTime start, DateTime end, CancellationToken cancellationToken)
    {
        log.LogInformation(
            $"Получение списка результатов по периоду с '{start}' по '{end}' {_mapName}/GetResultForPeriod");
        try
        {
            return Results.Ok(await repository.GetRangeResults(start, end));
        }
        catch (Exception ex)
        {
            var msg = ex.GetAllMessages();
            log.LogError($"Ошибка в {_mapName}/ExecuteExperiment {msg}");
            return Results.InternalServerError(msg);
        }
    }

    private static async Task<IResult> ExecuteExperiment(IRandomExperimentSetRepository repository,
        ILogger<RandomExperimentSet> log, SendExperimentOptionsDto opt, CancellationToken cancellationToken)
    {
        log.LogInformation($"Выполняется расчет {_mapName}/ExecuteExperiment");
        try
        {
            var id = await repository.CalcExperiment(opt);
            log.LogInformation($"Создан эксперимент с id = '{id}'");
            return Results.Ok(id);
        }
        catch (Exception ex)
        {
            var msg = ex.GetAllMessages();
            log.LogError($"Ошибка в {_mapName}/ExecuteExperiment {msg}");
            return Results.InternalServerError(msg);
        }
    }

    private static async Task<IResult> GetExperiment(IRandomExperimentSetRepository repository,
        ILogger<RandomExperimentSet> log, Guid id, CancellationToken cancellationToken)
    {
        log.LogInformation($"Получение записи эксперимента с id - '{id}' {_mapName}/GetExperiment");
        try
        {
            var result =
                await ((MongoBaseRepository<RandomExperimentSet>)repository).GetByIdAsync(id, cancellationToken);
            return Results.Ok(result?.MapToDto());
        }
        catch (Exception ex)
        {
            var msg = ex.GetAllMessages();
            log.LogError($"Ошибка в {_mapName}/GetExperiment {msg}'");
            return Results.InternalServerError(msg);
        }
    }
}
