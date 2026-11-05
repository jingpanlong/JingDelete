$ErrorActionPreference = 'Stop'
Set-Location -LiteralPath $PSScriptRoot
$compiler = Join-Path $env:WINDIR 'Microsoft.NET\Framework64\v4.0.30319\csc.exe'
if (-not (Test-Path -LiteralPath $compiler)) { $compiler = Join-Path $env:WINDIR 'Microsoft.NET\Framework\v4.0.30319\csc.exe' }
$referenceRoot = Join-Path ${env:ProgramFiles(x86)} 'Reference Assemblies\Microsoft\Framework\.NETFramework\v4.7.2'
$vb = Join-Path $referenceRoot 'Microsoft.VisualBasic.dll'
if (-not (Test-Path -LiteralPath $vb)) { $vb = Join-Path $env:WINDIR 'Microsoft.NET\Framework\v4.0.30319\Microsoft.VisualBasic.dll' }
New-Item -ItemType Directory -Force -Path bin | Out-Null
if (-not (Test-Path -LiteralPath app.ico) -or (Get-Item Brand.png).LastWriteTime -gt (Get-Item app.ico).LastWriteTime) { & .\create-icon.ps1 }
$common = @('/nologo', '/optimize+', '/target:winexe', '/platform:anycpu', '/utf8output', '/win32manifest:app.manifest', '/win32icon:app.ico', '/r:System.dll', '/r:System.Core.dll', '/r:System.Drawing.dll', '/r:System.Windows.Forms.dll')
& $compiler @common "/r:$vb" '/out:bin\JingChu.exe' '/resource:Brand.png,Brand.png' Core.cs Extras.cs Design.cs SoftwareIcons.cs App.cs Screens.cs Menus.cs AssemblyInfo.cs
if ($LASTEXITCODE -ne 0) { throw 'Application build failed' }
& $compiler @common '/out:bin\JingChu-Setup.exe' '/resource:bin\JingChu.exe,JingChu.exe' '/resource:Brand.png,Brand.png' Setup.cs Design.cs AssemblyInfo.cs
if ($LASTEXITCODE -ne 0) { throw 'Installer build failed' }
Write-Host 'Built: bin\JingChu.exe and bin\JingChu-Setup.exe'
