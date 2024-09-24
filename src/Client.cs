using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Threading;

static class Client
{
    static readonly ApplicationActivationManager manager = new();

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

            if (!Unmanaged.WriteProcessMemory(hProcess, lpBaseAddress, Marshal.StringToHGlobalUni(path), nSize, out _)) throw new Win32Exception(Marshal.GetLastWin32Error());

            hThread = Unmanaged.CreateRemoteThread(hProcess, default, 0, lpStartAddress, lpBaseAddress, 0, out _);
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
        var path = Path.GetFullPath("Client.dll"); SetAccessControl(path); int processId;
        try { manager.ActivateApplication("Microsoft.MinecraftUWP_8wekyb3d8bbwe!App", default, 2, out processId); }
        catch { throw new Win32Exception(Marshal.GetLastWin32Error()); }
        LoadRemoteLibrary(processId, path);
    }
}