# Feature 008: Insert Rule

## 概述

**Feature Name**: Insert规则 - 文本插入  
**Priority**: P0（关键 - 核心规则）  
**状态**: ✅ 已实现（WPF）  
**Dependencies**: Feature 001（规则引擎核心）, Feature 006（Meta Tags）

WPF 对齐说明：规则实现见 `ReNamerWPF/ReNamer/Rules/OtherRules.cs`，配置面板见 `ReNamerWPF/ReNamer/Views/RuleConfigs/InsertConfigPanel.xaml(.cs)`。

## 问题陈述

Insert规则用于在文件名的特定位置插入文本或Meta Tags:
1. **灵活插入位置** - 前缀/后缀/指定位置/文本前后/完全替换
2. **Meta Tags支持** - 插入文件属性/EXIF数据/日期时间等
3. **位置计算** - 支持从左/从右计算位置
4. **文本查找定位** - 在指定文本前/后插入

**应用场景**:
- 添加前缀/后缀(如"Photo_"前缀)
- 插入文件元数据(如"_<File:Size:KB>KB")
- 在特定位置插入文本(如第5个字符后插入"_")
- 在特定文本后添加内容(如在"IMG"后添加日期)

## 需求（基于 DFM 分析）

以下为原始 DFM/移植参考；WPF 已对齐实现。

### UI组件结构 (对照 docs/rules_requirements.md 第78-101行)

```pascal
type
  TInsertPosition = (
    ipPrefix,           // 前缀
    ipSuffix,           // 后缀
    ipPosition,         // 指定位置
    ipAfterText,        // 在文本之后
    ipBeforeText,       // 在文本之前
    ipReplaceName       // 替换整个文件名
  );

  TFrame_RuleInsert = class(TFrame)
  private
    // 输入
    Edit_Insert: TEdit;
    BitBtn_InsertMetaTag: TBitBtn;
    
    // 位置选择
    RadioButton_InsertPrefix: TRadioButton;
    RadioButton_InsertSuffix: TRadioButton;
    RadioButton_InsertPosition: TRadioButton;
    SpinEdit_InsertPosition: TSpinEdit;
    CheckBox_InsertRight: TCheckBox;
    RadioButton_InsertAfterText: TRadioButton;
    Edit_InsertAfterText: TEdit;
    RadioButton_InsertBeforeText: TRadioButton;
    Edit_InsertBeforeText: TEdit;
    RadioButton_ReplaceCurrentName: TRadioButton;
    
    // 选项
    CheckBox_InsertSkipExtension: TCheckBox;
  end;
```

**组件属性**:
```pascal
// Edit_Insert
Edit_Insert.Left := 80;
Edit_Insert.Top := 24;
Edit_Insert.Width := 289;
Edit_Insert.Font.Height := -13;

// BitBtn_InsertMetaTag
BitBtn_InsertMetaTag.Left := 80;
BitBtn_InsertMetaTag.Top := 49;
BitBtn_InsertMetaTag.Caption := 'Insert Meta Tag';
BitBtn_InsertMetaTag.Hint := 'Insert Meta Tag (Ctrl+Ins)';

// 位置RadioButtons
RadioButton_InsertPrefix.Checked := True;  // 默认前缀
RadioButton_InsertPosition.Left := 80;
RadioButton_InsertPosition.Top := 128;

// SpinEdit_InsertPosition
SpinEdit_InsertPosition.Left := 208;
SpinEdit_InsertPosition.Top := 126;
SpinEdit_InsertPosition.Width := 57;
SpinEdit_InsertPosition.MinValue := 1;
SpinEdit_InsertPosition.MaxValue := 260;
SpinEdit_InsertPosition.Value := 1;

// CheckBox_InsertRight
CheckBox_InsertRight.Left := 272;
CheckBox_InsertRight.Top := 128;
CheckBox_InsertRight.Caption := 'Right-to-left';

// CheckBox_InsertSkipExtension
CheckBox_InsertSkipExtension.Checked := True;  // 默认跳过扩展名
```

## Implementation Details

