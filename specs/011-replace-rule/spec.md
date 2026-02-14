# Feature 011: Replace Rule

## 概述

**Feature Name**: Replace规则 - 查找替换  
**Priority**: P0 (Critical - 最常用的规则之一)  
**状态**: ✅ 已实现（WPF）  
**Dependencies**: Feature 001（规则引擎核心）, Feature 006（Meta Tags 处理器）

WPF 对齐说明：规则实现见 `ReNamerWPF/ReNamer/Rules/ReplaceRule.cs`，配置面板见 `ReNamerWPF/ReNamer/Views/RuleConfigs/ReplaceConfigPanel.xaml(.cs)`。

## 问题陈述

Replace规则是ReNamer中**使用频率最高**的规则之一,提供灵活的文本查找和替换功能:
1. **简单文本替换** - 查找文本并替换为新文本
2. **正则表达式** - 支持高级模式匹配和捕获组引用($1..$9)
3. **大小写控制** - 区分大小写/忽略大小写
4. **Meta Tags集成** - 替换文本可包含Meta Tags(如文件属性/EXIF数据)
5. **多值替换** - 支持多个查找-替换对

**应用场景**:
- 批量替换特定文本片段(如"IMG"→"Photo")
- 使用正则表达式提取并重组文件名部分
- 插入文件元数据(如添加日期/大小等Meta Tag)
- 删除不需要的文本(替换为空字符串)

## 需求（基于 DFM 分析）

以下为 DFM 分析的历史参考；WPF 已实现 Replace 规则，功能以当前实现为准。

### UI组件结构 (对照 docs/rules_requirements.md 第61-121行)

```pascal
type
  TFrame_RuleReplace = class(TFrame)
  private
    // 输入控件
    Edit_Find: TEdit;           // 查找文本
    SB_AddSeparator: TSpeedButton;  // 多值分隔符按钮
    Edit_Replace: TEdit;         // 替换文本
    SB_InsertMetaTag: TSpeedButton; // Meta Tag插入按钮
    
    // 选项
    CheckBox_CaseSensitive: TCheckBox;
    CheckBox_UseRegEx: TCheckBox;
    CheckBox_SkipExtension: TCheckBox;
    
    // 帮助
    SpeedButton_Help: TSpeedButton;
  end;
```

**组件属性**:
```pascal
// Edit_Find
Edit_Find.Left := 96;
Edit_Find.Top := 24;
Edit_Find.Width := 265;
Edit_Find.Font.Height := -13;
Edit_Find.TabOrder := 0;

// SB_AddSeparator
SB_AddSeparator.Left := 362;
SB_AddSeparator.Top := 24;
SB_AddSeparator.Width := 23;
SB_AddSeparator.Height := 25;
SB_AddSeparator.Flat := True;
SB_AddSeparator.Hint := 'Separate multiple items';

// Edit_Replace
Edit_Replace.Left := 96;
Edit_Replace.Top := 56;
Edit_Replace.Width := 289;

// SB_InsertMetaTag
SB_InsertMetaTag.Hint := 'Insert Meta Tag (Ctrl+Ins)';

// CheckBox_CaseSensitive
CheckBox_CaseSensitive.Left := 96;
CheckBox_CaseSensitive.Top := 88;
CheckBox_CaseSensitive.Caption := 'Case sensitive';
CheckBox_CaseSensitive.Checked := False;  // 默认不区分大小写

// CheckBox_UseRegEx
CheckBox_UseRegEx.Left := 96;
CheckBox_UseRegEx.Top := 112;
CheckBox_UseRegEx.Caption := 'Use RegEx';
CheckBox_UseRegEx.Checked := False;  // 默认不使用正则

// CheckBox_SkipExtension
CheckBox_SkipExtension.Left := 96;
CheckBox_SkipExtension.Top := 136;
CheckBox_SkipExtension.Caption := 'Skip extension';
CheckBox_SkipExtension.Checked := True;  // 默认跳过扩展名
```

## Implementation Details

### 规则数据结构

