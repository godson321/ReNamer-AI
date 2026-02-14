using ReNamer.Models;
using ReNamer.Rules;

namespace ReNamer.Tests;

public class CaseRuleTests
{
    private static RenFile MakeFile() => new(@"C:\test.txt");

    [Fact]
    public void Case_Capitalize()
    {
        var rule = new CaseRule { CaseType = CaseType.Capitalize };
        Assert.Equal("Hello World.txt", rule.Execute("hello world.txt", MakeFile()));
    }

    [Fact]
    public void Case_Lower()
    {
        var rule = new CaseRule { CaseType = CaseType.Lower };
        Assert.Equal("hello.txt", rule.Execute("HELLO.txt", MakeFile()));
    }

    [Fact]
    public void Case_Upper()
    {
        var rule = new CaseRule { CaseType = CaseType.Upper };
        Assert.Equal("HELLO.txt", rule.Execute("hello.txt", MakeFile()));
    }

    [Fact]
    public void Case_Invert()
    {
        var rule = new CaseRule { CaseType = CaseType.Invert };
        Assert.Equal("hELLO.txt", rule.Execute("Hello.txt", MakeFile()));
    }

    [Fact]
    public void Case_FirstLetter()
    {
        var rule = new CaseRule { CaseType = CaseType.FirstLetter };
        Assert.Equal("Hello world.txt", rule.Execute("hello world.txt", MakeFile()));
    }

    [Fact]
    public void Case_Sentence()
    {
        var rule = new CaseRule { CaseType = CaseType.Sentence };
        Assert.Equal("Hello world.txt", rule.Execute("HELLO WORLD.txt", MakeFile()));
    }

    [Fact]
    public void Case_ExtensionAlwaysLower()
    {
        var rule = new CaseRule { CaseType = CaseType.None, ExtensionAlwaysLower = true };
        Assert.Equal("Test.txt", rule.Execute("Test.TXT", MakeFile()));
    }

    [Fact]
    public void Case_ExtensionAlwaysUpper()
    {
        var rule = new CaseRule { CaseType = CaseType.None, ExtensionAlwaysUpper = true };
        Assert.Equal("Test.TXT", rule.Execute("Test.txt", MakeFile()));
    }

    [Fact]
    public void Case_ForceCase()
    {
        var rule = new CaseRule { CaseType = CaseType.Lower, ForceCase = true, ForceCaseText = "DVD,CD" };
        var result = rule.Execute("my dvd collection.txt", MakeFile());
        Assert.Contains("DVD", result);
    }

    [Fact]
    public void Case_SkipExtension_False()
    {
        var rule = new CaseRule { CaseType = CaseType.Upper, SkipExtension = false };
        Assert.Equal("HELLO.TXT", rule.Execute("hello.txt", MakeFile()));
    }

    [Fact]
    public void Case_None_NoChange()
    {
        var rule = new CaseRule { CaseType = CaseType.None };
        Assert.Equal("Hello World.txt", rule.Execute("Hello World.txt", MakeFile()));
    }

    [Fact]
    public void Case_PreserveCase_KeepsAllCaps()
    {
        var rule = new CaseRule { CaseType = CaseType.Lower, PreserveCase = true };
        var result = rule.Execute("MY DVD COLLECTION.txt", MakeFile());
        Assert.Contains("DVD", result);
    }

    [Fact]
    public void Case_ForceCase_MultipleFragments()
    {
        var rule = new CaseRule { CaseType = CaseType.Lower, ForceCase = true, ForceCaseText = "DVD,CD,USB" };
        var result = rule.Execute("my dvd and cd and usb drive.txt", MakeFile());
        Assert.Contains("DVD", result);
        Assert.Contains("CD", result);
        Assert.Contains("USB", result);
    }

    [Fact]
    public void Case_ExtensionLowerAndUpper_LowerWins()
    {
        var rule = new CaseRule { CaseType = CaseType.None, ExtensionAlwaysLower = true, ExtensionAlwaysUpper = true };
        Assert.Equal("Test.txt", rule.Execute("Test.TXT", MakeFile()));
    }

    [Fact]
    public void Case_EmptyFileName()
    {
        var rule = new CaseRule { CaseType = CaseType.Upper };
        Assert.Equal(".txt", rule.Execute(".txt", MakeFile()));
    }
}

