using System.Text;

namespace UrlUtility.Services;

public class LoggerService
{
    // =====================================================
    // LOG FILE PATH
    // =====================================================

    private readonly string _logFilePath;

    // =====================================================
    // CONSTRUCTOR
    // =====================================================

    public LoggerService()
    {
        string projectFolder = Directory.GetParent(
            AppContext.BaseDirectory
        )!
            .Parent!
            .Parent!
            .FullName;

        string logsFolder = Path.Combine(
            projectFolder,
            "Logs"
        );

       
        Directory.CreateDirectory(logsFolder);

        _logFilePath = Path.Combine(
            logsFolder,
            "UrlUtilityLog.txt"
        );
    }

    // =====================================================
    // LOG OPERATION
    // =====================================================

    public async Task LogOperationAsync(
        List<UrlLogResult> results)
    {
        var logBuilder = new StringBuilder();

        // =====================================================
        // NEW OPERATION
        // =====================================================

        logBuilder.AppendLine();
        logBuilder.AppendLine(
            "============================================================"
        );

        logBuilder.AppendLine(
            "              URL UTILITY OPERATION LOG"
        );

        logBuilder.AppendLine(
            "============================================================"
        );

        logBuilder.AppendLine();

        // =====================================================
        // OPERATION DATE & TIME
        // =====================================================

        logBuilder.AppendLine(
            $"Operation Date : {DateTime.Now:dd-MM-yyyy}"
        );

        logBuilder.AppendLine(
            $"Operation Time : {DateTime.Now:hh:mm:ss tt}"
        );

        logBuilder.AppendLine();

        // =====================================================
        // EACH URL RESULT
        // =====================================================

        for (int i = 0; i < results.Count; i++)
        {
            var result = results[i];

            logBuilder.AppendLine(
                "------------------------------------------------------------"
            );

            logBuilder.AppendLine(
                $"URL #{i + 1}"
            );

            logBuilder.AppendLine();

            logBuilder.AppendLine(
                $"Date          : {result.Date:dd-MM-yyyy}"
            );

            logBuilder.AppendLine(
                $"Time          : {result.Date:hh:mm:ss tt}"
            );

            logBuilder.AppendLine(
                $"URL           : {result.Url}"
            );

            logBuilder.AppendLine(
                $"Method        : {result.Method}"
            );

            logBuilder.AppendLine(
                $"Status Code   : {(result.StatusCode > 0
                    ? result.StatusCode.ToString()
                    : "N/A")}"
            );

            logBuilder.AppendLine(
                $"Status        : {result.Status}"
            );

            logBuilder.AppendLine(
                $"Response Time : {result.ResponseTime} ms"
            );

            logBuilder.AppendLine(
                $"Success       : {result.Success}"
            );

            logBuilder.AppendLine();
        }

        // =====================================================
        // FINAL SUMMARY
        // =====================================================

        int successful =
            results.Count(x => x.Success);

        int failed =
            results.Count(x => !x.Success);

        logBuilder.AppendLine(
            "============================================================"
        );

        logBuilder.AppendLine(
            "                     FINAL SUMMARY"
        );

        logBuilder.AppendLine(
            "============================================================"
        );

        logBuilder.AppendLine();

        logBuilder.AppendLine(
            $"Total URLs : {results.Count}"
        );

        logBuilder.AppendLine(
            $"Successful : {successful}"
        );

        logBuilder.AppendLine(
            $"Failed     : {failed}"
        );

        logBuilder.AppendLine();

        logBuilder.AppendLine(
            $"Completed  : {DateTime.Now:dd-MM-yyyy hh:mm:ss tt}"
        );

        logBuilder.AppendLine();

        logBuilder.AppendLine(
            "============================================================"
        );

        logBuilder.AppendLine();

        // =====================================================
        // APPEND TO SAME LOG FILE
        // =====================================================

      

        await File.AppendAllTextAsync(
            _logFilePath,
            logBuilder.ToString()
        );

        // =====================================================
        // CONSOLE MESSAGE
        // =====================================================

        Console.WriteLine();

        Console.WriteLine(
            $"Log file saved at: {_logFilePath}"
        );

        Console.WriteLine();
    }

    // =========================================================
    // LOG RESULT MODEL
    // =========================================================

    public class UrlLogResult
    {
        public DateTime Date { get; set; }

        public string Url { get; set; } = string.Empty;

        public string Method { get; set; } = string.Empty;

        public int StatusCode { get; set; }

        public string Status { get; set; } = string.Empty;

        public long ResponseTime { get; set; }

        public bool Success { get; set; }
    }
}