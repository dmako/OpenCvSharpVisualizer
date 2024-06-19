namespace OpenCvSharpVisualizer;

public interface IRemoteImageProvider
{
    string SetImageData(byte[]? imageData);
}
