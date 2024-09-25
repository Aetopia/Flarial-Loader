using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Threading;
using Windows.Management.Deployment;

static class Client
{
    static readonly PackageManager packageManager = new();

    static readonly ApplicationActivationManager applicationActivationManager = new();

    static readonly PackageDebugSettings packageDebugSettings = new();

    static readonly SecurityIdentifier identifier = new("S-1-15-2-1");

    static readonly nint lpStartAddress;

    static Client()
    {
        var hModule = Unmanaged.LoadLibraryEx("Kernel32.dll", default, Unmanaged.LOAD_LIBRARY_SEARCH_SYSTEM32);
        lpStartAddress = Unmanaged.GetProcAddress(hModule, "LoadLibraryW");
        Unmanaged.FreeLibrary(hModule);
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
            hProcess = Unmanaged.OpenProcess(Unmanaged.PROCESS_ALL_ACCESS, false, processId);
            if (hProcess == default) throw new Win32Exception(Marshal.GetLastWin32Error());

            var nSize = sizeof(char) * (path.Length + 1);

            lpBaseAddress = Unmanaged.VirtualAllocEx(hProcess, default, nSize, Unmanaged.MEM_COMMIT | Unmanaged.MEM_RESERVE, Unmanaged.PAGE_EXECUTE_READWRITE);
            if (lpBaseAddress == default) throw new Win32Exception(Marshal.GetLastWin32Error());

            if (!Unmanaged.WriteProcessMemory(hProcess, lpBaseAddress, Marshal.StringToHGlobalUni(path), nSize)) throw new Win32Exception(Marshal.GetLastWin32Error());

            hThread = Unmanaged.CreateRemoteThread(hProcess, default, 0, lpStartAddress, lpBaseAddress, 0);
            if (hThread == default) throw new Win32Exception(Marshal.GetLastWin32Error());
            Unmanaged.WaitForSingleObject(hThread, Timeout.Infinite);
        }
        finally
        {
            Unmanaged.VirtualFreeEx(hProcess, lpBaseAddress, 0, Unmanaged.MEM_RELEASE);
            Unmanaged.CloseHandle(hThread);
            Unmanaged.CloseHandle(hProcess);
        }
    }

    internal static void Start()
    {
        var path = Path.GetFullPath("Client.dll"); SetAccessControl(path);

        var package = packageManager.FindPackagesForUser(string.Empty, "Microsoft.MinecraftUWP_8wekyb3d8bbwe").FirstOrDefault();
        if (package is null) Marshal.ThrowExceptionForHR(Unmanaged.ERROR_INSTALL_PACKAGE_NOT_FOUND);

        Marshal.ThrowExceptionForHR(packageDebugSettings.EnableDebugging(package.Id.FullName, null, null));
        Marshal.ThrowExceptionForHR(applicationActivationManager.ActivateApplication(package.GetAppListEntries().First().AppUserModelId, null, Unmanaged.AO_NOERRORUI, out var processId));
        LoadRemoteLibrary(processId, path);
    }
}