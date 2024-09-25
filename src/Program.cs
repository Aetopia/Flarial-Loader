using System;
using System.Collections.Specialized;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Windows.Forms;

static class Program
{
    static void Main()
    {
        CultureInfo.DefaultThreadCurrentCulture = CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;
        using Mutex mutex = new(true, "622D66FB-75AD-47CF-963B-A2C499E9DAF0", out var createdNew); if (!createdNew) return;
        Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);

        ((NameValueCollection)ConfigurationManager.GetSection("System.Windows.Forms.ApplicationConfigurationSection"))["DpiAwareness"] = "PerMonitorV2";
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        Application.SetUnhandledExceptionMode(UnhandledExceptionMode.ThrowException);
        Application.Run(new MainForm());
    }
}