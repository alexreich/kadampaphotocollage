using System.Windows.Threading;

namespace PhotoCollageScreensaver;

public partial class App : Application
{
    private ApplicationController controller;

    private void App_OnStartup(object sender, StartupEventArgs e)
    {
        var commandArg = string.Empty;
        if (e.Args.Length > 0)
        {
            // Only extract the short command (/s, /v, /p, /c) from the first argument
            if (e.Args[0].StartsWith("/"))
            {
                commandArg = e.Args[0].ToLower().Trim().Substring(0, 2);
            }
            // Ignore other arguments like --config-file, they are handled in ApplicationController
        }

        this.controller = new ApplicationController();
        switch (commandArg)
        {
            case "/p": // preview
                this.Shutdown();
                break;
            case "/v": // screensaver with silence
                this.controller.StartScreensaver(true);
                break;
            case "/s": // screensaver without silence
                this.controller.StartScreensaver(false);
                break;
            default: // no argument or /c both show config
                this.controller.StartSetup();
                break;
        }
    }

    private void App_OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        this.controller.HandleError(e.Exception);
        e.Handled = true;
    }
}
