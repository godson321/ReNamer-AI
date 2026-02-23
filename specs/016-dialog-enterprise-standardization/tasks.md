# 任务清单：对话框与规则面板企业化统一标准

**输入**：`specs/016-dialog-enterprise-standardization/spec.md`、`specs/016-dialog-enterprise-standardization/plan.md`  
**前置条件**：`spec.md`、`plan.md` 已完成

## 任务格式：`[ID] [P?] [US?] 描述`

- `[P]`：可并行执行（不同文件、无直接依赖）
- `[USx]`：对应用户故事（US1=SettingsDialog，US2=TextInputDialog，US3=规则面板全量）

---

## Phase 1：基础规范与样式基线

- [x] T001 [US3] 盘点在用规则面板清单并锁定改造范围（15 个）到 `specs/016-dialog-enterprise-standardization/plan.md`
- [x] T002 [US3] 校验 `ReNamerWPF/ReNamer/Resources/DesignSystemResources.xaml` 企业样式键完整性（卡片、输入、主次按钮）
- [x] T003 [US3] 在 `ReNamerWPF/ReNamer/Resources/Strings.zh-CN.xaml` / `ReNamerWPF/ReNamer/Resources/Strings.en-US.xaml` 核查并补齐新增标题与按钮文案键（本轮基线无缺项）
- [x] T004 [US3] 形成统一视觉契约（标签列 `118`、输入高 `28`、按钮最小 `128x36`、单位列最小 `84`）

---

## Phase 2：US1 - SettingsDialog 企业化改造（P1）

**目标**：`SettingsDialog.xaml` 达到统一骨架与按钮标准

- [x] T005 [US1] 将 `ReNamerWPF/ReNamer/Views/SettingsDialog.xaml` 重构为“顶部说明卡 + 中部内容卡 + 底部操作卡”
- [x] T006 [US1] 在 `ReNamerWPF/ReNamer/Views/SettingsDialog.xaml` 接入 `EnterpriseDialogCardStyle`
- [x] T007 [US1] 在 `ReNamerWPF/ReNamer/Views/SettingsDialog.xaml` 接入 `EnterpriseDialogInputTextBoxStyle` / `EnterpriseDialogInputComboBoxStyle`
- [x] T008 [US1] 在 `ReNamerWPF/ReNamer/Views/SettingsDialog.xaml` 接入 `EnterpriseDialogPrimaryActionButtonStyle` / `EnterpriseDialogSecondaryActionButtonStyle`
- [x] T009 [US1] 在 `ReNamerWPF/ReNamer/Views/SettingsDialog.xaml` 启用 `UseLayoutRounding="True"` 与 `SnapsToDevicePixels="True"`

**独立验收**：SettingsDialog 在中文环境无裁切，布局和按钮与企业标准一致

---

## Phase 3：US2 - TextInputDialog 企业化改造（P1）

**目标**：`TextInputDialog.xaml` 达到统一骨架与按钮标准

- [x] T010 [US2] 将 `ReNamerWPF/ReNamer/Views/TextInputDialog.xaml` 重构为“顶部说明卡 + 中部内容卡 + 底部操作卡”
- [x] T011 [US2] 在 `ReNamerWPF/ReNamer/Views/TextInputDialog.xaml` 接入 `EnterpriseDialogCardStyle`
- [x] T012 [US2] 在 `ReNamerWPF/ReNamer/Views/TextInputDialog.xaml` 接入统一输入和按钮样式
- [x] T013 [US2] 在 `ReNamerWPF/ReNamer/Views/TextInputDialog.xaml` 启用像素对齐并校正中文按钮显示

**独立验收**：TextInputDialog 的输入、提示、按钮区对齐一致，中文文案无裁切

---

## Phase 4：US3 - 规则面板全量统一（P1）

**目标**：15 个规则面板统一骨架、输入、按钮标准

### 4.1 批量骨架接入

