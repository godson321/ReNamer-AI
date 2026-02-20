# WPF 全量评审结果（用户确认版）

> 来源：用户在会话中提供的主 UI 与核心流程评审结论（不含 test 项目）
> 目标：将结论落盘，避免上下文丢失，作为 `015-wpf-review-optimization` 的执行依据。

## 0. 015 执行进展（2026-02-20）

### 收口更新（阶段 3/5/6）

| 任务 | 结果 | 说明 |
|---|---|---|
| T015 | ✅ 完成 | `RenameService.Rename` 增加 `CancellationToken` 感知与中断返回标识。 |
| T016 | ✅ 完成 | `AppSettings` 与 `MainWindow` 空 `catch` 已替换为日志记录 + 用户可读提示。 |
| T017 | ✅ 完成 | 回收站删除改为逐文件执行，失败原因按文件汇总展示。 |
| T022~T026 | ✅ 完成 | 主题颜色收敛到 Design Tokens；无边框系统行为/高对比度补齐；行高留白与列宽持久化完成；文字资源键统一。 |
| T029 | ✅ 完成 | 结合 `FEATURE_TESTING_GUIDE.md` 与本轮多次交互回归，关键路径（回收站删除/重命名确认/窗口位置/添加目录）通过。 |
| T005 | ✅ 完成 | 手工回归项与 T029 同步验收，已补齐记录。 |
| T030 | ✅ 完成 | 本文件已更新为最新完成状态。 |
| T031 | ✅ 完成 | `plan.md` 已补充变更摘要与剩余风险。 |

