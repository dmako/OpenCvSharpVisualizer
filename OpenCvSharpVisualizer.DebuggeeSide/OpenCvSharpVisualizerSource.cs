using Microsoft.VisualStudio.DebuggerVisualizers;
using OpenCvSharp;

namespace OpenCvSharpVisualizer.DebuggeeSide;

public class OpenCvSharpVisualizerSource : VisualizerObjectSource
{
    // shared resource for invalid or disposed Mat, to avoid the overhead of creating a new one for each such case
    // intentionally not disposed, as it is meant to be reused and should last for the lifetime of the debugged application
    private static readonly Mat invalidImage = Initialize();

    private static Mat Initialize()
    {
        try
        {
            using var resourceStream = typeof(OpenCvSharpVisualizerSource).Assembly.GetManifestResourceStream("data/disposed.png");
            var mat = Mat.FromStream(resourceStream, ImreadModes.Grayscale);
            return mat.CvPtr == IntPtr.Zero ? throw new InvalidDataException("Failed to load the default image for disposed Mat.") : mat;
        }
        catch
        {
            return new Mat(64, 64, MatType.CV_8UC1, Scalar.PaleVioletRed);
        }
    }

    public override void GetData(object target, Stream outgoingData)
    {
        switch (target)
        {
            case Mat mat:
                SerializeAsJson(outgoingData, CreateDataSource(mat));
                break;
            case UMat umat:
                SerializeAsJson(outgoingData, CreateDataSource(umat));
                break;
            default:
                throw new NotSupportedException(target.GetType().FullName);
        }
    }

    private static MatObjectDataSource CreateDataSource(Mat mat)
    {
        using var pngWriteableMat = PreparePngMat(mat);
        _ = Cv2.ImEncode(".png", pngWriteableMat, out var buffer);

        return new MatObjectDataSource
        {
            PngDataBase64 = Convert.ToBase64String(buffer),
            Width = mat.Width,
            Height = mat.Height,
            Description = mat.ToString()
        };
    }

    private static MatObjectDataSource CreateDataSource(UMat umat)
    {
        using var mat = umat.GetMat(AccessFlag.READ);
        using var pngWriteableMat = PreparePngMat(mat);
        _ = Cv2.ImEncode(".png", pngWriteableMat, out var buffer);

        return new MatObjectDataSource
        {
            PngDataBase64 = Convert.ToBase64String(buffer),
            Width = umat.Width,
            Height = umat.Height,
            Description = umat.ToString()
        };
    }

    private static Mat PreparePngMat(Mat mat)
    {
        if (mat.IsDisposed || mat.Empty())
        {
            return invalidImage;
        }

        var matType = mat.Type();

        if (matType.Depth == MatType.CV_USRTYPE1)
        {
            throw new NotSupportedException("Unsupported MatType: CV_USRTYPE1");
        }
        else if (matType.IsInteger && matType.Channels != 2)
        {
            return mat.Clone();
        }
        else if (matType.Channels == 1)
        {
            var result = new Mat(mat.Size(), MatType.CV_8UC1);
            mat.ConvertTo(result, MatType.CV_8UC1, 255.0);
            return result;
        }
        else if (matType.Channels == 3)
        {
            var result = new Mat(mat.Size(), MatType.CV_8UC3);
            mat.ConvertTo(result, MatType.CV_8UC3, 255.0);
            return result;
        }
        else if (matType.Channels == 4)
        {
            var result = new Mat(mat.Size(), MatType.CV_8UC4);
            mat.ConvertTo(result, MatType.CV_8UC4, 255.0);
            return result;
        }
        else if (matType.Channels == 2)
        {
            Cv2.Split(mat, out var twoChannels);
            using var ch1DisposeGuard = twoChannels[0];
            using var ch2DisposeGuard = twoChannels[1];
            var channels = new[] { twoChannels[0], twoChannels[1], twoChannels[0] };
            var result = new Mat(mat.Size(), MatType.CV_8UC3);
            Cv2.Merge(channels, result);
            return result;
        }
        throw new NotSupportedException("Unknown Mat format");
    }
}
