# Codex全局工作指南

## 通用规则

### Session启动提醒
- 每次新session请先执行：serena.activate_project（必要时用脚本复制命令片段）
- 每次新session请先加载 using-superpowers 技能，确保遵循技能使用规范（在任何响应前检查是否有适用的技能）

### 回答风格
- 回答必须使用中文，每次回复前都要说 “你好MOSS，我是启示录”
- 如果当前使用了skills，必须把正在用的skills的名字输出，然后再输出正常内容
- 对总结、Plan、Task、以及长内容的输出，优先进行逻辑整理后使用美观的Table格式整齐输出;普通内容正常输出

### 工具使用
1. 文件与代码检索：使用serena mcp来进行文件与代码的检索
2. 文件相关操作：对文件的创建、读取、编辑、删除等操作
- 优先使用apply_patch工具进行
- 读文件，apply_patch工具报错或出现问题的情况下使用desktop-commander mcp
- 任何情况下，禁止使用cmd、powershell或者python来进行文件相关操作

### 记忆写入规则
- 当用户明确说“让你记住/要记住”时，必须把该事项写入本项目 `AGENTS.md`

## 本项目规则

### 主题资源注意事项
- 凡是依赖 ThemeService 动态替换的 Brush（如 `CardBrush`），在样式中必须使用 `DynamicResource`，避免运行时出现 `UnsetValue` 报错

### 项目工程治理与流程
- 术语约定：`constitution.md` 在沟通中统一称为“项目工程治理规范”，避免使用“宪章”表述
- Git 流程约定：后续所有提交均直接在 `main` 分支进行，不创建子分支
- 命令示例与执行命令必须检查引号成对闭合（尤其是 PowerShell `-Command` 场景），避免出现等待补全输入的问题
- 编译通过不等于可运行：每次完成修改后，除编译外还必须启动应用验证一次（至少 `dotnet run --project ReNamerWPF/ReNamer/ReNamer.csproj`）

### 本项目界面设计规则
- 当用户要求“设计页面/界面”时：
  - CS 项目优先使用 `interface-design`
  - BS 项目优先使用 `ui-ux-pro-max` 或 `ui-designer`
- 过滤器/对话框企业化统一规则（可复用到各规则面板）：
  - 布局骨架统一为“顶部说明卡 + 中部内容卡 + 底部操作卡”，不使用 Tab 作为主结构
  - 卡片样式统一使用 `EnterpriseDialogCardStyle`（`DynamicResource` 主题资源、统一圆角/边框/内边距）
  - 对话框窗口统一开启 `UseLayoutRounding="True"` 与 `SnapsToDevicePixels="True"`
  - 表单标签列统一宽度 `118`，输入控件统一高度 `28`
  - 底部主次按钮统一使用企业按钮样式，按中文显示优先：`MinWidth=128`、`MinHeight=36`、`Padding=14,6`
  - 禁止按英文短文本固定按钮尺寸，避免中文文本被遮挡
  - 含“数值 + 单位”行时，单位下拉需保留足够宽度（建议不小于 `84`）并预留右侧间距，防止箭头/文本裁切
