# ReNamer 逆向工程学习项目

## 概述
本项目通过逆向分析 ReNamer 7.8 的架构和设计模式，重新实现一个文件批量重命名工具。

## 原始程序信息
- **程序**: ReNamer 7.8
- **架构**: x86 (32-bit)
- **框架**: Lazarus/Free Pascal + LCL
- **脚本引擎**: PascalScript (RemObjects)
- **正则库**: TRegExpr

## 项目结构

```
project/
├── ReNamerClone.lpr      # 主程序入口
├── src/
│   ├── rules/            # 规则系统
│   │   ├── uRule.pas           # 规则基类和核心数据结构
│   │   ├── uRuleReplace.pas    # 替换规则
│   │   ├── uRuleRegEx.pas      # 正则表达式规则
│   │   └── uRuleRegistry.pas   # 规则注册管理器
│   └── forms/            # 窗体DFM文件
│       ├── Form_Main.dfm
│       ├── Frame_RuleReplace.dfm
│       └── ...
└── res/                  # 资源文件
```

## 核心架构

### 1. 规则系统 (策略模式)

```pascal
TRule (基类)
  ├── TRuleReplace     (查找替换)
  ├── TRuleRegEx       (正则表达式)
  ├── TRuleInsert      (插入文本)
  ├── TRuleDelete      (删除字符)
  ├── TRuleCase        (大小写转换)
  ├── TRuleSerialize   (序列化编号)
  ├── TRuleExtension   (扩展名处理)
  ├── TRulePadding     (数字填充)
  ├── TRuleStrip       (剥离字符)
  ├── TRuleCleanUp     (清理特殊字符)
  ├── TRuleTranslit    (音译转换)
  ├── TRuleRearrange   (重排字段)
  ├── TRuleReformatDate(日期格式化)
  ├── TRuleRandomize   (随机化)
  ├── TRulePascalScript(自定义脚本)
  └── TRuleUserInput   (用户输入)
```

### 2. 核心类

- **TRule**: 规则基类，所有规则必须实现 `Execute()` 方法
- **TRuleConfig**: 规则配置基类，用于序列化/反序列化
- **TRenFile**: 文件对象，包含原始名、新名、路径等信息
- **TRuleList**: 规则列表，管理规则的添加、删除、执行
- **TRenFileList**: 文件列表，管理文件的添加、预览、重命名

### 3. 执行流程

```
1. 添加文件到 TRenFileList
2. 添加规则到 TRuleList
3. Preview() - 对每个文件应用所有规则，计算新文件名
4. Validate() - 检测冲突（重名、非法字符、路径过长）
5. Rename() - 执行实际重命名操作
6. UndoRename() - 撤销重命名（如需要）
```

## 开发指南

### 环境要求
- Lazarus 2.2+ (推荐 3.0)
- Free Pascal Compiler 3.2+
- Windows 10/11

### 如何添加新规则

1. 创建新单元 `uRuleXxx.pas`
2. 定义配置类 `TRuleConfigXxx`
3. 实现规则类 `TRuleXxx`，继承 `TRule`
4. 实现 `Execute()` 方法
5. 在 `uRuleRegistry.pas` 中注册规则

### 示例：添加插入规则

```pascal
unit uRuleInsert;

type
  TRuleConfigInsert = class(TRuleConfig)
    property InsertText: string;
    property Position: Integer;  // 0=开头, -1=结尾
    property SkipExtension: Boolean;
  end;

  TRuleInsert = class(TRule)
  protected
    function GetRuleName: string; override;
  public
    function Execute(const AFileName: string; AFile: TRenFile): string; override;
  end;
```

## 学习要点

### 设计模式
- **策略模式**: 规则系统 - 每种规则是一个策略
- **工厂模式**: TRuleRegistry - 根据名称创建规则实例
- **命令模式**: Rename/UndoRename - 可逆操作
- **观察者模式**: 预览实时更新（待实现）

### 关键技术
- DFM 窗体序列化格式
- TRegExpr 正则表达式库
- PascalScript 脚本引擎集成
- VirtualTreeView 高性能列表控件

## 待实现功能

- [ ] 完整的主窗体 UI
- [ ] 更多规则类型实现
- [ ] 预设(Preset)保存/加载
- [ ] 冲突检测和自动修复
- [ ] 撤销历史记录
- [ ] 拖放文件支持
- [ ] 文件过滤器
- [ ] 元数据标签支持
- [ ] PascalScript 脚本规则
- [ ] 命令行模式

## 参考资料

- [Lazarus Wiki](https://wiki.lazarus.freepascal.org/)
- [TRegExpr Documentation](https://regex.sorokin.engineer/en/latest/)
- [PascalScript Documentation](https://wiki.remobjects.com/wiki/PascalScript)
