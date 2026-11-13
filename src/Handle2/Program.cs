using Handle2;
using Handle2.HandleInfo;
using System.Text;
using System.Text.Json;

Console.OutputEncoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);
if (args.Contains("--force-english"))
{
    CommandLineOptions.ApplyEnglishCulture();
}
return CommandLineOptions.CreateCommand(Run).Parse(args.Length == 0 || ( args.Length == 1 && args[0] == "--force-english" ) ? ["--help"] : args).Invoke();

static int Run(CommandLineOptions options)
{
    if (options.DumpAllHandles && options.Json)
    {
        Console.Write(JsonSerializer.Serialize(HandleInfoRetriever.GetAllProcInfos(), HandleInfoJsonContext.Default.IEnumerableProcessInfo));
    }

    if (options.DumpAllHandles && !options.Json)
    {
        foreach (var procWithHandles in HandleInfoRetriever.GetAllProcInfos())
        {
            Console.WriteLine($"{procWithHandles.ProcessExecutablePath} pid: {procWithHandles.Pid} {procWithHandles.DomainName}/{procWithHandles.UserName}");

            foreach (var handle in procWithHandles.Handles)
            {
                if (handle.Name == null)
                {
                    continue;
                }

                var name = handle.FullNameIfItIsAFileOrAFolder ?? handle.Name;
                Console.WriteLine($"  {handle.HandleType,-15} {name}");
            }

            foreach (var moduleName in procWithHandles.ModuleNames)
            {
                Console.WriteLine($"  {"Module",-15} {moduleName}");
            }

            Console.WriteLine("------------------------------------------------------------------------------");
        }
    }

    if (options.Path != null && !options.Json)
    {
        foreach (var procWithHandles in HandleInfoRetriever.GetProcInfosLockingPath(options.Path))
        {
            Console.WriteLine($"{procWithHandles.ProcessExecutablePath} pid: {procWithHandles.Pid} {procWithHandles.DomainName}/{procWithHandles.UserName}");

            foreach (var handle in procWithHandles.Handles)
            {
                Console.WriteLine($"  {handle.FullNameIfItIsAFileOrAFolder}");
            }

            foreach (var moduleName in procWithHandles.ModuleNames)
            {
                Console.WriteLine($"  {moduleName}");
            }

            Console.WriteLine("------------------------------------------------------------------------------");
        }
    }

    if (options.Path != null && options.Json)
    {
        var processesLockingPath = HandleInfoRetriever.GetProcInfosLockingPath(options.Path);
        Console.Write(JsonSerializer.Serialize(processesLockingPath, HandleInfoJsonContext.Default.IEnumerableProcessInfo));
    }

    return 0;
}
