using Handle2.HandleInfo.Interop;
using Microsoft.Win32.SafeHandles;
using System.Diagnostics;
using System.Security.Principal;

namespace Handle2.HandleInfo.Utils;

public static class ProcessUtils
{
    public static (string domain, string user) GetOwnerDomainAndUserNames(SafeProcessHandle openedProcess)
    {
        if (!WinApi.OpenProcessToken(openedProcess, WinApi.TokenAccessRights.TOKEN_QUERY, out var tokenHandle))
        {
            return ("", "");
        }

        using (tokenHandle)
        {
            using var wi = new WindowsIdentity(tokenHandle.DangerousGetHandle());
            var domainAndUser = wi.Name.Split('\\');
            return (domainAndUser[0], domainAndUser[1]);
        }
    }

    public static string? GetExecutablePath(UIntPtr procId)
    {
        try
        {
            using var process = Process.GetProcessById((int)procId);
            return process.MainModule.FileName;
        }
        catch (Exception)
        {
            return null;
        }
    }

    public static SafeProcessHandle? OpenProcessToDuplicateHandle(UIntPtr pid)
    {
        var p = WinApi.OpenProcess(WinApi.ProcessAccessRights.PROCESS_DUP_HANDLE | WinApi.ProcessAccessRights.PROCESS_QUERY_INFORMATION, false, pid);
        return p.IsInvalid ? null : p;
    }
}
