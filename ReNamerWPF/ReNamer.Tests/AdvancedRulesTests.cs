using ReNamer.Models;
using ReNamer.Rules;

namespace ReNamer.Tests;

public class PaddingRuleTests
{
    private static RenFile MakeFile() => new(@"C:\test.txt");

    [Fact]
    public void Padding_AddZero()
    {
        var rule = new PaddingRule { AddZeroPadding = true, ZeroPaddingLength = 4 };
        Assert.Equal("file0005.txt", rule.Execute("file5.txt", MakeFile()));
    }

    [Fact]
    public void Padding_AddZero_MultipleNumbers()
    {
        var rule = new PaddingRule { AddZeroPadding = true, ZeroPaddingLength = 3 };
        Assert.Equal("track001-of-010.txt", rule.Execute("track1-of-10.txt", MakeFile()));
    }

    [Fact]
    public void Padding_RemoveZero()
    {
        var rule = new PaddingRule { RemoveZeroPadding = true };
        Assert.Equal("file5.txt", rule.Execute("file05.txt", MakeFile()));
    }

    [Fact]
    public void Padding_AddText_Left()
    {
        var rule = new PaddingRule { AddTextPadding = true, TextPaddingLength = 10, PaddingCharacters = "_" };
        Assert.Equal("______test.txt", rule.Execute("test.txt", MakeFile()));
    }

    [Fact]
    public void Padding_AddText_Right()
    {
        var rule = new PaddingRule { AddTextPadding = true, TextPaddingLength = 8, PaddingCharacters = "x", TextPaddingPosition = PaddingPosition.Right };
        Assert.Equal("testxxxx.txt", rule.Execute("test.txt", MakeFile()));
    }
}

public class StripRuleTests
{
    private static RenFile MakeFile() => new(@"C:\test.txt");

    [Fact]
    public void Strip_Digits()
    {
        var rule = new StripRule { StripDigits = true };
        Assert.Equal("file.txt", rule.Execute("file123.txt", MakeFile()));
    }

    [Fact]
    public void Strip_EnglishLetters()
    {
        var rule = new StripRule { StripEnglishLetters = true };
        Assert.Equal("123.txt", rule.Execute("abc123.txt", MakeFile()));
    }

    [Fact]
    public void Strip_Brackets()
    {
        var rule = new StripRule { StripBrackets = true };
        // () are stripped, but digits and spaces remain
        Assert.Equal("file 1.txt", rule.Execute("file (1).txt", MakeFile()));
    }

    [Fact]
    public void Strip_Leading()
    {
        var rule = new StripRule { StripDigits = true, Where = StripWhere.Leading };
        Assert.Equal("abc.txt", rule.Execute("123abc.txt", MakeFile()));
    }

    [Fact]
    public void Strip_Trailing()
    {
        var rule = new StripRule { StripDigits = true, Where = StripWhere.Trailing };
        Assert.Equal("abc.txt", rule.Execute("abc123.txt", MakeFile()));
    }

    [Fact]
    public void Strip_AllExcept()
    {
        var rule = new StripRule { StripDigits = true, StripAllExceptSelected = true };
        // Keep only digits, strip everything else
        Assert.Equal("123.txt", rule.Execute("abc123def.txt", MakeFile()));
    }

    [Fact]
    public void Strip_UserDefined()
    {
        var rule = new StripRule { StripUserDefined = true, UserDefinedChars = "@#" };
        Assert.Equal("hello.txt", rule.Execute("he@ll#o.txt", MakeFile()));
    }
}

public class CleanUpRuleTests
{
    private static RenFile MakeFile() => new(@"C:\test.txt");

    [Fact]
    public void CleanUp_StripRoundBrackets()
    {
        // FixSpaces=true (default) trims the extra space
        var rule = new CleanUpRule { StripRoundBrackets = true };
        Assert.Equal("file.txt", rule.Execute("file (copy).txt", MakeFile()));
    }

