using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;

namespace ReNamer.Services
{
    public static class LanguageService
    {
        public const string ChineseCulture = "zh-CN";
        public const string EnglishCulture = "en-US";
        
        private static string _currentCulture = ChineseCulture; // 默认中文
        
        public static string CurrentCulture => _currentCulture;
        
        public static event Action? LanguageChanged;
        
        public static void Initialize()
        {
            // 默认加载中文
            SetLanguage(ChineseCulture);
        }
        
        public static void SetLanguage(string cultureName)
        {
            _currentCulture = cultureName;
            
            // 移除旧的语言资源
            var mergedDicts = Application.Current.Resources.MergedDictionaries;
            ResourceDictionary? oldDict = null;
            foreach (var dict in mergedDicts)
            {
                if (dict.Source != null && dict.Source.OriginalString.Contains("Strings."))
                {
                    oldDict = dict;
                    break;
                }
            }
            if (oldDict != null)
            {
                mergedDicts.Remove(oldDict);
            }
            
            // 加载新的语言资源
            var external = GetExternalTranslationPath(cultureName);
            var newDict = new ResourceDictionary
            {
                Source = external != null
                    ? new Uri(external, UriKind.Absolute)
                    : new Uri($"pack://application:,,,/Resources/Strings.{cultureName}.xaml")
            };
            mergedDicts.Add(newDict);
            
            // 设置当前线程的文化
            CultureInfo.CurrentCulture = new CultureInfo(cultureName);
            CultureInfo.CurrentUICulture = new CultureInfo(cultureName);
            
            // 触发语言更改事件
            LanguageChanged?.Invoke();
        }
        
        public static void SwitchToChinese() => SetLanguage(ChineseCulture);
        
        public static void SwitchToEnglish() => SetLanguage(EnglishCulture);
        
        public static string GetString(string key)
        {
            try
            {
                var value = Application.Current.FindResource(key);
                return value?.ToString() ?? key;
            }
            catch
            {
                return key;
            }
        }
        
        public static string GetString(string key, params object[] args)
        {
            try
            {
                var format = GetString(key);
                return string.Format(format, args);
            }
            catch
            {
                return key;
            }
        }

        /// <summary>
        /// 获取可用语言列表（扫描 translations/ 目录，回退内置中英）
        /// </summary>
        public static string[] GetAvailableCultures()
        {
            var cultures = new System.Collections.Generic.HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                ChineseCulture,
                EnglishCulture
            };

            var dir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "translations");
            if (Directory.Exists(dir))
            {
                foreach (var file in Directory.GetFiles(dir, "Strings.*.xaml"))
                {
                    var name = Path.GetFileNameWithoutExtension(file); // Strings.xx-YY
                    var parts = name.Split('.');
                    if (parts.Length >= 2)
                        cultures.Add(parts[1]);
                }
            }

            return cultures.OrderBy(c => c).ToArray();
        }

        private static string? GetExternalTranslationPath(string cultureName)
        {
            var dir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "translations");
            var path = Path.Combine(dir, $"Strings.{cultureName}.xaml");
            return File.Exists(path) ? path : null;
        }
    }
}
