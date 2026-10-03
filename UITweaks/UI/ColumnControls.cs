using System;

using UITweaks.Layout;

using Mafi;
using Mafi.Localization;
using Mafi.Unity.Ui.Hud;
using Mafi.Unity.UiToolkit.Component;
using Mafi.Unity.UiToolkit.Library;

namespace UITweaks.UI;

internal sealed class ColumnControls : IDisposable
{
    private readonly Row _row;
    private readonly ButtonText _decrease;
    private readonly ButtonText _increase;
    private readonly ButtonText _automatic;
    private readonly Label _count;
    private int _lastColumns = -1;
    private bool _lastManualOverride;
    private bool _lastDragging;

    internal ColumnControls(
        PinnedProductsHud hud,
        Action decrease,
        Action increase,
        Action automatic)
    {
        _decrease = new ButtonText("−".AsLoc(), decrease)
            .Compact()
            .Tooltip("Use one fewer resource column.".AsLoc());

        _increase = new ButtonText("+".AsLoc(), increase)
            .Compact()
            .Tooltip("Use one more resource column.".AsLoc());

        _automatic = new ButtonText("Auto".AsLoc(), automatic)
            .Compact()
            .Tooltip("Clear the manual override and use the resource-count threshold.".AsLoc());

        _count = new Label("1".AsLoc());
        _row = new Row().Gap(2.pt()).AlignItemsCenter();
        _row.Add(_decrease, _count, _increase, _automatic);
        hud.Header.Add(_row);
    }

    internal void Refresh(int columns, bool manualOverride, bool dragging)
    {
        if (_lastColumns == columns
            && _lastManualOverride == manualOverride
            && _lastDragging == dragging)
        {
            return;
        }

        _lastColumns = columns;
        _lastManualOverride = manualOverride;
        _lastDragging = dragging;

        _count.Value(columns.ToString().AsLoc());
        _count.Tooltip((manualOverride
            ? "Resource columns: manual override."
            : "Resource columns: automatic.").AsLoc());

        _decrease.Enabled(!dragging && columns > LayoutPolicy.MinColumns);
        _increase.Enabled(!dragging && columns < LayoutPolicy.MaxColumns);
        _automatic.Enabled(!dragging);
    }

    public void Dispose()
    {
        _row.RemoveFromHierarchy();
    }
}
