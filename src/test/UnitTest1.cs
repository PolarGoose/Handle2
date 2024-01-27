using App.HandleInfo;
using App.HandleInfo.Interop;
using NUnit.Framework;
using System.Diagnostics;

namespace Test;

[TestFixture]
public class Tests
{
    [SetUp]
    public void Setup()
    {
    }

    // [Test]
    // public void Test1()
    // {
    //     var handles = NtDll.QuerySystemHandleInformation();
    //     foreach (var h in handles)
    //     {
    //         using var openedProcess = WinApi.OpenProcess(WinApi.ProcessAccessRights.PROCESS_DUP_HANDLE, false, h.UniqueProcessId);
    //         if (openedProcess.IsInvalid)
    //         {
    //             Debug.WriteLine($"Failed to open process {h.UniqueProcessId}");
    //             continue;
    //         }
    //         Debug.WriteLine($"Successfully opened process {h.UniqueProcessId}");
    // 
    //         var curProcess = WinApi.GetCurrentProcess();
    // 
    //         var res = WinApi.DuplicateHandle(
    //           openedProcess,
    //           h.HandleValue,
    //           curProcess,
    //           out var dupHandle,
    //           0,
    //           false,
    //           0);
    //         if (!res)
    //         {
    //             Debug.WriteLine($"Failed to duplicate handle");
    //             continue;
    //         }
    // 
    //         using (dupHandle)
    //         {
    //             Debug.WriteLine($"Successfully duplicated handle");
    //             Debug.WriteLine(h.Object);
    //             Debug.WriteLine(NtDll.GetHandleName(dupHandle.DangerousGetHandle()));
    //             Debug.WriteLine(NtDll.GetHandleType(dupHandle.DangerousGetHandle()));
    //             Debug.WriteLine($"FinalPath={WinApi.GetFinalPathNameByHandle(dupHandle)}");
    //         }
    //     }
    // }

    [Test]
    public void TestDumpHandles()
    {
        HandleInfoRetriever.GetAllHandles();

    }

    // [Test]
    // public void Test1()
    // {
    //     var handles = NtApiDotNet.NtSystemInfo.GetHandles();
    //     foreach (var h in handles)
    //     {
    //         Console.WriteLine(h.ProcessId);
    //     }
    // }

    // [Test]
    // public void Test2()
    // {
    //     try
    //     {
    //         // Get all system handles
    //         var handles = NtSystemInfo.GetHandles();
    // 
    //         foreach (var handle in handles)
    //         {
    //             try
    //             {
    //                 // Attempt to duplicate the handle to get more information
    //                 using (var obj = NtGeneric.DuplicateFrom(handle))
    //                 {
    //                     Console.WriteLine($"Handle: {handle.Handle}, Type: {obj.NtTypeName}, Name: {obj.FullPath}");
    //                 }
    //             }
    //             catch
    //             {
    //                 // If handle duplication fails, just print basic info
    //                 Console.WriteLine($"Handle: {handle.Handle}, Type: {handle.ObjectType}, ProcessId: {handle.ProcessId}");
    //             }
    //         }
    //     }
    //     catch (Exception ex)
    //     {
    //         Console.WriteLine($"An error occurred: {ex.Message}");
    //     }
    // }
}
