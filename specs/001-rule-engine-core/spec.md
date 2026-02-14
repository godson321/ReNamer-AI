# Feature 001: Rule Engine Core

## 概述

**Feature Name**: 规则引擎核心系统  
**Priority**: P0（关键 - 所有其他功能依赖此系统）  
**状态**: ✅ 已实现（WPF，PascalScript 为简化子集）  
**Target Platform**: WPF（C#，当前实现）/ Lazarus・Free Pascal（未来移植）

## 问题陈述

ReNamer_RE 需要一个灵活、可扩展的规则引擎,能够:
1. 支持 18 种规则类型,按顺序链式应用于文件名
2. 每个规则独立封装,可启用/禁用
3. 规则参数可序列化(用于预设保存/加载)
4. 高性能处理大量文件(10,000+ files)

## Current State (WPF Implementation Analysis)

### ✅ 已实现的规则类型 (C#/WPF)

| # | 规则名称 | 类名 | 状态 | 关键功能 |
|---|---------|------|------|---------|
| 1 | Replace | `ReplaceRule` | ✅ 完整 | FindText, ReplaceText, Occurrence(All/First/Last), CaseSensitive, WholeWordsOnly, UseWildcards, UseRegex |
| 2 | Insert | `InsertRule` | ✅ 完整 | InsertText, 6种位置模式(Prefix/Suffix/Position/AfterText/BeforeText/Replace) |
| 3 | Delete | `DeleteRule` | ✅ 完整 | From(Position/Delimiter), Until(Count/Delimiter/TillEnd), RightToLeft, LeaveDelimiter |
| 4 | Remove | `RemoveRule` | ✅ 完整 | Pattern, Occurrence, CaseSensitive, WholeWordsOnly, UseWildcards |
| 5 | Case | `CaseRule` | ✅ 完整 | 7种大小写模式，PreserveCase/ForceCase/扩展名大小写控制 |
| 6 | Serialize | `SerializeRule` | ✅ 完整 | 编号系统、零填充、重置条件 |
| 7 | RegEx | `RegexRule` | ✅ 完整 | 独立规则，支持捕获组引用 |
| 8 | Extension | `ExtensionRule` | ✅ 完整 | 新扩展名、二进制签名检测、追加模式 |
| 9 | Padding | `PaddingRule` | ✅ 完整 | 数字零填充、文本填充、填充位置 |
| 10 | Strip | `StripRule` | ✅ 完整 | 6类字符集、Unicode范围、反选模式、剥离位置 |
| 11 | Clean Up | `CleanUpRule` | ✅ 完整 | 括号、空格、Unicode Emoji/Marks、CamelCase |
| 12 | Transliterate | `TransliterateRule` | ✅ 完整 | 字母表转换、前向/反向、大小写调整 |
| 13 | Rearrange | `RearrangeRule` | ✅ 完整 | 3种分割模式、$N/$-N引用、$0原名 |
| 14 | Reformat Date | `ReformatDateRule` | ✅ 完整 | 多源格式、目标格式、时间调整 |
| 15 | Randomize | `RandomizeRule` | ✅ 完整 | 长度、唯一性、字符集、插入位置 |
| 16 | PascalScript | PascalScriptRule | ✅ 已实现（简化） | 结果表达式、常用函数与计数器、脚本模板 |
| 17 | User Input | `UserInputRule` | ✅ 完整 | 逐行输入、3种模式(Replace/InsertBefore/InsertAfter) |
| 18 | Mapping | `MappingRule` | ✅ 完整 | 表格映射、部分匹配、反向映射、复用控制 |

**统计**: 
- ✅ 完整实现: 18/18 (100%)
- ⚠️ 部分实现: 0/18 (0%)
- ❌ 未实现: 1/18 (6%)

### 核心架构分析

#### 1. 规则接口设计 (C#)

```csharp
public interface IRule : INotifyPropertyChanged
{
    string RuleName { get; }
    string Description { get; }
    bool IsEnabled { get; set; }
    string Execute(string fileName, RenFile file);
}

public abstract class RuleBase : IRule
{
    protected static (string baseName, string extension) SplitFileName(
        string fileName, bool skipExtension);
}
```

