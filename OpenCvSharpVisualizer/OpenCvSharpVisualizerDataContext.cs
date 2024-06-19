using System.Runtime.Serialization;
using Microsoft.VisualStudio.Extensibility.DebuggerVisualizers;
using Microsoft.VisualStudio.Extensibility.UI;
using Microsoft.VisualStudio.RpcContracts.DebuggerVisualizers;
using OpenCvSharpVisualizer.DebuggeeSide;

namespace OpenCvSharpVisualizer;

[DataContract]
public class OpenCvSharpVisualizerDataContext : NotifyPropertyChangedObject, IDisposable
{
    private readonly VisualizerTarget _visualizerTarget;
    private MatObjectDataSource? _model;
    private IRemoteImageProvider _imageProvider;

    public OpenCvSharpVisualizerDataContext(VisualizerTarget visualizerTarget, IRemoteImageProvider imageProvider)
    {
        _visualizerTarget = visualizerTarget;
        _imageProvider = imageProvider;
        visualizerTarget.StateChanged += OnStateChangedAsync;
        ImageUrl = _imageProvider.SetImageData(null);
        OpenExternalCommand = new OpenInExternalViewerCommand(this);
    }

    [DataMember]
    public int Width => _model?.Width ?? 0;

    [DataMember]
    public int Height => _model?.Height ?? 0;

    [DataMember]
    public string ImageUrl { get; private set; }

    [DataMember]
    public string Description => _model?.Description ?? string.Empty;

    private bool _useBorder;
    [DataMember]
    public bool UseBorder
    {
        get => _useBorder;
        set
        {
            _ = SetProperty(ref _useBorder, value);
            RaiseNotifyPropertyChangedEvent(nameof(BorderThickness));
        }
    }

    [DataMember]
    public int BorderThickness => _useBorder ? 2 : 0;

    [DataMember]
    public IAsyncCommand OpenExternalCommand { get; }

    private async Task<MatObjectDataSource?> GetRequestAsync(VisualizerTargetStateNotification args)
    {
        return args switch
        {
            VisualizerTargetStateNotification.Available or VisualizerTargetStateNotification.ValueUpdated => await _visualizerTarget.ObjectSource.RequestDataAsync<MatObjectDataSource>(jsonSerializer: null, CancellationToken.None),
            VisualizerTargetStateNotification.Unavailable => null,
            _ => throw new NotSupportedException("Unsupported visualizer target state notification"),
        };
    }

    private async Task OnStateChangedAsync(object? sender, VisualizerTargetStateNotification args)
    {
        var dataSource = await GetRequestAsync(args);
        if (string.IsNullOrEmpty(dataSource?.PngDataBase64))
        {
            ResetBindings();
            return;
        }

        try
        {
            _model = dataSource;
            var rawData = Convert.FromBase64String(dataSource.PngDataBase64);
            ImageUrl = _imageProvider.SetImageData(rawData);

            RaiseNotifyPropertyChangedEvent(nameof(ImageUrl));
            RaiseNotifyPropertyChangedEvent(nameof(Width));
            RaiseNotifyPropertyChangedEvent(nameof(Height));
            RaiseNotifyPropertyChangedEvent(nameof(Description));
        }
        catch
        {
            ResetBindings();
        }
    }

    private void ResetBindings()
    {
        ImageUrl = string.Empty;
        _model = null;
    }

    public void Dispose()
    {
        _visualizerTarget.StateChanged -= OnStateChangedAsync;
        _visualizerTarget.Dispose();

        ResetBindings();
        GC.SuppressFinalize(this);
    }
}
