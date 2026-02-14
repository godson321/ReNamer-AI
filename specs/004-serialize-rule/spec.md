# Feature 004: Serialize Rule Implementation

## 概述

**Feature Name**: Serialize规则(序列化/编号)  
**Priority**: P1 (High - 最复杂的规则之一)  
**状态**: ✅ 已实现（WPF）  
**Dependencies**: Feature 001（规则引擎核心）

WPF 对齐说明：规则实现见 `ReNamerWPF/ReNamer/Rules/OtherRules.cs`，配置面板见 `ReNamerWPF/ReNamer/Views/RuleConfigs/SerializeConfigPanel.xaml(.cs)`。

## 问题陈述

Serialize规则是ReNamer中最强大的规则之一,用于为文件添加序号。需要支持:
1. **灵活编号** - 起始值、步长、重复次数
2. **智能重置** - 按文件数、文件夹、文件名变化重置
3. **零填充** - 自动补零到指定长度
4. **多种编号系统** - 十进制、十六进制、八进制、二进制、罗马数字、自定义符号
5. **插入位置** - 前缀、后缀、指定位置、替换整个文件名

WPF版本缺少此规则,需要从DFM定义完整实现。

## 需求（基于 DFM 分析）

以下为 DFM 分析的历史参考；WPF 已实现 Serialize 规则，功能以当前实现为准。

### Frame定义 (对照 docs/rules_requirements.md 第182-213行)

```pascal
type
  TFrame_RuleSerialize = class(TFrame)
  private
    // 编号参数
    SpinEdit_SerializeIndex: TSpinEdit;      // 起始索引
    SE_Repeat: TSpinEdit;                    // 重复次数
    SpinEdit_SerializeStep: TSpinEdit;       // 步长
    
    // 重置条件
    CB_ResetEvery: TCheckBox;                // 每N个文件重置
    SE_ResetEveryCount: TSpinEdit;           // 重置计数
    CheckBox_ResetIfFolderChanges: TCheckBox;// 文件夹变化重置
    CheckBox_ResetIfFileNameChanges: TCheckBox; // 文件名变化重置
    
    // 零填充
    CheckBox_SerializePadToLength: TCheckBox; // 启用零填充
    SpinEdit_SerializePadToLength: TSpinEdit; // 填充长度
    
    // 编号系统
    ComboBox_NumberingSystem: TComboBox;      // 系统选择
    Edit_CustomNumberingSymbols: TEdit;       // 自定义符号
    
    // 插入位置
    GB_InsertWhere: TGroupBox;
    RadioButton_SerializePrefix: TRadioButton;   // 前缀
    RadioButton_SerializeSuffix: TRadioButton;   // 后缀
    RadioButton_SerializePosition: TRadioButton; // 指定位置
    SpinEdit_SerializePosition: TSpinEdit;       // 位置值
    RadioButton_ReplaceCurrentName: TRadioButton;// 替换整个名称
    CheckBox_SkipExtension: TCheckBox;           // 跳过扩展名
  end;
```

### 控件属性详解

#### 编号参数 (第187-189行)
```pascal
// 起始索引
SpinEdit_SerializeIndex: TSpinEdit
  Left = 138
  Top = 24
  Width = 57
  TabOrder = 0
  MinValue = -999999999
  MaxValue = 999999999
  Value = 1  // 默认从1开始
  OnKeyDown = SpinEditKeyDown

// 重复次数
SE_Repeat: TSpinEdit
  Left = 138
  Top = 48
  Width = 57
  TabOrder = 1
  MinValue = 1
  MaxValue = 99999999
  Value = 1  // 默认每个编号只用一次
  OnKeyDown = SpinEditKeyDown

// 步长
SpinEdit_SerializeStep: TSpinEdit
  Left = 138
  Top = 72
  Width = 57
  TabOrder = 2
  MinValue = -99999999
  MaxValue = 99999999
  Value = 1  // 默认每次+1
  OnKeyDown = SpinEditKeyDown
```

