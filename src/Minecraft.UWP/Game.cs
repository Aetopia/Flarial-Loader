namespace Minecraft.UWP;

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
using static Native;

static class Game
{
    static readonly PackageManager PackageManager = new();

    static readonly ApplicationActivationManager ApplicationActivationManager = new();

    static readonly PackageDebugSettings PackageDebugSettings = new();

    static readonly SecurityIdentifier Identifier = new("S-1-15-2-1");

    static readonly nint lpStartAddress;

    static Game()
    {
        nint hModule = default;
        try { hModule = LoadLibraryEx("Kernel32.dll", default, LOAD_LIBRARY_SEARCH_SYSTEM32); lpStartAddress = GetProcAddress(hModule, "LoadLibraryW"); }
        finally { FreeLibrary(hModule); }
    }

    static void LoadLibrary(int processId, string path)
    {
        FileInfo info = new(path = Path.GetFullPath(path)); var security = info.GetAccessControl();
        security.AddAccessRule(new(Identifier, FileSystemRights.ReadAndExecute, AccessControlType.Allow));
        info.SetAccessControl(security);

        nint hProcess = default, lpBaseAddress = default, hThread = default;
        try
        {
            hProcess = OpenProcess(PROCESS_ALL_ACCESS, false, processId);
            if (hProcess == default) throw new Win32Exception(Marshal.GetLastWin32Error());

            var size = sizeof(char) * (path.Length + 1);

            lpBaseAddress = VirtualAllocEx(hProcess, default, size, MEM_COMMIT | MEM_RESERVE, PAGE_EXECUTE_READWRITE);
            if (lpBaseAddress == default) throw new Win32Exception(Marshal.GetLastWin32Error());

            if (!WriteProcessMemory(hProcess, lpBaseAddress, Marshal.StringToHGlobalUni(path), size)) throw new Win32Exception(Marshal.GetLastWin32Error());

            hThread = CreateRemoteThread(hProcess, default, 0, lpStartAddress, lpBaseAddress, 0);
            if (hThread == default) throw new Win32Exception(Marshal.GetLastWin32Error());
            WaitForSingleObject(hThread, Timeout.Infinite);
        }
        finally
        {
            VirtualFreeEx(hProcess, lpBaseAddress, 0, MEM_RELEASE);
            CloseHandle(hThread);
            CloseHandle(hProcess);
        }
    }

    internal static void Launch(string path)
    {
        var package = PackageManager.FindPackagesForUser(string.Empty, "Microsoft.MinecraftUWP_8wekyb3d8bbwe").FirstOrDefault();

        if (package is null) Marshal.ThrowExceptionForHR(ERROR_INSTALL_PACKAGE_NOT_FOUND);
        else if (package.Id.Architecture != RuntimeInformation.OSArchitecture switch
        {
            Architecture.X86 => ProcessorArchitecture.X86,
            Architecture.X64 => ProcessorArchitecture.X64,
            Architecture.Arm => ProcessorArchitecture.Arm,
            Architecture.Arm64 => ProcessorArchitecture.Arm64,
            _ => ProcessorArchitecture.Unknown
        }) Marshal.ThrowExceptionForHR(ERROR_INSTALL_WRONG_PROCESSOR_ARCHITECTURE);

        LoadLibrary(package.Launch(), path);
    }

    static int Launch(this Package source)
    {
        Marshal.ThrowExceptionForHR(PackageDebugSettings.EnableDebugging(source.Id.FullName, default, default));
        Marshal.ThrowExceptionForHR(PackageDebugSettings.GetPackageExecutionState(source.Id.FullName, out var packageExecutionState));
        var path = ApplicationDataManager.CreateForPackageFamily(source.Id.FamilyName).LocalFolder.Path;

        var state = packageExecutionState is not PackageExecutionState.Unknown or PackageExecutionState.Terminated;
        if (state) state = !File.Exists(Path.Combine(path, @"games\com.mojang\minecraftpe\resource_init_lock"));

        using ManualResetEventSlim @event = new(state); using FileSystemWatcher watcher = new(path) { NotifyFilter = NotifyFilters.FileName, IncludeSubdirectories = true, EnableRaisingEvents = true };
        watcher.Deleted += (_, e) => { if (e.Name.Equals(@"games\com.mojang\minecraftpe\resource_init_lock", StringComparison.OrdinalIgnoreCase)) @event.Set(); };

        Marshal.ThrowExceptionForHR(ApplicationActivationManager.ActivateApplication(source.GetAppListEntries().First().AppUserModelId, null, AO_NOERRORUI, out var processId));
        @event.Wait(); return processId;
    }
}