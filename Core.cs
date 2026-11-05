using System;
using System.IO;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Threading;
using Microsoft.Win32;

namespace JingChu {
public static class Native {
 [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)] public struct FindData {
  public uint attr; public System.Runtime.InteropServices.ComTypes.FILETIME creation,access,write;
  public uint hi,lo,r0,r1;
  [MarshalAs(UnmanagedType.ByValTStr,SizeConst=260)] public string name;
  [MarshalAs(UnmanagedType.ByValTStr,SizeConst=14)] public string alt;
 }
 [DllImport("kernel32.dll",CharSet=CharSet.Unicode,SetLastError=true)] public static extern uint GetFileAttributesW(string p);
 [DllImport("kernel32.dll",CharSet=CharSet.Unicode,SetLastError=true)] public static extern bool SetFileAttributesW(string p,uint a);
 [DllImport("kernel32.dll",CharSet=CharSet.Unicode,SetLastError=true)] public static extern bool DeleteFileW(string p);
 [DllImport("kernel32.dll",CharSet=CharSet.Unicode,SetLastError=true)] public static extern bool RemoveDirectoryW(string p);
 [DllImport("kernel32.dll",CharSet=CharSet.Unicode,SetLastError=true)] public static extern bool MoveFileExW(string p,string dest,uint flags);
 [DllImport("kernel32.dll",CharSet=CharSet.Unicode,SetLastError=true)] public static extern IntPtr FindFirstFileW(string p,out FindData d);
 [DllImport("kernel32.dll",CharSet=CharSet.Unicode,SetLastError=true)] public static extern bool FindNextFileW(IntPtr h,out FindData d);
 [DllImport("kernel32.dll")] public static extern bool FindClose(IntPtr h);
 [DllImport("shell32.dll",CharSet=CharSet.Unicode,SetLastError=true)] static extern IntPtr CommandLineToArgvW(string cmd,out int count);
 [DllImport("kernel32.dll")] static extern IntPtr LocalFree(IntPtr p);
 public static string[] SplitCommand(string cmd) {
  int n; IntPtr p=CommandLineToArgvW(cmd,out n); if(p==IntPtr.Zero) throw new Win32Exception();
  try { var a=new string[n]; for(int i=0;i<n;i++) a[i]=Marshal.PtrToStringUni(Marshal.ReadIntPtr(p,i*IntPtr.Size)); return a; } finally { LocalFree(p); }
 }
 [StructLayout(LayoutKind.Sequential)] public struct UniqueProcess { public int pid; public System.Runtime.InteropServices.ComTypes.FILETIME start; }
 [StructLayout(LayoutKind.Sequential,CharSet=CharSet.Unicode)] public struct ProcessInfo {
  public UniqueProcess process;
  [MarshalAs(UnmanagedType.ByValTStr,SizeConst=256)] public string app;
  [MarshalAs(UnmanagedType.ByValTStr,SizeConst=64)] public string service;
  public uint type,status,session; [MarshalAs(UnmanagedType.Bool)] public bool restartable;
 }
 [DllImport("rstrtmgr.dll",CharSet=CharSet.Unicode)] public static extern int RmStartSession(out uint handle,int flags,StringBuilder key);
 [DllImport("rstrtmgr.dll",CharSet=CharSet.Unicode)] public static extern int RmRegisterResources(uint h,uint count,string[] files,uint apps,UniqueProcess[] proc,uint services,string[] svcs);
 [DllImport("rstrtmgr.dll")] public static extern int RmGetList(uint h,out uint needed,ref uint count,[In,Out] ProcessInfo[] info,ref uint reasons);
 [DllImport("rstrtmgr.dll")] public static extern int RmEndSession(uint h);
}
public class InstalledApp {
 public string Name,Version,Publisher,Location,Command,KeyPath,IconSpec; public RegistryHive Hive; public RegistryView View;
 public override string ToString(){return Name;}
}
public class RegItem {
 public RegistryHive Hive; public RegistryView View; public string Path,ValueName,Description;
 public string FullName { get {return (Hive==RegistryHive.LocalMachine?"HKEY_LOCAL_MACHINE":"HKEY_CURRENT_USER")+"\\"+Path;} }
 public override string ToString(){return FullName+(ValueName==null?"":" → "+ValueName)+" ["+(View==RegistryView.Registry64?"64":"32")+" 位]";}
}
public static partial class Engine {
 public static readonly string DataDir=(AppDomain.CurrentDomain.GetData("JingChu.TestData") as string)??Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),"JingChu");
 public static readonly string BackupDir=Path.Combine(DataDir,"Backups");
 public static readonly string LogDir=Path.Combine(DataDir,"Logs");
 static readonly object logLock=new object();
 public static bool Admin {get {using(var i=WindowsIdentity.GetCurrent())return new WindowsPrincipal(i).IsInRole(WindowsBuiltInRole.Administrator);}}
 public static void Log(string text) {try {lock(logLock){Directory.CreateDirectory(LogDir); File.AppendAllText(Path.Combine(LogDir,DateTime.Now.ToString("yyyy-MM-dd")+".log"),DateTime.Now.ToString("HH:mm:ss")+" "+text+Environment.NewLine,Encoding.UTF8);}}catch {}}
 public static string NativePath(string p) {if(p.StartsWith(@"\\?\"))return p; return p.StartsWith(@"\\")?@"\\?\UNC\"+p.Substring(2):@"\\?\"+p;}
 public static string Plain(string p) {if(p.StartsWith(@"\\?\UNC\",StringComparison.OrdinalIgnoreCase))return @"\\"+p.Substring(8); if(p.StartsWith(@"\\?\"))return p.Substring(4); return p;}
 public static string Validate(string input) {
  string p=Plain(input.Trim('"')).TrimEnd('\\');
  if(p.StartsWith(@"\\.\")||p.IndexOfAny(new[]{'*','?','\0','\r','\n'})>=0)throw new Exception("不允许设备路径、通配符或控制字符。");
  if(!System.Text.RegularExpressions.Regex.IsMatch(p,@"^(?:[a-zA-Z]:\\|\\\\[^\\]+\\[^\\]+\\)"))throw new Exception("请选择完整的文件或文件夹路径；不允许磁盘/共享根目录。");
  if(p.Substring(2).Contains(":"))throw new Exception("不允许备用数据流路径。");
  string[] parts=p.Split('\\');
  if(parts.Any(x=>x=="."||x==".."))throw new Exception("不允许相对路径片段。");
  string guard=string.Join("\\",parts.Select(x=>x.TrimEnd(' ','.'))).TrimEnd('\\');
  var protectedPaths=new List<string>{Environment.GetFolderPath(Environment.SpecialFolder.Windows),Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86),Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),Environment.GetEnvironmentVariable("PUBLIC"),AppDomain.CurrentDomain.BaseDirectory.TrimEnd('\\'),DataDir};
  foreach(string s in protectedPaths)if(!string.IsNullOrEmpty(s)&&(s.Equals(guard,StringComparison.OrdinalIgnoreCase)||s.StartsWith(guard+"\\",StringComparison.OrdinalIgnoreCase)))throw new Exception("为防止误删，已拦截系统、用户、程序或备份目录及其上级："+p);
  // System directories themselves and everything inside Windows are protected.
  string win=Environment.GetFolderPath(Environment.SpecialFolder.Windows);
  if(guard.StartsWith(win+"\\",StringComparison.OrdinalIgnoreCase))throw new Exception("禁止处理 Windows 系统目录内的项目。");
  foreach(string critical in new[]{Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),"Microsoft"),Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),"WindowsApps")})if(guard.Equals(critical,StringComparison.OrdinalIgnoreCase)||guard.StartsWith(critical+"\\",StringComparison.OrdinalIgnoreCase))throw new Exception("禁止处理 Windows 共享系统数据或商店应用目录。");
  return p;
 }
 static uint Attr(string p){uint a=Native.GetFileAttributesW(NativePath(p));if(a==uint.MaxValue)throw new Win32Exception(Marshal.GetLastWin32Error(),p);return a;}
 public static bool Exists(string p){return Native.GetFileAttributesW(NativePath(p))!=uint.MaxValue;}
 public static List<string> Children(string p, bool repair) {
  var list=new List<string>(); Native.FindData d; IntPtr h=Native.FindFirstFileW(NativePath(p)+@"\*",out d); int err=Marshal.GetLastWin32Error();
  if(h==new IntPtr(-1)&&err==5&&repair){Repair(p);h=Native.FindFirstFileW(NativePath(p)+@"\*",out d);err=Marshal.GetLastWin32Error();}
  if(h==new IntPtr(-1)){if(err==2)return list;throw new Win32Exception(err,p);}
  try {while(true){if(d.name!="."&&d.name!="..")list.Add(p+"\\"+d.name);if(!Native.FindNextFileW(h,out d)){err=Marshal.GetLastWin32Error();if(err!=18)throw new Win32Exception(err,p);break;}}}finally{Native.FindClose(h);} return list;
 }
 public static string Describe(string p) {uint a=Attr(p);return ((a&16)!=0?"文件夹":"文件")+((a&1024)!=0?" / 链接":"")+" / 属性 0x"+a.ToString("X");}
 public static string Quote(string s) {return "\""+s.Replace("\"","\\\"")+"\"";}
 public static int Run(string exe,string args,out string output) {
  using(var proc=new Process()){proc.StartInfo=new ProcessStartInfo(exe,args){UseShellExecute=false,CreateNoWindow=true,RedirectStandardOutput=true,RedirectStandardError=true};proc.Start();
   var stderr=proc.StandardError.ReadToEndAsync();output=proc.StandardOutput.ReadToEnd();proc.WaitForExit();output+=stderr.Result;return proc.ExitCode;}
 }
 static void Repair(string p) {
  if(!Admin)throw new Exception("权限不足。请通过右上角按钮以管理员身份重启后重试。");
  // Never repair a link: takeown may operate on its destination.
  uint a=Attr(p);if((a&1024)!=0)throw new Exception("链接权限不足；为保护链接目标，未修改其权限。");
  string output; int code=Run(Path.Combine(Environment.SystemDirectory,"takeown.exe"),"/F "+Quote(NativePath(p))+" /A",out output);Log("TAKEOWN "+p+" code="+code+" "+output);
  string sid;using(var i=WindowsIdentity.GetCurrent())sid=i.User.Value;
  code=Run(Path.Combine(Environment.SystemDirectory,"icacls.exe"),Quote(NativePath(p))+" /grant "+Quote("*"+sid+":(F)")+" /L /Q",out output);Log("ICACLS "+p+" code="+code+" "+output);
  if(code!=0)throw new Exception("权限修复失败："+output);
 }
 public static long Delete(string input,bool repair,bool reboot,CancellationToken token,Action<string> progress) {
  string p=Validate(input);long count=0;DeleteNode(p,repair,reboot,token,progress,ref count,0);return count;
 }
 static void DeleteNode(string p,bool repair,bool reboot,CancellationToken token,Action<string> progress,ref long count,int depth) {
  token.ThrowIfCancellationRequested();if(depth>512)throw new Exception("目录层级超过安全上限。");uint a=Attr(p);bool dir=(a&16)!=0,link=(a&1024)!=0;
  if(dir&&!link)foreach(string c in Children(p,repair))DeleteNode(c,repair,reboot,token,progress,ref count,depth+1);
  token.ThrowIfCancellationRequested();progress(p);
  if(reboot) {
   if(!Admin)throw new Exception("重启后删除需要管理员权限。");
   if(p.StartsWith(@"\\"))throw new Exception("重启后删除不支持网络路径。");
   if(!Native.MoveFileExW(NativePath(p),null,4))throw new Win32Exception(Marshal.GetLastWin32Error(),p);
   Log("QUEUED_REBOOT "+p);count++;return;
  }
  if(!link){uint clean=a&~7u;if(clean==0)clean=128;Native.SetFileAttributesW(NativePath(p),clean);}
  bool ok=dir?Native.RemoveDirectoryW(NativePath(p)):Native.DeleteFileW(NativePath(p));int err=Marshal.GetLastWin32Error();
  if(!ok&&err==5&&repair){Repair(p);if(!link){uint clean=a&~7u;Native.SetFileAttributesW(NativePath(p),clean==0?128:clean);}ok=dir?Native.RemoveDirectoryW(NativePath(p)):Native.DeleteFileW(NativePath(p));err=Marshal.GetLastWin32Error();}
  if(!ok)throw new Win32Exception(err,p);count++;Log("DELETED "+p);
 }
 public static void Recycle(string p) {
  p=Validate(p);uint a=Attr(p);if((a&1024)!=0)throw new Exception("链接请使用永久删除，以避免回收站处理链接目标。");
  if(p.Length>=248)throw new Exception("回收站模式不处理长路径，请使用永久删除。");
  if((a&16)!=0)Microsoft.VisualBasic.FileIO.FileSystem.DeleteDirectory(p,Microsoft.VisualBasic.FileIO.UIOption.AllDialogs,Microsoft.VisualBasic.FileIO.RecycleOption.SendToRecycleBin,Microsoft.VisualBasic.FileIO.UICancelOption.ThrowException);
  else Microsoft.VisualBasic.FileIO.FileSystem.DeleteFile(p,Microsoft.VisualBasic.FileIO.UIOption.AllDialogs,Microsoft.VisualBasic.FileIO.RecycleOption.SendToRecycleBin,Microsoft.VisualBasic.FileIO.UICancelOption.ThrowException);
  Log("RECYCLED "+p);
 }
 public static void Rename(string p,string newPath) {p=Validate(p);newPath=Validate(newPath);if(!Native.MoveFileExW(NativePath(p),NativePath(newPath),0))throw new Win32Exception(Marshal.GetLastWin32Error());Log("MOVED "+p+" -> "+newPath);}
 public static string Stats(string input,CancellationToken token) {long files=0,dirs=0,bytes=0,links=0;var stack=new Stack<string>();stack.Push(Validate(input));while(stack.Count>0){token.ThrowIfCancellationRequested();string p=stack.Pop();uint a=Attr(p);if((a&1024)!=0){links++;continue;}if((a&16)!=0){dirs++;foreach(var c in Children(p,false))stack.Push(c);}else{Native.FindData d;var h=Native.FindFirstFileW(NativePath(p),out d);if(h==new IntPtr(-1))throw new Win32Exception();try{bytes+=((long)d.hi<<32)+d.lo;files++;}finally{Native.FindClose(h);}}}return "文件 "+files+"；文件夹 "+dirs+"；链接 "+links+"；大小 "+(bytes/1048576.0).ToString("N2")+" MB";}
 public static List<Native.ProcessInfo> Lockers(IEnumerable<string> inputs,CancellationToken token) {
  var files=new List<string>();var stack=new Stack<string>(inputs.Select(Validate));while(stack.Count>0){token.ThrowIfCancellationRequested();string p=stack.Pop();uint a=Attr(p);if((a&1024)!=0)continue;if((a&16)!=0){foreach(string c in Children(p,false))stack.Push(c);}else files.Add(p);if(files.Count+stack.Count>10000)throw new Exception("占用查询最多处理 10000 个项目，请缩小所选范围。");}
  if(files.Count==0)return new List<Native.ProcessInfo>();uint h;int rc=Native.RmStartSession(out h,0,new StringBuilder(33));if(rc!=0)throw new Win32Exception(rc,"Restart Manager 无法建立会话（Win32 "+rc+"）："+new Win32Exception(rc).Message);
  try {rc=Native.RmRegisterResources(h,(uint)files.Count,files.ToArray(),0,null,0,null);if(rc!=0)throw new Win32Exception(rc);
   uint needed=0,n=0,reasons=0;rc=Native.RmGetList(h,out needed,ref n,null,ref reasons);
   for(int attempt=0;attempt<4;attempt++){if(rc==0&&needed==0)return new List<Native.ProcessInfo>();if(rc!=234)throw new Win32Exception(rc);n=needed;var data=new Native.ProcessInfo[n];rc=Native.RmGetList(h,out needed,ref n,data,ref reasons);if(rc==0)return data.Take((int)n).ToList();}throw new Exception("占用状态变化过快，请重试。");
  }finally{Native.RmEndSession(h);}
 }
 public static void Terminate(Native.ProcessInfo info) {
  using(var p=Process.GetProcessById(info.process.pid)) {
   long expected=((long)(uint)info.process.start.dwHighDateTime<<32)|(uint)info.process.start.dwLowDateTime;
   if(p.StartTime.ToFileTimeUtc()!=expected)throw new Exception("进程已变化，请重新扫描。");
   if(p.Id<=4||p.Id==Process.GetCurrentProcess().Id||p.SessionId==0)throw new Exception("不能结束系统会话或本程序进程。");
   string exe=p.MainModule.FileName;if(exe.StartsWith(Environment.GetFolderPath(Environment.SpecialFolder.Windows)+"\\",StringComparison.OrdinalIgnoreCase))throw new Exception("不能结束 Windows 系统进程。");
   p.Kill();Log("TERMINATED "+info.process.pid+" "+info.app);
  }
 }
 static RegistryView[] Views {get{return Environment.Is64BitOperatingSystem?new[]{RegistryView.Registry64,RegistryView.Registry32}:new[]{RegistryView.Registry32};}}
 public static List<InstalledApp> Apps() {
  var list=new List<InstalledApp>();foreach(var hive in new[]{RegistryHive.CurrentUser,RegistryHive.LocalMachine})foreach(var view in Views)using(var root=RegistryKey.OpenBaseKey(hive,view))using(var key=root.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall")){
   if(key==null)continue;foreach(string n in key.GetSubKeyNames())try{using(var k=key.OpenSubKey(n)){string name=Convert.ToString(k.GetValue("DisplayName")),cmd=Convert.ToString(k.GetValue("UninstallString"));if(name.Length==0||cmd.Length==0)continue;list.Add(new InstalledApp{Name=name,Version=Convert.ToString(k.GetValue("DisplayVersion")),Publisher=Convert.ToString(k.GetValue("Publisher")),Location=Convert.ToString(k.GetValue("InstallLocation")),Command=cmd,IconSpec=Convert.ToString(k.GetValue("DisplayIcon")),Hive=hive,View=view,KeyPath=key.Name.Substring(key.Name.IndexOf('\\')+1)+"\\"+n});}}catch(Exception e){Log("APP_SCAN "+e.Message);}
  }return list.OrderBy(x=>x.Name).ToList();
 }
 public static Process Uninstall(InstalledApp app) {
  string command=Environment.ExpandEnvironmentVariables(app.Command).Trim();string exe,args;
  if(command.StartsWith("\"")){int end=command.IndexOf('"',1);if(end<0)throw new Exception("卸载命令的引号不完整。");exe=command.Substring(1,end-1);args=command.Substring(end+1).Trim();}
  else {int end=command.IndexOf(".exe",StringComparison.OrdinalIgnoreCase);if(end>=0){exe=command.Substring(0,end+4);args=command.Substring(end+4).Trim();}else{var split=command.IndexOf(' ');exe=split<0?command:command.Substring(0,split);args=split<0?"":command.Substring(split+1);}}
  if(Path.GetFileName(exe).Equals("msiexec.exe",StringComparison.OrdinalIgnoreCase)||exe.Equals("msiexec",StringComparison.OrdinalIgnoreCase)) {exe=Path.Combine(Environment.SystemDirectory,"msiexec.exe");args=System.Text.RegularExpressions.Regex.Replace(args,@"(?i)(^|\s)/i(?=\s|\{)","$1/X");}
  if(!Path.IsPathRooted(exe)){string candidate=Path.Combine(Environment.SystemDirectory,exe);if(!File.Exists(candidate))throw new Exception("卸载程序不是完整路径，请检查命令："+command);exe=candidate;}
  if(!File.Exists(exe))throw new Exception("卸载程序不存在："+exe+"。可扫描该软件的残留后手动处理。");
  Log("UNINSTALL "+exe+" "+args);return Process.Start(new ProcessStartInfo(exe,args){UseShellExecute=true});
 }
 public static List<RegItem> ScanRegistry(string term,bool exactOnly=false) {
  if(term.Trim().Length<(exactOnly?1:3))throw new Exception("关键词至少 3 个字符；请使用软件独有名称，避免通用词。");var list=new List<RegItem>();
  foreach(var hive in new[]{RegistryHive.CurrentUser,RegistryHive.LocalMachine})foreach(var view in Views)using(var root=RegistryKey.OpenBaseKey(hive,view)){
   using(var key=root.OpenSubKey("SOFTWARE")){if(key!=null)foreach(var n in key.GetSubKeyNames())if((exactOnly?n.Equals(term,StringComparison.OrdinalIgnoreCase):n.IndexOf(term,StringComparison.OrdinalIgnoreCase)>=0)&&!BlockedKeyName(n))list.Add(new RegItem{Hive=hive,View=view,Path="SOFTWARE\\"+n,Description="名称匹配；可能由多个软件共用，须核对"});}
   using(var key=root.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall")){if(key!=null)foreach(var n in key.GetSubKeyNames())try{using(var k=key.OpenSubKey(n)){string name=Convert.ToString(k.GetValue("DisplayName"));if((exactOnly?name.Equals(term,StringComparison.OrdinalIgnoreCase):name.IndexOf(term,StringComparison.OrdinalIgnoreCase)>=0))list.Add(new RegItem{Hive=hive,View=view,Path=@"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\"+n,Description="卸载登记："+name});}}catch(Exception e){Log("REG_SCAN "+e.Message);}}
  }return list;
 }
 static bool BlockedKeyName(string n){return new[]{"Microsoft","Windows","Classes","Policies","WOW6432Node","Clients","RegisteredApplications"}.Contains(n,StringComparer.OrdinalIgnoreCase);}
 public static List<string> ScanFolders(string term) {
  if(term.Trim().Length<3)throw new Exception("关键词至少 3 个字符。");var list=new List<string>();
  var roots=new[]{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86)};
  foreach(string root in roots.Distinct(StringComparer.OrdinalIgnoreCase)){if(string.IsNullOrEmpty(root))continue;try{foreach(var p in Children(root,false)){uint a=Attr(p);if((a&16)!=0&&Path.GetFileName(p).IndexOf(term,StringComparison.OrdinalIgnoreCase)>=0){try{Validate(p);list.Add(p);}catch{}}}}catch(Exception e){Log("FOLDER_SCAN "+root+" "+e.Message);}}
  return list;
 }
 public static string Backup(RegItem item) {
  Directory.CreateDirectory(BackupDir);string file=Path.Combine(BackupDir,DateTime.Now.ToString("yyyyMMdd-HHmmss-fff")+"-"+Guid.NewGuid().ToString("N").Substring(0,6)+"-"+(item.View==RegistryView.Registry64?"64":"32")+".reg");string output;
  int rc=Run(Path.Combine(Environment.SystemDirectory,"reg.exe"),"export "+Quote(item.FullName)+" "+Quote(file)+" /y /reg:"+(item.View==RegistryView.Registry64?"64":"32"),out output);
  if(rc!=0||!File.Exists(file)||new FileInfo(file).Length<10)throw new Exception("注册表备份失败；已取消修改："+output);
  File.WriteAllText(file+".txt",item.ToString()+Environment.NewLine+item.Description,Encoding.UTF8);Log("REG_BACKUP "+item+" -> "+file);return file;
 }
 public static void RemoveRegistry(RegItem item) {
  // Only allow the exact categories produced by this application.
  string prefix=@"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\";
  bool direct=item.Path.Length>9&&item.Path.StartsWith("SOFTWARE\\",StringComparison.OrdinalIgnoreCase)&&item.Path.Substring(9).IndexOf('\\')<0&&!BlockedKeyName(item.Path.Substring(9));
  if(item.ValueName!=null||(!direct&&!item.Path.StartsWith(prefix,StringComparison.OrdinalIgnoreCase)))throw new Exception("此注册表路径不在清理范围。");
  Backup(item);using(var root=RegistryKey.OpenBaseKey(item.Hive,item.View))root.DeleteSubKeyTree(item.Path,true);Log("REG_DELETED "+item);
 }
 public static void Restore(string file) {
  if(!file.EndsWith(".reg",StringComparison.OrdinalIgnoreCase))throw new Exception("请选择 .reg 备份。");
  string view=file.EndsWith("-32.reg",StringComparison.OrdinalIgnoreCase)?"32":"64",output;
  if(!Environment.Is64BitOperatingSystem)view="32";
  int rc=Run(Path.Combine(Environment.SystemDirectory,"reg.exe"),"import "+Quote(file)+" /reg:"+view,out output);if(rc!=0)throw new Exception(output);Log("REG_RESTORE "+file);
 }
 public static List<RegItem> Startup() {
  var list=new List<RegItem>();foreach(var hive in new[]{RegistryHive.CurrentUser,RegistryHive.LocalMachine})foreach(var view in Views)using(var root=RegistryKey.OpenBaseKey(hive,view))using(var key=root.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run")){if(key==null)continue;foreach(var n in key.GetValueNames())list.Add(new RegItem{Hive=hive,View=view,Path=@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run",ValueName=n,Description=Convert.ToString(key.GetValue(n))});}return list;
 }
 public static void DisableStartup(RegItem item) {
  if(item.Path!=@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run"||item.ValueName==null)throw new Exception("无效的启动项。");Backup(item);using(var root=RegistryKey.OpenBaseKey(item.Hive,item.View))using(var key=root.OpenSubKey(item.Path,true))key.DeleteValue(item.ValueName,true);Log("STARTUP_DISABLED "+item);
 }
}
}