### 已完成（阶段 1~6）
- [x] T001 修复“回收站删除失败提示条件”逻辑错误。
- [x] T002 接入 `ConfirmRename` 开关（关闭时跳过确认弹窗）。
- [x] T003 恢复窗口位置（使用 `WindowLeft/Top`）并增加越界可见性保护。
- [x] T004 为“添加目录”总入口增加异常隔离与受限目录容错。
- [x] T006 梳理 `AppSettings` “可配不可用”项并建立映射清单。
- [x] T007 接入 `PreviewOnFileAdd` 与 `HighlightChanges`（新增文件自动预览 + 变化高亮受配置控制）。
- [x] T008 接入 `AutoValidate`、`WarnInvalidChars`、`WarnLongPaths`（预览自动校验 + 重命名前风险确认 + 校验开关生效）。
- [x] T009 接入 `AutoRemoveRenamed`、`CreateUndoLog`（重命名后自动移除成功项 + 可配置生成 Undo 日志）。
- [x] T010 接入 `ConflictResolution`（跳过冲突 / 自动后缀 / 覆盖已存在目标）。
- [x] T011 优化 AddFolders 默认导入策略（首次目录导入默认包含文件）。
- [x] T012 扩展 `ValidateNewNames`（保留名 / 长路径 / 大小写冲突 / 跨卷预检查）。
- [x] T013 实时预览改造为 `async + Task.Run + CancellationToken`（自动预览支持取消与防抖，手动预览复用同一异步管线）。
- [x] T014 重命名流程改造为 `async + Task.Run + CancellationToken`（UI 线程不再直接执行重命名循环）。
- [x] T015 `RenameService` 增加取消感知与中断返回标识（支持上层取消链路）。
- [x] T016 空 `catch` 已替换为“日志记录 + 用户可理解提示”（`AppSettings` 与 `MainWindow`）。
- [x] T017 回收站删除失败改为按文件汇总原因并展示。
- [x] T037 首页列表大数据性能优化（ListView 虚拟化 + 预览/重命名进度上报节流）。
- [x] T038 首页滚动性能优化（`ExifDateDisplay` 异步懒加载，避免滚动时同步 EXIF IO）。
- [x] T039 首页文件表格重构为 `DataGrid`（行列双虚拟化 + 容器复用）。
- [x] T040 首页文件表格企业级视觉改造（统一表头/行/单元格风格，增强选中与悬停层次）。
- [x] T032 实时预览按规则顺序自动生效（规则调整后即时刷新最终结果）。
- [x] T033 预览差异高亮：新旧文件名不一致时对“新文件名”使用差异色，一致保持默认色。
- [x] T034 移除顶部“预览”按钮与文件菜单“预览”入口，仅保留实时预览链路。
- [x] T035 文件表格排序方向可视化：表头显示升序/降序箭头。
- [x] T036 文件表格增加 Delete 快捷键删除选中项，并在右键菜单显示 `Del` 提示。
- [x] T021 状态栏文件统计文本资源化（中英文均改为格式化资源串）。
- [x] T019 复用输入对话框组件落地（支持校验、Enter/Esc、资源化按钮文案）。
- [x] T020 MainWindow 临时输入窗口调用点已切换到复用对话框（路径输入/新名称编辑/规则备注/选择与标记输入）。
- [x] T041 文件表格第一列增加“全选/全不选”复选框（批量切换 IsMarked）。
- [x] T022 主题颜色来源收敛为 Design Tokens 单一源（`App.xaml` + `DesignSystemResources.xaml` + `Theme.xaml`）。
- [x] T023 无边框窗口系统行为补齐（高对比度、系统菜单、Alt+Space、双击标题栏、最大化拖拽恢复）。
- [x] T024 文件列表行高与留白优化（高 DPI 友好）。
- [x] T025 列宽持久化改造（保存每列最后宽度，重置恢复默认基线，不再回退固定 120）。
- [x] T026 文字前景语义资源键统一（移除 `TextPrimaryBrush`，统一 `TextBrush`）。
- [x] T005/T029 人工回归记录补齐（回收站删除、重命名确认、窗口位置、添加目录关键路径）。
- [x] T012 构建验证通过（沙箱外）：`dotnet build ReNamerWPF/ReNamer/ReNamer.csproj`。
- [x] T027 构建验证通过：`dotnet build ReNamerWPF/ReNamer/ReNamer.sln`。
- [x] T028 回归测试通过：`dotnet test ReNamerWPF/ReNamer/ReNamer.sln`（134/134）。
- [x] T012 启动冒烟验证通过（沙箱外）：`dotnet run --project ReNamerWPF/ReNamer/ReNamer.csproj`（短时拉起后结束进程）。
- [x] T012 回归测试通过（沙箱外）：`dotnet test ReNamerWPF/ReNamer/ReNamer.sln`（134/134）。
- [x] T013 构建验证通过（沙箱外）：`dotnet build ReNamerWPF/ReNamer/ReNamer.csproj`。
- [x] T013 启动冒烟验证通过（沙箱外）：`dotnet run --project ReNamerWPF/ReNamer/ReNamer.csproj`（短时拉起后结束进程）。
- [x] T014 构建验证通过（沙箱外）：`dotnet build ReNamerWPF/ReNamer/ReNamer.csproj`。
- [x] T014 启动冒烟验证通过（沙箱外）：`dotnet run --project ReNamerWPF/ReNamer/ReNamer.csproj`（短时拉起后结束进程）。
- [x] T037 构建验证通过（沙箱外）：`dotnet build ReNamerWPF/ReNamer/ReNamer.csproj`。
- [x] T037 启动冒烟验证通过（沙箱外）：`dotnet run --project ReNamerWPF/ReNamer/ReNamer.csproj`（短时拉起后结束进程）。
- [x] T038 构建验证通过（沙箱外）：`dotnet build ReNamerWPF/ReNamer/ReNamer.csproj`。
- [x] T038 启动冒烟验证通过（沙箱外）：`dotnet run --project ReNamerWPF/ReNamer/ReNamer.csproj`（短时拉起后结束进程）。
- [x] T039 构建验证通过（沙箱外）：`dotnet build ReNamerWPF/ReNamer/ReNamer.csproj`。
- [x] T039 启动冒烟验证通过（沙箱外）：`dotnet run --project ReNamerWPF/ReNamer/ReNamer.csproj`（短时拉起后结束进程）。
- [x] T040 构建验证通过（沙箱外）：`dotnet build ReNamerWPF/ReNamer/ReNamer.csproj`。
- [x] T040 启动冒烟验证通过（沙箱外）：`dotnet run --project ReNamerWPF/ReNamer/ReNamer.csproj`（短时拉起后结束进程）。
- 说明：普通沙箱模式下仍可能出现 WPF 中间文件 `Access denied` 误报；在本机提权/VS 环境验证通过。

