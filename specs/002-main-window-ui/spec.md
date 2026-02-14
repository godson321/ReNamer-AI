# Feature 002: Main Window UI

## 概述

**Feature Name**: 主窗口用户界面  
**Priority**: P0（关键 - 应用程序主入口）  
**状态**: ✅ 已实现（WPF）  
**Dependencies**: 无（可独立开发，后续集成规则引擎）

WPF 对齐说明：对应 `ReNamerWPF/ReNamer/Views/MainWindow.xaml` 与 `ReNamerWPF/ReNamer/Views/MainWindow.xaml.cs`，已实现菜单/工具栏/规则面板/文件列表/状态栏及快捷键。

## 问题陈述

ReNamer_RE 需要一个专业的主窗口界面,精确复现原版 ReNamer 7.8 的布局、功能和交互模式。主窗口包含:
1. 工具栏 - 主要操作按钮(Add Files/Folders, Preview, Rename)
2. 规则面板 - 规则列表管理
3. 文件面板 - 虚拟文件列表(支持大量文件)
4. 状态栏 - 进度显示和统计信息
5. 菜单栏 - 完整功能访问

## 需求（基于 DFM 分析）

以下为原始 DFM/移植参考；WPF 已按实际实现对齐。

### 窗口属性 (对照 docs/rules_requirements.md 第595-615行)

```pascal
type
  TFormMain = class(TForm)
  private
    { Private declarations }
  public
    { Public declarations }
  published
    // 窗口基本属性
    // Width=600, Height=470 (ClientWidth=600, ClientHeight=470)
    // Position=poScreenCenter
    // Caption='ReNamer'
    // KeyPreview=True (全局快捷键)
    // AllowDropFiles=True (拖放支持)
    
    // 主要组件
    ToolBarMain: TToolBar;
    PanelMain: TPanel;
    PanelRules: TPanel;
    SplitterMiddle: TSplitter;
    PanelFiles: TPanel;
    StatusBar: TStatusBar;
    MainMenu: TMainMenu;
  end;
```

**设计参数**:
- 初始尺寸: 600×470 px
- 位置: 屏幕居中 (`poScreenCenter`)
- 最小尺寸: 400×300 px (防止过小不可用)
- 支持自由调整大小,窗口位置/大小需持久化到配置文件

### 布局结构 (从上到下)

```
┌─────────────────────────────────────────────────────────┐
│ MainMenu (菜单栏)                                        │
├─────────────────────────────────────────────────────────┤
│ ToolBarMain (高度38px, Align=alTop)                     │
│ [Add Files] → [Add Folders] → → [Preview] → → [Rename] │
├─────────────────────────────────────────────────────────┤
│ PanelMain (Align=alClient, Top=40, BorderSpacing.Top=2) │
│ ┌─────────────────────────────────────────────────────┐ │
│ │ PanelRules (Align=alTop, Height=145)                │ │
│ │ ToolBarRules: [Add] [Remove] [Up] [Down]            │ │
│ │ ListViewRules (3列: #, Rule, Statement)             │ │
│ │   ☑ 1  Replace    "old" → "new"                     │ │
│ │   ☐ 2  Serialize  Index 001-999                     │ │
│ └─────────────────────────────────────────────────────┘ │
│ ─────────────────────────────────────────────────────── │ ← SplitterMiddle (可拖动)
│ ┌─────────────────────────────────────────────────────┐ │
│ │ PanelFiles (Align=alClient)                         │ │
│ │ ToolBarFiles: [Files▾] [Columns▾] [Options▾] ...   │ │
│ │ VSTFiles (虚拟字符串树, 21列)                        │ │
│ │ State  Name        New Name     Error               │ │
│ │   ✓    file1.txt   file1_new.txt                    │ │
│ │   →    file2.txt   file2_new.txt                    │ │
│ └─────────────────────────────────────────────────────┘ │
├─────────────────────────────────────────────────────────┤
│ StatusBar (高度23px)                                     │
│ [进度条:0%] | Files:10 Marked:8 Selected:2 |          │
└─────────────────────────────────────────────────────────┘
```

## Component Specifications

### 1. 主工具栏 (ToolBarMain)

