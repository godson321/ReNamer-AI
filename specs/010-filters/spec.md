# Feature 010: File Filters System

## 概述

**Feature Name**: 文件过滤器系统  
**Priority**: P1 (High - 批量处理的关键功能)  
**状态**: ✅ 已实现（WPF）  
**Dependencies**: Feature 002（主窗口 UI）

WPF 对齐说明：过滤器配置见 `ReNamerWPF/ReNamer/Views/FiltersDialog.xaml(.cs)`，持久化见 `ReNamerWPF/ReNamer/Services/AppSettings.cs`。

## 问题陈述

批量文件重命名时,用户需要精确控制哪些文件被包含/排除。Filters系统提供:
1. **文件名模式匹配** - 通配符/正则表达式包含/排除
2. **大小过滤** - 最小/最大文件大小范围
3. **日期过滤** - 创建/修改日期范围
4. **属性过滤** - 只读/隐藏/系统文件等
5. **文件夹递归控制** - 递归深度/排除特定文件夹

**应用场景**:
- 添加文件夹时自动过滤(Add Folders递归扫描)
- 批量标记/取消标记文件
- 导入文件列表时过滤

**核心原则**: 过滤器仅影响**文件添加时**的扫描,已添加的文件不受影响(通过Mark/Clear手动管理)

## 需求（基于 DFM 分析）

以下为 DFM 分析的历史参考；WPF 已实现过滤器系统，功能以当前实现为准。

### 访问入口 (对照 docs/rules_requirements.md 第1352行)

```pascal
// 方式1: 菜单
MenuItem_Filters: TMenuItem
  Caption = "Filters"
  ShortCut = Ctrl+F
  OnClick = MenuItemFiltersClick

// 方式2: 工具栏按钮
ToolButton_Filters: TToolButton
  Caption = "Filters"
  ImageIndex = 3
  OnClick = ToolButton_FiltersClick

// 方式3: 快捷键
Action_Filters: TAction
  Caption = "Filters"
  ShortCut = VK_F (Ctrl+F)
  OnExecute = ActionFiltersExecute
```

### Filters对话框 (FormFilters)

```pascal
type
  TFormFilters = class(TForm)
  private
    // 启用状态
    CheckBox_EnableFilters: TCheckBox;
    
    // 文件名过滤
    GroupBox_FileName: TGroupBox;
    RadioButton_IncludeMask: TRadioButton;
    RadioButton_ExcludeMask: TRadioButton;
    Edit_FileMask: TEdit;
    CheckBox_CaseSensitive: TCheckBox;
    CheckBox_UseRegEx: TCheckBox;
    
    // 大小过滤
    GroupBox_FileSize: TGroupBox;
    CheckBox_EnableSizeFilter: TCheckBox;
    SpinEdit_MinSize: TSpinEdit;
    ComboBox_MinSizeUnit: TComboBox;  // Bytes/KB/MB/GB
    SpinEdit_MaxSize: TSpinEdit;
    ComboBox_MaxSizeUnit: TComboBox;
    
    // 日期过滤
    GroupBox_DateTime: TGroupBox;
    CheckBox_EnableDateFilter: TCheckBox;
    RadioButton_CreatedDate: TRadioButton;
    RadioButton_ModifiedDate: TRadioButton;
    DateTimePicker_FromDate: TDateTimePicker;
    DateTimePicker_ToDate: TDateTimePicker;
    
    // 属性过滤
    GroupBox_Attributes: TGroupBox;
    CheckBox_IncludeReadOnly: TCheckBox;
    CheckBox_IncludeHidden: TCheckBox;
    CheckBox_IncludeSystem: TCheckBox;
    CheckBox_IncludeArchive: TCheckBox;
    
    // 文件夹控制
    GroupBox_Folders: TGroupBox;
    CheckBox_RecursiveSubfolders: TCheckBox;
    SpinEdit_MaxDepth: TSpinEdit;
    Edit_ExcludeFolders: TEdit;
    
    // 按钮
    Button_OK: TButton;
    Button_Cancel: TButton;
    Button_Reset: TButton;
  end;
```

## Filter Types (过滤器类型)

### 1. 文件名模式过滤 (Filename Mask)

