# Feature 012: Rearrange Rule

## 概述

**Feature Name**: Rearrange规则 - 重新排列  
**Priority**: P0 (Critical - 核心规则)  
**状态**: ✅ 已实现（WPF）  
**Dependencies**: Feature 001（规则引擎核心）, Feature 006（Meta Tags 处理器）

WPF 对齐说明：规则实现见 `ReNamerWPF/ReNamer/Rules/AdvancedRules.cs`，配置面板见 `ReNamerWPF/ReNamer/Views/RuleConfigs/RearrangeConfigPanel.xaml(.cs)`。

## 问题陈述

Rearrange规则用于拆分文件名并重新排列各部分:
1. **分隔符拆分** - 使用分隔符将文件名拆分为多个部分
2. **位置拆分** - 按固定位置拆分
3. **精确模式** - 按分隔符的精确模式拆分
4. **重组模式** - 使用$1..$N引用各部分,支持Meta Tags
5. **从右处理** - 支持从右到左拆分

**应用场景**:
- 交换文件名部分(如"LastName_FirstName"→"FirstName_LastName")
- 提取并重组特定部分(如"2024-01-15_file"→"file_20240115")
- 与Meta Tags结合(如"$1_<File:Size:KB>KB_$2")

## 需求（基于 DFM 分析）

以下为 DFM 分析的历史参考；WPF 已实现 Rearrange 规则，功能以当前实现为准。

### UI组件结构 (对照 docs/rules_requirements.md 第368-389行)

```pascal
type
  TRearrangeMode = (
    rmUsingDelimiters,           // 使用分隔符
    rmUsingPositions,            // 使用位置
    rmUsingExactPattern          // 精确模式
  );

  TFrame_RuleRearrange = class(TFrame)
  private
    // 分割模式
    RB_UsingDelimiters: TRadioButton;
    RB_UsingPositions: TRadioButton;
    RB_UsingExactPattern: TRadioButton;
    
    // 输入
    Edit_Delimiters: TEdit;
    SB_AddSeparator: TSpeedButton;
    Edit_NewPattern: TEdit;
    SB_InsertMetaTag: TSpeedButton;
    
    // 选项
    CB_SkipExtension: TCheckBox;
    CB_RightToLeft: TCheckBox;
  end;
```

**组件属性**:
```pascal
// RadioButtons
RB_UsingDelimiters.Left := 112;
RB_UsingDelimiters.Top := 8;
RB_UsingDelimiters.Checked := True;  // 默认使用分隔符

// Edit_Delimiters
Edit_Delimiters.Left := 24;
Edit_Delimiters.Top := 72;
Edit_Delimiters.Width := 329;
Edit_Delimiters.Font.Height := -13;

// SB_AddSeparator
SB_AddSeparator.Left := 354;
SB_AddSeparator.Top := 72;
SB_AddSeparator.Width := 23;
SB_AddSeparator.Hint := 'Separate multiple items';

// Edit_NewPattern
Edit_NewPattern.Left := 24;
Edit_NewPattern.Top := 123;
Edit_NewPattern.Width := 329;

// SB_InsertMetaTag
SB_InsertMetaTag.Hint := 'Insert Meta Tag (Ctrl+Ins)';

// CB_SkipExtension
CB_SkipExtension.Checked := True;  // 默认跳过扩展名

// CB_RightToLeft
CB_RightToLeft.Caption := 'Right-to-left';
CB_RightToLeft.Checked := False;
```

## Implementation Details

### 规则数据结构

```pascal
type
  TRuleRearrange = class(TRule)
  private
    FMode: TRearrangeMode;
    FDelimiters: string;        // 分隔符列表(分号分隔)
    FPositions: string;         // 位置列表(分号分隔)
    FNewPattern: string;        // 新模式($1..$N, $-1..$-N, $0)
    FSkipExtension: Boolean;
    FRightToLeft: Boolean;
  public
    constructor Create; override;
    function Apply(const AFileName: string; AFileIndex: Integer): string; override;
    function GetDescription: string; override;
    
    procedure LoadFromJSON(const AJSON: TJSONObject); override;
    procedure SaveToJSON(const AJSON: TJSONObject); override;
  end;
```

### 分隔符拆分

