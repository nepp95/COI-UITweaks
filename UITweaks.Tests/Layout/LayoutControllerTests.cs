using Microsoft.VisualStudio.TestTools.UnitTesting;
using UITweaks.Layout;
using UITweaks.Tests.TestDoubles;

namespace UITweaks.Tests.Layout;

[TestClass]
public sealed class LayoutControllerTests
{
    [TestMethod]
    public void DisabledLeavesNativeLayoutAlone()
    {
        var layout = new FakeLayout();
        var controller = new LayoutController();
        Assert.IsTrue(controller.Apply(layout, 0));
        Assert.AreEqual(0, layout.Resets);
    }

    [TestMethod]
    public void CustomColumnsSuppressNativeLayout()
    {
        var layout = new FakeLayout();
        var controller = new LayoutController();
        Assert.IsFalse(controller.Apply(layout, 2));
        Assert.AreEqual(2, layout.Columns);
    }

    [TestMethod]
    public void UnchangedColumnsDoNotRebuildRows()
    {
        var layout = new FakeLayout();
        var controller = new LayoutController();
        controller.Apply(layout, 2);
        controller.Apply(layout, 2);
        Assert.AreEqual(1, layout.Changes);
    }

    [TestMethod]
    public void ColumnChangeIsDeferredDuringDrag()
    {
        var layout = new FakeLayout();
        var controller = new LayoutController();
        controller.Apply(layout, 2);
        layout.IsDragging = true;
        Assert.IsFalse(controller.Apply(layout, 1));
        Assert.AreEqual(2, layout.Columns);
    }

    [TestMethod]
    public void DisableWaitsUntilDragEndsThenRestoresBaseline()
    {
        var layout = new FakeLayout();
        var controller = new LayoutController();
        controller.Apply(layout, 2);
        layout.IsDragging = true;
        Assert.IsFalse(controller.Apply(layout, 0));
        Assert.AreEqual(0, layout.Resets);
        layout.IsDragging = false;
        Assert.IsTrue(controller.Apply(layout, 0));
        Assert.AreEqual(1, layout.Columns);
        Assert.AreEqual(1, layout.Resets);
    }

    [TestMethod]
    public void RepeatedDisableRestoresBaselineOnlyOnce()
    {
        var layout = new FakeLayout();
        var controller = new LayoutController();
        controller.Apply(layout, 2);
        controller.Apply(layout, 0);
        controller.Apply(layout, 0);
        Assert.AreEqual(1, layout.Resets);
    }

    [TestMethod]
    public void RestoreResetsColumnsAndIsIdempotent()
    {
        var layout = new FakeLayout();
        var controller = new LayoutController();
        controller.Apply(layout, 2);
        controller.Restore(layout);
        Assert.AreEqual(1, layout.Columns);
        Assert.AreEqual(1, layout.Resets);
        controller.Restore(layout);
        Assert.AreEqual(1, layout.Resets);
    }
}
