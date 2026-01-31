using System.IO;

namespace PhotoCollageScreensaver.Logging;

public class TextLogger : ILogger
{
    private readonly string directory;

    public TextLogger(string directoryPath)
    {
        this.directory = Path.Combine(directoryPath, @"logs");
        EnsureDirectoryExists();
    }

    public void Log(string message)
    {
        EnsureDirectoryExists();
        var fullPath = this.FullFilePath;
        File.AppendAllText(fullPath, this.GetLogEntry(message));
    }

    public void Log(string message, string stackTrace)
    {
        EnsureDirectoryExists();
        var fullPath = this.FullFilePath;
        var lines = new List<string>()
            {
                this.GetLogEntry(message),
                "Stack Trace:",
                stackTrace
            };
        File.AppendAllLines(fullPath, lines);
    }

    private void EnsureDirectoryExists()
    {
        if (!Directory.Exists(this.directory))
        {
            Directory.CreateDirectory(this.directory);
        }
    }

    private string GetLogEntry(string message)
    {
        var date = DateTime.Now.ToString();
        return string.Concat(date, "  ==>  ", message);
    }

    private string GetFileName() => "log-" + DateTime.Today.ToString("yyyy-MM-dd") + ".txt";

    private string FullFilePath => Path.Combine(this.directory, this.GetFileName());
}