**移植建议**:
```pascal
type
  IRenameRule = interface
    ['{GUID}']
    function GetRuleName: string;
    function GetDescription: string;
    function GetIsEnabled: Boolean;
    procedure SetIsEnabled(Value: Boolean);
    function Execute(const FileName: string; FileData: TRenFile): string;
    
    property RuleName: string read GetRuleName;
    property Description: string read GetDescription;
    property IsEnabled: Boolean read GetIsEnabled write SetIsEnabled;
  end;
  
  TRuleBase = class(TInterfacedObject, IRenameRule)
  protected
    FIsEnabled: Boolean;
    procedure SplitFileName(const FileName: string; SkipExtension: Boolean;
      out BaseName, Extension: string);
  public
    constructor Create; virtual;
    function GetRuleName: string; virtual; abstract;
    function GetDescription: string; virtual; abstract;
    function GetIsEnabled: Boolean; virtual;
    procedure SetIsEnabled(Value: Boolean); virtual;
    function Execute(const FileName: string; FileData: TRenFile): string; 
      virtual; abstract;
  end;
```

#### 2. RenameService 核心流程 (C#)

```csharp
public class RenameService
{
    public void Preview(IEnumerable<RenFile> files, IEnumerable<IRule> rules)
    {
        foreach (var file in files)
        {
            var currentName = file.OriginalName;
            foreach (var rule in rules)
            {
                if (rule.IsEnabled)
                    currentName = rule.Execute(currentName, file);
            }
            file.NewName = currentName;
        }
    }
}
```

**移植建议**:
```pascal
type
  TRenameService = class
  public
    procedure Preview(Files: TObjectList<TRenFile>; Rules: TInterfaceList);
    function Rename(Files: TObjectList<TRenFile>): TRenameResult;
    function UndoRename(Files: TObjectList<TRenFile>): TRenameResult;
    function ValidateNewNames(Files: TObjectList<TRenFile>): TStringList;
  end;
  
  TRenameResult = record
    SuccessCount: Integer;
    FailedCount: Integer;
  end;
```

#### 3. 辅助函数库 (RuleHelpers.cs)

**已实现功能** (需完整查看代码):
- `ReplaceWildcard()`: 通配符替换 (?, *, [, ])
- `ReplaceWholeWords()`: 完整单词替换
- `RemoveWildcard()`: 通配符移除
- `RemoveWholeWords()`: 完整单词移除

**移植注意事项**:
- Pascal字符串处理与C#不同,需重写算法
- 通配符匹配建议使用正则表达式库(TRegExpr)或手动实现DFA

## 缺失项（WPF）

当前 WPF 版本已补齐 PascalScript（简化子集）；其余规则已实现。以下内容保留为需求参考。

### 🔴 关键缺失规则

#### 1. SerializeRule（WPF 已实现）
**原版参数** (对照 docs/rules_requirements.md 第182-213行):
```pascal
type
  TSerializeRule = class(TRuleBase)
  private
    FIndexStarts: Integer;         // 起始编号(默认1)
    FRepeat: Integer;              // 重复次数(默认1)
    FStep: Integer;                // 步进值(默认1)
    FResetEvery: Boolean;          // 是否每N个重置
    FResetEveryCount: Integer;     // 重置计数
    FResetIfFolderChanges: Boolean;// 文件夹变化时重置
    FResetIfFileNameChanges: Boolean; // 文件名变化时重置
    FPadToLength: Boolean;         // 零填充
    FPadLength: Integer;           // 填充长度
    FNumberingSystem: string;      // 编号系统(十进制/十六进制/自定义)
    FCustomSymbols: string;        // 自定义编号符号
    FInsertWhere: TInsertPosition; // 插入位置(Prefix/Suffix/Position/Replace)
    FPosition: Integer;            // 位置值
    FSkipExtension: Boolean;       // 跳过扩展名(默认True)
  public
    function Execute(const FileName: string; FileData: TRenFile): string; override;
  end;
```

