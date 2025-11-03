using Bunit;
using System;
using Xunit;

public class Class1 : TestContext
{
    public class UserService
    {
        public bool IsAdult(int age) => age >= 19;
    }

	[Fact]
	public void Method()
	{
	    var result = _sut.IsAdult(20);
        Assert.True(result);
    }

    private readonly UserService _sut = new();

    [Theory]
    [InlineData(19, true)]
    [InlineData(18, false)]
    public void IsAdult_ReturnsExpected(int age, bool expected)
    {
        var ok = _sut.IsAdult(age);
        Assert.Equal(expected, ok);
    }
}
