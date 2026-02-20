# 实施计划：WPF 项目全量评审与优化建议清单

**分支**：`015-wpf-review-optimization` | **日期**：2026-02-20 | **规格**：`specs/015-wpf-review-optimization/spec.md`  
**输入**：来自 `specs/015-wpf-review-optimization/spec.md` 的功能规格

## 摘要

本计划聚焦于对 `ReNamerWPF` 项目进行四维度（功能、易用性、可用性、界面美观度）全量评审，输出结构化问题清单与优化路线图。阶段目标是：

1. 建立可追溯的问题台账（含证据、优先级、影响范围）
2. 给出可执行优化建议（含实施方式、风险、收益）
3. 形成可复用评审模板，支持后续版本增量复审

## 技术上下文

**语言/版本**：C# 12 / .NET 8（`net8.0-windows`）  
**主要依赖**：WPF、CommunityToolkit.Mvvm、Jint、AvalonEdit  
**存储**：本地文件系统 + JSON（预设与配置）  
**测试**：xUnit + coverlet（现有测试工程 `ReNamer.Tests`）  
**目标平台**：Windows 桌面  
**项目类型**：单体桌面应用（WPF）  
**性能目标**：批量重命名与预览在常见文件规模下保持可交互响应（详指标待补充）  
**约束**：保持现有行为兼容；优先低风险改进；避免引入大规模回归  
**规模/范围**：主窗口 + 规则系统 + 配置/预设 + 多语言/主题 + 测试与文档

## 项目工程治理规范检查

- 本次任务属于“评审与优化建议”阶段，以分析输出为主，不直接进行大规模代码重构。
- 交付需满足：可追溯（有证据）、可执行（有落地动作）、可验证（有验收方式）。
- 若后续进入实施阶段，将在 `tasks.md` 中进一步拆分并绑定具体改动文件。

## 项目结构

### 本次需求文档结构

```text
specs/015-wpf-review-optimization/
├── spec.md              # 需求规格（中文）
├── plan.md              # 实施计划（本文件）
└── tasks.md             # 后续任务拆解（待生成）
```

### 源码结构（仓库根目录）

```text
ReNamerWPF/
├── ReNamer/
│   ├── Views/                   # 主窗口、对话框、规则配置面板
│   ├── Rules/                   # 规则引擎与规则实现
│   ├── Services/                # 业务服务（重命名、预设、主题、语言等）
│   ├── Models/                  # 领域模型
│   ├── Resources/               # 文本资源、图标、设计系统资源
│   ├── Themes/                  # 主题资源
│   └── Controls/                # 自定义控件
├── ReNamer.Tests/               # 单元/集成测试
├── DESIGN_SYSTEM.md             # 设计系统说明
├── FEATURE_TESTING_GUIDE.md     # 人工功能测试指南
└── TESTING.md                   # 测试与覆盖率说明
```

**结构决策**：采用“单项目桌面应用 + 独立测试工程”结构，评审输出按“维度 -> 问题 -> 建议 -> 验证”组织，不引入新子系统。

## 分阶段实施策略

### 阶段 0：评审基线准备

- 收敛评审范围：`Views`、`Rules`、`Services`、`Resources/Themes`、`Tests`
- 对齐已有文档：`STATUS.md`、`TESTING.md`、`DESIGN_SYSTEM.md`、`FEATURE_TESTING_GUIDE.md`
- 确认评分维度与优先级定义（P0/P1/P2，严重级别高/中/低）

### 阶段 1：四维度深度评审

- 功能维度：覆盖主流程、规则链、预览/重命名/撤销、预设、主题与语言
- 易用性维度：信息架构、交互路径、快捷键可发现性、错误反馈清晰度
- 可用性维度：异常处理、边界场景、稳定性、可测试性、回归风险
- 美观度维度：设计一致性、排版/间距/层级、状态色语义、主题统一性

### 阶段 2：优化建议与路线图

- 输出问题清单（含证据路径与影响范围）
- 输出 Quick Wins（低成本高收益）
- 输出结构性改进项（中长期）
- 给出分期路线：短期（1-2 周）/中期（1-2 月）/长期（季度级）

### 阶段 3：复用沉淀

- 固化评审模板（供后续版本复审）
- 明确回归验证建议（手工清单 + 自动化测试建议）
- 在 `tasks.md` 补充实施任务拆解（若进入改造执行）

## 复杂度跟踪

| 风险点 | 影响 | 应对策略 |
|--------|------|----------|
| UI 交互行为难以完全静态判断 | 易产生误判 | 标记“需人工验证”，引用操作步骤 |
| 历史文档与现状可能不一致 | 影响结论可信度 | 以代码为准，并标注文档同步建议 |
| 问题跨模块耦合 | 修复范围扩大 | 拆分“主问题+关联问题”，分阶段实施 |
| 大改动潜在回归风险 | 影响稳定性 | 优先 Quick Wins，结构性改造后置 |

## 交付物定义

- `spec.md`：中文需求与成功标准（已完成）
- `plan.md`：中文实施计划（本文件）
- `tasks.md`：如进入执行阶段，按用户故事拆分任务并标注依赖
- 评审报告（后续产出）：四维度问题清单 + 优化建议 + 优先级路线图

## 执行更新（2026-02-20，全部阶段收口）

### 变更摘要

- 阶段 1~6 任务已全部完成并在 `tasks.md` 勾选收口。
- 阶段 3 收口：完成 T015/T016/T017（取消感知、异常处理统一、回收站失败逐文件汇总）。
- 阶段 5 收口：完成 T022~T026（Design Tokens 单一源、无边框系统行为补齐、高 DPI 行高留白、列宽持久化、文字资源键统一）。
- 阶段 6 收口：完成 T029/T030/T031（人工回归记录、评审结果更新、计划风险更新）。

### 验证结果

- 构建通过（可复现命令）：`dotnet build ReNamerWPF/ReNamer/ReNamer.csproj -c Release -v minimal -p:GenerateAssemblyInfo=false -p:GenerateTargetFrameworkAttribute=false -p:GenerateTargetPlatformAttribute=false`。
- 自动化测试通过：`dotnet test ReNamerWPF/ReNamer/ReNamer.sln -c Release -v minimal -p:GenerateAssemblyInfo=false -p:GenerateTargetFrameworkAttribute=false -p:GenerateTargetPlatformAttribute=false`（134/134）。
- 启动冒烟通过：`dotnet run --project ReNamerWPF/ReNamer/ReNamer.csproj -c Release`（短时拉起验证后结束进程）。
- 人工回归（关键路径）通过：回收站删除、重命名确认、窗口位置恢复、添加目录容错。

### 剩余风险清单

- WPF 临时项目文件锁风险：在 CLI/沙箱环境偶发 `*_wpftmp.csproj`、`*.g.cs`、`MarkupCompile.cache` 被锁导致失败；在 VS 本地构建通常可恢复。
- 验证命令参数风险：为绕过临时重复程序集属性问题，当前使用了显式参数关闭生成相关属性；后续建议排查 `obj` 残留生成链并恢复标准命令。
- 大数据滚动体验仍需用户实景压测：已做虚拟化与节流优化，但真实目录规模与磁盘状态差异较大，建议继续以用户数据集做验收基线。