```pascal
type
  TRuleReplace = class(TRule)
  private
    FFindText: string;
    FReplaceText: string;
    FCaseSensitive: Boolean;
    FUseRegEx: Boolean;
    FSkipExtension: Boolean;
  public
    constructor Create; override;
    function Apply(const AFileName: string; AFileIndex: Integer): string; override;
    function GetDescription: string; override;
    
    // 序列化
    procedure LoadFromJSON(const AJSON: TJSONObject); override;
    procedure SaveToJSON(const AJSON: TJSONObject); override;
    
    property FindText: string read FFindText write FFindText;
    property ReplaceText: string read FReplaceText write FReplaceText;
    property CaseSensitive: Boolean read FCaseSensitive write FCaseSensitive;
    property UseRegEx: Boolean read FUseRegEx write FUseRegEx;
    property SkipExtension: Boolean read FSkipExtension write FSkipExtension;
  end;
```

### 简单文本替换

```pascal
function TRuleReplace.SimpleReplace(const AText: string): string;
var
  FindList, ReplaceList: TStringArray;
  I: Integer;
  SearchText, ReplaceStr: string;
  CompareFunc: function(const S1, S2: string): Boolean;
begin
  Result := AText;
  
  // 解析多值输入(用分号分隔)
  FindList := FFindText.Split([';']);
  ReplaceList := FReplaceText.Split([';']);
  
  // 设置比较函数
  if FCaseSensitive then
    CompareFunc := @SameText  // 精确匹配
  else
    CompareFunc := @SameTextIgnoreCase;  // 忽略大小写
  
  // 处理每一对查找-替换
  for I := 0 to High(FindList) do
  begin
    SearchText := Trim(FindList[I]);
    if SearchText = '' then
      Continue;
    
    // 获取对应的替换文本(如果数量不匹配,默认为空)
    if I <= High(ReplaceList) then
      ReplaceStr := Trim(ReplaceList[I])
    else
      ReplaceStr := '';
    
    // 执行替换
    if FCaseSensitive then
      Result := StringReplace(Result, SearchText, ReplaceStr, [rfReplaceAll])
    else
      Result := StringReplace(Result, SearchText, ReplaceStr, 
                              [rfReplaceAll, rfIgnoreCase]);
  end;
end;
```

**多值替换示例**:
```pascal
// 输入
FindText := 'IMG;DSC;DCIM';
ReplaceText := 'Photo;Camera;';  // DCIM替换为空(删除)

// 处理
'IMG_001.jpg'  → 'Photo_001.jpg'
'DSC_002.jpg'  → 'Camera_002.jpg'
'DCIM_003.jpg' → '_003.jpg'
```

### 正则表达式替换

```pascal
function TRuleReplace.RegExReplace(const AText: string): string;
var
  RegEx: TRegExpr;
  ProcessedReplace: string;
begin
  RegEx := TRegExpr.Create;
  try
    RegEx.Expression := FFindText;
    RegEx.ModifierI := not FCaseSensitive;  // i = case insensitive
    
    // 处理替换文本中的Meta Tags
    ProcessedReplace := FReplaceText;
    if Pos('<', ProcessedReplace) > 0 then
      ProcessedReplace := ProcessMetaTags(ProcessedReplace, FCurrentFileIndex);
    
    // 执行替换
    Result := RegEx.Replace(AText, ProcessedReplace, True);  // True = ReplaceAll
  finally
    RegEx.Free;
  end;
end;
```

**捕获组引用示例**:
```pascal
// 提取日期并重组
FindText := '(\d{4})-(\d{2})-(\d{2})';
ReplaceText := '$3.$2.$1';
'2024-01-15_file' → '15.01.2024_file'

// 提取文件名部分
FindText := '^(IMG|DSC)_(\d+)';
ReplaceText := 'Photo_$2';
'IMG_123.jpg' → 'Photo_123.jpg'

// 删除括号内容
FindText := '\s*\([^)]+\)';
ReplaceText := '';
'file (copy).txt' → 'file.txt'
```

### Meta Tags集成

```pascal
function TRuleReplace.ProcessMetaTags(const AText: string; 
  AFileIndex: Integer): string;
var
  TagProcessor: TMetaTagProcessor;
  FilePath: string;
begin
  // 如果没有Meta Tag标记,直接返回
  if Pos('<', AText) = 0 then
  begin
    Result := AText;
    Exit;
  end;
  
  // 获取文件路径
  FilePath := GetFilePathByIndex(AFileIndex);
  
  // 处理Meta Tags
  TagProcessor := TMetaTagProcessor.Create;
  try
    Result := TagProcessor.ProcessTags(AText, FilePath, AFileIndex);
  finally
    TagProcessor.Free;
  end;
end;
```

