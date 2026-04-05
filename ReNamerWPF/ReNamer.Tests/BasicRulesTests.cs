using ReNamer.Models;
using ReNamer.Rules;

namespace ReNamer.Tests;

public class ReplaceRuleTests
{
    private static RenFile MakeFile(string name = "test.txt") => new($@"C:\{name}");

    [Fact]
    public void Replace_All_Occurrences()
    {
        var rule = new ReplaceRule { FindText = "a", ReplaceText = "b", Occurrence = ReplaceOccurrence.All };
        Assert.Equal("bbc.txt", rule.Execute("aac.txt", MakeFile()));
    }

    [Fact]
    public void Replace_First_Only()
    {
        var rule = new ReplaceRule { FindText = "a", ReplaceText = "b", Occurrence = ReplaceOccurrence.First };
        Assert.Equal("bac.txt", rule.Execute("aac.txt", MakeFile()));
    }

    [Fact]
    public void Replace_Last_Only()
    {
        var rule = new ReplaceRule { FindText = "a", ReplaceText = "b", Occurrence = ReplaceOccurrence.Last };
        Assert.Equal("abc.txt", rule.Execute("aac.txt", MakeFile()));
    }

    [Fact]
    public void Replace_CaseSensitive()
    {
        var rule = new ReplaceRule { FindText = "A", ReplaceText = "X", CaseSensitive = true };
        Assert.Equal("hello.txt", rule.Execute("hello.txt", MakeFile()));
        Assert.Equal("Xello.txt", rule.Execute("Aello.txt", MakeFile()));
    }

    [Fact]
    public void Replace_CaseInsensitive()
    {
        var rule = new ReplaceRule { FindText = "HELLO", ReplaceText = "world", CaseSensitive = false };
        Assert.Equal("world.txt", rule.Execute("hello.txt", MakeFile()));
    }

    [Fact]
    public void Replace_SkipExtension_True()
    {
        var rule = new ReplaceRule { FindText = "t", ReplaceText = "X", SkipExtension = true };
        Assert.Equal("XesX.txt", rule.Execute("test.txt", MakeFile()));
    }

    [Fact]
    public void Replace_SkipExtension_False()
    {
        var rule = new ReplaceRule { FindText = "t", ReplaceText = "X", SkipExtension = false };
        Assert.Equal("XesX.XxX", rule.Execute("test.txt", MakeFile()));
    }

    [Fact]
    public void Replace_EmptyFind_NoChange()
    {
        var rule = new ReplaceRule { FindText = "", ReplaceText = "X" };
        Assert.Equal("test.txt", rule.Execute("test.txt", MakeFile()));
    }

    [Fact]
    public void Replace_Multiple_WithSingleReplaceText()
    {
        var rule = new ReplaceRule
        {
            FindText = "foo|bar",
            ReplaceText = "X",
            Occurrence = ReplaceOccurrence.All
        };
        Assert.Equal("X_X.txt", rule.Execute("foo_bar.txt", MakeFile()));
    }

    [Fact]
    public void Replace_Multiple_WholeWordsOnly()
    {
        var rule = new ReplaceRule
        {
            FindText = "cat|dog",
            ReplaceText = "pet|pet",
            WholeWordsOnly = true
        };
        Assert.Equal("catdog pet.txt", rule.Execute("catdog dog.txt", MakeFile()));
    }

    [Fact]
    public void Replace_Wildcards_CaseSensitive()
    {
        var rule = new ReplaceRule
        {
            FindText = "a*",
            ReplaceText = "X",
            UseWildcards = true,
            CaseSensitive = true
        };
        Assert.Equal("Abc.txt", rule.Execute("Abc.txt", MakeFile()));
        Assert.Equal("X.txt", rule.Execute("abc.txt", MakeFile()));
    }

    [Fact]
    public void Replace_Regex_Last_WithGroups()
    {
        var rule = new ReplaceRule
        {
            FindText = "(a)(b)",
            ReplaceText = "$2$1",
            UseRegex = true,
            Occurrence = ReplaceOccurrence.Last
        };
        Assert.Equal("ab ba.txt", rule.Execute("ab ab.txt", MakeFile()));
    }

    [Fact]
    public void Replace_Multiple_EscapedSeparatorLiteral()
    {
        var rule = new ReplaceRule
        {
            FindText = @"\|",
            ReplaceText = "_"
        };
        Assert.Equal("a_b.txt", rule.Execute("a|b.txt", MakeFile()));
    }