**关键算法**:
```pascal
function TSerializeRule.GetCurrentIndex: Integer;
begin
  Result := FIndexStarts + (FFileCounter div FRepeat) * FStep;
  
  // 重置条件检查
  if FResetEvery and (FFileCounter mod FResetEveryCount = 0) then
    ResetCounter;
  if FResetIfFolderChanges and (CurrentFolder <> LastFolder) then
    ResetCounter;
  if FResetIfFileNameChanges and (CurrentBaseName <> LastBaseName) then
    ResetCounter;
    
  Inc(FFileCounter);
end;

function FormatNumber(Index: Integer; const NumberingSystem: string): string;
begin
  case NumberingSystem of
    'Decimal': Result := IntToStr(Index);
    'Hexadecimal': Result := IntToHex(Index, 0);
    'Octal': Result := IntToOct(Index);
    'Binary': Result := IntToBin(Index);
    'Roman': Result := IntToRoman(Index); // 需实现
    'Custom': Result := IntToCustomBase(Index, FCustomSymbols);
  end;
  
  if FPadToLength and (Length(Result) < FPadLength) then
    Result := StringOfChar('0', FPadLength - Length(Result)) + Result;
end;
```

#### 2. ExtensionRule（WPF 已实现）
**原版参数** (对照 docs/rules_requirements.md 第236-248行):
```pascal
type
  TExtensionRule = class(TRuleBase)
  private
    FNewExtension: string;         // 新扩展名(不含点)
    FAppendToOriginal: Boolean;    // 追加到原文件名
    FDetectBinSign: Boolean;       // 二进制签名检测
    FRemoveDuplicateExtensions: Boolean; // 移除重复扩展名
    FCaseSensitive: Boolean;       // 大小写敏感
  public
    function Execute(const FileName: string; FileData: TRenFile): string; override;
  end;
```

**二进制签名检测实现**:
```pascal
function DetectFileTypeBySignature(const FilePath: string): string;
const
  Signatures: array[0..5] of record
    Magic: array of Byte;
    Extension: string;
  end = (
    (Magic: ($FF, $D8, $FF); Extension: 'jpg'),
    (Magic: ($89, $50, $4E, $47); Extension: 'png'),
    (Magic: ($47, $49, $46); Extension: 'gif'),
    (Magic: ($50, $4B, $03, $04); Extension: 'zip'),
    (Magic: ($25, $50, $44, $46); Extension: 'pdf'),
    (Magic: ($D0, $CF, $11, $E0); Extension: 'doc') // Office compound
  );
var
  F: TFileStream;
  Buffer: array[0..7] of Byte;
  I, J: Integer;
  Match: Boolean;
begin
  Result := '';
  if not FileExists(FilePath) then Exit;
  
  F := TFileStream.Create(FilePath, fmOpenRead or fmShareDenyNone);
  try
    if F.Size < 4 then Exit;
    F.Read(Buffer, Min(F.Size, SizeOf(Buffer)));
    
    for I := Low(Signatures) to High(Signatures) do
    begin
      Match := True;
      for J := 0 to High(Signatures[I].Magic) do
        if Buffer[J] <> Signatures[I].Magic[J] then
        begin
          Match := False;
          Break;
        end;
      if Match then
      begin
        Result := Signatures[I].Extension;
        Exit;
      end;
    end;
  finally
    F.Free;
  end;
end;
```

#### 3. RegExRule（WPF 已实现，独立正则规则）
WPF 已提供独立 RegexRule，以下为原始需求参考:
```pascal
type
  TRegExRule = class(TRuleBase)
  private
    FExpression: string;           // 正则表达式
    FReplace: string;              // 替换文本(支持$1..$9)
    FCaseSensitive: Boolean;       // 大小写敏感
    FSkipExtension: Boolean;       // 跳过扩展名(默认True)
  public
    function Execute(const FileName: string; FileData: TRenFile): string; override;
  end;
```

**实现**:
```pascal
uses
  RegExpr; // TRegExpr from https://regex.sorokin.engineer/

function TRegExRule.Execute(const FileName: string; FileData: TRenFile): string;
var
  BaseName, Ext: string;
  RegEx: TRegExpr;
begin
  if FExpression = '' then Exit(FileName);
  
  SplitFileName(FileName, FSkipExtension, BaseName, Ext);
  
  RegEx := TRegExpr.Create;
  try
    RegEx.Expression := FExpression;
    RegEx.ModifierI := not FCaseSensitive; // i = case-insensitive
    
    if RegEx.Exec(BaseName) then
      Result := RegEx.Replace(BaseName, FReplace, True) + Ext
    else
      Result := FileName;
  finally
    RegEx.Free;
  end;
end;
```

