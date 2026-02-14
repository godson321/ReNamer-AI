# 实施任务：文档对齐（ReNamerWPF）

## 阶段 1：清单与状态对齐

- [x] 1.1 复核 `docs/review_checklist.md` 与 `docs/rules_requirements.md` 的一致性
  - 逐段核对差异（菜单/规则/快捷键/列）
  - 标注仍未实现的项（以 requirements 为准）
  - **依赖**：无

- [x] 1.2 统一 `ReNamerWPF/STATUS.md` 与清单一致性
  - 更新完成度与“待完成功能”
  - 统一中文术语（保留菜单/功能名英文）
  - **依赖**：1.1

## 阶段 2：验证/分析摘要对齐

- [x] 2.1 对齐 `IMPLEMENTATION_VERIFICATION.md`
  - 更新对齐说明
  - 清理中英文混杂的描述性文本
  - **依赖**：1.2

- [x] 2.2 对齐 `ANALYSIS_SUMMARY.md`
  - 更新完成度与差距列表
  - 统一中文术语
  - **依赖**：1.2

- [x] 2.3 对齐 `VERIFICATION_SUMMARY.md`
  - 更新对齐说明
  - 统一中文术语
  - **依赖**：1.2

## 阶段 3：用户文档中文化

- [x] 3.1 对齐 `ReNamerWPF/FEATURE_TESTING_GUIDE.md`
  - 描述性文本中文化
  - 对齐实现差异说明（如 Add Folders 非递归）
  - **依赖**：1.2

- [x] 3.2 对齐 `ReNamerWPF/MODERN_UI_PREVIEW.md`
  - 描述性文本中文化
  - 移除过时指引（替换文件步骤）
  - **依赖**：1.2

- [x] 3.3 对齐 `ReNamerWPF/DESIGN_SYSTEM.md`
  - 描述性文本中文化
  - 尺寸参数与当前实现一致
  - **依赖**：1.2

- [x] 3.4 对齐 `ReNamerWPF/TESTING.md`
  - 描述性文本中文化
  - 保留命令/代码块英文
  - **依赖**：1.2

## 阶段 4：一致性复核

- [x] 4.1 全量复核文档一致性
  - 以 `docs/rules_requirements.md` 为准
  - 确认各文档无明显冲突
  - **依赖**：2.1, 2.2, 2.3, 3.1, 3.2, 3.3, 3.4

## 说明

- 仅修改描述性文本；菜单/功能名可保留英文。
- 不修改任何应用代码。
