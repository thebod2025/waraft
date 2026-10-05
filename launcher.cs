using System;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Reflection;

namespace WaraftLauncher
{
    class Program
    {
        static int Cursor;
        static int Page;
        const int PageSize = 5;
        static List<VersionInfo> Versions = new List<VersionInfo>();
        static string VersionsDir;

        // Settings
        static int LoadingDurationMs = 800;
        static ConsoleColor LoadingColor = ConsoleColor.Cyan;
        static bool UseSpinner = false;
        static string Language = "zh";
        static bool CloseAfterLaunch = true;
        static bool UseBlobUrl = true;
        static string ConfigPath;

        class VersionInfo
        {
            public string DirName;
            public string DisplayName;
            public DateTime CreationTime;
            public string HtmlPath;
        }

        static Dictionary<string, Dictionary<string, string>> Texts;

        static string T(string key)
        {
            return Texts[Language][key];
        }

        static void InitTexts()
        {
            Texts = new Dictionary<string, Dictionary<string, string>>();

            var zh = new Dictionary<string, string>();
            zh.Add("Title", "Waraft  TUI  Launcher");
            zh.Add("Page", "页");
            zh.Add("Total", "共");
            zh.Add("Versions", "个版本");
            zh.Add("Time", "时间");
            zh.Add("PrevPage", "<< page{0} <<");
            zh.Add("NextPage", ">> page{0} >>");
            zh.Add("Wrap", " (循环)");
            zh.Add("Move", "移动");
            zh.Add("Launch", "启动");
            zh.Add("Quit", "退出");
            zh.Add("Settings", "设置");
            zh.Add("StarMap", "星图");
            zh.Add("Loading", "启动中");
            zh.Add("ShuttingDown", "关闭中");
            zh.Add("Goodbye", "再见。");
            zh.Add("SettingsTitle", "设置");
            zh.Add("Select", "选择");
            zh.Add("Adjust", "调整");
            zh.Add("Confirm", "确认");
            zh.Add("Cancel", "取消");
            zh.Add("SaveReturn", "保存并返回");
            zh.Add("LoadingDuration", "加载动画时长 (ms)");
            zh.Add("LoadingColorLabel", "加载条颜色");
            zh.Add("ProgressStyle", "进度样式");
            zh.Add("LoadMode", "加载方式");
            zh.Add("LanguageLabel", "语言");
            zh.Add("CloseAfterLaunchLabel", "启动后关闭启动器");
            zh.Add("Spinner", "|-/ spinner");
            zh.Add("Blocks", "填方块 ███");
            zh.Add("BlobUrl", "blob URL");
            zh.Add("DirectFile", "直接 file://");
            zh.Add("Chinese", "中文");
            zh.Add("English", "English");
            zh.Add("Yes", "是");
            zh.Add("No", "否");
            zh.Add("EnterNumber", "输入数字 (Shift+数字键)，回车确认: ");
            zh.Add("EnterBool", "输入 T/F (Shift+T/F)，回车确认: ");
            zh.Add("InvalidInput", "无效输入");
            zh.Add("OpenedInBrowser", "已在浏览器中打开: ");
            zh.Add("Error", "错误");
            zh.Add("VersionCreated", "制作时间: ");
            Texts.Add("zh", zh);

            var en = new Dictionary<string, string>();
            en.Add("Title", "Waraft  TUI  Launcher");
            en.Add("Page", "Page");
            en.Add("Total", "Total");
            en.Add("Versions", "versions");
            en.Add("Time", "Time");
            en.Add("PrevPage", "<< page{0} <<");
            en.Add("NextPage", ">> page{0} >>");
            en.Add("Wrap", " (wrap)");
            en.Add("Move", "Move");
            en.Add("Launch", "Launch");
            en.Add("Quit", "Quit");
            en.Add("Settings", "Settings");
            en.Add("StarMap", "Star Map");
            en.Add("Loading", "Launching");
            en.Add("ShuttingDown", "Shutting down");
            en.Add("Goodbye", "Goodbye.");
            en.Add("SettingsTitle", "Settings");
            en.Add("Select", "Select");
            en.Add("Adjust", "Adjust");
            en.Add("Confirm", "Confirm");
            en.Add("Cancel", "Cancel");
            en.Add("SaveReturn", "Save & Return");
            en.Add("LoadingDuration", "Loading Duration (ms)");
            en.Add("LoadingColorLabel", "Loading Color");
            en.Add("ProgressStyle", "Progress Style");
            en.Add("LoadMode", "Load Mode");
            en.Add("LanguageLabel", "Language");
            en.Add("CloseAfterLaunchLabel", "Close After Launch");
            en.Add("Spinner", "|-/ spinner");
            en.Add("Blocks", "Blocks ███");
            en.Add("BlobUrl", "blob URL");
            en.Add("DirectFile", "Direct file://");
            en.Add("Chinese", "中文");
            en.Add("English", "English");
            en.Add("Yes", "Yes");
            en.Add("No", "No");
            en.Add("EnterNumber", "Enter number (Shift+digit), press Enter: ");
            en.Add("EnterBool", "Enter T/F (Shift+T/F), press Enter: ");
            en.Add("InvalidInput", "Invalid input");
            en.Add("OpenedInBrowser", "Opened in browser: ");
            en.Add("Error", "Error");
            en.Add("VersionCreated", "Created: ");
            Texts.Add("en", en);
        }

