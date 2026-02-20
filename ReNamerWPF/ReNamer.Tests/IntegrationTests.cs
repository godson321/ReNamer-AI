using ReNamer.Models;
using ReNamer.Rules;
using ReNamer.Services;

namespace ReNamer.Tests;

/// <summary>
/// 规则链集成测试 - 多规则组合执行
/// </summary>
public class RuleChainTests
{
    private static RenFile MakeFile(string name = "test.txt") => new($@"C:\{name}");

    [Fact]
    public void Chain_Replace_Then_Case()
    {
        var file = MakeFile("hello_world.txt");
        var rules = new IRule[]
        {
            new ReplaceRule { FindText = "_", ReplaceText = " " },
            new CaseRule { CaseType = CaseType.Capitalize }
        };

        string name = file.OriginalName;
        foreach (var rule in rules)
            name = rule.Execute(name, file);

        Assert.Equal("Hello World.txt", name);
    }

    [Fact]
    public void Chain_CleanUp_Then_Case_Then_Serialize()
    {
        var files = new[]
        {
            MakeFile("my.file.name_here.txt"),
            MakeFile("another_file.name.txt")
        };

        var cleanUp = new CleanUpRule { ReplaceDotWithSpace = true, ReplaceUnderscoreWithSpace = true };
        var caseRule = new CaseRule { CaseType = CaseType.Capitalize };
        var serialize = new SerializeRule { StartNumber = 1, Step = 1, Padding = 2, InsertWhere = SerializePosition.Prefix };
        serialize.Reset();

        var results = new List<string>();
        foreach (var file in files)
        {
            string name = file.OriginalName;
            name = cleanUp.Execute(name, file);
            name = caseRule.Execute(name, file);
            name = serialize.Execute(name, file);
            results.Add(name);
        }

        Assert.Equal("01My File Name Here.txt", results[0]);
        Assert.Equal("02Another File Name.txt", results[1]);
    }

    [Fact]
    public void Chain_Delete_Then_Insert()
    {
        var file = MakeFile("IMG_20241225_photo.jpg");
        var delete = new DeleteRule { FromPosition = 1, UntilCount = 4 }; // remove "IMG_"
        var insert = new InsertRule { InsertText = "Photo_", InsertPosition = InsertPositionType.Prefix };

        string name = file.OriginalName;
        name = delete.Execute(name, file);
        name = insert.Execute(name, file);

        Assert.Equal("Photo_20241225_photo.jpg", name);
    }

    [Fact]
    public void Chain_Regex_Then_Insert()
    {
        var file = MakeFile("document (1).txt");
        var regex = new RegexRule { Expression = @"\s*\(\d+\)", ReplaceText = "" };
        var insert = new InsertRule { InsertText = "_done", InsertPosition = InsertPositionType.Suffix };

        string name = file.OriginalName;
        name = regex.Execute(name, file);
        name = insert.Execute(name, file);

        Assert.Equal("document_done.txt", name);
    }

    [Fact]
    public void Chain_Strip_Then_Padding()
    {
        var file = MakeFile("file__123.txt");
        var strip = new StripRule { StripSymbols = true }; // removes _
        var padding = new PaddingRule { AddZeroPadding = true, ZeroPaddingLength = 5 };

        string name = file.OriginalName;
        name = strip.Execute(name, file);
        name = padding.Execute(name, file);

        Assert.Equal("file00123.txt", name);
    }
}

/// <summary>
/// RenameService 集成测试
/// </summary>
public class RenameServiceTests
{
    private static RenFile MakeFile(string name = "test.txt") => new($@"C:\{name}");

    [Fact]
    public void RenameService_PreviewWithSingleRule()
    {
        var service = new RenameService();
        var rules = new List<IRule> { new ReplaceRule { FindText = "old", ReplaceText = "new" } };

        var files = new List<RenFile> { MakeFile("old_file.txt"), MakeFile("another_old.txt") };
        service.Preview(files, rules);

        Assert.Equal("new_file.txt", files[0].NewName);
        Assert.Equal("another_new.txt", files[1].NewName);
    }

    [Fact]
    public void RenameService_PreviewWithMultipleRules()
    {
        var service = new RenameService();
        var rules = new List<IRule>
        {
            new ReplaceRule { FindText = "_", ReplaceText = " " },
            new CaseRule { CaseType = CaseType.Capitalize }
        };

        var files = new List<RenFile> { MakeFile("hello_world.txt") };
        service.Preview(files, rules);

        Assert.Equal("Hello World.txt", files[0].NewName);
    }

    [Fact]
    public void RenameService_DisabledRuleSkipped()
    {
        var service = new RenameService();
        var rules = new List<IRule> { new ReplaceRule { FindText = "test", ReplaceText = "REPLACED", IsEnabled = false } };

        var files = new List<RenFile> { MakeFile("test.txt") };
        service.Preview(files, rules);

        Assert.Equal("test.txt", files[0].NewName);
    }

    [Fact]
    public void RenameService_EmptyRuleList()
    {
        var service = new RenameService();
        var files = new List<RenFile> { MakeFile("test.txt") };
        service.Preview(files, new List<IRule>());

        Assert.Equal("test.txt", files[0].NewName);
    }

    [Fact]
    public void RenameService_SerializeWithPreview()
    {
        var service = new RenameService();
        var serialRule = new SerializeRule { StartNumber = 1, Step = 1, Padding = 2, InsertWhere = SerializePosition.Prefix };
        serialRule.Reset();
        var rules = new List<IRule> { serialRule };

        var files = new List<RenFile> { MakeFile("a.txt"), MakeFile("b.txt"), MakeFile("c.txt") };
        service.Preview(files, rules);

        Assert.Equal("01a.txt", files[0].NewName);
        Assert.Equal("02b.txt", files[1].NewName);
        Assert.Equal("03c.txt", files[2].NewName);
    }

