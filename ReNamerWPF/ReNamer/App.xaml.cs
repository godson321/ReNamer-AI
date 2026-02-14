using System.Windows;
using ReNamer.Services;

namespace ReNamer;

public partial class App : Application
{
    private void Application_Startup(object sender, StartupEventArgs e)
    {
        // 初始化语言服务（默认中文）
        LanguageService.Initialize();
    }
}