        static void Main(string[] args)
        {
            InitTexts();

            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            if (args.Length > 0 && Directory.Exists(args[0]))
            {
                baseDir = args[0];
            }
            VersionsDir = Path.Combine(baseDir, "versions");
            ConfigPath = Path.Combine(baseDir, "launcher.cfg");
            string starMapPath = Path.Combine(baseDir, "versionmap.exe");

            LoadConfig();

            if (!Directory.Exists(VersionsDir))
            {
                Console.WriteLine("[ERROR] versions folder not found: " + VersionsDir);
                Console.ReadLine();
                return;
            }

            foreach (string d in Directory.GetDirectories(VersionsDir))
            {
                string[] htmlFiles = Directory.GetFiles(d, "*.html");
                if (htmlFiles.Length > 0)
                {
                    var info = new VersionInfo
                    {
                        DirName = Path.GetFileName(d),
                        DisplayName = Path.GetFileName(d),
                        CreationTime = Directory.GetCreationTime(d),
                        HtmlPath = htmlFiles[0]
                    };
                    Versions.Add(info);
                }
            }
            Versions.Sort((a, b) => a.CreationTime.CompareTo(b.CreationTime));

            if (Versions.Count == 0)
            {
                Console.WriteLine("[ERROR] No version directories with HTML found in " + VersionsDir);
                Console.ReadLine();
                return;
            }

            int pages = (Versions.Count + PageSize - 1) / PageSize;
            Cursor = 0;
            Page = 0;

            ShowLoading(T("Loading"), pages);

            bool interactive = !Console.IsOutputRedirected;

            while (true)
            {
                if (interactive) Render(pages);
                ConsoleKeyInfo key;
                try
                {
                    key = Console.ReadKey(true);
                }
                catch
                {
                    break;
                }
                ConsoleKey k = key.Key;

                if (k == ConsoleKey.UpArrow)
                {
                    if (Cursor > 0) Cursor--;
                    else if (Page > 0) { Page--; Cursor = PageSize - 1; }
                    else { Page = pages - 1; Cursor = Math.Min(PageSize - 1, Versions.Count - Page * PageSize - 1); }
                }
                else if (k == ConsoleKey.DownArrow)
                {
                    int itemsOnPage = Math.Min(PageSize, Versions.Count - Page * PageSize);
                    if (Cursor < itemsOnPage - 1) Cursor++;
                    else if (Page < pages - 1) { Page++; Cursor = 0; }
                    else { Page = 0; Cursor = 0; }
                }
                else if (k == ConsoleKey.Enter)
                {
                    int idx = Page * PageSize + Cursor;
                    if (idx < Versions.Count)
                    {
                        ShowLoading(T("Loading"), pages);
                        if (UseBlobUrl)
                        {
                            LaunchViaBrowser(Versions[idx].HtmlPath);
                        }
                        else
                        {
                            try { Process.Start(Versions[idx].HtmlPath); }
                            catch (Exception ex) { Console.WriteLine("  [" + T("Error") + "] " + ex.Message); Thread.Sleep(1500); }
                        }
                        if (CloseAfterLaunch) return;
                    }
                }
                else if (k == ConsoleKey.Escape || (key.KeyChar == 'q') || (key.KeyChar == 'Q'))
                {
                    ShowLoading(T("ShuttingDown"), pages);
                    break;
                }
                else if (k == ConsoleKey.S)
                {
                    ShowSettings();
                    pages = (Versions.Count + PageSize - 1) / PageSize;
                }
                else if (k == ConsoleKey.M)
                {
                    if (File.Exists(starMapPath))
                    {
                        try
                        {
                            Process.Start(starMapPath, VersionsDir);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("  [" + T("Error") + "] " + ex.Message);
                            Thread.Sleep(1500);
                        }
                    }
                    else
                    {
                        Console.WriteLine("  [" + T("Error") + "] versionmap.exe not found");
                        Thread.Sleep(1500);
                    }
                }
            }

            Console.WriteLine();
            Console.WriteLine("  " + T("Goodbye"));
        }

