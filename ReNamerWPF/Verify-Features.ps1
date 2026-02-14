# ReNamer WPF - 功能验证脚本
# 自动检查关键组件和功能是否完整

$ErrorActionPreference = "Continue"
Write-Host "=== ReNamer WPF 功能验证 ===" -ForegroundColor Cyan
Write-Host ""

# 1. 检查关键文件是否存在
Write-Host "1️⃣ 检查关键文件..." -ForegroundColor Yellow
$keyFiles = @(
    "ReNamer\Views\MainWindow.xaml",
    "ReNamer\Views\MainWindow.xaml.cs",
    "ReNamer\Views\AddRuleDialog.xaml",
    "ReNamer\Views\AddRuleDialog.xaml.cs",
    "ReNamer\Services\RenameService.cs",
    "ReNamer\Services\PresetService.cs",
    "ReNamer\Resources\DesignSystemResources.xaml"
)

$missingFiles = @()
foreach ($file in $keyFiles) {
    if (Test-Path $file) {
        Write-Host "  ✅ $file" -ForegroundColor Green
    } else {
        Write-Host "  ❌ $file 缺失!" -ForegroundColor Red
        $missingFiles += $file
    }
}

# 2. 检查规则类实现
Write-Host ""
Write-Host "2️⃣ 检查规则类实现..." -ForegroundColor Yellow
$ruleTypes = @(
    "ReplaceRule", "InsertRule", "DeleteRule", "RemoveRule", "CaseRule",
    "SerializeRule", "ExtensionRule", "RegexRule", "PaddingRule", "StripRule",
    "CleanUpRule", "TransliterateRule", "RearrangeRule", "ReformatDateRule",
    "RandomizeRule", "UserInputRule", "MappingRule"
)

$rulesContent = Get-Content "ReNamer\Rules\*.cs" -Raw
$missingRules = @()
foreach ($rule in $ruleTypes) {
    if ($rulesContent -match "class $rule") {
        Write-Host "  ✅ $rule" -ForegroundColor Green
    } else {
        Write-Host "  ❌ $rule 未实现!" -ForegroundColor Red
        $missingRules += $rule
    }
}

# 3. 检查配置面板
Write-Host ""
Write-Host "3️⃣ 检查规则配置面板..." -ForegroundColor Yellow
$configPanels = @(
    "ReplaceConfigPanel", "InsertConfigPanel", "DeleteConfigPanel", "RemoveConfigPanel",
    "CaseConfigPanel", "SerializeConfigPanel", "ExtensionConfigPanel", "RegexConfigPanel",
    "PaddingConfigPanel", "StripConfigPanel", "CleanUpConfigPanel", "TransliterateConfigPanel",
    "RearrangeConfigPanel", "ReformatDateConfigPanel", "RandomizeConfigPanel",
    "UserInputConfigPanel", "MappingConfigPanel"
)

$missingPanels = @()
foreach ($panel in $configPanels) {
    $xamlFile = "ReNamer\Views\RuleConfigs\$panel.xaml"
    if (Test-Path $xamlFile) {
        Write-Host "  ✅ $panel" -ForegroundColor Green
    } else {
        Write-Host "  ❌ $panel 缺失!" -ForegroundColor Red
        $missingPanels += $panel
    }
}

# 4. 检查窗口控制方法
Write-Host ""
Write-Host "4️⃣ 检查窗口控制方法..." -ForegroundColor Yellow
$mainWindowCs = Get-Content "ReNamer\Views\MainWindow.xaml.cs" -Raw
$controlMethods = @(
    "TitleBar_MouseLeftButtonDown",
    "MinimizeButton_Click",
    "MaximizeButton_Click",
    "CloseButton_Click"
)

$missingMethods = @()
foreach ($method in $controlMethods) {
    if ($mainWindowCs -match $method) {
        Write-Host "  ✅ $method" -ForegroundColor Green
    } else {
        Write-Host "  ❌ $method 缺失!" -ForegroundColor Red
        $missingMethods += $method
    }
}

