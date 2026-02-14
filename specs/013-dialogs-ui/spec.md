# Feature 013: 弹窗与规则配置面板 UI 优化

## 概述

**Feature Name**: 弹窗与规则配置面板优化  
**Priority**: P0（影响整体一致性与可用性）  
**状态**: 规划中  
**Dependencies**: Feature 002（主窗口 UI 风格基线）

## 问题陈述

除主界面外，多个弹窗与规则配置面板存在以下问题：
1. 层级不清、对比度弱，阅读负担大  
2. 组件间距不统一，布局高低不齐  
3. 文案与多语言资源混杂（硬编码英文）

## 目标

1. 统一弹窗与规则配置面板的视觉层级与密度  
2. 统一按钮与表单控件的规格与对齐  
3. 完成描述性文案的资源化与多语言一致性  

## 范围

- `ReNamerWPF/ReNamer/Views/AddRuleDialog.xaml`
- `ReNamerWPF/ReNamer/Views/SettingsDialog.xaml`
- `ReNamerWPF/ReNamer/Views/FiltersDialog.xaml`
- `ReNamerWPF/ReNamer/Views/RuleConfigs/*.xaml`
- `ReNamerWPF/ReNamer/Views/RuleConfigs/RuleConfigHelper.cs`
- `ReNamerWPF/ReNamer/Resources/Strings.*.xaml`
- `ReNamerWPF/ReNamer/Resources/DesignSystemResources.xaml`

## 不在范围

- 业务逻辑改动
- 规则引擎算法变更
- 新增功能或交互流程改变

## 验收标准

- 弹窗与规则配置面板层级清晰、对比度合理  
- 组件间距与尺寸统一，布局不跳动  
- 描述性文案均使用资源，避免中英混杂  
