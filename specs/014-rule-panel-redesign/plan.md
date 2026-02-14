# Implementation Plan: Rule Config Panel UI Redesign
**Branch**: `014-rule-panel-redesign` | **Date**: 2026-02-14 | **Spec**: `specs/014-rule-panel-redesign/spec.md`
**Input**: Feature specification from `/specs/014-rule-panel-redesign/spec.md`

## Summary
在不改变规则功能逻辑的前提下，统一 AddRuleDialog 中规则配置面板的布局节奏、分组层次、本地化与主题兼容性。技术路径是先建设通用能力（Placeholder 附加属性、设计系统样式扩展、缺失本地化资源），再按面板优先级批量改造 XAML，最后通过静态检查 + 编译测试验证“无硬编码文本/颜色、无空 TextBlock 占位、100% 输入提示覆盖”。

## Technical Context
**Language/Version**: C# 12 + .NET 8.0 (`net8.0-windows`) + WPF XAML  
**Primary Dependencies**: WPF, CommunityToolkit.Mvvm 8.2.2, 自定义控件 `NumericUpDown`, 主题服务 `ThemeService`  
**Storage**: N/A（本特性只修改 UI 资源与 XAML，不引入新持久化存储）  
**Testing**: xUnit (`ReNamer.Tests`) + `dotnet test ReNamerWPF/ReNamer.sln` + 手动 UI 验证清单  
**Target Platform**: Windows 10/11 桌面应用（WPF）  
**Project Type**: 单体桌面项目（`ReNamerWPF/ReNamer` + `ReNamerWPF/ReNamer.Tests`）  
**Performance Goals**: 面板切换与输入交互保持即时响应（无新增阻塞逻辑），滚动与布局调整不出现卡顿  
**Constraints**: 不改规则算法与默认参数；保持与原版行为一致；禁止硬编码用户可见文本与颜色；优先复用现有资源键命名约定  
**Scale/Scope**: 18 个规则配置面板（规范重点为 17 个核心面板，含 `Remove` 同步对齐），涉及 `Views/RuleConfigs`、`Resources`、`Themes`

## Constitution Check
*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

### Pre-Phase 0 Gate Review
- 功能精确重现（Principle I）: PASS  
  - 仅改 UI 结构、样式和资源绑定，不改规则执行逻辑、参数语义、默认值。
- UI/UX 一致性（Principle II）: PASS  
  - 本特性直接提升布局一致性、主题适配和分组可读性。
- 数据完整性（Principle III）: PASS  
  - 不触及文件预览/重命名核心流程，不新增文件系统副作用。
- 规则引擎架构（Principle IV）: PASS  
  - 保持 `IRuleConfigPanel` 和既有 code-behind 事件契约。
- 性能与可扩展性（Principle V）: PASS  
  - 仅 UI 层变更，无新增重计算循环。
- 测试驱动（Principle VII）: PASS（执行层面要求）  
  - 交付前执行全量现有测试，并补充 UI 一致性验收步骤。

### Post-Phase 1 Design Re-Check
- PASS：Phase 0/1 产物未引入与宪章冲突的实现假设。
- PASS：文档中的所有改动路径均限定在 UI、资源与校验流程。

## Project Structure
### Documentation (this feature)
```text
specs/014-rule-panel-redesign/
├── plan.md
├── research.md
├── data-model.md
├── quickstart.md
├── contracts/
│   └── rule-panel-audit.openapi.yaml
└── tasks.md             # 由 /speckit.tasks 生成（本阶段不创建）
```

### Source Code (repository root)
```text
ReNamerWPF/
├── ReNamer/
│   ├── Helpers/
│   │   └── PlaceholderBehavior.cs                # 新增
│   ├── Resources/
│   │   ├── DesignSystemResources.xaml            # 扩展样式/Brush
│   │   ├── Strings.en-US.xaml                    # 新增/修正资源键
│   │   └── Strings.zh-CN.xaml                    # 新增/修正资源键
│   ├── Themes/
│   │   └── Theme.xaml
│   └── Views/
│       ├── AddRuleDialog.xaml
│       └── RuleConfigs/
│           ├── *.xaml                            # 18 个面板逐个改造
│           └── *.xaml.cs                         # 仅必要联动改动
└── ReNamer.Tests/
    ├── CoreRulesTests.cs
    ├── BasicRulesTests.cs
    ├── AdvancedRulesTests.cs
    └── IntegrationTests.cs
```

**Structure Decision**: 采用单体 WPF 项目结构；本特性仅在 `ReNamerWPF/ReNamer` 的 UI 与资源层实施改造，测试继续复用 `ReNamerWPF/ReNamer.Tests` 的既有回归集。

## Implementation Phases
### Phase 0: Shared Foundations
1. 新增 Placeholder 附加属性（`PlaceholderBehavior`），支持 TextBox/可编辑 ComboBox 水印提示。
2. 在 `DesignSystemResources.xaml` 增加分组标题样式、只读输入背景 Brush、轻量分组样式。
3. 补齐 Strip 面板等缺失资源键，并统一使用 `RuleCfg_*` 前缀命名约定。

### Phase 1: P0/P1 First (High-Impact Panels)
按已确认顺序优先改造高频面板：
1. Replace
2. Insert
3. Delete
4. Case
5. Serialize
6. 其余面板按复杂度推进（Extension/CleanUp/Strip/Regex/Rearrange/...）。

每个面板改造模板：
1. 统一 Margin / 标签列宽 / 行间距。
2. 添加 Placeholder。
3. 删除布局 hack（空 TextBlock、不可预测 DockPanel）。
4. 补分组标题或 GroupBox。
5. 清除硬编码文本与颜色。

### Phase 2: Global Consistency Pass
1. 静态检查：硬编码颜色、硬编码字符串、空 TextBlock 占位。
2. 编译 + 全量测试。
3. 手动验收亮/暗主题与中/英文切换。

## Complexity Tracking
无宪章违规项，不需要复杂度豁免。
