# Feature Specification: 规则配置面板 UI 重设计

**Feature Branch**: `014-rule-panel-redesign`  
**Created**: 2026-02-14  
**Status**: Draft  
**Input**: 优化全部 17 个规则配置面板的可用性、易用性和美观度

## 概述

当前 17 个规则配置面板（Rule Config Panels）虽然功能完整，但在布局一致性、视觉层次、交互引导和本地化方面存在系统性问题。本次重设计的目标是在不改变功能逻辑的前提下，统一面板设计语言，提升用户操作效率和视觉体验。

### 当前问题摘要

1. **布局不一致**: 面板 Margin（16,12 / 16,8 / 8）、标签列宽（72 / 80 / 100 / 140px）、行间距各不相同
2. **缺乏视觉层次**: 主要输入区和次要选项无权重区分，无分组标题
3. **输入引导缺失**: TextBox 无 Placeholder 提示，用户不知道应输入何种格式
4. **本地化遗漏**: Strip 面板全部文本为硬编码英文
5. **主题兼容性差**: 存在硬编码颜色值（`Foreground="Gray"`、`Background="#F0F0F0"`），暗色模式下显示异常
6. **布局 Hack**: Extension 面板使用空 TextBlock 占位，代码难以维护

## User Scenarios & Testing

### User Story 1 - 面板视觉一致性 (Priority: P1)

用户打开 AddRuleDialog 后，在 17 种规则之间切换时，每个面板应拥有统一的视觉节奏和布局结构，不会因切换规则类型而产生"换了一个应用"的割裂感。

**Why this priority**: 一致性是用户信任感和学习效率的基础。不一致的面板让用户每次都需重新理解界面布局。

**Independent Test**: 逐一切换 17 种规则，检查 Margin、标签宽度、行间距、分组样式是否统一。

**Acceptance Scenarios**:

1. **Given** 用户打开 AddRuleDialog, **When** 在规则列表中依次点击每种规则, **Then** 所有面板的外边距、标签列宽、行间距、分组样式保持一致
2. **Given** 用户使用暗色主题, **When** 查看任意面板, **Then** 无硬编码颜色导致的文字/背景不可见问题
3. **Given** 用户调整对话框大小, **When** 窗口变宽或变窄, **Then** 面板内控件自适应拉伸，无截断或大片空白

---

### User Story 2 - 输入引导与 Placeholder (Priority: P1)

用户初次使用某规则时，TextBox 应通过 Placeholder（水印）提示输入格式或示例值，减少试错成本。

**Why this priority**: 空白输入框是新用户最大的认知障碍。Placeholder 是最低成本的引导方式。

**Independent Test**: 逐一打开每个面板，检查所有 TextBox 是否显示有意义的 Placeholder 文字。

**Acceptance Scenarios**:

1. **Given** 用户打开 Replace 面板, **When** 查看 Find 和 Replace 输入框, **Then** 各显示提示文字（如"输入要查找的文本"、"输入替换文本"）
2. **Given** 用户在输入框中输入内容, **When** 输入框获得焦点且有内容, **Then** Placeholder 消失
3. **Given** 用户清空输入框, **When** 输入框失去焦点, **Then** Placeholder 重新显示

---

### User Story 3 - 视觉分组与层次 (Priority: P2)

每个面板中，功能相关的控件（主输入区、位置选项、高级选项）应有清晰的视觉分组，用户能快速定位需要修改的区域。

**Why this priority**: 降低认知负荷，让用户快速找到目标设置项。

**Independent Test**: 检查每个面板是否通过 section header 或 GroupBox 将控件分组。

**Acceptance Scenarios**:

1. **Given** 用户打开 Serialize 面板（最复杂的面板）, **When** 查看面板, **Then** 能通过分组标题区分"编号参数"、"重置条件"、"格式选项"、"插入位置"四个区域
2. **Given** 用户打开 CleanUp 面板, **When** 查看 CheckBox 列表, **Then** 散装 CheckBox 按功能分组（空格处理 / Unicode / 其他），有标题分隔
3. **Given** 用户打开 Replace 面板, **When** 查看选项区, **Then** Occurrences 和 Flags 两组各有分组标题

---

### User Story 4 - 本地化完整性 (Priority: P2)

所有面板的用户可见文本必须通过 DynamicResource 实现本地化，不存在硬编码的英文字符串。

**Why this priority**: Strip 面板全部文本为硬编码英文，与其他面板不一致，影响中文用户体验。

**Independent Test**: 切换语言至中文后，检查所有面板是否还有英文残留。

**Acceptance Scenarios**:

