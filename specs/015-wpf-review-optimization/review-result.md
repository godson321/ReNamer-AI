# WPF 全量评审结果（用户确认版）

> 来源：用户在会话中提供的主 UI 与核心流程评审结论（不含 test 项目）
> 目标：将结论落盘，避免上下文丢失，作为 `015-wpf-review-optimization` 的执行依据。

## 0. 015 执行进展（2026-02-20）

### 已完成（阶段 1 + 基础验证）
- [x] T001 修复“回收站删除失败提示条件”逻辑错误。
- [x] T002 接入 `ConfirmRename` 开关（关闭时跳过确认弹窗）。
- [x] T003 恢复窗口位置（使用 `WindowLeft/Top`）并增加越界可见性保护。
- [x] T004 为“添加目录”总入口增加异常隔离与受限目录容错。
- [x] T006 梳理 `AppSettings` “可配不可用”项并建立映射清单。
- [x] T007 接入 `PreviewOnFileAdd` 与 `HighlightChanges`（新增文件自动预览 + 变化高亮受配置控制）。
- [x] T008 接入 `AutoValidate`、`WarnInvalidChars`、`WarnLongPaths`（预览自动校验 + 重命名前风险确认 + 校验开关生效）。
- [ ] 本轮构建验证受环境文件锁影响：`obj/Debug/net8.0-windows/App.g.cs` 与 `ReNamer_MarkupCompile.cache` 访问被拒绝（待本机解锁后复验）。
- [x] T027 构建验证通过：`dotnet build ReNamerWPF/ReNamer/ReNamer.sln`。
- [x] T028 回归测试通过：`dotnet test ReNamerWPF/ReNamer/ReNamer.sln`（134/134）。
- [x] 应用启动冒烟验证通过：`dotnet run --project ReNamerWPF/ReNamer/ReNamer.csproj` 受同一文件锁影响失败，改为直接启动 `ReNamerWPF/ReNamer/bin/Debug/net8.0-windows/ReNamer.exe`（PID 424）并人工终止。

### 代码落点（本轮）
- `ReNamerWPF/ReNamer/Views/MainWindow.xaml.cs`
  - `AddFilePaths`：增加路径级异常隔离与受限目录容错，失败计数写入状态栏。
  - `Rename_Click`：按 `_appSettings.ConfirmRename` 决定是否弹确认。
  - `DeleteToRecycleBin_Click`：删除失败提示改为基于实际删除结果。
  - `RestoreWindowState`：恢复 `WindowLeft/Top`，新增 `IsWindowRectVisible` 防越界。

### 待完成
- [ ] T005：按手工回归项（删除到回收站/重命名确认/窗口位置/添加目录）补充人工验证记录。
- [ ] 阶段 2~5 任务已启动，已完成 T006/T007/T008，待执行 T009~T026。

### T006：AppSettings“可配不可用”映射清单