**示例**:
- 起始=1, 重复=2, 步长=1 → 1, 1, 2, 2, 3, 3, ...
- 起始=10, 重复=1, 步长=5 → 10, 15, 20, 25, ...
- 起始=100, 重复=3, 步长=-1 → 100, 100, 100, 99, 99, 99, 98, ...

#### 重置条件 (第191-195行)
```pascal
// 每N个文件重置
CB_ResetEvery: TCheckBox
  Caption = "Reset every:"
  Left = 24
  Top = 99
  TabOrder = 3
  Checked = False
  
SE_ResetEveryCount: TSpinEdit
  Left = 138
  Top = 97
  Width = 57
  TabOrder = 4
  MinValue = 1
  MaxValue = 99999999
  Value = 1
  OnChange = SE_ResetEveryCountChange
  OnKeyDown = SpinEditKeyDown

// 文件夹变化重置
CheckBox_ResetIfFolderChanges: TCheckBox
  Caption = "Reset if folder changes"
  Left = 24
  Top = 126
  TabOrder = 5
  Checked = False

// 文件名变化重置
CheckBox_ResetIfFileNameChanges: TCheckBox
  Caption = "Reset if file name changes"
  Left = 24
  Top = 152
  TabOrder = 6
  Checked = False
```

**重置逻辑**:
```pascal
// 伪代码示例
CurrentIndex := StartIndex;
FileCounter := 0;
LastFolder := '';
LastFileName := '';

for each File in FileList do
begin
  // 检查重置条件
  if CB_ResetEvery.Checked and (FileCounter >= SE_ResetEveryCount.Value) then
  begin
    CurrentIndex := StartIndex;
    FileCounter := 0;
  end;
  
  if CheckBox_ResetIfFolderChanges.Checked and (File.Folder <> LastFolder) then
  begin
    CurrentIndex := StartIndex;
    LastFolder := File.Folder;
  end;
  
  if CheckBox_ResetIfFileNameChanges.Checked and (File.BaseName <> LastFileName) then
  begin
    CurrentIndex := StartIndex;
    LastFileName := File.BaseName;
  end;
  
  // 应用编号
  ApplySerialNumber(File, CurrentIndex);
  
  // 更新计数器
  Inc(FileCounter);
  if (FileCounter mod Repeat = 0) then
    CurrentIndex := CurrentIndex + Step;
end;
```

#### 零填充 (第197-199行)
```pascal
CheckBox_SerializePadToLength: TCheckBox
  Caption = "Pad with zeros to length:"
  Left = 24
  Top = 186
  TabOrder = 7
  Checked = False

SpinEdit_SerializePadToLength: TSpinEdit
  Left = 224
  Top = 184
  Width = 57
  TabOrder = 8
  MinValue = 1
  MaxValue = 260  // 文件名最大长度
  Value = 1
  OnChange = SpinEdit_SerializePadToLengthChange
  OnKeyDown = SpinEditKeyDown
```

**填充示例**:
- 编号=5, 填充长度=3 → "005"
- 编号=42, 填充长度=5 → "00042"
- 编号=1000, 填充长度=3 → "1000" (超过长度不截断)

#### 编号系统 (第201-203行)
```pascal
ComboBox_NumberingSystem: TComboBox
  Left = 24
  Top = 240
  Width = 176
  TabOrder = 10
  Style = csDropDownList
  OnChange = ComboBox_NumberingSystemChange
  
  Items:
    - "Decimal (0,1,2,3...)"
    - "Hexadecimal (0,1,2...9,A,B,C...)"
    - "Hexadecimal lowercase"
    - "Octal (0,1,2...7)"
    - "Binary (0,1)"
    - "Roman numerals (I,II,III,IV...)"
    - "Custom symbols"

Edit_CustomNumberingSymbols: TEdit
  Left = 224
  Top = 240
  Width = 193
  TabOrder = 11
  Enabled = False  // 仅当选择"Custom symbols"时启用
```

