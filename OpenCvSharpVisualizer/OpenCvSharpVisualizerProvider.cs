using Microsoft.VisualStudio.Extensibility;
using Microsoft.VisualStudio.Extensibility.DebuggerVisualizers;
using Microsoft.VisualStudio.RpcContracts.RemoteUI;

namespace OpenCvSharpVisualizer;

[VisualStudioContribution]
public class OpenCvSharpVisualizerProvider : DebuggerVisualizerProvider
{
    private readonly HttpImageProvider _httpImageProvider = new();

    public OpenCvSharpVisualizerProvider(OpenCvSharpVisualizerExtension extension, VisualStudioExtensibility extensibility)
        : base(extension, extensibility)
    {
    }

    /// <inheritdoc/>
    public override DebuggerVisualizerProviderConfiguration DebuggerVisualizerProviderConfiguration =>
        new(
            new VisualizerTargetType("%OpenCvSharpVisualizer.OpenCvSharpVisualizerProvider.Mat.DisplayName%", typeof(OpenCvSharp.Mat)),
            new VisualizerTargetType("%OpenCvSharpVisualizer.OpenCvSharpVisualizerProvider.UMat.DisplayName%", typeof(OpenCvSharp.UMat))
        )
        {
            VisualizerObjectSourceType = new(typeof(DebuggeeSide.OpenCvSharpVisualizerSource)),
            Style = VisualizerStyle.ToolWindow
        };

    /// <inheritdoc/>
    public override async Task<IRemoteUserControl> CreateVisualizerAsync(VisualizerTarget visualizerTarget, CancellationToken cancellationToken)
        => await Task.FromResult(new OpenCvSharpVisualizerControl(visualizerTarget, _httpImageProvider));
}
