# 实施计划：002 主窗口 UI 优化（WPF）

## 目标
在不新增功能的前提下，优化主窗口视觉层级与可用性，解决已反馈问题（按钮遮挡/层级不齐、表格对比度弱、边距过大、多语言混杂），并保持与现有实现一致。

## 范围
- `ReNamerWPF/ReNamer/Views/MainWindow.xaml(.cs)` 的布局与样式调整
- `ReNamerWPF/ReNamer/Resources/DesignSystemResources.xaml` 的设计令牌/样式修订
- `ReNamerWPF/ReNamer/Themes/Theme.xaml` 的主题刷子补齐（必要时）
- `ReNamerWPF/ReNamer/Resources/Strings.*.xaml` 的描述性文本统一

## 不在范围
- 业务逻辑、规则引擎与文件处理算法
- 新增功能或改变交互流程
- 第三方控件替换

## 设计意图（待确认）
在实现前确认以下三点：
1. 主要使用者画像
2. 主要完成任务
3. 希望界面传达的感受

## 实施策略
1. 明确设计方向与层级策略（颜色、对比度、间距、深度）
2. 统一顶部主操作区与工具栏按钮尺寸/对齐，修复遮挡
3. 强化规则表格与文件表格的层级区分（背景、边界、标题行）
4. 收敛页面边距与内边距，提升信息密度但不压迫
5. 统一可本地化文本来源，避免中英混杂
6. 复核与文档对齐（必要时更新 `MODERN_UI_PREVIEW.md`/`DESIGN_SYSTEM.md`）

## 风险与对策
- 风险：主题刷子动态替换导致 `UnsetValue`  
  对策：样式全部使用 `DynamicResource` 绑定
- 风险：WindowChrome 命中区域导致按钮不可点击  
  对策：确认 `shell:WindowChrome.IsHitTestVisibleInChrome="True"`

## 验收标准
- 顶部按钮无遮挡，尺寸一致，层级清晰
- 规则表格与文件表格区分明显，标题行可读性提升
- 页面边距合理（空白不过量）
- 界面描述性文字中文统一（功能名可保留英文）
