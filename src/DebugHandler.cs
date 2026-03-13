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
    DateTimeOffset Timestamp)
{
    /// <summary>
    /// The request body content, if <see cref="DebugHandler.CaptureRequestBody"/> was enabled.
    /// </summary>
    public string? RequestBody { get; init; }

    /// <summary>
    /// The response body content, if <see cref="DebugHandler.CaptureResponseBody"/> was enabled.
    /// </summary>
    public string? ResponseBody { get; init; }

    /// <summary>
    /// The request headers, if <see cref="DebugHandler.CaptureHeaders"/> was enabled.
    /// </summary>
    public Dictionary<string, string>? RequestHeaders { get; init; }

    /// <summary>
    /// The response headers, if <see cref="DebugHandler.CaptureHeaders"/> was enabled.
    /// </summary>
    public Dictionary<string, string>? ResponseHeaders { get; init; }
}

/// <summary>
/// A <see cref="DelegatingHandler"/> that times every outgoing request and
/// invokes a callback with the resulting <see cref="RequestLog"/>.
/// </summary>
public sealed class DebugHandler : DelegatingHandler
{
    private readonly Action<RequestLog> _callback;

    /// <summary>
    /// When <c>true</c>, the request body is read and stored in <see cref="RequestLog.RequestBody"/>.
    /// Default is <c>false</c>.
    /// </summary>
    public bool CaptureRequestBody { get; set; }

    /// <summary>
    /// When <c>true</c>, the response body is read and stored in <see cref="RequestLog.ResponseBody"/>.
    /// Default is <c>false</c>.
    /// </summary>
    public bool CaptureResponseBody { get; set; }

    /// <summary>
    /// When <c>true</c>, request and response headers are stored in
    /// <see cref="RequestLog.RequestHeaders"/> and <see cref="RequestLog.ResponseHeaders"/>.
    /// Default is <c>false</c>.
    /// </summary>
    public bool CaptureHeaders { get; set; }

    /// <summary>
    /// Maximum number of characters to capture from a request or response body.
    /// Bodies larger than this value are truncated. Default is 65 536 (64 KB).
    /// </summary>
    public int MaxBodyCaptureSize { get; set; } = 65_536;

    /// <summary>
    /// Optional threshold for slow-request detection. When a request takes longer
    /// than this value, <see cref="OnSlowRequest"/> is invoked.
    /// </summary>
    public TimeSpan? SlowRequestThreshold { get; set; }

    /// <summary>
    /// Callback invoked when a request exceeds <see cref="SlowRequestThreshold"/>.
    /// Called in addition to the normal logging callback.
    /// </summary>
    public Action<RequestLog>? OnSlowRequest { get; set; }

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

        // Capture request body before sending (the stream may be consumed).
        string? requestBody = null;
        if (CaptureRequestBody && request.Content is not null)
        {
            requestBody = await request.Content.ReadAsStringAsync(cancellationToken);
            if (requestBody.Length > MaxBodyCaptureSize)
                requestBody = requestBody[..MaxBodyCaptureSize];
        }

        HttpResponseMessage response = await base.SendAsync(request, cancellationToken);

        sw.Stop();

        // Capture response body.
        string? responseBody = null;
        if (CaptureResponseBody)
        {
            responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
            if (responseBody.Length > MaxBodyCaptureSize)
                responseBody = responseBody[..MaxBodyCaptureSize];
        }

        // Capture headers.
        Dictionary<string, string>? requestHeaders = null;
        Dictionary<string, string>? responseHeaders = null;
        if (CaptureHeaders)
        {
            requestHeaders = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var header in request.Headers)
                requestHeaders[header.Key] = string.Join(", ", header.Value);
            if (request.Content is not null)
            {
                foreach (var header in request.Content.Headers)
                    requestHeaders[header.Key] = string.Join(", ", header.Value);
            }

            responseHeaders = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var header in response.Headers)
                responseHeaders[header.Key] = string.Join(", ", header.Value);
            foreach (var header in response.Content.Headers)
                responseHeaders[header.Key] = string.Join(", ", header.Value);
        }

        var log = new RequestLog(
            Method: request.Method.Method,
            RequestUri: request.RequestUri,
            StatusCode: (int)response.StatusCode,
            ElapsedMilliseconds: sw.ElapsedMilliseconds,
            Timestamp: timestamp)
        {
            RequestBody = requestBody,
            ResponseBody = responseBody,
            RequestHeaders = requestHeaders,
            ResponseHeaders = responseHeaders
        };

        _callback(log);

        if (SlowRequestThreshold.HasValue
            && sw.Elapsed >= SlowRequestThreshold.Value
            && OnSlowRequest is not null)
        {
            OnSlowRequest(log);
        }

        return response;
    }
}
