// No form Load, SQL, search, selection action or editing callback is executed.
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;
using System.Threading;
using System.Globalization;
using System.Windows.Forms;
using System.Web.Script.Serialization;

internal static class GridFormattingProbe {
    private static readonly BindingFlags Flags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;
    private static string Hash(byte[] data) { using(var sha=SHA256.Create()) return Convert.ToBase64String(sha.ComputeHash(data)); }
    private static string DataHash(DataTable table) { using(var stream=new MemoryStream()) { table.WriteXml(stream); return Hash(stream.ToArray()); } }
    private static int Shared(DataGridView grid) {
        int count=0;
        for(int i=0;i<grid.Rows.Count;i++) if(grid.Rows.SharedRow(i).Index==-1) count++;
        return count;
    }
    [STAThread] private static int Main(string[] args) {
        try {
            Thread.CurrentThread.CurrentCulture=CultureInfo.GetCultureInfo("en-US");
            var assembly=Assembly.LoadFrom(args[0]);
            var type=assembly.GetType("WorkOrderEntry.frmItemLookUp",true);
            var layout=type.GetMethod("GridColumnWidth",Flags);
            var results=new List<object>();
            foreach(int width in new[]{1024,1366}) foreach(float scale in new[]{1f,1.25f}) foreach(int rows in new[]{0,1,100,500}) {
                using(var form=(Form)Activator.CreateInstance(type)) {
                    type.GetField("isLoadingRecalledOrder",Flags).SetValue(form,true);
                    form.BindingContext=new BindingContext();
                    var grid=(DataGridView)type.GetProperty("gridItem",Flags).GetValue(form,null);
                    grid.Dock=DockStyle.None; grid.Size=new Size(width-370,320);
                    if(scale!=1f) form.Scale(new SizeF(scale,scale));
                    layout.Invoke(form,null);
                    type.GetField("gridItemLayoutApplied",Flags).SetValue(form,true);
                    var table=new DataTable("SyntheticItems");
                    Type[] types={typeof(string),typeof(string),typeof(double),typeof(double),typeof(double),typeof(double),typeof(int),typeof(string),typeof(double),typeof(int),typeof(string),typeof(string),typeof(int),typeof(bool),typeof(string),typeof(string),typeof(string),typeof(double),typeof(double)};
                    for(int c=0;c<19;c++) table.Columns.Add(grid.Columns[c].DataPropertyName,types[c]);
                    for(int i=0;i<rows;i++) table.Rows.Add("CODE"+i,"SYNTHETIC LONG ITEM DESCRIPTION "+i,1234.5678,99,1000.125,1,1,"SKU",100,i,"Description","Extended description",0,false,"-","SKU","FULL DESCRIPTION",1500.25,-3.125);
                    string inputHash=DataHash(table);
                    var bindTimes=new List<double>(); var repeatTimes=new List<double>();
                    int sharedAfter=0;
                    for(int trial=0;trial<7;trial++) {
                        grid.DataSource=null;
                        var watch=Stopwatch.StartNew();
                        grid.DataSource=table;
                        layout.Invoke(form,null);
                        watch.Stop();
                        sharedAfter=Shared(grid);
                        if(trial>0) bindTimes.Add(watch.Elapsed.TotalMilliseconds);
                        if(rows>0) grid.CurrentCell=grid.Rows[rows/2].Cells[1];
                        watch.Restart();
                        for(int pass=0;pass<10;pass++) layout.Invoke(form,null);
                        watch.Stop();
                        if(trial>0) repeatTimes.Add(watch.Elapsed.TotalMilliseconds/10);
                    }
                    int selected=grid.CurrentCell==null?-1:grid.CurrentCell.RowIndex;
                    int selectedColumn=grid.CurrentCell==null?-1:grid.CurrentCell.ColumnIndex;
                    // Programmatically altered heights must still be normalized.
                    if(rows>0) { grid.Rows[0].Height=27; grid.Rows[rows-1].Height=63; }
                    layout.Invoke(form,null);
                    for(int i=0;i<rows;i++) if(grid.Rows.SharedRow(i).Height!=44) throw new Exception("Row height changed");
                    if((grid.CurrentCell==null?-1:grid.CurrentCell.RowIndex)!=selected || (grid.CurrentCell==null?-1:grid.CurrentCell.ColumnIndex)!=selectedColumn) throw new Exception("Layout changed selection");
                    if(!Object.ReferenceEquals(table,grid.DataSource) || inputHash!=DataHash(table) || grid.RowCount!=rows) throw new Exception("Layout changed results");
                    var columns=new List<object>();
                    foreach(DataGridViewColumn col in grid.Columns) columns.Add(new {
                        col.Name,col.DataPropertyName,col.Width,col.MinimumWidth,col.Visible,col.Frozen,col.DisplayIndex,col.ReadOnly,
                        AutoSize=(int)col.AutoSizeMode,Sort=(int)col.SortMode,
                        Format=col.DefaultCellStyle.Format,Wrap=(int)col.DefaultCellStyle.WrapMode,
                        Align=(int)col.DefaultCellStyle.Alignment,Padding=col.DefaultCellStyle.Padding.ToString()
                    });
                    string pixels;
                    using(var image=new Bitmap(grid.Width,grid.Height)) using(var stream=new MemoryStream()) {
                        grid.DrawToBitmap(image,grid.ClientRectangle); image.Save(stream,ImageFormat.Png); pixels=Hash(stream.ToArray());
                        if(rows==500 && scale==1f) image.Save(Path.Combine(args[1],"Grid-"+width+".png"));
                    }
                    bindTimes.Sort(); repeatTimes.Sort();
                    results.Add(new {
                        Key=width+"|"+scale+"|"+rows,Data=inputHash,Rows=rows,Columns=columns,Selected=selected,SelectedColumn=selectedColumn,
                        HeaderHeight=grid.ColumnHeadersHeight,TemplateHeight=grid.RowTemplate.Height,Font=grid.Font.ToString(),
                        SelectionMode=(int)grid.SelectionMode,grid.ReadOnly,grid.ShowCellToolTips,Pixels=pixels,
                        SharedRows=sharedAfter,BindMedianMs=bindTimes[bindTimes.Count/2],RepeatMedianMs=repeatTimes[repeatTimes.Count/2]
                    });
                }
            }
            File.WriteAllText(Path.Combine(args[1],"Results.json"),new JavaScriptSerializer().Serialize(results));
            Console.WriteLine("PASS: 16 isolated grid cases completed; no SQL or form Load.");
            return 0;
        } catch(Exception ex) { Console.WriteLine(ex); return 1; }
    }
}
