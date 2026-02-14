# ReNamer WPF 需求对比检查清单

**更新时间**: 2026-02-13（对齐当前实现）  
**冲突处理**: 若与需求有冲突，以 `docs/rules_requirements.md` 为准。

基于 `rules_requirements.md` 全量需求逐项审查，标注当前实现状态。

## 一、规则窗口 (Form_AddRule)

**已实现:**
- 18种规则类型注册 (含 PascalScript) — `AddRuleDialog.xaml.cs`
- 左侧规则类型列表 + 右侧GroupBox配置区域
- Add Rule/Close 按钮, Enter/Esc 快捷键
- 编辑模式 (Title变"Edit Rule", 禁用规则切换, 加载现有参数)
- 双击列表确认添加
- AllowDropFiles (拖放文件转发给主窗口)

**缺失/不足:**
- [x] 左侧列表右键菜单 "Description" (查看规则描述)

## 二、规则Execute逻辑

### 2.1 Replace — ✅完整
- FindText/ReplaceText/CaseSensitive/WholeWordsOnly/SkipExtension/UseWildcards/Occurrence(All/First/Last)
- 通配符支持 `?*[]` + `$n` 反向引用

### 2.2 Insert — ✅完整
- 6种插入位置: Prefix/Suffix/Position/AfterText/BeforeText/ReplaceCurrentName
- RightToLeft, SkipExtension

### 2.3 Delete — ✅完整
- From: Position/Delimiter, Until: Count/Delimiter/TillEnd
- DeleteCurrentName, RightToLeft, SkipExtension, LeaveDelimiter

### 2.4 Remove — ✅完整
- Pattern, Occurrence(All/First/Last), CaseSensitive, WholeWordsOnly, SkipExtension, UseWildcards

### 2.5 Case — ✅完整
- 7种模式 + PreserveCase + ForceCase + ExtAlwaysLower/Upper

### 2.6 Serialize — ✅完整
- StartNumber/Repeat/Step/ResetEvery/ResetIfFolder/ResetIfFileName
- PadToLength/NumberingSystem(Decimal/Hex/Octal/Binary/Custom)/InsertWhere

### 2.7 RegEx — ✅完整
- Expression/ReplaceText/CaseSensitive/SkipExtension

### 2.8 Extension — ✅完整
- NewExtension/AppendToOriginal/DetectBinarySignature/RemoveDuplicate/CaseSensitive

### 2.9 Padding — ✅完整
- AddZeroPadding/RemoveZeroPadding (互斥), AddTextPadding/Position

### 2.10 Strip — ✅完整
- 6种字符类型 + Where(Everywhere/Leading/Trailing) + StripAllExceptSelected + CaseSensitive

### 2.11 CleanUp — ✅完整
- 括号剥离/字符替换空格/SkipVersionNumbers/FixSpaces/NormalizeUnicodeSpaces
- StripEmoji/StripMarks/InsertSpaceBeforeCapitals/PrepareForSharePoint

### 2.12 Transliterate — ✅完整
- Alphabet/Direction/AutoCaseAdjustment

### 2.13 Rearrange — ✅完整
- SplitMode(Delimiters/Positions/ExactPattern), NewPattern, $N/$-N/$0, RightToLeft

### 2.14 ReformatDate — ✅完整
- SourceFormat/TargetFormat/WholeWordsOnly/CustomMonths/AdjustTime

### 2.15 Randomize — ✅完整
- Length/Unique/UseDigits/UseEnglishLetters/UseUserDefined/InsertWhere

### 2.16 UserInput — ✅完整
- InputText/Mode(Replace/InsertBefore/InsertAfter)/SkipExtension

### 2.17 Mapping — ✅完整
- Mappings/AllowReuse/PartialMatch/InverseMapping/CaseSensitive/SkipExtension(默认false)

## 三、规则ConfigPanel UI

**已实现:** 全部17个规则均有配置面板 (ReplaceConfigPanel ~ MappingConfigPanel)

**所有面板通用功能:**
- ✅ Meta Tag 按钮 (Replace/Insert/Rearrange)
- ✅ 分隔符按钮 (Replace/Remove/Rearrange/ReformatDate 的 "Separate multiple items")

**各面板与需求差异检查 (已补齐):**
- [x] Replace: SpeedButton_ReplaceAddSeparator + SpeedButton_ReplaceWithMetaTag
- [x] Insert: BitBtn_InsertMetaTag
- [x] Remove: SpeedButton_RemoveAddSeparator
- [x] RegEx: SB_InsertExpression (弹出表达式菜单) + SpeedButton_Help
- [x] Rearrange: SB_AddSeparator + SB_InsertMetaTag
- [x] ReformatDate: SpeedButton_Help 弹出菜单 (含 Separator + DateTime格式参考)
- [x] Serialize: NumberingSystem ComboBox 已填充 Items (Decimal/Hex/Octal/Binary/Custom)

## 四、主窗口 (MainWindow)

### 4.1 菜单栏

**已实现:**
- File: New Project, New Instance, Undo, Add Files, Add Folders, Paste Files, Preview, Rename, Exit
- Presets: Load, Save, Manage Presets, Browse Presets Folder, Import Preset, Rescan Presets
- Settings: 单一入口（F8）
- Help: Help Online, Quick Guide, User Manual, Forum, Lite vs Pro, Purchase, Register/Unregister, Version History, Copyrights, Language, About

**缺失:**
- [x] File → Add Paths
- [x] Settings 菜单子项已拆分 (General/Preview/Rename/Meta Tags/Misc/Filters)
- [x] Presets → Save As (Ctrl+S 为 Save As; Ctrl+Shift+S 为 Save)
- [x] Presets → Create Links

