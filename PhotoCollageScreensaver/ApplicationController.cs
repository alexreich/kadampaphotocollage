using PhotoCollageScreensaver.Logging;
using PhotoCollageScreensaver.Data;
using PhotoCollageScreensaver.ViewModels;
using PhotoCollageScreensaver.Views;
using System.IO;
using System.Linq;
using PhotoCollage.Common;

namespace PhotoCollageScreensaver;

public class ApplicationController
{
    private readonly CollageSettings configuration;
    private readonly ISettingsRepository configurationRepository;
    private readonly ILogger logger;
    private CollagePresenter collagePresenter;

    public ApplicationController()
    {
        // Default settings directory (uses user's Pictures folder)
        var defaultDataDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyPictures), "");

        // Allow overriding the configuration directory via environment variable or command-line
        // Use either the PHOTO_COLLAGE_CONFIG_DIR env var or the --config-dir="path" argument
        string configDir = Environment.GetEnvironmentVariable("PHOTO_COLLAGE_CONFIG_DIR");
        if (string.IsNullOrEmpty(configDir))
        {
            var args = Environment.GetCommandLineArgs();
            // look for --config-dir=...
            var arg = args.Skip(1).FirstOrDefault(a => a.StartsWith("--config-dir="));
            if (arg != null)
            {
                configDir = arg.Substring("--config-dir=".Length).Trim('"');
            }
            else
            {
                // look for --config-dir <path>
                for (int i = 1; i < args.Length - 1; i++)
                {
                    if (args[i] == "--config-dir")
                    {
                        configDir = args[i + 1].Trim('"');
                        break;
                    }
                }
            }
        }

        var localDataDirectory = !string.IsNullOrEmpty(configDir) && Directory.Exists(configDir)
            ? configDir
            : defaultDataDirectory;

        // Allow specifying an alternate configuration filename in the same folder
        // Use either the PHOTO_COLLAGE_CONFIG_FILE env var or the --config-file="name" argument
        string configFile = Environment.GetEnvironmentVariable("PHOTO_COLLAGE_CONFIG_FILE");
        if (string.IsNullOrEmpty(configFile))
        {
            var args2 = Environment.GetCommandLineArgs();
            var argFile = args2.Skip(1).FirstOrDefault(a => a.StartsWith("--config-file="));
            if (argFile != null)
            {
                configFile = argFile.Substring("--config-file=".Length).Trim('"');
            }
            else
            {
                for (int i = 1; i < args2.Length - 1; i++)
                {
                    if (args2[i] == "--config-file")
                    {
                        configFile = args2[i + 1].Trim('"');
                        break;
                    }
                }
            }
        }

        string configFileName = "photo-collage.config";
        string configFolderToUse = localDataDirectory;
        if (!string.IsNullOrEmpty(configFile))
        {
            if (Path.IsPathRooted(configFile))
            {
                // full path provided
                configFolderToUse = Path.GetDirectoryName(configFile);
                configFileName = Path.GetFileName(configFile);
            }
            else
            {
                // filename only - keep the previously determined folder
                configFileName = configFile;
            }
        }

        this.logger = new TextLogger(configFolderToUse);
        this.configurationRepository = new FileSystemSettingsRepository(configFolderToUse, configFileName);
        this.configuration = this.configurationRepository.Load();
        
        // Check for --ignore-mouse-movements argument
        var cmdLineArgs = Environment.GetCommandLineArgs();
        if (cmdLineArgs.Skip(1).Any(a => a.Equals("--ignore-mouse-movements", StringComparison.OrdinalIgnoreCase)))
        {
            this.configuration.IgnoreMouseMovements = true;
        }
    }

    public void StartScreensaver(bool silenceEnabled)
    {
        this.configuration.SilenceEnabled = silenceEnabled;
        var collagePresenter = this.collagePresenter ??= new CollagePresenter(this, this.configuration);
        int count = 0;
        var screens = Monitors.Monitor.GetScreens();
        foreach (var screen in Monitors.Monitor.GetScreens())
        {
            //if (!screen.IsPrimary)
            //{
                var collageWindow = new CollageWindow(this, this.configuration.IgnoreMouseMovements);
                collagePresenter.SetupWindow(collageWindow, screen);
            //}
            count++;
        }

        collagePresenter.StartAnimation();
    }

    public void StartSetup() => new SetupWindow(this).Show();

    public SetupViewModel MakeSetupViewModel() => new SetupViewModel(this.configuration, this);

    public void SaveConfiguration() => this.configurationRepository.Save(this.configuration);

    public void Shutdown() => Application.Current.Shutdown();

    public void HandleError(Exception exception, bool showMessage = false)
    {
        this.LogMessage(exception);
        if (showMessage)
        {
            this.DisplayErrorMessage(exception.Message);
        }
    }

    public void DisplayErrorMessage(string message) => _ = MessageBox.Show(message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);

    private void LogMessage(Exception exception)
    {
        if (this.configuration.UseVerboseLogging)
        {
            this.logger.Log(exception.Message, exception.StackTrace);
        }
        else
        {
            this.logger.Log(exception.Message);
        }
    }
}