**编号系统实现**:
```pascal
type
  TNumberingSystem = (
    nsDecimal,
    nsHexadecimalUpper,
    nsHexadecimalLower,
    nsOctal,
    nsBinary,
    nsRomanNumerals,
    nsCustom
  );

function FormatNumber(AValue: Integer; ASystem: TNumberingSystem; 
  ACustomSymbols: string; APadLength: Integer): string;
begin
  case ASystem of
    nsDecimal:
      Result := IntToStr(AValue);
      
    nsHexadecimalUpper:
      Result := IntToHex(AValue, 1);
      
    nsHexadecimalLower:
      Result := LowerCase(IntToHex(AValue, 1));
      
    nsOctal:
      Result := IntToOctal(AValue);
      
    nsBinary:
      Result := IntToBinary(AValue);
      
    nsRomanNumerals:
      Result := IntToRoman(AValue);
      
    nsCustom:
      Result := IntToCustomBase(AValue, ACustomSymbols);
  end;
  
  // 零填充(罗马数字不填充)
  if (ASystem <> nsRomanNumerals) and (APadLength > 0) then
    Result := PadLeft(Result, APadLength, '0');
end;
```

**罗马数字转换**:
```pascal
function IntToRoman(AValue: Integer): string;
const
  Values: array[1..13] of Integer = (1000,900,500,400,100,90,50,40,10,9,5,4,1);
  Numerals: array[1..13] of string = ('M','CM','D','CD','C','XC','L','XL','X','IX','V','IV','I');
var
  I: Integer;
begin
  Result := '';
  for I := 1 to 13 do
  begin
    while AValue >= Values[I] do
    begin
      Result := Result + Numerals[I];
      Dec(AValue, Values[I]);
    end;
  end;
end;
```

**自定义符号**:
```pascal
// 示例: 符号="ABCD", 值=10
// 10 div 4 = 2 余 2 → 'C' + 'C' = "CC"
function IntToCustomBase(AValue: Integer; ASymbols: string): string;
var
  Base: Integer;
  Remainder: Integer;
begin
  if ASymbols = '' then
    raise Exception.Create('Custom symbols cannot be empty');
    
  Base := Length(ASymbols);
  Result := '';
  
  if AValue = 0 then
    Exit(ASymbols[1]);
    
  while AValue > 0 do
  begin
    Remainder := AValue mod Base;
    Result := ASymbols[Remainder + 1] + Result;
    AValue := AValue div Base;
  end;
end;
```

#### 插入位置 (第205-212行)
```pascal
GB_InsertWhere: TGroupBox
  Caption = "Insert where:"
  Left = 224
  Top = 16
  Width = 193
  Height = 144
  TabOrder = 9
  Anchors = [akTop, akRight]

// 前缀
RadioButton_SerializePrefix: TRadioButton
  Caption = "Prefix"
  Left = 14
  Top = 6
  Checked = True  // 默认选中
  TabStop = True
  TabOrder = 0

// 后缀
RadioButton_SerializeSuffix: TRadioButton
  Caption = "Suffix"
  Left = 14
  Top = 27
  TabOrder = 1

// 指定位置
RadioButton_SerializePosition: TRadioButton
  Caption = "Position:"
  Left = 14
  Top = 48
  TabOrder = 2

SpinEdit_SerializePosition: TSpinEdit
  Left = 120
  Top = 46
  Width = 49
  TabOrder = 4
  MinValue = 1
  MaxValue = 260
  Value = 1
  OnChange = SpinEdit_SerializePositionChange
  OnKeyDown = SpinEditKeyDown

// 替换整个文件名
RadioButton_ReplaceCurrentName: TRadioButton
  Caption = "Replace current name"
  Left = 14
  Top = 69
  TabOrder = 3

// 跳过扩展名
CheckBox_SkipExtension: TCheckBox
  Caption = "Skip extension"
  Left = 14
  Top = 96
  TabOrder = 5
  Checked = True  // 默认勾选
  State = cbChecked
```

