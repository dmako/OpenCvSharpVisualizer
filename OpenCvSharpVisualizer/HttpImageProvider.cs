using System.Net;
using AsyncAwaitBestPractices;

namespace OpenCvSharpVisualizer;

public sealed class HttpImageProvider : IRemoteImageProvider, IDisposable
{
    public const string ListenerPrefix = "http://localhost:40506/cvimage/";

    private readonly HttpListener _listener = new();
    private readonly CancellationTokenSource _cancellationTokenSource = new();
    private readonly object _imageDataLock = new();
    private bool _disposedValue = false;
    private byte[]? _imageData = null;

    public HttpImageProvider()
    {
        StartAsync(ListenerPrefix, _cancellationTokenSource.Token)
            .SafeFireAndForget();
    }

    public void Stop() => _cancellationTokenSource.Cancel();

    public async Task StartAsync(string listenerPrefix, CancellationToken cancellationToken)
    {
        _listener.Prefixes.Add(listenerPrefix);
        _listener.Start();

        using var cancellationGuard = cancellationToken.Register(() => _listener.Stop());

        while (!cancellationToken.IsCancellationRequested)
        {
            var listenerContext = await _listener.GetContextAsync();
            try
            {
                ProcessRequest(listenerContext);
            }
            catch
            {
                listenerContext.Response.StatusCode = 404;
                listenerContext.Response.Close();
            }
        }
    }

    private void ProcessRequest(HttpListenerContext listenerContext)
    {
        var request = listenerContext.Request;
        var response = listenerContext.Response;

        var url = request.Url;
        var path = url!.AbsoluteUri;
        if (path.StartsWith(ListenerPrefix, StringComparison.CurrentCultureIgnoreCase))
        {
            response.ContentType = "image/png";
            if (_imageData is { Length: > 0 })
            {
                lock (_imageDataLock)
                {
                    response.OutputStream.Write(_imageData, 0, _imageData.Length);
                    response.Close();
                }
            }
            else
            {
                response.StatusCode = 404;
                response.Close();
            }
        }
        else
        {
            response.StatusCode = 404;
            response.Close();
        }
    }

    public string SetImageData(byte[]? imageData)
    {
        lock (_imageDataLock)
        {
            _imageData = imageData;
            return $"{ListenerPrefix}{Guid.NewGuid()}.png";
        }
    }

    private void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            if (disposing)
            {
                Stop();
                _cancellationTokenSource.Dispose();
                ((IDisposable)_listener).Dispose();
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

public static class HttpListenerExtensions
{
    public static Task GetContextAsync(this HttpListener listener)
        => Task.Factory.FromAsync(listener.BeginGetContext, listener.EndGetContext, TaskCreationOptions.None);
}