**通配符模式** (默认):
```pascal
type
  TFileMaskMode = (
    fmmInclude,  // 包含匹配的文件
    fmmExclude   // 排除匹配的文件
  );

function MatchesMask(const AFileName, AMask: string; 
  ACaseSensitive: Boolean): Boolean;
var
  Masks: TStringArray;
  I: Integer;
begin
  Result := False;
  
  // 支持多个mask,用分号分隔
  Masks := AMask.Split([';']);
  
  for I := 0 to High(Masks) do
  begin
    if MatchesWildcard(AFileName, Trim(Masks[I]), ACaseSensitive) then
    begin
      Result := True;
      Exit;
    end;
  end;
end;

function MatchesWildcard(const AText, APattern: string; 
  ACaseSensitive: Boolean): Boolean;
var
  Text, Pattern: string;
begin
  if ACaseSensitive then
  begin
    Text := AText;
    Pattern := APattern;
  end
  else
  begin
    Text := LowerCase(AText);
    Pattern := LowerCase(APattern);
  end;
  
  // 使用Masks单元的MatchesMask
  Result := Masks.MatchesMask(Text, Pattern);
end;
```

**通配符支持**:
- `*` - 匹配任意字符(0个或多个)
- `?` - 匹配单个字符
- `[abc]` - 匹配集合中任意字符
- `[!abc]` - 匹配不在集合中的字符

**示例**:
```pascal
// 包含模式
"*.jpg;*.png;*.gif"  // 仅图片
"file*.txt"          // file开头的txt文件
"[ABC]*"             // A/B/C开头的文件

// 排除模式
"*.tmp;*.bak"        // 排除临时/备份文件
"~*"                 // 排除波浪号开头的文件
"*_backup_*"         // 排除包含_backup_的文件
```

**正则表达式模式** (可选):
```pascal
function MatchesRegEx(const AFileName, ARegEx: string; 
  ACaseSensitive: Boolean): Boolean;
var
  RegEx: TRegExpr;
begin
  RegEx := TRegExpr.Create;
  try
    RegEx.Expression := ARegEx;
    RegEx.ModifierI := not ACaseSensitive;  // i = case insensitive
    Result := RegEx.Exec(AFileName);
  finally
    RegEx.Free;
  end;
end;
```

**示例**:
```pascal
// 正则表达式
"^\d{4}-\d{2}-\d{2}"  // YYYY-MM-DD开头的文件
".*\.(jpg|png|gif)$"  // 图片扩展名
"^(IMG|DSC)_\d+"      // IMG_或DSC_加数字
```

### 2. 文件大小过滤 (File Size)

```pascal
type
  TSizeUnit = (suBytes, suKB, suMB, suGB);
  
  TFileSizeFilter = record
    Enabled: Boolean;
    MinSize: Int64;     // 字节
    MaxSize: Int64;     // 字节
  end;

function ConvertToBytes(ASize: Integer; AUnit: TSizeUnit): Int64;
begin
  case AUnit of
    suBytes: Result := ASize;
    suKB:    Result := ASize * 1024;
    suMB:    Result := ASize * 1024 * 1024;
    suGB:    Result := ASize * Int64(1024) * 1024 * 1024;
  end;
end;

function PassesSizeFilter(AFileSize: Int64; 
  const AFilter: TFileSizeFilter): Boolean;
begin
  if not AFilter.Enabled then
  begin
    Result := True;
    Exit;
  end;
  
  Result := (AFileSize >= AFilter.MinSize) and 
            (AFileSize <= AFilter.MaxSize);
end;
```

**UI配置**:
```pascal
// 最小大小
SpinEdit_MinSize.Value := 0;
ComboBox_MinSizeUnit.ItemIndex := 1;  // KB

// 最大大小
SpinEdit_MaxSize.Value := 100;
ComboBox_MaxSizeUnit.ItemIndex := 2;  // MB

// 结果: 0KB - 100MB
MinSize := ConvertToBytes(0, suKB);      // 0 bytes
MaxSize := ConvertToBytes(100, suMB);     // 104,857,600 bytes
```

**示例场景**:
- 仅处理小文件: 0 - 10MB
- 仅处理大文件: 100MB - 无限制
- 排除空文件: 1 byte - 无限制
- 处理中等文件: 1KB - 1GB

### 3. 日期过滤 (Date/Time)

