namespace ChihuahuaOS.Bootloader.Tui.Subsections;

internal interface ISubsection
{
    public void OnEnterScreen();

    public (string Title, string[] Description) GetTitleAndDescriptionAt(int globalCursorRowPosition);

    public (string Title, string[] Values) GetValuesForPropertyAt(int globalCursorRowPosition);

    public string? GetValueForProperty(int globalCursorRowPosition);

    public void SetValueForProperty(int globalCursorRowPosition, string newValue);

    public void Draw(int globalScrollRowPosition, int globalRedrawRowPosition, int rowsToRedraw);
}