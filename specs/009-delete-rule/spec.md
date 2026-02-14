# Feature 009: Delete Rule

## 概述

**Feature Name**: Delete规则 - 按位置删除  
**Priority**: P0（关键 - 核心规则）  
**状态**: ✅ 已实现（WPF）  
**Dependencies**: Feature 001（规则引擎核心）

WPF 对齐说明：规则实现见 `ReNamerWPF/ReNamer/Rules/OtherRules.cs`，配置面板见 `ReNamerWPF/ReNamer/Views/RuleConfigs/DeleteConfigPanel.xaml(.cs)`。

## 问题陈述

Delete规则用于删除文件名中指定范围的字符:
1. **位置/计数删除** - 从第N个字符开始删除M个字符
2. **分隔符删除** - 从分隔符到分隔符/末尾之间删除
3. **方向控制** - 从左到右/从右到左
4. **删除整个文件名** - 特殊模式,删除全部

**应用场景**:
- 删除固定位置的字符(如删除前3个字符)
- 删除从特定分隔符到末尾的内容(如删除"_"之后所有内容)
- 从右边删除指定字符数(如删除最后5个字符)

## 需求（基于 DFM 分析）

以下为原始 DFM/移植参考；WPF 已对齐实现。

### UI组件结构 (对照 docs/rules_requirements.md 第104-128行)

```pascal
type
  TDeleteFromMode = (
    dfmPosition,    // 从位置开始
    dfmDelimiter    // 从分隔符开始
  );
  
  TDeleteUntilMode = (
    dumCount,       // 删除指定字符数
    dumDelimiter,   // 删除到分隔符
    dumEnd          // 删除到末尾
  );

  TFrame_RuleDelete = class(TFrame)
  private
    // From组
    GroupBox_From: TGroupBox;
    RadioButton_DeleteFromPosition: TRadioButton;
    SpinEdit_DeleteFromPosition: TSpinEdit;
    RadioButton_DeleteFromDelimiter: TRadioButton;
    Edit_DeleteFromDelimiter: TEdit;
    
    // Until组
    GroupBox_Until: TGroupBox;
    RadioButton_DeleteUntilCount: TRadioButton;
    SpinEdit_DeleteUntilCount: TSpinEdit;
    RadioButton_DeleteUntilDelimiter: TRadioButton;
    Edit_DeleteUntilDelimiter: TEdit;
    RadioButton_DeleteUntilEnd: TRadioButton;
    
    // 选项
    CheckBox_DeleteCurrentName: TCheckBox;
    CheckBox_DeleteSkipExtension: TCheckBox;
    CheckBox_DeleteRightToLeft: TCheckBox;
    CheckBox_LeaveDelimiter: TCheckBox;
  end;
```

**组件属性**:
```pascal
// GroupBox_From
GroupBox_From.Left := 16;
GroupBox_From.Top := 16;
GroupBox_From.Width := 192;
GroupBox_From.Height := 93;
GroupBox_From.Caption := 'From:';

// SpinEdit_DeleteFromPosition
SpinEdit_DeleteFromPosition.Left := 120;
SpinEdit_DeleteFromPosition.Top := 7;
SpinEdit_DeleteFromPosition.MinValue := 1;
SpinEdit_DeleteFromPosition.MaxValue := 260;
SpinEdit_DeleteFromPosition.Value := 1;

// Edit_DeleteFromDelimiter
Edit_DeleteFromDelimiter.MaxLength := 260;

// GroupBox_Until
GroupBox_Until.Left := 224;
GroupBox_Until.Top := 16;
GroupBox_Until.Width := 193;
GroupBox_Until.Height := 121;
GroupBox_Until.Caption := 'Until:';

// SpinEdit_DeleteUntilCount
SpinEdit_DeleteUntilCount.MinValue := 1;
SpinEdit_DeleteUntilCount.MaxValue := 260;
SpinEdit_DeleteUntilCount.Value := 1;

// CheckBox_DeleteSkipExtension
CheckBox_DeleteSkipExtension.Checked := True;  // 默认跳过扩展名
```

