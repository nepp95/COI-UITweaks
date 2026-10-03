using Microsoft.VisualStudio.TestTools.UnitTesting;
using UITweaks.Layout;

namespace UITweaks.Tests.Layout;

[TestClass]
public sealed class LayoutPolicyTests
{
    [DataRow(1, true, 0, 15, 0, DisplayName = "empty list")]
    [DataRow(1, true, 0, 15, 14, DisplayName = "below threshold")]
    [DataRow(1, true, 0, 15, 15, DisplayName = "exact threshold")]
    [DataRow(2, true, 0, 15, 16, DisplayName = "first resource over threshold")]
    [DataRow(2, true, 0, 15, 1000, DisplayName = "many resources remain two columns")]
    [DataRow(2, true, 0, 7, 8, DisplayName = "custom threshold")]
    [DataRow(2, true, 0, 1, 2, DisplayName = "minimum threshold")]
    [DataRow(1, true, 0, 0, 1, DisplayName = "invalid threshold clamps")]
    [DataRow(1, true, 1, 15, 100, DisplayName = "forced single with many resources")]
    [DataRow(2, true, 2, 15, 1, DisplayName = "forced double with one resource")]
    [DataRow(2, true, 2, 15, 0, DisplayName = "forced double empty")]
    [DataRow(3, true, 3, 15, 5, DisplayName = "forced three columns")]
    [DataRow(4, true, 4, 15, 40, DisplayName = "forced four columns")]
    [DataRow(4, true, 4, 15, 0, DisplayName = "forced four columns with empty list")]
    [DataRow(0, false, 2, 15, 100, DisplayName = "disabled overrides fixed setting")]
    [DataRow(0, false, 0, 15, 100, DisplayName = "disabled automatic")]
    [DataRow(2, true, 9, 15, 16, DisplayName = "invalid fixed setting uses threshold")]
    [DataRow(1, true, -1, 15, 15, DisplayName = "negative fixed setting uses threshold")]
    [TestMethod]
    public void GetColumnsReturnsExpectedCount(int expected, bool enabled, int fixedColumns, int threshold, int count)
    {
        Assert.AreEqual(expected, LayoutPolicy.GetColumns(enabled, fixedColumns, threshold, count));
    }

    [TestMethod]
    public void PinningAndUnpinningRecalculatesColumns()
    {
        foreach (int count in new[] { 14, 15, 16, 17, 16, 15, 14, 16 })
        {
            Assert.AreEqual(count > 15 ? 2 : 1, LayoutPolicy.GetColumns(true, 0, 15, count));
        }
    }

    [TestMethod]
    [DataRow(1, -1, 1)]
    [DataRow(4, 1, 4)]
    [DataRow(1, 1, 2)]
    [DataRow(2, 1, 3)]
    [DataRow(4, -1, 3)]
    public void AdjustColumnsStepsWithinLimits(int current, int direction, int expected)
    {
        Assert.AreEqual(expected, LayoutPolicy.AdjustColumns(current, direction));
    }
}