```pascal
function TRuleRearrange.SplitByDelimiters(const AText: string): TStringArray;
var
  DelimList: TStringArray;
  I, Pos, LastPos: Integer;
  Parts: TList<string>;
  Delim: string;
begin
  Parts := TList<string>.Create;
  try
    // 解析分隔符列表
    DelimList := FDelimiters.Split([';']);
    
    if FRightToLeft then
    begin
      // 从右到左:反转文本和分隔符
      // TODO: 实现从右到左逻辑
    end
    else
    begin
      // 从左到右:顺序查找分隔符
      LastPos := 1;
      I := 1;
      
      while I <= Length(AText) do
      begin
        // 检查是否匹配任何分隔符
        for Delim in DelimList do
        begin
          if Copy(AText, I, Length(Delim)) = Trim(Delim) then
          begin
            // 找到分隔符,添加前一部分
            if I > LastPos then
              Parts.Add(Copy(AText, LastPos, I - LastPos));
              
            I := I + Length(Delim);
            LastPos := I;
            Break;
          end;
        end;
        
        Inc(I);
      end;
      
      // 添加最后一部分
      if LastPos <= Length(AText) then
        Parts.Add(Copy(AText, LastPos, Length(AText)));
    end;
    
    // 转换为数组
    SetLength(Result, Parts.Count);
    for I := 0 to Parts.Count - 1 do
      Result[I] := Parts[I];
  finally
    Parts.Free;
  end;
end;
```

**分隔符拆分示例**:
```pascal
// 原文件名: "LastName_FirstName_2024"
Delimiters := "_"
→ Parts: ["LastName", "FirstName", "2024"]

// 多个分隔符
Delimiters := "_; "
"Last_First 2024"
→ Parts: ["Last", "First", "2024"]

// 从右到左
RightToLeft := True
Delimiters := "_"
"A_B_C"
→ Parts: ["C", "B", "A"]
```

### 位置拆分

```pascal
function TRuleRearrange.SplitByPositions(const AText: string): TStringArray;
var
  PosList: TStringArray;
  Positions: array of Integer;
  I, LastPos: Integer;
  Parts: TList<string>;
begin
  Parts := TList<string>.Create;
  try
    // 解析位置列表
    PosList := FPositions.Split([';']);
    SetLength(Positions, Length(PosList));
    
    for I := 0 to High(PosList) do
      Positions[I] := StrToIntDef(Trim(PosList[I]), 0);
      
    // 排序位置
    TArray.Sort<Integer>(Positions);
    
    // 按位置拆分
    LastPos := 1;
    for I := 0 to High(Positions) do
    begin
      if Positions[I] > LastPos then
      begin
        Parts.Add(Copy(AText, LastPos, Positions[I] - LastPos));
        LastPos := Positions[I];
      end;
    end;
    
    // 添加最后一部分
    if LastPos <= Length(AText) then
      Parts.Add(Copy(AText, LastPos, Length(AText)));
    
    // 转换为数组
    SetLength(Result, Parts.Count);
    for I := 0 to Parts.Count - 1 do
      Result[I] := Parts[I];
  finally
    Parts.Free;
  end;
end;
```

**位置拆分示例**:
```pascal
// 原文件名: "Document2024"
Positions := "8"
→ Parts: ["Documen", "t2024"]

// 多个位置
Positions := "4;8"
"Document2024"
→ Parts: ["Doc", "umen", "t2024"]
```

### 重组模式

```pascal
function TRuleRearrange.Rearrange(const AParts: TStringArray; 
  AFileIndex: Integer): string;
var
  Pattern: string;
  I, Index: Integer;
  TagProcessor: TMetaTagProcessor;
begin
  Pattern := FNewPattern;
  
  // 处理Meta Tags
  if Pos('<', Pattern) > 0 then
  begin
    TagProcessor := TMetaTagProcessor.Create;
    try
      Pattern := TagProcessor.ProcessTags(Pattern, 
        GetFilePathByIndex(AFileIndex), AFileIndex);
    finally
      TagProcessor.Free;
    end;
  end;
  
  // 替换$0(原文件名)
  if Pos('$0', Pattern) > 0 then
    Pattern := StringReplace(Pattern, '$0', 
      GetOriginalFileName(AFileIndex), [rfReplaceAll]);
  
  // 替换$1..$N(从前向后)
  for I := Length(AParts) downto 1 do
    Pattern := StringReplace(Pattern, '$' + IntToStr(I), 
      AParts[I - 1], [rfReplaceAll]);
  
  // 替换$-1..$-N(从后向前)
  for I := 1 to Length(AParts) do
    Pattern := StringReplace(Pattern, '$-' + IntToStr(I), 
      AParts[Length(AParts) - I], [rfReplaceAll]);
  
  Result := Pattern;
end;
```