#### 4. PascalScriptRule（WPF 已实现，简化子集）
WPF 已实现可运行的 PascalScript 子集，用于覆盖常见脚本重命名场景（不包含语法高亮/自动补全）。

支持能力：
- `Result := <表达式>`（不写 `Result` 时整段视为表达式）
- 字符串拼接：使用 `+`
- 字符串字面量：`'text'`（`''` 转义）
- 变量：`Name`, `Ext`, `FullName`, `Path`, `FolderPath`, `Folder`, `Counter`, `Size`, `SizeKB`, `SizeMB`, `Created`, `Modified`, `ExifDate`
- 函数：`UpperCase`, `LowerCase`, `Trim`, `Replace`, `LeftStr`, `RightStr`, `MidStr`
- 计数器：`CounterStart` / `CounterStep` / `CounterDigits`
- 输出后可选 Meta Tags 解析

限制：
- 不支持完整 PascalScript 语法
- 不提供语法高亮与自动补全

### 🟡 历史不完整项（WPF 已补齐）

WPF 已实现 PreserveCase/ForceCase/扩展名大小写控制，以下内容保留为需求参考。

#### 1. CaseRule 缺失功能（历史参考）
**对照原版** (docs/rules_requirements.md 第155-179行):

```pascal
type
  TCaseMode = (
    cmCapitalizeEveryWord,     // 每个单词首字母大写
    cmAllLowerCase,            // 全部小写
    cmAllUpperCase,            // 全部大写
    cmInvertCase,              // 反转大小写
    cmFirstLetterCapital,      // 仅首字母大写
    cmSentenceCase,            // 句子大小写
    cmNoneOfAbove              // 不转换
  );
  
  TCaseRule = class(TRuleBase)
  private
    FCaseMode: TCaseMode;
    FPreserveCase: Boolean;          // ⚠️ WPF缺失
    FForceCase: string;              // ⚠️ WPF缺失 - "CD,DVD,DJ"
    FSkipExtension: Boolean;
    FExtensionAlwaysLowerCase: Boolean;
    FExtensionAlwaysUpperCase: Boolean;
  public
    function Execute(const FileName: string; FileData: TRenFile): string; override;
  end;
```

**ForceCase 实现**:
```pascal
function TCaseRule.ApplyForceCase(const Input: string): string;
var
  Fragments: TStringList;
  I, Pos: Integer;
  Fragment: string;
begin
  Result := Input;
  if FForceCase = '' then Exit;
  
  Fragments := TStringList.Create;
  try
    Fragments.Delimiter := ',';
    Fragments.StrictDelimiter := True;
    Fragments.DelimitedText := FForceCase;
    
    for I := 0 to Fragments.Count - 1 do
    begin
      Fragment := Fragments[I];
      // 替换所有出现(不区分大小写)
      Pos := 1;
      while Pos > 0 do
      begin
        Pos := PosEx(Fragment, Result, Pos, [rfIgnoreCase]);
        if Pos > 0 then
        begin
          Delete(Result, Pos, Length(Fragment));
          Insert(Fragment, Result, Pos); // 插入原始大小写
          Inc(Pos, Length(Fragment));
        end;
      end;
    end;
  finally
    Fragments.Free;
  end;
end;
```

## Architecture Requirements

### 1. 规则注册表 (Rule Registry)
```pascal
type
  TRuleRegistry = class
  private
    FRuleClasses: TStringList; // Name -> TRuleClass
  public
    procedure RegisterRule(const RuleName: string; RuleClass: TRuleClass);
    function CreateRule(const RuleName: string): IRenameRule;
    function GetRuleNames: TStringList;
  end;

var
  RuleRegistry: TRuleRegistry; // 全局单例
  
// 注册示例
initialization
  RuleRegistry := TRuleRegistry.Create;
  RuleRegistry.RegisterRule('Replace', TReplaceRule);
  RuleRegistry.RegisterRule('Insert', TInsertRule);
  // ... 注册所有18种规则

finalization
  RuleRegistry.Free;
```

### 2. 规则序列化 (Preset Support)
```pascal
type
  TRuleSerializer = class
  public
    class function RuleToJSON(Rule: IRenameRule): TJSONObject;
    class function JSONToRule(JSON: TJSONObject): IRenameRule;
    class procedure SavePreset(Rules: TInterfaceList; const FileName: string);
    class function LoadPreset(const FileName: string): TInterfaceList;
  end;
```

