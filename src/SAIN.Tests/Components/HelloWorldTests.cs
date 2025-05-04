using Xunit;

public class HelloWorldTests
{
    [Fact]
    public void HelloWorld_ReturnsExpectedMessage()
    {
        var result = "Hello, World!";
        Assert.Equal("Hello, World!", result);
    }
}