**重组示例**:
```pascal
// Parts: ["Last", "First", "2024"]

// 示例1: 交换顺序
NewPattern := "$2 $1"
→ "First Last"

// 示例2: 使用从后向前引用
NewPattern := "$-1_$-3"
→ "2024_Last"

// 示例3: 保留原名+添加
NewPattern := "$0_edited"
→ "Last_First_2024_edited"

// 示例4: 结合Meta Tags
NewPattern := "$2_$1_<File:Size:KB>KB"
→ "First_Last_1024KB"

// 示例5: 重复使用部分
NewPattern := "$1-$1-$2"
→ "Last-Last-First"
```

### 精确模式

```pascal
function TRuleRearrange.SplitByExactPattern(const AText: string): TStringArray;
var
  DelimList: TStringArray;
  I, Pos: Integer;
  Parts: TList<string>;
  CurrentPart: string;
  ExpectedDelim: string;
begin
  // 精确模式:按分隔符的精确顺序拆分
  Parts := TList<string>.Create;
  try
    DelimList := FDelimiters.Split([';']);
    CurrentPart := '';
    I := 1;
    Pos := 0;
    
    while I <= Length(AText) do
    begin
      // 检查当前位置是否匹配期望的分隔符
      if Pos < Length(DelimList) then
        ExpectedDelim := Trim(DelimList[Pos])
      else
        ExpectedDelim := '';
        
      if (ExpectedDelim <> '') and 
         (Copy(AText, I, Length(ExpectedDelim)) = ExpectedDelim) then
      begin
        // 匹配,保存当前部分
        Parts.Add(CurrentPart);
        CurrentPart := '';
        I := I + Length(ExpectedDelim);
        Inc(Pos);
      end
      else
      begin
        // 不匹配,添加到当前部分
        CurrentPart := CurrentPart + AText[I];
        Inc(I);
      end;
    end;
    
    // 添加最后一部分
    if CurrentPart <> '' then
      Parts.Add(CurrentPart);
    
    // 转换为数组
    SetLength(Result, Parts.Count);
    for I := 0 to Parts.Count - 1 do
      Result[I] := Parts[I];
  finally
    Parts.Free;
  end;
end;
```

**精确模式示例**:
```pascal
// Delimiters := "_; "
// 文件名: "Last_First 2024"
// 
// 普通模式: ["Last", "First", "2024"]
//   (任意顺序匹配"_"或" ")
//
// 精确模式: ["Last", "First", "2024"]
//   (必须先"_"再" ")
//
// 文件名: "Last First_2024"
// 普通模式: ["Last", "First", "2024"]
// 精确模式: ["Last First", "2024"]
//   (期望"_"但遇到" ",不匹配)
```

### 主应用逻辑

```pascal
function TRuleRearrange.Apply(const AFileName: string; 
  AFileIndex: Integer): string;
var
  BaseName, Extension: string;
  Parts: TStringArray;
begin
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
  
  // 拆分文件名
  case FMode of
    rmUsingDelimiters:
      Parts := SplitByDelimiters(BaseName);
    rmUsingPositions:
      Parts := SplitByPositions(BaseName);
    rmUsingExactPattern:
      Parts := SplitByExactPattern(BaseName);
  end;
  
  // 如果没有拆分出任何部分,返回原名
  if Length(Parts) = 0 then
  begin
    Result := AFileName;
    Exit;
  end;
  
  // 重组
  Result := Rearrange(Parts, AFileIndex);
  
  // 重新组合扩展名
  Result := Result + Extension;
end;
```

## UI Implementation

### Frame布局

```
┌─────────────────────────────────────────────┐
│ Rearrange                                   │
├─────────────────────────────────────────────┤
│ Split using:                                │
│   ● Delimiters                              │
│   ○ Positions                               │
│   ○ Exact pattern of delimiters            │
│                                             │
│ [_; -                                 ] [⋮]│
│                                             │
│ New pattern:                                │
│ [$2_$1                                ] [▾]│
│                                             │
│ Hint: Use $1..$N to reference delimited   │
│ parts in the new pattern, $-1..$-N to     │
│ reference from the end, $0 for the        │
│ original name.                             │
│                                             │
│ ☑ Skip extension                           │
│ ☐ Right-to-left                            │
└─────────────────────────────────────────────┘
```

### 事件处理

