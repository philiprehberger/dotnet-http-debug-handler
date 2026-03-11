# Philiprehberger.HttpDebugHandler

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

## API

### `RequestLog`

| Property | Type | Description |
|----------|------|-------------|
| `Method` | `string` | HTTP method (GET, POST, …) |
| `RequestUri` | `Uri?` | Request URL |
| `StatusCode` | `int` | HTTP response status code |
| `ElapsedMilliseconds` | `long` | Round-trip time in milliseconds |
| `Timestamp` | `DateTimeOffset` | UTC time the request was sent |

### `DebugHandler`

| Member | Description |
|--------|-------------|
| `DebugHandler()` | Writes formatted log lines to `Console` |
| `DebugHandler(Action<RequestLog>)` | Invokes the provided callback for every request |

## License

MIT