```pascal
type
  TDateFilterType = (
    dftCreated,    // 创建日期
    dftModified,   // 修改日期
    dftAccessed    // 访问日期
  );
  
  TDateFilter = record
    Enabled: Boolean;
    FilterType: TDateFilterType;
    FromDate: TDateTime;
    ToDate: TDateTime;
  end;

function PassesDateFilter(const AFilePath: string; 
  const AFilter: TDateFilter): Boolean;
var
  FileInfo: TSearchRec;
  FileDate: TDateTime;
begin
  if not AFilter.Enabled then
  begin
    Result := True;
    Exit;
  end;
  
  if FindFirst(AFilePath, faAnyFile, FileInfo) <> 0 then
  begin
    Result := False;
    Exit;
  end;
  
  try
    case AFilter.FilterType of
      dftCreated:  FileDate := FileInfo.TimeStamp;  // 创建时间
      dftModified: FileDate := FileInfo.TimeStamp;  // 修改时间
      dftAccessed: FileDate := Now;  // Windows不直接提供访问时间
    end;
    
    Result := (FileDate >= AFilter.FromDate) and 
              (FileDate <= AFilter.ToDate);
  finally
    FindClose(FileInfo);
  end;
end;
```

**UI配置**:
```pascal
// 过滤修改日期在2024-01-01到2024-12-31的文件
RadioButton_ModifiedDate.Checked := True;
DateTimePicker_FromDate.Date := EncodeDate(2024, 1, 1);
DateTimePicker_ToDate.Date := EncodeDate(2024, 12, 31);
```

**示例场景**:
- 最近7天修改的文件
- 2023年创建的文件
- 特定日期范围的照片

### 4. 文件属性过滤 (File Attributes)

```pascal
type
  TAttributeFilter = record
    IncludeReadOnly: Boolean;   // 包含只读文件
    IncludeHidden: Boolean;     // 包含隐藏文件
    IncludeSystem: Boolean;     // 包含系统文件
    IncludeArchive: Boolean;    // 包含存档文件
  end;

function PassesAttributeFilter(const AFilePath: string; 
  const AFilter: TAttributeFilter): Boolean;
var
  Attrs: Integer;
begin
  Attrs := FileGetAttr(AFilePath);
  
  if Attrs = -1 then
  begin
    Result := False;
    Exit;
  end;
  
  // 检查只读属性
  if (Attrs and faReadOnly <> 0) and not AFilter.IncludeReadOnly then
  begin
    Result := False;
    Exit;
  end;
  
  // 检查隐藏属性
  if (Attrs and faHidden <> 0) and not AFilter.IncludeHidden then
  begin
    Result := False;
    Exit;
  end;
  
  // 检查系统属性
  if (Attrs and faSysFile <> 0) and not AFilter.IncludeSystem then
  begin
    Result := False;
    Exit;
  end;
  
  // 检查存档属性
  if (Attrs and faArchive <> 0) and not AFilter.IncludeArchive then
  begin
    Result := False;
    Exit;
  end;
  
  Result := True;
end;
```

**默认配置**:
```pascal
// 默认包含普通文件,排除隐藏/系统文件
CheckBox_IncludeReadOnly.Checked := True;   // 包含只读
CheckBox_IncludeHidden.Checked := False;    // 排除隐藏
CheckBox_IncludeSystem.Checked := False;    // 排除系统
CheckBox_IncludeArchive.Checked := True;    // 包含存档
```

### 5. 文件夹控制 (Folder Control)

```pascal
type
  TFolderFilter = record
    Recursive: Boolean;            // 递归子文件夹
    MaxDepth: Integer;             // 最大递归深度(0=无限制)
    ExcludeFolders: TStringList;   // 排除的文件夹名(通配符)
  end;

function ShouldScanFolder(const AFolderName: string; 
  ACurrentDepth: Integer; const AFilter: TFolderFilter): Boolean;
var
  I: Integer;
begin
  // 检查递归设置
  if not AFilter.Recursive then
  begin
    Result := (ACurrentDepth = 0);
    Exit;
  end;
  
  // 检查深度限制
  if (AFilter.MaxDepth > 0) and (ACurrentDepth >= AFilter.MaxDepth) then
  begin
    Result := False;
    Exit;
  end;
  
  // 检查排除文件夹
  for I := 0 to AFilter.ExcludeFolders.Count - 1 do
  begin
    if MatchesWildcard(AFolderName, AFilter.ExcludeFolders[I], False) then
    begin
      Result := False;
      Exit;
    end;
  end;
  
  Result := True;
end;
```

**UI配置**:
```pascal
// 递归子文件夹,最大深度3层,排除node_modules和.git
CheckBox_RecursiveSubfolders.Checked := True;
SpinEdit_MaxDepth.Value := 3;
Edit_ExcludeFolders.Text := 'node_modules;.git;__pycache__';
```

