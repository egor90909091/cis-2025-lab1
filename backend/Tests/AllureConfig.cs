using NUnit.Framework;

[assembly: LevelOfParallelism(1)]

namespace Backend.Tests;

[SetUpFixture]
public class AllureConfig
{
    [OneTimeSetUp]
    public void Setup()
    {
        // Allure.NUnit использует переменную ALLURE_RESULTS_DIRECTORY (не DIR!)
        var resultsDir = Environment.GetEnvironmentVariable("ALLURE_RESULTS_DIRECTORY") 
            ?? Environment.GetEnvironmentVariable("ALLURE_RESULTS_DIR")
            ?? Path.Combine(Directory.GetCurrentDirectory(), "allure-results");
        
        Console.WriteLine($"[Allure Config] Working directory: {Directory.GetCurrentDirectory()}");
        Console.WriteLine($"[Allure Config] ALLURE_RESULTS_DIRECTORY: {Environment.GetEnvironmentVariable("ALLURE_RESULTS_DIRECTORY")}");
        Console.WriteLine($"[Allure Config] ALLURE_RESULTS_DIR: {Environment.GetEnvironmentVariable("ALLURE_RESULTS_DIR")}");
        Console.WriteLine($"[Allure Config] Target results directory: {resultsDir}");
        
        // Явно устанавливаем переменную если она не установлена
        if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("ALLURE_RESULTS_DIRECTORY")))
        {
            Environment.SetEnvironmentVariable("ALLURE_RESULTS_DIRECTORY", resultsDir);
            Console.WriteLine($"[Allure Config] Set ALLURE_RESULTS_DIRECTORY to: {resultsDir}");
        }
        
        // Создаем директорию если не существует
        Directory.CreateDirectory(resultsDir);
    }

    [OneTimeTearDown]
    public void Teardown()
    {
        Console.WriteLine("[Allure Config] Tests completed");
    }
}

