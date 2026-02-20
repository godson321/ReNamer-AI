# 实施任务：015 WPF 项目全量评审优化落地

**输入**：
- `specs/015-wpf-review-optimization/spec.md`
- `specs/015-wpf-review-optimization/plan.md`
- `specs/015-wpf-review-optimization/review-result.md`

**目标**：按优先级落地评审结论，先修 P0/P1，再做交互与视觉一致性优化。

---

## 阶段 1：P0/P1 核心修复（先稳）

- [x] T001 [US1] 修复“回收站删除失败提示条件”逻辑错误于 `ReNamerWPF/ReNamer/Views/MainWindow.xaml.cs`
- [x] T002 [US1] 在重命名流程接入 `ConfirmRename` 开关（关闭时跳过确认弹窗）于 `ReNamerWPF/ReNamer/Views/MainWindow.xaml.cs`
- [x] T003 [US1] 恢复窗口位置（使用 `WindowLeft/Top`）并增加越界保护于 `ReNamerWPF/ReNamer/Views/MainWindow.xaml.cs`
- [x] T004 [US1] 为“添加目录”总入口增加异常隔离与受限目录容错于 `ReNamerWPF/ReNamer/Views/MainWindow.xaml.cs`
- [x] T005 [US1] 手工回归验证（删除到回收站/重命名确认/窗口位置/添加目录）并记录结果于 `specs/015-wpf-review-optimization/review-result.md`

**检查点**：P0/P1 问题可复现项全部关闭，主流程无明显行为回归。

---

## 阶段 2：配置项落地（功能完整性）

- [x] T006 [US2] 梳理 `AppSettings` 中“可配不可用”项并建立映射清单于 `specs/015-wpf-review-optimization/review-result.md`
- [x] T007 [US2] 接入 `PreviewOnFileAdd` 与 `HighlightChanges` 于 `ReNamerWPF/ReNamer/Views/MainWindow.xaml.cs`
- [x] T008 [US2] 接入 `AutoValidate`、`WarnInvalidChars`、`WarnLongPaths` 于 `ReNamerWPF/ReNamer/Views/MainWindow.xaml.cs` 与 `ReNamerWPF/ReNamer/Services/RenameService.cs`
- [x] T009 [US2] 接入 `AutoRemoveRenamed` 与 `CreateUndoLog` 于 `ReNamerWPF/ReNamer/Views/MainWindow.xaml.cs`
- [x] T010 [US2] 接入 `ConflictResolution` 策略于 `ReNamerWPF/ReNamer/Services/RenameService.cs`
- [x] T011 [US2] 优化 AddFolders 默认导入策略（首次默认导入文件）于 `ReNamerWPF/ReNamer/Services/AppSettings.cs` 与 `ReNamerWPF/ReNamer/Views/MainWindow.xaml.cs`
- [x] T012 [US2] 扩展 `ValidateNewNames`（保留名/路径长度/大小写冲突/跨卷预检查）于 `ReNamerWPF/ReNamer/Services/RenameService.cs`

**检查点**：设置项和主流程行为一致，不再出现“可配不可用”。

---

## 阶段 3：可用性与稳定性强化

- [x] T013 [US3] 将“实时预览计算”改为 `async + Task.Run + CancellationToken`（替代手动预览流程）于 `ReNamerWPF/ReNamer/Views/MainWindow.xaml.cs`
- [x] T014 [US3] 将“重命名”流程改为 `async + Task.Run + CancellationToken` 于 `ReNamerWPF/ReNamer/Views/MainWindow.xaml.cs`
- [x] T015 [US3] 在 `RenameService` 增加取消感知与中断返回于 `ReNamerWPF/ReNamer/Services/RenameService.cs`
- [x] T016 [US3] 替换空 `catch`：统一记录日志并输出用户可理解提示于 `ReNamerWPF/ReNamer/Services/AppSettings.cs` 与 `ReNamerWPF/ReNamer/Views/MainWindow.xaml.cs`
- [x] T017 [US3] 回收站删除失败改为按文件汇总原因并展示于 `ReNamerWPF/ReNamer/Views/MainWindow.xaml.cs`
- [x] T037 [US3] 首页列表大数据性能优化：启用 ListView 虚拟化并将预览/重命名进度上报改为节流更新于 `ReNamerWPF/ReNamer/Views/MainWindow.xaml` 与 `ReNamerWPF/ReNamer/Views/MainWindow.xaml.cs`
- [x] T038 [US3] 降低滚动 IO 抖动：`ExifDateDisplay` 改为异步懒加载，避免滚动阶段同步读取 EXIF 元数据于 `ReNamerWPF/ReNamer/Models/RenFile.cs`
- [x] T039 [US3] 首页文件表格重构为 `DataGrid`（行列虚拟化 + 可回收容器），并同步迁移排序、列显示菜单、列宽持久化逻辑于 `ReNamerWPF/ReNamer/Views/MainWindow.xaml` 与 `ReNamerWPF/ReNamer/Views/MainWindow.xaml.cs`

**检查点**：大批量操作时 UI 可响应，异常可观测、可定位。

---

## 阶段 4：交互易用性优化