**示例**:
```
Folder/
├── File1.txt         ← 深度0, 扫描
├── SubA/
│   ├── File2.txt     ← 深度1, 扫描
│   └── SubB/
│       └── File3.txt ← 深度2, 扫描
├── SubC/
│   └── SubD/
│       └── SubE/
│           └── File4.txt ← 深度3, 超过限制,跳过
└── node_modules/     ← 排除文件夹,跳过
    └── ...
```

## Filter Persistence (过滤器持久化)

### 配置保存

```pascal
type
  TFilterConfig = record
    // 启用状态
    Enabled: Boolean;
    
    // 文件名过滤
    FileMaskMode: TFileMaskMode;
    FileMask: string;
    CaseSensitive: Boolean;
    UseRegEx: Boolean;
    
    // 大小过滤
    SizeFilter: TFileSizeFilter;
    
    // 日期过滤
    DateFilter: TDateFilter;
    
    // 属性过滤
    AttributeFilter: TAttributeFilter;
    
    // 文件夹控制
    FolderFilter: TFolderFilter;
  end;

procedure SaveFilterConfig(const AConfig: TFilterConfig);
var
  JSON: TJSONObject;
begin
  JSON := TJSONObject.Create;
  try
    JSON.Add('Enabled', AConfig.Enabled);
    JSON.Add('FileMask', AConfig.FileMask);
    JSON.Add('FileMaskMode', Ord(AConfig.FileMaskMode));
    JSON.Add('CaseSensitive', AConfig.CaseSensitive);
    JSON.Add('UseRegEx', AConfig.UseRegEx);
    
    JSON.Add('SizeFilterEnabled', AConfig.SizeFilter.Enabled);
    JSON.Add('MinSize', AConfig.SizeFilter.MinSize);
    JSON.Add('MaxSize', AConfig.SizeFilter.MaxSize);
    
    JSON.Add('DateFilterEnabled', AConfig.DateFilter.Enabled);
    JSON.Add('DateFilterType', Ord(AConfig.DateFilter.FilterType));
    JSON.Add('FromDate', DateTimeToStr(AConfig.DateFilter.FromDate));
    JSON.Add('ToDate', DateTimeToStr(AConfig.DateFilter.ToDate));
    
    JSON.Add('IncludeReadOnly', AConfig.AttributeFilter.IncludeReadOnly);
    JSON.Add('IncludeHidden', AConfig.AttributeFilter.IncludeHidden);
    JSON.Add('IncludeSystem', AConfig.AttributeFilter.IncludeSystem);
    JSON.Add('IncludeArchive', AConfig.AttributeFilter.IncludeArchive);
    
    JSON.Add('Recursive', AConfig.FolderFilter.Recursive);
    JSON.Add('MaxDepth', AConfig.FolderFilter.MaxDepth);
    JSON.Add('ExcludeFolders', AConfig.FolderFilter.ExcludeFolders.CommaText);
    
    // 保存到配置文件
    SaveJSONToFile(JSON, GetConfigPath + 'filters.json');
  finally
    JSON.Free;
  end;
end;
```

## Integration (集成到文件添加流程)

### Add Folders 递归扫描