### 4.2 主工具栏 — ✅基本完整
- Add Files, Add Folders, 箭头, Preview, 箭头, Rename — 全部有矢量图标

### 4.3 规则面板

**已实现:** Add/Remove/Up/Down 工具栏 + ListView(CheckBox, Rule, Statement)

**已补齐:**
- ✅ 序号列 "#" (spec: Column 0 = #, MinWidth=50)
- ✅ 规则拖放排序 (spec: OnDragDrop/OnDragOver)

### 4.4 规则右键菜单

**已实现:** Add Rule, Edit Rule, Duplicate Rule, Remove Rule, Remove All, Move Up/Down, Select All, Mark All, Unmark All

**已补齐:**
- ✅ "Add Rule (above)" — 在选中规则上方插入
- ✅ "Add Rule (below)" — 在选中规则下方插入
- ✅ "Export to Clipboard" — 规则序列化到剪贴板
- ✅ "Comment..." (Shift+Enter) — 添加规则注释

### 4.5 文件面板

**已实现:** 列结构已扩展，默认显示 5 列 (State/Name/NewName/Error + CheckBox)
- Columns 工具栏按钮 + 列显示/隐藏切换菜单（可切换隐藏列）

**缺失的列 (需求共21列):**
- (无)

**缺失功能:**
- [x] 列标题右键菜单 (PM_FilesColumns: 动态列切换 + Cancel Sorting)
- [x] 默认列可见性: 仅 State/Name/NewName/Error 默认可见, 其余隐藏
- [x] 文件拖放排序 (列表内拖动调整顺序)
- [x] 双击列边框自动调整列宽

### 4.6 文件右键菜单

**已实现:** Edit New Name, Shell(Open File, Open Folder), Mark(5项), Clear(6项), Select(2项), Move(Up/Down), Remove Selected

**缺失项:**
- [x] Analyze Name (菜单已补齐)

### 4.7 键盘快捷键

**已实现 (全局):** F1, Shift+F1, F3/F4/F5/F6, F8, Ctrl+N, Ctrl+Shift+N, Ctrl+Shift+Z, Ctrl+Shift+V, Ctrl+S, Ctrl+F, Ctrl+P, Alt+F4

**缺失 (全局):**
- [x] Ctrl+Shift+S (Save Preset)

**已实现 (文件列表):** F2, Enter, Ctrl+Enter, Del, Ctrl+Del, Ctrl+D, Ctrl+A, Ctrl+I, Ctrl+Up/Down, Ctrl+E, Ctrl+L, Ctrl+M, Shift+M, Shift+U, Insert

**缺失 (文件列表):**
- [x] Shift+Enter (Open with Notepad)
- [x] Alt+Enter (File Properties)
- [x] Ctrl+Shift+X (Cut files), Ctrl+Shift+C (Copy files)

**已实现 (规则列表):** Ins, Enter, Del, Shift+Ins, Shift+Del, Ctrl+Up/Down, Ctrl+A, Shift+M, Shift+U

**缺失 (规则列表):**
- (无)

**Options快捷键:** 已实现 (Shift+S/V/F/A/C/I/R)

### 4.8 StatusBar

**已实现:** 3面板 (Progress text + ProgressBar, Files stats, Info)

**缺失:**
- [x] Click/MouseMove 事件

## 五、核心服务

### 5.1 RenameService — ✅基本完整
- Preview / Rename / UndoRename / ValidateNewNames
- **问题:** Rename 不检查 IsMarked

### 5.2 PresetService — ✅完整
- SaveToJson / LoadFromJson, 支持全部17种规则

### 5.3 LanguageService — ✅完整
- 中英文切换, 字符串资源

## 六、模型 (RenFile)

**已实现:** FullPath, OriginalName, NewName, FolderPath, Extension, Size, SizeDisplay, SizeKB, SizeMB, Created, Modified, IsFolder, IsMarked, IsRenamed, HasChanged, State, Error, OldPath, BaseName, NewPath, NameDigits, PathDigits, NameLength, NewNameLength, PathLength, NewPathLength

**缺失:**
- [x] ExifDate 属性 (需读取EXIF数据)

## 七、Options功能 (PM_Options菜单)

**已实现:** Validate + Autosize Columns + Fix Conflicts + Analyze Sample Text + Apply Rules to Clipboard + Count Files + Sort for Folders

**缺失:** (无)

## 八、Export功能 (PM_Export菜单)

**已实现:** 批处理导出/剪贴板导出/导入 (New Names/Names+New Names/Paths/All Columns/Batch/Import List)

**缺失:**
- [x] Export Files and Undo (原路径|撤销路径)
- [x] Export Files and Preview (原路径|新名)
- [x] Import Files and Preview (从文件导入)

## 九、其他缺失功能

- [x] Meta Tag 解析/替换逻辑已实现（Insert/Replace/Rearrange 支持解析）
- ✅ Filters 对话框 (已实现)
- ✅ Settings 对话框 (已实现)
- ✅ 预设管理器 (Manage Presets)
- [x] 窗口状态持久化：位置/大小/分隔条/列宽/可见性均已持久化
- [x] Undo 使用 OldPath 而非 OriginalName

## 十、Bug / 逻辑问题

- [x] `RenameService.Rename` 已检查 `IsMarked`（更健壮）
- [x] Presets 快捷键映射错误: Ctrl+S 应为 Save As, Ctrl+Shift+S 才是 Save
- [x] Undo 仍使用 `OriginalName` 而非 `OldPath`（撤销路径不准确）
