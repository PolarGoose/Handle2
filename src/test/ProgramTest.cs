using CliWrap;
using CliWrap.Buffered;
using NUnit.Framework;
using System.Text;

namespace Test;

[TestFixture]
public class ProgramTest
{
    [Test]
    public void Prints_help_when_no_arguments_are_passed()
    {
        var res = RunHandle2([]);

        Assert.That(res.ExitCode, Is.EqualTo(0));
        Assert.That(res.StandardError, Contains.Substring("console utility that displays"));
    }

    [Test]
    public void Prints_help_when_help_parameter_is_passed()
    {
        var res = RunHandle2(["--help"]);

        Assert.That(res.ExitCode, Is.EqualTo(0));
        Assert.That(res.StandardError, Contains.Substring("console utility that displays"));
    }

    [TestCase(["non existing parameter"])]
    [TestCase(["--path", "123", "--dump-all-handles"])]
    [TestCase(["--path"])]
    [TestCase(["--path --json"])]
    [TestCase(["--dump-all-handles param"])]
    public void Prints_help_and_an_error_message_when_wrong_parameters_are_passed(object[] args)
    {
        var res = RunHandle2(args.Select(a => a.ToString()));

        Assert.That(res.ExitCode, Is.EqualTo(0));
        Assert.That(res.StandardError, Contains.Substring("console utility that displays"));
        Assert.That(res.StandardError, Contains.Substring("ERROR(S):"));
    }

    [Test]
    public void Dump_all_handle_information()
    {
        var res = RunHandle2(["--dump-all-handles"]);

        Assert.That(res.ExitCode, Is.EqualTo(0));
        Assert.That(res.StandardOutput, Contains.Substring(@"C:\Windows\system32\sihost.exe"));
        Assert.That(res.StandardOutput, Contains.Substring(@"Desktop"));
        Assert.That(res.StandardOutput, Contains.Substring(@"\Default"));
        Assert.That(res.StandardOutput, Contains.Substring(@"C:\Windows\System32\"));
        Assert.That(res.StandardOutput, Contains.Substring(@"C:\Windows\System32\en-US\KernelBase.dll.mui"));
    }

    [Test]
    public void Dump_all_handle_information_json()
    {
        var res = RunHandle2(["--dump-all-handles", "--json"]);

        Assert.That(res.ExitCode, Is.EqualTo(0));
        Assert.That(res.StandardOutput, Contains.Substring(@"GrantedAccess"));
        Assert.That(res.StandardOutput, Contains.Substring(@"FILE_TYPE_UNKNOWN"));
        Assert.That(res.StandardOutput, Contains.Substring(@"C:\\Windows\\System32\\en-US\\propsys.dll.mui"));
        Assert.That(res.StandardOutput, Contains.Substring(@"\\Device\\HarddiskVolume"));
        Assert.That(res.StandardOutput, Contains.Substring(@"        ""HandleType"": ""Section"","));
    }

    [Test]
    public void Path_non_locked_folder()
    {
        var res = RunHandle2(["--path", @"C:\Windows\system.ini"]);

        Assert.That(res.ExitCode, Is.EqualTo(0));
        Assert.That(res.StandardOutput, Is.Empty);
    }

    [Test]
    public void Path_that_does_not_exist()
    {
        var res = RunHandle2(["--path", @"C:\non_existing_path"]);

        Assert.That(res.ExitCode, Is.EqualTo(1));
        Assert.That(res.StandardOutput, Contains.Substring("Error:"));
        Assert.That(res.StandardOutput, Contains.Substring("does not exist."));
    }

    [Test]
    public void Path_json_that_does_not_exist()
    {
        var res = RunHandle2(["--path", @"C:\non_existing_path", "--json"]);

        Assert.That(res.ExitCode, Is.EqualTo(1));
        Assert.That(res.StandardOutput, Contains.Substring("Error:"));
        Assert.That(res.StandardOutput, Contains.Substring("does not exist."));
    }

    [TestCase(@"C:\Windows")]
    [TestCase(@"C:/Windows")]
    [TestCase(@"C:/windows")]
    public void Path_locked_folder(string path)
    {
        var res = RunHandle2(["--path", path]);

        Assert.That(res.ExitCode, Is.EqualTo(0));
        Assert.That(res.StandardOutput, Contains.Substring(@"C:\Windows\system32\svchost.exe"));
        Assert.That(res.StandardOutput, Contains.Substring(@"C:\Windows\System32\"));
        Assert.That(res.StandardOutput, Contains.Substring(@"C:\Windows\System32\en-US\KernelBase.dll.mui"));
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

    private static BufferedCommandResult RunHandle2(IEnumerable<string> args)
    {
        return Cli
            .Wrap(Path.Combine(AppContext.BaseDirectory, "Handle2.exe"))
            .WithValidation(CommandResultValidation.None)
            .WithArguments(args)
            .ExecuteBufferedAsync(Encoding.UTF8)
            .GetAwaiter()
            .GetResult();
    }
}