### 规则数据结构

```pascal
type
  TRuleInsert = class(TRule)
  private
    FInsertText: string;
    FPosition: TInsertPosition;
    FPositionValue: Integer;         // 用于ipPosition
    FRightToLeft: Boolean;           // 从右计算位置
    FSearchText: string;             // 用于ipAfterText/ipBeforeText
    FSkipExtension: Boolean;
  public
    constructor Create; override;
    function Apply(const AFileName: string; AFileIndex: Integer): string; override;
    function GetDescription: string; override;
    
    procedure LoadFromJSON(const AJSON: TJSONObject); override;
    procedure SaveToJSON(const AJSON: TJSONObject); override;
    
    property InsertText: string read FInsertText write FInsertText;
    property Position: TInsertPosition read FPosition write FPosition;
    property PositionValue: Integer read FPositionValue write FPositionValue;
    property RightToLeft: Boolean read FRightToLeft write FRightToLeft;
    property SearchText: string read FSearchText write FSearchText;
    property SkipExtension: Boolean read FSkipExtension write FSkipExtension;
  end;
```

### Meta Tags处理

```pascal
function TRuleInsert.ProcessInsertText(AFileIndex: Integer): string;
var
  TagProcessor: TMetaTagProcessor;
  FilePath: string;
begin
  // 如果没有Meta Tag,直接返回
  if Pos('<', FInsertText) = 0 then
  begin
    Result := FInsertText;
    Exit;
  end;
  
  // 处理Meta Tags
  FilePath := GetFilePathByIndex(AFileIndex);
  TagProcessor := TMetaTagProcessor.Create;
  try
    Result := TagProcessor.ProcessTags(FInsertText, FilePath, AFileIndex);
  finally
    TagProcessor.Free;
  end;
end;
```

### 位置计算

```pascal
function TRuleInsert.CalculateInsertPosition(const AText: string): Integer;
var
  Pos: Integer;
begin
  case FPosition of
    ipPrefix:
      Result := 1;  // 开头
      
    ipSuffix:
      Result := Length(AText) + 1;  // 结尾
      
    ipPosition:
    begin
      if FRightToLeft then
        // 从右边计算
        Result := Length(AText) - FPositionValue + 2
      else
        // 从左边计算
        Result := FPositionValue;
        
      // 边界检查
      if Result < 1 then
        Result := 1
      else if Result > Length(AText) + 1 then
        Result := Length(AText) + 1;
    end;
    
    ipAfterText:
    begin
      Pos := System.Pos(FSearchText, AText);
      if Pos > 0 then
        Result := Pos + Length(FSearchText)
      else
        Result := -1;  // 未找到,不插入
    end;
    
    ipBeforeText:
    begin
      Pos := System.Pos(FSearchText, AText);
      if Pos > 0 then
        Result := Pos
      else
        Result := -1;  // 未找到,不插入
    end;
    
    ipReplaceName:
      Result := -2;  // 特殊标记:完全替换
  end;
end;
```

**位置示例**:
```pascal
// 原文件名: "Document"
ipPrefix → Position=1
  插入"New_" → "New_Document"

ipSuffix → Position=9 (Length+1)
  插入"_v2" → "Document_v2"

ipPosition, Value=3, LeftToRight → Position=3
  插入"-" → "Do-cument"

ipPosition, Value=3, RightToLeft → Position=6 (8-3+2=7-1=6)
  插入"-" → "Docum-ent"

ipAfterText, Search="Doc" → Position=4
  插入"_" → "Doc_ument"

ipBeforeText, Search="ment" → Position=5
  插入"u" → "Docuument"
```

### 主应用逻辑

