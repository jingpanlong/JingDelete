using System;
using System.IO;
using System.Text;
using System.Linq;
using System.Threading;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace JingChu {
public class CleanCandidate {
 public string Path,Kind;public long Bytes,WriteStamp; public int Mode;
 public override string ToString(){return Path;}
}
public class CleanScan {
 public List<CleanCandidate> Items=new List<CleanCandidate>();public List<string> Warnings=new List<string>();public long Visited;public bool Limit;
}
public class StartupFile {
 public string Path,Name,Scope,Receipt,Payload;public bool Disabled;
 public override string ToString(){return Name+" / "+Scope+(Disabled?" / 已禁用":" / 启用中");}
}
public static partial class Engine {
 public static int WaitForUninstall(System.Diagnostics.Process process,CancellationToken token){if(process==null)throw new Exception("卸载器未提供可监测进程，请完成卸载后手动扫描。");while(!process.WaitForExit(250))token.ThrowIfCancellationRequested();token.ThrowIfCancellationRequested();return process.ExitCode;}
 public static bool UninstallEntryExists(InstalledApp app){using(var root=Microsoft.Win32.RegistryKey.OpenBaseKey(app.Hive,app.View))using(var key=root.OpenSubKey(app.KeyPath))return key!=null;}
 public static string ResidualName(string name){string s=System.Text.RegularExpressions.Regex.Replace(name??"",@"(?i)\s*(?:v?\d+(?:\.\d+)+.*|\((?:x64|x86|64-bit|32-bit)\))\s*$","").Trim();return s.Length>0?s:(name??"").Trim();}
 public static List<RegItem> ScanAfterUninstall(InstalledApp app,CancellationToken token){var result=new List<RegItem>();var seen=new HashSet<string>(StringComparer.OrdinalIgnoreCase);foreach(string term in new[]{ResidualName(app.Name),app.Name}.Distinct()){token.ThrowIfCancellationRequested();if(string.IsNullOrWhiteSpace(term))continue;foreach(var r in ScanRegistry(term,term.Length<3)){string id=r.Hive+"|"+r.View+"|"+r.Path;if(seen.Add(id))result.Add(r);}}return result;}
 static long Stamp(Native.FindData d){return ((long)(uint)d.write.dwHighDateTime<<32)|(uint)d.write.dwLowDateTime;}
 public static Native.FindData FileData(string p){Native.FindData d;IntPtr h=Native.FindFirstFileW(NativePath(p),out d);if(h==new IntPtr(-1))throw new Win32Exception(Marshal.GetLastWin32Error(),p);Native.FindClose(h);return d;}
 public static string SizeText(long n){string[] u={"B","KB","MB","GB","TB"};double x=n;int i=0;while(x>=1024&&i<u.Length-1){x/=1024;i++;}return x.ToString(i==0?"N0":"N2")+" "+u[i];}
 public static string Preview(IEnumerable<string> targets,CancellationToken token){var b=new StringBuilder();foreach(var raw in targets){token.ThrowIfCancellationRequested();try{string p=Validate(raw);b.AppendLine(p);b.AppendLine("  "+Describe(p));b.AppendLine("  "+Stats(p,token));}catch(OperationCanceledException){throw;}catch(Exception e){b.AppendLine("  无法分析："+e.Message);}b.AppendLine();}b.AppendLine("此预览只读取属性和统计，不代表目标一定可删除；未进行删除或权限修改。");return b.ToString();}
 // mode 0: old temporary files, 1: genuinely empty directories, 2: large files, 3: zero-byte files.
 public static CleanScan ScanCleanup(string input,int mode,int ageDays,long minimumBytes,CancellationToken token,Action<string> progress){
  if(mode<0||mode>3)throw new Exception("未知扫描模式。");if(ageDays<1)throw new Exception("临时文件至少保留 1 天。");if(minimumBytes<1)throw new Exception("大文件阈值必须大于零。");
  string root=Validate(input);uint ra=Attr(root);if((ra&16)==0||(ra&1024)!=0)throw new Exception("请选择普通文件夹；不扫描目录链接。");
  var result=new CleanScan();var pending=new Stack<string>();pending.Push(root);DateTime cutoff=DateTime.UtcNow.AddDays(-ageDays);
  while(pending.Count>0){token.ThrowIfCancellationRequested();string p=pending.Pop();result.Visited++;if(result.Visited%100==0)progress(p);
   try{Validate(p);uint a=Attr(p);if((a&1024)!=0)continue;
    if((a&16)!=0){var children=Children(p,false);if(children.Count==0&&mode==1&&!p.Equals(root,StringComparison.OrdinalIgnoreCase))result.Items.Add(new CleanCandidate{Path=p,Kind="空文件夹",Mode=1});foreach(string c in children)pending.Push(c);}
    else{var d=FileData(p);long size=((long)d.hi<<32)+d.lo,stamp=Stamp(d);bool match=mode==0?DateTime.FromFileTimeUtc(stamp)<=cutoff:mode==2?size>=minimumBytes:mode==3&&size==0;
     if(match)result.Items.Add(new CleanCandidate{Path=p,Kind=mode==0?"到期临时文件":mode==2?"大文件":"零字节文件",Bytes=size,WriteStamp=stamp,Mode=mode});}
   }catch(OperationCanceledException){throw;}catch(Exception e){if(result.Warnings.Count<100)result.Warnings.Add(p+" · "+e.Message);}
   if(result.Items.Count>=20000||result.Visited>=300000){result.Limit=true;break;}
  }return result;
 }
 public static void DeleteCandidate(CleanCandidate item,bool recycle,CancellationToken token){
  token.ThrowIfCancellationRequested();string p=Validate(item.Path);uint a=Attr(p);if((a&1024)!=0)throw new Exception("目标已变为链接，已跳过。");
  if(item.Mode==1){if((a&16)==0)throw new Exception("目标类型已变化。");if(Children(p,false).Count!=0)throw new Exception("文件夹已不为空，已跳过。");
   // RemoveDirectory, rather than recursive deletion: newly created contents cannot be erased.
   if(!Native.RemoveDirectoryW(NativePath(p)))throw new Win32Exception(Marshal.GetLastWin32Error(),p);Log("EMPTY_DIR_DELETED "+p);return;}
  if((a&16)!=0)throw new Exception("目标已变为目录，已跳过。");var now=FileData(p);long size=((long)now.hi<<32)+now.lo;
  if(size!=item.Bytes||Stamp(now)!=item.WriteStamp)throw new Exception("文件大小或修改时间已变化，请重新扫描。");
  if(recycle)Recycle(p);else Delete(p,false,false,token,x=>{});
 }
 public static string StartupUser {get{return (AppDomain.CurrentDomain.GetData("JingChu.TestStartupUser") as string)??Environment.GetFolderPath(Environment.SpecialFolder.Startup);}}
 public static string StartupCommon {get{return (AppDomain.CurrentDomain.GetData("JingChu.TestStartupCommon") as string)??Environment.GetFolderPath(Environment.SpecialFolder.CommonStartup);}}
 public static string StartupDisabled {get{return System.IO.Path.Combine(DataDir,"DisabledStartup");}}
 public static List<StartupFile> StartupFiles(){var items=new List<StartupFile>();foreach(string scope in new[]{"User","Common"}){string root=scope=="User"?StartupUser:StartupCommon;if(!Directory.Exists(root))continue;try{foreach(var p in Children(root,false)){uint a=Attr(p);if((a&(16|1024))!=0||System.IO.Path.GetFileName(p).Equals("desktop.ini",StringComparison.OrdinalIgnoreCase))continue;items.Add(new StartupFile{Path=p,Name=System.IO.Path.GetFileName(p),Scope=scope});}}catch(Exception e){Log("STARTUP_FOLDER_SCAN "+e.Message);}}
  if(Directory.Exists(StartupDisabled))foreach(string receipt in Directory.GetFiles(StartupDisabled,"*.meta"))try{var lines=File.ReadAllLines(receipt,Encoding.UTF8);if(lines.Length<2||(lines[0]!="User"&&lines[0]!="Common")||!ValidLeaf(lines[1]))continue;string payload=receipt+".payload";if(!File.Exists(payload))continue;items.Add(new StartupFile{Path=System.IO.Path.Combine(lines[0]=="User"?StartupUser:StartupCommon,lines[1]),Name=lines[1],Scope=lines[0],Receipt=receipt,Payload=payload,Disabled=true});}catch(Exception e){Log("STARTUP_RECEIPT "+e.Message);}return items;
 }
 static bool ValidLeaf(string n){return n.Length>0&&n!="."&&n!=".."&&System.IO.Path.GetFileName(n)==n&&n.IndexOfAny(System.IO.Path.GetInvalidFileNameChars())<0;}
 static void VerifyStartupPath(StartupFile item){if((item.Scope!="User"&&item.Scope!="Common")||!ValidLeaf(item.Name)||!string.Equals(item.Path,System.IO.Path.Combine(item.Scope=="User"?StartupUser:StartupCommon,item.Name),StringComparison.OrdinalIgnoreCase))throw new Exception("启动项来源不符合预期。");}
 public static void DisableStartupFile(StartupFile item){VerifyStartupPath(item);if(item.Disabled)throw new Exception("此项已经禁用。");uint a=Attr(item.Path);if((a&(16|1024))!=0)throw new Exception("不处理目录或重解析链接。");Directory.CreateDirectory(StartupDisabled);string receipt=System.IO.Path.Combine(StartupDisabled,Guid.NewGuid().ToString("N")+".meta"),payload=receipt+".payload";
  try{File.Copy(item.Path,payload,false);File.SetAttributes(payload,FileAttributes.Normal);File.WriteAllLines(receipt,new[]{item.Scope,item.Name},Encoding.UTF8);File.Delete(item.Path);}catch{if(File.Exists(item.Path)){if(File.Exists(payload))File.Delete(payload);if(File.Exists(receipt))File.Delete(receipt);}throw;}Log("STARTUP_FILE_DISABLED "+item.Path);
 }
 public static void EnableStartupFile(StartupFile item){VerifyStartupPath(item);if(!item.Disabled||!string.Equals(System.IO.Path.GetDirectoryName(item.Receipt),StartupDisabled,StringComparison.OrdinalIgnoreCase)||item.Payload!=item.Receipt+".payload")throw new Exception("无效的启动项备份。");if(File.Exists(item.Path))throw new Exception("原位置已有同名文件，为避免覆盖已取消恢复。");Directory.CreateDirectory(System.IO.Path.GetDirectoryName(item.Path));File.Copy(item.Payload,item.Path,false);File.Delete(item.Payload);File.Delete(item.Receipt);Log("STARTUP_FILE_ENABLED "+item.Path);}
 public static void ExportCsv(string destination,string[] headers,IEnumerable<string[]> rows){Func<string,string> q=s=>"\""+(s??"").Replace("\"","\"\"")+"\"";using(var writer=new StreamWriter(destination,false,new UTF8Encoding(true))){writer.WriteLine(string.Join(",",headers.Select(q)));foreach(var row in rows)writer.WriteLine(string.Join(",",row.Select(s=>q(s!=null&&s.Length>0&&"=+-@".IndexOf(s[0])>=0?"'"+s:s))));}}
}
}
