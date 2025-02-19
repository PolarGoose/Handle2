using Handle2.HandleInfo.Interop;
using Microsoft.Win32.SafeHandles;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Text;

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

    public static string? GetProcessExeFullName(SafeProcessHandle processHandle)
    {
        for (var capacity = 2048; ; capacity *= 2)
        {
            var buffer = new StringBuilder(capacity);
            int size = capacity;

            if (!WinApi.QueryFullProcessImageName(processHandle, 0, buffer, ref size))
            {
                int error = Marshal.GetLastWin32Error();
                if (error != WinApi.ERROR_INSUFFICIENT_BUFFER)
                {
                    return null;
                }
                continue;
            }

            return buffer.ToString(0, size);
        }
    }


    public static SafeProcessHandle? OpenProcessToDuplicateHandle(UIntPtr pid)
    {
        var p = WinApi.OpenProcess(WinApi.ProcessAccessRights.PROCESS_DUP_HANDLE | WinApi.ProcessAccessRights.PROCESS_QUERY_INFORMATION, false, pid);
        return p.IsInvalid ? null : p;
    }
}
