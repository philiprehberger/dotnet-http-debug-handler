using Xunit;
using Philiprehberger.HttpDebugHandler;

namespace Philiprehberger.HttpDebugHandler.Tests;

public class DebugHandlerTests
{
    [Fact]
    public void Constructor_WithCallback_DoesNotThrow()
    {
        var handler = new DebugHandler(log => { });
        Assert.NotNull(handler);
    }

    [Fact]
    public void Constructor_WithNullCallback_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new DebugHandler(null!));
    }

    [Fact]
    public void DefaultConstructor_DoesNotThrow()
    {
        var handler = new DebugHandler();
        Assert.NotNull(handler);
    }

    [Fact]
    public void CaptureProperties_DefaultToFalse()
    {
        var handler = new DebugHandler(log => { });

        Assert.False(handler.CaptureRequestBody);
        Assert.False(handler.CaptureResponseBody);
        Assert.False(handler.CaptureHeaders);
    }

    [Fact]
    public void MaxBodyCaptureSize_DefaultsTo65536()
    {
        var handler = new DebugHandler(log => { });

        Assert.Equal(65_536, handler.MaxBodyCaptureSize);
    }

    [Fact]
    public void SlowRequestThreshold_DefaultsToNull()
    {
        var handler = new DebugHandler(log => { });

        Assert.Null(handler.SlowRequestThreshold);
        Assert.Null(handler.OnSlowRequest);
    }
}
