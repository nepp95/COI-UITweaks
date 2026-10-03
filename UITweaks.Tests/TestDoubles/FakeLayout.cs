using UITweaks.Layout;

namespace UITweaks.Tests.TestDoubles;

internal sealed class FakeLayout : IColumnLayout
{
    public bool IsDragging { get; set; }

    public int Columns { get; private set; } = 1;
    public int Changes { get; private set; }

    public int Resets { get; private set; }
    public void SetColumns(int columns)
    {
        Columns = columns;
        Changes++;
    }

    public void ResetAutomaticLayout()
    {
        Columns = 1;
        Resets++;
    }
}