**Meta Tags替换示例**:
```pascal
// 添加文件大小
FindText := '^(.*)$';
ReplaceText := '$1_<File:Size:KB>KB';
'document.pdf' → 'document_1024KB.pdf'

// 添加EXIF日期
FindText := '^';  // 前缀
ReplaceText := '<EXIF:Date:YYYYMMDD>_';
'photo.jpg' → '20240115_photo.jpg'

// 组合多个Meta Tag
FindText := '';
ReplaceText := '<File:Name:NoExt>_<File:Width>x<File:Height>';
'image.png' → 'image_1920x1080.png'
```

### 主应用逻辑

```pascal
function TRuleReplace.Apply(const AFileName: string; 
  AFileIndex: Integer): string;
var
  BaseName, Extension: string;
  ProcessedText: string;
begin
  FCurrentFileIndex := AFileIndex;
  
  // 如果查找文本为空,直接返回
  if FFindText = '' then
  begin
    Result := AFileName;
    Exit;
  end;
  
  // 分离扩展名
  if FSkipExtension then
  begin
    BaseName := ChangeFileExt(AFileName, '');
    Extension := ExtractFileExt(AFileName);
  end
  else
  begin
    BaseName := AFileName;
    Extension := '';
  end;
  
  // 应用替换
  if FUseRegEx then
    ProcessedText := RegExReplace(BaseName)
  else
    ProcessedText := SimpleReplace(BaseName);
  
  // 重新组合
  Result := ProcessedText + Extension;
end;
```

### 描述生成

```pascal
function TRuleReplace.GetDescription: string;
var
  FindPreview, ReplacePreview: string;
const
  MAX_PREVIEW_LEN = 30;
begin
  // 截断过长的文本
  if Length(FFindText) > MAX_PREVIEW_LEN then
    FindPreview := Copy(FFindText, 1, MAX_PREVIEW_LEN) + '...'
  else
    FindPreview := FFindText;
    
  if Length(FReplaceText) > MAX_PREVIEW_LEN then
    ReplacePreview := Copy(FReplaceText, 1, MAX_PREVIEW_LEN) + '...'
  else
    ReplacePreview := FReplaceText;
  
  // 构建描述
  Result := Format('Replace "%s" with "%s"', [FindPreview, ReplacePreview]);
  
  if FUseRegEx then
    Result := Result + ' (RegEx)';
    
  if FCaseSensitive then
    Result := Result + ' (case-sensitive)';
    
  if not FSkipExtension then
    Result := Result + ' (inc. extension)';
end;
```

## UI Implementation

### Frame布局

```
┌─────────────────────────────────────────────┐
│ Replace                                     │
├─────────────────────────────────────────────┤
│ Find:    [IMG;DSC;DCIM              ] [⋮]  │
│                                             │
│ Replace: [Photo;Camera;             ] [▾]  │
│          (Meta Tags可插入)                  │
│                                             │
│ ☐ Case sensitive                           │
│ ☐ Use RegEx                                │
│ ☑ Skip extension                           │
│                                             │
│                                    [Help]  │
└─────────────────────────────────────────────┘
```

### 事件处理

```pascal
procedure TFrame_RuleReplace.SB_AddSeparatorClick(Sender: TObject);
begin
  // 在光标位置插入分号分隔符
  Edit_Find.SelText := ';';
  Edit_Find.SetFocus;
end;

procedure TFrame_RuleReplace.SB_InsertMetaTagClick(Sender: TObject);
var
  PopupMenu: TPopupMenu;
begin
  // 弹出Meta Tag菜单
  PopupMenu := CreateMetaTagPopupMenu;
  try
    PopupMenu.PopupComponent := SB_InsertMetaTag;
    PopupMenu.Popup(SB_InsertMetaTag.ClientToScreen(Point(0, SB_InsertMetaTag.Height)));
  finally
    PopupMenu.Free;
  end;
end;

procedure TFrame_RuleReplace.InsertMetaTag(const ATag: string);
begin
  Edit_Replace.SelText := ATag;
  Edit_Replace.SetFocus;
end;

procedure TFrame_RuleReplace.SpeedButton_HelpClick(Sender: TObject);
begin
  // 打开在线帮助
  OpenURL('https://www.den4b.com/wiki/ReNamer:Rules:Replace');
end;
```