| 配置项 | 当前状态 | 证据（定义/使用） | 处理建议 |
|---|---|---|---|
| `LoadLastPreset` | 可配不可用 | 定义：`ReNamerWPF/ReNamer/Services/AppSettings.cs:15`；仅在设置页读写：`ReNamerWPF/ReNamer/Views/SettingsDialog.xaml.cs:23`、`ReNamerWPF/ReNamer/Views/SettingsDialog.xaml.cs:64` | 在启动流程接入“自动加载上次预设”（对应 T009/T011 后续） |
| `CheckForUpdates` | 可配不可用 | 定义：`ReNamerWPF/ReNamer/Services/AppSettings.cs:17`；仅在设置页读写：`ReNamerWPF/ReNamer/Views/SettingsDialog.xaml.cs:25`、`ReNamerWPF/ReNamer/Views/SettingsDialog.xaml.cs:66` | 增加启动或菜单触发的更新检查链路 |
| `ConfirmOnExit` | 可配不可用 | 定义：`ReNamerWPF/ReNamer/Services/AppSettings.cs:18`；仅在设置页读写：`ReNamerWPF/ReNamer/Views/SettingsDialog.xaml.cs:26`、`ReNamerWPF/ReNamer/Views/SettingsDialog.xaml.cs:67` | 在窗口关闭事件接入退出确认分支 |
| `ShowInSystemTray` | 可配不可用 | 定义：`ReNamerWPF/ReNamer/Services/AppSettings.cs:19`；仅在设置页读写：`ReNamerWPF/ReNamer/Views/SettingsDialog.xaml.cs:27`、`ReNamerWPF/ReNamer/Views/SettingsDialog.xaml.cs:68` | 增加托盘图标与最小化到托盘行为 |
| `PreviewOnFileAdd` | 已接入（T007） | 定义：`ReNamerWPF/ReNamer/Services/AppSettings.cs:25`；设置读写：`ReNamerWPF/ReNamer/Views/SettingsDialog.xaml.cs:46`、`ReNamerWPF/ReNamer/Views/SettingsDialog.xaml.cs:77`；主流程接入：`ReNamerWPF/ReNamer/Views/MainWindow.xaml.cs:575` | 已按开关在 `AddFilePaths` 后触发预览 |
| `HighlightChanges` | 已接入（T007） | 定义：`ReNamerWPF/ReNamer/Services/AppSettings.cs:26`；设置读写：`ReNamerWPF/ReNamer/Views/SettingsDialog.xaml.cs:47`、`ReNamerWPF/ReNamer/Views/SettingsDialog.xaml.cs:78`；主流程接入：`ReNamerWPF/ReNamer/Views/MainWindow.xaml.cs:990`、`ReNamerWPF/ReNamer/Views/MainWindow.xaml.cs:1047` | 变化状态由配置控制，关闭时不显示“→”高亮 |
| `AutoValidate` | 已接入（T008） | 定义：`ReNamerWPF/ReNamer/Services/AppSettings.cs:27`；设置读写：`ReNamerWPF/ReNamer/Views/SettingsDialog.xaml.cs:48`、`ReNamerWPF/ReNamer/Views/SettingsDialog.xaml.cs:79`；主流程接入：`ReNamerWPF/ReNamer/Views/MainWindow.xaml.cs:1038`、`ReNamerWPF/ReNamer/Views/MainWindow.xaml.cs:1060` | 预览后自动执行校验，重命名前对校验风险二次确认 |
| `WarnInvalidChars` | 已接入（T008） | 定义：`ReNamerWPF/ReNamer/Services/AppSettings.cs:28`；设置读写：`ReNamerWPF/ReNamer/Views/SettingsDialog.xaml.cs:49`、`ReNamerWPF/ReNamer/Views/SettingsDialog.xaml.cs:80`；校验接入：`ReNamerWPF/ReNamer/Views/MainWindow.xaml.cs:1121`、`ReNamerWPF/ReNamer/Services/RenameService.cs:110` | 校验结果按开关启用/禁用非法字符警告 |
| `WarnLongPaths` | 已接入（T008） | 定义：`ReNamerWPF/ReNamer/Services/AppSettings.cs:29`；设置读写：`ReNamerWPF/ReNamer/Views/SettingsDialog.xaml.cs:50`、`ReNamerWPF/ReNamer/Views/SettingsDialog.xaml.cs:81`；校验接入：`ReNamerWPF/ReNamer/Views/MainWindow.xaml.cs:1121`、`ReNamerWPF/ReNamer/Services/RenameService.cs:110` | 校验结果按开关启用/禁用长路径警告 |
| `AutoRemoveRenamed` | 可配不可用 | 定义：`ReNamerWPF/ReNamer/Services/AppSettings.cs:33`；仅在设置页读写：`ReNamerWPF/ReNamer/Views/SettingsDialog.xaml.cs:53`、`ReNamerWPF/ReNamer/Views/SettingsDialog.xaml.cs:84` | 重命名后按开关自动移除已完成项（对应 T009） |
| `CreateUndoLog` | 可配不可用 | 定义：`ReNamerWPF/ReNamer/Services/AppSettings.cs:34`；仅在设置页读写：`ReNamerWPF/ReNamer/Views/SettingsDialog.xaml.cs:54`、`ReNamerWPF/ReNamer/Views/SettingsDialog.xaml.cs:85` | 将 Undo 日志生成改为可配置（对应 T009） |
| `ConflictResolution` | 可配不可用 | 定义：`ReNamerWPF/ReNamer/Services/AppSettings.cs:36`；仅在设置页读写：`ReNamerWPF/ReNamer/Views/SettingsDialog.xaml.cs:57`~`ReNamerWPF/ReNamer/Views/SettingsDialog.xaml.cs:90` | 在 `RenameService` 冲突分支接入策略（对应 T010） |
| `FolderSaveAsDefault` | 部分生效（仅持久化） | 定义：`ReNamerWPF/ReNamer/Services/AppSettings.cs:64`；过滤器弹窗读写：`ReNamerWPF/ReNamer/Views/FiltersDialog.xaml.cs:31`、`ReNamerWPF/ReNamer/Views/FiltersDialog.xaml.cs:45` | 明确“保存为默认”的生效时机并在启动/导入时应用 |

