# Tasks: 规则配置面板 UI 重设计
**Input**: 设计文档来自 `specs/014-rule-panel-redesign/`
**Prerequisites**: `plan.md`（必需）, `spec.md`（必需）, `research.md`, `data-model.md`, `contracts/`, `quickstart.md`

**Tests**: 本特性以现有回归测试 + 手动 UI 验证为主，不新增单元测试文件。
**Organization**: 任务按 User Story 分组，并按“规则面板一个个拆”的顺序执行。

## Format: `[ID] [P?] [Story] Description`
- **[P]**: 可并行（不同文件、无阻塞依赖）
- **[Story]**: 对应用户故事（US1~US6）
- 每条任务都包含明确文件路径

## Phase 1: Setup（共享基础设施）
**Purpose**: 建立后续所有面板改造依赖的通用能力

- [X] T001 新增 Placeholder 附加属性实现于 `ReNamerWPF/ReNamer/Helpers/PlaceholderBehavior.cs`
- [X] T002 在 `ReNamerWPF/ReNamer/Resources/DesignSystemResources.xaml` 增加 `SectionHeaderStyle`、`LightGroupBoxStyle`、`ReadOnlyTextBoxBrush`
- [X] T003 在 `ReNamerWPF/ReNamer/Resources/Strings.en-US.xaml` 补充 Placeholder 与分组标题资源键（含 Strip 缺失键）
- [X] T004 [P] 在 `ReNamerWPF/ReNamer/Resources/Strings.zh-CN.xaml` 补充 Placeholder 与分组标题资源键（与 T003 对齐）
- [X] T005 在 `ReNamerWPF/ReNamer/Views/RuleConfigs` 目录内所有目标面板 XAML 统一引入 `xmlns:helpers="clr-namespace:ReNamer.Helpers"`

---

## Phase 2: Foundational（阻塞前置）
**Purpose**: 在进入 User Story 之前，先完成跨面板通用规范

**⚠️ CRITICAL**: 本阶段未完成前，不进入任何用户故事实施

- [X] T006 统一 `ReNamerWPF/ReNamer/Views/RuleConfigs/*.xaml` 根容器基线（优先 `Margin="16,12"`，保留必要 MinWidth）
- [ ] T007 统一 `ReNamerWPF/ReNamer/Views/RuleConfigs/*.xaml` 标签列宽与对齐策略（简单面板 80，复杂面板 100；标题右对齐、文本左对齐、多行编辑区内容顶部左对齐；同级标签行必须复用同列骨架，字段标签不得非规范加粗；同类输入列宽必须一致，不允许 68/120 等历史混用）
- [ ] T008 统一 `ReNamerWPF/ReNamer/Views/RuleConfigs/*.xaml` 行间距策略（8）和分组间距策略（16）
- [ ] T009 在 `ReNamerWPF/ReNamer/Views/RuleConfigs/*.xaml` 将 Hint 文本样式统一到 `DialogHintText`
- [X] T010 在 `ReNamerWPF/ReNamer/Views/RuleConfigs/RuleConfigHelper.cs` 去除硬编码菜单文案并改为资源读取

**Checkpoint**: 基础规范可复用，进入逐面板拆解阶段

---

## Phase 3: User Story 1 - 面板视觉一致性（Priority: P1）🎯 MVP
**Goal**: 各规则面板切换时布局节奏一致（Margin/标签宽度/行距/最小宽度）
**Independent Test**: 在 AddRuleDialog 中逐一切换所有面板，观察布局一致性与拉伸行为