**属性**:
```pascal
ToolBarMain: TToolBar
  Align = alTop
  AutoSize = True
  ButtonHeight = 36
  ButtonWidth = 23
  Images = ImageListBig (32×32图标)
  ShowCaptions = True
  List = True  // 图标在左,文字在右
  EdgeBorders = [ebBottom]  // 仅底部有边框线
```

**按钮定义** (对照 docs/rules_requirements.md 第633-641行):

| Left | 类型 | Caption | Action | 快捷键 | ImageIndex | 说明 |
|------|------|---------|--------|--------|------------|------|
| 1 | TToolButton | "Add Files" | Action_AddFiles | F3 | 3 | 打开文件选择对话框 |
| 94 | TToolButton | "Add Folders" | Action_AddFolders | F4 | 4 | 打开文件夹选择对话框 |
| 202 | TImage | - | - | - | - | 装饰性箭头图标(14×13) |
| 226 | TToolButton | "Preview" | Action_Preview | F5 | 0 | 应用规则预览 |
| 312 | TImage | - | - | - | - | 装饰性箭头图标 |
| 336 | TToolButton | "Rename" | Action_Rename | F6 | 1 | 执行重命名 |

**关键实现**:
```pascal
procedure TFormMain.CreateMainToolbar;
begin
  ToolBarMain := TToolBar.Create(Self);
  with ToolBarMain do
  begin
    Parent := Self;
    Align := alTop;
    AutoSize := True;
    ButtonHeight := 36;
    ButtonWidth := 23;
    Images := ImageListBig;
    ShowCaptions := True;
    List := True;
    EdgeBorders := [ebBottom];
  end;
  
  // Add Files 按钮
  with TToolButton.Create(ToolBarMain) do
  begin
    Parent := ToolBarMain;
    Left := 1;
    Caption := 'Add Files';
    Action := ActionAddFiles;
    ImageIndex := 3;
  end;
  
  // 装饰箭头
  with TImage.Create(ToolBarMain) do
  begin
    Parent := ToolBarMain;
    Left := 202;
    Width := 24;
    Height := 36;
    Center := True;
    Picture.LoadFromFile('arrow.png'); // 或嵌入资源
  end;
  
  // ... 其他按钮
end;
```

### 2. 规则面板 (PanelRules)

**结构**:
```pascal
PanelRules: TPanel
  Align = alTop
  Height = 145  // 初始高度,用户可拖动Splitter调整
  BevelOuter = bvNone
  
  ├─ ToolBarRules: TToolBar (Dock=alTop)
  │    ShowCaptions = True
  │    ButtonHeight = 20
  │    Images = ImageListToolbarRules (13×13)
  │    
  └─ ListViewRules: TListView (Align=alClient)
       ViewStyle = vsReport
       Checkboxes = True  // 启用复选框
       MultiSelect = True
       ReadOnly = True
       RowSelect = True
```

**ToolBarRules 按钮** (对照第653-657行):

| Caption | ImageIndex | OnClick | 说明 |
|---------|------------|---------|------|
| "Add" | 0 | ToolButton_AddRuleClick | 打开添加规则对话框 |
| "Remove" | 1 | ToolButton_RemoveRuleClick | 删除选中规则 |
| "Up" | 2 | ToolButton_MoveUpRuleClick | 上移规则 |
| "Down" | 3 | ToolButton_MoveDownRuleClick | 下移规则 |

**ListViewRules 列定义** (对照第669-672行):

```pascal
// 列0: 序号
Column0: TListColumn
  Caption = '#'
  MinWidth = 50
  
// 列1: 规则类型
Column1: TListColumn
  Caption = 'Rule'
  MinWidth = 100
  Width = 100
  
// 列2: 规则描述
Column2: TListColumn
  Caption = 'Statement'
  MinWidth = 100
  Width = 300
```