- [x] T014 [P] [US3] 改造 `ReNamerWPF/ReNamer/Views/RuleConfigs/CaseConfigPanel.xaml`
- [x] T015 [P] [US3] 改造 `ReNamerWPF/ReNamer/Views/RuleConfigs/CleanUpConfigPanel.xaml`
- [x] T016 [P] [US3] 改造 `ReNamerWPF/ReNamer/Views/RuleConfigs/DeleteConfigPanel.xaml`
- [x] T017 [P] [US3] 改造 `ReNamerWPF/ReNamer/Views/RuleConfigs/InsertConfigPanel.xaml`
- [x] T018 [P] [US3] 改造 `ReNamerWPF/ReNamer/Views/RuleConfigs/JavaScriptConfigPanel.xaml`
- [x] T019 [P] [US3] 改造 `ReNamerWPF/ReNamer/Views/RuleConfigs/MappingConfigPanel.xaml`
- [x] T020 [P] [US3] 改造 `ReNamerWPF/ReNamer/Views/RuleConfigs/PaddingConfigPanel.xaml`
- [x] T021 [P] [US3] 改造 `ReNamerWPF/ReNamer/Views/RuleConfigs/RandomizeConfigPanel.xaml`
- [x] T022 [P] [US3] 改造 `ReNamerWPF/ReNamer/Views/RuleConfigs/RearrangeConfigPanel.xaml`
- [x] T023 [P] [US3] 改造 `ReNamerWPF/ReNamer/Views/RuleConfigs/ReformatDateConfigPanel.xaml`
- [x] T024 [P] [US3] 改造 `ReNamerWPF/ReNamer/Views/RuleConfigs/RegexConfigPanel.xaml`
- [ ] T025 [P] [US3] 改造 `ReNamerWPF/ReNamer/Views/RuleConfigs/ReplaceConfigPanel.xaml`
- [ ] T026 [P] [US3] 改造 `ReNamerWPF/ReNamer/Views/RuleConfigs/SerializeConfigPanel.xaml`
- [ ] T027 [P] [US3] 改造 `ReNamerWPF/ReNamer/Views/RuleConfigs/TransliterateConfigPanel.xaml`
- [ ] T028 [P] [US3] 改造 `ReNamerWPF/ReNamer/Views/RuleConfigs/UserInputConfigPanel.xaml`
- [ ] T028A [US3] 统一规则面板改造约束：不新增顶部说明卡，保持紧凑内容布局

### 4.2 规则统一校正

- [ ] T029 [US3] 全量规则面板统一标签列宽、输入高度和分组留白
- [ ] T030 [US3] 全量规则面板统一主次按钮样式（中文优先尺寸）
- [ ] T031 [US3] 全量规则面板检查 `DynamicResource` 使用，移除可能导致主题切换异常的 `StaticResource` 误用
- [ ] T032 [US3] 对含“数值+单位”行的规则面板统一单位列最小宽度（`>=84`）

**独立验收**：15 个规则面板视觉规范一致，中文无裁切，主题切换稳定

---

## Phase 5：验证与收口

- [ ] T033 [US1] 执行构建验证：`dotnet build ReNamerWPF/ReNamer/ReNamer.csproj -c Debug -v minimal`
- [ ] T034 [US1] 执行启动冒烟：`dotnet run --project ReNamerWPF/ReNamer/ReNamer.csproj -c Debug`
- [ ] T035 [US3] 人工回归：SettingsDialog（中文、缩放、主题切换）
- [ ] T036 [US3] 人工回归：TextInputDialog（中文、缩放、主题切换）
- [ ] T037 [US3] 人工回归：15 个规则面板（对齐、按钮、单位下拉、防遮挡）
- [ ] T038 [US3] 在 `specs/016-dialog-enterprise-standardization/plan.md` 记录验证结果与风险收口
- [ ] T039 [US3] 最后收口：将相关 XAML 中可复用的硬编码尺寸提取到 `DesignSystemResources.xaml` 样式资源（保持行为不变）

---

## 依赖与执行顺序

- Phase 1 完成后执行 Phase 2/3/4
- Phase 2 与 Phase 3 可并行
- Phase 4 可按面板并行推进（T014~T028 标记为 `[P]`）
- Phase 5 必须在 2/3/4 全部完成后执行
- T039 作为最终收口项，放在 Phase 5 末尾执行
- 若遇到 `obj` 文件锁，先解除锁定再执行 T033/T034