    [Fact]
    public void CleanUp_StripSquareBrackets()
    {
        var rule = new CleanUpRule { StripSquareBrackets = true };
        Assert.Equal("file.txt", rule.Execute("file [v2].txt", MakeFile()));
    }

    [Fact]
    public void CleanUp_ReplaceDot()
    {
        var rule = new CleanUpRule { ReplaceDotWithSpace = true };
        Assert.Equal("hello world.txt", rule.Execute("hello.world.txt", MakeFile()));
    }

    [Fact]
    public void CleanUp_ReplaceDot_SkipVersions()
    {
        // "app.1" - dot is followed by digit, so NOT replaced with SkipVersions
        // Only dots not adjacent to digits are replaced
        var rule = new CleanUpRule { ReplaceDotWithSpace = true, SkipVersionNumbers = true };
        Assert.Equal("app.1.2.3.txt", rule.Execute("app.1.2.3.txt", MakeFile()));
    }

    [Fact]
    public void CleanUp_ReplaceUnderscore()
    {
        var rule = new CleanUpRule { ReplaceUnderscoreWithSpace = true };
        Assert.Equal("hello world.txt", rule.Execute("hello_world.txt", MakeFile()));
    }

    [Fact]
    public void CleanUp_FixSpaces()
    {
        var rule = new CleanUpRule { FixSpaces = true };
        Assert.Equal("hello world.txt", rule.Execute("  hello   world  .txt", MakeFile()));
    }

    [Fact]
    public void CleanUp_CamelCase()
    {
        var rule = new CleanUpRule { InsertSpaceBeforeCapitals = true };
        Assert.Equal("Hello World.txt", rule.Execute("HelloWorld.txt", MakeFile()));
    }

    [Fact]
    public void CleanUp_StripUnicodeMarks()
    {
        var rule = new CleanUpRule { StripUnicodeMarks = true };
        Assert.Equal("cafe.txt", rule.Execute("café.txt", MakeFile()));
    }
}

public class TransliterateRuleTests
{
    private static RenFile MakeFile() => new(@"C:\test.txt");

    [Fact]
    public void Transliterate_Forward()
    {
        var rule = new TransliterateRule { Alphabet = "ä=ae\nö=oe\nü=ue" };
        Assert.Equal("Muenchen.txt", rule.Execute("München.txt", MakeFile()));
    }

    [Fact]
    public void Transliterate_Backward()
    {
        var rule = new TransliterateRule { Alphabet = "ä=ae\nö=oe\nü=ue", DirectionForward = false };
        Assert.Equal("München.txt", rule.Execute("Muenchen.txt", MakeFile()));
    }

    [Fact]
    public void Transliterate_AutoCase()
    {
        var rule = new TransliterateRule { Alphabet = "ä=ae", AutoCaseAdjustment = true };
        Assert.Equal("Aepfel.txt", rule.Execute("Äpfel.txt", MakeFile()));
    }
}

public class ChineseConvertRuleTests
{
    private static RenFile MakeFile(string name = "test.txt") => new($@"C:\{name}");

    [Fact]
    public void ChineseConvert_TraditionalToSimplified()
    {
        var rule = new ChineseConvertRule
        {
            Direction = ChineseConvertDirection.TraditionalToSimplified
        };

        Assert.Equal("漫画战录.txt", rule.Execute("漫畫戰錄.txt", MakeFile("漫畫戰錄.txt")));
    }

    [Fact]
    public void ChineseConvert_SimplifiedToTraditional()
    {
        var rule = new ChineseConvertRule
        {
            Direction = ChineseConvertDirection.SimplifiedToTraditional
        };

        Assert.Equal("漫畫戰錄.txt", rule.Execute("漫画战录.txt", MakeFile("漫画战录.txt")));
    }

    [Fact]
    public void ChineseConvert_CanIncludeExtension_WhenSkipExtensionDisabled()
    {
        var rule = new ChineseConvertRule
        {
            Direction = ChineseConvertDirection.TraditionalToSimplified,
            SkipExtension = false
        };

        Assert.Equal("战录.小说", rule.Execute("戰錄.小說", MakeFile("戰錄.小說")));
    }
}