### 代码落点（本轮）
- `ReNamerWPF/ReNamer/Views/MainWindow.xaml.cs`
  - `AddFilePaths`：增加路径级异常隔离与受限目录容错，失败计数写入状态栏。
  - `AddFilePaths`：首次目录导入时自动应用“包含文件”默认策略并持久化。
  - `Rename_Click`：接入 `_appSettings.ConfirmRename` / `_appSettings.AutoRemoveRenamed` / `_appSettings.CreateUndoLog` / `_appSettings.ConflictResolution`。
  - `DeleteToRecycleBin_Click`：删除失败提示改为基于实际删除结果。
  - `RestoreWindowState`：恢复 `WindowLeft/Top`，新增 `IsWindowRectVisible` 防越界。
  - `AutoPreviewIfEnabled` / `Preview_Click` / `Rename_Click`：改为复用 `ExecutePreviewAsync`（异步预览、取消、自动触发防抖）。
  - 新增 `BeginNewPreviewRequest`、`CancelPendingPreview`：管理预览任务取消与版本一致性。
  - `Rename_Click`：改为后台 `Task.Run` 执行 `_renameService.Rename`，并通过 `IProgress` 回传 UI 进度。
  - 新增 `BeginNewRenameRequest`、`CancelPendingRename`：管理重命名任务取消与版本一致性。
  - 新增 `ShouldReportProgress`，将进度更新节流为约 1% 粒度，减少大批量 UI 刷新压力。
- `ReNamerWPF/ReNamer/Views/MainWindow.xaml`
  - `lvFiles` / `lvRules` 启用 `VirtualizingPanel.IsVirtualizing` 与 `VirtualizationMode=Recycling`，降低大数据滚动卡顿。
  - `lvFiles` 从 `ListView+GridView` 重构为 `DataGrid`，启用 `EnableRowVirtualization` 与 `EnableColumnVirtualization`。
  - 新增 `EnterpriseDataGrid*` 样式组（头部/行/单元格），统一企业级视觉与交互反馈。
  - 进一步增强深浅分层（表头/奇偶行/悬停/排序态）并统一单元格文本垂直居中（显示与编辑态一致）。
- `ReNamerWPF/ReNamer/Models/RenFile.cs`
  - `ExifDateDisplay` 改为异步懒加载，滚动渲染阶段不再同步打开文件读取 EXIF。
  - 增加 EXIF 读取并发门限（`SemaphoreSlim`），避免大批量滚动时磁盘并发抖动。
  - 增加 EXIF 文件类型白名单（仅 jpg/jpeg/tif/tiff），避免非图片触发 `PresentationCore` 解码异常噪音。
- `ReNamerWPF/ReNamer/Services/RenameService.cs`
  - `Rename`：新增冲突策略分支（Skip / AddSuffix / Overwrite），并处理批内目标冲突。
  - `ValidateNewNames`：新增保留名校验、大小写冲突检测、跨卷预检查、目标已存在预检与路径规范化。
  - 新增 `ComputePreview`：预览计算与 UI 写回分离，支持 `CancellationToken` 中断。
- `ReNamerWPF/ReNamer/Services/AppSettings.cs`
  - 增加 `FolderImportDefaultsInitialized`，用于控制首次目录导入默认策略只生效一次。
  - 新增 `LastLoadError`、`LastSaveError`，`Load/Save` 异常改为可追踪并返回结果。
- `ReNamerWPF/ReNamer/Resources/DesignSystemResources.xaml`
  - 扩展并集中维护主题色/语义画刷 token，作为颜色单一来源。
