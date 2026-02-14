# Feature 003: Virtual File List

## 概述

**Feature Name**: 虚拟文件列表  
**Priority**: P0（关键 - 核心数据展示组件）  
**状态**: ✅ 已实现（WPF，ListView 替代 VST）  
**Dependencies**: 无（可独立开发测试）

WPF 对齐说明：当前使用 `ListView` 实现文件列表（列显示/隐藏、排序、拖拽、双击自适应、快捷键等），不使用 Lazarus 的 `TLazVirtualStringTree`。相关实现见 `ReNamerWPF/ReNamer/Views/MainWindow.xaml(.cs)` 与 `ReNamerWPF/ReNamer/Services/AppSettings.cs`。

## 问题陈述

ReNamer_RE 需要一个高性能的文件列表组件,能够:
1. **高性能** - 处理百万级文件无卡顿(虚拟模式)
2. **21列数据** - 显示完整文件信息和重命名结果
3. **交互编辑** - 行内编辑 New Name,拖放排序,复选框标记
4. **排序过滤** - 多列排序,动态列显示/隐藏
5. **状态可视化** - 图标+颜色区分重命名状态

核心技术: `TLazVirtualStringTree` (高性能虚拟列表控件)

## 需求（基于 DFM 分析）

以下为 Lazarus/VST 方案的历史参考；WPF 已以 ListView 对齐实现。

### 控件基本属性 (对照 docs/rules_requirements.md 第710-761行)

```pascal
VSTFiles: TLazVirtualStringTree
  Parent = PanelFiles
  Align = alClient
  Images = ImageListSmall  // 16×16 状态图标
  PopupMenu = PM_Files     // 右键菜单
  
  // 拖放支持
  DragMode = dmAutomatic
  DragOperations = [doMove]
  DragType = dtVCL
  
  // 列标题
  Header.Height = 24
  Header.PopupMenu = PM_FilesColumns  // 右键列标题弹出列管理菜单
  Header.Options = [hoColumnResize, hoDblClickResize, hoDrag, 
                    hoHotTrack, hoShowSortGlyphs, hoVisible]
```

**Header.Options 详解**:
- `hoColumnResize` - 拖拽列边框调整宽度
- `hoDblClickResize` - 双击列边框自动调整为最佳宽度
- `hoDrag` - 拖拽列标题重新排列顺序
- `hoHotTrack` - 鼠标悬停高亮列标题
- `hoShowSortGlyphs` - 显示排序箭头(升序/降序)
- `hoVisible` - 显示列标题行

### TreeOptions 详解 (对照第728-761行)

#### AutoOptions
```pascal
TreeOptions.AutoOptions = [
  toAutoDropExpand,      // 拖放时自动展开节点(无树形结构不触发)
  toAutoScroll,          // 拖放到边缘时自动滚动
  toAutoScrollOnExpand,  // 展开时滚动确保子节点可见
  toAutoTristateTracking,// 复选框三态跟踪
  toAutoDeleteMovedNodes,// 拖动后自动删除原位置节点
  toAutoChangeScale      // DPI变化时自动缩放
]
```

#### MiscOptions
```pascal
TreeOptions.MiscOptions = [
  toCheckSupport,        // 启用复选框(标记文件是否参与重命名)
  toFullRepaintOnResize, // 窗口调整时完全重绘
  toInitOnSave,          // 保存时初始化未初始化节点
  toToggleOnDblClick,    // 双击切换复选框状态
  toWheelPanning,        // 鼠标滚轮平滑滚动
  toFullRowDrag,         // 整行任意位置可拖拽
  toEditOnClick          // 单击已选中单元格进入编辑
]
```

#### PaintOptions
```pascal
TreeOptions.PaintOptions = [
  toHideFocusRect,       // 隐藏焦点虚线框
  toShowButtons,         // 显示展开/折叠按钮(无树形不可见)
  toShowDropmark,        // 拖放时显示插入位置指示线
  toShowHorzGridLines,   // 显示水平网格线
  toShowVertGridLines,   // 显示垂直网格线
  toThemeAware,          // 使用系统主题
  toUseBlendedImages     // 图标半透明混合(禁用时变淡)
]
```

#### SelectionOptions
```pascal
TreeOptions.SelectionOptions = [
  toExtendedFocus,       // 焦点可在列间移动(左右箭头)
  toFullRowSelect,       // 点击任意列选中整行
  toMultiSelect,         // Ctrl多选/Shift范围选
  toSimpleDrawSelection  // 简单矩形绘制选中高亮
]
```