### Implementation for User Story 1（按面板顺序逐个拆）
- [X] T011 [US1] 统一 Replace 面板布局于 `ReNamerWPF/ReNamer/Views/RuleConfigs/ReplaceConfigPanel.xaml`
- [X] T012 [US1] 统一 Insert 面板布局于 `ReNamerWPF/ReNamer/Views/RuleConfigs/InsertConfigPanel.xaml`
- [X] T013 [US1] 统一 Delete 面板布局于 `ReNamerWPF/ReNamer/Views/RuleConfigs/DeleteConfigPanel.xaml`
- [X] T014 [US1] 统一 Case 面板布局于 `ReNamerWPF/ReNamer/Views/RuleConfigs/CaseConfigPanel.xaml`
- [X] T015 [US1] 统一 Serialize 面板布局于 `ReNamerWPF/ReNamer/Views/RuleConfigs/SerializeConfigPanel.xaml`
- [X] T016 [US1] 统一 Extension 面板布局于 `ReNamerWPF/ReNamer/Views/RuleConfigs/ExtensionConfigPanel.xaml`
- [X] T017 [US1] 统一 CleanUp 面板布局于 `ReNamerWPF/ReNamer/Views/RuleConfigs/CleanUpConfigPanel.xaml`
- [X] T018 [US1] 统一 Strip 面板布局于 `ReNamerWPF/ReNamer/Views/RuleConfigs/StripConfigPanel.xaml`
- [ ] T019 [US1] 统一 Regex 面板布局于 `ReNamerWPF/ReNamer/Views/RuleConfigs/RegexConfigPanel.xaml`
- [ ] T020 [US1] 统一 Rearrange 面板布局于 `ReNamerWPF/ReNamer/Views/RuleConfigs/RearrangeConfigPanel.xaml`
- [ ] T021 [US1] 统一 Remove 面板布局于 `ReNamerWPF/ReNamer/Views/RuleConfigs/RemoveConfigPanel.xaml`
- [ ] T022 [US1] 统一 Randomize 面板布局于 `ReNamerWPF/ReNamer/Views/RuleConfigs/RandomizeConfigPanel.xaml`
- [ ] T023 [US1] 统一 Padding 面板布局于 `ReNamerWPF/ReNamer/Views/RuleConfigs/PaddingConfigPanel.xaml`
- [X] T024 [US1] 统一 Transliterate 面板布局于 `ReNamerWPF/ReNamer/Views/RuleConfigs/TransliterateConfigPanel.xaml`
- [X] T025 [US1] 统一 ReformatDate 面板布局于 `ReNamerWPF/ReNamer/Views/RuleConfigs/ReformatDateConfigPanel.xaml`
- [X] T026 [US1] 统一 UserInput 面板布局于 `ReNamerWPF/ReNamer/Views/RuleConfigs/UserInputConfigPanel.xaml`
- [ ] T027 [US1] 统一 Mapping 面板布局于 `ReNamerWPF/ReNamer/Views/RuleConfigs/MappingConfigPanel.xaml`
- [ ] T028 [US1] 统一 PascalScript 面板布局于 `ReNamerWPF/ReNamer/Views/RuleConfigs/PascalScriptConfigPanel.xaml`

**Checkpoint**: US1 完成后，布局一致性可独立验收

---

## Phase 4: User Story 2 - 输入引导与 Placeholder（Priority: P1）
**Goal**: 所有需要输入的文本控件提供明确输入提示
**Independent Test**: 逐面板检查 Placeholder 显示/隐藏行为（空、输入、失焦）

### Implementation for User Story 2（按面板顺序逐个拆）
- [X] T029 [US2] 为 Replace 输入框增加 Placeholder 于 `ReNamerWPF/ReNamer/Views/RuleConfigs/ReplaceConfigPanel.xaml`
- [X] T030 [US2] 为 Insert 输入框增加 Placeholder 于 `ReNamerWPF/ReNamer/Views/RuleConfigs/InsertConfigPanel.xaml`
- [X] T031 [US2] 为 Delete 分隔符输入框增加 Placeholder 于 `ReNamerWPF/ReNamer/Views/RuleConfigs/DeleteConfigPanel.xaml`
- [X] T032 [US2] 为 Case ForceCase 输入框增加 Placeholder 于 `ReNamerWPF/ReNamer/Views/RuleConfigs/CaseConfigPanel.xaml`
- [X] T033 [US2] 为 Serialize 自定义符号输入框增加 Placeholder 于 `ReNamerWPF/ReNamer/Views/RuleConfigs/SerializeConfigPanel.xaml`
- [X] T034 [US2] 为 Extension 扩展名输入框增加 Placeholder 于 `ReNamerWPF/ReNamer/Views/RuleConfigs/ExtensionConfigPanel.xaml`
- [X] T035 [US2] 为 CleanUp（如有文本输入）补充输入提示于 `ReNamerWPF/ReNamer/Views/RuleConfigs/CleanUpConfigPanel.xaml`（无文本输入控件，N/A）
- [X] T036 [US2] 为 Strip 用户自定义/Unicode 范围输入框增加 Placeholder 于 `ReNamerWPF/ReNamer/Views/RuleConfigs/StripConfigPanel.xaml`
- [X] T037 [US2] 为 Regex Expression/Replace 输入框增加 Placeholder 于 `ReNamerWPF/ReNamer/Views/RuleConfigs/RegexConfigPanel.xaml`
- [X] T038 [US2] 为 Rearrange Delimiters/NewPattern 输入框增加 Placeholder 于 `ReNamerWPF/ReNamer/Views/RuleConfigs/RearrangeConfigPanel.xaml`
- [X] T039 [US2] 为 Remove Pattern 输入框增加 Placeholder 于 `ReNamerWPF/ReNamer/Views/RuleConfigs/RemoveConfigPanel.xaml`
- [X] T040 [US2] 为 Randomize 用户自定义字符输入框增加 Placeholder 于 `ReNamerWPF/ReNamer/Views/RuleConfigs/RandomizeConfigPanel.xaml`
- [X] T041 [US2] 为 Padding 填充字符输入框增加 Placeholder 于 `ReNamerWPF/ReNamer/Views/RuleConfigs/PaddingConfigPanel.xaml`
- [X] T042 [US2] 为 Transliterate 字母表输入框增加 Placeholder 于 `ReNamerWPF/ReNamer/Views/RuleConfigs/TransliterateConfigPanel.xaml`
- [X] T043 [US2] 为 ReformatDate 可编辑格式输入控件增加 Placeholder 于 `ReNamerWPF/ReNamer/Views/RuleConfigs/ReformatDateConfigPanel.xaml`
- [X] T044 [US2] 为 UserInput 多行输入框增加 Placeholder 于 `ReNamerWPF/ReNamer/Views/RuleConfigs/UserInputConfigPanel.xaml`
- [X] T045 [US2] 为 PascalScript 脚本编辑框增加 Placeholder 于 `ReNamerWPF/ReNamer/Views/RuleConfigs/PascalScriptConfigPanel.xaml`