```pascal
procedure TFormMain.AddFolderRecursive(const AFolderPath: string; 
  ADepth: Integer; const AFilters: TFilterConfig);
var
  SearchRec: TSearchRec;
  FilePath: string;
  FolderName: string;
begin
  // 检查文件夹是否应扫描
  FolderName := ExtractFileName(ExcludeTrailingPathDelimiter(AFolderPath));
  if not ShouldScanFolder(FolderName, ADepth, AFilters.FolderFilter) then
    Exit;
    
  // 扫描文件
  if FindFirst(AFolderPath + '*.*', faAnyFile, SearchRec) = 0 then
  begin
    try
      repeat
        if (SearchRec.Name = '.') or (SearchRec.Name = '..') then
          Continue;
          
        FilePath := AFolderPath + SearchRec.Name;
        
        // 如果是文件夹,递归扫描
        if (SearchRec.Attr and faDirectory) <> 0 then
        begin
          AddFolderRecursive(FilePath + PathDelim, ADepth + 1, AFilters);
        end
        // 如果是文件,应用过滤器
        else
        begin
          if PassesAllFilters(FilePath, SearchRec, AFilters) then
            AddFileToList(FilePath);
        end;
      until FindNext(SearchRec) <> 0;
    finally
      FindClose(SearchRec);
    end;
  end;
end;

function TFormMain.PassesAllFilters(const AFilePath: string; 
  const ASearchRec: TSearchRec; const AFilters: TFilterConfig): Boolean;
begin
  // 如果过滤器未启用,全部通过
  if not AFilters.Enabled then
  begin
    Result := True;
    Exit;
  end;
  
  // 文件名过滤
  if AFilters.FileMask <> '' then
  begin
    if AFilters.UseRegEx then
    begin
      if not MatchesRegEx(ASearchRec.Name, AFilters.FileMask, 
                          AFilters.CaseSensitive) then
      begin
        Result := (AFilters.FileMaskMode = fmmExclude);
        Exit;
      end;
    end
    else
    begin
      if not MatchesMask(ASearchRec.Name, AFilters.FileMask, 
                         AFilters.CaseSensitive) then
      begin
        Result := (AFilters.FileMaskMode = fmmExclude);
        Exit;
      end;
    end;
    
    // 匹配成功
    if AFilters.FileMaskMode = fmmExclude then
    begin
      Result := False;  // 排除模式,匹配的不通过
      Exit;
    end;
  end;
  
  // 大小过滤
  if not PassesSizeFilter(ASearchRec.Size, AFilters.SizeFilter) then
  begin
    Result := False;
    Exit;
  end;
  
  // 日期过滤
  if not PassesDateFilter(AFilePath, AFilters.DateFilter) then
  begin
    Result := False;
    Exit;
  end;
  
  // 属性过滤
  if not PassesAttributeFilter(AFilePath, AFilters.AttributeFilter) then
  begin
    Result := False;
    Exit;
  end;
  
  Result := True;
end;
```

### Apply to Existing Files (应用到已有文件)

```pascal
// 批量标记/取消标记符合过滤器的文件
procedure TFormMain.ApplyFiltersToFileList(AMark: Boolean);
var
  Node: PVirtualNode;
  Data: PFileData;
  SearchRec: TSearchRec;
begin
  VSTFiles.BeginUpdate;
  try
    Node := VSTFiles.GetFirst;
    while Node <> nil do
    begin
      Data := VSTFiles.GetNodeData(Node);
      
      if FindFirst(Data^.FullPath, faAnyFile, SearchRec) = 0 then
      begin
        try
          if PassesAllFilters(Data^.FullPath, SearchRec, CurrentFilters) then
          begin
            Node^.CheckState := csCheckedNormal;
            Data^.IsMarked := True;
          end
          else if AMark = False then  // 仅在取消标记模式下清除
          begin
            Node^.CheckState := csUncheckedNormal;
            Data^.IsMarked := False;
          end;
        finally
          FindClose(SearchRec);
        end;
      end;
      
      Node := VSTFiles.GetNext(Node);
    end;
  finally
    VSTFiles.EndUpdate;
  end;
  
  UpdateStatusBar;
end;
```

## UI Implementation

### Filters Dialog Layout

```
┌────────────────────────────────────────────────────┐
│ Filters                                       [X]  │
├────────────────────────────────────────────────────┤
│ ☑ Enable Filters                                  │
│                                                    │
│ ╔═══════════════════════════════════════════════╗ │
│ ║ Filename Pattern                              ║ │
│ ╠═══════════════════════════════════════════════╣ │
│ ║ ○ Include matching files                     ║ │
│ ║ ● Exclude matching files                     ║ │
│ ║                                               ║ │
│ ║ Mask: [*.tmp;*.bak;~*          ]             ║ │
│ ║ ☐ Case sensitive  ☐ Use RegEx                ║ │
│ ╚═══════════════════════════════════════════════╝ │
│                                                    │
│ ╔═══════════════════════════════════════════════╗ │
│ ║ File Size                                     ║ │
│ ╠═══════════════════════════════════════════════╣ │
│ ║ ☑ Enable size filter                         ║ │
│ ║ Min: [0     ] [KB ▾]  Max: [100  ] [MB ▾]   ║ │
│ ╚═══════════════════════════════════════════════╝ │
│                                                    │
│ ╔═══════════════════════════════════════════════╗ │
│ ║ Date/Time                                     ║ │
│ ╠═══════════════════════════════════════════════╣ │
│ ║ ☐ Enable date filter                         ║ │
│ ║ ● Modified date  ○ Created date              ║ │
│ ║ From: [2024-01-01 📅]  To: [2024-12-31 📅]  ║ │
│ ╚═══════════════════════════════════════════════╝ │
│                                                    │
│ ╔═══════════════════════════════════════════════╗ │
│ ║ File Attributes                               ║ │
│ ╠═══════════════════════════════════════════════╣ │
│ ║ ☑ Read-only  ☐ Hidden  ☐ System  ☑ Archive  ║ │
│ ╚═══════════════════════════════════════════════╝ │
│                                                    │
│ ╔═══════════════════════════════════════════════╗ │
│ ║ Folder Control                                ║ │
│ ╠═══════════════════════════════════════════════╣ │
│ ║ ☑ Recursive subfolders  Max depth: [0  ] ∞  ║ │
│ ║ Exclude: [node_modules;.git;__pycache__]     ║ │
│ ╚═══════════════════════════════════════════════╝ │
│                                                    │
│                    [Reset]  [Cancel]  [OK]        │
└────────────────────────────────────────────────────┘
```

