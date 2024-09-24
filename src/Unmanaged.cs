using System.Security;
using System.Runtime.InteropServices;

static class Unmanaged
{
    internal const int LOAD_LIBRARY_SEARCH_SYSTEM32 = 0x00000800;

    internal const int MEM_RELEASE = 0x00008000;

    internal const int PROCESS_ALL_ACCESS = 0X1FFFFF;

    internal const int MEM_COMMIT = 0x00001000;

    internal const int MEM_RESERVE = 0x00002000;

    internal const int PAGE_EXECUTE_READWRITE = 0x40;

    [DllImport("Kernel32", SetLastError = true), DefaultDllImportSearchPaths(DllImportSearchPath.System32), SuppressUnmanagedCodeSecurity]
    internal static extern nint OpenProcess(int dwDesiredAccess, bool bInheritHandle, int dwProcessId);

    [DllImport("Kernel32", SetLastError = true), DefaultDllImportSearchPaths(DllImportSearchPath.System32), SuppressUnmanagedCodeSecurity]
    internal static extern int WaitForSingleObject(nint hHandle, int dwMilliseconds);

    [DllImport("Kernel32", SetLastError = true), DefaultDllImportSearchPaths(DllImportSearchPath.System32), SuppressUnmanagedCodeSecurity]
    internal static extern bool CloseHandle(nint hObject);

    [DllImport("Kernel32", SetLastError = true), DefaultDllImportSearchPaths(DllImportSearchPath.System32), SuppressUnmanagedCodeSecurity]
    internal static extern nint VirtualAllocEx(nint hProcess, nint lpAddress, int dwSize, int flAllocationType, int flProtect);

    [DllImport("Kernel32", SetLastError = true), DefaultDllImportSearchPaths(DllImportSearchPath.System32), SuppressUnmanagedCodeSecurity]
    internal static extern bool WriteProcessMemory(nint hProcess, nint lpBaseAddress, nint lpBuffer, int nSize, out int lpNumberOfBytesWritten);

    [DllImport("Kernel32", SetLastError = true), DefaultDllImportSearchPaths(DllImportSearchPath.System32), SuppressUnmanagedCodeSecurity]
    internal static extern nint CreateRemoteThread(nint hProcess, nint lpThreadAttributes, int dwStackSize, nint lpStartAddress, nint lpParameter, int dwCreationFlags, out int lpThreadId);

    [DllImport("Kernel32", CharSet = CharSet.Auto, SetLastError = true), DefaultDllImportSearchPaths(DllImportSearchPath.System32), SuppressUnmanagedCodeSecurity]
    internal static extern nint LoadLibraryEx(string lpLibFileName, nint hFile, int dwFlags);

    [DllImport("Kernel32", SetLastError = true), DefaultDllImportSearchPaths(DllImportSearchPath.System32), SuppressUnmanagedCodeSecurity]
    internal static extern bool FreeLibrary(nint hLibModule);

    [DllImport("Kernel32", SetLastError = true), DefaultDllImportSearchPaths(DllImportSearchPath.System32), SuppressUnmanagedCodeSecurity]
    internal static extern nint GetProcAddress(nint hModule, string lpProcName);

    [DllImport("Kernel32", SetLastError = true), DefaultDllImportSearchPaths(DllImportSearchPath.System32), SuppressUnmanagedCodeSecurity]
    internal static extern bool VirtualFreeEx(nint hProcess, nint lpAddress, int dwSize, int dwFreeType);

    [DllImport("Shell32", CharSet = CharSet.Auto, SetLastError = true), DefaultDllImportSearchPaths(DllImportSearchPath.System32), SuppressUnmanagedCodeSecurity]
 internal   static extern int ShellMessageBox(nint hAppInst = default, nint hWnd = default, string lpcText = default, string lpcTitle = "Flarial Client", int fuStyle = 0x00000010);
}