using System.Drawing;
using System.Drawing.Imaging;
using System.Net;

namespace OpenCvSharpVisualizer;

public class HttpImageProvider : IRemoteImageProvider, IDisposable
{
    public const string ListenerPrefix = "http://localhost:40505/cvimage/";

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
                }
            }
            else
            {
                using var bmpImage = new Bitmap(128, 128);
                var g = Graphics.FromImage(bmpImage);
                g.Clear(Color.Transparent);
                using var font = new Font("Arial", 20);
                var stringSize = g.MeasureString("?", font);
                var titleLocation = new PointF((bmpImage.Width - stringSize.Width) / 2.0f, (bmpImage.Height - stringSize.Height) / 2.0f);
                g.DrawString("?", font, Brushes.LightGray, titleLocation);

                using var memStream = new MemoryStream();
                bmpImage.Save(memStream, ImageFormat.Png);
                memStream.WriteTo(response.OutputStream);
            }
            response.Close();
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
