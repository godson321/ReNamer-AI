# Research: Rule Config Panel UI Redesign
## Context
本特性目标是统一 18 个规则配置面板（重点 17 个核心面板）的布局、一致性、输入引导、本地化和主题兼容性，并满足 `spec.md` 中 FR-001 ~ FR-026 与 SC-001 ~ SC-007。

## Decision 1: Placeholder 实现方式
- Decision: 使用附加属性 + `VisualBrush`（`PlaceholderBehavior`）为 TextBox/可编辑 ComboBox 提供水印提示。
- Rationale: 不改控件模板即可逐步接入；对现有样式侵入小；可按控件粒度启用。
- Alternatives considered:
  - 重写全局 TextBox ControlTemplate：影响面过大，回归风险高。
  - 引入第三方控件库：增加依赖，不符合本次最小变更原则。

## Decision 2: 面板布局基线
- Decision: 统一采用 `Margin="16,12"`，简单面板标签列 80，复杂面板标签列 100，行间距 8，分组间距 16。
- Rationale: 与现有高质量面板（Replace/Insert/Remove）节奏一致，兼顾紧凑与可读性。
- Alternatives considered:
  - 全部使用 72（当前 `FormLabelStyle`）：复杂面板（Serialize/ReformatDate）标签易截断。
  - 全部使用 140：浪费横向空间，窄窗表现差。

## Decision 3: 分组表达方式
- Decision: 主要使用带边框 GroupBox（复杂区域）+ section header（轻量区域）。
- Rationale: 与澄清结果一致；可同时满足层次清晰与视觉统一。
- Alternatives considered:
  - 仅用 TextBlock 标题：边界弱，复杂面板可读性不足。
  - 全部 GroupBox：视觉负担重，简单面板显得拥挤。

## Decision 4: 本地化键策略
- Decision: 继续使用 `Strings.en-US.xaml` / `Strings.zh-CN.xaml`，优先复用 `RuleCfg_*` 命名空间，补齐 Strip 面板缺失键。
- Rationale: 项目当前已大量使用 `RuleCfg_*` 资源键；迁移成本低且一致性高。
- Alternatives considered:
  - 新建独立 resource 文件：增加维护成本并分散语言资源。

## Decision 5: 主题兼容策略
- Decision: 禁止面板内硬编码颜色，统一绑定 `TextSecondaryBrush`、`CardBrush`、`SurfaceBrush` 等由 `ThemeService` 动态更新的 Brush。
- Rationale: `ThemeService` 已负责 Light/Dark 切换，复用现有机制可避免主题失配。
- Alternatives considered:
  - 局部继续保留十六进制颜色：暗色模式下可读性风险不可控。

## Decision 6: 布局 Hack 清理策略
- Decision: 移除 `Text=""` 空 TextBlock 占位；DockPanel 场景改为 Grid 明确列定义。
- Rationale: Grid 可预测，便于统一标签列和输入列的对齐策略。
- Alternatives considered:
  - 保留 hack 仅改样式：无法解决可维护性问题（FR-008）。

## Decision 7: 图标策略
- Decision: 优先使用项目内 `Resources/Icons/IconResources.xaml` 几何图标；缺失时使用文本+Tooltip 过渡。
- Rationale: 不增加外部依赖，保持现有图标系统可控。
- Alternatives considered:
  - 在线引入图标包：需额外授权/打包流程，超出本阶段实施边界。

## Decision 8: 实施顺序
- Decision: 按高频面板优先（Replace → Insert → Delete → Case → Serialize → 其他），并先做基础设施后做面板批量改造。
- Rationale: 在最短路径上提升可见质量，降低一次性大改回归风险。
- Alternatives considered:
  - 按文件字母序修改：业务价值不高，无法优先解决痛点。

## Decision 9: 验证策略
- Decision: 使用“静态扫描 + 编译 + 单元测试 + 手动 UI 场景”四层校验。
- Rationale: 该特性横跨 XAML 结构与视觉表现，纯自动化无法覆盖全部体验指标。
- Alternatives considered:
  - 仅运行 `dotnet test`：无法检测硬编码文本/颜色与视觉分组效果。

## Decision 10: 合规边界
- Decision: 不修改规则算法和参数语义，仅调整 UI 呈现层。
- Rationale: 满足宪章“功能精确重现”原则，避免引入行为回归。
- Alternatives considered:
  - 同步做交互逻辑优化（实时预览等）：已明确不在本次范围。
