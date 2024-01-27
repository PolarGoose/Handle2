using App.HandleInfo.Interop;
using Microsoft.Win32.SafeHandles;
using System.Collections.Generic;
using System.Diagnostics;

namespace App.HandleInfo;

internal readonly record struct HandleInfo(
    uint Value,
    string Type,
    string Name,

    // In case the handle has a type "File", this field contains the full path to the file.
    // Otherwise this field is null.
    FileInfo FullNameIfItIsAFileOrAFolder,

    uint GrantedAccess,
    uint Attributes,
    uint AddressInTheKernelMemory);

internal readonly record struct ProcessInfo (
    int Pid,
    FileInfo Executable,
    string UserName,
    string DomainName,
    List<HandleInfo> Handles);

public static class HandleInfoRetriever
{
    public static void GetAllHandles()
    {
        var handles = NtDll.QuerySystemHandleInformation();
        HashSet<UIntPtr> uniquePids = new();
        foreach (var h in handles)
        {
            uniquePids.Add(h.UniqueProcessId);
        }

        Dictionary<UIntPtr, SafeProcessHandle> openedProcesses = new();
        HashSet<UIntPtr> failedToOpenProcesses = new();
        foreach (var pid in uniquePids)
        {
            if (failedToOpenProcesses.Contains(pid))
            {
                continue;
            }

            var proc = WinApi.OpenProcess(WinApi.ProcessAccessRights.PROCESS_DUP_HANDLE, false, pid);
            if (proc.IsInvalid)
            {
                Debug.WriteLine($"Failed to open process {pid}");
                failedToOpenProcesses.Add(pid);
                continue;
            }
            openedProcesses.Add(pid, proc);
        }

        var curProcess = WinApi.GetCurrentProcess();
        foreach (var h in handles)
        {
            if (!openedProcesses.ContainsKey(h.UniqueProcessId))
            {
                continue;
            }

            var openedProcess = openedProcesses[h.UniqueProcessId];
            var res = WinApi.DuplicateHandle(
              openedProcess,
              h.HandleValue,
              curProcess,
              out var dupHandle,
              0,
              false,
              0);

            if (!res)
            {
                Debug.WriteLine($"Failed to duplicate handle");
                continue;
            }

            using (dupHandle)
            {
                Debug.WriteLine($"Successfully duplicated handle");
                Debug.WriteLine(h.Object);
                Debug.WriteLine(NtDll.GetHandleName(dupHandle.DangerousGetHandle()));
                Debug.WriteLine(NtDll.GetHandleType(dupHandle.DangerousGetHandle()));
                Debug.WriteLine($"FinalPath={WinApi.GetFinalPathNameByHandle(dupHandle)}");
            }
        }
    }
}