public class ChineseNumberRuleTests
{
    private static RenFile MakeFile(string name = "test.txt") => new($@"C:\{name}");

    [Fact]
    public void ChineseNumber_Strict_ConvertsStructuredIntegers()
    {
        var rule = new ChineseNumberRule();

        Assert.Equal("第12卷-第203话.txt", rule.Execute("第十二卷-第二百三话.txt", MakeFile("第十二卷-第二百三话.txt")));
    }

    [Fact]
    public void ChineseNumber_Strict_ConvertsDigitSequencesAndDates()
    {
        var rule = new ChineseNumberRule();

        Assert.Equal("2024年3月21日.txt", rule.Execute("二〇二四年三月二十一日.txt", MakeFile("二〇二四年三月二十一日.txt")));
        Assert.Equal("第301话.txt", rule.Execute("第三〇一话.txt", MakeFile("第三〇一话.txt")));
    }

    [Fact]
    public void ChineseNumber_Strict_SupportsTraditionalAndFinancialDigits()
    {
        var rule = new ChineseNumberRule();

        Assert.Equal("第1002話.txt", rule.Execute("第壹仟零贰話.txt", MakeFile("第壹仟零贰話.txt")));
    }

    [Fact]
    public void ChineseNumber_Strict_SkipsAmbiguousExpressions()
    {
        var rule = new ChineseNumberRule();

        Assert.Equal(
            "百分之三十_十来集_三分之一.txt",
            rule.Execute("百分之三十_十来集_三分之一.txt", MakeFile("百分之三十_十来集_三分之一.txt")));
        Assert.Equal("一万二.txt", rule.Execute("一万二.txt", MakeFile("一万二.txt")));
    }

    [Fact]
    public void ChineseNumber_Loose_ConvertsColloquialForms()
    {
        var rule = new ChineseNumberRule
        {
            AllowLooseForms = true
        };

        Assert.Equal("12000_2300_21_32.txt", rule.Execute("一万二_两千三_廿一_卅二.txt", MakeFile("一万二_两千三_廿一_卅二.txt")));
    }

    [Fact]
    public void ChineseNumber_CanIncludeExtension_WhenSkipExtensionDisabled()
    {
        var rule = new ChineseNumberRule
        {
            SkipExtension = false
        };

        Assert.Equal("战录.123", rule.Execute("战录.一二三", MakeFile("战录.一二三")));
    }
}

public class RearrangeRuleTests
{
    private static RenFile MakeFile() => new(@"C:\test.txt");

    [Fact]
    public void Rearrange_SwapParts()
    {
        var rule = new RearrangeRule { Delimiters = " - ", NewPattern = "$2 - $1" };
        Assert.Equal("Song - Artist.txt", rule.Execute("Artist - Song.txt", MakeFile()));
    }

    [Fact]
    public void Rearrange_ReferenceOriginal()
    {
        var rule = new RearrangeRule { Delimiters = " - ", NewPattern = "$0 ($1)" };
        Assert.Equal("A - B (A).txt", rule.Execute("A - B.txt", MakeFile()));
    }

    [Fact]
    public void Rearrange_NegativeIndex()
    {
        var rule = new RearrangeRule { Delimiters = "-", NewPattern = "$-1" };
        Assert.Equal("c.txt", rule.Execute("a-b-c.txt", MakeFile()));
    }

    [Fact]
    public void Rearrange_ByPositions()
    {
        var rule = new RearrangeRule { SplitMode = RearrangeSplitMode.Positions, Delimiters = "3,6", NewPattern = "$2$1$3" };
        Assert.Equal("defabcghi.txt", rule.Execute("abcdefghi.txt", MakeFile()));
    }

    [Fact]
    public void Rearrange_MultiDigitPlaceholder()
    {
        var rule = new RearrangeRule
        {
            Delimiters = "-",
            NewPattern = "$10-$1"
        };
        Assert.Equal("j-a.txt", rule.Execute("a-b-c-d-e-f-g-h-i-j.txt", MakeFile()));
    }
}

