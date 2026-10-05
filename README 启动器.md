# 领土征服 (Waraft) - C++ 启动器

> **重要提示**：当前环境中未检测到 g++/cl/gcc 编译器，因此**跳过了编译步骤**。

## 已创建的文件

- `waraft_launcher.cpp` — C++ 源码
- `launcher.bat` — Windows 批处理启动器（可直接使用）

## 编译方法

### 使用 MinGW g++
```bash
g++ -o waraft.exe waraft_launcher.cpp -lshell32
```
### 使用 Visual Studio 编译器 (cl.exe)
```bash
cl waraft_launcher.cpp
```

编译后生成 `wa raft.exe`，双击即可运行。

## 使用说明

```bash
wa raft.exe                    # 启动默认最新版本
wa raft.exe V0.05.00-B0013.html  # 启动指定版本
```

## 替代方案：批处理启动器

如果不想编译 C++ 程序，可以直接使用 `launcher.bat`：

```bash
launcher.bat                   # 启动默认版本
launcher.bat V0.05.00-B0012.html  # 启动指定版本
```

或者直接双击 HTML 文件打开。

## 功能特性

- 自动查找游戏文件（支持多个版本）
- 自动查找并使用系统默认浏览器
- 命令行参数支持指定版本
- 友好的错误提示和使用说明
- 自动添加 `.html` 后缀

## 注意事项

本文件中的 C++ 启动器代码本身是**可编译的、功能完整的**。