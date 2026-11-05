using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Reflection;
using System.Windows.Forms;

namespace JingChu {
public static class BrandDialogs {
 public static void ApplyIcon(Form form){form.Icon=Icon.ExtractAssociatedIcon(Application.ExecutablePath);form.ShowIcon=true;}
 public static DialogResult ShowForm(Form form,IWin32Window owner){ApplyIcon(form);return form.ShowDialog(owner);}
 public static Form Create(string message,string caption,MessageBoxButtons buttons){var f=new Form{Text=caption,ClientSize=new Size(660,365),StartPosition=FormStartPosition.CenterParent,FormBorderStyle=FormBorderStyle.FixedDialog,MaximizeBox=false,MinimizeBox=false,BackColor=Look.Canvas,Font=new Font("Microsoft YaHei UI",10F)};ApplyIcon(f);f.Controls.Add(new PictureBox{Image=Look.Logo,SizeMode=PictureBoxSizeMode.Zoom,Location=new Point(22,20),Size=new Size(52,52)});f.Controls.Add(new Label{Text=caption,Location=new Point(88,30),Size=new Size(545,40),Font=Look.CardFont,ForeColor=Look.Ink});f.Controls.Add(new TextBox{Text=message,ReadOnly=true,Multiline=true,ScrollBars=ScrollBars.Vertical,BorderStyle=BorderStyle.None,BackColor=Look.Canvas,Location=new Point(26,92),Size=new Size(608,200)});var bar=new FlowLayoutPanel{Dock=DockStyle.Bottom,Height=60,FlowDirection=FlowDirection.RightToLeft,Padding=new Padding(14,8,14,8)};f.Controls.Add(bar);DialogResult[] results=buttons==MessageBoxButtons.OKCancel?new[]{DialogResult.Cancel,DialogResult.OK}:buttons==MessageBoxButtons.YesNo?new[]{DialogResult.No,DialogResult.Yes}:buttons==MessageBoxButtons.YesNoCancel?new[]{DialogResult.Cancel,DialogResult.No,DialogResult.Yes}:buttons==MessageBoxButtons.RetryCancel?new[]{DialogResult.Cancel,DialogResult.Retry}:new[]{DialogResult.OK};foreach(var result in results){string title=result==DialogResult.OK?"确定":result==DialogResult.Cancel?"取消":result==DialogResult.Yes?"是":result==DialogResult.No?"否":"重试";var b=new SoftButton{Text=title,DialogResult=result,Width=100,Primary=result==DialogResult.OK||result==DialogResult.Yes};bar.Controls.Add(b);if(result==DialogResult.Cancel||result==DialogResult.No)f.CancelButton=b;else if(f.AcceptButton==null)f.AcceptButton=b;}return f;}
 public static DialogResult Show(string message){return Show(null,message,"景·除",MessageBoxButtons.OK,MessageBoxIcon.None);}
 public static DialogResult Show(string message,string caption){return Show(null,message,caption,MessageBoxButtons.OK,MessageBoxIcon.None);}
 public static DialogResult Show(string message,string caption,MessageBoxButtons buttons){return Show(null,message,caption,buttons,MessageBoxIcon.None);}
 public static DialogResult Show(string message,string caption,MessageBoxButtons buttons,MessageBoxIcon icon){return Show(null,message,caption,buttons,icon);}
 public static DialogResult Show(IWin32Window owner,string message,string caption,MessageBoxButtons buttons,MessageBoxIcon icon){using(var f=Create(message,caption,buttons)){if(owner==null)owner=Form.ActiveForm;return owner==null?f.ShowDialog():f.ShowDialog(owner);}}
}
public static class Look {
 public static readonly Font HeaderFont=new Font("Microsoft YaHei UI",9F,FontStyle.Bold),CardFont=new Font("Microsoft YaHei UI",12F,FontStyle.Bold),BodyFont=new Font("Microsoft YaHei UI",9F),EmptyFont=new Font("Microsoft YaHei UI",10F),ArrowFont=new Font("Segoe UI",13F);
 public static readonly Color Canvas=Color.FromArgb(242,246,247),Card=Color.White,Ink=Color.FromArgb(23,48,53),Muted=Color.FromArgb(106,128,133),Teal=Color.FromArgb(8,125,117),Night=Color.FromArgb(17,43,46),Mint=Color.FromArgb(197,241,217),Line=Color.FromArgb(222,232,233),Danger=Color.FromArgb(181,66,67);
 public static GraphicsPath Round(RectangleF r,float radius){float d=Math.Min(radius*2,Math.Min(r.Width,r.Height));var p=new GraphicsPath();p.AddArc(r.X,r.Y,d,d,180,90);p.AddArc(r.Right-d,r.Y,d,d,270,90);p.AddArc(r.Right-d,r.Bottom-d,d,d,0,90);p.AddArc(r.X,r.Bottom-d,d,d,90,90);p.CloseFigure();return p;}
 static Image logo;
 public static Image Logo {get{if(logo==null){using(var s=Assembly.GetExecutingAssembly().GetManifestResourceStream("Brand.png")){if(s!=null)using(var x=Image.FromStream(s))logo=new Bitmap(x);else {var b=new Bitmap(64,64);using(var g=Graphics.FromImage(b)){g.Clear(Teal);g.DrawString("景",new Font("Microsoft YaHei UI",30,FontStyle.Bold),Brushes.White,3,4);}logo=b;}}}return logo;}}
 public static void NiceList(ListView list){list.OwnerDraw=true;list.DrawColumnHeader+=(s,e)=>{using(var b=new SolidBrush(Color.FromArgb(233,241,241)))e.Graphics.FillRectangle(b,e.Bounds);TextRenderer.DrawText(e.Graphics,e.Header.Text,Look.HeaderFont,new Rectangle(e.Bounds.X+10,e.Bounds.Y,e.Bounds.Width-14,e.Bounds.Height),Muted,TextFormatFlags.Left|TextFormatFlags.VerticalCenter|TextFormatFlags.EndEllipsis);};list.DrawItem+=(s,e)=>e.DrawDefault=true;list.DrawSubItem+=(s,e)=>e.DrawDefault=true;var rows=new ImageList{ImageSize=new Size(1,34),ColorDepth=ColorDepth.Depth32Bit};rows.Images.Add(new Bitmap(1,34));list.SmallImageList=rows;list.ShowItemToolTips=true;list.Font=new Font("Microsoft YaHei UI",9F);list.BorderStyle=BorderStyle.None;}
 public static void Glyph(Graphics g,string kind,Rectangle bounds,Color color){g.SmoothingMode=SmoothingMode.AntiAlias;float x=bounds.X,y=bounds.Y,w=bounds.Width,h=bounds.Height;using(var p=new Pen(color,1.8F)){p.StartCap=p.EndCap=LineCap.Round;
  if(kind=="home"){g.DrawLines(p,new[]{new PointF(x+2,y+9),new PointF(x+w/2,y+2),new PointF(x+w-2,y+9)});g.DrawLines(p,new[]{new PointF(x+5,y+8),new PointF(x+5,y+h-2),new PointF(x+w-5,y+h-2),new PointF(x+w-5,y+8)});}
  else if(kind=="files"){g.DrawRectangle(p,x+4,y+2,w-8,h-4);g.DrawLine(p,x+8,y+8,x+w-8,y+8);g.DrawLine(p,x+8,y+13,x+w-8,y+13);}
  else if(kind=="apps"){for(int a=0;a<2;a++)for(int b=0;b<2;b++)g.DrawRectangle(p,x+3+a*10,y+3+b*10,6,6);}
  else if(kind=="scan"){g.DrawEllipse(p,x+2,y+2,w-8,h-8);g.DrawLine(p,x+w-8,y+h-8,x+w-2,y+h-2);}
  else if(kind=="spark"){g.DrawLine(p,x+w/2,y+2,x+w/2,y+h-2);g.DrawLine(p,x+2,y+h/2,x+w-2,y+h/2);g.DrawLine(p,x+5,y+5,x+w-5,y+h-5);g.DrawLine(p,x+w-5,y+5,x+5,y+h-5);}
  else if(kind=="startup"){g.DrawArc(p,x+3,y+4,w-6,h-7,210,300);g.DrawLine(p,x+w/2,y+2,x+w/2,y+h/2);}
  else if(kind=="backup"){g.DrawRectangle(p,x+3,y+7,w-6,h-10);g.DrawRectangle(p,x+2,y+3,w-4,5);g.DrawLine(p,x+9,y+13,x+w-9,y+13);}
  else {g.DrawEllipse(p,x+3,y+3,w-6,h-6);g.DrawLine(p,x+w/2,y+10,x+w/2,y+h-7);g.DrawEllipse(p,x+w/2-0.5F,y+6,1,1);}
 }}
}
public class SoftButton:Button {
 public bool Primary,Danger;bool hover;public SoftButton(){SetStyle(ControlStyles.UserPaint|ControlStyles.AllPaintingInWmPaint|ControlStyles.OptimizedDoubleBuffer|ControlStyles.SupportsTransparentBackColor,true);FlatStyle=FlatStyle.Flat;FlatAppearance.BorderSize=0;UseVisualStyleBackColor=false;BackColor=Color.Transparent;Height=38;MinimumSize=new Size(96,38);Padding=new Padding(14,0,14,0);Cursor=Cursors.Hand;Font=new Font("Microsoft YaHei UI",9F);}
 protected override void OnPaintBackground(PaintEventArgs e){
  // ButtonBase's themed/flat background can leave a dark rectangular surface
  // behind our rounded shape. Paint the parent at the matching coordinates,
  // including gradients on cards, instead of invoking ButtonBase painting.
  if(Parent is CardPanel){var saved=e.Graphics.Save();try{e.Graphics.TranslateTransform(-Left,-Top);var args=new PaintEventArgs(e.Graphics,new Rectangle(Left,Top,Width,Height));InvokePaintBackground(Parent,args);}finally{e.Graphics.Restore(saved);}return;}
  Color background=Parent==null?Look.Canvas:Parent.BackColor;if(background.A<255)background=Look.Canvas;e.Graphics.Clear(background);
 }
 protected override void OnMouseEnter(EventArgs e){hover=true;Invalidate();base.OnMouseEnter(e);}protected override void OnMouseLeave(EventArgs e){hover=false;Invalidate();base.OnMouseLeave(e);}
 protected override void OnPaint(PaintEventArgs e){OnPaintBackground(e);e.Graphics.SmoothingMode=SmoothingMode.AntiAlias;Color fill=Primary?(hover?Color.FromArgb(6,105,100):Look.Teal):Danger?(hover?Color.FromArgb(250,227,227):Color.FromArgb(253,239,238)):hover?Color.FromArgb(230,241,240):Color.White;Color text=Primary?Color.White:Danger?Look.Danger:Look.Teal;if(!Enabled){fill=Color.FromArgb(234,239,239);text=Color.FromArgb(158,176,178);}using(var path=Look.Round(new RectangleF(1,1,Width-3,Height-3),9)){using(var b=new SolidBrush(fill))e.Graphics.FillPath(b,path);if(!Primary)using(var p=new Pen(Danger?Color.FromArgb(242,210,210):Look.Line))e.Graphics.DrawPath(p,path);}TextRenderer.DrawText(e.Graphics,Text,Font,ClientRectangle,text,TextFormatFlags.HorizontalCenter|TextFormatFlags.VerticalCenter|TextFormatFlags.EndEllipsis);if(Focused&&ShowFocusCues)ControlPaint.DrawFocusRectangle(e.Graphics,new Rectangle(5,5,Width-10,Height-10));}
}
public class NavButton:Button {
 public string Kind;public bool Active;bool hover;
 public NavButton(){SetStyle(ControlStyles.UserPaint|ControlStyles.AllPaintingInWmPaint|ControlStyles.OptimizedDoubleBuffer,true);FlatStyle=FlatStyle.Flat;FlatAppearance.BorderSize=0;Cursor=Cursors.Hand;Height=44;Font=new Font("Microsoft YaHei UI",10F);}
 protected override void OnMouseEnter(EventArgs e){hover=true;Invalidate();base.OnMouseEnter(e);}protected override void OnMouseLeave(EventArgs e){hover=false;Invalidate();base.OnMouseLeave(e);}
 protected override void OnPaint(PaintEventArgs e){e.Graphics.Clear(Look.Night);if(Active||hover)using(var p=Look.Round(new RectangleF(0,0,Width-1,Height-1),11))using(var b=new SolidBrush(Active?Color.FromArgb(31,79,77):Color.FromArgb(25,57,59)))e.Graphics.FillPath(b,p);var c=Active?Look.Mint:Color.FromArgb(161,190,191);Look.Glyph(e.Graphics,Kind,new Rectangle(15,11,23,23),c);TextRenderer.DrawText(e.Graphics,Text,Font,new Rectangle(52,0,Width-60,Height),Active?Color.White:Color.FromArgb(189,210,210),TextFormatFlags.Left|TextFormatFlags.VerticalCenter);if(Active)using(var b=new SolidBrush(Look.Mint))e.Graphics.FillEllipse(b,Width-20,Height/2-3,6,6);}
}
public class CardPanel:Panel {
 public bool Dark;public int Radius=18;public CardPanel(){DoubleBuffered=true;BackColor=Look.Canvas;Padding=new Padding(18);}
 protected override void OnPaintBackground(PaintEventArgs e){e.Graphics.Clear(Parent==null?Look.Canvas:Parent.BackColor);e.Graphics.SmoothingMode=SmoothingMode.AntiAlias;using(var p=Look.Round(new RectangleF(1,1,Width-3,Height-3),Radius)){if(Dark)using(var b=new LinearGradientBrush(ClientRectangle,Color.FromArgb(17,60,61),Color.FromArgb(20,89,82),30F))e.Graphics.FillPath(b,p);else using(var b=new SolidBrush(Look.Card))e.Graphics.FillPath(b,p);if(!Dark)using(var pen=new Pen(Look.Line))e.Graphics.DrawPath(pen,p);}}
}
public class EmptyListView:ListView {
 public string EmptyText="暂无项目\n添加文件，或点击上方按钮开始扫描";
 protected override void WndProc(ref Message m){base.WndProc(ref m);if(m.Msg==15&&Items.Count==0&&IsHandleCreated){using(var g=Graphics.FromHwnd(Handle)){var r=new Rectangle(16,Math.Max(50,ClientSize.Height/2-20),Math.Max(10,ClientSize.Width-32),65);TextRenderer.DrawText(g,EmptyText,Look.EmptyFont,r,Look.Muted,TextFormatFlags.HorizontalCenter|TextFormatFlags.WordBreak);}}}
}
public class QuickCard:Control {
 public string Kind,Title,Detail;bool hover;public QuickCard(){SetStyle(ControlStyles.UserPaint|ControlStyles.AllPaintingInWmPaint|ControlStyles.OptimizedDoubleBuffer,true);Cursor=Cursors.Hand;BackColor=Look.Canvas;}
 protected override void OnMouseEnter(EventArgs e){hover=true;Invalidate();base.OnMouseEnter(e);}protected override void OnMouseLeave(EventArgs e){hover=false;Invalidate();base.OnMouseLeave(e);}
 protected override void OnPaint(PaintEventArgs e){e.Graphics.Clear(Look.Canvas);e.Graphics.SmoothingMode=SmoothingMode.AntiAlias;using(var p=Look.Round(new RectangleF(1,1,Width-3,Height-3),16)){using(var b=new SolidBrush(hover?Color.FromArgb(235,249,242):Color.White))e.Graphics.FillPath(b,p);using(var pen=new Pen(hover?Color.FromArgb(155,209,188):Look.Line))e.Graphics.DrawPath(pen,p);}using(var b=new SolidBrush(Color.FromArgb(228,245,236)))e.Graphics.FillEllipse(b,18,19,42,42);Look.Glyph(e.Graphics,Kind,new Rectangle(27,28,24,24),Look.Teal);TextRenderer.DrawText(e.Graphics,Title,Look.CardFont,new Rectangle(75,17,Width-96,30),Look.Ink,TextFormatFlags.Left);TextRenderer.DrawText(e.Graphics,Detail,Look.BodyFont,new Rectangle(75,50,Width-94,55),Look.Muted,TextFormatFlags.Left|TextFormatFlags.WordBreak);TextRenderer.DrawText(e.Graphics,"↗",Look.ArrowFont,new Rectangle(Width-37,16,24,25),Look.Teal);}
}
}
