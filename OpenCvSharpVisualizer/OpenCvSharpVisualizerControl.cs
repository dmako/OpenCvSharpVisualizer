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

    public override Task<string> GetXamlAsync(CancellationToken cancellationToken)
        => base.GetXamlAsync(cancellationToken);

    public override Task ControlLoadedAsync(CancellationToken cancellationToken)
        => base.ControlLoadedAsync(cancellationToken);

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            TypedContext?.Dispose();
        }
        base.Dispose(disposing);
    }
}
