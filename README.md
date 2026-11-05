# Jing Delete / 景·除

![banner](screenshots/banner.png)

[![Latest release](https://img.shields.io/github/v/release/jingpanlong/JingDelete?label=latest)](https://github.com/jingpanlong/JingDelete/releases)
[![Total downloads](https://img.shields.io/github/downloads/jingpanlong/JingDelete/total)](https://github.com/jingpanlong/JingDelete/releases)
[![Platform](https://img.shields.io/badge/platform-Windows-blue)](https://github.com/jingpanlong/JingDelete)
[![.NET](https://img.shields.io/badge/.NET-Framework%204.7.2%2B-512BD4)](https://docs.microsoft.com/dotnet/framework/)
[![License](https://img.shields.io/github/license/jingpanlong/JingDelete)](LICENSE)

**Jing Delete (景·除)** is a Windows local file & software cleanup tool: file deletion, software uninstall, leftover cleanup, disk-space tidying, and startup management. Everything runs on your machine — **no networking, no telemetry**. Dangerous operations (deletion, cleanup, permission changes) always require explicit confirmation.

> Author: WeChat Official Account「景同志」, Xiaohongshu「景同学」. The app's "About" page includes the GitHub repository link and download entry.

---

## Quick Download

| Version | Notes | Download |
| --- | --- | --- |
| Portable (single file) | `JingChu.exe` — extract and run; relies on the system .NET Framework | [JingChu.exe](https://github.com/jingpanlong/JingDelete/releases/latest/download/JingChu.exe) |
| Installer | `JingChu-Setup.exe` — per-user install with Start Menu and uninstall entry | [JingChu-Setup.exe](https://github.com/jingpanlong/JingDelete/releases/latest/download/JingChu-Setup.exe) |

You can also browse historical versions and checksums on the [Releases](https://github.com/jingpanlong/JingDelete/releases) page.

> The program is not code-signed, so Windows may show an "Unknown publisher" warning. This version does not request or forge a signature; to verify, compare the SHA-256 shown on the release page.

---

## Key Features

![Key Features](screenshots/features.png)

1. **File operations** — drag & drop / picker / paste paths; long-path, trailing-dot/space, and Unicode name handling; pre-delete preview, lock/occupancy detection, same-volume rename & move.
2. **Software uninstall** — reads classic desktop uninstall registry entries and launches each program's own uninstaller; Ctrl/Shift multi-select and batch queue, with a unified post-uninstall registry-candidate list.
3. **Leftover cleanup** — keyword scan of top-level SOFTWARE subkeys, uninstall entries, and common directories' first level; results start unchecked; registry is always backed up before modification.
4. **Disk space** — four scans: stale temp files, empty folders, large files, zero-byte files; results exportable to CSV.
5. **Startup management** — registry Run keys and per-user/public Startup folders in one place; Startup-folder items can be disabled (with backup) and restored.
6. **Backup & logs** — registry backups before modification, restorable and importable; daily logs, live results, and today's log exportable.
7. **Local-first** — no networking, no telemetry; backups and logs stay on the local machine.
8. **Open source** — source available; issues and pull requests welcome.

---

## Table of Contents

- [Build](#build)
- [Implemented Features](#implemented-features)
- [Usage Flow](#usage-flow)
- [Data & Requirements](#data--requirements)
- [Capability Boundaries](#capability-boundaries)
- [Testing](#testing)
- [File Structure](#file-structure)
- [Open Source & Contributing](#open-source--contributing)
- [License](#license)

---

## Build

Windows 10/11, .NET Framework 4.7.2 or later. No NuGet packages or .NET SDK required.

In this directory, run Windows PowerShell:

```powershell
powershell.exe -NoProfile -ExecutionPolicy Bypass -File .\build.ps1
```

Outputs: `bin\JingChu.exe`, `bin\JingChu-Setup.exe`. Both are single-file; the portable build depends on the system .NET Framework rather than bundling the whole runtime. The installer embeds the compiled program and copies itself as the uninstaller.

The build uses the C# compiler from the Windows Framework directory. When the 4.7.2 reference assemblies are present it uses Microsoft.VisualBasic from there; otherwise it falls back to the Framework directory's assembly. `app.ico` is provided with the source; `create-icon.ps1` can regenerate it.

## Implemented Features

1. Multi-file/folder drag & drop; file picker, folder picker, manual path input.
2. Target list, checkbox handling, remove from list, per-item status feedback.
3. File/directory/link property view; recursive file/directory counts and size.
4. Recycle-bin deletion; permanent deletion; Unicode, long-path, trailing-dot/space name handling.
5. Clear read-only/hidden/system attributes before deletion; admins may optionally take ownership and grant the current user permissions per object.
6. Same-volume rename/move, without overwriting existing targets.
7. Restart Manager occupancy detection; request graceful close; after confirmation, terminate non-system processes and verify process creation time.
8. Admins may queue local targets (children before parents) for Windows restart-delete, without auto-reboot.
9. Desktop-software uninstall-entry scan, name/publisher search, launch the registered uninstaller, open install directory.
10. Keyword scan for leftover candidates: common directories' first level, SOFTWARE direct subkeys, uninstall entries.
11. Leftovers start unchecked; confirm each before cleanup; registry is force-backed-up before modification.
12. 32/64-bit registry-view backup & import; backup list and directory entry.
13. Current-user / local-machine Run startup list; disable after backup, restore from backup.
14. Daily logs, live results, logs directory, today's log export.
15. Background execution, cancel remaining steps, admin restart, system-directory interception.
16. Per-user install, Start Menu and optional desktop shortcut, system uninstall entry, standalone uninstaller.
17. Eight Chinese pages, software notes, privacy notes, and author contact (including the GitHub open-source link).

## Usage Flow

**Files**: add/drag → review & check → prefer Recycle Bin; choose permanent delete for special paths. Permanent delete and restart-delete require typing `DELETE`. Permission repair is off by default; enabling it may change original permissions and the tool does not back up the original ACL.

**Software**: refresh list → select → run uninstall → refresh after completion → enter a unique keyword to scan leftovers → review each candidate → check and clean. Deleting an uninstall entry does not uninstall the software; directories and registry keys may be shared by multiple products from the same vendor.

**Startup**: refresh → select → disable. The backup covers the entire Run key; restoring merges all values from the backup and may re-enable other startup items.

## Data & Requirements

- Backups: `%LOCALAPPDATA%\JingChu\Backups`.
- Logs: `%LOCALAPPDATA%\JingChu\Logs`. Paths and software names may appear in logs.
- Default install: `%LOCALAPPDATA%\Programs\JingChu`; per-user install.
- Uninstalling Jing Delete keeps backups and logs, and does not recursively delete unknown files in the install directory.
- Ordinary file operations need no admin; local-machine registry, permission repair, and restart-delete may require admin.
- Admin restart discards the current list; if run as a different admin account, HKCU, the user directory, and backup location also switch to that account.
- Running as admin, Windows may block dragging files from a non-elevated Explorer; use the Add button or paste paths.
- The program is not code-signed; Windows may show an unknown publisher. This version does not request or forge a signature.
- Author contact is shown by the names you provided; no unverified account links are attached.

## Capability Boundaries

No kernel driver; does not bypass antivirus/anti-cheat self-protection; does not touch Windows internal directories, disk/share roots, user root, or the program/backup root. WindowsApps and ProgramData\Microsoft are also blocked.

Directory junctions and symbolic links are not recursed into; permission repair also refuses takeown on links. Race conditions where a directory is maliciously replaced mid-operation are outside this version's defensive boundary; close related software before processing.

Recycle-Bin mode rejects link or long-path root targets. Whether recycling is allowed is decided by Windows; for network or special devices Windows may show extra dialogs — read carefully and cancel if needed. A failed/cancelled permanent delete or restart-delete may have already processed some children; there is no transaction rollback. A successful restart-delete schedule does not guarantee final deletion at boot, and there is no built-in undo.

Software scan reads only classic desktop uninstall entries, not all Store apps. Leftovers are name-matched candidates, not a full-disk/full-registry scan and not an intelligent "useless key" judgment. Driver uninstall, browser cleanup, duplicate-file cleanup, and system repair are not implemented.

Occupancy detection is limited by Restart Manager capability and system policy. No detected process on a file/directory does not mean it is unoccupied. For system safety, terminating programs inside Windows directories, system sessions, and this program itself is forbidden.

## Testing

```powershell
powershell.exe -NoProfile -ExecutionPolicy Bypass -File .\test.ps1 -TestRoot D:\some-workspace\new-test-directory
```

The test root must not exist yet. The test program creates only dedicated temp files, a dedicated HKCU test key, a test uninstall entry, and a test shortcut — it does not touch real software; the registry test key is cleaned in `finally`. Test files and backups remain in the specified directory for inspection. Restart Manager and registry export need access to the relevant Windows services/registry; a restricted sandbox may deny them.

123 automated checks currently pass, plus new button-background pixel, icon extraction, search, and refresh checks, and the original regression covering path protection, Unicode/read-only, long paths and anomalous names, link-target preservation, cancel, move-without-overwrite, locked-file errors and release, occupancy identification, registry backup/delete/restore, architecture views, simulated uninstaller invocation, installer unpack/shortcut/registration, self-delete after uninstall and exit, preserving unknown files, install UI and eight-page rendering. Full end-to-end testing of real third-party uninstalls, UAC, admin ACL repair, real restart-delete, and interactive Recycle Bin is not yet done.

## File Structure

- `Core.cs`: Windows native APIs, file handling, registry & software management.
- `Extras.cs`: space scanning, candidate review/delete, Startup-folder backup/restore, CSV & preview.
- `App.cs`: Chinese WinForms main UI.
- `Screens.cs`: home, space tidying, startup management, author page, preview window.
- `Design.cs`: unified colors, rounded cards, buttons, graphic navigation, list styles.
- `SoftwareIcons.cs`: software icon path resolution, resource extraction, missing fallback.
- `Setup.cs`: installer & uninstaller.
- `Tests.cs`: isolated functional tests and page rendering.
- `build.ps1` / `test.ps1`: offline build & test scripts.
- `app.manifest` / `AssemblyInfo.cs` / `app.ico`: program metadata.
- `Brand.png` / `BRAND.md`: transparent logo and image-generation prompts/asset notes.
- `screenshots/`: README imagery (banner and feature graphic).

The `AppDomain` test-data slot in source is set only by the test entry point; the normal program has no corresponding command-line or environment entry.

## Open Source & Contributing

This repository is the open-source home of **Jing Delete (景·除)**:

- Repository: <https://github.com/jingpanlong/JingDelete>
- Issues & suggestions: <https://github.com/jingpanlong/JingDelete/issues>
- Releases & downloads: <https://github.com/jingpanlong/JingDelete/releases>

Pull requests are welcome. Please run `test.ps1` (see Testing above) and confirm the regression passes before submitting.

## License

Released under the [MIT License](LICENSE). The logo and brand visuals (including `Brand.png`, `app.ico`) are licensed the same way, but no trademark search or registration has been performed for this version.