## Data Model

### 节点数据结构

```pascal
type
  /// 文件重命名状态
  TRenameState = (
    rsReady,       // 准备就绪(已添加,未预览)
    rsPreviewOK,   // 预览成功(无冲突)
    rsPreviewError,// 预览错误(非法字符等)
    rsConflict,    // 名称冲突(目标已存在)
    rsRenamed,     // 已重命名
    rsRenameError, // 重命名失败(权限不足等)
    rsUndone       // 已撤销
  );

  /// 文件数据(每个节点关联一个)
  PFileData = ^TFileData;
  TFileData = record
    // 基本信息
    FullPath: string;           // 完整路径(C:\Folder\file.txt)
    OriginalName: string;       // 原始文件名(file.txt)
    Extension: string;          // 扩展名(.txt)
    
    // 预览结果
    NewName: string;            // 规则计算的新文件名
    ManualNewName: string;      // 手动编辑的新文件名(优先级高)
    NewFullPath: string;        // 新完整路径
    
    // 文件属性
    Size: Int64;                // 文件大小(字节)
    CreatedTime: TDateTime;     // 创建时间
    ModifiedTime: TDateTime;    // 修改时间
    ExifDate: TDateTime;        // EXIF日期(图片)
    
    // 状态
    State: TRenameState;        // 重命名状态
    IsMarked: Boolean;          // 是否标记(复选框)
    ErrorMessage: string;       // 错误信息
    
    // 撤销支持
    OldPath: string;            // 撤销时的旧路径
    
    // 计算字段(按需计算,缓存)
    NameDigits: string;         // 文件名中的数字
    PathDigits: string;         // 路径中的数字
  end;
```

### 文件管理类

```pascal
type
  /// 文件列表管理器
  TFileListManager = class
  private
    FFiles: TList<PFileData>;   // 文件数据列表
    FVST: TLazVirtualStringTree; // 关联的VST控件
    
    function GetFileCount: Integer;
    function GetMarkedCount: Integer;
    function GetSelectedCount: Integer;
  public
    constructor Create(AVST: TLazVirtualStringTree);
    destructor Destroy; override;
    
    // 文件添加/移除
    procedure AddFile(const AFilePath: string);
    procedure AddFolder(const AFolderPath: string; ARecursive: Boolean);
    procedure RemoveFile(ANode: PVirtualNode);
    procedure ClearAll;
    
    // 数据访问
    function GetFileData(ANode: PVirtualNode): PFileData;
    procedure UpdatePreview(ARuleEngine: IRuleEngine);
    
    // 统计
    property FileCount: Integer read GetFileCount;
    property MarkedCount: Integer read GetMarkedCount;
    property SelectedCount: Integer read GetSelectedCount;
  end;
```

## Column Definitions (21 Columns)

### 默认可见列 (对照第764-768行)

| Index | Name | Caption | Width | Alignment | Options |
|-------|------|---------|-------|-----------|---------|
| 0 | State | State | 150 | taLeftJustify | [coVisible, coAllowFocus, coEnabled, coParentBidiMode, coParentColor, coResizable, coShowDropMark, coSmartResize, coAllowClick, coDraggable, coEditable] |
| 3 | Name | Name | 150 | taLeftJustify | [同上] |
| 4 | NewName | New Name | 150 | taLeftJustify | [同上] |
| 20 | Error | Error | 100 | taLeftJustify | [同上] |

### 默认隐藏列 (对照第770-787行)

| Index | Name | Caption | Alignment | Description |
|-------|------|---------|-----------|-------------|
| 1 | Path | Path | taLeftJustify | 完整路径 |
| 2 | Folder | Folder | taLeftJustify | 所在文件夹 |
| 5 | NewPath | New Path | taLeftJustify | 新完整路径 |
| 6 | Size | Size | **taRightJustify** | 文件大小(字节) |
| 7 | SizeKB | Size KB | **taRightJustify** | KB大小 |
| 8 | SizeMB | Size MB | **taRightJustify** | MB大小 |
| 9 | Created | Created | taLeftJustify | 创建时间 |
| 10 | Modified | Modified | taLeftJustify | 修改时间 |
| 11 | Extension | Extension | taLeftJustify | 扩展名 |
| 12 | NameDigits | Name Digits | **taRightJustify** | 文件名中的数字 |
| 13 | PathDigits | Path Digits | taLeftJustify | 路径中的数字 |
| 14 | NameLength | Name Length | **taRightJustify** | 文件名长度 |
| 15 | NewNameLength | New Name Length | **taRightJustify** | 新文件名长度 |
| 16 | PathLength | Path Length | **taRightJustify** | 路径长度 |
| 17 | NewPathLength | New Path Length | **taRightJustify** | 新路径长度 |
| 18 | ExifDate | Exif Date | taLeftJustify | EXIF日期 |
| 19 | OldPath | Old Path | taLeftJustify | 旧路径(撤销用) |