        static void Render(int pages)
        {
            Console.Clear();
            Console.WriteLine("======================================================");
            Console.WriteLine("                " + T("Title"));
            Console.WriteLine("======================================================");
            string timeStr = DateTime.Now.ToString("HH:mm:ss");
            Console.WriteLine(string.Format("    {0} {1}/{2}    {3} {4} {5}    {6}: {7}",
                T("Page"), Page + 1, pages, T("Total"), Versions.Count, T("Versions"), T("Time"), timeStr));
            Console.WriteLine("------------------------------------------------------");

            int prevPage = Page > 0 ? Page : pages - 1;
            Console.WriteLine("  " + string.Format(T("PrevPage"), prevPage + 1) + (Page == 0 ? T("Wrap") : ""));

            int start = Page * PageSize;
            int end = Math.Min(start + PageSize, Versions.Count);
            for (int i = start; i < end; i++)
            {
                int lc = i - start;
                var ver = Versions[i];
                if (lc == Cursor)
                {
                    Console.ForegroundColor = LoadingColor;
                    Console.WriteLine("  >> " + ver.DisplayName + "  [" + T("VersionCreated") + ver.CreationTime.ToString("yyyy-MM-dd HH:mm") + "]");
                    Console.ResetColor();
                }
                else
                {
                    Console.WriteLine("     " + ver.DisplayName + "  [" + T("VersionCreated") + ver.CreationTime.ToString("yyyy-MM-dd HH:mm") + "]");
                }
            }

            int nextPage = Page < pages - 1 ? Page + 2 : 1;
            Console.WriteLine("  " + string.Format(T("NextPage"), nextPage) + (Page == pages - 1 ? T("Wrap") : ""));

            Console.WriteLine("------------------------------------------------------");
            Console.WriteLine(string.Format("  [↑/↓] {0}  [Enter] {1}  [Esc/Q] {2}  [S] {3}  [M] {4}",
                T("Move"), T("Launch"), T("Quit"), T("Settings"), T("StarMap")));
            Console.WriteLine("======================================================");
        }

        static void ShowLoading(string label, int pages)
        {
            Console.Clear();
            Console.WriteLine("======================================================");
            Console.WriteLine("                " + T("Title"));
            Console.WriteLine("======================================================");
            Console.WriteLine("");
            Console.Write("  " + label + " ");
            int barWidth = 20;
            int steps = UseSpinner ? 20 : barWidth;
            int delay = LoadingDurationMs / steps;

            if (UseSpinner)
            {
                char[] spinner = { '|', '/', '-', '\\' };
                for (int i = 0; i < steps; i++)
                {
                    Console.ForegroundColor = LoadingColor;
                    Console.Write("\b" + spinner[i % 4]);
                    Console.ResetColor();
                    Thread.Sleep(delay);
                }
                Console.Write("\b ");
            }
            else
            {
                Console.Write("[");
                for (int i = 0; i < barWidth; i++)
                {
                    Console.ForegroundColor = LoadingColor;
                    Console.Write("█");
                    Console.ResetColor();
                    Thread.Sleep(delay);
                }
                Console.Write("]");
            }
            Console.WriteLine();
            Console.WriteLine("======================================================");
            Thread.Sleep(200);
        }

        static void LaunchViaBrowser(string filePath)
        {
            try
            {
                string content = File.ReadAllText(filePath, Encoding.UTF8);
                string tempPath = Path.Combine(Path.GetTempPath(), "waraft_" + Guid.NewGuid().ToString("N") + ".html");
                File.WriteAllText(tempPath, content, new UTF8Encoding(false));
                Process.Start(tempPath);
                Console.WriteLine("  " + T("OpenedInBrowser") + Path.GetFileName(filePath));
                Thread.Sleep(1200);
            }
            catch (Exception ex)
            {
                Console.WriteLine("  [" + T("Error") + "] " + ex.Message);
                Thread.Sleep(1500);
            }
        }