**插入逻辑**:
```pascal
procedure ApplySerialize(var AFileName: string; ASerialNumber: string; 
  AInsertMode: TSerializeInsertMode; APosition: Integer; ASkipExtension: Boolean);
var
  Name, Ext: string;
begin
  if ASkipExtension then
  begin
    Name := ChangeFileExt(AFileName, '');
    Ext := ExtractFileExt(AFileName);
  end
  else
  begin
    Name := AFileName;
    Ext := '';
  end;
  
  case AInsertMode of
    simPrefix:
      Name := ASerialNumber + Name;
      
    simSuffix:
      Name := Name + ASerialNumber;
      
    simPosition:
      Insert(ASerialNumber, Name, APosition);
      
    simReplace:
      Name := ASerialNumber;
  end;
  
  AFileName := Name + Ext;
end;
```

## Rule Interface Implementation

```pascal
type
  /// Serialize规则参数
  TSerializeParams = record
    // 编号参数
    IndexStarts: Integer;     // 起始值
    Repeat: Integer;          // 重复次数
    Step: Integer;            // 步长
    
    // 重置条件
    ResetEvery: Boolean;      // 启用定期重置
    ResetEveryCount: Integer; // 重置间隔
    ResetIfFolderChanges: Boolean;  // 文件夹变化重置
    ResetIfFileNameChanges: Boolean;// 文件名变化重置
    
    // 零填充
    PadToLength: Boolean;     // 启用填充
    PadLength: Integer;       // 填充长度
    
    // 编号系统
    NumberingSystem: TNumberingSystem;
    CustomSymbols: string;
    
    // 插入位置
    InsertMode: TSerializeInsertMode;
    Position: Integer;
    SkipExtension: Boolean;
  end;

  /// Serialize规则实现
  TSerializeRule = class(TInterfacedObject, IRule)
  private
    FParams: TSerializeParams;
    
    // 运行时状态
    FCurrentIndex: Integer;
    FFileCounter: Integer;
    FLastFolder: string;
    FLastFileName: string;
    
    function FormatSerialNumber(AIndex: Integer): string;
    procedure CheckResetConditions(const AFile: TRenFile);
  public
    constructor Create(const AParams: TSerializeParams);
    
    // IRule 接口
    function GetRuleType: TRuleType;
    function GetDescription: string;
    procedure Execute(var AFileName: string; const AFile: TRenFile);
    procedure Reset;
    function Serialize: TJSONObject;
    class function Deserialize(AJson: TJSONObject): IRule;
  end;
```

### Execute实现

```pascal
procedure TSerializeRule.Execute(var AFileName: string; const AFile: TRenFile);
var
  SerialNumber: string;
  UsageCount: Integer;
begin
  // 检查重置条件
  CheckResetConditions(AFile);
  
  // 格式化序列号
  SerialNumber := FormatSerialNumber(FCurrentIndex);
  
  // 插入序列号到文件名
  ApplySerialize(AFileName, SerialNumber, FParams.InsertMode, 
                 FParams.Position, FParams.SkipExtension);
  
  // 更新计数器
  Inc(FFileCounter);
  
  // 检查是否需要前进到下一个序号
  UsageCount := FFileCounter mod FParams.Repeat;
  if (UsageCount = 0) then
    FCurrentIndex := FCurrentIndex + FParams.Step;
end;
```

### Reset实现

```pascal
procedure TSerializeRule.Reset;
begin
  FCurrentIndex := FParams.IndexStarts;
  FFileCounter := 0;
  FLastFolder := '';
  FLastFileName := '';
end;
```

### CheckResetConditions实现

```pascal
procedure TSerializeRule.CheckResetConditions(const AFile: TRenFile);
var
  CurrentFolder: string;
  CurrentBaseName: string;
begin
  // 每N个文件重置
  if FParams.ResetEvery and (FFileCounter >= FParams.ResetEveryCount) then
  begin
    FCurrentIndex := FParams.IndexStarts;
    FFileCounter := 0;
  end;
  
  // 文件夹变化重置
  if FParams.ResetIfFolderChanges then
  begin
    CurrentFolder := ExtractFilePath(AFile.FullPath);
    if (FLastFolder <> '') and (CurrentFolder <> FLastFolder) then
    begin
      FCurrentIndex := FParams.IndexStarts;
      FFileCounter := 0;
    end;
    FLastFolder := CurrentFolder;
  end;
  
  // 文件名变化重置
  if FParams.ResetIfFileNameChanges then
  begin
    CurrentBaseName := ChangeFileExt(ExtractFileName(AFile.FullPath), '');
    if (FLastFileName <> '') and (CurrentBaseName <> FLastFileName) then
    begin
      FCurrentIndex := FParams.IndexStarts;
      FFileCounter := 0;
    end;
    FLastFileName := CurrentBaseName;
  end;
end;
```