**列创建示例**:
```pascal
procedure TFormMain.InitializeVSTColumns;
begin
  with VSTFiles.Header.Columns.Add do
  begin
    Position := 0;
    Width := 150;
    Text := 'State';
    Alignment := taLeftJustify;
    Options := [coVisible, coAllowFocus, coEnabled, coParentBidiMode, 
                coParentColor, coResizable, coShowDropMark, coSmartResize, 
                coAllowClick, coDraggable, coEditable];
  end;
  
  // ... 添加其他20列
  
  // 隐藏列默认不包含 coVisible
  with VSTFiles.Header.Columns.Add do
  begin
    Position := 1;
    Text := 'Path';
    Alignment := taLeftJustify;
    Options := [coAllowFocus, coEnabled, coResizable, coAllowClick];
    // 注意: 无 coVisible
  end;
end;
```

## Virtual Mode Implementation (虚拟模式)

### 核心事件流 (对照第791-806行)

#### 1. OnGetNodeDataSize
```pascal
procedure TFormMain.VSTFilesGetNodeDataSize(Sender: TBaseVirtualTree; 
  var NodeDataSize: Integer);
begin
  // 告诉VST每个节点需要多少字节存储数据指针
  NodeDataSize := SizeOf(PFileData); // 指针大小(4或8字节)
end;
```

#### 2. OnGetText
```pascal
procedure TFormMain.VSTFilesGetText(Sender: TBaseVirtualTree; 
  Node: PVirtualNode; Column: TColumnIndex; TextType: TVSTTextType; 
  var CellText: string);
var
  Data: PFileData;
begin
  Data := Sender.GetNodeData(Node);
  if Data = nil then Exit;
  
  case Column of
    0: CellText := RenameStateToString(Data^.State);
    3: CellText := Data^.OriginalName;
    4: begin
         if Data^.ManualNewName <> '' then
           CellText := Data^.ManualNewName  // 优先手动编辑
         else
           CellText := Data^.NewName;       // 规则计算结果
       end;
    6: CellText := FormatBytes(Data^.Size);       // "1,234,567 bytes"
    7: CellText := Format('%.2f', [Data^.Size / 1024]);        // KB
    8: CellText := Format('%.2f', [Data^.Size / (1024*1024)]); // MB
    9: CellText := FormatDateTime('yyyy-mm-dd hh:nn:ss', Data^.CreatedTime);
    10: CellText := FormatDateTime('yyyy-mm-dd hh:nn:ss', Data^.ModifiedTime);
    11: CellText := Data^.Extension;
    12: CellText := Data^.NameDigits;
    14: CellText := IntToStr(Length(Data^.OriginalName));
    15: if Data^.ManualNewName <> '' then
          CellText := IntToStr(Length(Data^.ManualNewName))
        else
          CellText := IntToStr(Length(Data^.NewName));
    20: CellText := Data^.ErrorMessage;
    // ... 其他列
  end;
end;
```

#### 3. OnGetImageIndex
```pascal
procedure TFormMain.VSTFilesGetImageIndex(Sender: TBaseVirtualTree; 
  Node: PVirtualNode; Kind: TVTImageKind; Column: TColumnIndex; 
  var Ghosted: Boolean; var ImageIndex: Integer);
var
  Data: PFileData;
begin
  if Column <> 0 then Exit; // 仅State列显示图标
  
  Data := Sender.GetNodeData(Node);
  if Data = nil then Exit;
  
  case Data^.State of
    rsReady:        ImageIndex := 0;  // 空白/准备图标
    rsPreviewOK:    ImageIndex := 1;  // 绿色勾图标
    rsPreviewError: ImageIndex := 2;  // 红色叉图标
    rsConflict:     ImageIndex := 3;  // 橙色警告图标
    rsRenamed:      ImageIndex := 4;  // 蓝色成功图标
    rsRenameError:  ImageIndex := 5;  // 红色错误图标
    rsUndone:       ImageIndex := 6;  // 灰色撤销图标
  end;
end;
```