- `ReNamerWPF/ReNamer/Themes/Theme.xaml`
  - 清理重复颜色定义，仅保留基于 token 的样式层。
- `ReNamerWPF/ReNamer/App.xaml`
  - 资源字典加载顺序明确为“Design Tokens -> Theme 样式”。
- `ReNamerWPF/ReNamer/Views/AddRuleDialog.xaml`
  - 文字前景资源键统一为 `TextBrush`。

### 待完成
- 当前 `tasks.md` 全部任务已勾选完成，本节保留为归档说明。

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
| `WarnInvalidChars` | 已接入（T008） | 定义：`ReNamerWPF/ReNamer/Services/AppSettings.cs:28`；设置读写：`ReNamerWPF/ReNamer/Views/SettingsDialog.xaml.cs:49`、`ReNamerWPF/ReNamer/Views/SettingsDialog.xaml.cs:80`；校验接入：`ReNamerWPF/ReNamer/Views/MainWindow.xaml.cs:1166`、`ReNamerWPF/ReNamer/Services/RenameService.cs:212` | 校验结果按开关启用/禁用非法字符警告 |
| `WarnLongPaths` | 已接入（T008） | 定义：`ReNamerWPF/ReNamer/Services/AppSettings.cs:29`；设置读写：`ReNamerWPF/ReNamer/Views/SettingsDialog.xaml.cs:50`、`ReNamerWPF/ReNamer/Views/SettingsDialog.xaml.cs:81`；校验接入：`ReNamerWPF/ReNamer/Views/MainWindow.xaml.cs:1166`、`ReNamerWPF/ReNamer/Services/RenameService.cs:212` | 校验结果按开关启用/禁用长路径警告 |
| `AutoRemoveRenamed` | 已接入（T009） | 定义：`ReNamerWPF/ReNamer/Services/AppSettings.cs:33`；设置读写：`ReNamerWPF/ReNamer/Views/SettingsDialog.xaml.cs:53`、`ReNamerWPF/ReNamer/Views/SettingsDialog.xaml.cs:84`；主流程接入：`ReNamerWPF/ReNamer/Views/MainWindow.xaml.cs:1125` | 重命名成功后按开关自动从列表移除已完成项 |
| `CreateUndoLog` | 已接入（T009） | 定义：`ReNamerWPF/ReNamer/Services/AppSettings.cs:34`；设置读写：`ReNamerWPF/ReNamer/Views/SettingsDialog.xaml.cs:54`、`ReNamerWPF/ReNamer/Views/SettingsDialog.xaml.cs:85`；主流程接入：`ReNamerWPF/ReNamer/Views/MainWindow.xaml.cs:1120`、`ReNamerWPF/ReNamer/Views/MainWindow.xaml.cs:1186` | 重命名成功后按开关自动生成 Undo 日志文件 |
| `ConflictResolution` | 已接入（T010） | 定义：`ReNamerWPF/ReNamer/Services/AppSettings.cs:36`；设置读写：`ReNamerWPF/ReNamer/Views/SettingsDialog.xaml.cs:57`~`ReNamerWPF/ReNamer/Views/SettingsDialog.xaml.cs:90`；主流程接入：`ReNamerWPF/ReNamer/Views/MainWindow.xaml.cs:1116`、`ReNamerWPF/ReNamer/Services/RenameService.cs:65` | 冲突策略支持“跳过 / 自动后缀 / 覆盖已存在目标” |
| `FolderSaveAsDefault` | 部分生效（仅持久化） | 定义：`ReNamerWPF/ReNamer/Services/AppSettings.cs:63`；过滤器弹窗读写：`ReNamerWPF/ReNamer/Views/FiltersDialog.xaml.cs:31`、`ReNamerWPF/ReNamer/Views/FiltersDialog.xaml.cs:45` | 明确“保存为默认”的生效时机并在启动/导入时应用 |

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
- 扩展 `ValidateNewNames`（T012 已完成）：
  - 已补充保留名（CON/PRN/COMx/LPTx）、路径长度、大小写冲突、跨卷预检查。

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
