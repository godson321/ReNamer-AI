# 实施计划：对话框与规则面板企业化统一标准

**分支**：`016-dialog-enterprise-standardization` | **日期**：2026-02-22 | **规格**：`specs/016-dialog-enterprise-standardization/spec.md`  
**输入**：来自 `specs/016-dialog-enterprise-standardization/spec.md` 的功能规格

## 摘要

本计划将企业化 UI 规则从局部页面推广到三大范围：

1. `SettingsDialog.xaml`
2. `TextInputDialog.xaml`
3. 所有在用规则面板 `Views/RuleConfigs/*.xaml`（15 个）

交付目标是形成统一骨架、统一按钮标准、统一输入与标签对齐规范，并保证中文优先显示与主题动态资源兼容。

## 技术上下文

**语言/版本**：C# 12 / .NET 8（`net8.0-windows`）  
**主要依赖**：WPF、CommunityToolkit.Mvvm  
**存储**：N/A（本需求不引入新存储）  
**测试**：`dotnet build` + `dotnet run` + 人工视觉回归  
**目标平台**：Windows 桌面  
**项目类型**：单体桌面应用（WPF）  
**性能目标**：UI 统一改造后保持现有交互流畅，无明显布局卡顿  
**约束**：不改业务逻辑，仅做结构/样式/资源层标准化  
**规模/范围**：
- `ReNamerWPF/ReNamer/Views/SettingsDialog.xaml`
- `ReNamerWPF/ReNamer/Views/TextInputDialog.xaml`
- `ReNamerWPF/ReNamer/Views/RuleConfigs/*.xaml`（15 个）
- `ReNamerWPF/ReNamer/Resources/DesignSystemResources.xaml`
- `ReNamerWPF/ReNamer/Resources/Strings.zh-CN.xaml`
- `ReNamerWPF/ReNamer/Resources/Strings.en-US.xaml`

## 项目工程治理规范检查

- 术语统一：`constitution.md` 对外统一称“项目工程治理规范”。
- 主题资源要求：依赖 ThemeService 的 Brush 必须使用 `DynamicResource`。
- 分支流程：按项目约定直接在 `main` 提交。
- 运行验证：修改完成后必须执行一次 `dotnet run --project ReNamerWPF/ReNamer/ReNamer.csproj`。

## 项目结构

### 本次需求文档结构

```text
specs/016-dialog-enterprise-standardization/
├── spec.md
├── plan.md
└── tasks.md
```

### 目标源码结构（关键文件）

```text
ReNamerWPF/ReNamer/
├── Resources/
│   ├── DesignSystemResources.xaml
│   ├── Strings.zh-CN.xaml
│   └── Strings.en-US.xaml
└── Views/
    ├── SettingsDialog.xaml
    ├── TextInputDialog.xaml
    └── RuleConfigs/
        ├── CaseConfigPanel.xaml
        ├── CleanUpConfigPanel.xaml
        ├── DeleteConfigPanel.xaml
        ├── InsertConfigPanel.xaml
        ├── JavaScriptConfigPanel.xaml
        ├── MappingConfigPanel.xaml
        ├── PaddingConfigPanel.xaml
        ├── RandomizeConfigPanel.xaml
        ├── RearrangeConfigPanel.xaml
        ├── ReformatDateConfigPanel.xaml
        ├── RegexConfigPanel.xaml
        ├── ReplaceConfigPanel.xaml
        ├── SerializeConfigPanel.xaml
        ├── TransliterateConfigPanel.xaml
        └── UserInputConfigPanel.xaml
```

**结构决策**：采用“Design System 样式先行 + 页面分批接入 + 最终统一验证”的分层实施方式。

## Phase 1 执行结果（基线冻结）

### 1) 规则面板清单锁定（15 个）

| 序号 | 面板文件 |
|---|---|
| 1 | `ReNamerWPF/ReNamer/Views/RuleConfigs/CaseConfigPanel.xaml` |
| 2 | `ReNamerWPF/ReNamer/Views/RuleConfigs/CleanUpConfigPanel.xaml` |
| 3 | `ReNamerWPF/ReNamer/Views/RuleConfigs/DeleteConfigPanel.xaml` |
| 4 | `ReNamerWPF/ReNamer/Views/RuleConfigs/InsertConfigPanel.xaml` |
| 5 | `ReNamerWPF/ReNamer/Views/RuleConfigs/JavaScriptConfigPanel.xaml` |
| 6 | `ReNamerWPF/ReNamer/Views/RuleConfigs/MappingConfigPanel.xaml` |
| 7 | `ReNamerWPF/ReNamer/Views/RuleConfigs/PaddingConfigPanel.xaml` |
| 8 | `ReNamerWPF/ReNamer/Views/RuleConfigs/RandomizeConfigPanel.xaml` |
| 9 | `ReNamerWPF/ReNamer/Views/RuleConfigs/RearrangeConfigPanel.xaml` |
| 10 | `ReNamerWPF/ReNamer/Views/RuleConfigs/ReformatDateConfigPanel.xaml` |
| 11 | `ReNamerWPF/ReNamer/Views/RuleConfigs/RegexConfigPanel.xaml` |
| 12 | `ReNamerWPF/ReNamer/Views/RuleConfigs/ReplaceConfigPanel.xaml` |
| 13 | `ReNamerWPF/ReNamer/Views/RuleConfigs/SerializeConfigPanel.xaml` |
| 14 | `ReNamerWPF/ReNamer/Views/RuleConfigs/TransliterateConfigPanel.xaml` |
| 15 | `ReNamerWPF/ReNamer/Views/RuleConfigs/UserInputConfigPanel.xaml` |

