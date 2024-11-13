using Minecraft;
using System.Net;
using System.Windows;
using System.Windows.Interop;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Reflection;
using System.Windows.Media.Imaging;
using System.Runtime.InteropServices;

sealed class Window : System.Windows.Window
{
    [DllImport("Shell32", CharSet = CharSet.Auto, SetLastError = true), DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    internal static extern int ShellMessageBox(nint hAppInst = default, nint hWnd = default, string lpcText = default, string lpcTitle = default, int fuStyle = 0x00000010);

    enum Unit { B, KB, MB, GB }

    internal Window()
    {
        using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(".ico");
        Icon = BitmapFrame.Create(stream, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.OnLoad);
        UseLayoutRounding = true;
        Title = "Flarial Loader";
        ResizeMode = ResizeMode.NoResize;
        WindowStartupLocation = WindowStartupLocation.CenterScreen;
        SizeToContent = SizeToContent.WidthAndHeight;

        Canvas canvas = new() { Width = 381, Height = 115 }; Content = canvas;

        TextBlock block1 = new() { Text = "Updating Flarial Client..." };
        canvas.Children.Add(block1); Canvas.SetLeft(block1, 11); Canvas.SetTop(block1, 15);

        TextBlock block2 = new() { Text = "Preparing..." };
        canvas.Children.Add(block2); Canvas.SetLeft(block2, 11); Canvas.SetTop(block2, 84);

        ProgressBar bar = new() { Width = 359, Height = 23, IsIndeterminate = true, };
        canvas.Children.Add(bar); Canvas.SetLeft(bar, 11); Canvas.SetTop(bar, 46);

        Dispatcher.UnhandledException += (_, e) =>
         {
             e.Handled = true; var exception = e.Exception;
             while (exception.InnerException is not null) exception = exception.InnerException;
             ShellMessageBox(hWnd: new WindowInteropHelper(this).Handle, lpcTitle: "Flarial Loader", lpcText: exception.Message);
             Close();
         };

        using WebClient client = new();
        client.DownloadProgressChanged += (sender, e) => Dispatcher.Invoke(() =>
        {
            if (bar.Value != e.ProgressPercentage)
                bar.Value = e.ProgressPercentage;
        });

        ContentRendered += async (_, _) => await Task.Run(() =>
        {
            var (Url, Update) = GitHub.Get("dll/latest.dll", "Flarial.Client.dll"); if (Update)
            {
                Dispatcher.Invoke(() => { block2.Text = "Downloading..."; bar.IsIndeterminate = false; });
                client.DownloadFileTaskAsync(Url, "Flarial.Client.dll").GetAwaiter().GetResult();
                Dispatcher.Invoke(() => { block2.Text = null; bar.IsIndeterminate = true; bar.Value = 0; });
            }
            Dispatcher.Invoke(() => block2.Text = "Waiting...");
            Injector.Inject(Game.Launch(), "Flarial.Client.dll");
            Dispatcher.Invoke(Close);
        });
    }
}