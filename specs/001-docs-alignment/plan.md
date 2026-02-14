# 实施计划：文档对齐（ReNamerWPF）

## 目标

在不改动应用代码的前提下，完成文档的中文化与实现对齐，确保与 `docs/rules_requirements.md` 一致。

## 范围

### 纳入范围
- `docs/` 与 `ReNamerWPF/` 内的说明性文档中文化与对齐
- 规范性/状态性文档的更新说明与一致性校对
- 仅修改描述性文本（菜单名/功能名可保留英文）

### 不在范围
- 任何应用代码修改
- 需求功能补齐或实现变更

## 文档清单与处理策略

1. `docs/review_checklist.md`
   - 处理：对齐实现状态；中文化已完成
   - 冲突依据：以 `docs/rules_requirements.md` 为准

2. `ReNamerWPF/STATUS.md`
   - 处理：中文统一、术语一致、完成度与清单一致

3. `IMPLEMENTATION_VERIFICATION.md`
   - 处理：中文统一、阶段/术语中文化、更新说明保持

4. `ANALYSIS_SUMMARY.md`
   - 处理：中文统一、术语修正、完成度说明保持

5. `VERIFICATION_SUMMARY.md`
   - 处理：中文统一、术语修正、更新说明保持

6. `ReNamerWPF/FEATURE_TESTING_GUIDE.md`
   - 处理：中文化；描述与当前实现一致

7. `ReNamerWPF/MODERN_UI_PREVIEW.md`
   - 处理：中文化；去除过时“替换文件”指引；说明与当前实现一致

8. `ReNamerWPF/TESTING.md`
   - 处理：整篇中文化；保留命令/代码块

9. `ReNamerWPF/DESIGN_SYSTEM.md`
   - 处理：整篇中文化；与当前 UI 尺寸对齐

10. `specs/001-docs-alignment/spec.md`
   - 处理：已中文化，作为本次规格基准

## 核心一致性规则

- 发生冲突时，以 `docs/rules_requirements.md` 为准
- “描述性文本中文化”：不强制翻译菜单项/按钮文本
- 更新直接修改原文，不追加分区
- 每个被修改文档要有“更新时间”或清晰更新说明

## 风险与对策

- 风险：文档间自相矛盾
  - 对策：以 `docs/review_checklist.md` 作为统一对照表，并引用 `docs/rules_requirements.md`

- 风险：术语中英混杂导致误解
  - 对策：统一中文术语，菜单名/功能名保留英文

## 验收标准

- 所有指定文档均为中文描述，保留必要英文菜单/功能名
- 文档间无明显矛盾（若有，以 checklist + requirements 为准）
- 无应用代码变更

## 执行步骤

1. 全量扫描文档中的英文描述性内容并中文化
2. 对照当前实现修正文档误差
3. 二次复核文档一致性与“更新日期”存在性
4. 输出变更清单供用户确认
