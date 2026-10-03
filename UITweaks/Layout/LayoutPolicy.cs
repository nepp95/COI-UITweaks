using System;

namespace UITweaks.Layout;

internal static class LayoutPolicy
{
    internal const int DefaultRowsBeforeSplit = 15;
    internal const int MinColumns = 1;
    internal const int MaxColumns = 4;

    internal static int AdjustColumns(int currentColumns, int direction)
    {
        int nextColumns = currentColumns + Math.Sign(direction);
        return Math.Max(MinColumns, Math.Min(MaxColumns, nextColumns));
    }

    // Zero delegates layout to the game.
    internal static int GetColumns(
        bool enabled,
        int fixedColumns,
        int rowsBeforeSplit,
        int pinnedCount)
    {
        if (!enabled)
        {
            return 0;
        }

        if (fixedColumns >= MinColumns && fixedColumns <= MaxColumns)
        {
            return fixedColumns;
        }

        return pinnedCount > Math.Max(1, rowsBeforeSplit) ? 2 : 1;
    }
}
