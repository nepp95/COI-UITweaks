namespace UITweaks.Layout;

internal interface IColumnLayout
{
    bool IsDragging { get; }
    int Columns { get; }
    void SetColumns(int columns);
    void ResetAutomaticLayout();
}