    [Fact]
    public void Replace_Multiple_First_GlobalOnce()
    {
        var rule = new ReplaceRule
        {
            FindText = "a|b",
            ReplaceText = "X|Y",
            Occurrence = ReplaceOccurrence.First
        };
        Assert.Equal("Xbaba.txt", rule.Execute("ababa.txt", MakeFile()));
    }

    [Fact]
    public void Replace_Multiple_Last_GlobalOnce()
    {
        var rule = new ReplaceRule
        {
            FindText = "a|b",
            ReplaceText = "X|Y",
            Occurrence = ReplaceOccurrence.Last
        };
        Assert.Equal("ababX.txt", rule.Execute("ababa.txt", MakeFile()));
    }
}

public class InsertRuleTests
{
    private static RenFile MakeFile(string name = "test.txt") => new($@"C:\{name}");

    [Fact]
    public void Insert_Prefix()
    {
        var rule = new InsertRule { InsertText = "pre_", InsertPosition = InsertPositionType.Prefix };
        Assert.Equal("pre_test.txt", rule.Execute("test.txt", MakeFile()));
    }

    [Fact]
    public void Insert_Suffix()
    {
        var rule = new InsertRule { InsertText = "_suf", InsertPosition = InsertPositionType.Suffix };
        Assert.Equal("test_suf.txt", rule.Execute("test.txt", MakeFile()));
    }

    [Fact]
    public void Insert_Position()
    {
        var rule = new InsertRule { InsertText = "XX", InsertPosition = InsertPositionType.Position, Position = 3 };
        Assert.Equal("teXXst.txt", rule.Execute("test.txt", MakeFile()));
    }

    [Fact]
    public void Insert_RightToLeft()
    {
        var rule = new InsertRule { InsertText = "XX", InsertPosition = InsertPositionType.Position, Position = 2, RightToLeft = true };
        Assert.Equal("teXXst.txt", rule.Execute("test.txt", MakeFile()));
    }

    [Fact]
    public void Insert_AfterText()
    {
        var rule = new InsertRule { InsertText = "123", InsertPosition = InsertPositionType.AfterText, AfterText = "te" };
        Assert.Equal("te123st.txt", rule.Execute("test.txt", MakeFile()));
    }

    [Fact]
    public void Insert_BeforeText()
    {
        var rule = new InsertRule { InsertText = "123", InsertPosition = InsertPositionType.BeforeText, BeforeText = "st" };
        Assert.Equal("te123st.txt", rule.Execute("test.txt", MakeFile()));
    }

    [Fact]
    public void Insert_ReplaceCurrentName()
    {
        var rule = new InsertRule { InsertText = "newname", InsertPosition = InsertPositionType.ReplaceCurrentName };
        Assert.Equal("newname.txt", rule.Execute("test.txt", MakeFile()));
    }

    [Fact]
    public void Insert_EmptyText_NoChange()
    {
        var rule = new InsertRule { InsertText = "", InsertPosition = InsertPositionType.Prefix };
        Assert.Equal("test.txt", rule.Execute("test.txt", MakeFile()));
    }
}

public class DeleteRuleTests
{
    private static RenFile MakeFile(string name = "test.txt") => new($@"C:\{name}");

    [Fact]
    public void Delete_FromPosition_UntilCount()
    {
        var rule = new DeleteRule { FromPosition = 2, UntilCount = 2 };
        Assert.Equal("tt.txt", rule.Execute("test.txt", MakeFile()));
    }

    [Fact]
    public void Delete_FromPosition_TillEnd()
    {
        var rule = new DeleteRule { FromPosition = 3, UntilType = DeleteUntilType.TillEnd };
        Assert.Equal("te.txt", rule.Execute("test.txt", MakeFile()));
    }

    [Fact]
    public void Delete_CurrentName()
    {
        var rule = new DeleteRule { DeleteCurrentName = true };
        Assert.Equal(".txt", rule.Execute("test.txt", MakeFile()));
    }

    [Fact]
    public void Delete_RightToLeft()
    {
        var rule = new DeleteRule { FromPosition = 1, UntilCount = 2, RightToLeft = true };
        Assert.Equal("te.txt", rule.Execute("test.txt", MakeFile()));
    }

    [Fact]
    public void Delete_FromDelimiter()
    {
        var rule = new DeleteRule { FromType = DeleteFromType.Delimiter, FromDelimiter = "-", UntilType = DeleteUntilType.TillEnd };
        Assert.Equal("hello.txt", rule.Execute("hello-world.txt", MakeFile()));
    }

