param([Parameter(Mandatory=$true)][string]$TestRoot)
$ErrorActionPreference = 'Stop'
Set-Location -LiteralPath $PSScriptRoot
& .\build.ps1
$compiler = Join-Path $env:WINDIR 'Microsoft.NET\Framework64\v4.0.30319\csc.exe'
$vb = Join-Path ${env:ProgramFiles(x86)} 'Reference Assemblies\Microsoft\Framework\.NETFramework\v4.7.2\Microsoft.VisualBasic.dll'
& $compiler /nologo /target:exe /platform:anycpu /utf8output /main:Tests /win32icon:app.ico /out:bin\Tests.exe /r:System.dll /r:System.Core.dll /r:System.Drawing.dll /r:System.Windows.Forms.dll "/r:$vb" /resource:bin\JingChu.exe,JingChu.exe /resource:Brand.png,Brand.png Core.cs Extras.cs Design.cs SoftwareIcons.cs App.cs Screens.cs Menus.cs Setup.cs Tests.cs
if ($LASTEXITCODE -ne 0) { throw 'Test build failed' }
& .\bin\Tests.exe $TestRoot
if ($LASTEXITCODE -ne 0) { throw 'Tests failed' }
