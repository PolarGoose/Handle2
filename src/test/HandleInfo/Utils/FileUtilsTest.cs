using Handle2.HandleInfo.Utils;
using NUnit.Framework;

namespace Test.HandleInfo.Utils;

[TestFixture]
internal class FileUtilsTest
{
    [Test]
    public void AddTrailingSeparatorIfItIsAFolderTest()
    {
        Assert.That(FileUtils.AddTrailingSeparatorIfItIsAFolder(@"C:\Windows\System32"), Is.EqualTo(@"C:\Windows\System32\"));
        Assert.That(FileUtils.AddTrailingSeparatorIfItIsAFolder(@"C:\Windows\System32\"), Is.EqualTo(@"C:\Windows\System32\\"));
        Assert.That(FileUtils.ToCanonicalPath(@"C:\Windows\System32\ntdll.dll"), Is.EqualTo(@"C:\Windows\System32\ntdll.dll"));
    }

    [Test]
    public void CanonicalPathTest()
    {
        Assert.That(FileUtils.ToCanonicalPath(@"C:\Windows\System32"), Is.EqualTo(@"C:\Windows\System32\"));
        Assert.That(FileUtils.ToCanonicalPath(@"C:\Windows\System32\"), Is.EqualTo(@"C:\Windows\System32\"));
        Assert.That(FileUtils.ToCanonicalPath(@"C:\Windows\System32\\"), Is.EqualTo(@"C:\Windows\System32\"));
        Assert.That(FileUtils.ToCanonicalPath(@"C:\Windows\\System32/"), Is.EqualTo(@"C:\Windows\System32\"));
        Assert.That(FileUtils.ToCanonicalPath(@"C:/Windows/System32"), Is.EqualTo(@"C:\Windows\System32\"));

        Assert.That(FileUtils.ToCanonicalPath(@"C:/Windows/System32/ntdll.dll"), Is.EqualTo(@"C:\Windows\System32\ntdll.dll"));
        Assert.That(FileUtils.ToCanonicalPath(@"C:/Windows/System32//ntdll.dll"), Is.EqualTo(@"C:\Windows\System32\ntdll.dll"));
        Assert.That(FileUtils.ToCanonicalPath(@"C:\Windows\\System32\ntdll.dll"), Is.EqualTo(@"C:\Windows\System32\ntdll.dll"));
    }
}