```pascal
function TRuleInsert.Apply(const AFileName: string; 
  AFileIndex: Integer): string;
var
  BaseName, Extension: string;
  InsertPos: Integer;
  ProcessedText: string;
begin
  // 如果插入文本为空,直接返回
  if FInsertText = '' then
  begin
    Result := AFileName;
    Exit;
  end;
  
  // 处理Meta Tags
  ProcessedText := ProcessInsertText(AFileIndex);
  
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
  
  // 计算插入位置
  InsertPos := CalculateInsertPosition(BaseName);
  
  // 执行插入
  case InsertPos of
    -2:  // 完全替换
      Result := ProcessedText;
      
    -1:  // 未找到搜索文本,不变
      Result := BaseName;
      
    else
      // 在指定位置插入
      Result := Copy(BaseName, 1, InsertPos - 1) + 
                ProcessedText + 
                Copy(BaseName, InsertPos, Length(BaseName));
  end;
  
  // 重新组合扩展名
  Result := Result + Extension;
end;
```

### 描述生成

```pascal
function TRuleInsert.GetDescription: string;
var
  InsertPreview, PosDesc: string;
const
  MAX_PREVIEW_LEN = 20;
begin
  // 截断过长的插入文本
  if Length(FInsertText) > MAX_PREVIEW_LEN then
    InsertPreview := Copy(FInsertText, 1, MAX_PREVIEW_LEN) + '...'
  else
    InsertPreview := FInsertText;
  
  // 构建位置描述
  case FPosition of
    ipPrefix:
      PosDesc := 'at prefix';
    ipSuffix:
      PosDesc := 'at suffix';
    ipPosition:
      if FRightToLeft then
        PosDesc := Format('at position %d (right-to-left)', [FPositionValue])
      else
        PosDesc := Format('at position %d', [FPositionValue]);
    ipAfterText:
      PosDesc := Format('after "%s"', [FSearchText]);
    ipBeforeText:
      PosDesc := Format('before "%s"', [FSearchText]);
    ipReplaceName:
      PosDesc := 'replace name';
  end;
  
  Result := Format('Insert "%s" %s', [InsertPreview, PosDesc]);
  
  if not FSkipExtension then
    Result := Result + ' (inc. extension)';
end;
```

## UI Implementation

### Frame布局

```
┌─────────────────────────────────────────────┐
│ Insert                                      │
├─────────────────────────────────────────────┤
│ Insert: [Photo_<EXIF:Date:YYYYMMDD>_    ] │
│                                             │
│         [Insert Meta Tag ▾]                │
│                                             │
│ Where:                                      │
│   ● Prefix                                  │
│   ○ Suffix                                  │
│   ○ Position: [5  ] ☐ Right-to-left       │
│   ○ After text:  [IMG     ]                │
│   ○ Before text: [.jpg    ]                │
│   ○ Replace current name                   │
│                                             │
│   ☑ Skip extension                         │
└─────────────────────────────────────────────┘
```

### 事件处理

```pascal
procedure TFrame_RuleInsert.BitBtn_InsertMetaTagClick(Sender: TObject);
var
  PopupMenu: TPopupMenu;
  MenuItem: TMenuItem;
begin
  PopupMenu := TPopupMenu.Create(Self);
  try
    // 文件属性
    MenuItem := TMenuItem.Create(PopupMenu);
    MenuItem.Caption := 'File Properties';
    AddMetaTagSubItems(MenuItem, mtgFileProperties);
    PopupMenu.Items.Add(MenuItem);
    
    // EXIF
    MenuItem := TMenuItem.Create(PopupMenu);
    MenuItem.Caption := 'EXIF Data';
    AddMetaTagSubItems(MenuItem, mtgEXIF);
    PopupMenu.Items.Add(MenuItem);
    
    // 日期时间
    MenuItem := TMenuItem.Create(PopupMenu);
    MenuItem.Caption := 'Date/Time';
    AddMetaTagSubItems(MenuItem, mtgDateTime);
    PopupMenu.Items.Add(MenuItem);
    
    // 弹出菜单
    PopupMenu.PopupComponent := BitBtn_InsertMetaTag;
    PopupMenu.Popup(BitBtn_InsertMetaTag.ClientToScreen(
      Point(0, BitBtn_InsertMetaTag.Height)));
  finally
    PopupMenu.Free;
  end;
end;

procedure TFrame_RuleInsert.InsertMetaTag(const ATag: string);
begin
  Edit_Insert.SelText := ATag;
  Edit_Insert.SetFocus;
end;

procedure TFrame_RuleInsert.RadioButtonClick(Sender: TObject);
begin
  // 启用/禁用相关控件
  SpinEdit_InsertPosition.Enabled := RadioButton_InsertPosition.Checked;
  CheckBox_InsertRight.Enabled := RadioButton_InsertPosition.Checked;
  Edit_InsertAfterText.Enabled := RadioButton_InsertAfterText.Checked;
  Edit_InsertBeforeText.Enabled := RadioButton_InsertBeforeText.Checked;
end;
```