## Implementation Details

### 规则数据结构

```pascal
type
  TRuleDelete = class(TRule)
  private
    FFromMode: TDeleteFromMode;
    FFromPosition: Integer;
    FFromDelimiter: string;
    
    FUntilMode: TDeleteUntilMode;
    FUntilCount: Integer;
    FUntilDelimiter: string;
    
    FDeleteCurrentName: Boolean;  // 删除整个文件名
    FSkipExtension: Boolean;
    FRightToLeft: Boolean;
    FLeaveDelimiter: Boolean;     // 保留分隔符
  public
    constructor Create; override;
    function Apply(const AFileName: string; AFileIndex: Integer): string; override;
    function GetDescription: string; override;
    
    procedure LoadFromJSON(const AJSON: TJSONObject); override;
    procedure SaveToJSON(const AJSON: TJSONObject); override;
  end;
```

### 删除范围计算

```pascal
function TRuleDelete.CalculateDeleteRange(const AText: string;
  out AStartPos, AEndPos: Integer): Boolean;
var
  FromPos, UntilPos: Integer;
  WorkText: string;
begin
  Result := True;
  
  // 如果是删除整个文件名模式
  if FDeleteCurrentName then
  begin
    AStartPos := 1;
    AEndPos := Length(AText);
    Exit;
  end;
  
  // 如果从右到左,反转文本
  if FRightToLeft then
    WorkText := ReverseString(AText)
  else
    WorkText := AText;
  
  // 计算起始位置
  case FFromMode of
    dfmPosition:
    begin
      FromPos := FFromPosition;
      if (FromPos < 1) or (FromPos > Length(WorkText)) then
      begin
        Result := False;  // 超出范围,不删除
        Exit;
      end;
    end;
    
    dfmDelimiter:
    begin
      FromPos := Pos(FFromDelimiter, WorkText);
      if FromPos = 0 then
      begin
        Result := False;  // 未找到分隔符,不删除
        Exit;
      end;
      
      if not FLeaveDelimiter then
        // 包含分隔符
      else
        // 从分隔符之后开始
        FromPos := FromPos + Length(FFromDelimiter);
    end;
  end;
  
  // 计算结束位置
  case FUntilMode of
    dumCount:
    begin
      UntilPos := FromPos + FUntilCount - 1;
      if UntilPos > Length(WorkText) then
        UntilPos := Length(WorkText);
    end;
    
    dumDelimiter:
    begin
      // 从起始位置之后查找分隔符
      UntilPos := PosEx(FUntilDelimiter, WorkText, FromPos);
      if UntilPos = 0 then
      begin
        Result := False;  // 未找到结束分隔符,不删除
        Exit;
      end;
      
      if not FLeaveDelimiter then
        UntilPos := UntilPos + Length(FUntilDelimiter) - 1
      else
        UntilPos := UntilPos - 1;
    end;
    
    dumEnd:
      UntilPos := Length(WorkText);
  end;
  
  // 如果从右到左,转换回正向位置
  if FRightToLeft then
  begin
    AStartPos := Length(AText) - UntilPos + 1;
    AEndPos := Length(AText) - FromPos + 1;
  end
  else
  begin
    AStartPos := FromPos;
    AEndPos := UntilPos;
  end;
end;
```

**删除示例**:
```pascal
// 原文件名: "Document_2024_v1"

// 示例1: 从位置3删除5个字符
FromMode = dfmPosition, FromPosition = 3
UntilMode = dumCount, UntilCount = 5
→ 删除"cumen" → "Do_2024_v1"

// 示例2: 从第一个"_"删除到末尾
FromMode = dfmDelimiter, FromDelimiter = "_"
UntilMode = dumEnd
LeaveDelimiter = False
→ 删除"_2024_v1" → "Document"

// 示例3: 从第一个"_"之后到第二个"_"之前
FromMode = dfmDelimiter, FromDelimiter = "_"
UntilMode = dumDelimiter, UntilDelimiter = "_"
LeaveDelimiter = True
→ 删除"2024" → "Document__v1"

// 示例4: 从右到左,删除最后3个字符
RightToLeft = True
FromMode = dfmPosition, FromPosition = 1
UntilMode = dumCount, UntilCount = 3
→ 删除"_v1" → "Document_2024"
```

