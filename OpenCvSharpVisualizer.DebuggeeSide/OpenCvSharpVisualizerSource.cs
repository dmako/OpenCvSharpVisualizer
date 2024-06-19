using Microsoft.VisualStudio.DebuggerVisualizers;
using OpenCvSharp;

namespace OpenCvSharpVisualizer.DebuggeeSide;

public class OpenCvSharpVisualizerSource : VisualizerObjectSource
{
    public override void GetData(object target, Stream outgoingData)
    {
        switch (target)
        {
            case Mat mat:
                SerializeAsJson(outgoingData, CreateDataSource(mat, mat.ToString()));
                break;
            case UMat umat:
                SerializeAsJson(outgoingData, CreateDataSource(umat, umat.ToString()));
                break;
            default:
                throw new NotSupportedException(target.GetType().FullName);
        }
    }

    private static MatObjectDataSource CreateDataSource(InputArray inputArray, string description)
    {
        _ = Cv2.ImEncode(".png", inputArray, out var buffer);
        return new MatObjectDataSource
        {
            PngDataBase64 = Convert.ToBase64String(buffer),
            Width = inputArray.Size().Width,
            Height = inputArray.Size().Height,
            Description = description
        };
    }
}
