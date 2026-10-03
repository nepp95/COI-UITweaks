using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UITweaks.Tests.Compatibility;

[TestClass]
public sealed class GameContractTests
{
    [TestMethod]
    [TestCategory("GameCompatibility")]
    public void InstalledGameSupportsExpectedHudMembers()
    {
        string? configuredRoot = Environment.GetEnvironmentVariable("COI_ROOT");
        string gameRoot = configuredRoot ?? @"C:\Program Files (x86)\Steam\steamapps\common\Captain of Industry";
        string managedPath = Path.Combine(gameRoot, "Captain of Industry_Data", "Managed");

        if (!File.Exists(Path.Combine(managedPath, "Mafi.Unity.dll")))
        {
            if (configuredRoot != null)
            {
                Assert.Fail($"Mafi.Unity.dll not found under COI_ROOT: {configuredRoot}");
            }

            Assert.Inconclusive("Set COI_ROOT to the game installation to run this metadata-only check.");
        }

        GameContract.Verify(managedPath);
    }
}