### 主应用逻辑

```pascal
function TRuleDelete.Apply(const AFileName: string; 
  AFileIndex: Integer): string;
var
  BaseName, Extension: string;
  StartPos, EndPos: Integer;
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
  
  // 计算删除范围
  if not CalculateDeleteRange(BaseName, StartPos, EndPos) then
  begin
    // 无法计算范围,返回原文件名
    Result := AFileName;
    Exit;
  end;
  
  // 执行删除
  Result := Copy(BaseName, 1, StartPos - 1) + 
            Copy(BaseName, EndPos + 1, Length(BaseName));
  
  // 重新组合扩展名
  Result := Result + Extension;
end;
```

### 描述生成

```pascal
function TRuleDelete.GetDescription: string;
var
  FromDesc, UntilDesc: string;
begin
  if FDeleteCurrentName then
  begin
    Result := 'Delete current name';
    Exit;
  end;
  
  // From描述
  case FFromMode of
    dfmPosition:
      FromDesc := Format('position %d', [FFromPosition]);
    dfmDelimiter:
      FromDesc := Format('delimiter "%s"', [FFromDelimiter]);
  end;
  
  // Until描述
  case FUntilMode of
    dumCount:
      UntilDesc := Format('%d characters', [FUntilCount]);
    dumDelimiter:
      UntilDesc := Format('delimiter "%s"', [FUntilDelimiter]);
    dumEnd:
      UntilDesc := 'end';
  end;
  
  Result := Format('Delete from %s until %s', [FromDesc, UntilDesc]);
  
  if FRightToLeft then
    Result := Result + ' (right-to-left)';
    
  if not FLeaveDelimiter then
    Result := Result + ' (remove delimiters)';
    
  if not FSkipExtension then
    Result := Result + ' (inc. extension)';
end;
```

## UI Implementation

### Frame布局

```
┌─────────────────────────────────────────────┐
│ Delete                                      │
├─────────────────────────────────────────────┤
│ ╔═══════════════╗  ╔══════════════════════╗│
│ ║ From:         ║  ║ Until:               ║│
│ ╠═══════════════╣  ╠══════════════════════╣│
│ ║ ● Position:   ║  ║ ● Count:             ║│
│ ║   [3  ]       ║  ║   [5  ]              ║│
│ ║               ║  ║                      ║│
│ ║ ○ Delimiter:  ║  ║ ○ Delimiter:         ║│
│ ║   [_      ]   ║  ║   [_      ]          ║│
│ ║               ║  ║                      ║│
│ ║               ║  ║ ○ Till the end       ║│
│ ╚═══════════════╝  ╚══════════════════════╝│
│                                             │
│ ☐ Delete current name                      │
│ ☑ Skip extension                           │
│ ☐ Right-to-left                            │
│ ☐ Do not remove delimiters                 │
└─────────────────────────────────────────────┘
```

### 事件处理

```pascal
procedure TFrame_RuleDelete.CheckBox_DeleteCurrentNameChange(Sender: TObject);
begin
  // 勾选时禁用From/Until组
  GroupBox_From.Enabled := not CheckBox_DeleteCurrentName.Checked;
  GroupBox_Until.Enabled := not CheckBox_DeleteCurrentName.Checked;
end;

procedure TFrame_RuleDelete.RadioButtonClick(Sender: TObject);
begin
  // 启用/禁用相关控件
  SpinEdit_DeleteFromPosition.Enabled := 
    RadioButton_DeleteFromPosition.Checked;
  Edit_DeleteFromDelimiter.Enabled := 
    RadioButton_DeleteFromDelimiter.Checked;
    
  SpinEdit_DeleteUntilCount.Enabled := 
    RadioButton_DeleteUntilCount.Checked;
  Edit_DeleteUntilDelimiter.Enabled := 
    RadioButton_DeleteUntilDelimiter.Checked;
    
  // 只有分隔符模式才能使用LeaveDelimiter选项
  CheckBox_LeaveDelimiter.Enabled := 
    RadioButton_DeleteFromDelimiter.Checked or 
    RadioButton_DeleteUntilDelimiter.Checked;
end;
```

