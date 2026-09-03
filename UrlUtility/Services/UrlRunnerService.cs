using System.Diagnostics;
using System.Text.Json;

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

        var urls = File.ReadAllLines(fileName)
            .Where(url => !string.IsNullOrWhiteSpace(url))
            .Select(url => url.Trim())
            .ToList();

        if (urls.Count == 0)
        {
            Console.WriteLine("ERROR: No URLs found in urls.txt.");
            return;
        }

        Console.WriteLine("========================================");
        Console.WriteLine("          INITIALIZE STARTED");
        Console.WriteLine("========================================");
        Console.WriteLine();

        Console.WriteLine($"Total URLs found: {urls.Count}");
        Console.WriteLine();

        // Run all URLs at the same time
        var tasks = urls.Select(ExecuteUrlAsync).ToList();

        // Wait for ALL URLs
        var results = await Task.WhenAll(tasks);

        // ==========================================
        // FINAL SUMMARY TABLE
        // ==========================================

        Console.WriteLine();
        Console.WriteLine();
        Console.WriteLine("==============================================================");
        Console.WriteLine("                     FINAL RESULTS");
        Console.WriteLine("==============================================================");
        Console.WriteLine();

        Console.WriteLine(
            "{0,-8} {1,-10} {2,-12} {3,-10}",
            "SERVER",
            "STATUS",
            "TIME",
            "RESULT"
        );

        Console.WriteLine(
            "--------------------------------------------------------------"
        );

        foreach (var result in results)
        {
            string shortName = GetShortUrlName(result.Url);

            string status = result.StatusCode > 0
                ? result.StatusCode.ToString()
                : result.Status;

            string resultText = result.Success
                ? "SUCCESS"
                : "FAILED";

            Console.WriteLine(
                "{0,-8} {1,-10} {2,-12} {3,-10}",
                shortName,
                status,
                $"{result.ResponseTime} ms",
                resultText
            );
        }

        Console.WriteLine();
        Console.WriteLine(
            "--------------------------------------------------------------"
        );

        int successful = results.Count(x => x.Success);
        int failed = results.Count(x => !x.Success);

        Console.WriteLine($"Total:      {results.Length}");
        Console.WriteLine($"Successful: {successful}");
        Console.WriteLine($"Failed:     {failed}");

        Console.WriteLine();
        Console.WriteLine("==============================================================");
        Console.WriteLine("                 INITIALIZE COMPLETED");
        Console.WriteLine("==============================================================");
    }

    private async Task<UrlResult> ExecuteUrlAsync(string url)
    {
        var stopwatch = Stopwatch.StartNew();

        try
        {
            using var response = await _httpClient.GetAsync(url);

            stopwatch.Stop();

            var responseBody = await response.Content.ReadAsStringAsync();

            var result = new UrlResult
            {
                Url = url,
                StatusCode = (int)response.StatusCode,
                Status = response.StatusCode.ToString(),
                ResponseTime = stopwatch.ElapsedMilliseconds,
                Success = response.IsSuccessStatusCode
            };

            // ==========================================
            // INDIVIDUAL URL RESPONSE
            // ==========================================

            Console.WriteLine();
            Console.WriteLine("========================================");
            Console.WriteLine($"URL: {url}");
            Console.WriteLine("========================================");

            Console.WriteLine();
            Console.WriteLine("------------- INITIALIZE DETAILS -------------");

            Console.WriteLine($"Status Code   : {result.StatusCode}");
            Console.WriteLine($"Status        : {result.Status}");
            Console.WriteLine($"Response Time : {result.ResponseTime} ms");
            Console.WriteLine($"Success       : {result.Success}");

            // If API returned something
            if (!string.IsNullOrWhiteSpace(responseBody))
            {
                Console.WriteLine();

                try
                {
                    using JsonDocument document =
                        JsonDocument.Parse(responseBody);

                    Console.WriteLine("Response Body:");

                    string formattedJson =
                        JsonSerializer.Serialize(
                            document.RootElement,
                            new JsonSerializerOptions
                            {
                                WriteIndented = true
                            }
                        );

                    Console.WriteLine(formattedJson);
                }
                catch (JsonException)
                {
                    Console.WriteLine("Response Body:");
                    Console.WriteLine(responseBody);
                }
            }
            else
            {
                Console.WriteLine();
                Console.WriteLine("Response Body : No response body returned.");
            }

            Console.WriteLine("----------------------------------------------");

            return result;
        }
        catch (TaskCanceledException)
        {
            stopwatch.Stop();

            Console.WriteLine();
            Console.WriteLine("========================================");
            Console.WriteLine($"URL: {url}");
            Console.WriteLine("========================================");

            Console.WriteLine();
            Console.WriteLine("------------- INITIALIZE DETAILS -------------");

            Console.WriteLine("Status Code   : TIMEOUT");
            Console.WriteLine("Status        : Timeout");
            Console.WriteLine(
                $"Response Time : {stopwatch.ElapsedMilliseconds} ms"
            );
            Console.WriteLine("Success       : False");

            Console.WriteLine("----------------------------------------------");

            return new UrlResult
            {
                Url = url,
                StatusCode = 0,
                Status = "TIMEOUT",
                ResponseTime = stopwatch.ElapsedMilliseconds,
                Success = false
            };
        }
        catch (HttpRequestException ex)
        {
            stopwatch.Stop();

            Console.WriteLine();
            Console.WriteLine("========================================");
            Console.WriteLine($"URL: {url}");
            Console.WriteLine("========================================");

            Console.WriteLine();
            Console.WriteLine("------------- INITIALIZE DETAILS -------------");

            Console.WriteLine("Status Code   : CONNECTION ERROR");
            Console.WriteLine("Status        : ConnectionError");
            Console.WriteLine(
                $"Response Time : {stopwatch.ElapsedMilliseconds} ms"
            );
            Console.WriteLine("Success       : False");
            Console.WriteLine($"Error         : {ex.Message}");

            Console.WriteLine("----------------------------------------------");

            return new UrlResult
            {
                Url = url,
                StatusCode = 0,
                Status = "CONNECTION ERROR",
                ResponseTime = stopwatch.ElapsedMilliseconds,
                Success = false
            };
        }
        catch (Exception ex)
        {
            stopwatch.Stop();

            Console.WriteLine();
            Console.WriteLine("========================================");
            Console.WriteLine($"URL: {url}");
            Console.WriteLine("========================================");

            Console.WriteLine();
            Console.WriteLine("------------- INITIALIZE DETAILS -------------");

            Console.WriteLine("Status Code   : ERROR");
            Console.WriteLine("Status        : Error");
            Console.WriteLine(
                $"Response Time : {stopwatch.ElapsedMilliseconds} ms"
            );
            Console.WriteLine("Success       : False");
            Console.WriteLine($"Error         : {ex.Message}");

            Console.WriteLine("----------------------------------------------");

            return new UrlResult
            {
                Url = url,
                StatusCode = 0,
                Status = "ERROR",
                ResponseTime = stopwatch.ElapsedMilliseconds,
                Success = false
            };
        }
    }

    private string GetShortUrlName(string url)
    {
        try
        {
            var uri = new Uri(url);
            string host = uri.Host;

            if (host.StartsWith("mwsa."))
                return "mwsa";

            if (host.StartsWith("ms."))
                return "ms";

            if (host.StartsWith("mwsb."))
                return "mwsb";

            if (host.StartsWith("mwsc."))
                return "mwsc";

            if (host.StartsWith("mwsf."))
                return "mwsf";

            if (host.StartsWith("mst."))
                return "mst";

            return host;
        }
        catch
        {
            return url;
        }
    }

    private class UrlResult
    {
        public string Url { get; set; } = string.Empty;

        public int StatusCode { get; set; }

        public string Status { get; set; } = string.Empty;

        public long ResponseTime { get; set; }

        public bool Success { get; set; }
    }
}
