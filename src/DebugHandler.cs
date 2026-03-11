using System.Diagnostics;

namespace Philiprehberger.HttpDebugHandler;

/// <summary>
/// Captures metadata about a single outgoing HTTP request.
/// </summary>
public sealed record RequestLog(
    string Method,
    Uri? RequestUri,
    int StatusCode,
    long ElapsedMilliseconds,
    DateTimeOffset Timestamp
);

/// <summary>
/// A <see cref="DelegatingHandler"/> that times every outgoing request and
/// invokes a callback with the resulting <see cref="RequestLog"/>.
/// </summary>
public sealed class DebugHandler : DelegatingHandler
{
    private readonly Action<RequestLog> _callback;

    /// <summary>
    /// Initialises a new instance that writes log entries to <paramref name="callback"/>.
    /// </summary>
    public DebugHandler(Action<RequestLog> callback)
    {
        _callback = callback ?? throw new ArgumentNullException(nameof(callback));
    }

    /// <summary>
    /// Initialises a new instance that writes log entries to <see cref="Console.WriteLine(string)"/>.
    /// </summary>
    public DebugHandler()
    {
        _callback = log =>
            Console.WriteLine(
                $"[{log.Timestamp:O}] {log.Method} {log.RequestUri} -> {log.StatusCode} ({log.ElapsedMilliseconds} ms)"
            );
    }

    /// <inheritdoc/>
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var sw = Stopwatch.StartNew();
        var timestamp = DateTimeOffset.UtcNow;

        HttpResponseMessage response = await base.SendAsync(request, cancellationToken);

        sw.Stop();

        var log = new RequestLog(
            Method: request.Method.Method,
            RequestUri: request.RequestUri,
            StatusCode: (int)response.StatusCode,
            ElapsedMilliseconds: sw.ElapsedMilliseconds,
            Timestamp: timestamp
        );

        _callback(log);

        return response;
    }
}