**关键事件**:
```pascal
// 双击编辑规则
procedure TFormMain.ListViewRulesDblClick(Sender: TObject);
begin
  if ListViewRules.Selected <> nil then
    EditRule(ListViewRules.Selected.Index);
end;

// 拖放排序
procedure TFormMain.ListViewRulesDragDrop(Sender, Source: TObject; X, Y: Integer);
var
  DropItem: TListItem;
begin
  DropItem := ListViewRules.GetItemAt(X, Y);
  if (Source = ListViewRules) and (DropItem <> nil) then
    MoveRule(ListViewRules.Selected.Index, DropItem.Index);
end;

// 勾选/取消规则时触发预览更新
procedure TFormMain.ListViewRulesItemChecked(Sender: TObject; Item: TListItem);
begin
  // 更新规则启用状态
  RuleList[Item.Index].IsEnabled := Item.Checked;
  // 自动预览
  if AutoPreviewEnabled then
    Preview;
end;
```

### 3. 分隔条 (SplitterMiddle)

```pascal
SplitterMiddle: TSplitter
  Align = alTop
  Height = 5
  Cursor = crVSplit
  AutoSnap = False
  MinSize = 100  // 规则面板和文件面板最小高度
  ResizeAnchor = akTop
  OnMoved = SplitterMiddleMovedEvent
```

**保存布局**:
```pascal
procedure TFormMain.SplitterMiddleMoved(Sender: TObject);
begin
  // 保存规则面板高度比例到配置
  Config.WriteInteger('Layout', 'RulesPanelHeight', PanelRules.Height);
end;

procedure TFormMain.FormCreate(Sender: TObject);
begin
  // 恢复上次布局
  PanelRules.Height := Config.ReadInteger('Layout', 'RulesPanelHeight', 145);
end;
```

### 4. 文件面板 (PanelFiles)

**结构**:
```pascal
PanelFiles: TPanel
  Align = alClient
  BevelOuter = bvNone
  
  ├─ ToolBarFiles: TToolBar (Dock=alTop)
  │    ShowCaptions = True
  │    ButtonHeight = 18
  │    Images = ImageListToolbarFiles (12×12)
  │    
  └─ VSTFiles: TLazVirtualStringTree (Align=alClient)
       Header.Height = 24
       TreeOptions.MiscOptions = [toCheckSupport, ...]
       TreeOptions.SelectionOptions = [toFullRowSelect, toMultiSelect, ...]
```

**ToolBarFiles 按钮** (对照第702-708行):

| Caption | ImageIndex | OnClick | 弹出菜单 |
|---------|------------|---------|---------|
| "Files" | 6 | ToolButton_FilesClick | PM_Files操作菜单 |
| "Columns" | 6 | ToolButton_ColumnsClick | PM_FilesColumns列管理 |
| "Options" | 2 | ToolButton_OptionsClick | PM_Options选项菜单 |
| "Export" | 5 | ToolButton_ExportClick | PM_Export导出菜单 |
| "Filters" | 3 | ToolButton_FiltersClick | 打开过滤器设置 |
| "Analyze" | 7 | ToolButton_AnalyzeClick | 分析文件名 |

**VSTFiles 完整规范见 Feature 003** (虚拟文件列表单独规范,包含21列定义、虚拟模式、拖放、排序等)

### 5. 状态栏 (StatusBar)

**属性**:
```pascal
StatusBar: TStatusBar
  Height = 23
  SimplePanel = False  // 使用多面板模式
  ShowHint = True
```

**面板定义** (对照第867-883行):

```pascal
// Panel[0]: 进度条(自绘)
StatusBarPanel0: TStatusPanel
  Width = 166
  Style = psOwnerDraw
  // 自绘逻辑在 StatusBarDrawPanel 事件中

// Panel[1]: 文件统计
StatusBarPanel1: TStatusPanel
  Width = 200
  Style = psText
  Text = 'Files: 150 | Marked: 120 | Selected: 5'

// Panel[2]: 辅助信息
StatusBarPanel2: TStatusPanel
  Width = 50
  Style = psText
  Text = ''  // 当前排序列名等
```