## Testing Requirements

### Unit Tests

```pascal
type
  TRuleDeleteTest = class(TTestCase)
  published
    procedure TestDeletePosition;
    procedure TestDeleteDelimiter;
    procedure TestDeleteToEnd;
    procedure TestDeleteRightToLeft;
    procedure TestLeaveDelimiter;
    procedure TestDeleteCurrentName;
    procedure TestSkipExtension;
  end;

procedure TRuleDeleteTest.TestDeletePosition;
var
  Rule: TRuleDelete;
begin
  Rule := TRuleDelete.Create;
  try
    Rule.FromMode := dfmPosition;
    Rule.FromPosition := 3;
    Rule.UntilMode := dumCount;
    Rule.UntilCount := 5;
    
    AssertEquals('Do_2024.txt', Rule.Apply('Document_2024.txt', 0));
  finally
    Rule.Free;
  end;
end;

procedure TRuleDeleteTest.TestDeleteDelimiter;
var
  Rule: TRuleDelete;
begin
  Rule := TRuleDelete.Create;
  try
    Rule.FromMode := dfmDelimiter;
    Rule.FromDelimiter := '_';
    Rule.UntilMode := dumEnd;
    Rule.LeaveDelimiter := False;
    
    AssertEquals('Document.txt', Rule.Apply('Document_2024.txt', 0));
  finally
    Rule.Free;
  end;
end;

procedure TRuleDeleteTest.TestDeleteRightToLeft;
var
  Rule: TRuleDelete;
begin
  Rule := TRuleDelete.Create;
  try
    Rule.FromMode := dfmPosition;
    Rule.FromPosition := 1;
    Rule.UntilMode := dumCount;
    Rule.UntilCount := 3;
    Rule.RightToLeft := True;
    
    // 从右边删除3个字符 "_v1"
    AssertEquals('Document_2024.txt', 
                 Rule.Apply('Document_2024_v1.txt', 0));
  finally
    Rule.Free;
  end;
end;

procedure TRuleDeleteTest.TestDeleteCurrentName;
var
  Rule: TRuleDelete;
begin
  Rule := TRuleDelete.Create;
  try
    Rule.DeleteCurrentName := True;
    
    AssertEquals('.txt', Rule.Apply('anything.txt', 0));
  finally
    Rule.Free;
  end;
end;
```

### Integration Tests

- 测试Delete与Insert组合(先删除后插入)
- 测试分隔符未找到的情况
- 测试边界条件(位置超出范围)
- 性能测试: 10000个文件应用Delete<200ms

## Performance Requirements

- 位置删除: <0.01ms/文件
- 分隔符删除: <0.05ms/文件
- 范围计算: <0.01ms/文件
- UI响应性: CheckBox切换立即更新UI状态

## 验收标准

WPF 现状：Delete 规则已实现；以下清单用于历史对照。

### Phase 1: 基本删除
- [ ] TRuleDelete类实现
- [ ] 位置+计数删除
- [ ] 删除到末尾
- [ ] 跳过扩展名选项

### Phase 2: 分隔符删除
- [ ] 从分隔符删除
- [ ] 删除到分隔符
- [ ] 保留分隔符选项

### Phase 3: 高级功能
- [ ] 从右到左删除
- [ ] 删除整个文件名
- [ ] 边界条件处理

## Dependencies

### 系统单元
- `SysUtils` - 字符串操作
- `StrUtils` - ReverseString, PosEx

### 内部依赖
- `Feature 001` - TRule基类

## References

- Pascal字符串函数文档
- ReNamer Delete规则文档

---

**Version**: 1.0  
**Created**: 2026-02-12  
**Last Updated**: 2026-02-12  
**Related**: Feature 001 (Rule Engine)
