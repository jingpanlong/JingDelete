# Jing Delete / 景·除

![banner](screenshots/banner.png)

[![最新发布](https://img.shields.io/github/v/release/jingpanlong/JingDelete?label=latest)](https://github.com/jingpanlong/JingDelete/releases)
[![总下载量](https://img.shields.io/github/downloads/jingpanlong/JingDelete/total)](https://github.com/jingpanlong/JingDelete/releases)
[![平台](https://img.shields.io/badge/platform-Windows-blue)](https://github.com/jingpanlong/JingDelete)
[![.NET](https://img.shields.io/badge/.NET-Framework%204.7.2%2B-512BD4)](https://docs.microsoft.com/dotnet/framework/)
[![许可证](https://img.shields.io/github/license/jingpanlong/JingDelete)](LICENSE)

**Jing Delete（景·除）** 是一款 Windows 本地文件与软件清理工具：文件删除、软件卸载、残留清理、磁盘空间整理、启动项管理。一切都在本机完成——**无网络访问、无遥测**。涉及危险操作（删除、清理、权限变更）一律需要显式确认。

> 作者：微信公众号「景同志」、小红书「景同学」。软件「关于」页内置 GitHub 仓库链接与下载入口。

---

## 快速下载

| 版本 | 说明 | 下载 |
| --- | --- | --- |
| 便携版（单文件） | `JingChu.exe`——解压即用，依赖系统 .NET Framework | [JingChu.exe](https://github.com/jingpanlong/JingDelete/releases/latest/download/JingChu.exe) |
| 安装版 | `JingChu-Setup.exe`——当前用户安装，含开始菜单与卸载入口 | [JingChu-Setup.exe](https://github.com/jingpanlong/JingDelete/releases/latest/download/JingChu-Setup.exe) |

历史版本与校验值可在 [Releases](https://github.com/jingpanlong/JingDelete/releases) 页面查看。

> 本程序未进行代码签名，Windows 可能提示“未知发布者”。本版本不申请也不伪造签名；如需验证，请比对发布页上的 SHA-256。

---

## 核心功能

![核心功能](screenshots/features.png)

1. **文件操作**——拖拽 / 选择器 / 粘贴路径；长路径、结尾点号或空格、Unicode 文件名处理；删除前预览、占用与锁定检测、同卷重命名与移动。
2. **软件卸载**——读取经典桌面卸载注册表项，并启动各程序自带的卸载器；Ctrl/Shift 多选与批量排队，统一给出卸载后的注册表候选清单。
3. **残留清理**——对顶级 SOFTWARE 子键、卸载项、常见目录第一层做关键词扫描；结果默认不勾选；修改注册表前始终先备份。
4. **磁盘空间**——四类扫描：陈旧临时文件、空文件夹、大文件、零字节文件；结果可导出 CSV。
5. **启动项管理**——注册表 Run 键与当前用户/公共「启动」文件夹统一查看；启动文件夹项可禁用（含备份）与恢复。
6. **备份与日志**——修改前备份注册表，可恢复、可导入；每日日志、实时结果与今日日志均可导出。
7. **本地优先**——无网络、无遥测；备份与日志均保存在本机。
8. **开源**——源码开放，欢迎 Issue 与 Pull Request。

---

## 目录

- [截图](#截图)
- [构建](#构建)
- [已实现功能](#已实现功能)
- [使用流程](#使用流程)
- [数据与要求](#数据与要求)
- [能力边界](#能力边界)
- [测试](#测试)
- [文件结构](#文件结构)
- [开源与贡献](#开源与贡献)
- [许可证](#许可证)

---

## 截图

Jing Delete（景·除）真实界面截图：

| 文件处理 | 软件卸载 |
| --- | --- |
| ![文件处理](screenshots/01-%E6%96%87%E4%BB%B6%E5%A4%84%E7%90%86.png) | ![软件卸载](screenshots/02-%E8%BD%AF%E4%BB%B6%E5%8D%B8%E8%BD%BD.png) |

| 残留检查 | 空间整理 |
| --- | --- |
| ![残留检查](screenshots/03-%E6%AE%8B%E7%95%99%E6%A3%80%E6%9F%A5.png) | ![空间整理](screenshots/04-%E7%A9%BA%E9%97%B4%E6%95%B4%E7%90%86.png) |

| 开机启动 | 备份日志 |
| --- | --- |
| ![开机启动](screenshots/05-%E5%BC%80%E6%9C%BA%E5%90%AF%E5%8A%A8.png) | ![备份日志](screenshots/06-%E5%A4%87%E4%BB%BD%E6%97%A5%E5%BF%97.png) |

---

## 构建

Windows 10/11，.NET Framework 4.7.2 或更高。无需 NuGet 包，也无需 .NET SDK。

在本目录下运行 Windows PowerShell：

```powershell
powershell.exe -NoProfile -ExecutionPolicy Bypass -File .\build.ps1
```

输出：`bin\JingChu.exe`、`bin\JingChu-Setup.exe`。两者均为单文件；便携版依赖系统 .NET Framework，而非内置整个运行时。安装版内嵌编译后的程序，并将自身复制为卸载程序。

构建使用 Windows Framework 目录下的 C# 编译器。当 4.7.2 参考程序集存在时，使用其中的 Microsoft.VisualBasic；否则回退到 Framework 目录下的程序集。源码附带 `app.ico`；`create-icon.ps1` 可重新生成。

## 已实现功能

1. 多文件/文件夹拖拽；文件选择器、文件夹选择器、手动输入路径。
2. 目标列表、复选框处理、从列表移除、逐条状态反馈。
3. 文件/目录/链接属性查看；递归文件/目录计数与体积。
4. 回收站删除；永久删除；Unicode、长路径、结尾点号或空格文件名处理。
5. 删除前清除只读/隐藏/系统属性；管理员可逐对象选择性取得所有权并授予当前用户权限。
6. 同卷重命名/移动，不覆盖已有目标。
7. Restart Manager 占用检测；请求优雅关闭；确认后终止非系统进程并校验进程创建时间。
8. 管理员可将本地目标（子在前、父在后）加入 Windows 重启删除队列，无需自动重启。
9. 桌面软件卸载项扫描、名称/发布者搜索、启动注册卸载器、打开安装目录。
10. 残留候选关键词扫描：常见目录第一层、SOFTWARE 直接子键、卸载项。
11. 残留默认不勾选；清理前逐条确认；修改注册表前强制备份。
12. 32/64 位注册表视图备份与导入；备份清单与目录项。
13. 当前用户/本机 Run 启动清单；禁用前备份，可从备份恢复。
14. 每日日志、实时结果、日志目录、今日日志导出。
15. 后台执行、取消剩余步骤、管理员重启、系统目录拦截。
16. 当前用户安装、开始菜单与可选桌面快捷方式、系统卸载入口、独立卸载程序。
17. 八个中文页面、软件说明、隐私说明与作者联系（含 GitHub 开源链接）。

## 使用流程

**文件**：添加/拖拽 → 检查与确认 → 优先回收站；特殊路径选择永久删除。永久删除与重启删除需输入 `DELETE`。权限修复默认关闭；开启可能改变原有权限，且本工具不备份原 ACL。

**软件**：刷新列表 → 选择 → 运行卸载 → 完成后刷新 → 输入唯一关键词扫描残留 → 逐条查看候选 → 勾选并清理。删除卸载项并不会卸载软件；目录与注册表键可能被同厂商多个产品共享。

**启动项**：刷新 → 选择 → 禁用。备份覆盖整个 Run 键；恢复时会合并备份中的全部值，可能重新启用其它启动项。

## 数据与要求

- 备份：`%LOCALAPPDATA%\JingChu\Backups`。
- 日志：`%LOCALAPPDATA%\JingChu\Logs`。路径与软件名可能出现在日志中。
- 默认安装：`%LOCALAPPDATA%\Programs\JingChu`；当前用户安装。
- 卸载 Jing Delete 会保留备份与日志，不会递归删除安装目录下的未知文件。
- 普通文件操作无需管理员；本机注册表、权限修复、重启删除可能需要管理员。
- 管理员重启会丢弃当前列表；若以不同管理员账户运行，HKCU、用户目录与备份位置也会切换到该账户。
- 以管理员运行时，Windows 可能阻止从非提权的资源管理器拖入文件；请使用「添加」按钮或粘贴路径。
- 本程序未做代码签名，Windows 可能提示未知发布者；本版本不申请也不伪造签名。
- 作者联系方式以你提供的名称为准；不附带任何未经验证的账号链接。

## 能力边界

无内核驱动；不绕过杀毒/反作弊自我保护；不触碰 Windows 内部目录、磁盘/共享根、用户根、程序/备份根。WindowsApps 与 ProgramData\Microsoft 同样被屏蔽。

不递归进入目录 junctions 与符号链接；权限修复同样拒绝链接上的 takeown。若目录在操作过程中被恶意替换造成的竞态，不在本版本防御边界内；处理前请关闭相关软件。

回收站模式拒绝链接或长路径根目标。是否允许回收由 Windows 决定；对于网络或特殊设备，Windows 可能弹出额外对话框——请仔细阅读，必要时取消。失败/取消的永久删除或重启删除可能已处理部分子项；无事务回滚。成功的重启删除排期不保证开机后最终删除，且无内置撤销。

软件扫描仅读取经典桌面卸载项，并非全部 Store 应用。残留是按名称匹配的候选，而非全盘/全注册表扫描，也不是智能“无用项”判断。驱动卸载、浏览器清理、重复文件清理、系统修复均未实现。

占用检测受 Restart Manager 能力与系统策略限制。文件/目录上未检测到进程，不代表其未被占用。出于系统安全，禁止终止 Windows 目录内、系统会话内以及本程序自身的进程。

## 测试

```powershell
powershell.exe -NoProfile -ExecutionPolicy Bypass -File .\test.ps1 -TestRoot D:\some-workspace\new-test-directory
```

测试根目录必须尚不存在。测试程序仅创建专用临时文件、专用 HKCU 测试键、测试卸载项与测试快捷方式——不触碰真实软件；注册表测试键在 `finally` 中清理。测试文件与备份保留在指定目录供检查。Restart Manager 与注册表导出需要访问相关 Windows 服务/注册表；受限沙箱可能拒绝。

目前 123 项自动化检查通过，外加新增的按钮背景像素、图标提取、搜索与刷新检查，以及原来的回归覆盖：路径保护、Unicode/只读、长路径与异常名、链接目标保留、取消、移动不覆盖、锁定文件报错与释放、占用识别、注册表备份/删除/恢复、架构视图、模拟卸载器调用、安装包解包/快捷方式/注册、卸载后自删与退出、保留未知文件、安装 UI 与八页面渲染。真实第三方卸载、UAC、管理员 ACL 修复、真实重启删除与交互式回收站的完整端到端测试尚未完成。

## 文件结构

- `Core.cs`：Windows 原生 API、文件处理、注册表与软件管理。
- `Extras.cs`：空间扫描、候选审查/删除、启动文件夹备份/恢复、CSV 与预览。
- `App.cs`：中文 WinForms 主界面。
- `Screens.cs`：首页、空间整理、启动项管理、作者页、预览窗口。
- `Design.cs`：统一配色、圆角卡片、按钮、图形导航、列表样式。
- `SoftwareIcons.cs`：软件图标路径解析、资源提取、缺失回退。
- `Setup.cs`：安装与卸载程序。
- `Tests.cs`：隔离功能测试与页面渲染。
- `build.ps1` / `test.ps1`：离线构建与测试脚本。
- `app.manifest` / `AssemblyInfo.cs` / `app.ico`：程序元数据。
- `Brand.png` / `BRAND.md`：透明 Logo 与图像生成提示/素材说明。
- `screenshots/`：README 配图（横幅与功能图）。

源代码中 `AppDomain` 测试数据槽仅由测试入口设置；正常运行程序无对应命令行或环境变量入口。

## 开源与贡献

本仓库是 **Jing Delete（景·除）** 的开源主页：

- 仓库：<https://github.com/jingpanlong/JingDelete>
- Issue 与建议：<https://github.com/jingpanlong/JingDelete/issues>
- 发布与下载：<https://github.com/jingpanlong/JingDelete/releases>

欢迎 Pull Request。提交前请运行 `test.ps1`（见上文测试）并确认回归通过。

## 许可证

基于 [MIT 许可证](LICENSE) 发布。Logo 与品牌视觉（含 `Brand.png`、`app.ico`）以相同方式授权，但本版本未做任何商标检索或注册。
