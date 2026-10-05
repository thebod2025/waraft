#include <windows.h>
#include <conio.h>
#include <shlwapi.h>
#include <vector>
#include <string>
#include <algorithm>
#include <fstream>

#pragma comment(lib, "Shlwapi.lib")

static const int MAX_VERSIONS = 20;

struct Version {
    int build;
    std::string name;
    std::string dir;
};

static Version versions[MAX_VERSIONS];
static int versionCount = 0;

struct Change {
    int minBuild, maxBuild;
    std::string summary;
    std::string detail;
};

static const Change changes[] = {
    { 7, 8,   "初始版本", "战争系统上线\n- 宣战、站队、战争倒计时\n- 战争评分系统\n- 阵营关系影响战争" },
    { 8, 9,   "战争扩展", "战争倒计时机制\n- 预写评论系统\n- 战争防御/进攻选择\n- 退出战争功能" },
    { 9, 10,  "平衡调整", "优化战争机制\n- 调整关系衰减速度\n- 战争损失计算优化\n- UI 响应优化" },
    { 10, 11, "性能优化", "性能与稳定性提升\n- 土地计算重构\n- 营队 AI 优化\n- 缓存重建机制" },
    { 11, 12, "地图升级", "10000² 大地图\n- 领土征服模式\n- 更多国家与阵营\n- 新 UI 配色方案" },
    { 12, 13, "Bug 修复", "修复关键问题\n- 修复 SyntaxError 语法错误\n- 修复 file:// 跨域加载\n- 修复弹窗关闭异常\n- 按钮点击事件修复" },
    { 13, 14, "新功能", "版本 V0.05.01-B0014\n- 继承 B0013 全部特性\n- 进一步优化加载\n- 启动器支持 blob URL" }
};

static int curNode = 0;
static int curChange = 0;

static void render() {
    HANDLE hOut = GetStdHandle(STD_OUTPUT_HANDLE);
    CONSOLE_SCREEN_BUFFER_INFO csbi;
    GetConsoleScreenBufferInfo(hOut, &csbi);
    int width = csbi.srWindow.Right - csbi.srWindow.Left;
    int height = csbi.srWindow.Bottom - csbi.srWindow.Top;

    COORD coord = {0, 0};
    SetConsoleCursorPosition(hOut, coord);

    char buf[4096];
    int off = 0;

    // 标题
    off += sprintf(buf + off, "╔══════════════════════════════════════════════════════════╗\n");
    off += sprintf(buf + off, "║              版本星图 (Version Star Map)                 ║\n");
    off += sprintf(buf + off, "╚══════════════════════════════════════════════════════════╝\n");
    off += sprintf(buf + off, "\n");

    // 版本节点
    off += sprintf(buf + off, "  版本节点:\n");
    int nodeWidth = 18;
    int cols = (width - 2) / nodeWidth;
    if (cols < 1) cols = 1;
    int rows = (versionCount + cols - 1) / cols;
    for (int r = 0; r < rows; r++) {
        for (int c = 0; c < cols; c++) {
            int idx = r * cols + c;
            if (idx >= versionCount) break;
            bool selected = (idx == curNode);
            char nodeMark = selected ? '●' : '○';
            const char* ver = versionCount > 0 ? versions[idx].name.c_str() : "空";
            off += sprintf(buf + off, "    [%c] %-14s", nodeMark, ver);
        }
        off += sprintf(buf + off, "\n");
    }
    off += sprintf(buf + off, "\n");

    // 变更说明
    if (versionCount > 0) {
        int idx = (curNode >= 0 && curNode < versionCount) ? curNode : 0;
        int build = versions[idx].build;
        off += sprintf(buf + off, "  版本 %s 变更详情:\n", versions[idx].name.c_str());
        off += sprintf(buf + off, "  %s\n", versions[idx].dir.c_str());
        off += sprintf(buf + off, "  ────────────────────────────────────────────────\n");

        for (int i = 0; i < sizeof(changes) / sizeof(changes[0]); i++) {
            int chMin = changes[i].minBuild;
            int chMax = changes[i].maxBuild;
            bool show = false;
            if (chMin <= build && build < chMax) show = true;
            bool active = (i == curChange);
            if (show) {
                char mark = active ? '>' : ' ';
                off += sprintf(buf + off, "    [%c] %s\n", mark, changes[i].summary.c_str());
                off += sprintf(buf + off, "        %s\n", changes[i].detail.c_str());
            }
        }
    }
    off += sprintf(buf + off, "\n");
    off += sprintf(buf + off, "  方向键移动节点 | 回车查看详情 | Q 退出\n");

    WriteConsoleA(hOut, buf, (DWORD)off, NULL, NULL);
}