### 2) 企业样式键基线核查

| 核查项 | 结果 | 位置 |
|---|---|---|
| `EnterpriseDialogCardStyle` | 通过 | `ReNamerWPF/ReNamer/Resources/DesignSystemResources.xaml` |
| `EnterpriseDialogInputTextBoxStyle` | 通过 | `ReNamerWPF/ReNamer/Resources/DesignSystemResources.xaml` |
| `EnterpriseDialogInputComboBoxStyle` | 通过 | `ReNamerWPF/ReNamer/Resources/DesignSystemResources.xaml` |
| `EnterpriseDialogPrimaryActionButtonStyle` | 通过 | `ReNamerWPF/ReNamer/Resources/DesignSystemResources.xaml` |
| `EnterpriseDialogSecondaryActionButtonStyle` | 通过 | `ReNamerWPF/ReNamer/Resources/DesignSystemResources.xaml` |

### 3) 文案资源键基线核查

| 语言 | 核查结果 | 说明 |
|---|---|---|
| `Strings.zh-CN.xaml` | 通过 | `Dialog_AddRule` / `Dialog_Close` / `Dialog_Save` / `Dialog_Cancel` / `Dialog_Configuration` / `Dialog_RuleTypes` 均存在 |
| `Strings.en-US.xaml` | 通过 | 对应英文键均存在 |

> 结论：本轮基线核查阶段未发现必须新增的标题/按钮资源键，后续在具体页面改造中若出现新文案再补充。

### 4) 视觉契约冻结（本轮生效）

- 标签列宽：`118`
- 输入控件高度：`28`
- 主次按钮最小尺寸：`MinWidth >= 128`、`MinHeight >= 36`
- 按钮内边距：`Padding = 14,6`
- 单位列最小宽度：`>= 84`
- 规则面板保持紧凑化布局：不强制顶部说明卡

## 分阶段实施策略

### 阶段 1：规范冻结与样式基线

- 冻结企业化布局契约：三段骨架、标签列 `118`、输入高 `28`
- 冻结按钮标准：`MinWidth>=128`、`MinHeight>=36`、`Padding=14,6`
- 核查通用样式键完整性，补齐缺失资源键与文案键

### 阶段 2：SettingsDialog 接入

- 将 `SettingsDialog.xaml` 改造成统一三段骨架
- 接入企业按钮样式与输入样式
- 补齐中文环境防裁切细节

### 阶段 3：TextInputDialog 接入

- 将 `TextInputDialog.xaml` 改造成统一三段骨架
- 输入区、提示区与按钮区统一基线和间距
- 接入像素对齐设置

### 阶段 4：规则面板批量接入（15 面板）

- 逐面板统一标签/输入骨架与按钮标准
- 对含“数值+单位”行面板落实单位列最小宽度 `84`
- 对特殊布局面板保留业务交互，仅替换外观与对齐规范
- 规则面板保持紧凑化布局，不强制增加顶部说明卡（顶部说明卡仅用于对话框）

### 阶段 5：验证与收口

- 执行构建与运行冒烟
- 执行中文环境视觉回归（裁切、遮挡、对齐、主题切换）
- 更新任务状态与收口结论

## 验证结果与风险收口（2026-02-23）

| 验证项 | 结果 | 结论 |
|---|---|---|
| T033 构建验证 | `dotnet build ReNamerWPF/ReNamer/ReNamer.csproj` 成功（0 错误，0 警告） | 通过 |
| T034 启动冒烟 | `dotnet run --project ReNamerWPF/ReNamer/ReNamer.csproj -c Debug --no-build` 成功启动 | 通过 |
| T035 SettingsDialog 回归 | 中文显示、缩放与主题切换按清单检查 | 通过 |
| T036 TextInputDialog 回归 | 中文显示、缩放与主题切换按清单检查 | 通过 |
| T037 规则面板回归 | 15 个规则面板完成对齐/按钮/单位下拉/防遮挡检查 | 通过 |

| 风险项 | 收口动作 | 当前状态 |
|---|---|---|
| `obj` 编译锁导致构建失败 | 冒烟阶段固定执行 build + run，异常时先释放占用进程再重试 | 已收口 |
| 中文文本裁切/遮挡 | 统一标签列宽、输入高度、按钮中文优先尺寸并逐面板检查 | 已收口 |
| 主题切换 `UnsetValue` | 规则面板统一将主题相关样式改为 `DynamicResource` 引用 | 已收口 |

## 风险与应对

| 风险点 | 影响 | 应对策略 |
|---|---|---|
| WPF `obj` 文件锁（`*.g.cs`、`MarkupCompile.cache`） | 构建失败，无法验证 | 关闭占用进程后重建；必要时清理锁文件并重试 |
| 规则面板数量多导致风格不一致回归 | 交付质量波动 | 按统一验收清单逐面板勾检 |
| 中文长度差异导致按钮/下拉裁切 | 界面可用性下降 | 强制最小尺寸与列宽标准，人工中文回归 |
| 主题切换触发 `UnsetValue` | 运行时异常 | 所有主题相关 Brush 强制 `DynamicResource` |

## 交付物

- `specs/016-dialog-enterprise-standardization/spec.md`
- `specs/016-dialog-enterprise-standardization/plan.md`（本文件）
- `specs/016-dialog-enterprise-standardization/tasks.md`
- 代码与资源改造（Settings/TextInput/RuleConfigs 全量接入）
