# ReNamer_RE Constitution

## Project Overview

**ReNamer_RE** 是 ReNamer 7.8 的重制版本,一个专业的批量文件重命名工具。项目目标是基于反编译的 DFM 资源,使用现代技术栈重建功能完整、高质量的桌面应用程序。

## Core Principles

### I. 功能精确重现 (Functional Fidelity)

**原则**: 所有功能必须精确重现 ReNamer 7.8 的行为,不引入未经验证的改进。

**实施要求**:
- 每个控件、属性、事件处理器都必须与 DFM 资源中的定义完全对应
- 18种规则类型的参数、默认值、选项必须完全一致
- 主界面布局、工具栏、菜单、快捷键必须与原版匹配
- 虚拟文件列表(VST)的 21 列、排序、编辑、拖放功能必须完整实现
- 文件状态(State)、标记(Mark)、选择(Select)的三态逻辑必须保持

**检查点**:
- 对照 `docs/rules_requirements.md` 的详细规格
- 对照 `docs/review_checklist.md` 验证每个细节
- 原版行为测试用例必须全部通过

### II. UI/UX 一致性 (UI/UX Consistency)

**原则**: 用户界面必须专业、一致、符合现代桌面应用标准。

**设计规范**:
- **布局**: 主工具栏(38px)→规则面板(可调,默认145px)→水平分隔条(5px)→文件面板(可调)→状态栏(23px)
- **颜色主题**: 支持系统主题(Theme-Aware),默认使用 Windows 视觉样式
- **字体**: 控件使用 Font.Height=-13(约10pt),代码编辑器使用 Courier New
- **图标系统**: 
  - ImageListBig: 32×32 (主工具栏)
  - ImageListSmall: 16×16 (文件列表、菜单、动作)
  - ImageListToolbarRules: 13×13
  - ImageListToolbarFiles: 12×12
- **焦点指示**: 虚拟树控件隐藏焦点框,使用整行高亮选择
- **网格线**: 显示水平/垂直网格线,使用系统颜色

**交互规范**:
- 拖放支持: 从资源管理器拖放文件到主窗口、规则间拖放排序、文件间拖放排序
- 双击行为: 规则列表双击编辑、文件列表双击切换标记
- 快捷键: 严格遵循原版快捷键映射(见文档第1152-1215行)
- 行内编辑: F2 触发编辑 New Name 列,Enter 确认,Esc 取消

### III. 数据完整性 (Data Integrity)

**原则**: 文件操作必须安全可靠,数据状态始终一致。

**安全机制**:
- **预览验证**: 所有重命名操作前必须先预览,验证新名合法性
- **冲突检测**: 自动检测重复新名,提供修复建议(Shift+F)
- **撤销支持**: 保存 Old Path 列,支持一级撤销(Ctrl+Shift+Z)
- **回收站删除**: 删除文件操作必须使用系统回收站(SHFileOperation)
- **错误处理**: 重命名失败时记录错误信息到 Error 列,状态标记为失败

**状态管理**:
- State 列状态: 未处理 | 预览有效(绿✓) | 预览无效(红×) | 重命名成功(绿✓) | 重命名失败(红×)
- Mark 状态: 复选框勾选表示参与重命名,独立于 Select 状态
- 持久化: 窗口位置/大小、列宽、分隔条位置、规则预设必须保存到配置文件

### IV. 规则引擎架构 (Rule Engine Architecture)

**原则**: 规则系统必须可扩展、可测试、性能高效。

**架构要求**:
- **规则接口**: 所有规则类型实现统一的 `IRule` 接口(或基类)
- **Frame 封装**: 每个规则的 UI 配置封装为独立 Frame,通过 `TForm_AddRule` 动态加载
- **参数序列化**: 规则参数支持序列化为文本(用于预设导出、剪贴板操作)
- **顺序执行**: 规则按列表顺序依次应用,每条规则接收上一条的输出
- **启用/禁用**: 通过复选框控制规则是否参与计算

**规则类型**:
1. Replace(替换): 通配符模式、反向引用($n)、出现次数控制
2. Insert(插入): 6种位置模式、Meta Tag 支持
3. Delete(删除): From/Until 组合、分隔符/位置/计数模式
4. Remove(移除): 等同 Replace 的空替换,无反向引用
5. Case(大小写): 7种转换模式、强制片段大小写
6. Serialize(序列化): 编号系统、零填充、重置条件
7. RegEx(正则): 支持 $1..$9 子表达式引用
8. Extension(扩展名): 二进制签名检测、追加模式
9. Padding(填充): 数字序列零填充、文本填充
10. Strip(剥离): 6类字符集、Unicode 范围、反选模式
11. Clean Up(清理): 括号内容、字符替换空格、Unicode 处理
12. Transliterate(音译): 字母表前向/反向转换
13. Rearrange(重排): 分隔符/位置/模式拆分、$1..$N 重组
14. Reformat Date(日期): 多格式识别、自定义月份、时间调整
15. Randomize(随机化): 长度、唯一性、字符集、插入位置
16. PascalScript(脚本): 完整 Pascal 脚本支持、语法高亮、自动补全
17. User Input(用户输入): 多行输入、插入/替换模式
18. Mapping(映射): 表格映射、部分匹配、反向映射

