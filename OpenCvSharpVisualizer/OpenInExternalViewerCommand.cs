using System.IO;
using System.Net.Http;
using Microsoft.VisualStudio.Extensibility;
using Microsoft.VisualStudio.Extensibility.UI;

namespace OpenCvSharpVisualizer;

public sealed class OpenInExternalViewerCommand : NotifyPropertyChangedObject, IAsyncCommand, IDisposable
{
    private readonly OpenCvSharpVisualizerDataContext context;
    private readonly HttpClient _httpClient = new();
    private bool _executionFailed = false;
    private bool _disposedValue = false;

    public OpenInExternalViewerCommand(OpenCvSharpVisualizerDataContext context)
    {
        this.context = context;
        this.context.PropertyChanged += OnContextPropertyChanged;
    }

    public bool CanExecute => !_executionFailed && !string.IsNullOrWhiteSpace(context.ImageUrl);

    private void OnContextPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (string.CompareOrdinal(e.PropertyName, nameof(context.ImageUrl)) == 0)
        {
            RaiseNotifyPropertyChangedEvent(nameof(CanExecute));
        }
    }

    public async Task ExecuteAsync(object? parameter, IClientContext clientContext, CancellationToken cancellationToken)
    {
        if (parameter is not string url || string.IsNullOrWhiteSpace(url))
        {
            return;
        }

        try
        {
            // the file is not deleted after being opened, to allow the external viewer to read it without racing with our deletion logic,
            // and to allow the user to open it manually if automatic opening fails for some reason
            var filePath = Path.Combine(Path.GetTempPath(),  Path.GetRandomFileName() + ".png");
            {
                var response = await _httpClient.GetAsync(url, cancellationToken);
                _ = response.EnsureSuccessStatusCode();
                using var networkStream = await response.Content.ReadAsStreamAsync(cancellationToken);
                using var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.Read);
                await networkStream.CopyToAsync(fileStream, cancellationToken);
            }

            var info = new System.Diagnostics.ProcessStartInfo(filePath)
            {
                UseShellExecute = true
            };
            _ = System.Diagnostics.Process.Start(info);
        }
        catch
        {
            _executionFailed = true;
            RaiseNotifyPropertyChangedEvent(nameof(CanExecute));
        }
    }

    private void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            if (disposing)
            {
                _httpClient.Dispose();
            }
            _disposedValue = true;
        }
    }

    void IDisposable.Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
