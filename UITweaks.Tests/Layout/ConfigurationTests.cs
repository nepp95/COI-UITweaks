using System.Text.Json;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UITweaks.Layout;

namespace UITweaks.Tests.Layout;

[TestClass]
public sealed class ConfigurationTests
{
    [TestMethod]
    public void DefaultThresholdIsFifteenAndMatchesPolicy()
    {
        using var config = ReadConfiguration();
        int threshold = config.RootElement.GetProperty("rows_before_split").GetProperty("default").GetInt32();
        Assert.AreEqual(15, threshold);
        Assert.AreEqual(LayoutPolicy.DefaultRowsBeforeSplit, threshold);
    }

    [TestMethod]
    public void SavedOverrideAllowsFourColumns()
    {
        using var config = ReadConfiguration();
        Assert.AreEqual(4, config.RootElement.GetProperty("fixed_columns").GetProperty("max").GetInt32());
    }

    private static JsonDocument ReadConfiguration()
    {
        return JsonDocument.Parse(File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "config.json")));
    }
}