### V. 性能与可扩展性 (Performance & Scalability)

**原则**: 应用必须能高效处理大量文件,保持响应性。

**性能要求**:
- **虚拟模式**: 文件列表使用 Virtual String Tree,支持百万级文件数
- **按需加载**: 仅计算可见节点的显示内容,通过 `OnGetText` 回调
- **批量操作**: 添加/删除大量文件时使用 `BeginUpdate/EndUpdate`
- **增量预览**: 仅对已标记(Checked)的文件应用规则
- **进度显示**: 预览/重命名过程在状态栏显示自绘进度条

**响应性**:
- 长时间操作(>100ms)必须显示进度条
- UI 线程不能阻塞,必要时使用后台线程(但文件系统操作需谨慎处理并发)
- 拖放、排序、滚动必须流畅(60fps)

### VI. 跨平台考虑 (Cross-Platform Awareness)

**原则**: 虽然当前聚焦 Windows,但架构应便于未来跨平台移植。

**技术选型**:
- **UI 框架**: 使用 Lazarus/Free Pascal + LCL(Lazarus Component Library)
  - 理由: 原版使用 Delphi,Lazarus 提供最佳兼容性,同时支持 Windows/Linux/macOS
  - LCL 提供原生控件包装,`TLazVirtualStringTree` 等价于 Delphi 的 `TVirtualStringTree`
- **文件系统**: 使用 LCL 的跨平台路径分隔符处理(DirectorySeparator)
- **Shell 集成**: Windows 特定功能(如文件关联、快捷方式)需条件编译({$IFDEF WINDOWS})
- **配置存储**: 使用 `TJSONConfig` 或 `TXMLConfig` 而非 Windows 注册表

**代码隔离**:
- 平台特定代码封装到独立单元(WinShellUtils.pas, UnixShellUtils.pas)
- 使用接口(IShellIntegration)提供统一 API

### VII. 测试驱动开发 (Test-Driven Development, NON-NEGOTIABLE)

**原则**: 所有功能必须先写测试,测试通过后才能合并代码。

**TDD 流程**:
1. **编写测试**: 根据需求文档定义测试用例(给定输入→期望输出)
2. **测试失败**: 运行测试,确认红色(未实现功能)
3. **实现功能**: 编写最小代码使测试通过
4. **测试通过**: 确认绿色
5. **重构**: 优化代码结构,保持测试通过

**测试范围**:
- **单元测试**: 每个规则类型的所有参数组合、边界条件、错误处理
- **集成测试**: 多规则组合、规则顺序、文件系统交互
- **UI 测试**: 控件交互、键盘快捷键、拖放操作(使用 GUI 测试框架)
- **回归测试**: 原版行为的端到端测试套件

**测试工具**:
- FPCUnit(Free Pascal 的单元测试框架)
- GUI 测试可使用 Sikuli/LazAutomate

### VIII. 代码质量标准 (Code Quality Standards)

**原则**: 代码必须清晰、可维护、遵循 Object Pascal 最佳实践。

**代码规范**:
- **命名约定**:
  - 类名: `TClassName`(T前缀)
  - 接口: `IInterfaceName`(I前缀)
  - 私有字段: `FFieldName`(F前缀)
  - 方法: PascalCase
  - 局部变量: camelCase 或 PascalCase(保持项目一致性)
- **注释**: 公共 API 必须有文档注释(使用 PasDoc 格式)
- **单一职责**: 每个类/方法只负责一件事
- **魔法数字**: 使用具名常量(如 `DEFAULT_RULE_PANEL_HEIGHT = 145`)

**文件组织**:
```
src/
├── Rules/            // 18 种规则类型实现
│   ├── RuleReplace.pas
│   ├── RuleInsert.pas
│   └── ...
├── Frames/           // 规则配置 Frame
│   ├── FrameRuleReplace.pas
│   ├── FrameRuleInsert.pas
│   └── ...
├── Forms/            // 主窗口、对话框
│   ├── FormMain.pas
│   ├── FormAddRule.pas
│   └── ...
├── Core/             // 核心逻辑
│   ├── FileManager.pas
│   ├── RuleEngine.pas
│   ├── PresetManager.pas
│   └── ...
├── Utils/            // 工具函数
│   ├── PathUtils.pas
│   ├── StringUtils.pas
│   └── ...
└── Platform/         // 平台特定代码
    ├── WinShellUtils.pas
    └── UnixShellUtils.pas
```

