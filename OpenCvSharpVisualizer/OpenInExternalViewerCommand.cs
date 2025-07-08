using System.IO;
using System.Net.Http;
using Microsoft.VisualStudio.Extensibility;
using Microsoft.VisualStudio.Extensibility.UI;

namespace OpenCvSharpVisualizer;

public class OpenInExternalViewerCommand : NotifyPropertyChangedObject, IAsyncCommand
{
    private readonly OpenCvSharpVisualizerDataContext context;
    private bool _executionFailed = false;

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
            var filePath = Path.ChangeExtension(Path.GetTempFileName(), "png");
            {
                using var client = new HttpClient();
                var response = await client.GetAsync(url, cancellationToken);
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
}
