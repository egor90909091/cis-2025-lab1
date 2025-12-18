using NUnit.Framework;

[assembly: LevelOfParallelism(1)]

namespace Backend.Tests;

[SetUpFixture]
public class AllureConfig
{
    [OneTimeSetUp]
    public void Setup()
    {
        // Allure.NUnit автоматически читает ALLURE_RESULTS_DIR из переменной окружения
        // или использует allureConfig.json
        var resultsDir = Environment.GetEnvironmentVariable("ALLURE_RESULTS_DIR") 
            ?? Path.Combine(Directory.GetCurrentDirectory(), "allure-results");
        
        Console.WriteLine($"[Allure Config] Working directory: {Directory.GetCurrentDirectory()}");
        Console.WriteLine($"[Allure Config] ALLURE_RESULTS_DIR: {Environment.GetEnvironmentVariable("ALLURE_RESULTS_DIR")}");
        Console.WriteLine($"[Allure Config] Target results directory: {resultsDir}");
        
        // Создаем директорию если не существует
        Directory.CreateDirectory(resultsDir);
    }

    [OneTimeTearDown]
    public void Teardown()
    {
        Console.WriteLine("[Allure Config] Tests completed");
    }
}