## Technical Constraints

### 技术栈

- **编程语言**: Object Pascal (Free Pascal Compiler 3.2.2+)
- **UI 框架**: Lazarus 3.0+ / LCL
- **构建系统**: fpcmake / lazbuild
- **版本控制**: Git
- **配置文件格式**: JSON (使用 fpjson 单元)

### 依赖库

**必需**:
- `LazVirtualStringTree`: 虚拟文件列表控件
- `SynEdit`: 代码编辑器(用于 PascalScript 规则)
- `PascalScript`: 嵌入式脚本引擎
- `fpjson`: JSON 解析
- `LCLIntf`: 跨平台接口封装

**可选**:
- `fpexif`: EXIF 日期提取(用于 Exif Date 列)
- `regexpr`: 正则表达式(TRegExpr 类)

### 构建与部署

- **目标平台**: Windows 10/11 x64 (首要),未来支持 Linux/macOS
- **发布模式**: 单个可执行文件 + 语言包目录(translations/)
- **安装程序**: 使用 Inno Setup 生成 Windows 安装包
- **自动更新**: 支持检查在线版本更新(可选功能)

## Development Workflow

### 分支策略

- `master`: 稳定版本,所有功能测试通过
- `develop`: 开发主线,每日集成
- `feature/XXX`: 功能分支,从 develop 拉取,完成后合并回 develop
- `hotfix/XXX`: 紧急修复,从 master 拉取,合并回 master 和 develop

### 提交规范

**Commit Message 格式**:
```
<type>(<scope>): <subject>

<body>

<footer>
```

**Type 类型**:
- feat: 新功能
- fix: 修复 bug
- refactor: 重构(不改变外部行为)
- test: 添加测试
- docs: 文档变更
- chore: 构建工具/依赖更新

**示例**:
```
feat(rules): implement Replace rule with wildcard support

- Add TRuleReplace class with parameters parsing
- Implement OnClick event for SpeedButton_ReplaceAddSeparator
- Add unit tests for all occurrence modes (All/First/Last)

Refs: #23
```

### Code Review 检查点

1. ✅ 测试已通过(单元测试 + 集成测试)
2. ✅ 代码符合规范(命名、注释、格式)
3. ✅ 对照需求文档验证功能完整性
4. ✅ 对照 review_checklist.md 检查细节
5. ✅ 无编译警告(使用 `-vw` 编译选项)
6. ✅ 内存泄漏检测通过(使用 HeapTrc)

## Quality Gates

### 发布前检查

**功能完整性**:
- [ ] 18 种规则类型全部实现
- [ ] 主界面所有菜单项/工具栏按钮可用
- [ ] 所有快捷键正常工作
- [ ] 拖放操作(文件/规则)正常
- [ ] 预设加载/保存/管理功能正常
- [ ] 导出/导入功能(批处理、剪贴板、列表文件)正常

**质量指标**:
- [ ] 单元测试覆盖率 ≥ 80%
- [ ] 所有关键路径有集成测试
- [ ] 原版行为测试套件 100% 通过
- [ ] 无内存泄漏(HeapTrc 报告清零)
- [ ] 性能测试: 10,000 文件列表操作 < 1s

**文档完整性**:
- [ ] 用户手册(User Manual)
- [ ] 快速入门(Quick Guide)
- [ ] API 文档(PasDoc 生成)
- [ ] CHANGELOG.md 更新

## Governance

### Constitution 优先级

- 本 Constitution 凌驾于所有其他开发实践和约定
- 任何与 Constitution 冲突的代码变更必须先修订 Constitution
- 修订 Constitution 需:
  1. 提出修订提案(Issue)
  2. 团队评审和讨论
  3. 多数同意后更新文档
  4. 记录修订历史(见下方)

### 合规性检查

- 所有 Pull Request 必须包含合规性检查清单
- Code Review 时验证是否符合 Constitution 原则
- 每月进行架构审查,确保代码库没有偏离 Constitution

### 异常处理

如果 Constitution 中的原则与实际需求冲突:
1. **不得** 直接违反原则
2. **必须** 先提出修订提案
3. 经团队批准后更新 Constitution
4. 记录决策理由和影响范围

---

**Version**: 1.0.0  
**Ratified**: 2026-02-12  
**Last Amended**: 2026-02-12  
**Amendment History**: 
- 2026-02-12: 初始版本,基于 ReNamer 7.8 反编译需求文档创建