**进度条自绘**:
```pascal
procedure TFormMain.StatusBarDrawPanel(StatusBar: TStatusBar; 
  Panel: TStatusPanel; const Rect: TRect);
var
  ProgressRect: TRect;
  ProgressWidth: Integer;
  PercentText: string;
begin
  if Panel.Index <> 0 then Exit;
  
  with StatusBar.Canvas do
  begin
    // 背景
    Brush.Color := clBtnFace;
    FillRect(Rect);
    
    // 进度条填充
    if FProgressPercent > 0 then
    begin
      ProgressWidth := Round((Rect.Right - Rect.Left) * FProgressPercent / 100);
      ProgressRect := Rect;
      ProgressRect.Right := Rect.Left + ProgressWidth;
      Brush.Color := clHighlight;
      FillRect(ProgressRect);
    end;
    
    // 百分比文字(居中)
    if FProgressPercent > 0 then
    begin
      PercentText := Format('Processing: %d%%', [FProgressPercent]);
      Brush.Style := bsClear;
      Font.Color := clWindowText;
      TextRect(Rect, PercentText, [tfCenter, tfVerticalCenter, tfSingleLine]);
    end;
  end;
end;
```

### 6. 菜单栏 (MainMenu)

**完整菜单结构** (对照第892-953行):

#### File 菜单
```pascal
MenuItem_File
  ├─ MI_NewProject (Ctrl+N)
  ├─ MI_NewInstance (Ctrl+Shift+N)
  ├─ ──────────
  ├─ MenuItem_Undo (Ctrl+Shift+Z)
  ├─ MenuItem_PasteFiles (Ctrl+Shift+V)
  ├─ MenuItem_AddPaths
  ├─ ──────────
  ├─ MenuItem_AddFiles (F3)
  ├─ MenuItem_AddFolders (F4)
  ├─ MenuItem_Preview (F5)
  ├─ MenuItem_Rename (F6)
  ├─ ──────────
  └─ MenuItem_Exit (Alt+F4)
```

#### Settings 菜单
```pascal
MenuItem_Settings
  ├─ MenuItem_SettingsShow (F8)
  ├─ ──────────
  ├─ MenuItem_SettingsGeneral
  ├─ MenuItem_SettingsPreview
  ├─ MenuItem_SettingsRename
  ├─ MenuItem_SettingsMetaTags
  ├─ MenuItem_SettingsMisc
  ├─ ──────────
  ├─ MenuItem_Filters (Ctrl+F)
  ├─ ──────────
  └─ MenuItem_ToggleViewMode
```

#### Presets 菜单
```pascal
MenuItem_Presets
  ├─ MI_LoadPreset (动态填充子菜单)
  ├─ MI_Save (Ctrl+Shift+S)
  ├─ MI_SavePreset (Ctrl+S)
  ├─ ──────────
  ├─ MI_ManagePresets (Ctrl+P)
  ├─ MI_BrowsePresets
  ├─ MI_AddPresets
  ├─ ──────────
  ├─ MI_CreatePresetsLinks
  └─ MI_RescanPresets
```

#### Language 菜单
```pascal
MI_Language
  SubMenuImages = ImageListLanguages (国旗图标)
  // 动态填充语言列表
  ├─ 🇬🇧 English
  ├─ 🇨🇳 简体中文
  ├─ 🇩🇪 Deutsch
  └─ ... (扫描 translations/ 目录)
```

#### Help 菜单
```pascal
MenuItem_Help
  ├─ MI_Help (F1)
  ├─ MI_Forum
  ├─ ──────────
  ├─ MI_QuickGuide
  ├─ MI_UserManual
  ├─ ──────────
  ├─ MI_LiteVsPro
  ├─ MI_Purchase
  ├─ ──────────
  ├─ MI_Register
  ├─ MI_Unregister
  ├─ ──────────
  ├─ MI_History
  ├─ MI_Copyrights
  └─ MI_About (Shift+F1)
```

## Key Features Implementation

### 1. 拖放文件支持 (AllowDropFiles)

```pascal
procedure TFormMain.FormDropFiles(Sender: TObject; const FileNames: array of string);
var
  I: Integer;
begin
  for I := Low(FileNames) to High(FileNames) do
    AddFileOrFolder(FileNames[I]);
  
  // 自动预览
  if AutoPreviewEnabled then
    Preview;
end;
```

### 2. 全局快捷键 (KeyPreview)

