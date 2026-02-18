using Microsoft.VisualStudio.Extensibility.DebuggerVisualizers;
using Microsoft.VisualStudio.Extensibility.UI;

namespace OpenCvSharpVisualizer;

/// <summary>
/// Remote user control to visualize the <see cref="OpenCvSharpVisualizerDataContext"/> value.
/// </summary>
internal partial class OpenCvSharpVisualizerControl : RemoteUserControl
{
    public OpenCvSharpVisualizerDataContext? TypedContext => DataContext as OpenCvSharpVisualizerDataContext;

    public OpenCvSharpVisualizerControl(VisualizerTarget visualizerTarget, IRemoteImageProvider imageProvider)
        : base(dataContext: new OpenCvSharpVisualizerDataContext(visualizerTarget, imageProvider))
    {
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            (TypedContext as IDisposable)?.Dispose();
        }
        base.Dispose(disposing);
    }
}