        static void ShowSettings()
        {
            int sel = 0;

            while (true)
            {
                Console.Clear();
                Console.WriteLine("======================================================");
                Console.WriteLine("                " + T("SettingsTitle"));
                Console.WriteLine("======================================================");

                string[] items = BuildSettingsItems();
                for (int i = 0; i < items.Length; i++)
                {
                    if (i == sel) { Console.ForegroundColor = LoadingColor; Console.WriteLine("  >> " + items[i]); Console.ResetColor(); }
                    else Console.WriteLine("     " + items[i]);
                }
                Console.WriteLine("------------------------------------------------------");
                Console.WriteLine(string.Format("  [↑/↓] {0}  [←/→] {1}  [Enter] {2}  [Esc] {3}  [Shift+数字/字母] 输入值",
                    T("Select"), T("Adjust"), T("Confirm"), T("Cancel")));
                Console.WriteLine("======================================================");

                var key = Console.ReadKey(true);
                if (key.Key == ConsoleKey.UpArrow) { sel = (sel - 1 + items.Length) % items.Length; }
                else if (key.Key == ConsoleKey.DownArrow) { sel = (sel + 1) % items.Length; }
                else if (key.Key == ConsoleKey.LeftArrow || key.Key == ConsoleKey.RightArrow)
                {
                    AdjustSetting(sel, key.Key == ConsoleKey.RightArrow);
                }
                else if (key.Key == ConsoleKey.Enter)
                {
                    if (sel == items.Length - 1) { SaveConfig(); break; }
                    else { EditSettingValue(sel); }
                }
                else if (key.Key == ConsoleKey.Escape) { break; }
            }
        }

        static string[] BuildSettingsItems()
        {
            if (Language == "zh")
            {
                return new[] {
                    T("LoadingDuration") + ": " + LoadingDurationMs,
                    T("LoadingColorLabel") + ": " + LoadingColor,
                    T("ProgressStyle") + ": " + (UseSpinner ? T("Spinner") : T("Blocks")),
                    T("LoadMode") + ": " + (UseBlobUrl ? T("BlobUrl") : T("DirectFile")),
                    T("LanguageLabel") + ": " + (Language == "zh" ? T("Chinese") : T("English")),
                    T("CloseAfterLaunchLabel") + ": " + (CloseAfterLaunch ? T("Yes") : T("No")),
                    T("SaveReturn")
                };
            }
            else
            {
                return new[] {
                    T("LoadingDuration") + ": " + LoadingDurationMs,
                    T("LoadingColorLabel") + ": " + LoadingColor,
                    T("ProgressStyle") + ": " + (UseSpinner ? T("Spinner") : T("Blocks")),
                    T("LoadMode") + ": " + (UseBlobUrl ? T("BlobUrl") : T("DirectFile")),
                    T("LanguageLabel") + ": " + (Language == "zh" ? T("Chinese") : T("English")),
                    T("CloseAfterLaunchLabel") + ": " + (CloseAfterLaunch ? T("Yes") : T("No")),
                    T("SaveReturn")
                };
            }
        }

        static void EditSettingValue(int idx)
        {
            Console.Clear();
            Console.WriteLine("======================================================");
            Console.WriteLine("                " + T("SettingsTitle"));
            Console.WriteLine("======================================================");

            switch (idx)
            {
                case 0: // LoadingDurationMs - number input
                    Console.Write(T("EnterNumber"));
                    int newDuration = ReadNumberInput();
                    if (newDuration >= 100 && newDuration <= 5000)
                    {
                        LoadingDurationMs = newDuration;
                    }
                    else
                    {
                        Console.WriteLine("  " + T("InvalidInput") + " (100-5000)");
                        Thread.Sleep(1000);
                    }
                    break;
                case 1: // LoadingColor - cycle colors
                    var colors = Enum.GetValues(typeof(ConsoleColor));
                    int ci = Array.IndexOf(colors, LoadingColor);
                    ci = (ci + 1) % colors.Length;
                    LoadingColor = (ConsoleColor)colors.GetValue(ci);
                    break;
                case 2: // ProgressStyle
                    UseSpinner = !UseSpinner;
                    break;
                case 3: // LoadMode
                    UseBlobUrl = !UseBlobUrl;
                    break;
                case 4: // Language
                    Language = Language == "zh" ? "en" : "zh";
                    break;
                case 5: // CloseAfterLaunch - boolean input
                    Console.Write(T("EnterBool"));
                    bool? newBool = ReadBoolInput();
                    if (newBool.HasValue)
                    {
                        CloseAfterLaunch = newBool.Value;
                    }
                    else
                    {
                        Console.WriteLine("  " + T("InvalidInput"));
                        Thread.Sleep(1000);
                    }
                    break;
            }
        }