**Checkpoint**: US2 完成后，输入引导可独立验收

---

## Phase 5: User Story 3 - 视觉分组与层次（Priority: P2）
**Goal**: 复杂配置区域具备清晰分组与定位能力
**Independent Test**: 各面板能在 3 秒内定位目标设置区域

### Implementation for User Story 3（按重点面板逐个拆）
- [X] T046 [US3] 为 Replace 的 Occurrences/Flags 添加分组标题于 `ReNamerWPF/ReNamer/Views/RuleConfigs/ReplaceConfigPanel.xaml`
- [X] T047 [US3] 为 Serialize 左列拆分"编号参数/重置条件/格式选项"于 `ReNamerWPF/ReNamer/Views/RuleConfigs/SerializeConfigPanel.xaml`
- [X] T048 [US3] 为 Extension 的 New Extension / Remove Duplicate 区域建立分组于 `ReNamerWPF/ReNamer/Views/RuleConfigs/ExtensionConfigPanel.xaml`
- [X] T049 [US3] 为 CleanUp 的散装 CheckBox 重组为"空格处理/Unicode/其他"于 `ReNamerWPF/ReNamer/Views/RuleConfigs/CleanUpConfigPanel.xaml`
- [ ] T050 [US3] 为 Case 的 ForceCase 与 Extension 选项建立独立分组于 `ReNamerWPF/ReNamer/Views/RuleConfigs/CaseConfigPanel.xaml`
- [X] T051 [US3] 为 Transliterate 右侧拆分“方向/说明/选项”分区于 `ReNamerWPF/ReNamer/Views/RuleConfigs/TransliterateConfigPanel.xaml`
- [ ] T052 [US3] 为 UserInput 选项区重构为清晰分组布局于 `ReNamerWPF/ReNamer/Views/RuleConfigs/UserInputConfigPanel.xaml`
- [ ] T053 [US3] 为 Mapping 选项区改为固定分组布局并增强空态引导于 `ReNamerWPF/ReNamer/Views/RuleConfigs/MappingConfigPanel.xaml`

**Checkpoint**: US3 完成后，视觉层次可独立验收

---

## Phase 6: User Story 4 - 本地化完整性（Priority: P2）
**Goal**: 面板用户可见文本 100% 来自资源文件
**Independent Test**: 中英文切换后无硬编码残留

### Implementation for User Story 4
- [ ] T054 [US4] 将 Strip 面板全部硬编码文本替换为 DynamicResource 于 `ReNamerWPF/ReNamer/Views/RuleConfigs/StripConfigPanel.xaml`
- [ ] T055 [US4] 将 Rearrange 面板硬编码 Tooltip 替换为资源键于 `ReNamerWPF/ReNamer/Views/RuleConfigs/RearrangeConfigPanel.xaml`
- [ ] T056 [US4] 将 Regex/其他面板按钮说明文案统一资源化于 `ReNamerWPF/ReNamer/Views/RuleConfigs/RegexConfigPanel.xaml`, `ReNamerWPF/ReNamer/Views/RuleConfigs/*.xaml`
- [ ] T057 [US4] 将 RuleConfigHelper 菜单分组与说明文本改为资源读取于 `ReNamerWPF/ReNamer/Views/RuleConfigs/RuleConfigHelper.cs`
- [ ] T058 [US4] 补齐并校对中英文资源映射于 `ReNamerWPF/ReNamer/Resources/Strings.en-US.xaml`, `ReNamerWPF/ReNamer/Resources/Strings.zh-CN.xaml`
- [ ] T059 [US4] 全量扫描并清理面板硬编码用户文本于 `ReNamerWPF/ReNamer/Views/RuleConfigs/*.xaml`