public class ReformatDateRuleTests
{
    private static RenFile MakeFile() => new(@"C:\test.txt");

    [Fact]
    public void ReformatDate_Basic()
    {
        // Use space separator so \b word boundary works (underscore is a \w char)
        var rule = new ReformatDateRule { SourceFormat = "yyyy-MM-dd", TargetFormat = "dd.MM.yyyy" };
        Assert.Equal("photo 25.12.2024.txt", rule.Execute("photo 2024-12-25.txt", MakeFile()));
    }

    [Fact]
    public void ReformatDate_NoMatch()
    {
        var rule = new ReformatDateRule { SourceFormat = "yyyy-MM-dd", TargetFormat = "dd.MM.yyyy" };
        Assert.Equal("nodate.txt", rule.Execute("nodate.txt", MakeFile()));
    }

    [Fact]
    public void ReformatDate_AdjustTime()
    {
        var rule = new ReformatDateRule
        {
            SourceFormat = "yyyy-MM-dd",
            TargetFormat = "yyyy-MM-dd",
            AdjustTime = true,
            AdjustTimeBy = 1,
            AdjustTimePart = "Days"
        };
        Assert.Equal("2024-12-26.txt", rule.Execute("2024-12-25.txt", MakeFile()));
    }
}

public class RandomizeRuleTests
{
    private static RenFile MakeFile() => new(@"C:\test.txt");

    [Fact]
    public void Randomize_GeneratesCorrectLength()
    {
        var rule = new RandomizeRule { Length = 8, InsertWhere = RandomizePosition.ReplaceCurrentName };
        rule.Reset();
        var result = rule.Execute("test.txt", MakeFile());
        Assert.Equal(8 + 4, result.Length); // 8 random + ".txt"
    }

    [Fact]
    public void Randomize_OnlyDigits()
    {
        var rule = new RandomizeRule { Length = 10, UseDigits = true, UseEnglishLetters = false, InsertWhere = RandomizePosition.ReplaceCurrentName };
        rule.Reset();
        var result = rule.Execute("test.txt", MakeFile());
        var name = result.Replace(".txt", "");
        Assert.All(name.ToCharArray(), c => Assert.True(char.IsDigit(c)));
    }

    [Fact]
    public void Randomize_Prefix()
    {
        var rule = new RandomizeRule { Length = 3, InsertWhere = RandomizePosition.Prefix };
        rule.Reset();
        var result = rule.Execute("test.txt", MakeFile());
        Assert.EndsWith("test.txt", result);
        Assert.Equal(7 + 4, result.Length);
    }

    [Fact]
    public void Randomize_Unique()
    {
        var rule = new RandomizeRule { Length = 6, Unique = true, InsertWhere = RandomizePosition.ReplaceCurrentName };
        rule.Reset();
        var results = new HashSet<string>();
        for (int i = 0; i < 50; i++)
        {
            var result = rule.Execute("test.txt", MakeFile());
            results.Add(result);
        }
        Assert.Equal(50, results.Count);
    }

    [Fact]
    public void Randomize_CanParallelize_WhenUniqueDisabled()
    {
        var rule = new RandomizeRule
        {
            Unique = false,
            InsertWhere = RandomizePosition.ReplaceCurrentName
        };

        Assert.True(rule.CanParallelizePreview);
    }

    [Fact]
    public void Randomize_CannotParallelize_WhenUniqueEnabled()
    {
        var rule = new RandomizeRule
        {
            Unique = true,
            InsertWhere = RandomizePosition.ReplaceCurrentName
        };

        Assert.False(rule.CanParallelizePreview);
    }
}

public class UserInputRuleTests
{
    private static RenFile MakeFile(string name = "test.txt") => new($@"C:\{name}");

    [Fact]
    public void UserInput_Replace()
    {
        var rule = new UserInputRule { InputText = "alpha\nbeta\ngamma", Mode = UserInputMode.Replace };
        rule.Reset();
        Assert.Equal("alpha.txt", rule.Execute("file1.txt", MakeFile("file1.txt")));
        Assert.Equal("beta.txt", rule.Execute("file2.txt", MakeFile("file2.txt")));
        Assert.Equal("gamma.txt", rule.Execute("file3.txt", MakeFile("file3.txt")));
    }