static std::string extractBuild(const std::string& name) {
    size_t p = name.find_last_of('-');
    if (p != std::string::npos && name.rfind("B", p) == p) {
        std::string num = name.substr(p + 1);
        if (!num.empty()) {
            try {
                return std::to_string(std::stoi(num));
            } catch (...) {
                return "0";
            }
        }
    }
    return "0";
}

static bool hasHtml(const std::string& dir) {
    std::string pat = dir + "\\*.html";
    WIN32_FIND_DATAA fd;
    HANDLE h = FindFirstFileA(pat.c_str(), &fd);
    if (h != INVALID_HANDLE_VALUE) {
        FindClose(h);
        return true;
    }
    return false;
}

static void loadVersions(const std::string& root) {
    std::string pat = root + "\\versions\\*";
    WIN32_FIND_DATAA fd;
    HANDLE h = FindFirstFileA(pat.c_str(), &fd);
    if (h == INVALID_HANDLE_VALUE) return;

    do {
        if (fd.dwFileAttributes & FILE_ATTRIBUTE_DIRECTORY) {
            if (strcmp(fd.cFileName, ".") == 0 || strcmp(fd.cFileName, "..") == 0)
                continue;
            if (hasHtml(std::string(fd.cFileName))) {
                if (versionCount < MAX_VERSIONS) {
                    versions[versionCount].name = fd.cFileName;
                    versions[versionCount].dir = fd.cFileName;
                    versions[versionCount].build = std::stoi(extractBuild(fd.cFileName));
                    versionCount++;
                }
            }
        }
    } while (FindNextFileA(h, &fd));
    FindClose(h);

    std::sort(versions, versions + versionCount,
              [](const Version& a, const Version& b) { return a.build < b.build; });

    curChange = 0;
    for (int i = 0; i < (int)sizeof(changes) / sizeof(changes[0]); i++) {
        if (versionCount > 0 && changes[i].minBuild <= versions[0].build)
            curChange = i;
    }
}

static void mainLoop(const std::string& root) {
    loadVersions(root);
    if (versionCount == 0) {
        printf("没有找到包含 HTML 的版本目录\n");
        printf("按任意键退出...\n");
        _getch();
        return;
    }

    while (1) {
        render();
        if (_kbhit()) {
            char key = _getch();
            if (key == 224 || key == 225) {
                key = _getch();
                if (key == 72) { // Up
                    curNode = (curNode - 1 + versionCount) % versionCount;
                    int build = versions[curNode].build;
                    curChange = 0;
                    for (int i = 0; i < (int)sizeof(changes) / sizeof(changes[0]); i++) {
                        if (changes[i].minBuild <= build && build < changes[i].maxBuild)
                            curChange = i;
                    }
                } else if (key == 80) { // Down
                    curNode = (curNode + 1) % versionCount;
                    int build = versions[curNode].build;
                    curChange = 0;
                    for (int i = 0; i < (int)sizeof(changes) / sizeof(changes[0]); i++) {
                        if (changes[i].minBuild <= build && build < changes[i].maxBuild)
                            curChange = i;
                    }
                }
            } else if (key == 13) { // Enter
                if (versionCount > 0) {
                    printf("\n");
                    printf("╔══════════════════════════════════════════════════════════╗\n");
                    printf("║        版本 %s 详细变更 (Build %d)                        ║\n",
                           versions[curNode].name.c_str(), versions[curNode].build);
                    printf("╚══════════════════════════════════════════════════════════╝\n");
                    printf("\n");
                    for (int i = 0; i < sizeof(changes) / sizeof(changes[0]); i++) {
                        if (changes[i].minBuild <= versions[curNode].build &&
                            versions[curNode].build < changes[i].maxBuild) {
                            printf("  [%s] %s\n", changes[i].summary.c_str(), changes[i].detail.c_str());
                            printf("\n");
                        }
                    }
                    printf("按任意键返回...\n");
                    _getch();
                }
            } else if (key == 'q' || key == 'Q') {
                break;
            }
        }
        Sleep(50);
    }
}

int WINAPI WinMain(HINSTANCE hInstance, HINSTANCE hPrevInstance, LPSTR lpCmdLine, int nCmdShow) {
    int argc = __argc;
    LPWSTR* argv = __wargv;

    std::string root = ".";
    if (argc > 1) {
        root = (const char*)argv[1];
    }

    while (!IsDebuggerPresent()) {
        mainLoop(root);
    }

    return 0;
}

int main(int argc, char** argv) {
    WinMain(0, 0, argv[1] ? (LPSTR)argv[1] : NULL, SW_SHOWDEFAULT);
    return 0;
}