- [x] T018 [US4] 移除 WPF 项目全部快捷键操作（主窗口/规则列表/文件列表）并清理菜单与资源中的快捷键提示于 `ReNamerWPF/ReNamer/Views/MainWindow.xaml.cs`、`ReNamerWPF/ReNamer/Views/MainWindow.xaml`、`ReNamerWPF/ReNamer/Resources/Strings.zh-CN.xaml`、`ReNamerWPF/ReNamer/Resources/Strings.en-US.xaml`
- [x] T019 [US4] 设计并实现可复用输入对话框组件（校验/Enter/Esc/资源化）于 `ReNamerWPF/ReNamer/Views/`
- [x] T020 [US4] 替换临时输入窗口调用点于 `ReNamerWPF/ReNamer/Views/MainWindow.xaml.cs`
- [x] T021 [US4] 状态栏文本全资源化（移除硬编码 `Files: ...`）于 `ReNamerWPF/ReNamer/Resources/Strings.zh-CN.xaml`、`ReNamerWPF/ReNamer/Resources/Strings.en-US.xaml`、`ReNamerWPF/ReNamer/Views/MainWindow.xaml.cs`
- [x] T032 [US4] 增加“实时预览”逻辑：规则变更后立即在表格刷新预览结果，并按规则顺序输出最终结果（例如 1→2→3 执行后的最终文件名）于 `ReNamerWPF/ReNamer/Views/MainWindow.xaml.cs` 与 `ReNamerWPF/ReNamer/Services/RenameService.cs`
- [x] T033 [US4] 预览差异高亮：新旧文件名不一致时使用差异颜色，一致时保持默认颜色于 `ReNamerWPF/ReNamer/Views/MainWindow.xaml` 与 `ReNamerWPF/ReNamer/Resources/DesignSystemResources.xaml`
- [x] T034 [US4] 删除顶部“预览”按钮及其相关命令入口（仅保留实时预览）于 `ReNamerWPF/ReNamer/Views/MainWindow.xaml` 与 `ReNamerWPF/ReNamer/Views/MainWindow.xaml.cs`
- [x] T035 [US4] 文件表格排序方向可视化：点击表头排序时显示升序/降序箭头标识于 `ReNamerWPF/ReNamer/Views/MainWindow.xaml` 与 `ReNamerWPF/ReNamer/Views/MainWindow.xaml.cs`
- [x] T036 [US4] 文件表格增加删除快捷键（Delete 删除选中项）并补充对应提示文案于 `ReNamerWPF/ReNamer/Views/MainWindow.xaml`、`ReNamerWPF/ReNamer/Views/MainWindow.xaml.cs`、`ReNamerWPF/ReNamer/Resources/Strings.zh-CN.xaml`、`ReNamerWPF/ReNamer/Resources/Strings.en-US.xaml`
- [x] T041 [US4] 文件表格第一列增加“全选/全不选”复选框（批量切换 IsMarked）于 `ReNamerWPF/ReNamer/Views/MainWindow.xaml` 与 `ReNamerWPF/ReNamer/Views/MainWindow.xaml.cs`

**检查点**：快捷键不易误触，对话框一致，语言切换无混杂；规则调整后可实时看到“最终顺序执行结果”；排序方向可直观看到升降序；文件列表删除可通过 Delete 快捷操作。

---

## 阶段 5：视觉一致性与主题治理

- [x] T022 [US5] 收敛主题颜色来源（Design Tokens 单一源）于 `ReNamerWPF/ReNamer/App.xaml`、`ReNamerWPF/ReNamer/Resources/DesignSystemResources.xaml`、`ReNamerWPF/ReNamer/Themes/Theme.xaml`
- [x] T023 [US5] 补齐无边框窗口的系统行为细节（高对比度/系统菜单/边缘交互）于 `ReNamerWPF/ReNamer/Views/MainWindow.xaml` 与 `ReNamerWPF/ReNamer/Views/MainWindow.xaml.cs`
- [x] T024 [US5] 调整文件列表行高与留白（高 DPI 友好）于 `ReNamerWPF/ReNamer/Views/MainWindow.xaml`
- [x] T025 [US5] 列宽持久化改造（保存每列最后宽度，避免重置 120）于 `ReNamerWPF/ReNamer/Views/MainWindow.xaml.cs` 与 `ReNamerWPF/ReNamer/Services/AppSettings.cs`
- [x] T026 [US5] 统一文字前景语义资源键（`TextPrimaryBrush`/`TextBrush`）于 `ReNamerWPF/ReNamer/Views/AddRuleDialog.xaml` 与主题资源文件
- [x] T040 [US5] 首页文件表格企业级视觉改造（DataGrid 统一头部/行/单元格样式，强化层级与选中态）于 `ReNamerWPF/ReNamer/Views/MainWindow.xaml`

**检查点**：视觉风格一致、主题语义统一、列表可读性提升。

---

## 阶段 6：验证与交付

- [x] T027 运行构建验证：`dotnet build ReNamerWPF/ReNamer/ReNamer.sln`
- [x] T028 运行回归测试：`dotnet test ReNamerWPF/ReNamer/ReNamer.sln`
- [x] T029 按 `ReNamerWPF/FEATURE_TESTING_GUIDE.md` 执行人工回归并记录差异
- [x] T030 更新评审结果与完成状态于 `specs/015-wpf-review-optimization/review-result.md`
- [x] T031 输出变更摘要与剩余风险清单于 `specs/015-wpf-review-optimization/plan.md`

---

## 依赖关系与执行顺序

- 阶段 1 是阻塞项，必须先完成。
- 阶段 2 依赖阶段 1；阶段 3 可与阶段 2 部分并行（不改同一函数时）。
- 阶段 4、阶段 5 在阶段 2/3 完成后执行，避免交叉回归。
- 阶段 6 最后执行，作为交付门禁。

## 并行机会

- [P] 配置项接入（T007~T011）可按模块并行。
- [P] 稳定性改造（T013~T017）可按流程拆分并行。
- [P] 视觉治理（T022~T026）可按 XAML/资源文件并行。

## 说明

- 本任务单按“先稳后优”的两阶段策略执行。
- 任何涉及行为变化的改动，需同步补充人工回归记录。
- 若发现新 P0 问题，可插队处理并在本文件追加任务编号。