**JSON格式示例**:
```json
{
  "rules": [
    {
      "type": "Replace",
      "enabled": true,
      "params": {
        "findText": "old",
        "replaceText": "new",
        "caseSensitive": false,
        "occurrence": "All",
        "skipExtension": true
      }
    },
    {
      "type": "Serialize",
      "enabled": true,
      "params": {
        "indexStarts": 1,
        "step": 1,
        "padToLength": 3,
        "insertWhere": "Suffix"
      }
    }
  ]
}
```

### 3. Meta Tag 支持
**原版 Meta Tag 列表** (需实现):
- `:File_Name:` - 原文件名
- `:File_Path:` - 完整路径
- `:File_Extension:` - 扩展名
- `:File_Size:` - 文件大小
- `:File_SizeKB:` - KB大小
- `:File_SizeMB:` - MB大小
- `:File_Created:` - 创建时间
- `:File_Modified:` - 修改时间
- `:File_Accessed:` - 访问时间
- `:Width:`, `:Height:` - 图片尺寸
- `:ExifDate:` - EXIF日期
- `:Random_Number:`, `:Random_Alpha:` - 随机值

```pascal
type
  TMetaTagProcessor = class
  public
    class function ExpandMetaTags(const Input: string; FileData: TRenFile): string;
  end;
  
implementation

class function TMetaTagProcessor.ExpandMetaTags(const Input: string; 
  FileData: TRenFile): string;
var
  S: string;
begin
  S := Input;
  S := StringReplace(S, ':File_Name:', FileData.OriginalName, [rfReplaceAll, rfIgnoreCase]);
  S := StringReplace(S, ':File_Path:', FileData.FullPath, [rfReplaceAll, rfIgnoreCase]);
  S := StringReplace(S, ':File_Extension:', FileData.Extension, [rfReplaceAll, rfIgnoreCase]);
  S := StringReplace(S, ':File_SizeKB:', FileData.SizeKB, [rfReplaceAll, rfIgnoreCase]);
  // ... 其他标签
  Result := S;
end;
```

## Performance Requirements

### 基准测试目标

| 操作 | 文件数 | 规则数 | 目标时间 | 测试条件 |
|------|--------|--------|----------|---------|
| 预览 | 10,000 | 5 | < 1s | 简单规则(Replace/Insert) |
| 预览 | 10,000 | 5 | < 3s | 复杂规则(RegEx/Serialize) |
| 重命名 | 1,000 | 5 | < 500ms | SSD,本地磁盘 |

### 优化策略
1. **批量操作**: 使用 `BeginUpdate/EndUpdate` 包装
2. **延迟计算**: 仅计算可见行的 Description
3. **字符串池**: 复用常见字符串(扩展名、路径)
4. **并行化**: 预览阶段可并行处理文件(需线程安全)

## Testing Strategy

### 单元测试框架
```pascal
uses
  FPCUnit, TestRegistry;

type
  TReplaceRuleTest = class(TTestCase)
  published
    procedure TestReplaceAll;
    procedure TestReplaceFirst;
    procedure TestReplaceLast;
    procedure TestCaseSensitive;
    procedure TestWholeWordsOnly;
    procedure TestWildcards;
    procedure TestSkipExtension;
  end;

procedure TReplaceRuleTest.TestReplaceAll;
var
  Rule: TReplaceRule;
  Input, Output: string;
begin
  Rule := TReplaceRule.Create;
  try
    Rule.FindText := 'foo';
    Rule.ReplaceText := 'bar';
    Rule.Occurrence := roAll;
    
    Input := 'foo test foo.txt';
    Output := Rule.Execute(Input, nil);
    AssertEquals('bar test bar.txt', Output);
  finally
    Rule.Free;
  end;
end;
```

### 测试覆盖率目标
- **核心接口**: 100%
- **每个规则**: ≥ 90%
- **边界情况**: 必须覆盖
  - 空文件名
  - 超长文件名(>260字符)
  - Unicode字符(emoji, CJK)
  - 特殊字符(`/\:*?"<>|`)

## 验收标准