## Testing Requirements

### Unit Tests

```pascal
type
  TRuleInsertTest = class(TTestCase)
  published
    procedure TestPrefix;
    procedure TestSuffix;
    procedure TestPosition;
    procedure TestPositionRightToLeft;
    procedure TestAfterText;
    procedure TestBeforeText;
    procedure TestReplaceName;
    procedure TestMetaTags;
    procedure TestSkipExtension;
  end;

procedure TRuleInsertTest.TestPrefix;
var
  Rule: TRuleInsert;
begin
  Rule := TRuleInsert.Create;
  try
    Rule.InsertText := 'New_';
    Rule.Position := ipPrefix;
    
    AssertEquals('New_file.txt', Rule.Apply('file.txt', 0));
  finally
    Rule.Free;
  end;
end;

procedure TRuleInsertTest.TestPositionRightToLeft;
var
  Rule: TRuleInsert;
begin
  Rule := TRuleInsert.Create;
  try
    Rule.InsertText := '_';
    Rule.Position := ipPosition;
    Rule.PositionValue := 3;
    Rule.RightToLeft := True;
    
    // "Document" Length=8, 8-3+2=7, 插入在第7个字符前
    // "Docume_nt"
    AssertEquals('Docume_nt.txt', Rule.Apply('Document.txt', 0));
  finally
    Rule.Free;
  end;
end;

procedure TRuleInsertTest.TestAfterText;
var
  Rule: TRuleInsert;
begin
  Rule := TRuleInsert.Create;
  try
    Rule.InsertText := '_Photo';
    Rule.Position := ipAfterText;
    Rule.SearchText := 'IMG';
    
    AssertEquals('IMG_Photo_001.jpg', Rule.Apply('IMG_001.jpg', 0));
    AssertEquals('DSC_001.jpg', Rule.Apply('DSC_001.jpg', 0));  // 未找到,不变
  finally
    Rule.Free;
  end;
end;
```

### Integration Tests

- 测试Insert与Serialize组合(先Insert后编号)
- 测试Meta Tags的正确展开
- 测试边界条件(空文件名/位置超出范围)
- 性能测试: 10000个文件应用Insert<300ms

## Performance Requirements

- 简单插入: <0.02ms/文件
- Meta Tags处理: <5ms/文件
- 位置计算: <0.01ms/文件
- UI响应性: RadioButton切换立即更新UI状态

## 验收标准

WPF 现状：Insert 规则与 Meta Tags 已实现；以下清单用于历史对照。

### Phase 1: 基本插入
- [ ] TRuleInsert类实现
- [ ] 前缀/后缀插入
- [ ] 指定位置插入
- [ ] 跳过扩展名选项

### Phase 2: 高级定位
- [ ] 从右计算位置
- [ ] 在文本前/后插入
- [ ] 替换整个文件名

### Phase 3: Meta Tags集成
- [ ] Meta Tag按钮UI
- [ ] 与Feature 006集成
- [ ] 所有31个Meta Tag测试

## Dependencies

### 系统单元
- `SysUtils` - 字符串操作
- `StrUtils` - 字符串工具

### 内部依赖
- `Feature 001` - TRule基类
- `Feature 006` - TMetaTagProcessor

## References

- Pascal字符串函数文档
- ReNamer Insert规则文档

---

**Version**: 1.0  
**Created**: 2026-02-12  
**Last Updated**: 2026-02-12  
**Related**: Feature 001 (Rule Engine), Feature 006 (Meta Tags)
