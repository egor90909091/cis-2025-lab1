using NUnit.Framework;
using Allure.Net.Commons;

[assembly: LevelOfParallelism(1)]

namespace Backend.Tests;

[SetUpFixture]
public class AllureConfig
{
    [OneTimeSetUp]
    public void Setup()
    {
        // Получаем директорию для результатов из переменной окружения или используем дефолтную
        var resultsDirectory = Environment.GetEnvironmentVariable("ALLURE_RESULTS_DIR") 
            ?? Path.Combine(Directory.GetCurrentDirectory(), "allure-results");
        
        // Создаем директорию если не существует
        Directory.CreateDirectory(resultsDirectory);
        
        // Настраиваем Allure
        AllureLifecycle.Instance.ResultsDirectory = resultsDirectory;
        
        Console.WriteLine($"Allure results will be saved to: {resultsDirectory}");
    }

    [OneTimeTearDown]
    public void Teardown()
    {
        AllureLifecycle.Instance.CleanupResultDirectory();
    }
}