1. **Given** 用户将语言设为中文, **When** 打开 Strip 面板, **Then** 所有标签、CheckBox 文本、RadioButton 文本均显示中文
2. **Given** 用户将语言设为英文, **When** 打开任意面板, **Then** 所有文本显示英文，无中文残留

---

### User Story 5 - 主题兼容性 (Priority: P2)

面板在亮色和暗色主题下都能正常显示，无不可读的颜色组合。

**Why this priority**: 项目已支持暗色模式，但面板中存在硬编码颜色值，暗色下显示异常。

**Independent Test**: 切换暗色主题后逐一查看所有面板。

**Acceptance Scenarios**:

1. **Given** 用户切换至暗色主题, **When** 打开 Strip 面板, **Then** 只读 TextBox 背景色使用主题 Brush，而非硬编码 `#F0F0F0`
2. **Given** 用户切换至暗色主题, **When** 打开 Rearrange 面板, **Then** Hint 文字使用主题颜色而非硬编码 `Foreground="Gray"`
3. **Given** 用户在暗色/亮色之间切换, **When** 查看任意面板, **Then** 所有文字、背景、边框均可辨认

---

### User Story 6 - 清除布局 Hack (Priority: P3)

消除面板中使用空 TextBlock 占位、DockPanel 导致控件宽度不可预测等 hack 写法，使用正确的布局方式。

**Why this priority**: 代码质量问题，影响可维护性和未来扩展。

**Independent Test**: 搜索面板 XAML 中的 `Text=""` 占位和 DockPanel 宽度问题，确认已全部替换。

**Acceptance Scenarios**:

1. **Given** Extension 面板 XAML, **When** 检查代码, **Then** 不存在 `<TextBlock Text="" Width="72"/>` 占位写法
2. **Given** Insert 面板中的 AfterText/BeforeText 行, **When** 拉伸窗口, **Then** TextBox 宽度自适应且可预测（使用 Grid 而非 DockPanel）
3. **Given** Case 面板右侧 ForceCase 区域, **When** 检查代码, **Then** 不存在空 TextBlock 用于占位的行

---

### Edge Cases

- 面板尺寸极小时（MinWidth/MinHeight 边界），控件是否发生重叠或截断？
- 某些面板内容超出可视区域时，ScrollViewer 是否正确工作？
- 主题切换后面板是否需要重新打开才能生效，还是立即刷新？

## Requirements

### Functional Requirements
#### 对齐规范术语（Alignment Terminology）

- **标签-内容双列对齐（Label-Field Two-Column Alignment）**：配置项统一使用“标签列 + 内容列”的双列网格布局。
- **固定标签列宽（Fixed Label Column Width）**：标签列使用统一固定宽度（简单面板 80px，复杂面板 100px）。
- **标签文本右对齐（Right-Aligned Label Text）**：标签文本在标签列内右对齐，确保与内容列形成稳定视觉锚点。
- **内容列左对齐（Left-Aligned Content Column）**：输入框、选项组、按钮等控件在内容列左对齐起排。
- **同级行骨架一致（Uniform Row Frame）**：同一面板内同级“标签-内容”行（如 Insert 与 Where）必须复用相同列骨架（列数/列宽/左边距一致），禁止通过额外 Margin 或字体加粗制造视觉偏移。
- **单行控件垂直居中（Single-line Controls Vertically Centered）**：单行输入与选择控件在行内垂直居中。
- **多行编辑区顶部左对齐（Multiline Editor Top-Left Aligned）**：多行文本/脚本编辑控件内容起点为顶部左侧。
- **弹窗默认屏幕居中（Dialog Startup: CenterScreen）**：弹出窗口首次打开默认位于屏幕中央。

#### 通用规范（适用于全部 17 个面板）

- **FR-001**: 所有面板外边距 MUST 统一为 `Margin="16,12"`
- **FR-002**: 所有标签列宽 MUST 统一为 80px（简单面板）或 100px（复杂面板如 Serialize），每个面板内部 MUST 保持一列宽度一致；标题/标签文本 MUST 右对齐；面板布局 MUST 遵循“标签-内容双列对齐”术语约定；同一面板内同级标签行 MUST 使用相同列骨架（列数/列宽/左边距一致）
- **FR-003**: 控件行间距 MUST 统一为 8px（相邻控件）和 16px（分组之间）；输入文本内容 MUST 左对齐；单行输入控件内容 MUST 垂直居中；多行/富文本编辑控件内容 MUST 顶部左对齐
- **FR-004**: 所有用户可见文本 MUST 使用 `{DynamicResource ...}` 实现本地化，不允许硬编码字符串
- **FR-005**: 所有颜色值 MUST 使用主题 Brush（如 `TextSecondaryBrush`、`SurfaceBrush`），不允许硬编码颜色
- **FR-006**: 所有 TextBox MUST 实现 Placeholder 机制显示格式提示
- **FR-007**: 每个面板中功能相关的控件 MUST 通过 section header（SemiBold TextBlock）或轻量 GroupBox 分组
- **FR-008**: 不允许使用空 TextBlock（`Text=""`）作为布局占位；缩进 MUST 通过 Margin 或 Grid 列实现
- **FR-009**: Hint / 说明文本 MUST 使用设计系统中的 `DialogHintText` 样式

