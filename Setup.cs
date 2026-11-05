using System;
using System.IO;
using System.Linq;
using System.Drawing;
using System.Text;
using System.Diagnostics;
using System.Reflection;
using System.Windows.Forms;
using Microsoft.Win32;
using JingChu;

namespace JingSetup {
static class Program {
 const string Product="景·除",AppFile="JingChu.exe",RemoveFile="Uninstall.exe";
 static string RegPath {get{return (AppDomain.CurrentDomain.GetData("JingChu.SetupTestReg") as string)??@"Software\Microsoft\Windows\CurrentVersion\Uninstall\JingChu";}}
 [STAThread] static void Main(string[] args){Application.EnableVisualStyles();Application.SetCompatibleTextRenderingDefault(false);try{if(args.Contains("/uninstall"))Remove(true);else Application.Run(new SetupForm());}catch(Exception e){BrandDialogs.Show(e.Message,"景·除安装程序",MessageBoxButtons.OK,MessageBoxIcon.Error);}}
 static void Shortcut(string path,string exe,string args){Type type=Type.GetTypeFromProgID("WScript.Shell");object shell=Activator.CreateInstance(type);object link=type.InvokeMember("CreateShortcut",BindingFlags.InvokeMethod,null,shell,new object[]{path});Type lt=link.GetType();lt.InvokeMember("TargetPath",BindingFlags.SetProperty,null,link,new object[]{exe});lt.InvokeMember("Arguments",BindingFlags.SetProperty,null,link,new object[]{args});lt.InvokeMember("WorkingDirectory",BindingFlags.SetProperty,null,link,new object[]{Path.GetDirectoryName(exe)});lt.InvokeMember("Save",BindingFlags.InvokeMethod,null,link,null);System.Runtime.InteropServices.Marshal.FinalReleaseComObject(link);System.Runtime.InteropServices.Marshal.FinalReleaseComObject(shell);}
 static string MenuDir {get{return (AppDomain.CurrentDomain.GetData("JingChu.SetupTestMenu") as string)??Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Programs),"景·除");}}
 static string DesktopLink {get{return (AppDomain.CurrentDomain.GetData("JingChu.SetupTestDesktop") as string)??Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory),"景·除.lnk");}}
 static void CheckDestination(string dir){if(!Path.IsPathRooted(dir)||dir.StartsWith(@"\\"))throw new Exception("请选择本机磁盘上的完整目录。");string full=Path.GetFullPath(dir).TrimEnd('\\');if(full.Length<5||full==Path.GetPathRoot(full).TrimEnd('\\'))throw new Exception("不能安装到磁盘根目录。");foreach(var s in new[]{Environment.GetFolderPath(Environment.SpecialFolder.Windows),Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86),Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)}){if(string.IsNullOrEmpty(s))continue;if(full.Equals(s,StringComparison.OrdinalIgnoreCase)||s.StartsWith(full+"\\",StringComparison.OrdinalIgnoreCase)||full.StartsWith(Environment.GetFolderPath(Environment.SpecialFolder.Windows)+"\\",StringComparison.OrdinalIgnoreCase))throw new Exception("请选择专用安装子目录，推荐使用默认目录。");}if(File.Exists(Path.Combine(dir,RemoveFile))){using(var key=Registry.CurrentUser.OpenSubKey(RegPath)){if(key==null||!string.Equals(Convert.ToString(key.GetValue("InstallLocation")),dir,StringComparison.OrdinalIgnoreCase))throw new Exception("该目录存在其他卸载程序，请更换空目录。");}}}
 static void Install(string dir,bool desktop){CheckDestination(dir);Directory.CreateDirectory(dir);string dest=Path.Combine(dir,AppFile),uninstall=Path.Combine(dir,RemoveFile);if(Path.GetFullPath(Application.ExecutablePath).Equals(uninstall,StringComparison.OrdinalIgnoreCase))throw new Exception("请运行下载的安装包进行安装。");
  using(var stream=Assembly.GetExecutingAssembly().GetManifestResourceStream("JingChu.exe")){if(stream==null)throw new Exception("安装包缺少程序资源。");using(var file=new FileStream(dest+".new",FileMode.Create,FileAccess.Write))stream.CopyTo(file);}
  try{if(File.Exists(dest))File.Delete(dest);File.Move(dest+".new",dest);File.Copy(Application.ExecutablePath,uninstall,true);
   Directory.CreateDirectory(MenuDir);Shortcut(Path.Combine(MenuDir,"景·除.lnk"),dest,"");Shortcut(Path.Combine(MenuDir,"卸载景·除.lnk"),uninstall,"/uninstall");if(desktop)Shortcut(DesktopLink,dest,"");
   using(var key=Registry.CurrentUser.CreateSubKey(RegPath)){key.SetValue("DisplayName",Product);key.SetValue("DisplayVersion","2.0.7");key.SetValue("Publisher","景同志 / 景同学");key.SetValue("InstallLocation",dir);key.SetValue("DisplayIcon",dest);key.SetValue("UninstallString","\""+uninstall+"\" /uninstall");key.SetValue("NoModify",1,RegistryValueKind.DWord);key.SetValue("NoRepair",1,RegistryValueKind.DWord);key.SetValue("EstimatedSize",(int)((new FileInfo(dest).Length+new FileInfo(uninstall).Length)/1024),RegistryValueKind.DWord);}
  }catch(Exception e){throw new Exception("安装未完成："+e.Message+"\n请关闭运行中的景·除后重试。目标目录可能保留已复制的程序。",e);}
 }
 static void Remove(bool interactive){string dir=Path.GetDirectoryName(Application.ExecutablePath);using(var key=Registry.CurrentUser.OpenSubKey(RegPath)){if(key==null||!string.Equals(Convert.ToString(key.GetValue("InstallLocation")),dir,StringComparison.OrdinalIgnoreCase))throw new Exception("安装登记与当前程序位置不一致，已取消卸载。");}
  if(interactive&&BrandDialogs.Show("将移除景·除程序、快捷方式和安装登记。\n注册表备份与日志保留在当前用户 LocalAppData\\JingChu。","卸载景·除",MessageBoxButtons.OKCancel,MessageBoxIcon.Question)!=DialogResult.OK)return;
  foreach(var p in Process.GetProcessesByName("JingChu")){try{if(string.Equals(p.MainModule.FileName,Path.Combine(dir,AppFile),StringComparison.OrdinalIgnoreCase))throw new InvalidOperationException("景·除仍在运行，请先关闭后再卸载。");}catch(InvalidOperationException){throw;}catch{}finally{p.Dispose();}}
  string app=Path.Combine(dir,AppFile);if(File.Exists(app))File.Delete(app);
  foreach(string link in new[]{Path.Combine(MenuDir,"景·除.lnk"),Path.Combine(MenuDir,"卸载景·除.lnk"),DesktopLink})if(File.Exists(link))File.Delete(link);
  if(Directory.Exists(MenuDir)&&Directory.GetFileSystemEntries(MenuDir).Length==0)Directory.Delete(MenuDir);
  Registry.CurrentUser.DeleteSubKeyTree(RegPath,false);
  string self=Application.ExecutablePath.Replace("'","''"),folder=dir.Replace("'","''");
  string ps="$ErrorActionPreference='SilentlyContinue'; Wait-Process -Id "+Process.GetCurrentProcess().Id+" -ErrorAction SilentlyContinue; Start-Sleep -Milliseconds 600; Remove-Item -LiteralPath '"+self+"' -Force; if ((Test-Path -LiteralPath '"+folder+"') -and @(Get-ChildItem -LiteralPath '"+folder+"' -Force).Count -eq 0) { Remove-Item -LiteralPath '"+folder+"' }";
  Process.Start(new ProcessStartInfo(Path.Combine(Environment.SystemDirectory,@"WindowsPowerShell\v1.0\powershell.exe"),"-NoProfile -NonInteractive -WindowStyle Hidden -EncodedCommand "+Convert.ToBase64String(Encoding.Unicode.GetBytes(ps))){UseShellExecute=false,CreateNoWindow=true});
  if(interactive)BrandDialogs.Show("卸载完成。关闭本窗口后清除卸载程序本身。\n备份与日志已保留。","景·除");
 }
 class SetupForm:Form {
  TextBox path;CheckBox desktop,launch;Button install;
  public SetupForm(){Text="安装景·除 2.0.7";Icon=Icon.ExtractAssociatedIcon(Application.ExecutablePath);ClientSize=new Size(740,585);FormBorderStyle=FormBorderStyle.FixedDialog;MaximizeBox=false;StartPosition=FormStartPosition.CenterScreen;Font=new Font("Microsoft YaHei UI",10F);BackColor=Look.Canvas;
   var hero=new CardPanel{Dark=true,Location=new Point(24,24),Size=new Size(692,175)};Controls.Add(hero);hero.Controls.Add(new PictureBox{Image=Look.Logo,SizeMode=PictureBoxSizeMode.Zoom,BackColor=Color.Transparent,Location=new Point(24,24),Size=new Size(122,122)});
   hero.Controls.Add(new Label{Text="景·除",Font=new Font(Font.FontFamily,30,FontStyle.Bold),ForeColor=Color.White,BackColor=Color.Transparent,Location=new Point(166,25),Size=new Size(330,60)});
   hero.Controls.Add(new Label{Text="Jing Delete  /  v2.0.7",ForeColor=Look.Mint,BackColor=Color.Transparent,Location=new Point(172,93),Size=new Size(400,28)});hero.Controls.Add(new Label{Text="把多余的，留在过去。",ForeColor=Color.FromArgb(182,218,207),BackColor=Color.Transparent,Location=new Point(172,126),Size=new Size(440,30)});
   var card=new CardPanel{Location=new Point(24,218),Size=new Size(692,238)};Controls.Add(card);card.Controls.Add(new Label{Text="安装位置",ForeColor=Look.Ink,Font=new Font(Font.FontFamily,12,FontStyle.Bold),BackColor=Color.White,Location=new Point(23,19),Size=new Size(450,30)});
   path=new TextBox{Text=Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),@"Programs\JingChu"),Location=new Point(25,65),Width=520};card.Controls.Add(path);
   var browse=new SoftButton{Text="浏览",Location=new Point(559,59),Size=new Size(108,38)};browse.Click+=(s,e)=>{using(var d=new FolderBrowserDialog{SelectedPath=path.Text})if(d.ShowDialog()==DialogResult.OK)path.Text=d.SelectedPath;};card.Controls.Add(browse);
   desktop=new CheckBox{Text="创建桌面快捷方式",Checked=true,BackColor=Color.White,Location=new Point(25,115),Size=new Size(290,30)};card.Controls.Add(desktop);launch=new CheckBox{Text="安装完成后启动",Checked=true,BackColor=Color.White,Location=new Point(354,115),Size=new Size(280,30)};card.Controls.Add(launch);
   card.Controls.Add(new Label{Text="按当前用户安装，不包含广告或捆绑程序。\nWindows 10/11 · .NET Framework 4.7.2+ · 本地运行",ForeColor=Look.Muted,BackColor=Color.White,Location=new Point(25,165),Size=new Size(640,56)});
   Controls.Add(new Label{Text="微信公众号：景同志\n小红书：景同学",Location=new Point(30,495),Size=new Size(420,55),ForeColor=Look.Muted});
   install=new SoftButton{Text="安装景·除  →",Primary=true,Location=new Point(525,501),Size=new Size(190,45)};install.Click+=(s,e)=>{install.Enabled=false;try{string dir=Path.GetFullPath(path.Text).TrimEnd('\\');Install(dir,desktop.Checked);BrandDialogs.Show("安装完成。","景·除");if(launch.Checked)Process.Start(Path.Combine(dir,AppFile));Close();}catch(Exception ex){BrandDialogs.Show(ex.Message,"安装未完成");install.Enabled=true;}};Controls.Add(install);
  }
 }
}
}