#### 4. OnPaintText (对照第802-805行)
```pascal
procedure TFormMain.VSTFilesPaintText(Sender: TBaseVirtualTree; 
  const TargetCanvas: TCanvas; Node: PVirtualNode; Column: TColumnIndex; 
  TextType: TVSTTextType);
var
  Data: PFileData;
begin
  Data := Sender.GetNodeData(Node);
  if Data = nil then Exit;
  
  // 根据状态设置文字颜色
  case Data^.State of
    rsPreviewOK:    TargetCanvas.Font.Color := clGreen;
    rsPreviewError,
    rsRenameError:  TargetCanvas.Font.Color := clRed;
    rsConflict:     TargetCanvas.Font.Color := clOrange;
    rsRenamed:      TargetCanvas.Font.Color := clBlue;
    rsUndone:       TargetCanvas.Font.Color := clGray;
  else
    TargetCanvas.Font.Color := clWindowText;
  end;
  
  // New Name 列特殊处理
  if Column = 4 then
  begin
    if Data^.NewName = Data^.OriginalName then
      TargetCanvas.Font.Color := clGray; // 未改变时显示灰色
  end;
  
  // 未标记的文件用淡色显示
  if not Data^.IsMarked then
    TargetCanvas.Font.Color := clGrayText;
end;
```

#### 5. OnFreeNode
```pascal
procedure TFormMain.VSTFilesFreeNode(Sender: TBaseVirtualTree; 
  Node: PVirtualNode);
var
  Data: PFileData;
begin
  Data := Sender.GetNodeData(Node);
  if Data <> nil then
  begin
    // 释放节点数据内存
    Dispose(Data);
    Data := nil;
  end;
end;
```

## Sorting Implementation (对照第808-818行)

### OnHeaderClick - 触发排序
```pascal
procedure TFormMain.VSTFilesHeaderClick(Sender: TVTHeader; 
  HitInfo: TVTHeaderHitInfo);
begin
  with Sender do
  begin
    if HitInfo.Column = SortColumn then
    begin
      // 点击当前排序列,切换方向
      if SortDirection = sdAscending then
        SortDirection := sdDescending
      else
        SortDirection := sdAscending;
    end
    else
    begin
      // 新排序列,默认升序
      SortColumn := HitInfo.Column;
      SortDirection := sdAscending;
    end;
    
    // 触发排序
    VSTFiles.SortTree(SortColumn, SortDirection);
  end;
end;
```

### OnCompareNodes - 比较逻辑
```pascal
procedure TFormMain.VSTFilesCompareNodes(Sender: TBaseVirtualTree; 
  Node1, Node2: PVirtualNode; Column: TColumnIndex; var Result: Integer);
var
  Data1, Data2: PFileData;
begin
  Data1 := Sender.GetNodeData(Node1);
  Data2 := Sender.GetNodeData(Node2);
  
  if (Data1 = nil) or (Data2 = nil) then
  begin
    Result := 0;
    Exit;
  end;
  
  case Column of
    0: Result := Ord(Data1^.State) - Ord(Data2^.State);
    3: Result := CompareText(Data1^.OriginalName, Data2^.OriginalName);
    4: Result := CompareText(Data1^.NewName, Data2^.NewName);
    6, 7, 8: Result := CompareValue(Data1^.Size, Data2^.Size); // 数值比较
    9: Result := CompareDateTime(Data1^.CreatedTime, Data2^.CreatedTime);
    10: Result := CompareDateTime(Data1^.ModifiedTime, Data2^.ModifiedTime);
    14: Result := Length(Data1^.OriginalName) - Length(Data2^.OriginalName);
    15: Result := Length(Data1^.NewName) - Length(Data2^.NewName);
    // ... 其他列
  else
    Result := 0;
  end;
end;
```

### Cancel Sorting
```pascal
procedure TFormMain.MenuItemCancelSortingClick(Sender: TObject);
begin
  VSTFiles.Header.SortColumn := NoColumn;
  VSTFiles.Header.SortDirection := sdAscending;
  // 恢复原始添加顺序(需要保存原始索引)
  RestoreOriginalOrder;
end;
```

## In-line Editing (对照第820-827行)

### OnEditing - 允许编辑检查
```pascal
procedure TFormMain.VSTFilesEditing(Sender: TBaseVirtualTree; 
  Node: PVirtualNode; Column: TColumnIndex; var Allowed: Boolean);
begin
  // 仅允许编辑 "New Name" 列
  Allowed := (Column = 4);
end;
```

