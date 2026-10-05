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

        // 设置
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
            public DateTime LastWriteTime;
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
            zh.Add("Spinner", "|/- spinner");
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
            zh.Add("LastWrite", "更新于: ");
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
            en.Add("Spinner", "|/- spinner");
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
            en.Add("LastWrite", "Last updated: ");
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
                        LastWriteTime = Directory.GetLastWriteTime(d),
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
                { key = Console.ReadKey(true); }
                catch
                { break; }
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
                        if (UseBlobUrl) { LaunchViaBrowser(Versions[idx].HtmlPath); }
                        else
                        {
                            try { Process.Start(Versions[idx].HtmlPath); }
                            catch (Exception ex) { Console.WriteLine("  [" + T("Error") + "] " + ex.Message); Thread.Sleep(1500); }
                        }
                        if (CloseAfterLaunch) return;
                    }
                }
                else if (k == ConsoleKey.Escape || (key.KeyChar == 'q') || (key.KeyChar == 'Q'))
                { ShowLoading(T("ShuttingDown"), pages); break; }
                else if (k == ConsoleKey.S)
                { ShowSettings(); pages = (Versions.Count + PageSize - 1) / PageSize; }
                else if (k == ConsoleKey.M)
                {
                    if (File.Exists(starMapPath))
                    { try { Process.Start(starMapPath, VersionsDir); }
                      catch (Exception ex) { Console.WriteLine("  [" + T("Error") + "] " + ex.Message); Thread.Sleep(1500); } }
                    else { Console.WriteLine("  [" + T("Error") + "] versionmap.exe not found"); Thread.Sleep(1500); }
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
                    Console.WriteLine("  >> " + ver.DisplayName + " [" + T("VersionCreated") + ver.CreationTime.ToString("yyyy-MM-dd") + " " + T("LastWrite") + ver.LastWriteTime.ToString("HH:mm:ss") + "]");
                    Console.ResetColor();
                }
                else
                { Console.WriteLine("     " + ver.DisplayName + " [" + T("VersionCreated") + ver.CreationTime.ToString("yyyy-MM-dd") + " " + T("LastWrite") + ver.LastWriteTime.ToString("HH:mm:ss") + "]"); }
            }
            int nextPage = Page < pages - 1 ? Page + 2 : 1;
            Console.WriteLine("  " + string.Format(T("NextPage"), nextPage) + (Page == pages - 1 ? T("Wrap") : ""));
            Console.WriteLine("------------------------------------------------------");
            Console.WriteLine(string.Format("  [↑/↓] {0}  [Enter] {1}  [Esc/Q] {2}  [S] {3}  [M] {4}",
                T("Move"), T("Launch"), T("Quit"), T("Settings"), T("StarMap")));
            Console.WriteLine("======================================================");
        }

        static void ShowLoading(string label, int pages)
        { /* same */ }
        static void LaunchViaBrowser(string filePath) { /* same */ }
        static void ShowSettings() { /* same */ }
        static string[] BuildSettingsItems() { /* same */ }
        static void EditSettingValue(int idx) { /* same */ }
        static int ReadNumberInput() { /* same */ }
        static bool? ReadBoolInput() { /* same */ }
        static void AdjustSetting(int idx, bool increase) { /* same */ }
        static void LoadConfig() { /* same */ }
        static void SaveConfig() { /* same */ }
    }
}
