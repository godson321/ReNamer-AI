# Quickstart: Rule Config Panel UI Redesign
## 1. 目标
快速完成并验证规则配置面板重设计，确保满足：
- 一致布局（Margin/标签宽度/行间距）
- 100% 输入提示（Placeholder）
- 0 硬编码用户文本
- 0 硬编码颜色
- 0 空 TextBlock 占位

## 2. 实施顺序
1. 基础设施（PlaceholderBehavior + 设计系统资源扩展 + 本地化补齐）
2. P0 面板优先（Strip）
3. 高频面板（Replace → Insert → Delete → Case → Serialize）
4. 其余面板一致性收敛
5. 全局静态检查 + 测试

## 3. 修改范围
- `ReNamerWPF/ReNamer/Helpers/PlaceholderBehavior.cs`（新增）
- `ReNamerWPF/ReNamer/Resources/DesignSystemResources.xaml`
- `ReNamerWPF/ReNamer/Resources/Strings.en-US.xaml`
- `ReNamerWPF/ReNamer/Resources/Strings.zh-CN.xaml`
- `ReNamerWPF/ReNamer/Views/RuleConfigs/*.xaml`
- 必要时的 `*.xaml.cs` 事件联动代码

## 4. 开发时检查命令（PowerShell）
### 编译与测试
```powershell
dotnet build .\ReNamerWPF\ReNamer.sln
dotnet test .\ReNamerWPF\ReNamer.sln
```

### 静态扫描（建议）
```powershell
# 空 TextBlock 占位
rg "Text=\"\""

# 常见硬编码颜色
rg "Foreground=\"Gray\"|Background=\"#[0-9A-Fa-f]{6}\"|Foreground=\"#[0-9A-Fa-f]{6}\"" .\ReNamerWPF\ReNamer\Views\RuleConfigs

# RuleConfig 面板中的潜在硬编码可见文本（需人工复核）
rg "Content=\"[^\{].+\"|Text=\"[^\{].+\"" .\ReNamerWPF\ReNamer\Views\RuleConfigs
```

## 5. 手动验收步骤
1. 启动应用并打开 AddRuleDialog。
2. 逐一切换全部规则面板，核对：
   - Margin 是否统一为 `16,12`
   - 标签列宽是否 80/100 且面板内部一致
   - 行距是否满足 8/16 节奏
3. 检查所有文本输入框：
   - 空时显示 Placeholder
   - 输入后 Placeholder 消失
4. 切换语言（中文/英文）：
   - Strip 与其他面板无硬编码残留
5. 切换主题（浅色/深色）：
   - 提示文字、只读框背景、分组边框保持可读
6. 拉伸和缩窄窗口：
   - 控件不重叠、不异常截断

## 6. 完成定义（Done）
- 满足 SC-001 ~ SC-007
- `dotnet build` 通过
- `dotnet test` 通过
- 代码审查确认无布局 hack 回归
