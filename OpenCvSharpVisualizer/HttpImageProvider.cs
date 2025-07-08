using System.Net;

namespace OpenCvSharpVisualizer;

public class HttpImageProvider : IRemoteImageProvider, IDisposable
{
    public const string ListenerPrefix = "http://localhost:40506/cvimage/";

    private readonly HttpListener _listener = new();
    private readonly CancellationTokenSource _cancellationTokenSource = new();
    private readonly object _imageDataLock = new();
    private byte[]? _imageData = null;

    public HttpImageProvider()
    {
        _ = StartAsync(ListenerPrefix, _cancellationTokenSource.Token);
    }

    public void Stop()
    {
        _cancellationTokenSource.Cancel();
    }

    public async Task StartAsync(string listenerPrefix, CancellationToken cancellationToken)
    {
        var listener = new HttpListener();
        listener.Prefixes.Add(listenerPrefix);
        listener.Start();

        while (!cancellationToken.IsCancellationRequested)
        {
            var listenerContext = await listener.GetContextAsync();
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

    public void Dispose()
    {
        _cancellationTokenSource.Dispose();
        ((IDisposable)_listener).Dispose();
        GC.SuppressFinalize(this);
    }
}

public static class HttpListenerExtensions
{
    public static Task GetContextAsync(this HttpListener listener)
        => Task.Factory.FromAsync(listener.BeginGetContext, listener.EndGetContext, TaskCreationOptions.None);
}