### OnCreateEditor - 创建编辑器
```pascal
procedure TFormMain.VSTFilesCreateEditor(Sender: TBaseVirtualTree; 
  Node: PVirtualNode; Column: TColumnIndex; out EditLink: IVTEditLink);
begin
  // 使用默认的 TStringEditLink
  EditLink := TStringEditLink.Create;
end;
```

### OnNewText - 保存编辑
```pascal
procedure TFormMain.VSTFilesNewText(Sender: TBaseVirtualTree; 
  Node: PVirtualNode; Column: TColumnIndex; NewText: string);
var
  Data: PFileData;
begin
  if Column <> 4 then Exit;
  
  Data := Sender.GetNodeData(Node);
  if Data = nil then Exit;
  
  // 保存手动编辑的新名(优先级高于规则计算)
  Data^.ManualNewName := Trim(NewText);
  
  // 验证新名称
  if not IsValidFileName(Data^.ManualNewName) then
  begin
    Data^.State := rsPreviewError;
    Data^.ErrorMessage := 'Invalid file name';
  end
  else
  begin
    Data^.State := rsPreviewOK;
    Data^.ErrorMessage := '';
    Data^.NewFullPath := ExtractFilePath(Data^.FullPath) + Data^.ManualNewName;
  end;
  
  Sender.InvalidateNode(Node); // 重绘节点
end;
```

### OnEditCancelled - 取消编辑
```pascal
procedure TFormMain.VSTFilesEditCancelled(Sender: TBaseVirtualTree; 
  Column: TColumnIndex);
begin
  // 不需要特殊处理,编辑器已丢弃内容
end;
```

## Drag & Drop (对照第829-837行)

### OnDragAllowed - 允许拖拽
```pascal
procedure TFormMain.VSTFilesDragAllowed(Sender: TBaseVirtualTree; 
  Node: PVirtualNode; Column: TColumnIndex; var Allowed: Boolean);
begin
  Allowed := True; // 允许所有节点拖拽
end;
```

### OnDragOver - 拖拽悬停
```pascal
procedure TFormMain.VSTFilesDragOver(Sender: TBaseVirtualTree; 
  Source: TObject; Shift: TShiftState; State: TDragState; 
  Pt: TPoint; Mode: TDropMode; var Effect: Integer; var Accept: Boolean);
begin
  Accept := (Source = Sender); // 仅接受自身拖拽
end;
```

### OnDragDrop - 完成拖放
```pascal
procedure TFormMain.VSTFilesDragDrop(Sender: TBaseVirtualTree; 
  Source: TObject; DataObject: IDataObject; Formats: TFormatArray; 
  Shift: TShiftState; Pt: TPoint; var Effect: Integer; Mode: TDropMode);
var
  AttachMode: TVTNodeAttachMode;
  TargetNode: PVirtualNode;
begin
  if Source <> Sender then Exit;
  
  // 获取放置目标
  TargetNode := Sender.GetNodeAt(Pt.X, Pt.Y);
  if TargetNode = nil then Exit;
  
  // 根据 Mode 确定插入位置
  case Mode of
    dmAbove: AttachMode := amInsertBefore;
    dmBelow: AttachMode := amInsertAfter;
  else
    Exit;
  end;
  
  // 移动选中节点到目标位置
  Sender.MoveTo(Sender.FocusedNode, TargetNode, AttachMode, False);
end;
```

## Checkbox Support (复选框)

### OnChecked - 勾选状态变化
```pascal
procedure TFormMain.VSTFilesChecked(Sender: TBaseVirtualTree; 
  Node: PVirtualNode);
var
  Data: PFileData;
begin
  Data := Sender.GetNodeData(Node);
  if Data = nil then Exit;
  
  // 同步到数据模型
  Data^.IsMarked := (Node^.CheckState = csCheckedNormal);
  
  // 更新状态栏统计
  UpdateStatusBar;
end;
```

