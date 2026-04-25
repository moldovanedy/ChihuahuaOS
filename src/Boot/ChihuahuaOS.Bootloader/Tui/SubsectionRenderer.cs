using ChihuahuaOS.Bootloader.Tui.Subsections;

namespace ChihuahuaOS.Bootloader.Tui;

public static class SubsectionRenderer
{
    private static readonly GraphicsSubsection GraphicsSubsection = new();

    public const int GLOBAL_START_POS = GraphicsSubsection.GLOBAL_START_POS;
    public const int GLOBAL_END_POS = GraphicsSubsection.GLOBAL_END_POS;

    public static void OnEnterScreen()
    {
        GraphicsSubsection.OnEnterScreen();
    }

    public static (string Title, string[] Description) GetTitleAndDescriptionAt(int globalCursorRowPosition)
    {
        return GraphicsSubsection.GetTitleAndDescriptionAt(globalCursorRowPosition);
    }

    public static (string Title, string[] Values) GetValuesForPropertyAt(int globalCursorRowPosition)
    {
        return GraphicsSubsection.GetValuesForPropertyAt(globalCursorRowPosition);
    }

    public static string? GetValueForProperty(int globalCursorRowPosition)
    {
        return GraphicsSubsection.GetValueForProperty(globalCursorRowPosition);
    }

    public static void SetValueForProperty(int globalCursorRowPosition, string newValue)
    {
        GraphicsSubsection.SetValueForProperty(globalCursorRowPosition, newValue);
    }

    public static void Draw(int globalScrollRowPosition, int globalRedrawRowPosition, int rowsToRedraw)
    {
        GraphicsSubsection.Draw(globalScrollRowPosition, globalRedrawRowPosition, rowsToRedraw);
    }
}