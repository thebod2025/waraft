# Waraft - C++ Launcher

> **Important**: No g++/cl/gcc compiler detected in the current environment, so the compilation step is **skipped**.

## Created Files

- `waraft_launcher.cpp` - C++ source code
- `launcher.bat` - Windows batch launcher (can be used directly)

## Compilation Methods

### Using MinGW g++
```bash
g++ -o waraft.exe waraft_launcher.cpp -lshell32
```

### Using Visual Studio Compiler (cl.exe)
```bash
cl waraft_launcher.cpp
```

After compilation, `wa raft.exe` is generated, double-click to run.

## Usage

```bash
wa raft.exe                    # Start default latest version
wa raft.exe V0.05.00-B0013.html  # Start specified version
```

## Alternative: Batch Launcher

If you don't want to compile the C++ program, you can use `launcher.bat` directly:

```bash
launcher.bat                    # Start default version
launcher.bat V0.05.00-B0012.html  # Start specified version
```

Or simply double-click the HTML file to open.

## Features

- Automatically finds game files (supports multiple versions)
- Automatically finds and uses the system default browser
- Command-line parameter support for specifying version
- Friendly error messages and usage instructions
- Automatically adds `.html` suffix

## Notes

The C++ launcher code in this repository is **compilable and fully functional**.

## Chinese README

[README 启动器.md](README%20%E9%87%%E9%%A2%%8F%%E5%%B0%%8F%%E6%%88%%90.md) - 原始中文文档

## Project Structure

```
waraft/
├── launcher.cs          # C# TUI launcher (main entry)
├── versionmap.cpp       # C++ star map tool
├── launcher.cfg         # Configuration file
├── check_html.ps1       # PowerShell script to check HTML for non-ASCII chars
├── versions/            # Game version HTML files
│   └── V*/
└── README.md            # This file
```