WPF 现状：规则与核心流程已实现（PascalScript 为简化子集）；以下清单主要用于未来移植/补齐参考。

### Phase 1: Core Infrastructure (本规范范围)
- [ ] `IRenameRule` 接口定义完成
- [ ] `TRuleBase` 基类实现完成
- [ ] `TRenameService` 核心流程完成
- [ ] `TRuleRegistry` 注册系统完成
- [ ] `TRuleSerializer` JSON序列化完成
- [ ] `TMetaTagProcessor` Meta Tag扩展完成

### Phase 2: Rule Implementation
- [ ] 所有 18 种规则类型实现完成
- [ ] SerializeRule 完整实现(包括所有编号系统)
- [ ] ExtensionRule 二进制签名检测实现
- [ ] RegExRule 独立实现(支持$1..$9)
- [ ] CaseRule ForceCase/PreserveCase 补充

### Phase 3: Quality Assurance
- [ ] 单元测试覆盖率 ≥ 80%
- [ ] 所有原版行为测试通过
- [ ] 性能基准测试达标
- [ ] 内存泄漏检测通过(HeapTrc)

### Phase 4: Documentation
- [ ] 每个规则类的 PasDoc 注释
- [ ] 使用示例代码
- [ ] 规则迁移指南(C# → Pascal)

## Dependencies

### 必需依赖
- `fpjson` (FPC标准库) - JSON序列化
- `RegExpr` (https://regex.sorokin.engineer/) - 正则表达式
- `LCLIntf` (Lazarus) - 跨平台文件系统接口

### 可选依赖
- `fpexif` - EXIF日期提取(用于 :ExifDate: Meta Tag)
- `PascalScript` - 脚本规则引擎（WPF 简化子集）

## Migration Notes (从 WPF 移植)

### C# → Pascal 主要差异

| C# 特性 | Pascal 等价 | 注意事项 |
|---------|-------------|---------|
| `INotifyPropertyChanged` | 手动实现或忽略 | WPF数据绑定特有,Lazarus不需要 |
| `IEnumerable<T>` | `TObjectList<T>` / `TInterfaceList` | 泛型集合 |
| `StringComparison.OrdinalIgnoreCase` | `AnsiCompareText` | 大小写不敏感比较 |
| `Path.GetExtension()` | `ExtractFileExt()` | 返回值包含点(`.txt`) |
| `Regex.Replace()` | `TRegExpr.Replace()` | API差异较大 |
| `LINQ` | 手动循环 | Pascal无LINQ,需改写 |

### 关键代码迁移示例

**C# Replace All**:
```csharp
private static string ReplaceAll(string input, string find, string replace, 
    StringComparison comparison)
{
    var result = input;
    int index;
    while ((index = result.IndexOf(find, comparison)) >= 0)
    {
        result = result.Remove(index, find.Length).Insert(index, replace);
    }
    return result;
}
```

**Pascal 等价**:
```pascal
function ReplaceAll(const Input, Find, Replace: string; 
  CaseSensitive: Boolean): string;
var
  Pos: Integer;
begin
  Result := Input;
  Pos := 1;
  while True do
  begin
    if CaseSensitive then
      Pos := PosEx(Find, Result, Pos)
    else
      Pos := PosEx(Find, Result, Pos, [rfIgnoreCase]);
      
    if Pos = 0 then Break;
    
    Delete(Result, Pos, Length(Find));
    Insert(Replace, Result, Pos);
    Inc(Pos, Length(Replace));
  end;
end;
```

## Next Steps

1. **本规范获批后**:
   - 创建 `src/Core/Rules.pas` 定义接口
   - 创建 `src/Core/RuleEngine.pas` 实现核心服务
   - 创建 `src/Rules/` 目录结构

2. **后续规范**:
   - `002-replace-rule-implementation.md` - Replace规则详细实现
   - `003-serialize-rule-implementation.md` - Serialize规则完整实现
   - `004-rule-config-ui-frames.md` - 规则配置Frame界面

3. **并行任务**:
   - 设置FPCUnit测试框架
   - 创建规则测试模板
   - 准备原版行为测试数据

---

**Version**: 1.0  
**Created**: 2026-02-12  
**Last Updated**: 2026-02-12  
**Author**: AI Assistant (based on WPF code analysis + DFM requirements)
