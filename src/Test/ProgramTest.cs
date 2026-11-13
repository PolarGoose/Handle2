using CliWrap;
using CliWrap.Buffered;
using Handle2;
using NUnit.Framework;
using NUnit.Framework.Constraints;
using System.Reflection;
using System.Text;

namespace Test;

[TestFixture]
[Parallelizable(scope: ParallelScope.All)]
public class ProgramTest
{
    [Test]
    public void Prints_help_when_no_arguments_are_passed()
    {
        var res = RunHandle2([]);

        Assert.That(res.ExitCode, Is.EqualTo(0));
        Assert.That(res.StandardOutput, Contains.Substring("console utility that displays"));
        Assert.That(res.StandardError, Is.Empty);
    }

    [TestCase("--help")]
    [TestCase("-h")]
    [TestCase("-?")]
    public void Prints_help_when_help_parameter_is_passed(string option)
    {
        var res = RunHandle2([option]);

        Assert.That(res.ExitCode, Is.EqualTo(0));
        Assert.That(res.StandardOutput, Contains.Substring("console utility that displays"));
        Assert.That(res.StandardOutput, Contains.Substring("https://github.com/PolarGoose/Handle2"));
        Assert.That(res.StandardOutput, Contains.Substring("--json"));
        Assert.That(res.StandardOutput, Contains.Substring("--path"));
        Assert.That(res.StandardOutput, Contains.Substring("--dump-all-handles"));
        Assert.That(res.StandardError, Is.Empty);
    }

    [Test]
    public void Prints_version_without_help_text()
    {
        var res = RunHandle2(["--version"]);
        var version = typeof(CommandLineOptions).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()!.InformationalVersion;

        Assert.That(res.ExitCode, Is.EqualTo(0));
        Assert.That(res.StandardOutput.Trim(), Is.EqualTo(version));
        Assert.That(res.StandardError, Is.Empty);
    }

    [TestCase(["--unknown"])]
    [TestCase(["--json"])]
    [TestCase(["non existing parameter"])]
    [TestCase(["--path", @"C:\Windows", "--dump-all-handles"])]
    [TestCase(["--path"])]
    [TestCase(["--path", "--json"])]
    [TestCase(["--path="])]
    [TestCase(["--path", " "])]
    [TestCase(["--path", @"C:\Windows", "--path", @"C:\Windows\System32"])]
    [TestCase(["--json=invalid", "--path", @"C:\Windows"])]
    [TestCase(["--json=true", "--json=false", "--path", @"C:\Windows"])]
    [TestCase(["--dump-all-handles=invalid"])]
    [TestCase(["--dump-all-handles=true", "--dump-all-handles=false"])]
    [TestCase(["--dump-all-handles=false"])]
    [TestCase(["--dump-all-handles", "false"])]
    [TestCase(["--path --json"])]
    [TestCase(["--dump-all-handles param"])]
    public void Prints_help_and_an_error_message_when_wrong_parameters_are_passed(object[] args)
    {
        var res = RunHandle2(args.Select(a => a.ToString()!));

        Assert.That(res.ExitCode, Is.EqualTo(1));
        Assert.That(res.StandardOutput, Contains.Substring("console utility that displays"));
        Assert.That(res.StandardError, Is.Not.Empty);
    }

