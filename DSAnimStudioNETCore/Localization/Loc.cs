using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace DSAnimStudio.Localization
{
    /// <summary>
    /// 界面语言。
    /// </summary>
    public enum LocLanguage
    {
        ChineseSimplified = 0,
        English = 1,
    }

    /// <summary>
    /// DS Anim Studio 界面本地化核心。
    ///
    /// 设计要点：
    ///  1. 代码里所有面向用户的字符串都走 <see cref="T(string)"/>（或 <see cref="TFormat"/>），
    ///     英文原文作为 key；运行时按当前语言查表得到译文。
    ///  2. ImGui 的 "##id"/"###id" 后缀是控件身份标识，必须原样保留，
    ///     否则改语言会让窗口/控件 ID 变化、配置持久化失效。见 <see cref="SplitIdSuffix"/>。
    ///  3. 词条来自内嵌资源（EmbRes/Lang/zh-Hans.json），也可被程序目录下
    ///     的 Lang/zh-Hans.json 覆盖，方便用户自行改词。
    ///  4. 查表结果做缓存（原文 -> 译文），每帧几十万次调用也不会有性能问题。
    ///  5. 语言状态保存在 DSAnimStudio_Config.json 的 UILanguage 字段里。
    /// </summary>
    public static class Loc
    {
        public const string EmbeddedChineseResource = "/EmbRes/Lang/zh-Hans.json";
        public const string ExternalChineseRelativePath = @"Lang\zh-Hans.json";

        private static volatile LocLanguage _language = LocLanguage.ChineseSimplified;

        /// <summary>当前语言。中文为默认。</summary>
        public static LocLanguage Language
        {
            get => _language;
            set
            {
                if (_language == value)
                    return;
                _language = value;
                ClearCaches();
                try { LanguageChanged?.Invoke(); } catch { }
            }
        }

        /// <summary>语言变化事件（用于重建 ImGui 字体、重载布局）。</summary>
        public static event Action LanguageChanged;

        public static bool IsChinese => _language == LocLanguage.ChineseSimplified;

        private static readonly object _lock = new object();

        /// <summary>英文原文 -> 简体中文。key 为 null 或空时忽略。</summary>
        private static Dictionary<string, string> _zhTable = new Dictionary<string, string>(StringComparer.Ordinal);
        private static bool _tablesLoaded = false;

        /// <summary>完整字符串缓存（含 ##/### 后缀的原文）。</summary>
        private static readonly ConcurrentDictionary<string, string> _cacheFull = new ConcurrentDictionary<string, string>(StringComparer.Ordinal);

        /// <summary>剥掉 ##/### 后缀后的可见文本缓存。</summary>
        private static readonly ConcurrentDictionary<string, string> _cacheText = new ConcurrentDictionary<string, string>(StringComparer.Ordinal);

        private static void ClearCaches()
        {
            _cacheFull.Clear();
            _cacheText.Clear();
        }

        /// <summary>
        /// 加载词条表。先读程序目录 Lang/zh-Hans.json（若存在），否则读内嵌资源。
        /// </summary>
        public static void EnsureLoaded()
        {
            if (_tablesLoaded)
                return;
            lock (_lock)
            {
                if (_tablesLoaded)
                    return;
                try
                {
                    string json = null;
                    try
                    {
                        var external = Path.Combine(AppDirectory, ExternalChineseRelativePath);
                        if (File.Exists(external))
                            json = File.ReadAllText(external, Encoding.UTF8);
                    }
                    catch
                    {
                        json = null;
                    }

                    if (string.IsNullOrWhiteSpace(json))
                    {
                        try
                        {
                            json = ReadEmbeddedText(EmbeddedChineseResource);
                        }
                        catch
                        {
                            json = null;
                        }
                    }

                    if (!string.IsNullOrWhiteSpace(json))
                    {
                        var parsed = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
                        if (parsed != null)
                        {
                            var dict = new Dictionary<string, string>(StringComparer.Ordinal);
                            foreach (var kvp in parsed)
                            {
                                if (string.IsNullOrEmpty(kvp.Key))
                                    continue;
                                dict[kvp.Key] = kvp.Value ?? string.Empty;
                            }
                            _zhTable = dict;
                        }
                    }
                }
                catch
                {
                    _zhTable = new Dictionary<string, string>(StringComparer.Ordinal);
                }
                finally
                {
                    _tablesLoaded = true;
                    ClearCaches();
                }
            }
        }

        /// <summary>程序 exe 所在目录。</summary>
        public static string AppDirectory
        {
            get
            {
                try
                {
                    var loc = Assembly.GetExecutingAssembly().Location;
                    if (!string.IsNullOrEmpty(loc))
                        return Path.GetDirectoryName(loc);
                }
                catch
                {
                }
                return AppContext.BaseDirectory;
            }
        }

        private static string ReadEmbeddedText(string resourcePath)
        {
            resourcePath = resourcePath?.Trim().Trim('/');
            resourcePath = $"{nameof(DSAnimStudio)}.{resourcePath.Replace("/", ".")}";
            var ass = Assembly.GetExecutingAssembly();
            using (Stream stream = ass.GetManifestResourceStream(resourcePath))
            {
                if (stream == null)
                    return null;
                using (var reader = new StreamReader(stream, Encoding.UTF8))
                {
                    return reader.ReadToEnd();
                }
            }
        }

        /// <summary>
        /// ImGui 标识后缀分隔符。返回可见文本与后缀。
        /// </summary>
        public static void SplitIdSuffix(string s, out string visible, out string suffix)
        {
            visible = s;
            suffix = string.Empty;
            if (string.IsNullOrEmpty(s))
                return;

            int idx = -1;
            for (int i = 0; i < s.Length - 1; i++)
            {
                if (s[i] == '#' && s[i + 1] == '#')
                {
                    idx = i;
                    break;
                }
            }

            if (idx < 0)
                return;

            visible = s.Substring(0, idx);
            suffix = s.Substring(idx);
        }

        /// <summary>查表：只对可见文本部分查，保留 ##/### 后缀。</summary>
        public static string T(string english)
        {
            if (string.IsNullOrEmpty(english))
                return english;

            if (_language == LocLanguage.English)
                return english;

            if (_cacheFull.TryGetValue(english, out var cached))
                return cached;

            EnsureLoaded();

            SplitIdSuffix(english, out var visible, out var suffix);

            string result = english;
            if (visible.Length == 0)
            {
                // 纯 "##id" / "###id"，无可见文本，原样返回
                result = english;
            }
            else
            {
                string translated = LookupVisible(visible);
                result = translated == null ? english : translated + suffix;
            }

            _cacheFull[english] = result;
            return result;
        }

        /// <summary>查表：只对可见文本部分查，不带后缀（用于纯文本场景，和 Tooltip 等）。</summary>
        public static string TText(string english)
        {
            if (string.IsNullOrEmpty(english))
                return english;
            if (_language == LocLanguage.English)
                return english;
            if (_cacheText.TryGetValue(english, out var cached))
                return cached;

            EnsureLoaded();
            var translated = LookupVisible(english) ?? english;
            _cacheText[english] = translated;
            return translated;
        }

        private static string LookupVisible(string visible)
        {
            if (string.IsNullOrEmpty(visible))
                return null;
            if (_zhTable.TryGetValue(visible, out var exact))
                return exact;

            // 精确匹配失败时，去掉首尾空白再试一次（很多条目是 "  Tab：" 这种缩进文本，
            // 直接按整串匹配会失配，留白交给调用方保持原样）。
            var trimmed = visible.Trim();
            if (trimmed.Length != visible.Length && trimmed.Length > 0 && _zhTable.TryGetValue(trimmed, out var trimmedHit))
            {
                int lead = visible.Length - visible.TrimStart().Length;
                int trail = visible.Length - visible.TrimEnd().Length;
                return new string(' ', lead) + trimmedHit + new string(' ', trail);
            }

            return null;
        }

        /// <summary>带占位符的格式化文本。参数为 {0} {1} ...</summary>
        public static string TFormat(string englishFormat, params object[] args)
        {
            if (string.IsNullOrEmpty(englishFormat))
                return englishFormat;
            var fmt = T(englishFormat);
            if (args == null || args.Length == 0)
                return fmt;
            try
            {
                return string.Format(fmt, args);
            }
            catch
            {
                return fmt;
            }
        }

        /// <summary><see cref="TFormat"/> 的短别名，插值字符串改写后使用。</summary>
        public static string TF(string englishFormat, params object[] args)
            => TFormat(englishFormat, args);

        /// <summary>
        /// ImGui 窗口标题。返回 "<译文>###<英文原名>" 或 "<英文原名>###<英文原名>"。
        ///
        /// 为什么要拼 "###原名"：ImGui 里 "###" 之后的文本才是窗口的身份标识，
        /// "###" 之前的可见部分不参与 ID 计算。因此两种语言下窗口 ID 恒等于
        /// CRC32("###原名")，切换语言不会让停靠布局（imgui.ini 里的 DockId /
        /// Selected）失效，而标题栏依旧显示本地化文本。
        /// </summary>
        public static string Win(string english)
        {
            if (string.IsNullOrEmpty(english))
                return english;

            string visible;
            if (_language == LocLanguage.English)
            {
                visible = english;
            }
            else
            {
                EnsureLoaded();
                var hit = LookupVisible(english);
                visible = hit ?? english;
            }

            // 已经自带 ### 的不重复拼接
            if (english.IndexOf("###", StringComparison.Ordinal) >= 0)
                return visible;

            return visible + "###" + english;
        }

        /// <summary>
        /// 从 <see cref="Win"/> 生成的标题里取回语言无关的稳定标识（"###" 之后的英文原名）。
        /// 用于把「窗口是否打开」这类状态存进配置文件，切换语言后依然能对上。
        /// </summary>
        public static string StableKey(string title)
        {
            if (string.IsNullOrEmpty(title))
                return title;
            int idx = title.IndexOf("###", StringComparison.Ordinal);
            return idx < 0 ? title : title.Substring(idx + 3);
        }

        /// <summary>对内嵌/外部词条做一次统计，用于自检。</summary>
        public static int EntryCount
        {
            get
            {
                EnsureLoaded();
                return _zhTable.Count;
            }
        }

        /// <summary>遍历所有词条（自检/导出用）。</summary>
        public static IEnumerable<KeyValuePair<string, string>> Entries
        {
            get
            {
                EnsureLoaded();
                return _zhTable.ToArray();
            }
        }

        /// <summary>
        /// 列出所有词条里出现的全部字符，用于构建 ImGui 字形范围。
        /// </summary>
        public static string GetAllTranslatedCharacters()
        {
            EnsureLoaded();
            var sb = new StringBuilder(1 << 20);
            foreach (var kvp in _zhTable)
            {
                sb.Append(kvp.Value);
                sb.Append(kvp.Key);
            }
            return sb.ToString();
        }
    }
}