### 批量勾选/取消
```pascal
procedure TFormMain.MenuItemMarkAllClick(Sender: TObject);
var
  Node: PVirtualNode;
begin
  VSTFiles.BeginUpdate;
  try
    Node := VSTFiles.GetFirst;
    while Node <> nil do
    begin
      Node^.CheckState := csCheckedNormal;
      Node := VSTFiles.GetNext(Node);
    end;
  finally
    VSTFiles.EndUpdate;
  end;
  UpdateStatusBar;
end;

procedure TFormMain.MenuItemUnmarkAllClick(Sender: TObject);
var
  Node: PVirtualNode;
begin
  VSTFiles.BeginUpdate;
  try
    Node := VSTFiles.GetFirst;
    while Node <> nil do
    begin
      Node^.CheckState := csUncheckedNormal;
      Node := VSTFiles.GetNext(Node);
    end;
  finally
    VSTFiles.EndUpdate;
  end;
  UpdateStatusBar;
end;
```

## Selection Management (对照第839-841行)

### OnChange - 选中变化
```pascal
procedure TFormMain.VSTFilesChange(Sender: TBaseVirtualTree; 
  Node: PVirtualNode);
begin
  // 更新状态栏 "Selected" 计数
  UpdateStatusBar;
  
  // 更新菜单/工具栏启用状态
  UpdateActions;
end;
```

### OnDblClick - 双击处理
```pascal
procedure TFormMain.VSTFilesDblClick(Sender: TObject);
var
  HitInfo: THitInfo;
begin
  VSTFiles.GetHitTestInfoAt(Mouse.CursorPos.X, Mouse.CursorPos.Y, True, HitInfo);
  
  // 双击勾选框:切换状态(由 toToggleOnDblClick 自动处理)
  // 双击其他区域:打开文件所在文件夹
  if HitInfo.HitColumn <> 0 then
    OpenFileLocation(VSTFiles.FocusedNode);
end;
```

## Keyboard Handling (对照第843-851行)

```pascal
procedure TFormMain.VSTFilesKeyDown(Sender: TObject; var Key: Word; 
  Shift: TShiftState);
begin
  case Key of
    VK_DELETE:
      if ssCtrl in Shift then
        ClearAllFiles        // Ctrl+Del: 清空所有
      else
        RemoveSelectedFiles; // Del: 删除选中
        
    VK_A:
      if ssCtrl in Shift then
        VSTFiles.SelectAll(False); // Ctrl+A: 全选
        
    VK_F2:
      VSTFiles.EditNode(VSTFiles.FocusedNode, 4); // F2: 编辑 New Name
      
    VK_UP:
      if ssCtrl in Shift then
        MoveSelectedFilesUp; // Ctrl+Up: 上移
        
    VK_DOWN:
      if ssCtrl in Shift then
        MoveSelectedFilesDown; // Ctrl+Down: 下移
        
    VK_RETURN:
      if ssShift in Shift then
        OpenFileInEditor     // Shift+Enter: 用编辑器打开
      else if ssCtrl in Shift then
        OpenFileProperties   // Ctrl+Enter: 属性
      else if ssAlt in Shift then
        OpenFileWith         // Alt+Enter: 打开方式
      else
        OpenFile;            // Enter: 默认打开
        
    VK_SPACE:
      ToggleCheckState(VSTFiles.FocusedNode); // 空格: 切换勾选
  end;
end;
```

## Column Management (列管理)

### PopupMenu: PM_FilesColumns
```pascal
PM_FilesColumns: TPopupMenu
  ├─ MI_ShowAllColumns
  ├─ MI_HideAllColumns
  ├─ ────────
  ├─ MI_Column_State (Checked)
  ├─ MI_Column_Path
  ├─ MI_Column_Folder
  ├─ MI_Column_Name (Checked)
  ├─ MI_Column_NewName (Checked)
  ├─ ... (21个列显示/隐藏菜单项)
  ├─ ────────
  ├─ MI_AutoSizeColumns
  └─ MI_CancelSorting
```