        static int ReadNumberInput()
        {
            string input = "";
            while (true)
            {
                var key = Console.ReadKey(true);
                if (key.Key == ConsoleKey.Enter)
                {
                    if (int.TryParse(input, out int val))
                        return val;
                    return -1;
                }
                else if (key.Key == ConsoleKey.Backspace)
                {
                    if (input.Length > 0)
                    {
                        input = input.Substring(0, input.Length - 1);
                        Console.Write("\b \b");
                    }
                }
                else if (key.KeyChar >= '0' && key.KeyChar <= '9')
                {
                    input += key.KeyChar;
                    Console.Write(key.KeyChar);
                }
            }
        }

        static bool? ReadBoolInput()
        {
            string input = "";
            while (true)
            {
                var key = Console.ReadKey(true);
                if (key.Key == ConsoleKey.Enter)
                {
                    if (input.Equals("T", StringComparison.OrdinalIgnoreCase))
                        return true;
                    if (input.Equals("F", StringComparison.OrdinalIgnoreCase))
                        return false;
                    return null;
                }
                else if (key.Key == ConsoleKey.Backspace)
                {
                    if (input.Length > 0)
                    {
                        input = input.Substring(0, input.Length - 1);
                        Console.Write("\b \b");
                    }
                }
                else if (key.KeyChar == 'T' || key.KeyChar == 't' || key.KeyChar == 'F' || key.KeyChar == 'f')
                {
                    input = key.KeyChar.ToString().ToUpper();
                    Console.Write(key.KeyChar.ToString().ToUpper());
                }
            }
        }

        static void AdjustSetting(int idx, bool increase)
        {
            switch (idx)
            {
                case 0:
                    LoadingDurationMs += increase ? 100 : -100;
                    if (LoadingDurationMs < 100) LoadingDurationMs = 100;
                    if (LoadingDurationMs > 5000) LoadingDurationMs = 5000;
                    break;
                case 1:
                    var colors = Enum.GetValues(typeof(ConsoleColor));
                    int ci = Array.IndexOf(colors, LoadingColor);
                    ci = (ci + (increase ? 1 : -1) + colors.Length) % colors.Length;
                    LoadingColor = (ConsoleColor)colors.GetValue(ci);
                    break;
                case 2:
                    UseSpinner = !UseSpinner;
                    break;
                case 3:
                    UseBlobUrl = !UseBlobUrl;
                    break;
                case 4:
                    Language = Language == "zh" ? "en" : "zh";
                    break;
                case 5:
                    CloseAfterLaunch = !CloseAfterLaunch;
                    break;
            }
        }

        static void LoadConfig()
        {
            if (!File.Exists(ConfigPath)) return;
            try
            {
                var lines = File.ReadAllLines(ConfigPath);
                foreach (var line in lines)
                {
                    var parts = line.Split('=');
                    if (parts.Length != 2) continue;
                    string k = parts[0].Trim(), v = parts[1].Trim();
                    if (k == "LoadingDurationMs") int.TryParse(v, out LoadingDurationMs);
                    else if (k == "LoadingColor") Enum.TryParse(v, out LoadingColor);
                    else if (k == "UseSpinner") bool.TryParse(v, out UseSpinner);
                    else if (k == "UseBlobUrl") bool.TryParse(v, out UseBlobUrl);
                    else if (k == "Language") Language = v;
                    else if (k == "CloseAfterLaunch") bool.TryParse(v, out CloseAfterLaunch);
                }
            } catch { }
        }

        static void SaveConfig()
        {
            try
            {
                var lines = new[]
                {
                    "LoadingDurationMs=" + LoadingDurationMs,
                    "LoadingColor=" + LoadingColor,
                    "UseSpinner=" + UseSpinner,
                    "UseBlobUrl=" + UseBlobUrl,
                    "Language=" + Language,
                    "CloseAfterLaunch=" + CloseAfterLaunch
                };
                File.WriteAllLines(ConfigPath, lines);
            } catch { }
        }
    }
}