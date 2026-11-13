using Handle2.HandleInfo.Utils;
using System.CommandLine;
using System.Globalization;

namespace Handle2;

internal sealed class CommandLineOptions
{
    public bool Json { get; private init; }
    public string? Path { get; private init; }
    public bool DumpAllHandles { get; private init; }
    public bool ForceEnglish { get; private init; }

    public static RootCommand CreateCommand(Func<CommandLineOptions, int> action)
    {
        const string operationError = "Specify exactly one of '--path' or '--dump-all-handles'.";

        var jsonOption = new Option<bool>("--json")
        {
            Description = "JSON output. See the HandleInfo and SYSTEM_HANDLE_TABLE_ENTRY_INFO_EX structures in the source code for field details."
        };
        var pathOption = new Option<string?>("--path")
        {
            Description = "Displays the processes locking a file or folder."
        };
        var dumpAllHandlesOption = new Option<bool>("--dump-all-handles")
        {
            Description = "Displays information about all system handles and modules."
        };
        var forceEnglishOption = new Option<bool>("--force-english")
        {
            Description = "Forces English output instead of the localized system language."
        };

        pathOption.Validators.Add(result =>
        {
            var path = result.GetValue(pathOption);
            if (string.IsNullOrWhiteSpace(path))
            {
                result.AddError("Option '--path' requires a file or folder path.");
            }
            else if (!FileUtils.Exists(path))
            {
                result.AddError($"Path '{path}' does not exist.");
            }
        });
        dumpAllHandlesOption.Validators.Add(result =>
        {
            if ((result.GetResult(pathOption) is not null) == result.GetValue(dumpAllHandlesOption))
            {
                result.AddError(operationError);
            }
        });

        var command = new RootCommand("""
            A console utility that displays information about system handles and identifies the processes locking a specific file or folder.
            https://github.com/PolarGoose/Handle2
            """)
        {
            Options = { jsonOption, pathOption, dumpAllHandlesOption, forceEnglishOption }
        };
        command.Validators.Add(result =>
        {
            if (result.GetResult(pathOption) is null && result.GetResult(dumpAllHandlesOption) is null)
            {
                result.AddError(operationError);
            }
        });
        command.SetAction(result =>
        {
            var options = new CommandLineOptions
            {
                Json = result.GetValue(jsonOption),
                Path = result.GetValue(pathOption),
                DumpAllHandles = result.GetValue(dumpAllHandlesOption),
                ForceEnglish = result.GetValue(forceEnglishOption)
            };

            if (options.ForceEnglish)
            {
                ApplyEnglishCulture();
            }

            return action(options);
        });

        return command;
    }

    public static void ApplyEnglishCulture()
    {
        var english = CultureInfo.GetCultureInfo("en-US");
        CultureInfo.DefaultThreadCurrentCulture = english;
        CultureInfo.DefaultThreadCurrentUICulture = english;
        CultureInfo.CurrentCulture = english;
        CultureInfo.CurrentUICulture = english;
    }
}