#### 逐面板要求

- **FR-010 (Replace)**: Find / Replace 输入框尾部按钮 MUST 使用统一的单图标按钮样式（禁止使用 `|` 或空白字符作为按钮可见内容），并通过 Tooltip 提供语义提示；Occurrences 和 Flags MUST 有分组标题
- **FR-011 (Insert)**: MetaTag 按钮 MUST 紧贴输入框右侧；Where 区域 MUST 使用 Grid 统一对齐；`Insert` 与 `Where` 标签 MUST 使用相同标签样式（非粗体）并共用同列骨架
- **FR-012 (Delete)**: "Delete current name" MUST 有视觉提示表明其为特殊模式（勾选后禁用 From/Until）
- **FR-013 (Serialize)**: 左列 MUST 分为"编号参数"、"重置条件"、"格式选项"三个带标题的区域
- **FR-014 (Extension)**: 两个功能块（New Extension / Remove Duplicate）MUST 有视觉分隔
- **FR-015 (Case)**: 右侧 ForceCase MUST 用 GroupBox 或 section header 包裹；Extension 选项 MUST 独立分组
- **FR-016 (Rearrange)**: Delimiters 输入框 MUST 有标签；New Pattern MUST 有 Placeholder 提示格式
- **FR-017 (Strip)**: 所有硬编码英文 MUST 替换为 DynamicResource；只读 TextBox MUST 使用主题背景色
- **FR-018 (CleanUp)**: 散装 CheckBox MUST 按功能分为"空格处理"、"Unicode"、"其他"三组
- **FR-019 (Regex)**: Help 按钮 MUST 有明确的图标或 Tooltip；Expression 输入框 SHOULD 有语法验证反馈
- **FR-020 (Randomize)**: Length 控件 MUST 使用 Grid 布局（NUD 靠左），避免拉伸空白
- **FR-021 (Padding)**: 内联样式 MUST 移除，统一使用 FormLabelStyle；TextBox 宽度 MUST 自适应
- **FR-022 (Transliterate)**: 右侧 MUST 通过 section header 分为"方向"、"说明"、"选项"三区
- **FR-023 (ReformatDate)**: AdjustBy 行 MUST 使用 Grid 三列布局，避免控件重叠
- **FR-024 (UserInput)**: 选项区 MUST 使用更清晰的分组布局；编辑区 MUST 有说明文字
- **FR-025 (Mapping)**: DataGrid MUST 有空状态提示；选项 MUST 使用固定 Grid 布局
- **FR-026 (PascalScript)**: 计数器参数 MUST 使用 Grid 布局对齐；脚本模板 SHOULD 使用下拉按钮
- **FR-027 (Dialogs)**: 所有弹出窗口（如 AddRuleDialog、FiltersDialog、SettingsDialog）MUST 默认在屏幕中央打开

### Key Entities

- **RuleConfigPanel**: 17 个 UserControl，每个对应一种规则类型的配置界面
- **DesignSystemResources**: 共享样式资源字典，定义 FormLabelStyle、DialogHintText 等
- **Strings.zh-CN / Strings.en-US**: 本地化资源文件

## Success Criteria

### Measurable Outcomes

- **SC-001**: 所有 17 个面板的外边距、标签列宽、水平/垂直对齐规则、行间距通过自动化检查或人工审查确认一致
- **SC-002**: 亮色和暗色主题下所有面板无硬编码颜色导致的可读性问题（0 个硬编码颜色值）
- **SC-003**: 中文和英文模式下所有面板无硬编码文本残留（0 个硬编码用户可见字符串）
- **SC-004**: 所有 TextBox 均有 Placeholder 提示（覆盖率 100%）
- **SC-005**: 面板 XAML 中无空 TextBlock 占位 hack（`Text=""` 出现次数为 0）
- **SC-006**: 所有面板编译通过，现有 139 个单元测试全部通过（无回归）
- **SC-007**: 每个面板的功能区域通过 section header 或 GroupBox 分组，用户能在 3 秒内定位目标设置
- **SC-008**: 所有弹出窗口首次打开时位于屏幕中央，无需手动调整位置
- **SC-009**: 同级标签行（如 Insert/Where）在同一面板内无可见水平错位，且字段标签无非规范加粗（标题除外）
