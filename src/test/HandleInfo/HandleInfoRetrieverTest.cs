using Handle2.HandleInfo;
using NUnit.Framework;

namespace Test.HandleInfo;

[TestFixture]
[Parallelizable(scope: ParallelScope.All)]
internal class HandleInfoRetrieverTest
{
    [Test]
    [Explicit]
    public void Shows_files_locked_on_network_share_drive()
    {
        var uniqueTmpFileOnSharedDrive = $@"\\wsl.localhost\Ubuntu-24.04\var\tmp\uniqueTmpFile_{Guid.NewGuid():N}.txt";

        try
        {
            using (File.Create(uniqueTmpFileOnSharedDrive))
            {
                var infos = HandleInfoRetriever.GetProcInfosLockingPath(uniqueTmpFileOnSharedDrive).ToList();
                Assert.That(infos, Is.Not.Empty);
                AssertLocksPath(infos[0], uniqueTmpFileOnSharedDrive);

                infos = HandleInfoRetriever.GetProcInfosLockingPath(@"\\wsl.localhost\Ubuntu-24.04\var\tmp").ToList();
                Assert.That(infos, Is.Not.Empty);
                AssertLocksPath(infos[0], uniqueTmpFileOnSharedDrive);
            }
        }
        finally
        {
            if (File.Exists(uniqueTmpFileOnSharedDrive))
            {
                File.Delete(uniqueTmpFileOnSharedDrive);
            }
        }
    }

    [Test]
    [Explicit]
    public void Shows_files_locked_on_mounted_drives()
    {
        var file = @"E:\test\test_doc.docx";

        var infos = HandleInfoRetriever.GetProcInfosLockingPath(file).ToList();
        Assert.That(infos, Is.Not.Empty);
        AssertLocksPath(infos[0], file);
    }

    private static void AssertLocksPath(ProcessInfo process, string lockedPath)
    {
        var path = process.Handles
                          .Select(handle => handle.FullNameIfItIsAFileOrAFolder)
                          .Concat(process.ModuleNames)
                          .FirstOrDefault(path => string.Equals(path, lockedPath, StringComparison.InvariantCultureIgnoreCase));

        Assert.That(path, Is.Not.Null,
            $"{process}\ndoesn't lock the path '{lockedPath}'.\nIt only locks the following paths:\n* {string.Join("\n* ", process.Handles.Select(handle => handle.FullNameIfItIsAFileOrAFolder).Concat(process.ModuleNames))}");
    }
}
