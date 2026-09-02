namespace UrlUtility.Services;

public class UrlRunnerService
{
    private readonly HttpClient _httpClient;

    public UrlRunnerService()
    {
        _httpClient = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(30)
        };
    }

    public async Task RunInitializeAsync()
    {
        const string fileName = "urls.txt";

        if (!File.Exists(fileName))
        {
            Console.WriteLine("ERROR: urls.txt file does not exist.");
            return;
        }

        var url = File.ReadAllText(fileName).Trim();

        if (string.IsNullOrWhiteSpace(url))
        {
            Console.WriteLine("ERROR: No URL found in urls.txt.");
            return;
        }

        Console.WriteLine("========================================");
        Console.WriteLine("          INITIALIZE STARTED");
        Console.WriteLine("========================================");
        Console.WriteLine();

        Console.WriteLine($"Calling: {url}");

        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        try
        {
            using var response = await _httpClient.GetAsync(url);

            stopwatch.Stop();

            Console.WriteLine();
            Console.WriteLine($"Status Code   : {(int)response.StatusCode}");
            Console.WriteLine($"Status        : {response.StatusCode}");
            Console.WriteLine($"Response Time : {stopwatch.ElapsedMilliseconds} ms");
            Console.WriteLine($"Success       : {response.IsSuccessStatusCode}");

            Console.WriteLine();
            Console.WriteLine("========================================");
            Console.WriteLine("          INITIALIZE COMPLETED");
            Console.WriteLine("========================================");
        }
        catch (TaskCanceledException)
        {
            stopwatch.Stop();

            Console.WriteLine();
            Console.WriteLine("Request timed out.");
            Console.WriteLine($"Response Time : {stopwatch.ElapsedMilliseconds} ms");
        }
        catch (Exception ex)
        {
            stopwatch.Stop();

            Console.WriteLine();
            Console.WriteLine("Request failed.");
            Console.WriteLine($"Error         : {ex.Message}");
            Console.WriteLine($"Response Time : {stopwatch.ElapsedMilliseconds} ms");
        }
    }
}