namespace OpenCvSharpVisualizer.DebuggeeSide;

[Serializable]
public class MatObjectDataSource
{
    /// <summary>
    /// Mat data in PNG format, encoded in Base64
    /// </summary>
    public string PngDataBase64 { get; set; } = string.Empty;

    /// <summary>
    /// Width of the image
    /// </summary>
    public required int Width { get; init; }

    /// <summary>
    /// Height of the image
    /// </summary>
    public required int Height { get; init; }

    /// <summary>
    /// Mat description
    /// </summary>
    public required string Description { get; init; }
}