**动态生成列菜单**:
```pascal
procedure TFormMain.BuildColumnsMenu;
var
  I: Integer;
  MenuItem: TMenuItem;
begin
  PM_FilesColumns.Items.Clear;
  
  // Show All / Hide All
  PM_FilesColumns.Items.Add(CreateMenuItem('Show All Columns', MenuItemShowAllColumnsClick));
  PM_FilesColumns.Items.Add(CreateMenuItem('Hide All Columns', MenuItemHideAllColumnsClick));
  PM_FilesColumns.Items.Add(CreateSeparator);
  
  // 为每列创建菜单项
  for I := 0 to VSTFiles.Header.Columns.Count - 1 do
  begin
    MenuItem := TMenuItem.Create(PM_FilesColumns);
    MenuItem.Caption := VSTFiles.Header.Columns[I].Text;
    MenuItem.Tag := I; // 保存列索引
    MenuItem.Checked := coVisible in VSTFiles.Header.Columns[I].Options;
    MenuItem.OnClick := MenuItemToggleColumnClick;
    PM_FilesColumns.Items.Add(MenuItem);
  end;
  
  PM_FilesColumns.Items.Add(CreateSeparator);
  PM_FilesColumns.Items.Add(CreateMenuItem('Auto-size Columns', MenuItemAutoSizeColumnsClick));
  PM_FilesColumns.Items.Add(CreateMenuItem('Cancel Sorting', MenuItemCancelSortingClick));
end;

procedure TFormMain.MenuItemToggleColumnClick(Sender: TObject);
var
  ColumnIndex: Integer;
begin
  ColumnIndex := (Sender as TMenuItem).Tag;
  
  with VSTFiles.Header.Columns[ColumnIndex] do
  begin
    if coVisible in Options then
      Options := Options - [coVisible]
    else
      Options := Options + [coVisible];
  end;
  
  (Sender as TMenuItem).Checked := not (Sender as TMenuItem).Checked;
end;
```

## Performance Optimization

### BeginUpdate / EndUpdate
```pascal
procedure TFormMain.AddFiles(const AFiles: TArray<string>);
var
  I: Integer;
begin
  VSTFiles.BeginUpdate; // 暂停重绘
  try
    for I := 0 to High(AFiles) do
      AddFileSingle(AFiles[I]);
  finally
    VSTFiles.EndUpdate; // 恢复重绘,一次性刷新
  end;
  
  UpdateStatusBar;
end;
```

### OnUpdating Event
```pascal
procedure TFormMain.VSTFilesUpdating(Sender: TBaseVirtualTree; 
  State: TVTUpdateState);
begin
  case State of
    usBegin:
      begin
        // 禁用Action自动更新
        ActionListMain.State := asSuspended;
        // 显示进度条
        StatusBarPanel0.Style := psOwnerDraw;
      end;
    usEnd:
      begin
        // 恢复Action自动更新
        ActionListMain.State := asNormal;
        // 隐藏进度条
        StatusBarPanel0.Style := psText;
      end;
  end;
end;
```

### OnColumnWidthDblClickResize (对照第855行)
```pascal
procedure TFormMain.VSTFilesColumnWidthDblClickResize(Sender: TVTHeader; 
  HitInfo: TVTHeaderHitInfo);
var
  MaxWidth: Integer;
begin
  // 双击列边框,自动调整为最佳宽度
  MaxWidth := VSTFiles.Header.Columns[HitInfo.Column].ComputeHeaderWidth;
  
  // 遍历所有节点计算文本最大宽度
  MaxWidth := Max(MaxWidth, ComputeColumnContentWidth(HitInfo.Column));
  
  VSTFiles.Header.Columns[HitInfo.Column].Width := MaxWidth + 10; // 加padding
end;

function TFormMain.ComputeColumnContentWidth(AColumn: TColumnIndex): Integer;
var
  Node: PVirtualNode;
  CellText: string;
  TextWidth: Integer;
begin
  Result := 0;
  
  Node := VSTFiles.GetFirst;
  while Node <> nil do
  begin
    VSTFilesGetText(VSTFiles, Node, AColumn, ttNormal, CellText);
    TextWidth := VSTFiles.Canvas.TextWidth(CellText);
    if TextWidth > Result then
      Result := TextWidth;
    
    Node := VSTFiles.GetNext(Node);
  end;
end;
```

## PopupMenu: PM_Files

```pascal
PM_Files: TPopupMenu
  ├─ MenuItem_Remove (Del)
  ├─ MenuItem_Clear (Ctrl+Del)
  ├─ ────────
  ├─ MenuItem_MarkAll
  ├─ MenuItem_UnmarkAll
  ├─ MenuItem_InvertMarks
  ├─ ────────
  ├─ MenuItem_OpenFile (Enter)
  ├─ MenuItem_OpenFolder (Shift+Enter)
  ├─ MenuItem_OpenWith (Alt+Enter)
  ├─ MenuItem_Properties (Ctrl+Enter)
  ├─ ────────
  ├─ MenuItem_CopyName
  ├─ MenuItem_CopyPath
  └─ MenuItem_CopyNewName
```

## Testing Requirements

