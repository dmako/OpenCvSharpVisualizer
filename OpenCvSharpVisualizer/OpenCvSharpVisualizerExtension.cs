using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.Extensibility;

namespace OpenCvSharpVisualizer;

/// <summary>
/// Extension entrypoint for the VisualStudio.Extensibility extension.
/// </summary>
[VisualStudioContribution]
public class OpenCvSharpVisualizerExtension : Extension
{
    /// <inheritdoc/>
    public override ExtensionConfiguration ExtensionConfiguration => new()
    {
        Metadata = new ExtensionMetadata(
            id: "OpenCvSharpVisualizer.4287478e-93e3-40b7-811a-46ab1ca973c0",
            version: ExtensionAssemblyVersion,
            publisherName: "David Makovský",
            displayName: "OpenCvSharp Visualizers",
            description: "Visualize OpenCVSharp types"
        )
        {
            Icon = "icon\\opencvsharpvis_32x32.png",
            InstallationTargetArchitecture = VisualStudioArchitecture.Amd64,
            Preview = true,
            Tags = ["OpenCv", "OpenCvSharp", "Visualizer"],
        }
    };

    /// <inheritdoc />
    protected override void InitializeServices(IServiceCollection serviceCollection)
        => base.InitializeServices(serviceCollection);
}
