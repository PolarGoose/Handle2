using Handle2.HandleInfo.Utils;
using NUnit.Framework;

namespace Test.HandleInfo.Utils;

[TestFixture]
[Parallelizable(scope: ParallelScope.All)]
internal class FileUtilsTest
{
    [Test]
    public void AddTrailingSeparatorIfItIsAFolderTest()
    {
        Assert.That(FileUtils.AddTrailingSeparatorIfItIsAFolder(@"C:\Windows\System32"), Is.EqualTo(@"C:\Windows\System32\"));
        Assert.That(FileUtils.AddTrailingSeparatorIfItIsAFolder(@"C:\Windows\System32\"), Is.EqualTo(@"C:\Windows\System32\"));
        Assert.That(new CanonicalPath(@"C:\Windows\System32\ntdll.dll").Path, Is.EqualTo(@"C:\Windows\System32\ntdll.dll"));
    }

    [Test]
    public void CanonicalPathTest()
    {
        Assert.That(new CanonicalPath(@"C:\Windows\System32").Path, Is.EqualTo(@"C:\Windows\System32\"));
        Assert.That(new CanonicalPath(@"C:\Windows\System32\").Path, Is.EqualTo(@"C:\Windows\System32\"));
        Assert.That(new CanonicalPath(@"C:\Windows\System32\\").Path, Is.EqualTo(@"C:\Windows\System32\"));
        Assert.That(new CanonicalPath(@"C:\Windows\\System32/").Path, Is.EqualTo(@"C:\Windows\System32\"));
        Assert.That(new CanonicalPath(@"C:/Windows/System32").Path, Is.EqualTo(@"C:\Windows\System32\"));

        Assert.That(new CanonicalPath(@"C:/Windows/System32/ntdll.dll").Path, Is.EqualTo(@"C:\Windows\System32\ntdll.dll"));
        Assert.That(new CanonicalPath(@"C:/Windows/System32//ntdll.dll").Path, Is.EqualTo(@"C:\Windows\System32\ntdll.dll"));
        Assert.That(new CanonicalPath(@"C:\Windows\\System32\ntdll.dll").Path, Is.EqualTo(@"C:\Windows\System32\ntdll.dll"));
    }

    [Test]
    [Explicit]
    public void CanonicalPath_converts_network_mounts_to_UNC_path()
    {
        Assert.That(new CanonicalPath(@"Z:\var\tmp\").Path, Is.EqualTo(@"\\wsl.localhost\Ubuntu-24.04\var\tmp\"));
        Assert.That(new CanonicalPath(@"Z:\var\tmp").Path, Is.EqualTo(@"\\wsl.localhost\Ubuntu-24.04\var\tmp\"));
        Assert.That(new CanonicalPath(@"Z:\init").Path, Is.EqualTo(@"\\wsl.localhost\Ubuntu-24.04\init"));
    }

    [Test]
    [Explicit]
    public void CanonicalPath_converts_path_on_mounted_drives()
    {
        Assert.That(new CanonicalPath(@"E:\test\test_doc.docx").Path, Is.EqualTo(@"E:\test\test_doc.docx"));
        Assert.That(new CanonicalPath(@"E:\test").Path, Is.EqualTo(@"E:\test\"));
        Assert.That(new CanonicalPath(@"E:\test\").Path, Is.EqualTo(@"E:\test\"));
    }
}
