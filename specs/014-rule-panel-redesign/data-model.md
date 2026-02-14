# Data Model: Rule Config Panel UI Redesign
## 1. RulePanelDefinition
描述单个规则配置面板的结构基线与范围信息。

### Fields
- `panelId` (string, required): 面板唯一标识（如 `replace`, `insert`, `strip`）。
- `xamlPath` (string, required): 面板 XAML 路径。
- `priority` (enum, required): `P0 | P1 | P2 | P3`。
- `labelColumnWidth` (int, required): 80 或 100。
- `rootMargin` (string, required): 规范值 `16,12`。
- `minWidth` (int, required): 面板最小宽度。
- `usesScrollViewer` (bool, required): 是否需要滚动容器。

### Validation
- `panelId` 全局唯一。
- `xamlPath` 必须存在于 `Views/RuleConfigs/`。
- `labelColumnWidth` 只能为 80 或 100。
- `rootMargin` 必须匹配统一规范。

## 2. PanelSection
描述面板内部的视觉分组（GroupBox 或 section header）。

### Fields
- `sectionId` (string, required)
- `panelId` (string, required, FK -> RulePanelDefinition.panelId)
- `titleResourceKey` (string, optional): 标题资源键（允许为空用于无标题组）。
- `sectionType` (enum, required): `GroupBox | HeaderBlock`
- `displayOrder` (int, required)
- `spacingBefore` (int, required)
- `spacingAfter` (int, required)

### Validation
- 同一 `panelId` 下 `displayOrder` 不能重复。
- `titleResourceKey` 非空时必须存在于双语资源文件。

## 3. FieldDescriptor
描述输入控件及其一致性要求。

### Fields
- `fieldId` (string, required)
- `panelId` (string, required, FK)
- `controlName` (string, required)
- `controlType` (enum, required): `TextBox | ComboBoxEditable | CheckBox | RadioButton | NumericUpDown | DataGrid`
- `labelResourceKey` (string, optional)
- `placeholderResourceKey` (string, optional)
- `usesThemeBrushOnly` (bool, required)
- `layoutContainerType` (enum, required): `Grid | StackPanel | GroupBox | DockPanel`

### Validation
- `TextBox`/`ComboBoxEditable` 推荐 `placeholderResourceKey` 非空（FR-006）。
- `usesThemeBrushOnly=true` 时不允许硬编码 `#RRGGBB` / `Foreground=\"Gray\"`。
- 禁止通过空 `TextBlock` 进行占位（FR-008）。

## 4. LocalizationEntry
描述本地化键在中英文资源中的映射。

### Fields
- `resourceKey` (string, required)
- `locale` (enum, required): `en-US | zh-CN`
- `value` (string, required)
- `sourceFile` (string, required)

### Validation
- 同一 `resourceKey` 必须同时存在于 `Strings.en-US.xaml` 与 `Strings.zh-CN.xaml`。
- 用户可见文本不允许硬编码到面板 XAML（FR-004）。

## 5. ThemeBinding
描述控件视觉属性与主题资源的绑定关系。

### Fields
- `panelId` (string, required, FK)
- `targetControl` (string, required)
- `property` (enum, required): `Foreground | Background | BorderBrush`
- `brushKey` (string, required)

### Validation
- `brushKey` 必须可由 `ThemeService` 更新链路覆盖。
- 不允许直接绑定字面颜色。

## 6. PanelConformanceResult
描述单面板验收结果，用于最终质量门禁。

### Fields
- `panelId` (string, required)
- `hardcodedTextCount` (int, required)
- `hardcodedColorCount` (int, required)
- `emptyTextBlockCount` (int, required)
- `placeholderCoverage` (decimal, required, 0-100)
- `sectionCoverage` (decimal, required, 0-100)
- `status` (enum, required): `Fail | Pass`

### Validation
- `status=Pass` 的条件：
  - `hardcodedTextCount = 0`
  - `hardcodedColorCount = 0`
  - `emptyTextBlockCount = 0`
  - `placeholderCoverage = 100`

## Relationships
- `RulePanelDefinition 1 -> N PanelSection`
- `RulePanelDefinition 1 -> N FieldDescriptor`
- `RulePanelDefinition 1 -> N ThemeBinding`
- `RulePanelDefinition 1 -> 1 PanelConformanceResult`
- `LocalizationEntry` 被 `PanelSection` 与 `FieldDescriptor` 通过 `resourceKey` 引用

## State Transitions
`PanelConformanceResult.status`:
- `Fail` -> `Pass`：完成本地化、主题、布局和 Placeholder 改造后，静态与手动检查同时通过。
- `Pass` -> `Fail`：后续改动引入任一硬编码文本/颜色或丢失 Placeholder 时回退。