### FormatSerialNumber实现

```pascal
function TSerializeRule.FormatSerialNumber(AIndex: Integer): string;
var
  PadLength: Integer;
begin
  // 确定填充长度
  if FParams.PadToLength then
    PadLength := FParams.PadLength
  else
    PadLength := 0;
    
  // 根据编号系统格式化
  Result := FormatNumber(AIndex, FParams.NumberingSystem, 
                         FParams.CustomSymbols, PadLength);
end;
```

### Serialization实现

```pascal
function TSerializeRule.Serialize: TJSONObject;
begin
  Result := TJSONObject.Create;
  
  // 规则类型
  Result.Add('RuleType', 'Serialize');
  
  // 编号参数
  Result.Add('IndexStarts', FParams.IndexStarts);
  Result.Add('Repeat', FParams.Repeat);
  Result.Add('Step', FParams.Step);
  
  // 重置条件
  Result.Add('ResetEvery', FParams.ResetEvery);
  Result.Add('ResetEveryCount', FParams.ResetEveryCount);
  Result.Add('ResetIfFolderChanges', FParams.ResetIfFolderChanges);
  Result.Add('ResetIfFileNameChanges', FParams.ResetIfFileNameChanges);
  
  // 零填充
  Result.Add('PadToLength', FParams.PadToLength);
  Result.Add('PadLength', FParams.PadLength);
  
  // 编号系统
  Result.Add('NumberingSystem', Ord(FParams.NumberingSystem));
  Result.Add('CustomSymbols', FParams.CustomSymbols);
  
  // 插入位置
  Result.Add('InsertMode', Ord(FParams.InsertMode));
  Result.Add('Position', FParams.Position);
  Result.Add('SkipExtension', FParams.SkipExtension);
end;

class function TSerializeRule.Deserialize(AJson: TJSONObject): IRule;
var
  Params: TSerializeParams;
begin
  Params.IndexStarts := AJson.Get('IndexStarts', 1);
  Params.Repeat := AJson.Get('Repeat', 1);
  Params.Step := AJson.Get('Step', 1);
  
  Params.ResetEvery := AJson.Get('ResetEvery', False);
  Params.ResetEveryCount := AJson.Get('ResetEveryCount', 1);
  Params.ResetIfFolderChanges := AJson.Get('ResetIfFolderChanges', False);
  Params.ResetIfFileNameChanges := AJson.Get('ResetIfFileNameChanges', False);
  
  Params.PadToLength := AJson.Get('PadToLength', False);
  Params.PadLength := AJson.Get('PadLength', 1);
  
  Params.NumberingSystem := TNumberingSystem(AJson.Get('NumberingSystem', 0));
  Params.CustomSymbols := AJson.Get('CustomSymbols', '');
  
  Params.InsertMode := TSerializeInsertMode(AJson.Get('InsertMode', 0));
  Params.Position := AJson.Get('Position', 1);
  Params.SkipExtension := AJson.Get('SkipExtension', True);
  
  Result := TSerializeRule.Create(Params);
end;
```

## Edge Cases & Validation

### 重复次数与步长组合
```pascal
// 场景1: 重复=3, 步长=2, 起始=1
// 结果: 1, 1, 1, 3, 3, 3, 5, 5, 5, ...

// 场景2: 重复=2, 步长=-1, 起始=10
// 结果: 10, 10, 9, 9, 8, 8, 7, 7, ...

// 场景3: 重复=1, 步长=0
// 结果: 1, 1, 1, 1, ... (所有文件相同编号)
```

