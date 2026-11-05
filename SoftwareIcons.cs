using System;
using System.IO;
using System.Linq;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
using System.Text;
using System.Runtime.InteropServices.ComTypes;

namespace JingChu {
public static class SoftwareIcons {
 [ComImport,Guid("000214F9-0000-0000-C000-000000000046"),InterfaceType(ComInterfaceType.InterfaceIsIUnknown)] interface IShellLinkW {
  void GetPath([Out,MarshalAs(UnmanagedType.LPWStr)]StringBuilder path,int max,IntPtr data,uint flags);void GetIDList(out IntPtr p);void SetIDList(IntPtr p);
  void GetDescription([Out,MarshalAs(UnmanagedType.LPWStr)]StringBuilder text,int max);void SetDescription([MarshalAs(UnmanagedType.LPWStr)]string text);
  void GetWorkingDirectory([Out,MarshalAs(UnmanagedType.LPWStr)]StringBuilder text,int max);void SetWorkingDirectory([MarshalAs(UnmanagedType.LPWStr)]string text);
  void GetArguments([Out,MarshalAs(UnmanagedType.LPWStr)]StringBuilder text,int max);void SetArguments([MarshalAs(UnmanagedType.LPWStr)]string text);
  void GetHotkey(out short key);void SetHotkey(short key);void GetShowCmd(out int value);void SetShowCmd(int value);
  void GetIconLocation([Out,MarshalAs(UnmanagedType.LPWStr)]StringBuilder path,int max,out int index);void SetIconLocation([MarshalAs(UnmanagedType.LPWStr)]string path,int index);
  void SetRelativePath([MarshalAs(UnmanagedType.LPWStr)]string path,uint reserved);void Resolve(IntPtr hwnd,uint flags);void SetPath([MarshalAs(UnmanagedType.LPWStr)]string path);
 }
 public static Bitmap Startup(object item){var reg=item as RegItem;if(reg!=null)return Load(new InstalledApp{Command=reg.Description});var file=item as StartupFile;if(file==null)return Fallback();string path=file.Disabled?file.Payload:file.Path;
  if(Path.GetExtension(file.Name).Equals(".lnk",StringComparison.OrdinalIgnoreCase)){object link=null;try{link=Activator.CreateInstance(Type.GetTypeFromCLSID(new Guid("00021401-0000-0000-C000-000000000046")));((IPersistFile)link).Load(path,0);var shortcut=(IShellLinkW)link;var icon=new StringBuilder(32768);int index;shortcut.GetIconLocation(icon,icon.Capacity,out index);var image=ReadSpec(Engine.Quote(icon.ToString())+","+index);if(image!=null)return image;var target=new StringBuilder(32768);shortcut.GetPath(target,target.Capacity,IntPtr.Zero,4);image=ReadSpec(target.ToString());if(image!=null)return image;}catch{}finally{if(link!=null&&Marshal.IsComObject(link))Marshal.FinalReleaseComObject(link);}return Fallback();}
  var direct=ReadSpec(path);return direct??Fallback();
 }
 [DllImport("shell32.dll",CharSet=CharSet.Unicode)] static extern uint ExtractIconExW(string file,int index,out IntPtr large,out IntPtr small,uint count);
 [DllImport("user32.dll")] static extern bool DestroyIcon(IntPtr handle);
 public static void Parse(string spec,out string path,out int index){
  path=Environment.ExpandEnvironmentVariables((spec??"").Trim());index=0;
  int comma=path.LastIndexOf(',');int value;if(comma>=0&&int.TryParse(path.Substring(comma+1).Trim(),out value)){index=value;path=path.Substring(0,comma).Trim();}
  path=path.Trim('"');
 }
 static Bitmap Canvas(){return new Bitmap(32,34,System.Drawing.Imaging.PixelFormat.Format32bppArgb);}
 static Bitmap FromIcon(Icon icon){var bitmap=Canvas();using(var g=Graphics.FromImage(bitmap)){g.Clear(Color.Transparent);g.InterpolationMode=InterpolationMode.HighQualityBicubic;g.DrawIcon(icon,new Rectangle(2,3,28,28));}return bitmap;}
 public static Bitmap ReadSpec(string spec){string path;int index;Parse(spec,out path,out index);if(string.IsNullOrWhiteSpace(path)||!File.Exists(path))return null;
  try{if(Path.GetExtension(path).Equals(".ico",StringComparison.OrdinalIgnoreCase)){using(var icon=new Icon(path))return FromIcon(icon);}
   IntPtr large=IntPtr.Zero,small=IntPtr.Zero;try{if(ExtractIconExW(path,index,out large,out small,1)==0)return null;IntPtr handle=large!=IntPtr.Zero?large:small;if(handle==IntPtr.Zero)return null;using(var icon=(Icon)Icon.FromHandle(handle).Clone())return FromIcon(icon);}finally{if(large!=IntPtr.Zero)DestroyIcon(large);if(small!=IntPtr.Zero&&small!=large)DestroyIcon(small);}
  }catch{return null;}
 }
 public static Bitmap Load(InstalledApp app){var bitmap=ReadSpec(app.IconSpec);if(bitmap!=null)return bitmap;
  // Prefer a product executable over an uninstaller's generic icon.
  try{if(!string.IsNullOrWhiteSpace(app.Location)&&Directory.Exists(app.Location))foreach(var file in Directory.EnumerateFiles(app.Location,"*.exe",SearchOption.TopDirectoryOnly).Where(p=>{string name=Path.GetFileName(p).ToLowerInvariant();return !name.Contains("unins")&&!name.Contains("setup")&&!name.Contains("update");}).Take(12)){bitmap=ReadSpec(file);if(bitmap!=null)return bitmap;}}catch{}
  try{string command=Environment.ExpandEnvironmentVariables(app.Command??"").Trim(),exe=null;if(command.StartsWith("\"")){int end=command.IndexOf('"',1);if(end>1)exe=command.Substring(1,end-1);}else{int end=command.IndexOf(".exe",StringComparison.OrdinalIgnoreCase);if(end>=0)exe=command.Substring(0,end+4);}bitmap=ReadSpec(exe);if(bitmap!=null)return bitmap;}catch{}
  return Fallback();
 }
 public static Bitmap Fallback(){var bitmap=Canvas();using(var g=Graphics.FromImage(bitmap)){g.Clear(Color.Transparent);g.SmoothingMode=SmoothingMode.AntiAlias;using(var b=new SolidBrush(Color.FromArgb(228,245,236)))g.FillEllipse(b,1,2,30,30);Look.Glyph(g,"apps",new Rectangle(5,6,23,23),Look.Teal);}return bitmap;}
}
}