    [Fact]
    public void RenameService_ValidateNewNames_DetectsDuplicates()
    {
        var service = new RenameService();
        var files = new List<RenFile>
        {
            MakeFile("a.txt"),
            MakeFile("b.txt")
        };
        files[0].NewName = "same.txt";
        files[1].NewName = "same.txt";

        var errors = service.ValidateNewNames(files);
        Assert.NotEmpty(errors);
        Assert.Contains("Duplicate", errors[0]);
    }
}

/// <summary>
/// RenFile 模型测试
/// </summary>
public class RenFileTests
{
    [Fact]
    public void RenFile_InitializesCorrectly()
    {
        var file = new RenFile(@"C:\folder\test.txt");
        Assert.Equal(@"C:\folder\test.txt", file.FullPath);
        Assert.Equal("test.txt", file.OriginalName);
        Assert.Equal("test.txt", file.NewName);
        Assert.Equal(@"C:\folder", file.FolderPath);
        Assert.Equal(".txt", file.Extension);
        Assert.Equal("test", file.BaseName);
        Assert.False(file.HasChanged);
    }

    [Fact]
    public void RenFile_HasChanged_WhenNewNameDiffers()
    {
        var file = new RenFile(@"C:\test.txt");
        Assert.False(file.HasChanged);
        file.NewName = "newname.txt";
        Assert.True(file.HasChanged);
    }

    [Fact]
    public void RenFile_Reset_RestoresOriginalName()
    {
        var file = new RenFile(@"C:\test.txt");
        file.NewName = "changed.txt";
        Assert.True(file.HasChanged);
        file.Reset();
        Assert.Equal("test.txt", file.NewName);
        Assert.False(file.HasChanged);
    }

    [Fact]
    public void RenFile_PropertyChanged_Fires()
    {
        var file = new RenFile(@"C:\test.txt");
        var changed = new List<string>();
        file.PropertyChanged += (_, e) => changed.Add(e.PropertyName!);
        file.NewName = "changed.txt";
        Assert.Contains("NewName", changed);
        Assert.Contains("HasChanged", changed);
    }

    [Fact]
    public void RenFile_IsMarked_DefaultTrue()
    {
        var file = new RenFile(@"C:\test.txt");
        Assert.True(file.IsMarked);
    }

    [Fact]
    public void RenFile_ComputedProperties()
    {
        var file = new RenFile(@"C:\folder\doc123.txt");
        Assert.Equal("123", file.NameDigits);
        Assert.Equal(10, file.NameLength);  // "doc123.txt"
        Assert.Equal(@"C:\folder\doc123.txt", file.NewPath);
        Assert.Equal(file.FullPath.Length, file.PathLength);
    }

    [Fact]
    public void RenFile_NewName_UpdatesComputedProperties()
    {
        var file = new RenFile(@"C:\folder\test.txt");
        var changed = new List<string>();
        file.PropertyChanged += (_, e) => changed.Add(e.PropertyName!);
        file.NewName = "renamed.txt";
        Assert.Contains("NewPath", changed);
        Assert.Contains("NewNameLength", changed);
        Assert.Contains("NewPathLength", changed);
    }

    [Fact]
    public void RenFile_Error_Property()
    {
        var file = new RenFile(@"C:\test.txt");
        Assert.Equal("", file.Error);
        file.Error = "Something went wrong";
        Assert.Equal("Something went wrong", file.Error);
    }
}

/// <summary>
/// PresetService 测试
/// </summary>
public class PresetServiceTests
{
    [Fact]
    public void SaveAndLoad_RoundTrip()
    {
        var rules = new List<IRule>
        {
            new ReplaceRule { FindText = "hello", ReplaceText = "world", CaseSensitive = true },
            new CaseRule { CaseType = CaseType.Upper, SkipExtension = false }
        };

        var json = PresetService.SaveToJson(rules);
        Assert.False(string.IsNullOrEmpty(json));

        var loaded = PresetService.LoadFromJson(json);
        Assert.Equal(2, loaded.Count);
        Assert.IsType<ReplaceRule>(loaded[0]);
        Assert.IsType<CaseRule>(loaded[1]);

        var replace = (ReplaceRule)loaded[0];
        Assert.Equal("hello", replace.FindText);
        Assert.Equal("world", replace.ReplaceText);
        Assert.True(replace.CaseSensitive);

        var caseRule = (CaseRule)loaded[1];
        Assert.Equal(CaseType.Upper, caseRule.CaseType);
    }

    [Fact]
    public void SaveAndLoad_PreservesEnabled()
    {
        var rules = new List<IRule> { new InsertRule { InsertText = "prefix_", IsEnabled = false } };
        var json = PresetService.SaveToJson(rules);
        var loaded = PresetService.LoadFromJson(json);
        Assert.Single(loaded);
        Assert.False(loaded[0].IsEnabled);
    }

    [Fact]
    public void Load_InvalidJson_ReturnsEmpty()
    {
        var loaded = PresetService.LoadFromJson("{}");
        Assert.Empty(loaded);
    }

    [Fact]
    public void Load_UnknownRuleType_Skipped()
    {
        var json = "{\"rules\":[{\"typeName\":\"UnknownRule\",\"isEnabled\":true}]}";
        var loaded = PresetService.LoadFromJson(json);
        Assert.Empty(loaded);
    }
}
