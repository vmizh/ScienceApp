using ScientificApp.RandomHistSerice.Model;

namespace ScientificApp.RandomHistService.CalcExperiment;

public static class RandomCalc
{
    private static Random random = new Random();
    public static RandomExperimentSet Calc(string appName, int countCalc, int minVal = 1, int maxVal=1000)
    {
        //var ret = new RandomExperimentSet
        var calcRes = new List<int>();
        var start = DateTime.Now;
        for (var i = 0; i < countCalc; i++)
        {
            calcRes.Add(random.Next(minVal,maxVal));
        }

        return new RandomExperimentSet
        {
            AppName = appName,
            Start = start,
            End = DateTime.Now,
            MinValue = minVal,
            MaxValue = maxVal,
            Result = calcRes,
            Id = Guid.CreateVersion7()
        };


    }
}