---

## 1. 关键问题（优先修复）

### P0（功能未落地）
- 多项设置只在设置页读写，但主流程未使用，属于“可配不可用”。
- 相关位置：
  - 定义：`ReNamerWPF/ReNamer/Services/AppSettings.cs:15`
  - 定义：`ReNamerWPF/ReNamer/Services/AppSettings.cs:18`
  - 定义：`ReNamerWPF/ReNamer/Services/AppSettings.cs:19`
  - 读写：`ReNamerWPF/ReNamer/Views/SettingsDialog.xaml.cs:23`
  - 读写：`ReNamerWPF/ReNamer/Views/SettingsDialog.xaml.cs:64`

### P0（逻辑 bug）
- 回收站删除后失败提示条件写错，当前与“是否最大化窗口”绑定。
- 相关位置：
  - `ReNamerWPF/ReNamer/Views/MainWindow.xaml.cs:2017`
  - `ReNamerWPF/ReNamer/Views/MainWindow.xaml.cs:2021`

### P0（配置与行为不一致）
- `ConfirmRename` 已有配置，但重命名流程始终弹确认。
- 相关位置：
  - 配置：`ReNamerWPF/ReNamer/Services/AppSettings.cs:32`
  - 流程：`ReNamerWPF/ReNamer/Views/MainWindow.xaml.cs:1057`

### P1（稳定性）
- 批量加目录时缺少总入口异常隔离，受限目录可能抛异常中断。
- 相关位置：
  - `ReNamerWPF/ReNamer/Views/MainWindow.xaml.cs:606`
  - `ReNamerWPF/ReNamer/Views/MainWindow.xaml.cs:630`

### P1（窗口状态 bug）
- 保存了 `WindowLeft/Top`，但恢复时未使用，启动总是居中。
- 相关位置：
  - 保存：`ReNamerWPF/ReNamer/Views/MainWindow.xaml.cs:2421`
  - 恢复：`ReNamerWPF/ReNamer/Views/MainWindow.xaml.cs:2371`

---

## 2. 按维度优化建议

### 2.1 功能完整性
- 将以下设置接入主流程：
  - `PreviewOnFileAdd`（`ReNamerWPF/ReNamer/Services/AppSettings.cs:25`）
  - `HighlightChanges`（`ReNamerWPF/ReNamer/Services/AppSettings.cs:26`）
  - `AutoValidate`（`ReNamerWPF/ReNamer/Services/AppSettings.cs:27`）
  - `WarnInvalidChars`（`ReNamerWPF/ReNamer/Services/AppSettings.cs:28`）
  - `WarnLongPaths`（`ReNamerWPF/ReNamer/Services/AppSettings.cs:29`）
  - `AutoRemoveRenamed`（`ReNamerWPF/ReNamer/Services/AppSettings.cs:33`）
  - `CreateUndoLog`（`ReNamerWPF/ReNamer/Services/AppSettings.cs:34`）
  - `ConflictResolution`（`ReNamerWPF/ReNamer/Services/AppSettings.cs:36`）
