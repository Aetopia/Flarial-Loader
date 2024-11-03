using System;
using System.IO;
using System.Linq;
using Windows.System;
using System.Threading;
using System.ComponentModel;
using Windows.Management.Core;
using Windows.ApplicationModel;
using System.Security.Principal;
using System.Security.AccessControl;
using Windows.Management.Deployment;
using System.Runtime.InteropServices;

static class Client
{
    static readonly PackageManager packageManager = new();

    static readonly ApplicationActivationManager applicationActivationManager = new();

    static readonly PackageDebugSettings packageDebugSettings = new();

    static readonly SecurityIdentifier identifier = new("S-1-15-2-1");

    static readonly nint lpStartAddress;

    static Client()
    {
        var hModule = Native.LoadLibraryEx("Kernel32.dll", default, Native.LOAD_LIBRARY_SEARCH_SYSTEM32);
        lpStartAddress = Native.GetProcAddress(hModule, "LoadLibraryW");
        Native.FreeLibrary(hModule);
    }

    static void SetAccessControl(string path)
    {
        if (File.Exists(path))
        {
            var security = File.GetAccessControl(path);
            security.AddAccessRule(new(identifier, FileSystemRights.ReadAndExecute, AccessControlType.Allow));
            File.SetAccessControl(path, security);
        }
        else throw new FileNotFoundException();
    }

    static void LoadRemoteLibrary(int processId, string path)
    {
        nint hProcess = default, lpBaseAddress = default, hThread = default;
        try
        {
            hProcess = Native.OpenProcess(Native.PROCESS_ALL_ACCESS, false, processId);
            if (hProcess == default) throw new Win32Exception(Marshal.GetLastWin32Error());

            var nSize = sizeof(char) * (path.Length + 1);

            lpBaseAddress = Native.VirtualAllocEx(hProcess, default, nSize, Native.MEM_COMMIT | Native.MEM_RESERVE, Native.PAGE_EXECUTE_READWRITE);
            if (lpBaseAddress == default) throw new Win32Exception(Marshal.GetLastWin32Error());

            if (!Native.WriteProcessMemory(hProcess, lpBaseAddress, Marshal.StringToHGlobalUni(path), nSize)) throw new Win32Exception(Marshal.GetLastWin32Error());

            hThread = Native.CreateRemoteThread(hProcess, default, 0, lpStartAddress, lpBaseAddress, 0);
            if (hThread == default) throw new Win32Exception(Marshal.GetLastWin32Error());
            Native.WaitForSingleObject(hThread, Timeout.Infinite);
        }
        finally
        {
            Native.VirtualFreeEx(hProcess, lpBaseAddress, 0, Native.MEM_RELEASE);
            Native.CloseHandle(hThread);
            Native.CloseHandle(hProcess);
        }
    }

    internal static void Start(string path)
    {
        SetAccessControl(path = Path.GetFullPath(path));

        var package = packageManager.FindPackagesForUser(string.Empty, "Microsoft.MinecraftUWP_8wekyb3d8bbwe").FirstOrDefault();
        if (package is null) Marshal.ThrowExceptionForHR(Native.ERROR_INSTALL_PACKAGE_NOT_FOUND);
        else if (package.Id.Architecture != ProcessorArchitecture.X64) Marshal.ThrowExceptionForHR(Native.ERROR_INSTALL_WRONG_PROCESSOR_ARCHITECTURE);

        LoadRemoteLibrary(package.Activate(), path);
    }

    static int Activate(this Package package)
    {
        Marshal.ThrowExceptionForHR(packageDebugSettings.EnableDebugging(package.Id.FullName, default, default));
        Marshal.ThrowExceptionForHR(packageDebugSettings.GetPackageExecutionState(package.Id.FullName, out var packageExecutionState));
        var path = ApplicationDataManager.CreateForPackageFamily(package.Id.FamilyName).LocalFolder.Path;

        var state = packageExecutionState is not PackageExecutionState.Unknown or PackageExecutionState.Terminated;
        if (state) state = !File.Exists(Path.Combine(path, @"games\com.mojang\minecraftpe\resource_init_lock"));

        using ManualResetEventSlim @event = new(state); using FileSystemWatcher watcher = new(path) { NotifyFilter = NotifyFilters.FileName, IncludeSubdirectories = true, EnableRaisingEvents = true };
        watcher.Deleted += (_, e) => { if (e.Name.Equals(@"games\com.mojang\minecraftpe\resource_init_lock", StringComparison.OrdinalIgnoreCase)) @event.Set(); };

        Marshal.ThrowExceptionForHR(applicationActivationManager.ActivateApplication(package.GetAppListEntries().First().AppUserModelId, null, Native.AO_NOERRORUI, out var processId));
        @event.Wait(); return processId;
    }
}