using Xunit;
using Philiprehberger.HttpDebugHandler;

namespace Philiprehberger.HttpDebugHandler.Tests;

public class RequestLogTests
{
    [Fact]
    public void Constructor_SetsRequiredProperties()
    {
        var uri = new Uri("https://example.com/api");
        var timestamp = DateTimeOffset.UtcNow;

        var log = new RequestLog("GET", uri, 200, 42, timestamp);

        Assert.Equal("GET", log.Method);
        Assert.Equal(uri, log.RequestUri);
        Assert.Equal(200, log.StatusCode);
        Assert.Equal(42, log.ElapsedMilliseconds);
        Assert.Equal(timestamp, log.Timestamp);
    }

    [Fact]
    public void OptionalProperties_DefaultToNull()
    {
        var log = new RequestLog("POST", null, 500, 100, DateTimeOffset.UtcNow);

        Assert.Null(log.RequestBody);
        Assert.Null(log.ResponseBody);
        Assert.Null(log.RequestHeaders);
        Assert.Null(log.ResponseHeaders);
    }

    [Fact]
    public void InitProperties_CanBeSet()
    {
        var headers = new Dictionary<string, string> { ["Content-Type"] = "application/json" };

        var log = new RequestLog("PUT", null, 200, 10, DateTimeOffset.UtcNow)
        {
            RequestBody = "{\"key\":\"value\"}",
            ResponseBody = "{\"ok\":true}",
            RequestHeaders = headers,
            ResponseHeaders = headers
        };

        Assert.Equal("{\"key\":\"value\"}", log.RequestBody);
        Assert.Equal("{\"ok\":true}", log.ResponseBody);
        Assert.NotNull(log.RequestHeaders);
        Assert.NotNull(log.ResponseHeaders);
    }
}