## Testing Requirements

### Unit Tests

```pascal
type
  TRuleReplaceTest = class(TTestCase)
  published
    procedure TestSimpleReplace;
    procedure TestCaseSensitive;
    procedure TestMultipleReplace;
    procedure TestRegExReplace;
    procedure TestRegExCapture;
    procedure TestMetaTags;
    procedure TestSkipExtension;
  end;

procedure TRuleReplaceTest.TestSimpleReplace;
var
  Rule: TRuleReplace;
begin
  Rule := TRuleReplace.Create;
  try
    Rule.FindText := 'IMG';
    Rule.ReplaceText := 'Photo';
    Rule.CaseSensitive := False;
    Rule.UseRegEx := False;
    
    AssertEquals('Photo_001.jpg', Rule.Apply('IMG_001.jpg', 0));
    AssertEquals('photo_002.jpg', Rule.Apply('img_002.jpg', 0));  // 忽略大小写
  finally
    Rule.Free;
  end;
end;

procedure TRuleReplaceTest.TestRegExCapture;
var
  Rule: TRuleReplace;
begin
  Rule := TRuleReplace.Create;
  try
    Rule.FindText := '(\d{4})-(\d{2})-(\d{2})';
    Rule.ReplaceText := '$3.$2.$1';
    Rule.UseRegEx := True;
    
    AssertEquals('15.01.2024_file.txt', 
                 Rule.Apply('2024-01-15_file.txt', 0));
  finally
    Rule.Free;
  end;
end;

procedure TRuleReplaceTest.TestMultipleReplace;
var
  Rule: TRuleReplace;
begin
  Rule := TRuleReplace.Create;
  try
    Rule.FindText := 'IMG;DSC;DCIM';
    Rule.ReplaceText := 'Photo;Camera;';
    
    AssertEquals('Photo_001.jpg', Rule.Apply('IMG_001.jpg', 0));
    AssertEquals('Camera_002.jpg', Rule.Apply('DSC_002.jpg', 0));
    AssertEquals('_003.jpg', Rule.Apply('DCIM_003.jpg', 0));  // 删除
  finally
    Rule.Free;
  end;
end;
```

### Integration Tests

- 测试Replace与其他规则组合(Insert+Replace+Serialize)
- 测试Meta Tags在Replace中的正确展开
- 测试正则表达式的复杂模式
- 性能测试: 10000个文件应用简单替换<500ms
- 性能测试: 1000个文件应用正则替换<2s

## Performance Requirements

- 简单文本替换: <0.05ms/文件
- 正则表达式替换: <2ms/文件
- Meta Tags处理: <5ms/文件(含EXIF读取)
- UI响应性: 编辑框输入无延迟

## 验收标准

WPF 现状：Replace 规则已实现；以下清单用于历史对照。

### Phase 1: 基本替换
- [ ] TRuleReplace类实现
- [ ] 简单文本查找替换
- [ ] 大小写敏感/不敏感
- [ ] 跳过扩展名选项

### Phase 2: 多值替换
- [ ] 分号分隔的多值支持
- [ ] AddSeparator按钮功能
- [ ] 不等长列表处理

### Phase 3: 正则表达式
- [ ] RegEx引擎集成
- [ ] 捕获组引用($1..$9)
- [ ] 错误处理和提示

### Phase 4: Meta Tags集成
- [ ] Meta Tag插入UI
- [ ] 与Feature 006集成
- [ ] 组合Meta Tags测试

## Dependencies

### 系统单元
- `SysUtils` - 字符串操作
- `RegExpr` - 正则表达式引擎
- `StrUtils` - 字符串工具

### 内部依赖
- `Feature 001` - TRule基类
- `Feature 006` - TMetaTagProcessor

## References

- RegExpr库文档: https://regex.sorokin.engineer/
- Pascal正则语法参考
- ReNamer Replace规则文档

---

**Version**: 1.0  
**Created**: 2026-02-12  
**Last Updated**: 2026-02-12  
**Related**: Feature 001 (Rule Engine), Feature 006 (Meta Tags)