### 重置条件优先级
```pascal
// 多个重置条件同时满足时,按以下顺序检查:
// 1. ResetEvery (按计数重置)
// 2. ResetIfFolderChanges (文件夹变化)
// 3. ResetIfFileNameChanges (文件名变化)

// 任何一个条件满足即重置,不继续检查后续条件
```

### 编号系统边界
```pascal
// 罗马数字: 仅支持1-3999
if (FParams.NumberingSystem = nsRomanNumerals) and 
   ((FCurrentIndex < 1) or (FCurrentIndex > 3999)) then
  raise Exception.CreateFmt('Roman numerals only support 1-3999, got %d', 
                            [FCurrentIndex]);

// 二进制: 超长字符串
// 1000000 (十进制) = 11110100001001000000 (二进制, 20位)
// 需要考虑文件名长度限制(255字符)

// 自定义符号: 空符号集
if (FParams.NumberingSystem = nsCustom) and 
   (Trim(FParams.CustomSymbols) = '') then
  raise Exception.Create('Custom symbols cannot be empty');
```

### 位置插入边界
```pascal
// Position超过文件名长度时的行为
// 示例: 文件名="abc", Position=10
// 行为: 插入到末尾 → "abc001" (等同于Suffix)

procedure ApplySerialize(var AFileName: string; ASerialNumber: string; 
  AInsertMode: TSerializeInsertMode; APosition: Integer; ASkipExtension: Boolean);
var
  Name, Ext: string;
  ActualPosition: Integer;
begin
  // ... 提取Name和Ext ...
  
  if AInsertMode = simPosition then
  begin
    ActualPosition := Min(APosition, Length(Name) + 1);
    Insert(ASerialNumber, Name, ActualPosition);
  end;
  
  // ...
end;
```

## Testing Requirements

### 单元测试
```pascal
type
  TSerializeRuleTest = class(TTestCase)
  published
    procedure TestBasicSequence;
    procedure TestRepeat;
    procedure TestNegativeStep;
    procedure TestResetEvery;
    procedure TestResetOnFolderChange;
    procedure TestResetOnFileNameChange;
    procedure TestPadding;
    procedure TestHexadecimal;
    procedure TestRomanNumerals;
    procedure TestCustomSymbols;
    procedure TestInsertModes;
  end;

procedure TSerializeRuleTest.TestBasicSequence;
var
  Rule: IRule;
  Params: TSerializeParams;
  FileName: string;
  File: TRenFile;
begin
  // 设置: 起始=1, 步长=1, 重复=1
  Params.IndexStarts := 1;
  Params.Step := 1;
  Params.Repeat := 1;
  Params.InsertMode := simPrefix;
  Params.SkipExtension := True;
  Params.NumberingSystem := nsDecimal;
  Params.PadToLength := False;
  
  Rule := TSerializeRule.Create(Params);
  
  // 测试: 3个文件应生成 1, 2, 3
  FileName := 'file.txt';
  Rule.Execute(FileName, File);
  AssertEquals('1file.txt', FileName);
  
  FileName := 'file.txt';
  Rule.Execute(FileName, File);
  AssertEquals('2file.txt', FileName);
  
  FileName := 'file.txt';
  Rule.Execute(FileName, File);
  AssertEquals('3file.txt', FileName);
end;

procedure TSerializeRuleTest.TestRepeat;
var
  Rule: IRule;
  Params: TSerializeParams;
  FileName: string;
  File: TRenFile;
begin
  // 设置: 起始=10, 步长=5, 重复=2
  Params.IndexStarts := 10;
  Params.Step := 5;
  Params.Repeat := 2;
  Params.InsertMode := simPrefix;
  Params.SkipExtension := True;
  Params.NumberingSystem := nsDecimal;
  Params.PadToLength := False;
  
  Rule := TSerializeRule.Create(Params);
  
  // 测试: 10, 10, 15, 15, 20
  FileName := 'file.txt';
  Rule.Execute(FileName, File);
  AssertEquals('10file.txt', FileName);
  
  FileName := 'file.txt';
  Rule.Execute(FileName, File);
  AssertEquals('10file.txt', FileName);
  
  FileName := 'file.txt';
  Rule.Execute(FileName, File);
  AssertEquals('15file.txt', FileName);
end;

procedure TSerializeRuleTest.TestRomanNumerals;
var
  Rule: IRule;
  Params: TSerializeParams;
  FileName: string;
  File: TRenFile;
begin
  Params.IndexStarts := 1;
  Params.Step := 1;
  Params.Repeat := 1;
  Params.InsertMode := simPrefix;
  Params.SkipExtension := True;
  Params.NumberingSystem := nsRomanNumerals;
  Params.PadToLength := False;
  
  Rule := TSerializeRule.Create(Params);
  
  // I, II, III, IV, V
  FileName := 'file.txt';
  Rule.Execute(FileName, File);
  AssertEquals('Ifile.txt', FileName);
  
  FileName := 'file.txt';
  Rule.Execute(FileName, File);
  AssertEquals('IIfile.txt', FileName);
  
  FileName := 'file.txt';
  Rule.Execute(FileName, File);
  AssertEquals('IIIfile.txt', FileName);
  
  FileName := 'file.txt';
  Rule.Execute(FileName, File);
  AssertEquals('IVfile.txt', FileName);
end;
```