- `AddFolders` 默认体验偏“空列表”：
  - 当前默认 `FolderIncludeAllFiles=false`，首次使用建议默认导入文件。
  - 参考：`ReNamerWPF/ReNamer/Services/AppSettings.cs:55`、`ReNamerWPF/ReNamer/Views/MainWindow.xaml.cs:539`
- 扩展 `ValidateNewNames`：
  - 当前仅重复/非法字符检查（`ReNamerWPF/ReNamer/Services/RenameService.cs:111`）
  - 建议补充保留名（CON/PRN）、路径长度、大小写冲突、跨卷预检查。

### 2.2 易用性（交互）
- 全局快捷键大量使用 `Shift+字母`（`ReNamerWPF/ReNamer/Views/MainWindow.xaml.cs:478`），建议迁移到 `Ctrl+Shift+` 并在菜单显式展示。
- 多个输入对话框为代码临时 Window：
  - `ReNamerWPF/ReNamer/Views/MainWindow.xaml.cs:1323`
  - `ReNamerWPF/ReNamer/Views/MainWindow.xaml.cs:2175`
  - 建议统一可复用对话框（校验/Enter/Esc/资源化）。
- 状态栏文案混用资源与硬编码：
  - `Files: ...` 硬编码于 `ReNamerWPF/ReNamer/Views/MainWindow.xaml.cs:423`
  - 建议全资源化，避免中英混杂。

### 2.3 可用性/可靠性
- 预览与重命名在 UI 线程执行：
  - UI 调用：`ReNamerWPF/ReNamer/Views/MainWindow.xaml.cs:1038`、`ReNamerWPF/ReNamer/Views/MainWindow.xaml.cs:1062`
  - 核心循环：`ReNamerWPF/ReNamer/Services/RenameService.cs:37`、`ReNamerWPF/ReNamer/Services/RenameService.cs:69`
  - 建议：`async + Task.Run + CancellationToken`。
- 多处空 `catch` 吞异常：
  - `ReNamerWPF/ReNamer/Services/AppSettings.cs:101`
  - `ReNamerWPF/ReNamer/Services/AppSettings.cs:114`
  - `ReNamerWPF/ReNamer/Views/MainWindow.xaml.cs:2252`
  - 建议：最少写日志并给用户可理解提示。
- 回收站删除失败反馈过于粗粒度：
  - `ReNamerWPF/ReNamer/Views/MainWindow.xaml.cs:2082`
  - 建议按文件维度汇总失败原因。

### 2.4 界面美观度/一致性
- 主题体系重复定义源：
  - `ReNamerWPF/ReNamer/App.xaml:10`
  - `ReNamerWPF/ReNamer/App.xaml:12`
  - 建议：Design Tokens 单一来源 + ThemeService 只替换 token。
- 主窗口无系统边框 + 自绘标题栏：
  - `ReNamerWPF/ReNamer/Views/MainWindow.xaml:12`
  - 建议补齐高对比度、系统菜单、边缘双击等系统行为细节。
- 列表项高度在高 DPI 偏紧：
  - `ReNamerWPF/ReNamer/Views/MainWindow.xaml:50`
  - 建议提升到 30~34 并增加行内留白。
- 列宽策略偏固定：
  - 固定列宽示例：`ReNamerWPF/ReNamer/Views/MainWindow.xaml:585`
  - 切换列显示重置为 120：`ReNamerWPF/ReNamer/Views/MainWindow.xaml.cs:1809`
  - 建议保存每列“最后宽度”。
- 资源键一致性风险：
  - `TextPrimaryBrush` 出现在规则弹窗：`ReNamerWPF/ReNamer/Views/AddRuleDialog.xaml:45`
  - 主色系常用 `TextBrush`，建议统一语义键。

---

## 3. 建议落地顺序（两阶段）

### 第一阶段（1~2 周，先稳）
- 修 P0/P1：设置落地、删除失败条件、窗口位置恢复、目录导入异常保护、重命名确认开关。

### 第二阶段（2~3 周，提体验）
- 异步化预览/重命名 + 取消按钮。
- 统一对话框组件。
- 快捷键规范化。
- 列宽/状态栏/多语言一致性治理。
- 主题 token 收敛。
