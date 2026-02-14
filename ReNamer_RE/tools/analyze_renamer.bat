@echo off
REM Ghidra 自动分析 ReNamer.exe

set GHIDRA_HOME=E:\90 AI\项目\ReNamer_RE\tools\ghidra_11.2.1_PUBLIC
set TARGET_EXE=E:\软件\文件重命名工具(ReNamer)7.8\ReNamer.exe
set PROJECT_DIR=E:\90 AI\项目\ReNamer_RE\ghidra_project
set PROJECT_NAME=ReNamer_Analysis

echo ==========================================
echo    ReNamer 逆向分析工具
echo ==========================================
echo.
echo 目标文件: %TARGET_EXE%
echo 项目目录: %PROJECT_DIR%
echo.

if not exist "%PROJECT_DIR%" mkdir "%PROJECT_DIR%"

echo [1] 启动 Ghidra GUI 进行交互式分析
echo [2] 运行无头分析 (headless) 并导出函数列表
echo [3] 退出
echo.
set /p choice="请选择 (1/2/3): "

if "%choice%"=="1" (
    echo 启动 Ghidra...
    start "" "%GHIDRA_HOME%\ghidraRun.bat"
    echo.
    echo === 手动操作步骤 ===
    echo 1. File - New Project - Non-Shared Project
    echo 2. 选择目录: %PROJECT_DIR%
    echo 3. 项目名称: %PROJECT_NAME%
    echo 4. File - Import File - 选择 ReNamer.exe
    echo 5. 双击导入的文件开始分析
    echo 6. 在 Symbol Tree 中浏览: Classes, Functions, Exports
)

if "%choice%"=="2" (
    echo 运行无头分析...
    "%GHIDRA_HOME%\support\analyzeHeadless.bat" ^
        "%PROJECT_DIR%" "%PROJECT_NAME%" ^
        -import "%TARGET_EXE%" ^
        -postScript "ExportFunctionInfo.java" ^
        -scriptPath "%GHIDRA_HOME%\Ghidra\Features\Base\ghidra_scripts"
    echo 分析完成！
)

pause
