using System.Diagnostics;
using System.Text;
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

        _httpClient.DefaultRequestHeaders.TryAddWithoutValidation(
            "Accept",
            "application/json"
        );
    }

    public async Task RunInitializeAsync()
    {
        const string urlFileName = "urls.txt";
        const string requestFileName = "request.json";

        // ==========================================
        // CHECK FILES
        // ==========================================

        if (!File.Exists(urlFileName))
        {
            Console.WriteLine("ERROR: urls.txt file does not exist.");
            return;
        }

        if (!File.Exists(requestFileName))
        {
            Console.WriteLine("ERROR: request.json file does not exist.");
            return;
        }

        // ==========================================
        // READ URLS
        // ==========================================

        var urls = File.ReadAllLines(urlFileName)
            .Where(url => !string.IsNullOrWhiteSpace(url))
            .Select(url => url.Trim())
            .ToList();

        if (urls.Count == 0)
        {
            Console.WriteLine("ERROR: No URLs found in urls.txt.");
            return;
        }

        // ==========================================
        // READ REQUEST BODY
        // ==========================================

        string requestBody = await File.ReadAllTextAsync(requestFileName);

        // ==========================================
        // VALIDATE JSON
        // ==========================================

        try
        {
            using JsonDocument document =
                JsonDocument.Parse(requestBody);
        }
        catch (JsonException)
        {
            Console.WriteLine(
                "ERROR: request.json contains invalid JSON."
            );

            return;
        }

        // ==========================================
        // HEADER
        // ==========================================

        Console.WriteLine();
        Console.WriteLine("============================================================");
        Console.WriteLine("                  INITIALIZE STARTED");
        Console.WriteLine("============================================================");
        Console.WriteLine();

        Console.WriteLine($"Total URLs found: {urls.Count}");
        Console.WriteLine();

        // ==========================================
        // SEND ALL REQUESTS
        // ==========================================

        var tasks = urls
            .Select(url => ExecuteUrlAsync(url, requestBody))
            .ToList();

        // IMPORTANT:
        // We wait for ALL requests first.
        // Nothing is printed from ExecuteUrlAsync().
        var results = await Task.WhenAll(tasks);

        // ==========================================
        // PRINT RESULTS IN URL ORDER
        // ==========================================

        for (int i = 0; i < results.Length; i++)
        {
            PrintResult(
                results[i],
                i + 1
            );
        }

        // ==========================================
        // FINAL SUMMARY
        // ==========================================

        Console.WriteLine();
        Console.WriteLine("============================================================");
        Console.WriteLine("                     FINAL SUMMARY");
        Console.WriteLine("============================================================");
        Console.WriteLine();

        Console.WriteLine($"Total URLs : {results.Length}");

        int successful = results.Count(x => x.Success);
        int failed = results.Count(x => !x.Success);

        Console.WriteLine($"Successful : {successful}");
        Console.WriteLine($"Failed     : {failed}");

        Console.WriteLine();

        Console.WriteLine("============================================================");
        Console.WriteLine("                  INITIALIZE COMPLETED");
        Console.WriteLine("============================================================");
    }

    // =========================================================
    // EXECUTE ONE URL
    // =========================================================

    private async Task<UrlResult> ExecuteUrlAsync(
        string url,
        string requestBody)
    {
        var stopwatch = Stopwatch.StartNew();

        try
        {
            using var request = new HttpRequestMessage(
                HttpMethod.Post,
                url
            );

            request.Content = new StringContent(
                requestBody,
                Encoding.UTF8,
                "application/json"
            );

            using var response =
                await _httpClient.SendAsync(request);

            string responseBody =
                await response.Content.ReadAsStringAsync();

            stopwatch.Stop();

            return new UrlResult
            {
                Url = url,
                StatusCode = (int)response.StatusCode,
                Status = response.StatusCode.ToString(),
                ResponseTime = stopwatch.ElapsedMilliseconds,
                Success = response.IsSuccessStatusCode,
                ResponseBody = responseBody
            };
        }
        catch (TaskCanceledException)
        {
            stopwatch.Stop();

            return new UrlResult
            {
                Url = url,
                StatusCode = 0,
                Status = "TIMEOUT",
                ResponseTime = stopwatch.ElapsedMilliseconds,
                Success = false,
                ResponseBody = "Request timed out."
            };
        }
        catch (HttpRequestException ex)
        {
            stopwatch.Stop();

            return new UrlResult
            {
                Url = url,
                StatusCode = 0,
                Status = "CONNECTION ERROR",
                ResponseTime = stopwatch.ElapsedMilliseconds,
                Success = false,
                ResponseBody = ex.Message
            };
        }
        catch (Exception ex)
        {
            stopwatch.Stop();

            return new UrlResult
            {
                Url = url,
                StatusCode = 0,
                Status = "ERROR",
                ResponseTime = stopwatch.ElapsedMilliseconds,
                Success = false,
                ResponseBody = ex.Message
            };
        }
    }

    // =========================================================
    // PRINT ONE COMPLETE RESULT
    // =========================================================

    private void PrintResult(
        UrlResult result,
        int number)
    {
        Console.WriteLine();
        Console.WriteLine("============================================================");
        Console.WriteLine($"                        URL #{number}");
        Console.WriteLine("============================================================");

        Console.WriteLine();
        Console.WriteLine($"URL            : {result.Url}");

        Console.WriteLine(
            $"Method         : POST"
        );

        if (result.StatusCode > 0)
        {
            Console.WriteLine(
                $"Status Code    : {result.StatusCode}"
            );
        }
        else
        {
            Console.WriteLine(
                $"Status Code    : N/A"
            );
        }

        Console.WriteLine(
            $"Status         : {result.Status}"
        );

        Console.WriteLine(
            $"Response Time  : {result.ResponseTime} ms"
        );

        Console.WriteLine(
            $"Success        : {result.Success}"
        );

        Console.WriteLine();

        Console.WriteLine(
            "-------------------- RESPONSE --------------------"
        );

        if (!string.IsNullOrWhiteSpace(result.ResponseBody))
        {
            PrintResponse(result.ResponseBody);
        }
        else
        {
            Console.WriteLine(
                "No response body returned."
            );
        }

        Console.WriteLine(
            "---------------------------------------------------"
        );

        Console.WriteLine();
    }

    // =========================================================
    // PRINT RESPONSE
    // =========================================================

    private void PrintResponse(string responseBody)
    {
        try
        {
            using JsonDocument document =
                JsonDocument.Parse(responseBody);

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
            // Response is not JSON
            Console.WriteLine(responseBody);
        }
    }

    // =========================================================
    // RESULT MODEL
    // =========================================================

    private class UrlResult
    {
        public string Url { get; set; } = string.Empty;

        public int StatusCode { get; set; }

        public string Status { get; set; } = string.Empty;

        public long ResponseTime { get; set; }

        public bool Success { get; set; }

        public string ResponseBody { get; set; } = string.Empty;
    }
}