**Checkpoint**: US4 完成后，本地化完整性可独立验收

---

## Phase 7: User Story 5 - 主题兼容性（Priority: P2）
**Goal**: 亮/暗主题下可读性一致，无硬编码颜色
**Independent Test**: 切换主题后逐面板检查前景/背景/边框可读性

### Implementation for User Story 5
- [ ] T060 [US5] 将 Strip 只读 TextBox 背景改为主题 Brush 于 `ReNamerWPF/ReNamer/Views/RuleConfigs/StripConfigPanel.xaml`
- [ ] T061 [US5] 将 Rearrange Hint 颜色改为主题 Brush 于 `ReNamerWPF/ReNamer/Views/RuleConfigs/RearrangeConfigPanel.xaml`
- [X] T062 [US5] 将 Transliterate Hint 颜色改为主题 Brush 于 `ReNamerWPF/ReNamer/Views/RuleConfigs/TransliterateConfigPanel.xaml`
- [ ] T063 [US5] 将其余面板 Hint/说明色统一为 `DialogHintText` 于 `ReNamerWPF/ReNamer/Views/RuleConfigs/*.xaml`
- [ ] T064 [US5] 全量扫描并清理面板硬编码颜色于 `ReNamerWPF/ReNamer/Views/RuleConfigs/*.xaml`

**Checkpoint**: US5 完成后，主题兼容性可独立验收

---

## Phase 8: User Story 6 - 清除布局 Hack（Priority: P3）
**Goal**: 去除空 TextBlock 占位与不可预测 DockPanel 布局
**Independent Test**: 搜索 `Text=""` 为 0；关键行在窄窗和拉伸场景表现稳定

### Implementation for User Story 6（按面板逐个拆）
- [X] T065 [US6] 移除 Extension 面板空 TextBlock 占位并改为 Grid/Margin 于 `ReNamerWPF/ReNamer/Views/RuleConfigs/ExtensionConfigPanel.xaml`
- [X] T066 [US6] 移除 Case 面板右侧空 TextBlock 占位并重构对齐于 `ReNamerWPF/ReNamer/Views/RuleConfigs/CaseConfigPanel.xaml`
- [X] T067 [US6] 将 Insert 的 AfterText/BeforeText 从 DockPanel 改为 Grid 于 `ReNamerWPF/ReNamer/Views/RuleConfigs/InsertConfigPanel.xaml`
- [ ] T068 [US6] 将 Rearrange 的关键输入行从 DockPanel 改为 Grid 于 `ReNamerWPF/ReNamer/Views/RuleConfigs/RearrangeConfigPanel.xaml`
- [ ] T069 [US6] 将 ReformatDate 的 AdjustBy 行改为三列 Grid 于 `ReNamerWPF/ReNamer/Views/RuleConfigs/ReformatDateConfigPanel.xaml`
- [ ] T070 [US6] 将 Randomize Length 行改为 Grid（NUD 靠左）于 `ReNamerWPF/ReNamer/Views/RuleConfigs/RandomizeConfigPanel.xaml`
- [ ] T071 [US6] 将 Mapping 选项区从 WrapPanel 改为固定 Grid 布局于 `ReNamerWPF/ReNamer/Views/RuleConfigs/MappingConfigPanel.xaml`
- [ ] T072 [US6] 将 PascalScript 计数器参数行改为 Grid 对齐于 `ReNamerWPF/ReNamer/Views/RuleConfigs/PascalScriptConfigPanel.xaml`

**Checkpoint**: US6 完成后，布局质量可独立验收

---

## Phase 9: Polish & Cross-Cutting
**Purpose**: 统一收口、回归验证与交付检查

- [ ] T073 执行静态扫描（硬编码文本/硬编码颜色/空 TextBlock/输入框尾部按钮文本符号/同级标签行骨架一致性/字段标签非规范加粗/同类输入列宽一致性）并修复问题于 `ReNamerWPF/ReNamer/Views/RuleConfigs/*.xaml`
- [ ] T074 运行构建验证 `dotnet build ReNamerWPF/ReNamer.sln`
- [ ] T075 运行回归测试 `dotnet test ReNamerWPF/ReNamer.sln`
- [ ] T076 依据 `specs/014-rule-panel-redesign/quickstart.md` 完成亮暗主题 + 中英文 + 窄窗手动验收
- [ ] T077 对照 `specs/014-rule-panel-redesign/spec.md` 的 SC-001~SC-007 完成最终核对

