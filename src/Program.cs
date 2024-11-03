using System;
using System.IO;
using System.Threading;
using System.Globalization;

static class Program
{
    [STAThread]
    static void Main()
    {
        using Mutex mutex = new(true, "622D66FB-75AD-47CF-963B-A2C499E9DAF0", out var createdNew); if (!createdNew) return;
        CultureInfo.DefaultThreadCurrentCulture = CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;
        Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory); new Window().ShowDialog();
    }
}