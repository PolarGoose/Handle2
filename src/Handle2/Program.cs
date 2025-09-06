using CommandLine;
using Handle2.HandleInfo;
using Handle2.HandleInfo.Utils;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

Console.OutputEncoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);
var jsonSerializationOptions = new JsonSerializerOptions
{
    WriteIndented = true,
    Converters = { new JsonStringEnumConverter() }
};

if (args.Length == 0)
    args = ["--help"];

string? errorMessage = null;

Parser.Default.ParseArguments<Options>(args)
    .WithParsed(o =>
    {
        if (o.DumpAllHandles && o.Json)
        {
            Console.Write(JsonSerializer.Serialize(HandleInfoRetriever.GetAllProcInfos(), jsonSerializationOptions));
        }

        if (o.DumpAllHandles && !o.Json)
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

        if (o.Path != null && !FileUtils.Exists(o.Path))
        {
            errorMessage = $"Error: {o.Path} does not exist.";
            return;
        }

        if (o.Path != null && !o.Json)
        {
            foreach (var procWithHandles in HandleInfoRetriever.GetProcInfosLockingPath(o.Path))
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

        if (o.Path != null && o.Json)
        {
            var processesLockingPath = HandleInfoRetriever.GetProcInfosLockingPath(o.Path);
            Console.Write(JsonSerializer.Serialize(processesLockingPath, jsonSerializationOptions));
        }
    });

if (errorMessage != null)
{
    Console.WriteLine(errorMessage);
    return 1;
}

return 0;

public sealed class Options
{
    [Option("json", Required = false, Default = false,
        HelpText = "JSON output. For details on the meanings of the fields provided, please consult the HandleInfo and SYSTEM_HANDLE_TABLE_ENTRY_INFO_EX structures in the source code.")]
    public bool Json { get; set; }

    [Option("path", Required = true, SetName = "path", HelpText = "Displays the processes locking the path")]
    public string? Path { get; set; }

    [Option("dump-all-handles", Required = true, Default = false, SetName = "dump-all-handles", HelpText = "Displays information about all system handles and modules")]
    public bool DumpAllHandles { get; set; }
}