    [Fact]
    public void Delete_LeaveDelimiter()
    {
        var rule = new DeleteRule
        {
            FromType = DeleteFromType.Delimiter, FromDelimiter = "-",
            UntilType = DeleteUntilType.TillEnd, LeaveDelimiter = true
        };
        Assert.Equal("hello-.txt", rule.Execute("hello-world.txt", MakeFile()));
    }

    [Fact]
    public void Delete_TextRemove_MultiplePatterns()
    {
        var rule = new DeleteRule
        {
            Mode = DeleteMode.TextRemove,
            RemovePattern = "foo|bar"
        };
        Assert.Equal("_.txt", rule.Execute("foo_bar.txt", MakeFile()));
    }

    [Fact]
    public void Delete_TextRemove_EscapedSeparatorLiteral()
    {
        var rule = new DeleteRule
        {
            Mode = DeleteMode.TextRemove,
            RemovePattern = @"\|"
        };
        Assert.Equal("ab.txt", rule.Execute("a|b.txt", MakeFile()));
    }

    [Fact]
    public void Delete_TextRemove_Multiple_First_GlobalOnce()
    {
        var rule = new DeleteRule
        {
            Mode = DeleteMode.TextRemove,
            RemovePattern = "a|b",
            RemoveOccurrence = RemoveOccurrence.First
        };
        Assert.Equal("baba.txt", rule.Execute("ababa.txt", MakeFile()));
    }
}

public class RemoveRuleTests
{
    private static RenFile MakeFile(string name = "test.txt") => new($@"C:\{name}");

    [Fact]
    public void Remove_All()
    {
        var rule = new RemoveRule { Pattern = "a", Occurrence = RemoveOccurrence.All };
        Assert.Equal("bcbc.txt", rule.Execute("abcabc.txt", MakeFile()));
    }

    [Fact]
    public void Remove_First()
    {
        var rule = new RemoveRule { Pattern = "a", Occurrence = RemoveOccurrence.First };
        Assert.Equal("bcabc.txt", rule.Execute("abcabc.txt", MakeFile()));
    }

    [Fact]
    public void Remove_Last()
    {
        var rule = new RemoveRule { Pattern = "a", Occurrence = RemoveOccurrence.Last };
        Assert.Equal("abcbc.txt", rule.Execute("abcabc.txt", MakeFile()));
    }

    [Fact]
    public void Remove_CaseSensitive()
    {
        var rule = new RemoveRule { Pattern = "A", CaseSensitive = true };
        Assert.Equal("abcbc.txt", rule.Execute("abcAbc.txt", MakeFile()));
    }

    [Fact]
    public void Remove_EmptyPattern_NoChange()
    {
        var rule = new RemoveRule { Pattern = "" };
        Assert.Equal("test.txt", rule.Execute("test.txt", MakeFile()));
    }

    [Fact]
    public void Remove_Multiple_WithWildcards()
    {
        var rule = new RemoveRule
        {
            Pattern = "ab|x?",
            UseWildcards = true,
            Occurrence = RemoveOccurrence.All
        };
        Assert.Equal("c.txt", rule.Execute("abcxx.txt", MakeFile()));
    }

    [Fact]
    public void Remove_Wildcards_CaseSensitive()
    {
        var rule = new RemoveRule
        {
            Pattern = "a*",
            UseWildcards = true,
            CaseSensitive = true
        };
        Assert.Equal("Abc.txt", rule.Execute("Abc.txt", MakeFile()));
        Assert.Equal(".txt", rule.Execute("abc.txt", MakeFile()));
    }

    [Fact]
    public void Remove_Multiple_EscapedSeparatorLiteral()
    {
        var rule = new RemoveRule
        {
            Pattern = @"\|",
            Occurrence = RemoveOccurrence.All
        };
        Assert.Equal("ab.txt", rule.Execute("a|b.txt", MakeFile()));
    }

    [Fact]
    public void Remove_Multiple_First_GlobalOnce()
    {
        var rule = new RemoveRule
        {
            Pattern = "a|b",
            Occurrence = RemoveOccurrence.First
        };
        Assert.Equal("baba.txt", rule.Execute("ababa.txt", MakeFile()));
    }

    [Fact]
    public void Remove_Multiple_Last_GlobalOnce()
    {
        var rule = new RemoveRule
        {
            Pattern = "a|b",
            Occurrence = RemoveOccurrence.Last
        };
        Assert.Equal("abab.txt", rule.Execute("ababa.txt", MakeFile()));
    }
}