### 单元测试
```pascal
type
  TVSTFilesTest = class(TTestCase)
  published
    procedure TestVirtualMode;
    procedure TestColumnSorting;
    procedure TestInlineEditing;
    procedure TestDragDrop;
    procedure TestCheckboxes;
    procedure TestPerformance_1M_Files;
  end;

procedure TVSTFilesTest.TestVirtualMode;
var
  VST: TLazVirtualStringTree;
  Node: PVirtualNode;
  Data: PFileData;
begin
  VST := TLazVirtualStringTree.Create(nil);
  try
    VST.NodeDataSize := SizeOf(PFileData);
    
    // 添加100个节点
    VST.BeginUpdate;
    try
      VST.RootNodeCount := 100;
    finally
      VST.EndUpdate;
    end;
    
    AssertEquals(100, VST.TotalCount);
    
    // 验证数据获取
    Node := VST.GetFirst;
    Data := VST.GetNodeData(Node);
    AssertNotNull(Data);
  finally
    VST.Free;
  end;
end;

procedure TVSTFilesTest.TestPerformance_1M_Files;
var
  VST: TLazVirtualStringTree;
  StartTime: TDateTime;
  ElapsedMs: Integer;
begin
  VST := TLazVirtualStringTree.Create(nil);
  try
    VST.NodeDataSize := SizeOf(PFileData);
    
    StartTime := Now;
    VST.BeginUpdate;
    try
      VST.RootNodeCount := 1000000; // 100万节点
    finally
      VST.EndUpdate;
    end;
    ElapsedMs := MilliSecondsBetween(Now, StartTime);
    
    AssertTrue(ElapsedMs < 1000, 'Adding 1M nodes should take <1s');
    AssertEquals(1000000, VST.TotalCount);
  finally
    VST.Free;
  end;
end;
```

### UI自动化测试
- 添加1000个文件,滚动到底部(<100ms)
- 点击列标题排序,验证顺序正确
- 双击列边框,验证自动调整宽度
- 拖拽列标题,验证列顺序改变
- F2编辑New Name,验证保存成功
- 空格切换勾选,验证状态同步
- 拖放文件排序,验证顺序变化

## 验收标准

WPF 现状：核心列表交互与列管理已实现（ListView 方案）；以下清单用于历史对照。

### Phase 1: 虚拟模式基础
- [ ] VST控件创建,21列定义正确
- [ ] OnGetNodeDataSize返回正确大小
- [ ] OnGetText返回所有列的正确文本
- [ ] OnGetImageIndex返回正确状态图标
- [ ] OnFreeNode正确释放内存

### Phase 2: 交互功能
- [ ] 点击列标题触发排序,显示箭头图标
- [ ] F2编辑New Name列,其他列拒绝编辑
- [ ] 拖放文件重新排序
- [ ] 空格/双击切换复选框状态
- [ ] Del删除选中文件,Ctrl+Del清空

### Phase 3: 列管理
- [ ] 右键列标题显示列管理菜单
- [ ] 显示/隐藏列工作正确
- [ ] 拖拽列标题重新排列顺序
- [ ] 双击列边框自动调整最佳宽度
- [ ] 列宽调整保存到配置

### Phase 4: 性能验证
- [ ] 添加10K文件<100ms
- [ ] 添加100K文件<1s
- [ ] 添加1M文件<3s
- [ ] 滚动10万行无卡顿
- [ ] 排序10万行<500ms

## Dependencies

### Lazarus 组件
- `TLazVirtualStringTree` - LazVirtualStringTree包
- `TImageList` - 状态图标
- `TPopupMenu` - 右键菜单

### 系统单元
- `Classes`, `SysUtils` - 基础类
- `Graphics` - 绘图
- `Math` - 数学函数

## Next Steps

1. **安装LazVirtualStringTree包**
   - 下载: https://github.com/blikblum/VirtualTreeView-Lazarus
   - 编译安装到Lazarus IDE

2. **创建测试项目**
   - 创建空白窗体
   - 放置VST控件
   - 测试虚拟模式基本功能

3. **实现TFileListManager**
   - 文件添加/移除逻辑
   - 数据模型管理
   - 与VST事件绑定

4. **实现所有VST事件**
   - OnGetText, OnGetImageIndex
   - OnCompareNodes, OnHeaderClick
   - OnEditing, OnNewText
   - OnDragDrop, OnChecked

5. **性能测试与优化**
   - 百万级文件压力测试
   - 内存泄漏检测
   - 滚动/排序性能优化

---

**Version**: 1.0  
**Created**: 2026-02-12  
**Last Updated**: 2026-02-12  
**Related**: Feature 002 (Main Window UI), Feature 001 (Rule Engine)