```pascal
procedure TFrame_RuleRearrange.SB_AddSeparatorClick(Sender: TObject);
begin
  Edit_Delimiters.SelText := ';';
  Edit_Delimiters.SetFocus;
end;

procedure TFrame_RuleRearrange.SB_InsertMetaTagClick(Sender: TObject);
var
  PopupMenu: TPopupMenu;
begin
  PopupMenu := CreateMetaTagPopupMenu;
  try
    PopupMenu.Popup(SB_InsertMetaTag.ClientToScreen(
      Point(0, SB_InsertMetaTag.Height)));
  finally
    PopupMenu.Free;
  end;
end;

procedure TFrame_RuleRearrange.Edit_NewPatternKeyDown(Sender: TObject; 
  var Key: Word; Shift: TShiftState);
begin
  // Ctrl+Ins触发Meta Tag插入
  if (Key = VK_INSERT) and (ssCtrl in Shift) then
    SB_InsertMetaTagClick(Sender);
end;
```

## Testing Requirements

### Unit Tests

```pascal
type
  TRuleRearrangeTest = class(TTestCase)
  published
    procedure TestSplitByDelimiters;
    procedure TestSplitByPositions;
    procedure TestRearrangeForward;
    procedure TestRearrangeBackward;
    procedure TestRearrangeOriginal;
    procedure TestMetaTags;
    procedure TestExactPattern;
    procedure TestRightToLeft;
  end;

procedure TRuleRearrangeTest.TestSplitByDelimiters;
var
  Rule: TRuleRearrange;
  Parts: TStringArray;
begin
  Rule := TRuleRearrange.Create;
  try
    Rule.Mode := rmUsingDelimiters;
    Rule.Delimiters := '_';
    
    Parts := Rule.SplitByDelimiters('Last_First_2024');
    AssertEquals(3, Length(Parts));
    AssertEquals('Last', Parts[0]);
    AssertEquals('First', Parts[1]);
    AssertEquals('2024', Parts[2]);
  finally
    Rule.Free;
  end;
end;

procedure TRuleRearrangeTest.TestRearrangeForward;
var
  Rule: TRuleRearrange;
begin
  Rule := TRuleRearrange.Create;
  try
    Rule.Delimiters := '_';
    Rule.NewPattern := '$2 $1';
    
    AssertEquals('First Last.txt', 
                 Rule.Apply('Last_First.txt', 0));
  finally
    Rule.Free;
  end;
end;

procedure TRuleRearrangeTest.TestRearrangeBackward;
var
  Rule: TRuleRearrange;
begin
  Rule := TRuleRearrange.Create;
  try
    Rule.Delimiters := '_';
    Rule.NewPattern := '$-1_$-3';
    
    // Parts: ["A", "B", "C"]
    // $-1 = C, $-3 = A
    AssertEquals('C_A.txt', Rule.Apply('A_B_C.txt', 0));
  finally
    Rule.Free;
  end;
end;
```

### Integration Tests

- 测试Rearrange与其他规则组合
- 测试复杂分隔符模式
- 测试边界条件(无分隔符/无模式)
- 性能测试: 10000个文件应用Rearrange<500ms

## Performance Requirements

- 分隔符拆分: <0.1ms/文件
- 位置拆分: <0.05ms/文件
- 重组: <0.05ms/文件
- UI响应性: RadioButton切换立即更新UI

## 验收标准

WPF 现状：Rearrange 规则已实现；以下清单用于历史对照。

### Phase 1: 基本重排
- [ ] TRuleRearrange类实现
- [ ] 分隔符拆分
- [ ] $1..$N引用
- [ ] 跳过扩展名选项

### Phase 2: 高级引用
- [ ] $-1..$-N反向引用
- [ ] $0原文件名引用
- [ ] 多个分隔符支持

### Phase 3: Meta Tags集成
- [ ] Meta Tag插入UI
- [ ] 与Feature 006集成
- [ ] 组合引用和Meta Tags

### Phase 4: 高级模式
- [ ] 位置拆分
- [ ] 精确模式
- [ ] 从右到左拆分

## Dependencies

### 系统单元
- `SysUtils` - 字符串操作
- `StrUtils` - 字符串工具
- `Generics.Collections` - TList<T>

### 内部依赖
- `Feature 001` - TRule基类
- `Feature 006` - TMetaTagProcessor

## References

- Pascal字符串操作文档
- ReNamer Rearrange规则文档

---

**Version**: 1.0  
**Created**: 2026-02-12  
**Last Updated**: 2026-02-12  
**Related**: Feature 001 (Rule Engine), Feature 006 (Meta Tags)