    [Test]
    public void Dump_all_handle_information()
    {
        var res = RunHandle2(["--dump-all-handles"]);

        Assert.That(res.ExitCode, Is.EqualTo(0));
        Assert.That(res.StandardOutput, ContainsIgnoreCase(@"C:\Windows\system32\sihost.exe"));
        Assert.That(res.StandardOutput, ContainsIgnoreCase(@"Desktop"));
        Assert.That(res.StandardOutput, ContainsIgnoreCase(@"\Default"));
        Assert.That(res.StandardOutput, ContainsIgnoreCase(@"  File            C:\Windows\System32\"));
        Assert.That(res.StandardOutput, ContainsIgnoreCase(@"  Module          C:\WINDOWS\SYSTEM32\ntdll.dll"));
    }

    [Test]
    public void Dump_all_handle_information_json()
    {
        var res = RunHandle2(["--dump-all-handles", "--json"]);

        Assert.That(res.ExitCode, Is.EqualTo(0));
        Assert.That(res.StandardOutput, Contains.Substring(@"GrantedAccess"));
        Assert.That(res.StandardOutput, Contains.Substring(@"FILE_TYPE_UNKNOWN"));
        Assert.That(res.StandardOutput, Contains.Substring(@"\\Device\\HarddiskVolume"));
        Assert.That(res.StandardOutput, Contains.Substring(@"        ""HandleType"": ""Section"","));
        Assert.That(res.StandardOutput, Contains.Substring(@"    ""ModuleNames"": ["));
        Assert.That(res.StandardOutput, ContainsIgnoreCase(@"      ""C:\\WINDOWS\\SYSTEM32\\ntdll.dll"","));
    }

    [Test]
    public void Path_non_locked_file()
    {
        var res = RunHandle2(["--path", @"C:\Windows\system.ini"]);

        Assert.That(res.ExitCode, Is.EqualTo(0));
        Assert.That(res.StandardOutput, Is.Empty);
        Assert.That(res.StandardError, Is.Empty);
    }

    [TestCase(new[] { @"--path=C:\Windows\system.ini" }, false)]
    [TestCase(new[] { "--json=true", "--path", @"C:\Windows\system.ini" }, true)]
    [TestCase(new[] { "--json", "true", "--path", @"C:\Windows\system.ini" }, true)]
    [TestCase(new[] { "--json=false", "--path", @"C:\Windows\system.ini" }, false)]
    [TestCase(new[] { "--json", "false", "--path", @"C:\Windows\system.ini" }, false)]
    [TestCase(new[] { "--json", "--json", "--path", @"C:\Windows\system.ini" }, true)]
    [TestCase(new[] { "--dump-all-handles=false", "--path", @"C:\Windows\system.ini" }, false)]
    public void Parses_path_options_and_returns_the_action_exit_code(string[] args, bool expectedJson)
    {
        CommandLineOptions? options = null;
        var command = CommandLineOptions.CreateCommand(parsedOptions =>
        {
            options = parsedOptions;
            return 42;
        });

        var exitCode = command.Parse(args).Invoke();

        Assert.That(exitCode, Is.EqualTo(42));
        Assert.That(options, Is.Not.Null);
        Assert.That(options.Path, Is.EqualTo(@"C:\Windows\system.ini"));
        Assert.That(options.Json, Is.EqualTo(expectedJson));
        Assert.That(options.DumpAllHandles, Is.False);
    }

    [Test]
    public void Path_that_does_not_exist()
    {
        var res = RunHandle2(["--path", @"C:\non_existing_path"]);

        Assert.That(res.ExitCode, Is.EqualTo(1));
        Assert.That(res.StandardError, Contains.Substring(@"C:\non_existing_path"));
        Assert.That(res.StandardError, Contains.Substring("does not exist."));
        Assert.That(res.StandardOutput, Contains.Substring("console utility that displays"));
    }

    [Test]
    public void Path_json_that_does_not_exist()
    {
        var res = RunHandle2(["--path", @"C:\non_existing_path", "--json"]);

        Assert.That(res.ExitCode, Is.EqualTo(1));
        Assert.That(res.StandardError, Contains.Substring(@"C:\non_existing_path"));
        Assert.That(res.StandardError, Contains.Substring("does not exist."));
        Assert.That(res.StandardOutput, Contains.Substring("console utility that displays"));
    }

    [TestCase(@"C:\Windows")]
    [TestCase(@"C:/Windows")]
    [TestCase(@"C:/windows")]
    public void Path_locked_folder(string path)
    {
        var res = RunHandle2(["--path", path]);

        Assert.That(res.ExitCode, Is.EqualTo(0));
        Assert.That(res.StandardOutput, ContainsIgnoreCase(@"C:\Windows\system32\svchost.exe"));
        Assert.That(res.StandardOutput, ContainsIgnoreCase(@"C:\Windows\System32\"));
        Assert.That(res.StandardOutput, ContainsIgnoreCase(@"C:\Windows\System32\ntdll.dll"));
        Assert.That(res.StandardOutput, Does.Not.Contain(@"\\Device\\HarddiskVolume"));
    }

    [TestCase(@"C:\Windows")]
    [TestCase(@"C:/Windows")]
    [TestCase(@"C:/windows")]
    public void Path_json_locked_folder(string path)
    {
        var res = RunHandle2(["--path", path, "--json"]);

        Assert.That(res.ExitCode, Is.EqualTo(0));
        Assert.That(res.StandardOutput, Contains.Substring(@"        ""HandleType"": ""File"","));
        Assert.That(res.StandardOutput, Contains.Substring(@"FILE_TYPE_DISK"));
        Assert.That(res.StandardOutput, Contains.Substring(@"\\Device\\HarddiskVolume"));
        Assert.That(res.StandardOutput, Does.Not.Contain(@"FILE_TYPE_UNKNOWN"));
    }

    [Test]
    public void When_path_is_a_file_only_shows_processes_locking_this_file()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), "Handle2Tests_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempDir);

        try
        {
            var file = Path.Combine(tempDir, "file.txt");
            var fileLocked = Path.Combine(tempDir, "fileLocked.txt");

            // Dummy writes to create the files
            File.WriteAllText(file, "not locked");
            File.WriteAllText(fileLocked, "locked");

            // This will make sure the file is locked
            using FileStream fileLockedStream = new FileStream(fileLocked, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
           
            var res = RunHandle2(["--path", file]);
            Assert.That(res.ExitCode, Is.EqualTo(0));
            Assert.That(res.StandardOutput, Is.Empty);

            res = RunHandle2(["--path", fileLocked]);
            Assert.That(res.ExitCode, Is.EqualTo(0));
            Assert.That(res.StandardOutput, Contains.Substring("fileLocked.txt"));
        }
        finally
        {
            Directory.Delete(tempDir, recursive: true);
        }
    }

    [Test]
    public void Works_with_files_containing_unicode_symbols()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), "Handle2Tests_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempDir);

        try
        {
            var file = Path.Combine(tempDir, "полярный гусь _ 北极鹅 _ ホッキョクガチョウ");

            // Dummy write to create the files
            File.WriteAllText(file, "locked");

            // This will make sure the file is locked
            using FileStream fileLockedStream = new FileStream(file, FileMode.Open, FileAccess.ReadWrite, FileShare.None);

            var res = RunHandle2(["--path", file]);
            Assert.That(res.ExitCode, Is.EqualTo(0));
            Assert.That(res.StandardOutput, Contains.Substring(file));

            // JSON escapes unicode symbols
            res = RunHandle2(["--json", "--path", file]);
            Assert.That(res.ExitCode, Is.EqualTo(0));
            Assert.That(res.StandardOutput, Contains.Substring(@"u043F\u043E\u043B\u044F\u0440\u043D\u044B\u0439 \u0433\u0443\u0441\u044C _ \u5317\u6781\u9E45 _ \u30DB\u30C3\u30AD\u30E7\u30AF\u30AC\u30C1\u30E7\u30A6"));
        }
        finally
        {
            Directory.Delete(tempDir, recursive: true);
        }
    }

    private static BufferedCommandResult RunHandle2(IEnumerable<string> args) =>
        Cli.Wrap(Path.Combine(AppContext.BaseDirectory, "Handle2.exe"))
           .WithValidation(CommandResultValidation.None)
           .WithArguments(args)
           .ExecuteBufferedAsync(Encoding.UTF8)
           .GetAwaiter()
           .GetResult();

    private static StringConstraint ContainsIgnoreCase(string expected) => Contains.Substring(expected).Using(StringComparison.OrdinalIgnoreCase);
}