```pascal
procedure TFormMain.FormKeyDown(Sender: TObject; var Key: Word; Shift: TShiftState);
begin
  case Key of
    VK_F3: ActionAddFiles.Execute;      // F3 - Add Files
    VK_F4: ActionAddFolders.Execute;    // F4 - Add Folders
    VK_F5: ActionPreview.Execute;       // F5 - Preview
    VK_F6: ActionRename.Execute;        // F6 - Rename
    VK_F8: ActionSettings.Execute;      // F8 - Settings
    VK_F1: if ssShift in Shift then     // Shift+F1 - About
             ActionAbout.Execute
           else
             OpenHelp;                   // F1 - Help
    // ... 其他快捷键
  end;
end;
```

### 3. 窗口状态持久化

```pascal
procedure TFormMain.SaveWindowState;
begin
  Config.WriteInteger('Window', 'Left', Left);
  Config.WriteInteger('Window', 'Top', Top);
  Config.WriteInteger('Window', 'Width', Width);
  Config.WriteInteger('Window', 'Height', Height);
  Config.WriteBool('Window', 'Maximized', WindowState = wsMaximized);
  Config.WriteInteger('Layout', 'RulesPanelHeight', PanelRules.Height);
  Config.WriteInteger('Layout', 'SplitterPosition', SplitterMiddle.Top);
end;

procedure TFormMain.RestoreWindowState;
begin
  // 恢复窗口位置/大小
  Left := Config.ReadInteger('Window', 'Left', (Screen.Width - Width) div 2);
  Top := Config.ReadInteger('Window', 'Top', (Screen.Height - Height) div 2);
  Width := Config.ReadInteger('Window', 'Width', 600);
  Height := Config.ReadInteger('Window', 'Height', 470);
  
  if Config.ReadBool('Window', 'Maximized', False) then
    WindowState := wsMaximized;
    
  // 恢复布局
  PanelRules.Height := Config.ReadInteger('Layout', 'RulesPanelHeight', 145);
end;

procedure TFormMain.FormClose(Sender: TObject; var CloseAction: TCloseAction);
begin
  SaveWindowState;
  SaveRulePreset('__last_session__'); // 保存当前规则
end;
```

## Action System (TActionList)

**28个Action定义** (对照第1092-1137行):

```pascal
ActionListMain: TActionList
  Images = ImageListSmall
  
  // Main Actions
  Action_AddFiles: TAction
    Caption = 'Add Files'
    ShortCut = VK_F3 (114)
    OnExecute = ActionAddFilesExecute
    
  Action_Preview: TAction
    Caption = 'Preview'
    ShortCut = VK_F5 (116)
    OnExecute = ActionPreviewExecute
    
  Action_Rename: TAction
    Caption = 'Rename'
    ShortCut = VK_F6 (117)
    OnExecute = ActionRenameExecute
    
  // Options Actions
  Action_AutosizeColumns: TAction
    Caption = 'Auto-size Columns'
    ShortCut = Shift+S (8275)
    ImageIndex = 7
    
  Action_ValidateNewNames: TAction
    Caption = 'Validate New Names'
    ShortCut = Shift+V (8278)
    ImageIndex = 8
    
  // ... 其他24个Action
```

## Image Resources

**5个ImageList** (对照第1142-1148行):

```pascal
ImageListBig: TImageList
  Width = 32
  Height = 32
  // Add Files, Add Folders, Preview, Rename 等主按钮图标

ImageListSmall: TImageList
  Width = 16
  Height = 16
  // 文件状态图标、菜单图标、Action图标

ImageListToolbarRules: TImageList
  Width = 13
  Height = 13
  // Add, Remove, Up, Down 规则工具栏图标

ImageListToolbarFiles: TImageList
  Width = 12
  Height = 12
  // Files, Columns, Options, Export, Filters, Analyze 文件工具栏图标

ImageListLanguages: TImageList
  Width = 16
  Height = 11
  // 国旗图标(运行时从 flags/ 目录加载)
```