### 集成测试
- 100个文件,验证序列正确性
- 多文件夹场景,测试文件夹重置
- 相同文件名场景,测试文件名重置
- 所有编号系统的转换正确性
- 零填充边界(1位到260位)

## Performance Requirements

- 处理10K文件的序列化<100ms
- 内存占用与文件数量成线性关系
- 罗马数字转换(1-3999)<1μs/次

## 验收标准

WPF 现状：Serialize 规则已实现；以下清单用于历史对照。

### Phase 1: 基本序列
- [ ] 起始值、步长、重复次数正确工作
- [ ] 十进制编号正确
- [ ] 前缀/后缀/位置/替换插入模式正确
- [ ] SkipExtension选项工作

### Phase 2: 重置条件
- [ ] ResetEvery按计数重置
- [ ] ResetIfFolderChanges文件夹变化重置
- [ ] ResetIfFileNameChanges文件名变化重置
- [ ] 多重置条件组合正确

### Phase 3: 编号系统
- [ ] 十六进制(大写/小写)
- [ ] 八进制
- [ ] 二进制
- [ ] 罗马数字(1-3999)
- [ ] 自定义符号

### Phase 4: 零填充
- [ ] PadToLength填充正确
- [ ] 超长数字不截断
- [ ] 罗马数字不填充

### Phase 5: 序列化
- [ ] JSON序列化/反序列化
- [ ] 参数完整保存
- [ ] 预设加载恢复状态

## Dependencies

### 系统单元
- `SysUtils` - 字符串处理
- `Math` - 数学函数
- `fpjson` - JSON序列化

### 自定义单元
- `RuleEngine.Interfaces` - IRule接口
- `RuleEngine.Types` - 规则类型定义

## Next Steps

1. **实现辅助函数**
   - IntToRoman罗马数字转换
   - IntToOctal八进制转换
   - IntToBinary二进制转换
   - IntToCustomBase自定义符号转换

2. **实现TSerializeRule类**
   - 构造函数和字段初始化
   - Execute核心逻辑
   - Reset重置逻辑
   - CheckResetConditions条件检查

3. **实现Frame界面**
   - 创建TFrame_RuleSerialize
   - 绑定所有控件事件
   - 参数验证逻辑

4. **单元测试**
   - 所有编号系统测试
   - 重置条件组合测试
   - 边界条件测试

5. **集成到规则引擎**
   - 注册到TRuleRegistry
   - 添加到规则选择对话框

---

**Version**: 1.0  
**Created**: 2026-02-12  
**Last Updated**: 2026-02-12  
**Related**: Feature 001 (Rule Engine Core), Feature 002 (Main Window UI)
