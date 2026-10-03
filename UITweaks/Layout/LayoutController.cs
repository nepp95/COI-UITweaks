namespace UITweaks.Layout;

internal sealed class LayoutController
{
    private bool _customApplied;

    // Returns true only when the game's layout method should run.
    internal bool Apply(IColumnLayout layout, int desiredColumns)
    {
        if (layout.IsDragging && (desiredColumns != 0 || _customApplied))
        {
            return false;
        }

        if (desiredColumns == 0)
        {
            if (_customApplied)
            {
                layout.ResetAutomaticLayout();
            }

            _customApplied = false;
            return true;
        }

        _customApplied = true;
        if (layout.Columns != desiredColumns)
        {
            layout.SetColumns(desiredColumns);
        }

        return false;
    }

    internal void Restore(IColumnLayout layout)
    {
        if (_customApplied)
        {
            layout.ResetAutomaticLayout();
        }

        _customApplied = false;
    }
}