public class SerializeRuleTests
{
    private static RenFile MakeFile(string name = "test.txt", string folder = @"C:\") => new(folder + name);

    [Fact]
    public void Serialize_Basic_Prefix()
    {
        var rule = new SerializeRule { StartNumber = 1, Step = 1, Padding = 3 };
        rule.Reset();
        Assert.Equal("001test.txt", rule.Execute("test.txt", MakeFile()));
        Assert.Equal("002file.txt", rule.Execute("file.txt", MakeFile("file.txt")));
        Assert.Equal("003doc.txt", rule.Execute("doc.txt", MakeFile("doc.txt")));
    }

    [Fact]
    public void Serialize_Suffix()
    {
        var rule = new SerializeRule { StartNumber = 10, Step = 5, InsertWhere = SerializePosition.Suffix, Padding = 2 };
        rule.Reset();
        Assert.Equal("test10.txt", rule.Execute("test.txt", MakeFile()));
        Assert.Equal("file15.txt", rule.Execute("file.txt", MakeFile("file.txt")));
    }

    [Fact]
    public void Serialize_Hexadecimal()
    {
        var rule = new SerializeRule { StartNumber = 255, NumberingSystem = "Hexadecimal", Padding = 2 };
        rule.Reset();
        Assert.Equal("FFtest.txt", rule.Execute("test.txt", MakeFile()));
    }

    [Fact]
    public void Serialize_Repeat()
    {
        var rule = new SerializeRule { StartNumber = 1, Step = 1, Repeat = 2, Padding = 1 };
        rule.Reset();
        Assert.Equal("1a.txt", rule.Execute("a.txt", MakeFile("a.txt")));
        Assert.Equal("1b.txt", rule.Execute("b.txt", MakeFile("b.txt")));
        Assert.Equal("2c.txt", rule.Execute("c.txt", MakeFile("c.txt")));
    }

    [Fact]
    public void Serialize_ReplaceName()
    {
        var rule = new SerializeRule { StartNumber = 1, InsertWhere = SerializePosition.ReplaceCurrentName, Padding = 3 };
        rule.Reset();
        Assert.Equal("001.txt", rule.Execute("test.txt", MakeFile()));
    }

    [Fact]
    public void Serialize_Octal()
    {
        var rule = new SerializeRule { StartNumber = 8, NumberingSystem = "Octal", Padding = 2 };
        rule.Reset();
        Assert.Equal("10test.txt", rule.Execute("test.txt", MakeFile()));
    }

    [Fact]
    public void Serialize_Binary()
    {
        var rule = new SerializeRule { StartNumber = 5, NumberingSystem = "Binary", Padding = 4 };
        rule.Reset();
        Assert.Equal("0101test.txt", rule.Execute("test.txt", MakeFile()));
    }

    [Fact]
    public void Serialize_Custom()
    {
        var rule = new SerializeRule { StartNumber = 0, NumberingSystem = "Custom", CustomNumberingSymbols = "abc", Padding = 1 };
        rule.Reset();
        Assert.Equal("atest.txt", rule.Execute("test.txt", MakeFile()));
        Assert.Equal("bfile.txt", rule.Execute("file.txt", MakeFile("file.txt")));
        Assert.Equal("cdata.txt", rule.Execute("data.txt", MakeFile("data.txt")));
    }

    [Fact]
    public void Serialize_PadToLength()
    {
        var rule = new SerializeRule { StartNumber = 5, PadToLength = true, PadToLengthValue = 5 };
        rule.Reset();
        Assert.Equal("00005test.txt", rule.Execute("test.txt", MakeFile()));
    }

    [Fact]
    public void Serialize_Position()
    {
        var rule = new SerializeRule { StartNumber = 1, InsertWhere = SerializePosition.Position, PositionValue = 3, Padding = 2 };
        rule.Reset();
        Assert.Equal("te01st.txt", rule.Execute("test.txt", MakeFile()));
    }

    [Fact]
    public void Serialize_PrefixSuffix()
    {
        var rule = new SerializeRule { StartNumber = 1, Prefix = "[", Suffix = "]", Padding = 2 };
        rule.Reset();
        Assert.Equal("[01]test.txt", rule.Execute("test.txt", MakeFile()));
    }

    [Fact]
    public void Serialize_ResetEvery()
    {
        var rule = new SerializeRule { StartNumber = 1, Step = 1, ResetEvery = true, ResetEveryCount = 2, Padding = 1 };
        rule.Reset();
        Assert.Equal("1a.txt", rule.Execute("a.txt", MakeFile("a.txt")));
        Assert.Equal("2b.txt", rule.Execute("b.txt", MakeFile("b.txt")));
        Assert.Equal("1c.txt", rule.Execute("c.txt", MakeFile("c.txt")));
    }

    [Fact]
    public void Serialize_ResetIfFolderChanges()
    {
        var rule = new SerializeRule { StartNumber = 1, ResetIfFolderChanges = true, Padding = 1 };
        rule.Reset();
        Assert.Equal("1a.txt", rule.Execute("a.txt", MakeFile("a.txt", @"C:\")));
        Assert.Equal("2b.txt", rule.Execute("b.txt", MakeFile("b.txt", @"C:\")));
        Assert.Equal("1c.txt", rule.Execute("c.txt", MakeFile("c.txt", @"D:\")));
    }

    [Fact]
    public void Serialize_ResetIfFileNameChanges()
    {
        var rule = new SerializeRule { StartNumber = 1, ResetIfFileNameChanges = true, Padding = 1 };
        rule.Reset();
        Assert.Equal("1a.txt", rule.Execute("a.txt", MakeFile("a.txt")));
        Assert.Equal("2a.txt", rule.Execute("a.txt", MakeFile("a.txt")));
        Assert.Equal("1b.txt", rule.Execute("b.txt", MakeFile("b.txt")));
    }

    [Fact]
    public void Serialize_NegativeNumber()
    {
        var rule = new SerializeRule { StartNumber = -5, Step = 1, Padding = 2 };
        rule.Reset();
        Assert.Equal("-05test.txt", rule.Execute("test.txt", MakeFile()));
        Assert.Equal("-04file.txt", rule.Execute("file.txt", MakeFile("file.txt")));
    }
}

public class ExtensionRuleTests
{
    private static RenFile MakeFile() => new(@"C:\test.txt");

    [Fact]
    public void Extension_Replace()
    {
        var rule = new ExtensionRule { NewExtension = "doc" };
        Assert.Equal("test.doc", rule.Execute("test.txt", MakeFile()));
    }

    [Fact]
    public void Extension_Append()
    {
        var rule = new ExtensionRule { NewExtension = "bak", AppendToOriginal = true };
        Assert.Equal("test.txt.bak", rule.Execute("test.txt", MakeFile()));
    }

    [Fact]
    public void Extension_RemoveDuplicate()
    {
        var rule = new ExtensionRule { NewExtensionEnabled = false, RemoveDuplicateExtensions = true };
        Assert.Equal("test.txt", rule.Execute("test.txt.txt", MakeFile()));
    }

    [Fact]
    public void Extension_WithDot()
    {
        var rule = new ExtensionRule { NewExtension = ".pdf" };
        Assert.Equal("test.pdf", rule.Execute("test.txt", MakeFile()));
    }

    [Fact]
    public void Extension_EmptyExtension_RemovesExtension()
    {
        var rule = new ExtensionRule { NewExtension = "" };
        Assert.Equal("test.", rule.Execute("test.txt", MakeFile()));
    }

    [Fact]
    public void Extension_CaseSensitive_RemoveDuplicate()
    {
        var rule = new ExtensionRule { NewExtensionEnabled = false, RemoveDuplicateExtensions = true, CaseSensitive = true };
        Assert.Equal("test.txt.TXT", rule.Execute("test.txt.TXT", MakeFile()));
    }

    [Fact]
    public void Extension_CaseInsensitive_RemoveDuplicate()
    {
        var rule = new ExtensionRule { NewExtensionEnabled = false, RemoveDuplicateExtensions = true, CaseSensitive = false };
        Assert.Equal("test.txt", rule.Execute("test.txt.TXT", MakeFile()));
    }

    [Fact]
    public void Extension_MultipleExtensions_RemoveDuplicate()
    {
        var rule = new ExtensionRule { NewExtensionEnabled = false, RemoveDuplicateExtensions = true };
        Assert.Equal("test.txt.bak", rule.Execute("test.txt.txt.bak.bak", MakeFile()));
    }

    [Fact]
    public void Extension_NewExtensionDisabled_NoChange()
    {
        var rule = new ExtensionRule { NewExtensionEnabled = false, NewExtension = "pdf" };
        Assert.Equal("test.txt", rule.Execute("test.txt", MakeFile()));
    }
}

public class RegexRuleTests
{
    private static RenFile MakeFile() => new(@"C:\test.txt");

    [Fact]
    public void Regex_BasicReplace()
    {
        var rule = new RegexRule { Expression = @"\d+", ReplaceText = "NUM" };
        Assert.Equal("fileNUM.txt", rule.Execute("file123.txt", MakeFile()));
    }

    [Fact]
    public void Regex_CaptureGroup()
    {
        var rule = new RegexRule { Expression = @"(\w+)-(\w+)", ReplaceText = "$2_$1" };
        Assert.Equal("world_hello.txt", rule.Execute("hello-world.txt", MakeFile()));
    }

    [Fact]
    public void Regex_CaseInsensitive()
    {
        var rule = new RegexRule { Expression = "HELLO", ReplaceText = "hi", CaseSensitive = false };
        Assert.Equal("hi.txt", rule.Execute("Hello.txt", MakeFile()));
    }

    [Fact]
    public void Regex_CaseSensitive_NoMatch()
    {
        var rule = new RegexRule { Expression = "HELLO", ReplaceText = "hi", CaseSensitive = true };
        Assert.Equal("Hello.txt", rule.Execute("Hello.txt", MakeFile()));
    }

    [Fact]
    public void Regex_InvalidExpression_NoChange()
    {
        var rule = new RegexRule { Expression = "[invalid", ReplaceText = "X" };
        Assert.Equal("test.txt", rule.Execute("test.txt", MakeFile()));
    }

    [Fact]
    public void Regex_EmptyExpression_NoChange()
    {
        var rule = new RegexRule { Expression = "", ReplaceText = "X" };
        Assert.Equal("test.txt", rule.Execute("test.txt", MakeFile()));
    }
}
