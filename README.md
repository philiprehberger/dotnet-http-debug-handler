# Philiprehberger.HttpDebugHandler

[![CI](https://github.com/philiprehberger/dotnet-http-debug-handler/actions/workflows/ci.yml/badge.svg)](https://github.com/philiprehberger/dotnet-http-debug-handler/actions/workflows/ci.yml)
[![NuGet](https://img.shields.io/nuget/v/Philiprehberger.HttpDebugHandler.svg)](https://www.nuget.org/packages/Philiprehberger.HttpDebugHandler)
[![License](https://img.shields.io/github/license/philiprehberger/dotnet-http-debug-handler)](LICENSE)

A `DelegatingHandler` that logs and times every outgoing `HttpClient` request.

## Install

```bash
dotnet add package Philiprehberger.HttpDebugHandler
```

## Usage

```csharp
using Philiprehberger.HttpDebugHandler;

// Default: writes to Console
var client = new HttpClient(new DebugHandler(new HttpClientHandler()));

// Custom callback
var handler = new DebugHandler(log =>
{
    Console.WriteLine($"{log.Method} {log.RequestUri} => {log.StatusCode} in {log.ElapsedMilliseconds} ms");
})
{
    InnerHandler = new HttpClientHandler()
};

var client = new HttpClient(handler);

var response = await client.GetAsync("https://example.com");
// Output: GET https://example.com/ => 200 in 142 ms
```

### ASP.NET Core / IHttpClientFactory

```csharp
builder.Services.AddTransient<DebugHandler>();
builder.Services.AddHttpClient("my-client")
    .AddHttpMessageHandler<DebugHandler>();
```

### Body Capture

Enable request and/or response body logging:

```csharp
var handler = new DebugHandler(log =>
{
    Console.WriteLine($"{log.Method} {log.RequestUri} => {log.StatusCode}");
    if (log.RequestBody is not null)
        Console.WriteLine($"  Request body: {log.RequestBody}");
    if (log.ResponseBody is not null)
        Console.WriteLine($"  Response body: {log.ResponseBody}");
})
{
    InnerHandler = new HttpClientHandler(),
    CaptureRequestBody = true,
    CaptureResponseBody = true,
    MaxBodyCaptureSize = 4096 // optional, default is 65536 (64 KB)
};
```

### Header Logging

Capture request and response headers:

```csharp
var handler = new DebugHandler(log =>
{
    if (log.RequestHeaders is not null)
        foreach (var h in log.RequestHeaders)
            Console.WriteLine($"  > {h.Key}: {h.Value}");

    if (log.ResponseHeaders is not null)
        foreach (var h in log.ResponseHeaders)
            Console.WriteLine($"  < {h.Key}: {h.Value}");
})
{
    InnerHandler = new HttpClientHandler(),
    CaptureHeaders = true
};
```

### Slow Request Warning

Get notified when a request exceeds a time threshold:

```csharp
var handler = new DebugHandler(log =>
{
    Console.WriteLine($"{log.Method} {log.RequestUri} => {log.StatusCode} in {log.ElapsedMilliseconds} ms");
})
{
    InnerHandler = new HttpClientHandler(),
    SlowRequestThreshold = TimeSpan.FromSeconds(2),
    OnSlowRequest = log =>
        Console.WriteLine($"SLOW REQUEST: {log.Method} {log.RequestUri} took {log.ElapsedMilliseconds} ms")
};
```

## API

### `RequestLog`

| Property | Type | Description |
|----------|------|-------------|
| `Method` | `string` | HTTP method (GET, POST, ...) |
| `RequestUri` | `Uri?` | Request URL |
| `StatusCode` | `int` | HTTP response status code |
| `ElapsedMilliseconds` | `long` | Round-trip time in milliseconds |
| `Timestamp` | `DateTimeOffset` | UTC time the request was sent |
| `RequestBody` | `string?` | Request body (when `CaptureRequestBody` is enabled) |
| `ResponseBody` | `string?` | Response body (when `CaptureResponseBody` is enabled) |
| `RequestHeaders` | `Dictionary<string, string>?` | Request headers (when `CaptureHeaders` is enabled) |
| `ResponseHeaders` | `Dictionary<string, string>?` | Response headers (when `CaptureHeaders` is enabled) |

### `DebugHandler`

| Member | Description |
|--------|-------------|
| `DebugHandler()` | Writes formatted log lines to `Console` |
| `DebugHandler(Action<RequestLog>)` | Invokes the provided callback for every request |
| `CaptureRequestBody` | `bool` — capture request body content (default `false`) |
| `CaptureResponseBody` | `bool` — capture response body content (default `false`) |
| `CaptureHeaders` | `bool` — capture request and response headers (default `false`) |
| `MaxBodyCaptureSize` | `int` — max characters to capture per body (default `65536`) |
| `SlowRequestThreshold` | `TimeSpan?` — threshold for slow request detection |
| `OnSlowRequest` | `Action<RequestLog>?` — callback for requests exceeding the threshold |

## Development

```bash
dotnet build src/Philiprehberger.HttpDebugHandler.csproj --configuration Release
```

## License

MIT
