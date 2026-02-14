# ReNamer WPF 现代化界面预览

**更新时间**: 2026-02-13（与当前实现对齐）

## 🎨 设计对比

### 原 Delphi 设计 vs 现代 WPF 设计

| 组件 | Delphi 原版 | 现代 WPF 版 |
|------|------------|------------|
| **窗口样式** | 系统标题栏 | 自定义标题栏（Fluent Design） |
| **主操作区** | ToolBar (36px) | CommandBar (64px) |
| **配色方案** | 系统颜色 | Fluent 配色（#0078D4 主色） |
| **按钮高度** | 不统一 | 48px（触摸友好）|
| **卡片设计** | 平面 | 阴影层次（Elevation）|
| **列表行高** | 默认 | 40px（现代间距）|
| **图标** | 装饰性箭头 | 语义化 Path 图标 |

---

## 🏗️ 界面结构

```
┌────────────────────────────────────────────────────┐
│ 🔵 自定义标题栏 (40px)                              │ ← Fluent 蓝色
│  ReNamer    [语言]             — □ ✕              │
├────────────────────────────────────────────────────┤
│ 菜单栏: 文件 | 预设 | 设置 | 帮助                │
├────────────────────────────────────────────────────┤
│ 🎯 CommandBar (64px) - 主操作区                   │
│  [📁 添加文件] [📂 添加文件夹] | [👁 预览] [🔄 重命名]│ ← 强调按钮
├────────────────────────────────────────────────────┤
│                                                    │
│  ┌─────────────────────────────────────────────┐  │
│  │ 规则面板 (卡片样式 + 阴影)                   │  │
│  │ ┌─ 工具栏 ────────────────────────────────┐ │  │
│  │ │ [+ 添加] [- 删除] | [↑] [↓]             │ │  │
│  │ ├──────────────────────────────────────────┤ │  │
│  │ │ ☑ | Insert      | 插入文本 "prefix_"    │ │  │
│  │ │ ☑ | Delete      | 删除 1-3 个字符      │ │  │
│  │ └──────────────────────────────────────────┘ │  │
│  └─────────────────────────────────────────────┘  │
│                                                    │
│  ═══════════ 可拖动分隔线 ═══════════              │
│                                                    │
│  ┌─────────────────────────────────────────────┐  │
│  │ 文件列表面板 (卡片样式 + 阴影)               │  │
│  │ ┌─ 工具栏 ────────────────────────────────┐ │  │
│  │ │ [📄 文件] [🔍 过滤] [⚙ 选项] [导出] [分析] │ │  │
│  │ │ [列] [Validate]                           │ │  │
│  │ ├──────────────────────────────────────────┤ │  │
│  │ │☑│ ✓ │ photo.jpg    │photo_001.jpg │...  │ │  │
│  │ │☑│ → │ document.pdf │document.pdf  │...  │ │  │
│  │ │☐│ × │ video.mp4    │video.mp4     │...  │ │  │
│  │ └──────────────────────────────────────────┘ │  │
│  └─────────────────────────────────────────────┘  │
├────────────────────────────────────────────────────┤
│ 📊 状态栏: Files: 125 | Marked: 45 | Selected: 3  │
└────────────────────────────────────────────────────┘
```

---

## ✨ 核心设计改进

### 1️⃣ 自定义标题栏
```xaml
<!-- Fluent Design 风格 -->
<Border Background="#0078D4" Height="40" CornerRadius="8,8,0,0">
  <Grid>
    <TextBlock Text="ReNamer" Foreground="White"/>
    <StackPanel> <!-- 窗口控制按钮 -->
      <Button>—</Button> <!-- 最小化 -->
      <Button>□</Button> <!-- 最大化 -->
      <Button>✕</Button> <!-- 关闭：Hover 红色 #E81123 -->
    </StackPanel>
  </Grid>
</Border>
```

**特性**：
- ✅ 拖动标题栏移动窗口
- ✅ 双击标题栏最大化/还原
- ✅ 关闭按钮 Hover 显示红色背景
- ✅ 圆角边框（8px）

---

### 2️⃣ CommandBar（主操作栏）

**高度**: 64px（增大触摸目标）  
**按钮设计**:
```xaml
<Button Height="48" Padding="16,0">
  <StackPanel Orientation="Horizontal">
    <Path Data="{StaticResource Icon_AddFile}" Fill="{StaticResource PrimaryBrush}"/>
    <TextBlock Text="添加文件" Margin="8,0,0,0"/>
  </StackPanel>
</Button>
```

**重命名按钮**（强调）:
```xaml
<Button Background="#0078D4" Foreground="White">
  <Button.Effect>
    <DropShadowEffect ShadowDepth="2" BlurRadius="8"/>
  </Button.Effect>
  <!-- 内容 -->
</Button>
```

---

### 3️⃣ 卡片样式面板