---

## Phase 10: 多语言专项检查（逐面板）
**Purpose**: 防止“英文散落未翻译”回归，执行面板级中英文核查

- [ ] T078 按面板顺序对 Replace~PascalScript 逐个执行中英文切换检查并记录修复点于 `ReNamerWPF/ReNamer/Views/RuleConfigs/*.xaml`
- [ ] T079 修复 `ReNamerWPF/ReNamer/Views/RuleConfigs/RuleConfigHelper.cs` 内部菜单分组/描述的英文硬编码
- [ ] T080 修复 `ReNamerWPF/ReNamer/Views/RuleConfigs/*.xaml.cs` 中用户可见英文提示（如 MessageBox/Tooltip 文案）
- [ ] T081 补齐 `ReNamerWPF/ReNamer/Resources/Strings.en-US.xaml` 缺失键并去重命名冲突
- [ ] T082 [P] 补齐 `ReNamerWPF/ReNamer/Resources/Strings.zh-CN.xaml` 对应翻译键并校对术语一致性
- [ ] T083 扫描并清理 AddRule 相关 UI 残余英文于 `ReNamerWPF/ReNamer/Views/AddRuleDialog.xaml`
- [ ] T084 扫描并清理规则面板相关对话框残余英文于 `ReNamerWPF/ReNamer/Views/FiltersDialog.xaml`, `ReNamerWPF/ReNamer/Views/SettingsDialog.xaml`
- [ ] T085 复测“中文环境不可见英文残留、英文环境不可见中文残留”全链路切换场景于 `specs/014-rule-panel-redesign/quickstart.md`
- [X] T086 统一弹出窗口默认打开位置为屏幕中央于 `ReNamerWPF/ReNamer/Views/AddRuleDialog.xaml`, `ReNamerWPF/ReNamer/Views/FiltersDialog.xaml`, `ReNamerWPF/ReNamer/Views/SettingsDialog.xaml`

---

## Dependencies & Execution Order
### Phase Dependencies
- Phase 1 -> Phase 2 -> Phase 3/4/5/6/7/8 -> Phase 9 -> Phase 10
- 建议执行顺序：US1 -> US2 -> US3 -> US4 -> US5 -> US6（降低重构冲突）

### Panel Execution Order（逐个面板）
1. Replace
2. Insert
3. Delete
4. Case
5. Serialize
6. Extension
7. CleanUp
8. Strip
9. Regex
10. Rearrange
11. Remove
12. Randomize
13. Padding
14. Transliterate
15. ReformatDate
16. UserInput
17. Mapping
18. PascalScript

### Within Each Panel
- 先布局一致性（US1）
- 再输入引导（US2）
- 再分组层次（US3）
- 再本地化（US4）
- 再主题兼容（US5）
- 最后清理 hack（US6）

---

## Parallel Opportunities
- T003 与 T004 可并行（中英文资源文件分离）
- 不同面板之间可并行，但当前需求建议按“逐面板顺序”串行推进
- Phase 9 的构建与测试可在修复完成后连续执行

---

## Implementation Strategy
### MVP First（先交付高价值）
1. 完成 Phase 1~2
2. 完成 US1 + US2 的前 5 个高频面板（Replace/Insert/Delete/Case/Serialize）
3. 先做一次可视验收和回归测试

### Incremental Delivery
1. 按面板顺序逐个完成 US1~US6
2. 每完成 3~5 个面板执行一次构建 + 快速手动检查
3. 全量完成后执行 Phase 9 一次性收口

### Notes
- 保持规则行为不变，只改 UI 呈现层
- 若同一文件在多个故事中反复修改，优先合并同一面板的任务提交，减少冲突
- 输入框尾部功能按钮（如分隔符/MetaTag）统一采用单图标按钮，不使用 `|` 或空白字符作为可见内容，并保留 Tooltip 语义提示
- 同一面板内同级标签行（如 Insert/Where）必须复用同列骨架，禁止通过额外 Margin 或字体加粗修“视觉对齐”；未满足该项不得勾选对应面板任务完成
- 同一面板内同类输入控件（NumericUpDown/TextBox）右侧输入列宽必须一致（例如 Delete 的 From/Until），禁止保留历史混用宽度；未满足该项不得勾选对应面板任务完成
