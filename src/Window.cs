using System;
using System.Net;
using System.Windows;
using System.Windows.Interop;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Reflection;
using System.Windows.Media.Imaging;

sealed class Window : System.Windows.Window
{
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
             Native.ShellMessageBox(hWnd: new WindowInteropHelper(this).Handle, lpcText: exception.Message);
             Close();
         };

        using WebClient client = new(); string value = default;

        client.DownloadProgressChanged += (sender, e) =>
        {
            static string _(float _) { var unit = (int)Math.Log(_, 1024); return $"{_ / Math.Pow(1024, unit):0.00} {(Unit)unit}"; }
            Dispatcher.Invoke(() =>
             {
                 if (bar.Value != e.ProgressPercentage)
                 {
                     bar.Value = e.ProgressPercentage;
                     block2.Text = $"Downloading {_(e.BytesReceived)} / {value ??= _(e.TotalBytesToReceive)}";
                 }
             });
        };

        client.DownloadFileCompleted += (sender, e) => value = null;

        ContentRendered += async (_, _) => await Task.Run(() =>
        {
            var (Url, Update) = GitHub.Get("dll/latest.dll", "Flarial.Client.dll");
            if (Update)
            {
                Dispatcher.Invoke(() => bar.IsIndeterminate = false);
                client.DownloadFileTaskAsync(Url, "Flarial.Client.dll").Wait();
                Dispatcher.Invoke(() => { block2.Text = null; bar.IsIndeterminate = true; bar.Value = 0; });
            }
            Dispatcher.Invoke(() => block2.Text = "Waiting...");
            Client.Start("Flarial.Client.dll");
            Dispatcher.Invoke(Close);
        });
    }
}