## Testing Requirements

### Unit Tests

```pascal
type
  TFilterTest = class(TTestCase)
  published
    procedure TestWildcardMask;
    procedure TestRegExMask;
    procedure TestSizeFilter;
    procedure TestDateFilter;
    procedure TestAttributeFilter;
    procedure TestFolderRecursion;
    procedure TestExcludeFolders;
  end;

procedure TFilterTest.TestWildcardMask;
begin
  AssertTrue(MatchesWildcard('test.txt', '*.txt', False));
  AssertTrue(MatchesWildcard('IMG_001.jpg', 'IMG_*.jpg', False));
  AssertFalse(MatchesWildcard('test.doc', '*.txt', False));
  
  // Case sensitive
  AssertTrue(MatchesWildcard('Test.TXT', '*.txt', False));
  AssertFalse(MatchesWildcard('Test.TXT', '*.txt', True));
end;

procedure TFilterTest.TestSizeFilter;
var
  Filter: TFileSizeFilter;
begin
  Filter.Enabled := True;
  Filter.MinSize := 1024;       // 1KB
  Filter.MaxSize := 1048576;    // 1MB
  
  AssertTrue(PassesSizeFilter(1024, Filter));      // 1KB - 通过
  AssertTrue(PassesSizeFilter(500000, Filter));    // 500KB - 通过
  AssertFalse(PassesSizeFilter(500, Filter));      // 500B - 失败
  AssertFalse(PassesSizeFilter(2097152, Filter));  // 2MB - 失败
end;
```

### Integration Tests

- 添加包含10000个文件的文件夹,验证过滤正确性
- 测试所有过滤器组合(文件名+大小+日期+属性)
- 测试递归深度限制
- 测试排除文件夹功能
- 性能测试: 扫描100000文件<5s

## Performance Requirements

- 文件名匹配(通配符): <0.1ms/文件
- 文件名匹配(正则): <1ms/文件
- 递归扫描10000文件: <2s
- UI响应性: 过滤器对话框打开<100ms

## 验收标准

WPF 现状：过滤器系统已实现；以下清单用于历史对照。

### Phase 1: 基本过滤器
- [ ] Filters对话框UI创建
- [ ] 文件名通配符过滤
- [ ] 大小过滤
- [ ] 启用/禁用开关

### Phase 2: 高级过滤
- [ ] 正则表达式支持
- [ ] 日期过滤
- [ ] 属性过滤
- [ ] 配置持久化

### Phase 3: 文件夹控制
- [ ] 递归深度限制
- [ ] 排除文件夹
- [ ] 性能优化

### Phase 4: 集成
- [ ] Add Folders自动应用过滤器
- [ ] Apply to existing files功能
- [ ] 状态栏显示过滤统计

## Dependencies

### 系统单元
- `Masks` - 通配符匹配
- `RegExpr` - 正则表达式
- `SysUtils` - 文件操作
- `fpjson` - 配置持久化

### UI组件
- `TDateTimePicker` - 日期选择
- `TSpinEdit` - 数值输入
- `TComboBox` - 单位选择

## References

- Windows File Attributes (faReadOnly/faHidden/faSysFile)
- Pascal Masks Unit (MatchesMask)
- RegExpr Library (TRegExpr)

---

**Version**: 1.0  
**Created**: 2026-02-12  
**Last Updated**: 2026-02-12  
**Related**: Feature 002 (Main Window UI)
