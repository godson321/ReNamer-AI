using ReNamer.Models;
using ReNamer.Rules;

namespace ReNamer.Tests;

public class JavaScriptRuleTests
{
    private static RenFile MakeFile(string name = "test.txt") => new($@"C:\{name}");

    [Fact]
    public void JavaScript_Assignment_Works()
    {
        var rule = new JavaScriptRule
        {
            ScriptText = "Result = UpperCase(Name);"
        };

        var result = rule.Execute("photo.jpg", MakeFile("photo.jpg"));
        Assert.Equal("PHOTO.jpg", result);
    }

    [Fact]
    public void JavaScript_CounterFormatting_Works()
    {
        var rule = new JavaScriptRule
        {
            ScriptText = "Result = Name + '_' + Counter;",
            CounterStart = 7,
            CounterStep = 2,
            CounterDigits = 3
        };
        rule.Reset();

        var first = rule.Execute("item.txt", MakeFile("item.txt"));
        var second = rule.Execute("item.txt", MakeFile("item.txt"));

        Assert.Equal("item_007.txt", first);
        Assert.Equal("item_009.txt", second);
    }

    [Fact]
    public void JavaScript_LegacyPascalAssignment_IsNormalized()
    {
        var rule = new JavaScriptRule
        {
            ScriptText = "Result := Name + '_' + Counter;"
        };
        rule.Reset();

        var result = rule.Execute("movie.mkv", MakeFile("movie.mkv"));
        Assert.Equal("movie_1.mkv", result);
    }

    [Fact]
    public void JavaScript_InvalidScript_FallsBackToOriginalName()
    {
        var rule = new JavaScriptRule
        {
            ScriptText = "if ("
        };

        var result = rule.Execute("original.txt", MakeFile("original.txt"));
        Assert.Equal("original.txt", result);
    }
}
