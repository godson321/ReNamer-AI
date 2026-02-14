# Implementation Plan: 003 Virtual File List

## Technology Stack

### Frontend
- Framework: WPF (.NET 8)
- Control: ListView (替代 TLazVirtualStringTree)
- Data Binding: MVVM 模式

### Key Components
- MainWindow.xaml(.cs) - 主窗口文件列表
- AppSettings.cs - 应用设置
- RenFile.cs - 文件模型

## Architecture

### Current Implementation Status
- ✅ 使用 ListView 实现虚拟文件列表
- ✅ 支持列显示/隐藏
- ✅ 支持排序
- ✅ 支持拖放
- ✅ 支持双击自适应
- ✅ 支持快捷键

### File Structure
```
ReNamerWPF/
  ReNamer/
    Views/
      MainWindow.xaml      - 主窗口
      MainWindow.xaml.cs   - 主窗口逻辑
    Models/
      RenFile.cs           - 文件模型
    Services/
      AppSettings.cs       - 应用设置
```

## Design Decisions

### Why ListView instead of VST?
- WPF 原生支持虚拟化
- ListView 更符合 WPF 编程模式
- 更易于维护和扩展

## Features Implemented

1. **21 列数据展示**
   - 文件名、新名称、大小、日期等

2. **交互功能**
   - 行内编辑 New Name
   - 拖放排序
   - 复选框标记

3. **排序过滤**
   - 多列排序
   - 动态列显示/隐藏

4. **状态可视化**
   - 图标区分重命名状态
   - 颜色标记

## Out of Scope
- 百万级文件性能优化（当前实现已满足常规使用）
- 自定义列功能扩展

## Dependencies
- Feature 001 (Rule Engine Core) - 规则引擎