**图标资源组织**:
```
resources/
  ├─ icons/
  │   ├─ 32x32/
  │   │   ├─ add_files.png
  │   │   ├─ add_folders.png
  │   │   ├─ preview.png
  │   │   └─ rename.png
  │   ├─ 16x16/
  │   │   ├─ state_ready.png
  │   │   ├─ state_success.png
  │   │   ├─ state_error.png
  │   │   └─ ... (50+ 小图标)
  │   ├─ 13x13/
  │   │   ├─ rule_add.png
  │   │   └─ ... (4个规则工具栏图标)
  │   └─ 12x12/
  │       └─ ... (6个文件工具栏图标)
  └─ flags/
      ├─ en.png
      ├─ zh_CN.png
      └─ ... (各国国旗16×11)
```

## Testing Requirements

### 单元测试
```pascal
type
  TFormMainTest = class(TTestCase)
  published
    procedure TestFormCreation;
    procedure TestWindowStateRestoration;
    procedure TestDragDropFiles;
    procedure TestKeyboardShortcuts;
    procedure TestSplitterResize;
    procedure TestActionEnableDisable;
  end;

procedure TFormMainTest.TestFormCreation;
var
  Form: TFormMain;
begin
  Form := TFormMain.Create(nil);
  try
    AssertNotNull(Form.ToolBarMain);
    AssertNotNull(Form.ListViewRules);
    AssertNotNull(Form.VSTFiles);
    AssertNotNull(Form.StatusBar);
    AssertEquals(3, Form.ListViewRules.Columns.Count);
  finally
    Form.Free;
  end;
end;
```

### UI自动化测试
使用 LazAutomate 或 Sikuli 测试:
- 菜单导航
- 工具栏按钮点击
- 拖放文件
- 键盘快捷键
- 窗口调整大小
- 分隔条拖动

## 验收标准

WPF 现状：主窗口核心结构、菜单/工具栏/规则与文件区域、状态栏及快捷键已实现；以下清单用于历史对照。

### Phase 1: 基本框架
- [ ] 窗口创建,初始大小600×470,屏幕居中
- [ ] 主工具栏包含6个元素(3按钮+2箭头+标题)
- [ ] 规则面板和文件面板通过Splitter分隔
- [ ] 状态栏3面板显示正确
- [ ] 菜单栏4个主菜单(File/Settings/Presets/Language/Help)

### Phase 2: 交互功能
- [ ] 拖放文件到窗口工作
- [ ] 全局快捷键F3/F4/F5/F6响应
- [ ] Splitter拖动调整面板高度
- [ ] 窗口位置/大小持久化保存/恢复
- [ ] Action启用/禁用状态根据上下文更新

### Phase 3: 视觉完善
- [ ] 所有ImageList加载正确图标
- [ ] 工具栏按钮显示图标+文字
- [ ] 装饰箭头居中对齐
- [ ] 状态栏进度条自绘正确
- [ ] 主题支持(系统主题感知)

### Phase 4: 国际化
- [ ] 语言菜单动态加载
- [ ] 所有字符串支持翻译
- [ ] 国旗图标显示正确
- [ ] 切换语言后界面立即更新

## Dependencies

### Lazarus 组件
- `TForm` - 主窗口基类
- `TToolBar` / `TToolButton` - 工具栏
- `TPanel` - 面板容器
- `TSplitter` - 分隔条
- `TListView` - 规则列表
- `TStatusBar` - 状态栏
- `TMainMenu` / `TMenuItem` - 菜单
- `TActionList` / `TAction` - 动作系统
- `TImageList` - 图标列表

### 第三方组件
- `TLazVirtualStringTree` (详见 Feature 003) - 虚拟文件列表

## Next Steps

1. **创建窗口骨架**
   - 创建 `src/Forms/FormMain.pas` 和 `.lfm`
   - 定义基本组件层次

2. **实现工具栏**
   - 加载图标资源
   - 绑定Action

3. **实现菜单系统**
   - 创建菜单项
   - 实现事件处理器

4. **集成规则列表** (依赖 Feature 001)
   - 连接RuleEngine
   - 实现Add/Edit/Remove规则

5. **集成文件列表** (依赖 Feature 003)
   - 嵌入VST组件
   - 实现文件添加/预览/重命名

---

**Version**: 1.0  
**Created**: 2026-02-12  
**Last Updated**: 2026-02-12  
**Related**: Feature 001 (Rule Engine), Feature 003 (Virtual File List)
