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
    public void RenameService_SerializeThenStatelessJavaScript_PreservesCorrectOutput()
    {
        var service = new RenameService();
        var serialRule = new SerializeRule { StartNumber = 1, Step = 1, Padding = 2, InsertWhere = SerializePosition.Prefix };
        var scriptRule = new JavaScriptRule
        {
            ScriptText = "Result = UpperCase(Name);"
        };
        var rules = new List<IRule> { serialRule, scriptRule };
        var files = Enumerable.Range(1, 600)
            .Select(i => MakeFile($"file{i:D4}.txt"))
            .ToList();

        service.Preview(files, rules);

        Assert.Equal("01FILE0001.txt", files[0].NewName);
        Assert.Equal("02FILE0002.txt", files[1].NewName);
        Assert.Equal("600FILE0600.txt", files[^1].NewName);
    }

    [Fact]
    public void RenameService_SerializeThenReusableMapping_PreservesCorrectOutput()
    {
        var service = new RenameService();
        var serialRule = new SerializeRule { StartNumber = 1, Step = 1, Padding = 2, InsertWhere = SerializePosition.Prefix };
        var mappingRule = new MappingRule
        {
            AllowReuse = true,
            PartialMatch = true,
            SkipExtension = true,
            Mappings = new()
            {
                new MappingEntry { Match = "FILE", NewName = "DOC" }
            }
        };
        var rules = new List<IRule> { serialRule, new JavaScriptRule { ScriptText = "Result = UpperCase(Name);" }, mappingRule };
        var files = Enumerable.Range(1, 600)
            .Select(i => MakeFile($"file{i:D4}.txt"))
            .ToList();

        service.Preview(files, rules);

        Assert.Equal("01DOC0001.txt", files[0].NewName);
        Assert.Equal("02DOC0002.txt", files[1].NewName);
        Assert.Equal("600DOC0600.txt", files[^1].NewName);
    }

    [Fact]
    public void RenameService_SerializeThenChineseConvert_PreservesCorrectOutput()
    {
        var service = new RenameService();
        var serialRule = new SerializeRule { StartNumber = 1, Step = 1, Padding = 2, InsertWhere = SerializePosition.Prefix };
        var convertRule = new ChineseConvertRule
        {
            Direction = ChineseConvertDirection.TraditionalToSimplified
        };
        var rules = new List<IRule> { serialRule, convertRule };
        var files = Enumerable.Range(1, 600)
            .Select(i => MakeFile($"漫畫戰錄{i:D4}.txt"))
            .ToList();

        service.Preview(files, rules);

        Assert.Equal("01漫画战录0001.txt", files[0].NewName);
        Assert.Equal("02漫画战录0002.txt", files[1].NewName);
        Assert.Equal("600漫画战录0600.txt", files[^1].NewName);
    }

    [Fact]
    public void RenameService_ChineseNumberRule_PreservesCorrectOutput()
    {
        var service = new RenameService();
        var rules = new List<IRule>
        {
            new ReplaceRule { FindText = "_", ReplaceText = " " },
            new ChineseNumberRule()
        };
        var files = new List<RenFile>
        {
            MakeFile("第十二卷_第二百三话.txt"),
            MakeFile("二〇二四年三月二十一日.txt")
        };

        service.Preview(files, rules);

        Assert.Equal("第12卷 第203话.txt", files[0].NewName);
        Assert.Equal("2024年3月21日.txt", files[1].NewName);
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

    [Fact]
    public void RenameService_ValidateNewNames_IncludesPreviouslyRenamedFiles()
    {
        var root = Path.Combine(Path.GetTempPath(), "RenameServiceTests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);

        var firstPath = Path.Combine(root, "first.txt");
        var secondPath = Path.Combine(root, "second.txt");
        File.WriteAllText(firstPath, "1");
        File.WriteAllText(secondPath, "2");

        try
        {
            var first = new RenFile(firstPath) { NewName = "first-renamed.txt" };
            var second = new RenFile(secondPath) { NewName = "second-renamed.txt" };

            Assert.True(first.Rename());
            Assert.True(second.Rename());

            first.NewName = "same.txt";
            second.NewName = "same.txt";

            var service = new RenameService();
            var errors = service.ValidateNewNames(new[] { first, second });

            Assert.Contains(errors, error => error.Contains("Duplicate name", StringComparison.Ordinal));
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public void RenameService_Preview_DoesNotOverrideManualNewName()
    {
        var service = new RenameService();
        var rules = new List<IRule> { new ReplaceRule { FindText = "old", ReplaceText = "new" } };
        var files = new List<RenFile> { MakeFile("old_file.txt") };
        files[0].NewName = "manual.txt";

        service.Preview(files, rules);

        Assert.Equal("manual.txt", files[0].NewName);
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
    public void RenFile_ApplyPreviewName_RespectsManualValue()
    {
        var file = new RenFile(@"C:\test.txt");
        file.NewName = "manual.txt";

        file.ApplyPreviewName("preview.txt");

        Assert.Equal("manual.txt", file.NewName);
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

    [Fact]
    public void RenFile_SameValueAssignments_DoNotRaisePropertyChanged()
    {
        var file = new RenFile(@"C:\folder\test.txt");
        var changed = new List<string>();

        file.PropertyChanged += (_, e) => changed.Add(e.PropertyName!);

        file.NewName = "test.txt";
        file.State = "";
        file.Error = "";
        file.OldPath = "";

        Assert.Empty(changed);
    }

    [Fact]
    public void RenFile_RenameAndUndo_UpdateCachedProperties()
    {
        var root = Path.Combine(Path.GetTempPath(), "RenFileTests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);

        var originalPath = Path.Combine(root, "doc123.txt");
        var renamedPath = Path.Combine(root, "renamed456.txt");
        File.WriteAllText(originalPath, "content");

        try
        {
            var file = new RenFile(originalPath)
            {
                NewName = Path.GetFileName(renamedPath)
            };

            Assert.True(file.Rename());
            Assert.Equal(renamedPath, file.FullPath);
            Assert.Equal("renamed456.txt", file.OriginalName);
            Assert.Equal("renamed456", file.BaseName);
            Assert.Equal("456", file.NameDigits);
            Assert.Equal(file.FullPath.Length, file.PathLength);
            Assert.Equal(new string(file.FullPath.Where(char.IsDigit).ToArray()), file.PathDigits);
            Assert.True(file.IsRenamed);

            Assert.True(file.UndoRename());
            Assert.Equal(originalPath, file.FullPath);
            Assert.Equal("doc123.txt", file.OriginalName);
            Assert.Equal("doc123", file.BaseName);
            Assert.Equal("123", file.NameDigits);
            Assert.Equal(file.FullPath.Length, file.PathLength);
            Assert.Equal(new string(file.FullPath.Where(char.IsDigit).ToArray()), file.PathDigits);
            Assert.False(file.IsRenamed);
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public void RenFile_CanRenameMultipleTimesWithoutReimporting()
    {
        var root = Path.Combine(Path.GetTempPath(), "RenFileTests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);

        var originalPath = Path.Combine(root, "source.txt");
        var firstPath = Path.Combine(root, "step1.txt");
        var secondPath = Path.Combine(root, "step2.txt");
        File.WriteAllText(originalPath, "content");

        try
        {
            var file = new RenFile(originalPath)
            {
                NewName = Path.GetFileName(firstPath)
            };

            Assert.True(file.Rename());
            Assert.Equal(firstPath, file.FullPath);

            file.NewName = Path.GetFileName(secondPath);

            Assert.True(file.Rename());
            Assert.Equal(secondPath, file.FullPath);
            Assert.Equal("step2.txt", file.OriginalName);
            Assert.Equal(firstPath, file.OldPath);
            Assert.True(file.IsRenamed);

            Assert.True(file.UndoRename());
            Assert.Equal(firstPath, file.FullPath);
            Assert.Equal("step1.txt", file.OriginalName);
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
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
    public void SaveAndLoad_ChineseConvertRule_PreservesProperties()
    {
        var rules = new List<IRule>
        {
            new ChineseConvertRule
            {
                Direction = ChineseConvertDirection.SimplifiedToTraditional,
                SkipExtension = false
            }
        };

        var json = PresetService.SaveToJson(rules);
        var loaded = PresetService.LoadFromJson(json);

        var rule = Assert.IsType<ChineseConvertRule>(Assert.Single(loaded));
        Assert.Equal(ChineseConvertDirection.SimplifiedToTraditional, rule.Direction);
        Assert.False(rule.SkipExtension);
    }

    [Fact]
    public void SaveAndLoad_ChineseNumberRule_PreservesProperties()
    {
        var rules = new List<IRule>
        {
            new ChineseNumberRule
            {
                AllowLooseForms = true,
                SkipExtension = false
            }
        };

        var json = PresetService.SaveToJson(rules);
        var loaded = PresetService.LoadFromJson(json);

        var rule = Assert.IsType<ChineseNumberRule>(Assert.Single(loaded));
        Assert.True(rule.AllowLooseForms);
        Assert.False(rule.SkipExtension);
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
