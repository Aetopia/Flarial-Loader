namespace Minecraft.UWP;

using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;

enum PackageExecutionState { Unknown, Running, Suspending, Suspended, Terminated }

[ComImport, Guid("F27C3930-8029-4AD1-94E3-3DBA417810C1"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
interface IPackageDebugSettings
{
    int EnableDebugging(string packageFullName, string debuggerCommandLine, string environment);

    int DisableDebugging(string packageFullName);

    int Suspend(string packageFullName);

    int Resume(string packageFullName);

    int TerminateAllProcesses(string packageFullName);

    int SetTargetSessionId(ulong sessionId);

    int EnumerateBackgroundTasks(string packageFullName, nint taskCount, nint taskIds, nint taskNames);

    int ActivateBackgroundTask(nint taskId);

    int StartServicing(string packageFullName);

    int StopServicing(string packageFullName);

    int StartSessionRedirection(string packageFullName, ulong sessionId);

    int StopSessionRedirection(string packageFullName);

    int GetPackageExecutionState(string packageFullName, out PackageExecutionState packageExecutionState);

    int RegisterForPackageStateChanges(string packageFullName, nint pPackageExecutionStateChangeNotification, nint pdwCookie);

    int UnregisterForPackageStateChanges(uint dwCookie);
}

[ComImport, Guid("B1AEC16F-2383-4852-B0E9-8F0B1DC66B4D")]
sealed class PackageDebugSettings : IPackageDebugSettings
{
    [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
    public extern int EnableDebugging(string packageFullName, string debuggerCommandLine, string environment);

    [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
    public extern int DisableDebugging(string packageFullName);

    [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
    public extern int Suspend(string packageFullName);

    [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
    public extern int Resume(string packageFullName);

    [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
    public extern int TerminateAllProcesses(string packageFullName);

    [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
    public extern int SetTargetSessionId(ulong sessionId);

    [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
    public extern int EnumerateBackgroundTasks(string packageFullName, nint taskCount, nint taskIds, nint taskNames);

    [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
    public extern int ActivateBackgroundTask(nint taskId);

    [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
    public extern int StartServicing(string packageFullName);

    [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
    public extern int StopServicing(string packageFullName);

    [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
    public extern int StartSessionRedirection(string packageFullName, ulong sessionId);

    [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
    public extern int StopSessionRedirection(string packageFullName);

    [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
    public extern int GetPackageExecutionState(string packageFullName, out PackageExecutionState packageExecutionState);

    [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
    public extern int RegisterForPackageStateChanges(string packageFullName, nint pPackageExecutionStateChangeNotification, nint pdwCookie);

    [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
    public extern int UnregisterForPackageStateChanges(uint dwCookie);
}