**规则面板 / 文件列表**:
```xaml
<Border Style="{StaticResource CardStyle}" Margin="16">
  <!-- CardStyle 包含：
       - Background: White
       - CornerRadius: 8
       - DropShadowEffect (Elevation1)
  -->
</Border>
```

**工具栏**:
- 背景：`#FAFAFA`
- 高度：48px
- 按钮：圆角 4px，Hover 灰色背景

---

### 4️⃣ 列表项样式

**高度**: 40px（符合现代间距规范）  
**交互状态**:
```css
默认:   背景: White
悬停:   背景: #F3F3F3 (SurfaceHoverColor)
选中:   背景: #E3F2FD (PrimaryLightColor)
        前景: #0078D4 (PrimaryColor)
```

**状态指示器**（✓ × →）:
```xaml
<TextBlock FontSize="16" FontWeight="Bold">
  <!-- ✓ 成功 → #4CAF50 -->
  <!-- × 失败 → #F44336 -->
  <!-- → 待定 → #0078D4 -->
</TextBlock>
```

---

### 5️⃣ 状态栏

**设计**:
- 背景：`#FAFAFA`
- 高度：32px
- 文字颜色：`#757575`（次要文本）
- 圆角：`0,0,8,8`（与窗口底部匹配）

---

## 🎯 设计原则

### Fluent 设计系统
1. **深度感知**（Depth）
   - 使用阴影创建层次（层级 1-4）
   - 卡片悬浮在背景之上

2. **流畅动画**（Motion）
   - 按钮 Hover 过渡：200ms cubic-bezier(0.4, 0.0, 0.2, 1)
   - 未来支持：页面切换动画

3. **材质**（Material）
   - 亚克力/云母效果（可选，需 Windows 11）

4. **缩放**（Scale）
   - 8px 基础间距
   - 最小触摸目标：44x44px

---

## 🚀 使用说明

当前现代化界面已集成到 `Views/MainWindow.xaml` 与 `Views/MainWindow.xaml.cs`，无需额外替换文件。

---

## 📊 性能优化

### 虚拟化列表
```xaml
<ListView VirtualizingPanel.IsVirtualizing="True"
          VirtualizingPanel.VirtualizationMode="Recycling">
  <!-- 大数据集时提升性能 -->
</ListView>
```

### 硬件加速
```xaml
<Window RenderOptions.BitmapScalingMode="HighQuality"
        RenderOptions.EdgeMode="Aliased">
  <!-- 阴影/圆角使用 GPU 渲染 -->
</Window>
```

---

## 🔮 未来增强

### 阶段 2 计划
- [x] **暗色主题**（Dark Mode）
- [ ] **亚克力背景**（Acrylic Material）
- [ ] **流畅动画**（页面切换）
- [ ] **自适应布局**（响应式设计）
- [ ] **触摸手势**（平板支持）

### 技术栈升级
- [ ] **WinUI 3**（长期目标）
- [ ] **MVVM Toolkit**（社区版本）

---

## 📸 设计预览

### 配色方案
```
主色:       #0078D4 (Fluent 蓝)
成功:       #4CAF50 (Material 绿)
错误:       #F44336 (Material 红)
警告:       #FF9800 (Material 橙)

背景:       #FFFFFF (白色)
表面:       #FAFAFA (浅灰)
边框:       #E0E0E0 (边框灰)

正文:       #212121 (深灰)
次要:       #757575 (次要灰)
```

### 间距系统
```
XS:  4px
S:   8px  ← 基础单位
M:  16px  (2x)
L:  24px  (3x)
XL: 32px  (4x)
```

### 阴影层次
```
层级1: 0 2px  4px rgba(0,0,0,0.12)  ← 卡片
层级2: 0 4px  8px rgba(0,0,0,0.16)  ← 悬浮按钮
层级3: 0 8px 16px rgba(0,0,0,0.20)  ← 对话框
层级4: 0 16px 24px rgba(0,0,0,0.24) ← 模态层
```

---

## 📝 注意事项

### ⚠️ 当前限制
1. **自定义标题栏**需要 `WindowStyle="None"`，可能影响 Aero Snap
   - 解决方案：手动实现窗口吸附逻辑

2. **圆角窗口** + **AllowsTransparency** 会影响性能
   - 建议：仅在高性能设备上启用

3. **阴影效果**在低 DPI 屏幕上可能模糊
   - 解决方案：检测 DPI 并动态调整

### ✅ 兼容性
- Windows 10 1809+
- .NET 6.0+
- WPF 6.0+

---

## 🎓 参考资料

- [Fluent Design System](https://www.microsoft.com/design/fluent/)
- [Material Design 3](https://m3.material.io/)
- [WPF 性能优化](https://learn.microsoft.com/wpf/advanced/optimizing-performance)

---

**最后更新**: 2026-02-13  
**设计师**: AI Assistant + 用户需求  
**版本**: 1.0 Modern Preview