    [Fact]
    public void UserInput_InsertBefore()
    {
        var rule = new UserInputRule { InputText = "pre_", Mode = UserInputMode.InsertBefore };
        rule.Reset();
        Assert.Equal("pre_test.txt", rule.Execute("test.txt", MakeFile()));
    }

    [Fact]
    public void UserInput_InsertAfter()
    {
        var rule = new UserInputRule { InputText = "_suf", Mode = UserInputMode.InsertAfter };
        rule.Reset();
        Assert.Equal("test_suf.txt", rule.Execute("test.txt", MakeFile()));
    }

    [Fact]
    public void UserInput_MoreFilesThanLines()
    {
        var rule = new UserInputRule { InputText = "only", Mode = UserInputMode.Replace };
        rule.Reset();
        Assert.Equal("only.txt", rule.Execute("file1.txt", MakeFile()));
        Assert.Equal("file2.txt", rule.Execute("file2.txt", MakeFile("file2.txt"))); // empty line → no change
    }
}

public class MappingRuleTests
{
    private static RenFile MakeFile(string name = "test.txt") => new($@"C:\{name}");

    [Fact]
    public void Mapping_ExactMatch()
    {
        var rule = new MappingRule
        {
            Mappings = new()
            {
                new MappingEntry { Match = "old", NewName = "new" },
                new MappingEntry { Match = "foo", NewName = "bar" }
            },
            SkipExtension = true
        };
        Assert.Equal("new.txt", rule.Execute("old.txt", MakeFile("old.txt")));
    }

    [Fact]
    public void Mapping_PartialMatch()
    {
        var rule = new MappingRule
        {
            Mappings = new() { new MappingEntry { Match = "old", NewName = "new" } },
            PartialMatch = true, SkipExtension = true
        };
        Assert.Equal("my_new_file.txt", rule.Execute("my_old_file.txt", MakeFile("my_old_file.txt")));
    }

    [Fact]
    public void Mapping_CaseInsensitive()
    {
        var rule = new MappingRule
        {
            Mappings = new() { new MappingEntry { Match = "OLD", NewName = "new" } },
            CaseSensitive = false, SkipExtension = true
        };
        Assert.Equal("new.txt", rule.Execute("old.txt", MakeFile("old.txt")));
    }

    [Fact]
    public void Mapping_CaseSensitive_NoMatch()
    {
        var rule = new MappingRule
        {
            Mappings = new() { new MappingEntry { Match = "OLD", NewName = "new" } },
            CaseSensitive = true, SkipExtension = true
        };
        Assert.Equal("old.txt", rule.Execute("old.txt", MakeFile("old.txt")));
    }

    [Fact]
    public void Mapping_InverseMapping()
    {
        var rule = new MappingRule
        {
            Mappings = new() { new MappingEntry { Match = "old", NewName = "new" } },
            InverseMapping = true, SkipExtension = true
        };
        Assert.Equal("old.txt", rule.Execute("new.txt", MakeFile("new.txt")));
    }

    [Fact]
    public void Mapping_NoMatch_NoChange()
    {
        var rule = new MappingRule
        {
            Mappings = new() { new MappingEntry { Match = "xyz", NewName = "abc" } },
            SkipExtension = true
        };
        Assert.Equal("test.txt", rule.Execute("test.txt", MakeFile()));
    }

    [Fact]
    public void Mapping_CanParallelize_WhenReuseAllowed()
    {
        var rule = new MappingRule
        {
            AllowReuse = true
        };

        Assert.True(rule.CanParallelizePreview);
    }

    [Fact]
    public void Mapping_CannotParallelize_WhenReuseDisabled()
    {
        var rule = new MappingRule
        {
            AllowReuse = false
        };

        Assert.False(rule.CanParallelizePreview);
    }
}