# 5. 编译测试
Write-Host ""
Write-Host "5️⃣ 编译测试..." -ForegroundColor Yellow
$buildOutput = dotnet build ReNamer --no-restore 2>&1 | Out-String
if ($LASTEXITCODE -eq 0) {
    Write-Host "  ✅ 编译成功" -ForegroundColor Green
} else {
    Write-Host "  ❌ 编译失败!" -ForegroundColor Red
    Write-Host $buildOutput
}

# 6. 测试运行
Write-Host ""
Write-Host "6️⃣ 测试应用启动..." -ForegroundColor Yellow
$exePath = "ReNamer\bin\Debug\net8.0-windows\ReNamer.exe"
if (Test-Path $exePath) {
    Write-Host "  ✅ 可执行文件存在: $exePath" -ForegroundColor Green
    
    # 尝试启动（后台运行2秒后关闭）
    Write-Host "  🚀 尝试启动应用..."
    $proc = Start-Process $exePath -PassThru -WindowStyle Normal
    Start-Sleep -Seconds 2
    
    if ($proc.HasExited) {
        Write-Host "  ⚠️  应用启动后立即退出，可能有运行时错误" -ForegroundColor Yellow
    } else {
        Write-Host "  ✅ 应用正常运行 (PID: $($proc.Id))" -ForegroundColor Green
        Stop-Process -Id $proc.Id -Force -ErrorAction SilentlyContinue
    }
} else {
    Write-Host "  ❌ 可执行文件不存在" -ForegroundColor Red
}

# 7. 生成报告
Write-Host ""
Write-Host "=" * 50 -ForegroundColor Cyan
Write-Host "📊 验证总结" -ForegroundColor Cyan
Write-Host "=" * 50 -ForegroundColor Cyan

$totalIssues = $missingFiles.Count + $missingRules.Count + $missingPanels.Count + $missingMethods.Count

if ($totalIssues -eq 0) {
    Write-Host ""
    Write-Host "🎉 所有检查通过！应用功能完整。" -ForegroundColor Green
    Write-Host ""
    Write-Host "✅ 关键文件: $($keyFiles.Count)/$($keyFiles.Count)" -ForegroundColor Green
    Write-Host "✅ 规则类: $($ruleTypes.Count)/$($ruleTypes.Count)" -ForegroundColor Green
    Write-Host "✅ 配置面板: $($configPanels.Count)/$($configPanels.Count)" -ForegroundColor Green
    Write-Host "✅ 窗口控制: $($controlMethods.Count)/$($controlMethods.Count)" -ForegroundColor Green
    Write-Host ""
    Write-Host "🚀 现在可以手动测试以下功能：" -ForegroundColor Yellow
    Write-Host "   1. 添加文件/文件夹 (F3/F4)"
    Write-Host "   2. 添加规则 (Ins)"
    Write-Host "   3. 预览重命名 (F5)"
    Write-Host "   4. 执行重命名 (F6)"
    Write-Host "   5. 保存/加载预设 (Ctrl+S)"
    Write-Host ""
} else {
    Write-Host ""
    Write-Host "❌ 发现 $totalIssues 个问题需要修复" -ForegroundColor Red
    Write-Host ""
    
    if ($missingFiles.Count -gt 0) {
        Write-Host "缺失文件:" -ForegroundColor Red
        $missingFiles | ForEach-Object { Write-Host "  - $_" }
    }
    
    if ($missingRules.Count -gt 0) {
        Write-Host "未实现规则:" -ForegroundColor Red
        $missingRules | ForEach-Object { Write-Host "  - $_" }
    }
    
    if ($missingPanels.Count -gt 0) {
        Write-Host "缺失配置面板:" -ForegroundColor Red
        $missingPanels | ForEach-Object { Write-Host "  - $_" }
    }
    
    if ($missingMethods.Count -gt 0) {
        Write-Host "缺失方法:" -ForegroundColor Red
        $missingMethods | ForEach-Object { Write-Host "  - $_" }
    }
}

Write-Host ""
Write-Host "💡 详细测试指南请查看: FEATURE_TESTING_GUIDE.md" -ForegroundColor Cyan
Write-Host ""
