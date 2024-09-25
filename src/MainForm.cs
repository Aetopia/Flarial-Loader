using System;
using System.Net;
using System.Drawing;
using System.Windows.Forms;
using System.Threading.Tasks;

class MainForm : Form
{
    enum Unit { B, KB, MB, GB }

    internal MainForm()
    {
        AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
        {
            var exception = (Exception)e.ExceptionObject;
            while (exception.InnerException != null) exception = exception.InnerException;
            Unmanaged.ShellMessageBox(hWnd: Handle, lpcText: exception.Message, lpcTitle: Text);
            Environment.Exit(0);
        };

        Font = new("MS Shell Dlg 2", 8);
        Text = "Flarial Loader";
        MinimizeBox = MaximizeBox = false;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        ClientSize = LogicalToDeviceUnits(new Size(380, 115));
        StartPosition = FormStartPosition.CenterScreen;

        Label label1 = new()
        {
            Text = "Preparing Flarial Client...",
            Width = LogicalToDeviceUnits(359),
            Height = LogicalToDeviceUnits(13),
            Location = new(LogicalToDeviceUnits(9), LogicalToDeviceUnits(23)),
            Margin = default
        };
        Controls.Add(label1);

        ProgressBar progressBar = new()
        {
            Width = LogicalToDeviceUnits(359),
            Height = LogicalToDeviceUnits(23),
            Location = new(LogicalToDeviceUnits(11), LogicalToDeviceUnits(46)),
            Margin = default,
            MarqueeAnimationSpeed = 30,
            Style = ProgressBarStyle.Marquee
        };
        Controls.Add(progressBar);

        Label label2 = new()
        {
            Text = null,
            Width = LogicalToDeviceUnits(275),
            Height = LogicalToDeviceUnits(13),
            Location = new(label1.Location.X, LogicalToDeviceUnits(80)),
            Margin = default
        };
        Controls.Add(label2);

        Button button = new()
        {
            Text = "Cancel",
            Width = LogicalToDeviceUnits(75),
            Height = LogicalToDeviceUnits(23),
            Location = new(LogicalToDeviceUnits(294), LogicalToDeviceUnits(81)),
            Margin = default
        };
        button.Click += (sender, e) => Close();
        Controls.Add(button);

        using WebClient client = new(); string value = default;

        client.DownloadProgressChanged += (sender, e) =>
        {
            static string _(float _) { var unit = (int)Math.Log(_, 1024); return $"{_ / Math.Pow(1024, unit):0.00} {(Unit)unit}"; }
            Invoke(() =>
            {
                if (progressBar.Value != e.ProgressPercentage)
                {
                    progressBar.Value = e.ProgressPercentage;
                    label2.Text = $"Downloading {_(e.BytesReceived)} / {value ??= _(e.TotalBytesToReceive)}";
                }
            });
        };

        client.DownloadFileCompleted += (sender, e) => value = null; ;

        Shown += async (sender, e) => await Task.Run(() =>
        {
            var content = GitHub.Get("dll/latest.dll", "Client.dll");
            if (content.Update)
            {
                Invoke(() => progressBar.Style = ProgressBarStyle.Blocks);
                client.DownloadFileTaskAsync(content.Url, "Client.dll").Wait();
                Invoke(() => { label2.Text = null; progressBar.Style = ProgressBarStyle.Marquee; progressBar.Value = 0; });
            }
            Client.Start();
            Close();
        });
    }
}